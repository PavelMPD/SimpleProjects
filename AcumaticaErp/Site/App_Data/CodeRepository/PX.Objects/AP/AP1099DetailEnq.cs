using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.AP
{
	[System.SerializableAttribute()]
	public partial class AP1099YearMaster : IBqlTable
	{
		#region FinYear
		public abstract class finYear : PX.Data.IBqlField
		{
		}
		protected String _FinYear;
		[PXDBString(4, IsFixed = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "1099 Year", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<AP1099Year.finYear>))]
		public virtual String FinYear
		{
			get
			{
				return this._FinYear;
			}
			set
			{
				this._FinYear = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.IBqlField
		{
		}
		protected Int32? _VendorID;
		[Vendor(typeof(Search<Vendor.bAccountID, Where<Vendor.vendor1099, Equal<boolTrue>>>), DisplayName = "Vendor", DescriptionField = typeof(Vendor.acctName))]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion	
	}

	public class AP1099DetailEnq : PXGraph<AP1099DetailEnq>
	{
		public PXCancel<AP1099YearMaster> Cancel;
		public PXFilter<AP1099YearMaster> YearVendor_Header;
		[PXFilterable]
		public PXSelectJoinGroupBy<AP1099Box, 
			LeftJoin<AP1099History, On<AP1099History.vendorID, Equal<Current<AP1099YearMaster.vendorID>>,
			And<AP1099History.boxNbr, Equal<AP1099Box.boxNbr>, 
			And<AP1099History.finYear, Equal<Current<AP1099YearMaster.finYear>>>>>>, 
			Where<boolTrue, Equal<boolTrue>>, 
			Aggregate<GroupBy<AP1099Box.boxNbr, Sum<AP1099History.histAmt>>>> YearVendor_Summary;

		public PXAction<AP1099YearMaster> firstVendor;
		[PXUIField(DisplayName = "First", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXFirstButton]
		public virtual IEnumerable FirstVendor(PXAdapter adapter)
		{
			AP1099YearMaster filter = (AP1099YearMaster)YearVendor_Header.Current;
			Vendor next_vendor = (Vendor)PXSelect<Vendor, Where<Vendor.vendor1099, Equal<boolTrue>>, OrderBy<Asc<Vendor.acctCD>>>.Select(this);
			if (next_vendor != null)
			{
				filter.VendorID = next_vendor.BAccountID;
			}
			return adapter.Get();
		}

		public PXAction<AP1099YearMaster> previousVendor;
		[PXUIField(DisplayName = "Prev", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable PreviousVendor(PXAdapter adapter)
		{
			AP1099YearMaster filter = (AP1099YearMaster) YearVendor_Header.Current;
			Vendor vendor = (Vendor) PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Current<AP1099YearMaster.vendorID>>>>.Select(this);
			if (vendor == null)
			{
				vendor = new Vendor();
				vendor.AcctCD = "";
			}
			Vendor next_vendor = (Vendor)PXSelect<Vendor, Where<Vendor.vendor1099, Equal<boolTrue>, And<Vendor.acctCD, Less<Required<Vendor.acctCD>>>>>.Select(this, vendor.AcctCD);
			if (next_vendor != null)
			{
				filter.VendorID = next_vendor.BAccountID;
			}
			return adapter.Get();
		}

		public PXAction<AP1099YearMaster> nextVendor;
		[PXUIField(DisplayName = "Next", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable NextVendor(PXAdapter adapter)
		{
			AP1099YearMaster filter = (AP1099YearMaster)YearVendor_Header.Current;

			Vendor vendor = (Vendor) PXSelect<Vendor, Where<Vendor.bAccountID, Equal<Current<AP1099YearMaster.vendorID>>>>.Select(this);
			if (vendor == null)
			{
				vendor = new Vendor();
				vendor.AcctCD = "";
			}
			Vendor next_vendor = (Vendor) PXSelect<Vendor, Where<Vendor.vendor1099, Equal<boolTrue>, And<Vendor.acctCD, Greater<Required<Vendor.acctCD>>>>>.Select(this, vendor.AcctCD);
			if (next_vendor != null)
			{
				filter.VendorID = next_vendor.BAccountID;
			}
			return adapter.Get();
		}

		public PXAction<AP1099YearMaster> lastVendor;
		[PXUIField(DisplayName = "Last", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLastButton]
		public virtual IEnumerable LastVendor(PXAdapter adapter)
		{
			AP1099YearMaster filter = (AP1099YearMaster)YearVendor_Header.Current;
			Vendor next_vendor = (Vendor)PXSelect<Vendor, Where<Vendor.vendor1099, Equal<boolTrue>>, OrderBy<Desc<Vendor.acctCD>>>.Select(this);
			if (next_vendor != null)
			{
				filter.VendorID = next_vendor.BAccountID;
			}
			return adapter.Get();
		}

	    public PXAction<AP1099YearMaster> year1099SummaryReport;
        [PXUIField(DisplayName = Messages.Year1099SummaryReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable Year1099SummaryReport(PXAdapter adapter)
        {
            AP1099YearMaster filter = YearVendor_Header.Current;
            if (filter != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                Branch cbranch = PXSelect<Branch, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(this);
                parameters["MasterBranchID"] = cbranch != null ? cbranch.BranchCD: null;
                parameters["FinYear"] = filter.FinYear;
                throw new PXReportRequiredException(parameters, "AP654000", Messages.Year1099SummaryReport); 
            }
            return adapter.Get();
        }

        public PXAction<AP1099YearMaster> year1099DetailReport;
        [PXUIField(DisplayName = Messages.Year1099DetailReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual IEnumerable Year1099DetailReport(PXAdapter adapter)
        {
            AP1099YearMaster filter = YearVendor_Header.Current;
            if (filter != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                Branch cbranch = PXSelect<Branch, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(this);
                parameters["MasterBranchID"] = cbranch != null ? cbranch.BranchCD : null;
                parameters["FinYear"] = filter.FinYear;
                throw new PXReportRequiredException(parameters, "AP654500", Messages.Year1099DetailReport);
            }
            return adapter.Get();
        }
        
        public AP1099DetailEnq()
		{
			APSetup setup = APSetup.Current;
			PXUIFieldAttribute.SetEnabled<AP1099Box.boxNbr>(YearVendor_Summary.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<AP1099Box.descr>(YearVendor_Summary.Cache, null, false);
			PXUIFieldAttribute.SetRequired<AP1099YearMaster.vendorID>(YearVendor_Header.Cache, true);
		}
		public PXSetup<APSetup> APSetup;
	}
}
