using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.CR.MassProcess;
using PX.SM;

namespace PX.Objects.CR
{
	public class CRSetupMaint : PXGraph<CRSetupMaint>
	{

		public PXSave<CRSetup> Save;
		public PXCancel<CRSetup> Cancel;
		public PXSelect<CRSetup> CRSetupRecord;
		public CM.CMSetupSelect CMSetup;

		public PXSelect<CRCampaignType> CampaignType;
		public PXSelectOrderBy<CROpportunityProbability, 
			OrderBy<Asc<CROpportunityProbability.probability>>> 
			OpportunityProbabilities;

		[PXHidden]
		public PXSelect<CRValidationRules> ValidationRules;

		public PXSelect<LeadContactValidationRules, Where<LeadContactValidationRules.validationType, Equal<ValidationTypesAttribute.leadContact>>> LeadContactValidationRules;
		public PXSelect<LeadAccountValidationRules, Where<LeadAccountValidationRules.validationType, Equal<ValidationTypesAttribute.leadAccount>>> LeadAccountValidationRules;
		public PXSelect<AccountValidationRules, Where<AccountValidationRules.validationType, Equal<ValidationTypesAttribute.account>>> AccountValidationRules;


		public CRSetupMaint()
		{
			InitRulesHandlres(LeadContactValidationRules);
			InitRulesHandlres(LeadAccountValidationRules);
			InitRulesHandlres(AccountValidationRules);
		}

		#region Event Handlers

		protected virtual void CRSetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PXUIFieldAttribute.SetVisible<CRSetup.defaultCuryID>(sender, null, CMSetup.Current.MCActivated == true);
			PXUIFieldAttribute.SetVisible<CRSetup.defaultRateTypeID>(sender, null, CMSetup.Current.MCActivated == true);
			PXUIFieldAttribute.SetVisible<CRSetup.allowOverrideCury>(sender, null, CMSetup.Current.MCActivated == true);
			PXUIFieldAttribute.SetVisible<CRSetup.allowOverrideRate>(sender, null, CMSetup.Current.MCActivated == true);
		}

		protected virtual void CRSetup_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			CROpportunityProbability p =  OpportunityProbabilities.Select();
            if (p == null)
            {
                p.StageCode = "A";
                p.Probability = 60;
                OpportunityProbabilities.Insert(p);
                p.StageCode = "L";
                p.Probability = 0;
                OpportunityProbabilities.Insert(p);
                p.StageCode = "P";
                p.Probability = 10;
                OpportunityProbabilities.Insert(p);
                p.StageCode = "Q";
                p.Probability = 20;
                OpportunityProbabilities.Insert(p);
                p.StageCode = "R";
                p.Probability = 80;
                OpportunityProbabilities.Insert(p);
                p.StageCode = "V";
                p.Probability = 40;
                OpportunityProbabilities.Insert(p);
                p.StageCode = "W";
                p.Probability = 100;
                OpportunityProbabilities.Insert(p);
                OpportunityProbabilities.Cache.IsDirty = false;
            }
		}

		private void InitRulesHandlres(PXSelectBase select)
		{
			Type cacheType = select.Cache.GetItemType();
			this.FieldSelecting.AddHandler(cacheType, typeof(CRValidationRules.matchingField).Name, (sender, e) => CreateFieldStateForFieldName(e.ReturnState, typeof(Contact)));
			this.RowInserted.AddHandler(cacheType, (sender, e) => UpdateGrammValidationDate());
			this.RowUpdated.AddHandler(cacheType, (sender, e) => UpdateGrammValidationDate());
			this.RowDeleted.AddHandler(cacheType, (sender, e) => UpdateGrammValidationDate());
		}

		
		protected virtual void CRSetup_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CRSetup row = e.Row as CRSetup;
			if (row != null && row.GrammValidationDateTime == null)			
				row.GrammValidationDateTime = PXTimeZoneInfo.Now;			
		}

		private void UpdateGrammValidationDate()
		{			
			CRSetup record = PXCache<CRSetup>.CreateCopy(this.CRSetupRecord.Current) as CRSetup;
			record.GrammValidationDateTime = null;			
			CRSetupRecord.Update(record);
		}

		#endregion

		private PXFieldState CreateFieldStateForFieldName(object returnState, Type type)
		{
			List<string> allowedValues = new List<string>();
			List<string> allowedLabels = new List<string>();

			Dictionary<string, string> fields = new Dictionary<string, string>();

			foreach (var field in PXCache.GetBqlTable(type)
						.GetProperties(BindingFlags.Instance | BindingFlags.Public)
						.SelectMany(p => p.GetCustomAttributes(true).Where(atr => atr is PXMassMergableFieldAttribute),(p, atr) => p))
			{

				PXFieldState fs = this.Caches[type].GetStateExt(null, field.Name) as PXFieldState;
				if(!fields.ContainsKey(field.Name))
					fields[field.Name] = fs != null ? fs.DisplayName : field.Name;
			}

			foreach (var item in fields.OrderBy(i => i.Value))
			{
				allowedValues.Add(item.Key);
				allowedLabels.Add(item.Value);
			}

			return PXStringState.CreateInstance(returnState, 60, null, "FieldName", false, 1, null,
												allowedValues.ToArray(), allowedLabels.ToArray(), true, null);
		}
	}
}
