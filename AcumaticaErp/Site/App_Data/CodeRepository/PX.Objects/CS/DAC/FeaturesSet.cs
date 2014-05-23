using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.CS
{
	[PXPrimaryGraph(typeof(FeaturesMaint))]
    [Serializable]
	public class FeaturesSet : IBqlTable
	{
		#region LicenseID
		public abstract class licenseID : PX.Data.IBqlField
		{
		}
		protected String _LicenseID;
		[PXString(64, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "License ID", Visible = false)]
		public virtual String LicenseID
		{
			get
			{
				return this._LicenseID;
			}
			set
			{
				this._LicenseID = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXDefault(3)]
		[PXIntList(
			new int[] { 0, 1, 2, 3 },
			new string[] { "Validated", "Failed Validation", "Pending Validation", "Pending Activation" }
		)]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		public int? Status
		{
			get;
			set;
		}
		#endregion
		#region ValidUntill
		public abstract class validUntill : PX.Data.IBqlField
		{
		}
		protected DateTime? _ValidUntill;
		[PXDBDate()]
		[PXUIField(DisplayName = "Next Validation Date", Enabled = false, Visible = false)]
		public virtual DateTime? ValidUntill
		{
			get
			{
				return this._ValidUntill;
			}
			set
			{
				this._ValidUntill = value;
			}
		}
		#endregion		
		#region ValidationCode
		public abstract class validationCode : PX.Data.IBqlField
		{
		}
		protected String _ValidationCode;
		[PXString(500, IsUnicode = true, InputMask = "")]
		public virtual String ValidationCode
		{
			get
			{
				return this._ValidationCode;
			}
			set
			{
				this._ValidationCode = value;
			}
		}
		#endregion

		#region OrganizationModule
		public abstract class organizationModule : PX.Data.IBqlField
		{
		}
		protected bool? _OrganizationModule;
		[Feature(true, DisplayName = "Organization", Enabled = false)]
		public virtual bool? OrganizationModule
		{
			get
			{
				return this._OrganizationModule;
			}
			set
			{
				this._OrganizationModule = value;
			}
		}
		#endregion

		#region FinancialModule
		public abstract class financialModule : PX.Data.IBqlField
		{
		}
		protected bool? _FinancialModule;
		[Feature(true, null, typeof(Select<GL.GLSetup>), DisplayName = "Finance", Enabled = false)]
		public virtual bool? FinancialModule
		{
			get
			{
				return this._FinancialModule;
			}
			set
			{
				this._FinancialModule = value;
			}
		}
		#endregion
		#region Branch
		public abstract class branch : PX.Data.IBqlField
		{
		}
		protected bool? _Branch;
		[Feature(typeof(FeaturesSet.financialModule), typeof(Select2<CS.BranchMaint.BranchBAccount,
			InnerJoin<GL.Branch, On<GL.Branch.bAccountID, NotEqual<CS.BranchMaint.BranchBAccount.bAccountID>>>>), 
			DisplayName = "Multi-Branch Support")]
		public virtual bool? Branch
		{
			get
			{
				return this._Branch;
			}
			set
			{
				this._Branch = value;
			}
		}
		#endregion
		#region Inter-Branch Accounting
		public abstract class interBranch : PX.Data.IBqlField
		{
             
		}
		//[FeatureRestrictor(typeof(Select2<GL.Ledger, InnerJoin<GL.Branch, On<GL.Branch.branchID, Equal<GL.Ledger.defBranchID>>>>))]
		//[FeatureRestrictor(typeof(Select2<GL.Ledger, InnerJoin<GL.GLHistory, On<GL.GLHistory.ledgerID, Equal<GL.Ledger.ledgerID>>>, 
		//	Where<GL.Ledger.balanceType,Equal<GL.LedgerBalanceType.actual>, And<GL.Ledger.postInterCompany, Equal<True>>>>))]
        [FeatureRestrictor(typeof(Select2<GL.BranchAcctMap, InnerJoin<GL.Branch, On<GL.Branch.branchID, Equal<GL.BranchAcctMap.branchID>, And<GL.Branch.active, Equal<True>>>>>))]
		[Feature(typeof(FeaturesSet.branch), DisplayName = "Inter-Branch Transactions")]
		public virtual bool? InterBranch { get; set; }
		#endregion
		#region Multicurrency
		public abstract class multicurrency : PX.Data.IBqlField
		{
		}
		protected bool? _Multicurrency;
		[Feature(typeof(FeaturesSet.financialModule), typeof(Select<CM.CMSetup>), DisplayName = "Multi-Currency Accounting")]
		public virtual bool? Multicurrency
		{
			get
			{
				return this._Multicurrency;
			}
			set
			{
				this._Multicurrency = value;
			}
		}
		#endregion
		#region DefferedRevenue
		public abstract class defferedRevenue : PX.Data.IBqlField
		{
		}
		protected bool? _DefferedRevenue;
		[Feature(typeof(FeaturesSet.financialModule), typeof(Select<DR.DRSchedule>), DisplayName = "Deferred Revenue Management")]
		public virtual bool? DefferedRevenue
		{
			get
			{
				return this._DefferedRevenue;
			}
			set
			{
				this._DefferedRevenue = value;
			}
		}
		#endregion
		#region SubAccount
		public abstract class subAccount : PX.Data.IBqlField
		{
		}
		protected bool? _SubAccount;
		[Feature(typeof(FeaturesSet.financialModule), typeof(Select<GL.Sub, Where<GL.Sub.subCD,  NotEqual<IN.INSubItem.Zero>>>), DisplayName = "Subaccounts")]
		public virtual bool? SubAccount
		{
			get
			{
				return this._SubAccount;
			}
			set
			{
				this._SubAccount = value;
			}
		}
		#endregion
		#region FixedAsset
		public abstract class fixedAsset : PX.Data.IBqlField
		{
		}
		protected bool? _FixedAsset;
		[Feature(typeof(FeaturesSet.financialModule), typeof(Select<FA.FASetup>), DisplayName = "Fixed Assets Management")]
		public virtual bool? FixedAsset
		{
			get
			{
				return this._FixedAsset;
			}
			set
			{
				this._FixedAsset = value;
			}
		}
		#endregion
		#region VATReporting
		public abstract class vATReporting : PX.Data.IBqlField
		{
		}
		protected bool? _VATReporting;
		[Feature(typeof(FeaturesSet.financialModule), typeof(Select<TX.Tax, Where<TX.Tax.taxType, Equal<TX.CSTaxType.vat>>>), DisplayName = "VAT Reporting")]
		public virtual bool? VATReporting
		{
			get
			{
				return this._VATReporting;
			}
			set
			{
				this._VATReporting = value;
			}
		}
		#endregion
		#region InvoiceRounding
		public abstract class invoiceRounding : PX.Data.IBqlField
		{
		}
		protected bool? _InvoiceRounding;
		[Feature(typeof(FeaturesSet.financialModule), DisplayName = "Invoice Rounding")]
		public virtual bool? InvoiceRounding
		{
			get;
			set;
		}
		#endregion
		#region Prebooking
		public abstract class prebooking : PX.Data.IBqlField
		{
		}
		protected bool? _Prebooking;
        [Feature(typeof(FeaturesSet.financialModule), typeof(Select<AP.APRegister, Where<AP.APRegister.prebookBatchNbr, IsNotNull>>), DisplayName = "Support for Expense Reclassification")]
		public virtual bool? Prebooking
		{
			get;
			set;
		}
		#endregion
        #region Contract
        public abstract class contractManagement : PX.Data.IBqlField
        {
        }
        protected bool? _ContractManagement;
				[Feature(typeof(FeaturesSet.financialModule), typeof(Select<CT.Contract, Where<CT.Contract.nonProject, Equal<False>, And<CT.Contract.baseType, Equal<CT.Contract.ContractBaseType>>>>), DisplayName = "Contract Management")]
        public virtual bool? ContractManagement
        {
            get;
            set;
        }
        #endregion

		#region DistributionModule
		public abstract class distributionModule : PX.Data.IBqlField
		{
		}
		protected bool? _DistributionModule;
		[Feature(typeof(FeaturesSet.financialModule), typeof(Select<IN.INSetup>), Top = true, DisplayName = "Distribution")]
		public virtual bool? DistributionModule
		{
			get
			{
				return this._DistributionModule;
			}
			set
			{
				this._DistributionModule = value;
			}
		}
		#endregion
		#region SubItem
		public abstract class subItem : PX.Data.IBqlField
		{
		}
		protected bool? _SubItem;
		[Feature(typeof(FeaturesSet.distributionModule), typeof(Select<IN.INSubItem, Where<IN.INSubItem.subItemCD, NotEqual<IN.INSubItem.Zero>>>), DisplayName = "Inventory Subitems")]
		public virtual bool? SubItem
		{
			get
			{
				return this._SubItem;
			}
			set
			{
				this._SubItem = value;
			}
		}
		#endregion
		#region AutoPackaging
		public abstract class autoPackaging : PX.Data.IBqlField
		{
		}
		protected bool? _AutoPackaging;
		[Feature(typeof(FeaturesSet.distributionModule), DisplayName = "Automatic Packaging")]
		public virtual bool? AutoPackaging
		{
			get
			{
				return this._AutoPackaging;
			}
			set
			{
				this._AutoPackaging = value;
			}
		}
		#endregion
		#region Warehouse
		public abstract class warehouse : PX.Data.IBqlField
		{
		}
		protected bool? _Warehouse;
		[Feature(true, typeof(FeaturesSet.distributionModule), typeof(Select<IN.INSite, Where<IN.INSite.siteCD, NotEqual<IN.INSite.main>>>), DisplayName = "Warehouses")]
		public virtual bool? Warehouse
		{
			get
			{
				return this._Warehouse;
			}
			set
			{
				this._Warehouse = value;
			}
		}
		#endregion
		#region WarehouseLocation
		public abstract class warehouseLocation : PX.Data.IBqlField
		{
		}
		protected bool? _WarehouseLocation;
		[Feature(true, typeof(FeaturesSet.warehouse), typeof(Select<IN.INLocation, Where<IN.INLocation.locationCD, NotEqual<IN.INLocation.main>>>), DisplayName = "Warehouse Locations")]
		public virtual bool? WarehouseLocation
		{
			get
			{
				return this._WarehouseLocation;
			}
			set
			{
				this._WarehouseLocation = value;
			}
		}
		#endregion
		#region BlanketPO
		public abstract class blanketPO : PX.Data.IBqlField
		{
		}
		protected bool? _BlanketPO;
		[Feature(false, typeof(FeaturesSet.distributionModule), typeof(Select<PO.POOrder,Where<PO.POOrder.orderType, Equal<PO.POOrderType.blanket>, Or<PO.POOrder.orderType, Equal<PO.POOrderType.standardBlanket>>>>), DisplayName = "Blanket Purchase Orders")]
		public virtual bool? BlanketPO
		{
			get
			{
				return this._BlanketPO;
			}
			set
			{
				this._BlanketPO = value;
			}
		}
		#endregion
		#region DropShipments
		public abstract class dropShipments : PX.Data.IBqlField
		{
		}
		protected bool? _DropShipments;
		[Feature(false, typeof(FeaturesSet.distributionModule), typeof(Select<PO.POOrder, Where<PO.POOrder.orderType, Equal<PO.POOrderType.dropShip>>>), DisplayName = "Drop Shipments")]
		public virtual bool? DropShipments
		{
			get
			{
				return this._DropShipments;
			}
			set
			{
				this._DropShipments = value;
			}
		}
		#endregion
		#region MultipleUnitMeasure
		public abstract class multipleUnitMeasure : PX.Data.IBqlField
		{
		}
		protected bool? _MultipleUnitMeasure;
		[FeatureRestrictor(typeof(Select<IN.InventoryItem, Where<IN.InventoryItem.baseUnit, NotEqual<IN.InventoryItem.salesUnit>, Or<IN.InventoryItem.baseUnit, NotEqual<IN.InventoryItem.purchaseUnit>>>>))]
		[FeatureRestrictor(typeof(Select<IN.INItemClass, Where<IN.INItemClass.baseUnit, NotEqual<IN.INItemClass.salesUnit>, Or<IN.INItemClass.baseUnit, NotEqual<IN.INItemClass.purchaseUnit>>>>))]
		[FeatureRestrictor(typeof(
			Select2<
				IN.InventoryItem
				, InnerJoin<IN.INItemSite, On<IN.InventoryItem.inventoryID, Equal<IN.INItemSite.inventoryID>>>
				, Where<IN.InventoryItem.baseUnit, NotEqual<IN.INItemSite.dfltSalesUnit>, Or<IN.InventoryItem.baseUnit, NotEqual<IN.INItemSite.dfltPurchaseUnit>>>
				>
			))]

		[Feature(false, typeof(FeaturesSet.distributionModule), DisplayName = "Multiple Unit of Measure")]
		public virtual bool? MultipleUnitMeasure
		{
			get
			{
				return this._MultipleUnitMeasure;
			}
			set
			{
				this._MultipleUnitMeasure = value;
			}
		}
		#endregion



		#region MiscModule
		public abstract class miscModule : PX.Data.IBqlField
		{
		}
		protected bool? _MiscModule;
		[Feature(true,DisplayName = "Misc", Enabled = false)]
		public virtual bool? MiscModule
		{
			get
			{
				return this._MiscModule;
			}
			set
			{
				this._MiscModule = value;
			}
		}
		#endregion
		#region RowLevelSecurity
		public abstract class rowLevelSecurity : PX.Data.IBqlField
		{
		}
		protected bool? _RowLevelSecurity;
		[Feature(typeof(FeaturesSet.miscModule), typeof(Select<PX.SM.RelationGroup>), DisplayName = "Row-Level Security")]
		public virtual bool? RowLevelSecurity
		{
			get
			{
				return this._RowLevelSecurity;
			}
			set
			{
				this._RowLevelSecurity = value;
			}
		}
		#endregion
		#region FieldLevelLogging
		public abstract class fieldLevelLogging : PX.Data.IBqlField
		{
		}
		protected bool? _FieldLevelLogging;
		[Feature(typeof(FeaturesSet.miscModule), DisplayName = "Field-Level Audit")]
		public virtual bool? FieldLevelLogging
		{
			get
			{
				return this._FieldLevelLogging;
			}
			set
			{
				this._FieldLevelLogging = value;
			}
		}
		#endregion
		#region AvalaraTax
		public abstract class avalaraTax : PX.Data.IBqlField
		{
		}
		protected bool? _AvalaraTax;
		[Feature(typeof(FeaturesSet.miscModule), DisplayName = "Avalara Tax Integration")]
		public virtual bool? AvalaraTax
		{
			get
			{
				return this._AvalaraTax;
			}
			set
			{
				this._AvalaraTax = value;
			}
		}
		#endregion		
		#region AddressValidation
		public abstract class addressValidation : PX.Data.IBqlField
		{
		}
		protected bool? _AddressValidation;
		[Feature(typeof(FeaturesSet.avalaraTax), DisplayName = "Address Validation")]
		public virtual bool? AddressValidation
		{
			get
			{
				return this._AddressValidation;
			}
			set
			{
				this._AddressValidation = value;
			}
		}
		#endregion
		#region NotificationModule
		public abstract class notificationModule : PX.Data.IBqlField
		{
		}
		protected bool? _NotificationModule;
		[Feature(false, typeof(FeaturesSet.miscModule), typeof(Select<PX.SM.AUNotification, Where<PX.SM.AUNotification.isActive, Equal<True>>>), DisplayName = "Notification Module")]
		public virtual bool? NotificationModule
		{
			get
			{
				return this._NotificationModule;
			}
			set
			{
				this._NotificationModule = value;
			}
		}
		#endregion
		

		#region CustomerModule
		public abstract class customerModule : PX.Data.IBqlField
		{
		    public const string FieldClass = "CRM";
		}
		protected bool? _CustomerModule;
		[Feature(typeof(FeaturesSet.organizationModule), typeof(Select<CR.CRSetup>), DisplayName = "Customer Management")]
		public virtual bool? CustomerModule
		{
			get
			{
				return this._CustomerModule;
			}
			set
			{
				this._CustomerModule = value;
			}
		}
		#endregion
		#region ContactDuplicate
		public abstract class contactDuplicate : PX.Data.IBqlField
		{			
		}
		protected bool? _ContactDuplicate;
		[Feature(typeof(FeaturesSet.customerModule), DisplayName = "Lead/Contact Duplicate Validation", Visible = false)]
		public virtual bool? ContactDuplicate
		{
			get
			{
				return this._ContactDuplicate;
			}
			set
			{
				this._ContactDuplicate = value;
			}
		}
		#endregion
		#region ProjectModule
		public abstract class projectModule : PX.Data.IBqlField
		{
		}
		protected bool? _ProjectModule;
		[Feature(typeof(FeaturesSet.organizationModule), typeof(Select<PM.PMProject, Where<PM.PMProject.baseType, Equal<PM.PMProject.ProjectBaseType>, And<PM.PMProject.nonProject, Equal<False>>>>), DisplayName = "Project Management")]
		public virtual bool? ProjectModule
		{
			get
			{
				return this._ProjectModule;
			}
			set
			{
				this._ProjectModule = value;
			}
		}
		#endregion		
		#region TimeReportingModule
		public abstract class timeReportingModule : PX.Data.IBqlField
		{
		}
		protected bool? _TimeReportingModule;
		[Feature(false, typeof(FeaturesSet.organizationModule), DisplayName = "Time Reporting on Activity")]
		public virtual bool? TimeReportingModule
		{
			get
			{
				return this._TimeReportingModule;
			}
			set
			{
				this._TimeReportingModule = value;
			}
		}
		#endregion
		#region AccountLocations
		public abstract class accountLocations : PX.Data.IBqlField
		{
		}
		protected bool? _AccountLocations;
		[Feature(true, typeof(FeaturesSet.organizationModule), DisplayName = "Account Locations", Visible=false)]
		public virtual bool? AccountLocations
		{
			get
			{
				return this._AccountLocations;
			}
			set
			{
				this._AccountLocations = value;
			}
		}
		#endregion
        #region TaxEntryFromGL
        public abstract class taxEntryFromGL : PX.Data.IBqlField
        {
        }
        protected bool? _TaxEntryFromGL;
        [Feature(false,typeof(FeaturesSet.financialModule),typeof(Select<GL.GLTran,Where<GL.GLTran.taxID,IsNotNull>>), DisplayName = "Tax Entry from GL Module")]
        public virtual bool? TaxEntryFromGL
        {
            get
            {
                return this._TaxEntryFromGL;
            }
            set
            {
                this._TaxEntryFromGL = value;
            }
        }
        #endregion		
        #region RutRotDeduction
        public abstract class rutRotDeduction : PX.Data.IBqlField { }
        protected bool? _RutRotDeduction;
        [Feature(false, typeof(FeaturesSet.financialModule), typeof(Select<GL.Branch,Where<GL.Branch.allowsRUTROT,Equal<boolTrue>>>), DisplayName = "ROT & RUT Deduction")]
        public virtual bool? RutRotDeduction
        {
            get
            {
                return this._RutRotDeduction;
            }
            set
            {
                this._RutRotDeduction = value;
            }
        }
        #endregion		
        #region VendorDiscounts
        public abstract class vendorDiscounts : PX.Data.IBqlField { }
        protected bool? _VendorDiscounts;
        [Feature(false, typeof(FeaturesSet.financialModule), DisplayName = "Vendor Discounts", Visible = false)]
        public virtual bool? VendorDiscounts
        {
            get
            {
                return this._VendorDiscounts;
            }
            set
            {
                this._VendorDiscounts = value;
            }
        }
        #endregion		
        #region ConsolidatedPosting
        public abstract class consolidatedPosting : PX.Data.IBqlField { }
        protected bool? _ConsolidatedPosting;
        [Feature(false, typeof(FeaturesSet.financialModule), DisplayName = "Consolidated Posting to GL")]
        public virtual bool? ConsolidatedPosting
        {
            get
            {
                return this._ConsolidatedPosting;
            }
            set
            {
                this._ConsolidatedPosting = value;
            }
        }
        #endregion
		#region SupportBreakQty
		public abstract class supportBreakQty : PX.Data.IBqlField { }
		protected bool? _SupportBreakQty;
        [Feature(false, typeof(FeaturesSet.financialModule), DisplayName = "Volume Pricing")]
		public virtual bool? SupportBreakQty
		{
			get
			{
				return this._SupportBreakQty;
			}
			set
			{
				this._SupportBreakQty = value;
			}
		}
		#endregion		
	}
}
