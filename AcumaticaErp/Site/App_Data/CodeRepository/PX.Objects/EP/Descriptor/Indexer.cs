using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.TM;

namespace PX.Objects.EP
{
	#region EPViewSelect<TItem, TPView>

	public class EPViewSelect<TItem, TPView> : PXSelect<EPView>
		where TItem : EPActivity
		where TPView : class, new()
	{
		private const string _SETVIEWED_ACTION_NAME = "SetViewed$";

		#region Ctor

		public EPViewSelect(PXGraph graph)
			: base(graph)
		{
			Initialize();
		}

		public EPViewSelect(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			Initialize();
		}

		private void Initialize()
		{
			if (!_Graph.Views.Caches.Contains(typeof(EPView))) _Graph.Views.Caches.Add(typeof(EPView));

			string name = _SETVIEWED_ACTION_NAME + typeof(TItem).Name;
			_Graph.Actions[name] =
				(PXAction)Activator.CreateInstance(
					typeof(PXNamedAction<>).MakeGenericType(
						new[] { typeof(TPView) }),
					new object[] { _Graph, name, new PXButtonDelegate(setViewed) });
		}

		#endregion

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable setViewed(PXAdapter adapter)
		{
			var current = _Graph.Caches[typeof(TItem)].Current;
			SetRecordViewed(current as EPActivity);
			return new[] { current };
		}

		public void SetRecordViewed(EPActivity activity)
		{
			EPViewStatusAttribute.MarkAsViewed(_Graph, activity, true);
		}
	}

	#endregion

	public class Approver<Operand> : IBqlComparison
		where Operand : IBqlOperand, new()
	{
		private IBqlCreator _operand;

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			result = null;
			value = null;
		}

		public void Parse(PXGraph graph, List<IBqlParameter> pars, List<Type> tables, List<Type> fields, List<IBqlSortColumn> sortColumns, StringBuilder text, BqlCommand.Selection selection)
		{
			if (graph != null && text != null)
			{
				text.Append(" IN ").Append(BqlCommand.SubSelect);
				text.Append(typeof (EPApproval).Name).Append('.').Append(typeof (EPApproval.refNoteID).Name);
				text.Append(" FROM ").Append(typeof (EPApproval).Name).Append(' ').Append(typeof (EPApproval).Name);
				text.Append(" WHERE (").Append(typeof (EPApproval).Name).Append('.').Append(typeof (EPApproval.ownerID).Name);
				text.Append('=');
				ParseOperand(graph, pars, tables, fields, sortColumns, text, selection);
				text.Append(" OR ");
				text.Append(typeof (EPApproval).Name).Append('.').Append(typeof (EPApproval.workgroupID).Name);
				text.Append(" IN ").Append(BqlCommand.SubSelect);
				text.Append(typeof (EPCompanyTreeH).Name).Append('.').Append(typeof (EPCompanyTreeH.workGroupID).Name);
				text.Append(" FROM ").Append(typeof (EPCompanyTreeH).Name).Append(' ').Append(typeof (EPCompanyTreeH).Name);
				text.Append(" INNER JOIN ").Append(typeof (EPCompanyTreeMember).Name).Append(' ').Append(typeof (EPCompanyTreeMember).Name);
				text.Append(" ON ").Append(typeof (EPCompanyTreeH).Name).Append('.').Append(typeof (EPCompanyTreeH.parentWGID).Name);
				text.Append('=').Append(typeof (EPCompanyTreeMember).Name).Append('.').Append(typeof (EPCompanyTreeMember.workGroupID).Name);
				text.Append(" AND ").Append(typeof (EPCompanyTreeH).Name).Append('.').Append(typeof (EPCompanyTreeH.parentWGID).Name);
				text.Append("!=").Append(typeof (EPCompanyTreeH).Name).Append('.').Append(typeof (EPCompanyTreeH.workGroupID).Name);
				text.Append(" AND ").Append(typeof (EPCompanyTreeMember).Name).Append('.').Append(typeof (EPCompanyTreeMember.active).Name).Append("=1");
				text.Append(" AND ").Append(typeof (EPCompanyTreeMember).Name).Append('.').Append(typeof (EPCompanyTreeMember.userID).Name);
				text.Append('=');
				ParseOperand(graph, pars, tables, fields, sortColumns, text, selection);
				text.Append(" WHERE 1=1");
				text.Append(')').Append(')').Append(')');
			}
			else
			{
				ParseOperand(graph, pars, tables, fields, sortColumns, text, selection);
				ParseOperand(graph, pars, tables, fields, sortColumns, text, selection);
			}
		}

		private void ParseOperand(PXGraph graph, List<IBqlParameter> pars, List<Type> tables, List<Type> fields, List<IBqlSortColumn> sortColumns, StringBuilder text, PX.Data.BqlCommand.Selection selection)
		{
			BqlCommand.EqualityList list = fields as BqlCommand.EqualityList;
			if (list != null) {
				list.NonStrict = true;
			}
			if (!typeof(IBqlCreator).IsAssignableFrom(typeof(Operand)))
			{
				if (graph != null && text != null)
					text.Append(" ").Append(BqlCommand.GetSingleField(typeof(Operand), graph, tables, selection));

				if (fields != null)
					fields.Add(typeof(Operand));
			}
			else
			{
				if (_operand == null)
					_operand = _operand.createOperand<Operand>();
				_operand.Parse(graph, pars, tables, fields, sortColumns, text, selection);
			}
		}
	}

}
