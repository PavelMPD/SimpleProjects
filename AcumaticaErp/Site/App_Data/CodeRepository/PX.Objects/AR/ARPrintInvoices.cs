using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.TM;
using PX.Objects.CR;

namespace PX.Objects.AR
{
	[TableAndChartDashboardType]
	public class ARPrintInvoices : PXGraph<ARPrintInvoices>
	{
		public PXFilter<PrintInvoicesFilter> Filter;
		public PXCancel<PrintInvoicesFilter> Cancel;
		[PXFilterable]
		public PXFilteredProcessing<ARInvoice, PrintInvoicesFilter> ARDocumentList;	
		public PXSetup<ARSetup> arsetup;

		public PXAction<PrintInvoicesFilter> ViewDocument;
		[PXUIField(DisplayName = "View Document", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable viewDocument(PXAdapter adapter)
		{
			if (ARDocumentList.Current != null)
			{
				PXRedirectHelper.TryRedirect(ARDocumentList.Cache, ARDocumentList.Current, "Document", PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		public ARPrintInvoices()
		{
			ARSetup setup = arsetup.Current;
			PXUIFieldAttribute.SetEnabled(ARDocumentList.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<ARInvoice.selected>(ARDocumentList.Cache, null, true);
			ARDocumentList.Cache.AllowInsert = false;
			ARDocumentList.Cache.AllowDelete = false;

			ARDocumentList.SetSelected<ARInvoice.selected>();
			ARDocumentList.SetProcessCaption(IN.Messages.Process);
			ARDocumentList.SetProcessAllCaption(IN.Messages.ProcessAll);
			
		}

		public virtual IEnumerable ardocumentlist(PXAdapter adapter)
		{						

			Type where = PX.TM.OwnedFilter.ProjectionAttribute.ComposeWhere(
				typeof(PrintInvoicesFilter),
				typeof(ARInvoice.workgroupID), 
				typeof(ARInvoice.ownerID));

			Type printWhere = 
				typeof(Where<ARInvoice.hold, Equal<False>, And<ARInvoice.scheduled, Equal<False>, And<ARInvoice.voided, Equal<False>,
								 And<ARInvoice.dontPrint, Equal<boolFalse>,
								 And<ARInvoice.printed, NotEqual<True>>>>>>);
			Type emailWhere =
				typeof(Where<ARInvoice.hold, Equal<False>, And<ARInvoice.scheduled, Equal<False>, And<ARInvoice.voided, Equal<False>,
								 And<ARInvoice.dontEmail, Equal<boolFalse>,
								 And<ARInvoice.emailed, NotEqual<True>>>>>>);
		
		
			string onlyNotPrinted = (string)ARDocumentList.GetTargetFill(null, null, null, Filter.Current.Action, "@OnlyNotPrinted");
			string onlyNotEmailed = (string)ARDocumentList.GetTargetFill(null, null, null, Filter.Current.Action, "@OnlyNotEmailed");

			Type whereAnd = Filter.Current.Action == "<SELECT>" ? typeof(Where<True, Equal<False>>) : typeof(Where<True, Equal<True>>);
		
			if (onlyNotEmailed != null)
				whereAnd = emailWhere;

			if (onlyNotPrinted!= null)
				whereAnd = printWhere;
			
			Type select =
				BqlCommand.Compose(
					typeof(Select2<,,>), typeof(ARInvoice),
					typeof(InnerJoin<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>>),
					typeof(Where<>), whereAnd);

			PXView view = new PXView(this, false, BqlCommand.CreateInstance(select));
			return view.SelectMulti();
		}		
		
		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}
		protected virtual void PrintInvoicesFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PrintInvoicesFilter o = (PrintInvoicesFilter)e.Row;
			
			if (o != null && !String.IsNullOrEmpty(o.Action))
			{
				ARDocumentList.SetProcessTarget(null, null, null, o.Action);
			}
		}
	}

    [Serializable]
	public partial class PrintInvoicesFilter : IBqlTable
	{
		#region CurrentOwnerID
		public abstract class currentOwnerID : PX.Data.IBqlField
		{
		}
		[PXDBGuid]
		public virtual Guid? CurrentOwnerID
		{
			get
			{
				return PXAccess.GetUserID();
			}
		}
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.IBqlField
		{
		}
		protected Guid? _OwnerID;
		[PXDBGuid]
		[PXUIField(DisplayName = "Assigned To")]
		[PX.TM.PXSubordinateOwnerSelector]
		public virtual Guid? OwnerID
		{
			get
			{
				return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
			}
			set
			{
				_OwnerID = value;
			}
		}
		#endregion
		#region MyOwner
		public abstract class myOwner : PX.Data.IBqlField
		{
		}
		protected Boolean? _MyOwner;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Me")]
		public virtual Boolean? MyOwner
		{
			get
			{
				return _MyOwner;
			}
			set
			{
				_MyOwner = value;
			}
		}
		#endregion
		#region WorkGroupID
		public abstract class workGroupID : PX.Data.IBqlField
		{
		}
		protected Int32? _WorkGroupID;
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>),
		 SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual Int32? WorkGroupID
		{
			get
			{
				return (_MyWorkGroup == true) ? null : _WorkGroupID;
			}
			set
			{
				_WorkGroupID = value;
			}
		}
		#endregion
		#region MyWorkGroup
		public abstract class myWorkGroup : PX.Data.IBqlField
		{
		}
		protected Boolean? _MyWorkGroup;
		[PXDefault(false)]
		[PXDBBool]
		[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyWorkGroup
		{
			get
			{
				return _MyWorkGroup;
			}
			set
			{
				_MyWorkGroup = value;
			}
		}
		#endregion
		#region FilterSet
		public abstract class filterSet : PX.Data.IBqlField
		{
		}
		[PXDefault(false)]
		[PXDBBool]
        public virtual Boolean? FilterSet
		{
			get
			{
				return
					this.OwnerID != null ||
					this.WorkGroupID != null ||
					this.MyWorkGroup == true;
			}
		}
		#endregion
		#region Action
		public abstract class action : PX.Data.IBqlField
		{
		}
		protected string _Action;
		[PXAutomationMenu]
		public virtual string Action
		{
			get
			{
				return this._Action;
			}
			set
			{
				this._Action = value;
			}
		}
		#endregion
	}
}
