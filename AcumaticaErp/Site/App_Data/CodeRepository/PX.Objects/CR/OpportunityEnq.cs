using System;
using PX.Data;
using PX.SM;

namespace PX.Objects.CR
{
	[DashboardType(PX.TM.OwnedFilter.DASHBOARD_TYPE, GL.TableAndChartDashboardTypeAttribute._AMCHARTS_DASHBOART_TYPE)]
	public class OpportunityEnq : PXGraph<OpportunityEnq>
	{
		#region Selects
		[PXHidden]
		public PXSelect<BAccount> BAccount;

		[PXViewName(Messages.Selection)]
		public PXFilter<OwnedFilter> 
			Filter;

		[PXViewName(Messages.Opportunities)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(OwnedFilter))]
		[PXViewDetailsButton(typeof(OwnedFilter),
				typeof(Select<BAccountCRM,
					Where<BAccountCRM.bAccountID, Equal<Current<CROpportunity.bAccountID>>>>),
				ActionName = "FilteredItems_BAccount_ViewDetails")]
		[PXViewDetailsButton(typeof(OwnedFilter),
			typeof(Select<BAccountCRM,
				Where<BAccountCRM.bAccountID, Equal<Current<CROpportunity.parentBAccountID>>>>),
				ActionName = "FilteredItems_BAccountParent_ViewDetails")]
		public PXOwnerFilteredSelectReadonly<OwnedFilter,
				Select2<CROpportunity,
				LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CROpportunity.bAccountID>>,
				LeftJoin<BAccountParent, On<BAccountParent.bAccountID, Equal<CROpportunity.parentBAccountID>>, 
				LeftJoin<CROpportunityProbability, On<CROpportunityProbability.stageCode, Equal<CROpportunity.stageID>>,
				LeftJoin<CRActivityStatistics, On<CROpportunity.noteID, Equal<CRActivityStatistics.noteID>>>>>>>,
				CROpportunity.workgroupID, CROpportunity.ownerID> 
			FilteredItems;
		#endregion

		#region Ctor

		public OpportunityEnq()
		{
			FilteredItems.NewRecordTarget = typeof(OpportunityMaint);

			PXDBAttributeAttribute.Activate(FilteredItems.Cache);
			PXDBAttributeAttribute.Activate(this.Caches[typeof(BAccount)]);
			PXDBAttributeAttribute.Activate(this.Caches[typeof(BAccountParent)]);

			var bAccountCache = Caches[typeof(BAccount)];
			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountCache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountCache, Messages.BAccountName);

			var parentBAccountCache = Caches[typeof(BAccountParent)];
			PXUIFieldAttribute.SetDisplayName<BAccountParent.acctCD>(parentBAccountCache, Messages.ParentAccount);
			PXUIFieldAttribute.SetDisplayName<BAccountParent.acctName>(parentBAccountCache, Messages.ParentAccountName);
			 
		}

		#endregion

		#region Actions
		public PXCancel<OwnedFilter> Cancel;
		#endregion

		#region Overrides

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		#endregion
	}
}
