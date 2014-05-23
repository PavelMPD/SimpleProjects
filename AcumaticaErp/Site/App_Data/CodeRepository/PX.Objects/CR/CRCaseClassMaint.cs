using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using System.Collections;
using System.Diagnostics;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.CS;

namespace PX.Objects.CR
{
	public class CRCaseClassMaint : PXGraph<CRCaseClassMaint, CRCaseClass>
	{
		[PXViewName(Messages.CaseClass)]
		public PXSelect<CRCaseClass>
			CaseClasses;

		[PXHidden]
		public PXSelect<CRCaseClass, 
			Where<CRCaseClass.caseClassID, Equal<Current<CRCaseClass.caseClassID>>>> 
			CaseClassesCurrent;

		[PXViewName(Messages.CaseClassReaction)]
		public PXSelect<CRClassSeverityTime, 
			Where<CRClassSeverityTime.caseClassID, Equal<Current<CRCaseClass.caseClassID>>>> 
			CaseClassesReaction;

		[PXViewName(Messages.Attributes)]
		public PXSelectJoin<CSAttributeGroup,
			InnerJoin<CSAttribute,
				On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
				Where<CSAttributeGroup.entityClassID, Equal<Current<CRCaseClass.caseClassID>>,
				And<CSAttributeGroup.type, Equal<CSAnswerType.caseAnswerType>>>,
				OrderBy<Asc<CSAttributeGroup.sortOrder>>>
				Mapping;
		
		[PXHidden]
		public PXSelect<CRSetup> 
			Setup;

		public PXSelect<CRCaseClassLaborMatrix, Where<CRCaseClassLaborMatrix.caseClassID, Equal<Current<CRCaseClass.caseClassID>>>>
			LaborMatrix;

		public PXAction<CRCaseClass> showDetails;
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
					throw new PXRedirectRequiredException(graph, Messages.CRAttributeMaint);
				}
			}

			return adapter.Get();
		}

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBLiteDefault(typeof(CRCaseClass.caseClassID))]
		protected virtual void CRClassSeverityTime_CaseClassID_CacheAttached(PXCache sender) { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBLiteDefault(typeof(CRCaseClass.caseClassID))]
		protected virtual void CSAttributeGroup_EntityClassID_CacheAttached(PXCache sender) { }

		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PXDefault(CSAnswerType.Case)]
		protected virtual void CSAttributeGroup_Type_CacheAttached(PXCache sender) { }

		public CRCaseClassMaint()
		{
			FieldDefaulting.AddHandler<IN.InventoryItem.stkItem>((sender, e) => { if (e.Row != null) e.NewValue = false; });
		}

		protected virtual void CRCaseClass_DefaultContractID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null) return;

			Contract contract = PXSelectReadonly<Contract,
				Where<Contract.contractID, Equal<Required<CRCase.contractID>>>>.
				Select(this, e.NewValue);

			if (contract != null)
			{
				int daysLeft;
				if (ContractMaint.IsExpired(contract, (DateTime)Accessinfo.BusinessDate))
				{
					e.NewValue = null;
					throw new PXSetPropertyException(Messages.ContractExpired, PXErrorLevel.Error);

				}
				if (ContractMaint.IsInGracePeriod(contract, (DateTime)Accessinfo.BusinessDate, out daysLeft))
				{
					e.NewValue = contract.ContractCD;
					throw new PXSetPropertyException(Messages.ContractInGracePeriod, PXErrorLevel.Warning, daysLeft);
				}
			}
		}

		protected virtual void CRCaseClass_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = e.Row as CRCaseClass;
			if (row == null) return;

			if (row.IsBillable == true)
			{
				row.RequireCustomer = true;
			}
		}

		protected virtual void CRCaseClass_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRCaseClass;
			if (row == null) return;

			Delete.SetEnabled(CanDelete(row));
		}

		protected virtual void CRCaseClass_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			var row = e.Row as CRCaseClass;
			if (row == null) return;

			CRSetup s = Setup.Select();

			if (s != null && s.DefaultCaseClassID == row.CaseClassID)
			{
				s.DefaultCaseClassID = null;
				Setup.Update(s);
			}
		}

		protected virtual void CRCaseClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var row = e.Row as CRCaseClass;
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
            if( row != null)
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

		private bool CanDelete(CRCaseClass row)
		{
			if (row != null)
			{
				CRCase c = PXSelect<CRCase, 
					Where<CRCase.caseClassID, Equal<Required<CRCase.caseClassID>>>>.
					SelectWindowed(this, 0, 1, row.CaseClassID);
				if (c != null)
				{
					return false;
				}
			}

			return true;
		}
	}
}
