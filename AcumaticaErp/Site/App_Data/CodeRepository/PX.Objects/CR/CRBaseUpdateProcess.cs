using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CR
{
	public abstract class CRBaseUpdateProcess<TGraph, TPrimary, TMarkAttribute, TClassField, TAnswerType> : CRBaseMassProcess<TGraph, TPrimary>, IMassProcess<TPrimary>
		where TGraph : PXGraph, IMassProcess<TPrimary>, new()
		where TPrimary : class, IBqlTable, new() 
		where TClassField : IBqlField 
		where TAnswerType : IBqlOperand
	{
		public PXSelect<FieldValue, Where<FieldValue.attributeID, IsNull>, OrderBy<Asc<FieldValue.order>>> Fields;
		public PXSelect<FieldValue, Where<FieldValue.attributeID, IsNotNull>, OrderBy<Asc<FieldValue.order>>> Attributes; 
		public PXSelect<CSAnswers> answers;

		protected CRBaseUpdateProcess()
		{
			//Init PXVirtual Static constructor
			typeof(FieldValue).GetCustomAttributes(typeof(PXVirtualAttribute), false);
			GetAttributeSuffixes(this, ref _suffixes);
		}

		public IEnumerable fields(PXAdapter adapter)
		{
			return Caches[typeof (FieldValue)].Cached.Cast<FieldValue>().Where(row => row.AttributeID == null);
		}

		public IEnumerable attributes(PXAdapter adapter)
		{
			RestrictAttributesByClass();
			return Caches[typeof(FieldValue)].Cached.Cast<FieldValue>().Where(row => row.AttributeID != null && row.Hidden != true);
		}

		protected virtual void FieldValue_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			FieldValue val = (FieldValue)e.NewRow;
			FieldValue oldValue = (FieldValue)e.Row;
			val.Selected = val.Selected == true || (oldValue.Value != val.Value);
		}

		protected virtual void FieldValue_Value_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			e.ReturnState = InitValueFieldState(e.Row as FieldValue);
		}

		protected PXFieldState InitValueFieldState(FieldValue field = null)
		{
			PXFieldState state;
			if (field != null && (state = Caches[typeof (TPrimary)].GetStateExt(null, field.Name) as PXFieldState) != null)
			{
				state.SetFieldName(typeof(FieldValue.value).Name);
				state.Value = field.Value;
				state.Enabled = true;
				return state;
			}
			else
			{
				PXFieldState defState = PXStringState.CreateInstance(null, null, null, typeof (FieldValue.value).Name,
				                                                     false, 0, null, null, null, null, null);
				defState.DisplayName = Messages.PropertyValue;
				return defState;
			}
		}

		private readonly List<string> _suffixes; 

		protected static void GetAttributeSuffixes(PXGraph graph , ref List<string> suffixes)
		{
			suffixes = suffixes ?? new List<string>(graph.Caches[typeof(TPrimary)].BqlTable
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.SelectMany(p => p.GetCustomAttributes(true).Where(atr => atr is PXDBAttributeAttribute), (p, atr) => p.Name));
		}

		public static IEnumerable<FieldValue> GetMarkedProperties(PXGraph graph, ref int firstSortOrder)
		{
			PXCache cache = graph.Caches[typeof(TPrimary)];
			int order = firstSortOrder;
			List<FieldValue> res = new List<FieldValue>(graph.Caches[typeof (TPrimary)].BqlTable.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				                               .SelectMany(p => p.GetCustomAttributes(true), (p, atr) => new {p, atr})
				                               .Where(@t => @t.atr is TMarkAttribute)
				                               .Select(@t => new {@t, state = cache.GetStateExt(null, @t.p.Name) as PXFieldState})
				                               .Where(@t => @t.state != null)
				                               .Select(@t => new FieldValue
					                               {
						                               Selected = false,
													   CacheName = typeof(TPrimary).FullName,
						                               Name = @t.@t.p.Name,
						                               DisplayName = @t.state.DisplayName,
						                               AttributeID = null,
						                               Order = order++
					                               }));
			firstSortOrder = order;
			return res;
		}

		public static IEnumerable<FieldValue> GetAttributeProperties(PXGraph graph, ref int firstSortOrder)
		{
			return GetAttributeProperties(graph, ref firstSortOrder, null);
		}

		public static IEnumerable<FieldValue> GetAttributeProperties(PXGraph graph, ref int firstSortOrder, List<string> suffixes)
		{
			PXCache cache = graph.Caches[typeof(TPrimary)];
			int order = firstSortOrder;
			List<FieldValue> res = new List<FieldValue>();
			GetAttributeSuffixes(graph, ref suffixes);
			foreach (string field in graph.Caches[typeof(TPrimary)].Fields)
			{
				if (!suffixes.Any(suffix => field.EndsWith(string.Format("_{0}", suffix)))) continue;
				PXFieldState state = cache.GetStateExt(null, field) as PXFieldState;
				if (state == null) continue;

				string displayName = state.DisplayName;
				string attrID = field;
				string local = field;
				foreach (string suffix in suffixes.Where(suffix => local.EndsWith(string.Format("_{0}", suffix))))
				{
					attrID = field.Replace(string.Format("_{0}", suffix), string.Empty);
					displayName = state.DisplayName.Replace(string.Format("${0}$-", suffix), string.Empty);
					break;
				}
				res.Add( new FieldValue
					{
						Selected = false,
						CacheName = typeof(TPrimary).FullName,
						Name = field,
						DisplayName = displayName,
						AttributeID = attrID,
						Order = order++ + 1000
					});
			}
			firstSortOrder = order;
			return res;
		}

		public static IEnumerable<FieldValue> GetProcessingProperties(PXGraph graph, ref int firstSortOrder)
		{
			return GetProcessingProperties(graph, ref firstSortOrder, null);
		}

		public static IEnumerable<FieldValue> GetProcessingProperties(PXGraph graph, ref int firstSortOrder, List<string> suffixes)
		{
			return GetMarkedProperties(graph, ref firstSortOrder).Union(GetAttributeProperties(graph, ref firstSortOrder, suffixes));
		}

		protected virtual IEnumerable<FieldValue> ProcessingProperties
		{
			get
			{
				int _firstOrder = 0;
				return GetProcessingProperties(this, ref _firstOrder, _suffixes);
			}
		}

		protected void FillPropertyValue(PXGraph graph, string viewName)
		{
			PXCache cache = Caches[typeof(FieldValue)];
			cache.Clear();
			foreach (FieldValue field in ProcessingProperties)
			{
				cache.Insert(field);
			}
			cache.IsDirty = false;
		}

		protected Dictionary<string, bool> GetClassAttributes(string ClassID)
		{
			return PXSelectJoin<CSAttributeGroup,
						InnerJoin<CSAttribute, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
						Where<CSAttributeGroup.entityClassID, Equal<Required<TClassField>>,
							And<CSAttributeGroup.type, Equal<TAnswerType>>>, OrderBy<Asc<CSAttributeGroup.sortOrder>>>
							.Select(this, ClassID)
							.RowCast<CSAttributeGroup>()
							.ToDictionary(g => g.AttributeID, g => g.Required == true);
		}

		public virtual void RestrictAttributesByClass()
		{
			FieldValue classFld = Caches[typeof(FieldValue)].Cached.Cast<FieldValue>().FirstOrDefault(field => field.Name.ToLower() == typeof(TClassField).Name.ToLower());
			string classID = classFld == null || classFld.Selected == false || classFld.Value == null ? null : classFld.Value;
			Dictionary<string, bool> classAttrs = GetClassAttributes(classID);
			foreach (FieldValue attr in Caches[typeof(FieldValue)].Cached.Cast<FieldValue>().Where(f => f.AttributeID != null))
			{
				attr.Hidden = !classAttrs.ContainsKey(attr.AttributeID);
				attr.Required = !(bool)attr.Hidden && classAttrs[attr.AttributeID];
			}
		}

		protected override bool AskParameters()
		{
			return Fields.AskExt(FillPropertyValue) == WebDialogResult.OK;
		}

		public override void ProccessItem(PXGraph graph, TPrimary item)
		{
			PXCache cache = graph.Caches[typeof (TPrimary)];
			TPrimary newItem = (TPrimary)cache.CreateInstance();
			PXCache<TPrimary>.RestoreCopy(newItem, item);
			string entityType = CSAnswerType.GetAnswerType(cache.GetItemType());
			string entityID = CSAnswerType.GetEntityID(cache.GetItemType());
			
			PXView primaryView = graph.Views[graph.PrimaryView];
			object[] searches = new object[primaryView.Cache.BqlKeys.Count];
			string[] sortcolumns = new string[primaryView.Cache.BqlKeys.Count];
			for (int i = 0; i < cache.BqlKeys.Count(); i++)
			{
				sortcolumns[i] = cache.BqlKeys[i].Name;
				searches[i] = cache.GetValue(newItem, sortcolumns[i]);
			}
			int startRow = 0, totalRows = 0;
			
			List<object> result = primaryView.Select(null, null, searches, sortcolumns, null, null, ref startRow, 1, ref totalRows);
			newItem = (TPrimary)cache.CreateCopy(PXResult.Unwrap<TPrimary>(result[0]));

			foreach (FieldValue fieldValue in Fields.Cache.Cached.Cast<FieldValue>().Where(o => o.AttributeID == null && o.Selected == true))
			{								
				PXFieldState state = cache.GetStateExt(newItem, fieldValue.Name) as PXFieldState;
				PXIntState intState = state as PXIntState;
				PXStringState strState = state as PXStringState;
				if ((intState != null && intState.AllowedValues != null && intState.AllowedValues.Length > 0 &&
					intState.AllowedValues.All(v => v != int.Parse(fieldValue.Value))) 
					||
					(strState != null && strState.AllowedValues != null && strState.AllowedValues.Length > 0 &&
					strState.AllowedValues.All(v => v != fieldValue.Value)))
				{					
					throw new PXSetPropertyException(ErrorMessages.UnallowedListValue, fieldValue.Value, fieldValue.Name);
				}
				if (state != null && !Equals(state.Value, fieldValue.Value))
				{
					cache.SetValueExt(newItem, fieldValue.Name, fieldValue.Value);
					cache.Update(newItem);
				}

				result = primaryView.Select(null, null, searches, sortcolumns, null, null, ref startRow, 1, ref totalRows);
				newItem = (TPrimary)cache.CreateCopy(PXResult.Unwrap<TPrimary>(result[0]));
			}

			PXCache attrCache = cache.Graph.Caches[typeof(CSAnswers)];

			foreach (FieldValue attrValue in Attributes.Cache.Cached.Cast<FieldValue>().Where(o => o.AttributeID != null && o.Selected == true))
			{
				CSAnswers attr = (CSAnswers)attrCache.CreateInstance();
				attr.AttributeID = attrValue.AttributeID;
				attr.EntityID = cache.GetValue(newItem, entityID) as int?;
				attr.EntityType = entityType;
				attr.Value = attrValue.Value;
				attrCache.Update(attr);
			}
		}
	}

	#region FieldValue
	[DebuggerDisplay("Display='{DisplayName}' Name='{Name}' Cache='{CacheName}'")]
	[Serializable]
	[PXVirtual]
	public partial class FieldValue : IBqlTable
	{

		#region Selected
		public abstract class selected : IBqlField { }
		private bool? _Selected;
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get { return _Selected; }
			set { _Selected = value; }
		}
		#endregion

		#region CacheName
		public abstract class cacheName : IBqlField { }
		[PXString]
		public string CacheName { get; set; }
		#endregion

		#region Name
		public abstract class name : IBqlField { }
		[PXString(IsKey = true)]
		public string Name { get; set; }
		#endregion

		#region DisplayName
		public abstract class displayName : IBqlField { }
		[PXString]
		[PXUIField(DisplayName = Messages.PropertyDisplayName, Enabled = false)]
		public string DisplayName { get; set; }
		#endregion

		#region Value
		public abstract class value : IBqlField { }
		[PXUIField(DisplayName = Messages.PropertyValue)]
		public virtual string Value { get; set; }
		#endregion
	
		#region AttributeID
		public abstract class attributeID : IBqlField { }
		[PXString]
		public virtual string AttributeID { get; set; }
		#endregion

		#region Order
		public abstract class order : IBqlField {}
		[PXInt]
		public virtual int? Order { get; set; }

		#endregion

		#region Hidden
		public abstract class hidden : IBqlField { }
		private bool? _Hidden;
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? Hidden
		{
			get { return _Hidden; }
			set 
			{
				_Hidden = value;
				_Selected = _Hidden == true ? false:_Selected;
			}
		}
		#endregion

		#region Required
		public abstract class required : IBqlField { }
		[PXBool]
		[PXUIField(DisplayName = "Required", Enabled = false)]
		public virtual bool? Required { get; set; }

		#endregion

	}
	#endregion

}
