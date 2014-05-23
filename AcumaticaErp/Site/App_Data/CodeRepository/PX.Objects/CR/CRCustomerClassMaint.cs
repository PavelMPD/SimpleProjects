using System.Collections.Generic;
using PX.Data;
using System.Collections;
using PX.Objects.CS;

namespace PX.Objects.CR
{
	public class CRCustomerClassMaint : PXGraph<CRCustomerClassMaint, CRCustomerClass>
	{

		[PXViewName(Messages.CustomerClass)]
		public PXSelect<CRCustomerClass>
			CustomerClass;

		[PXViewName(Messages.Attributes)]
		public PXSelectJoin<CSAttributeGroup,
			InnerJoin<CSAttribute,
			On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
			Where<CSAttributeGroup.entityClassID, Equal<Current<CRCustomerClass.cRCustomerClassID>>,
				And<CSAttributeGroup.type, Equal<CSAnswerType.accountAnswerType>>>,
			OrderBy<Asc<CSAttributeGroup.sortOrder>>>
			Mapping;

		[PXHidden]
		public PXSelect<CRSetup> 
			Setup;

		public PXAction<CRCustomerClass> showDetails;
		[PXUIField(DisplayName = Messages.Details,
			MapEnableRights = PXCacheRights.Select,
			MapViewRights = PXCacheRights.Select,
			Visible = false)]
		[PXLookupButton]
		public virtual IEnumerable ShowDetails(PXAdapter adapter)
		{
			var current = Mapping.Current;
			if (current != null && current.AttributeID != null)
			{
				var attibute = (CSAttribute)PXSelect<CSAttribute,
					Where<CSAttribute.attributeID, Equal<Required<CSAttribute.attributeID>>>>.
					Select(this, current.AttributeID);

				if (attibute != null)
				{
					var graph = PXGraph.CreateInstance<CSAttributeMaint>();
					graph.Clear();
					graph.Attributes.Current = attibute;
                    throw new PXRedirectRequiredException(graph, Messages.CRAttributeMaint) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			}

			return adapter.Get();
		}

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBLiteDefault(typeof(CRCustomerClass.cRCustomerClassID))]
		protected virtual void CSAttributeGroup_EntityClassID_CacheAttached(PXCache sender) { }

		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PXDefault(CSAnswerType.Account)]
		protected virtual void CSAttributeGroup_Type_CacheAttached(PXCache sender) { }

		protected virtual void CRCustomerClass_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRCustomerClass;
			if (row == null) return;
			
			Delete.SetEnabled(CanDelete(row));
		}

		protected virtual void CRCustomerClass_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			var row = e.Row as CRCustomerClass;
			if (row == null) return;
			
			CRSetup s = Setup.Select();

			if (s != null && s.DefaultCustomerClassID == row.CRCustomerClassID)
			{
				s.DefaultCustomerClassID = null;
				Setup.Update(s);
			}
		}

		protected virtual void CRCustomerClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var row = e.Row as CRCustomerClass;
			if (row == null) return;
			
			if (!CanDelete(row))
			{
				throw new PXException(Messages.RecordIsReferenced);
			}
		}

		protected virtual void CSAttributeGroup_DefaultValue_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			string AnswerValueField = "DefaultValue";
			int AnswerValueLength = 60;
			CSAttributeGroup row = e.Row as CSAttributeGroup;
			if (row != null)
			{
				CSAttribute question = new PXSelect<CSAttribute>(this).Search<CSAttribute.attributeID>(row.AttributeID);
				PXResultset<CSAttributeDetail> options = PXSelect<CSAttributeDetail,
					Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeGroup.attributeID>>>,
					OrderBy<Asc<CSAttributeDetail.sortOrder>>>.Select(this, row.AttributeID);

				int required = -1;
				if ((bool)row.Required)
				{
					required = 1;
				}

				if (options.Count > 0)
				{
					//ComboBox:

					List<string> allowedValues = new List<string>();
					List<string> allowedLabels = new List<string>();

					foreach (CSAttributeDetail option in options)
					{
						allowedValues.Add(option.ValueID);
						allowedLabels.Add(option.Description);
					}

					string mask = question != null ? question.EntryMask : null;

					e.ReturnState = PXStringState.CreateInstance(e.ReturnState, CSAttributeDetail.ParameterIdLength, true, AnswerValueField, false, required, mask, allowedValues.ToArray(), allowedLabels.ToArray(), true, null);
					if (question.ControlType == CSAttribute.MultiSelectCombo)
					{
						((PXStringState)e.ReturnState).MultiSelect = true;
					}
				}
				else if (question != null)
				{
					if (question.ControlType.Value == CSAttribute.CheckBox)
					{
						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(bool), false, false, required, null, null, false, AnswerValueField, null, null, null, PXErrorLevel.Undefined, true, true, null, PXUIVisibility.Visible, null, null, null);
					}
					else if (question.ControlType.Value == CSAttribute.Datetime)
					{
						string mask = question != null ? question.EntryMask : null;
						e.ReturnState = PXDateState.CreateInstance(e.ReturnState, AnswerValueField, false, required, mask, mask, null, null);
					}
					else
					{
						//TextBox:
						string mask = question != null ? question.EntryMask : null;

						e.ReturnState = PXStringState.CreateInstance(e.ReturnState, AnswerValueLength, null, AnswerValueField, false, required, mask, null, null, true, null);
					}
				}

			}
		}

		private bool CanDelete(CRCustomerClass row)
		{
			if (row != null)
			{
				BAccount c = PXSelect<BAccount, 
					Where<BAccount.classID, Equal<Required<BAccount.classID>>>>.
					SelectWindowed(this, 0, 1, row.CRCustomerClassID);
				if (c != null)
				{
					return false;
				}
			}

			return true;
		}
	}
}
