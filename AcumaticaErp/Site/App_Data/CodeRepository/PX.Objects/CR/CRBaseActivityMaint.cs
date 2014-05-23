using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	public class CRBaseActivityMaint<TGraph> : PXGraph<TGraph>, IActivityMaint 
		where TGraph : PXGraph
	{
		#region Selects
		[PXHidden]
		public PXSelect<BAccount> BaseBAccount;

		[PXHidden]
		public PXSelect<AP.Vendor> BaseVendor;

		[PXHidden]
		public PXSelect<AR.Customer> BaseCustomer;

		[PXHidden]
		public PXSelect<EPEmployee> BaseEmployee;
		
		[PXHidden]
		public EPViewSelect<EPActivity, EPActivity> EPViews;

		[PXHidden]
		public PXSelect<CRActivityStatistics> Stats;
		#endregion

		#region Ctor
		public CRBaseActivityMaint()
		{
			Views.Caches.Remove(typeof(CRActivityStatistics));
			Views.Caches.Add(typeof(CRActivityStatistics));
		}
		#endregion

		#region Actions
		public PXSave<EPActivity> Save;
		public PXSaveClose<EPActivity> SaveClose;
		public PXCancel<EPActivity> Cancel;
		public PXInsert<EPActivity> Insert;

		public PXAction<EPActivity> GotoEntity;
		[PXUIField(DisplayName = Messages.ViewEntity, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = Messages.ttipViewEntity)]
		protected virtual void gotoEntity()
		{
			EPActivity row = (EPActivity)Caches[typeof(EPActivity)].Current;
			if (row == null) return;

			new EntityHelper(this).NavigateToRow(row.RefNoteID, PXRedirectHelper.WindowMode.NewWindow);
		}
		
		public PXAction<EPActivity> GotoParentActivity;
		[PXUIField(DisplayName = "View Parent", MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = "View Parent Activity")]
		protected void gotoParentActivity()
		{
			EPActivity row = (EPActivity)Caches[typeof(EPActivity)].Current;
			if (row == null || row.ParentTaskID == null) return;

			EPActivity parentActivity = PXSelect<EPActivity, Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.Select(this, row.ParentTaskID);
			if (parentActivity != null && parentActivity.NoteID != null)
				new EntityHelper(this).NavigateToRow(parentActivity.NoteID, PXRedirectHelper.WindowMode.NewWindow);
		}
		#endregion

		#region IActivityMaint implementation
		public virtual void CancelRow(EPActivity row)
		{
		}

		public virtual void CompleteRow(EPActivity row)
		{
		}
		#endregion
	}
}
