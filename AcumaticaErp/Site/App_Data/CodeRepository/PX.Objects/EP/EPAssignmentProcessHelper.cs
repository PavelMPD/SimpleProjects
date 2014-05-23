using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data.Wiki.Parser;
using PX.Objects.CR;
using PX.Data;
using System.Collections;
using PX.SM;
using PX.TM;
using PX.Objects.CS;

namespace PX.Objects.EP
{
	/// <summary>
	/// Implements the Workgroup assignment process logic.
	/// </summary>
	public class EPAssignmentProcessHelper<Table> : PXGraph<EPAssignmentProcessHelper<Table>>
		where Table : class, PX.Data.EP.IAssign, IBqlTable
	{
		public PXView attributeView;

		/// <summary>
		/// This list is used to track the path passed to check and prevent cyclic references.
		/// </summary>
		private readonly List<int> path = new List<int>();
		private PXGraph processGraph;
		private Type processMapType;
		private IBqlTable currentItem;
		private CSAnswers currentAttribute;

		private readonly PXGraph _Graph;

		public EPAssignmentProcessHelper(PXGraph graph)
			: this()
		{
			_Graph = graph;
		}

		public EPAssignmentProcessHelper()
		{
			attributeView = new PXView(this, false, new Select<CSAnswers>(), (PXSelectDelegate)getAttributeRecord);
		}

		/// <summary>
		/// Assigns Owner and Workgroup to the given IAssign instance based on the assigmentment rules.
		/// </summary>
		/// <param name="item">IAssign object</param>
		/// <param name="assignmentMapID">Assignment map</param>
		/// <returns>True if workgroup was assigned; otherwise false</returns>
		/// <remarks>
		/// You have to manualy persist the IAssign object to save the changes.
		/// </remarks>
		public virtual bool Assign(Table item, int? assignmentMapID)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			if (assignmentMapID < 0)
				throw new ArgumentOutOfRangeException("assignmentMapID");

			path.Clear();
			EPAssignmentMap map = 
			PXSelect<EPAssignmentMap, Where<EPAssignmentMap.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>>>
				.SelectWindowed(this, 0, 1, assignmentMapID);
			if (map == null) return false;

			processMapType = GraphHelper.GetType(map.EntityType);
			Type itemType = item.GetType();

			PXSelectBase<EPAssignmentRoute> rs = new
				PXSelectReadonly<EPAssignmentRoute,
				Where<EPAssignmentRoute.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>,
				And<EPAssignmentRoute.parent, IsNull>>,
				OrderBy<Asc<EPAssignmentRoute.sequence>>>(this);

			PXResultset<EPAssignmentRoute> routes = rs.Select(assignmentMapID, null);
			this.processGraph = _Graph ?? new EntityHelper(this).GetPrimaryGraph(item, false);

			if (processGraph != null && processMapType != null)
			{
				if (processMapType.IsAssignableFrom(itemType))
					this.processGraph.Caches[itemType].Current = item;
				else if (itemType.IsAssignableFrom(processMapType))
				{
					object placed = this.processGraph.Caches[processMapType].CreateInstance();
					PXCache cache = (PXCache)Activator.CreateInstance(typeof(PXCache<>).MakeGenericType(itemType), this);
					cache.RestoreCopy(placed, item);
					this.processGraph.Caches[processMapType].Current = placed;
				}
				else
					return false;
			}

			return ProcessLevel(item, assignmentMapID, routes);
		}

		private bool ProcessLevel(Table item, int? assignmentMap, PXResultset<EPAssignmentRoute> routes)
		{
			PXSelectReadonly<EPAssignmentRoute,
				Where<EPAssignmentRoute.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>,
					And<EPAssignmentRoute.parent, Equal<Required<EPAssignmentRoute.assignmentRouteID>>>>, OrderBy<Asc<EPAssignmentRoute.sequence>>> rs = new PXSelectReadonly<EPAssignmentRoute, Where<EPAssignmentRoute.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>, And<EPAssignmentRoute.parent, Equal<Required<EPAssignmentRoute.assignmentRouteID>>>>, OrderBy<Asc<EPAssignmentRoute.sequence>>>(this);

			foreach (EPAssignmentRoute route in routes)
			{
				if (route.AssignmentRouteID == null) continue;

				path.Add(route.AssignmentRouteID.Value);

				if (IsPassed(item, route))
				{
					if (route.WorkgroupID != null || route.OwnerID != null || route.OwnerSource != null)
					{
						item.WorkgroupID = route.WorkgroupID;
						item.OwnerID = null;
						if (route.OwnerSource != null)
						{
							PXGraph graph = processGraph;
							PXCache cache = graph.Caches[typeof (Table)];
							string code = PXTemplateContentParser.Instance.Process(route.OwnerSource,
								graph, 
								typeof(Table), 
								cache.BqlKeys.Select(t => cache.GetValue(item, t.Name)).ToArray());
							EPEmployee emp =
							PXSelect<EPEmployee, Where<EPEmployee.acctCD, Equal<Required<EPEmployee.acctCD>>>>
								.SelectWindowed(this, 0, 1, code);
							item.OwnerID = emp != null ? emp.UserID : GUID.CreateGuid(code);
						}
						if (item.OwnerID == null) item.OwnerID = route.OwnerID;

						if(route.UseWorkgroupByOwner == true && item.WorkgroupID != null && item.OwnerID != null)
						{
							EPCompanyTreeMember member = 
							PXSelectJoin<EPCompanyTreeMember,
								InnerJoin<EPCompanyTreeH, On<EPCompanyTreeH.workGroupID, Equal<EPCompanyTreeMember.workGroupID>>,
								InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<EPCompanyTreeMember.userID>>>>,
								Where<EPCompanyTreeH.parentWGID, Equal<Required<EPCompanyTreeH.parentWGID>>,
									And<EPCompanyTreeMember.userID, Equal<Required<EPCompanyTreeMember.userID>>>>>
									.SelectWindowed(this, 0, 1,item.WorkgroupID,item.OwnerID);
							if (member != null)
								item.WorkgroupID = member.WorkGroupID;
							else
								item.OwnerID = null;
						}
						else if (item.WorkgroupID != null && item.OwnerID != null)
						{
							EPCompanyTreeMember member =
							PXSelectJoin<EPCompanyTreeMember,
								InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<EPCompanyTreeMember.userID>>>,
								Where<EPCompanyTreeMember.workGroupID, Equal<Required<EPCompanyTreeMember.workGroupID>>,
									And<EPCompanyTreeMember.userID, Equal<Required<EPCompanyTreeMember.userID>>>>>
								.SelectWindowed(this, 0, 1, item.WorkgroupID, item.OwnerID);

							if(member == null)
								item.OwnerID = null;
						}

						if (item.WorkgroupID != null && item.OwnerID == null)
						{
							EPCompanyTreeMember owner = PXSelectJoin<EPCompanyTreeMember,
								InnerJoin<EPEmployee, On<EPEmployee.userID, Equal<EPCompanyTreeMember.userID>>>,
                                Where<EPCompanyTreeMember.workGroupID, Equal<Required<EPCompanyTreeMember.workGroupID>>,
									And<EPCompanyTreeMember.isOwner, Equal<boolTrue>>>>.Select(this, item.WorkgroupID);

							if (owner != null)
								item.OwnerID = owner.UserID;
						}
						PXTrace.WriteInformation(Messages.ProcessRouteSequence, route.Sequence);
						return true;
					}

					if (route.RouteID != null)
						return !path.Contains(route.RouteID.Value) &&
						       ProcessLevel(item, assignmentMap, PXSelectReadonly<EPAssignmentRoute>
						                                         	.Search<EPAssignmentRoute.assignmentRouteID>(this, route.RouteID));
					PXResultset<EPAssignmentRoute> result = rs.Select(assignmentMap, route.AssignmentRouteID);
					return ProcessLevel(item, assignmentMap, result);
				}
			}

			return false;
		}

		private bool IsPassed(Table item, EPAssignmentRoute route)
		{
			try
			{				
				List<EPAssignmentRule> rules = new List<EPAssignmentRule>();
				foreach(EPAssignmentRule rule in PXSelectReadonly<EPAssignmentRule, 
					Where<EPAssignmentRule.assignmentRouteID, Equal<Required<EPAssignmentRoute.assignmentRouteID>>>>.Select(this,route.AssignmentRouteID))
					rules.Add(rule);

				if (rules.Count == 0) return true;
				switch (route.RuleType)
				{
					case RuleType.AllTrue:
						return rules.All(rule => IsTrue(item, rule));
					case RuleType.AtleastOneConditionIsTrue:
						return rules.Any(rule => IsTrue(item, rule));
					case RuleType.AtleastOneConditionIsFalse:
						return rules.Any(rule => !IsTrue(item, rule));
					default:
						return false;
				}
			}
			catch
			{
				return false;
			}
		}

		private bool IsTrue(Table item, EPAssignmentRule rule)
		{
			return 
				rule.FieldName.EndsWith("_Attributes")?
				IsAttributeRuleTrue((IAttributeSupport)item, rule) :
				IsItemRuleTrue(GetItemRecord(rule, item), rule);
		}

		//Refactor - Generalize if possible!!!
		private IBqlTable GetItemRecord(EPAssignmentRule rule, IBqlTable item)
		{
			PXGraph graph = this.processGraph;
			Type itemType = item.GetType();
			Type ruleType = GraphHelper.GetType(rule.Entity);

			if (ruleType.IsAssignableFrom(itemType)) return item;
			if (processMapType.IsAssignableFrom(ruleType) && graph != null) 
				return graph.Caches[processMapType].Current as IBqlTable;

			if (graph != null)
			{
				foreach (CacheEntityItem entry in EMailSourceHelper.TemplateEntity(this, null, item.GetType().FullName, graph.GetType().FullName))
				{
					Type entityType = GraphHelper.GetType(entry.SubKey);
					if (entityType.IsAssignableFrom(ruleType) && graph.Views.ContainsKey(entry.Key))
					{
						PXView view = graph.Views[entry.Key];
						object result = view.SelectSingleBound(new object[] {item});
						return (result is PXResult ? ((PXResult) result)[0] : result) as IBqlTable;
					}
				}
			}
			return item;
		}

		private bool IsItemRuleTrue(IBqlTable item, EPAssignmentRule rule)
		{
			if (item is EPEmployee && rule.FieldName.Equals(typeof(EPEmployee.workgroupID).Name, StringComparison.InvariantCultureIgnoreCase))
			{
				return IsEmployeeInWorkgroup((EPEmployee)item, rule);
			}

			currentItem = item;
			Type viewType = BqlCommand.Compose(typeof (Select<>), item.GetType());
			PXView itemView = new PXView(this, false, BqlCommand.CreateInstance(viewType), 
				(PXSelectDelegate)getItemRecord);

			if(rule.Condition == null) return false;

			PXFilterRow filter = new PXFilterRow(
				rule.FieldName, 
				(PXCondition)rule.Condition.Value, 
				GetFieldValue(item, rule.FieldName, rule.FieldValue), 
				null);
			int startRow = 0;
			int totalRows = 0;

			List<object> result = itemView.Select(null, null, null, null, null, new PXFilterRow[] { filter }, ref startRow, 1, ref totalRows);

			return result.Count > 0;			
		}

		private bool IsEmployeeInWorkgroup(EPEmployee employee, EPAssignmentRule rule)
		{
			object workgroupID = null;
			PXCache sourceCache = this.Caches[typeof(EPEmployee)];
			object copy = sourceCache.CreateCopy(employee);
			if (rule.FieldValue != null)
				sourceCache.SetValueExt(copy, rule.FieldName, rule.FieldValue);
			else
			{
				object newValue;
				sourceCache.RaiseFieldDefaulting(rule.FieldName, copy, out newValue);
				sourceCache.SetValue(copy, rule.FieldName, newValue);
			}
			workgroupID = sourceCache.GetValue(copy, rule.FieldName);

			PXSelectBase<EPCompanyTreeMember> select = new PXSelect<EPCompanyTreeMember,
					Where<EPCompanyTreeMember.userID, Equal<Required<EPCompanyTreeMember.userID>>,
						And<EPCompanyTreeMember.workGroupID, Equal<Required<EPCompanyTreeMember.workGroupID>>,
						And<EPCompanyTreeMember.active, Equal<True>>>>>(this);
			EPCompanyTreeMember member = select.SelectSingle(employee.UserID, workgroupID);

			return member != null;
		}

		private bool IsAttributeRuleTrue(IAttributeSupport item, EPAssignmentRule rule)
		{
			string field = rule.FieldName.Substring(0, rule.FieldName.Length - "_Attribute".Length-1);
			CSAttribute attribute = PXSelectReadonly<CSAttribute>.Search<CSAttribute.attributeID>(this, field);

			if (attribute == null || rule.Condition == null)
				//Field Name is not a valid question.
				return false;

			
			CSAnswers ans = PXSelect<CSAnswers,
				Where<CSAnswers.entityID, Equal<Required<CSAnswers.entityID>>,
				And<CSAnswers.attributeID, Equal<Required<CSAnswers.attributeID>>,
				And<CSAnswers.entityType, Equal<Required<CSAnswers.entityType>>>>>>.Select(_Graph ?? this, item.ID, field, item.EntityType);

			if (ans == null)
			{
				//Answer for the given question doesnot exist.
				switch (rule.Condition.Value)
				{
					case (int)PXCondition.ISNULL:
						return true;				
					case (int)PXCondition.ISNOTNULL:
						return false;				
					case (int) PXCondition.EQ:
						return string.IsNullOrEmpty(rule.FieldValue);				
					case (int) PXCondition.NE:
						return !string.IsNullOrEmpty(rule.FieldValue);
				}
				return false;
			}

			this.currentAttribute = ans;

			PXFilterRow filter = new PXFilterRow(typeof(CSAnswers.value).Name, (PXCondition)rule.Condition.Value, rule.FieldValue, null);
			int startRow = 0;
			int totalRows = 0;

			List<object> result = attributeView.Select(null, null, null, null, null, new PXFilterRow[] { filter }, ref startRow, 1, ref totalRows);

			return result.Count > 0;
		}

		private IEnumerable getItemRecord()
		{
			yield return currentItem;
		}

		private IEnumerable getAttributeRecord()
		{
			yield return currentAttribute;
		}

		private object GetFieldValue(IBqlTable item, string fieldname, string fieldvalue = null)
		{
			PXCache sourceCache = this.Caches[item.GetType()];
			object copy = sourceCache.CreateCopy(item);
			if (fieldvalue != null)
				sourceCache.SetValueExt(copy, fieldname, fieldvalue);
			else
			{
				object newValue;
				sourceCache.RaiseFieldDefaulting(fieldname, copy, out newValue);
				sourceCache.SetValue(copy, fieldname, newValue);
			}
			return sourceCache.GetValueExt(copy, fieldname);
		}
	}

	public class CRAssigmentScope : IDisposable
	{
		private readonly PX.Data.EP.IAssign source;
		private readonly int? workgroupID;
		private readonly Guid? ownerID;
		public CRAssigmentScope(PX.Data.EP.IAssign source)
		{
			this.source = source;
			this.workgroupID = source.WorkgroupID;
			this.ownerID = source.OwnerID;
		}
		#region IDisposable Members
		public virtual void Dispose()
		{
			this.source.WorkgroupID = workgroupID;
			this.source.OwnerID = ownerID;
		}
		#endregion
	}
}
