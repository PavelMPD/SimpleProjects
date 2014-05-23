using System;
using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CR
{
	[DashboardType(PX.TM.OwnedFilter.DASHBOARD_TYPE, GL.TableAndChartDashboardTypeAttribute._AMCHARTS_DASHBOART_TYPE)]
	public class BAccountEnq : PXGraph<BAccountEnq>
	{
		#region BAccountFilter

		[Serializable]
		public partial class BAccountFilter : OwnedFilter
		{
			#region ShowProspect
			public abstract class showProspect : IBqlField { }

			[PXDBBool]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Prospects")]
			public virtual Boolean? ShowProspect { get; set; }
			#endregion

			#region ShowCustomer
			public abstract class showCustomer : IBqlField { }

			[PXDBBool]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Customers")]
			public virtual Boolean? ShowCustomer { get; set; }
			#endregion

			#region ShowVendors
			public abstract class showVendor : IBqlField { }

			[PXDBBool]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Vendors")]
			public virtual Boolean? ShowVendor { get; set; }
			#endregion
		}

		#endregion

		#region Selects

		[PXHidden]
		public PXSelect<BAccount>
			BaseAccounts;

		[PXHidden]
		public PXSelect<BAccountParent>
			BAccount2View;

		[PXViewName(Messages.Selection)]
		public PXFilter<BAccountFilter> 
			Filter;

		[PXViewName(Messages.BusinessAccounts)]
		[PXFilterable]
		[PXViewDetailsButton(typeof(BAccountFilter))]
		[PXViewDetailsButton(typeof(BAccountFilter),
			typeof(Select<BAccountParent,
				Where<BAccountParent.bAccountID, Equal<Current<BAccount.parentBAccountID>>>>))]
		public PXOwnerFilteredSelectReadonly<BAccountFilter,
			Select2<BAccount,
			LeftJoin<Contact, 
				On<Contact.bAccountID, Equal<BAccount.bAccountID>,
					And<Contact.contactID, Equal<BAccount.defContactID>>>,
			LeftJoin<Address, On<Address.addressID, Equal<BAccount.defAddressID>>,
			LeftJoin<BAccountParent, On<BAccountParent.bAccountID, Equal<BAccount.parentBAccountID>>,
			LeftJoin<Location, On<Location.locationID, Equal<BAccount.defLocationID>>, 
			LeftJoin<State, 
					On<State.countryID, Equal<Address.countryID>, 
						And<State.stateID, Equal<Address.state>>>>>>>>,
			Where<Current<BAccountFilter.showProspect>, Equal<True>,
					And<BAccount.type, Equal<BAccountType.prospectType>,
				Or<Current<BAccountFilter.showCustomer>, Equal<True>,
					And2<
						Where<BAccount.type, Equal<BAccountType.customerType>, 
							Or<BAccount.type, Equal<BAccountType.combinedType>>>,
				Or<Current<BAccountFilter.showVendor>, Equal<True>,
					And<
						Where<BAccount.type, Equal<BAccountType.vendorType>, 
							Or<BAccount.type, Equal<BAccountType.combinedType>>>>>>>>>>,
			BAccount.workgroupID, BAccount.ownerID>
			FilteredItems;

		#endregion

		#region Ctors

		public BAccountEnq()
		{
			PXDBAttributeAttribute.Activate(BaseAccounts.Cache);
			PXDBAttributeAttribute.Activate(this.Caches[typeof(BAccountParent)]);
			FilteredItems.NewRecordTarget = typeof(BusinessAccountMaint);

			var bAccountCache = FilteredItems.Cache;
			bAccountCache.DisplayName = Messages.Customer;
			PXUIFieldAttribute.SetDisplayName<BAccount.acctCD>(bAccountCache, Messages.BAccountCD);
			PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(bAccountCache, Messages.BAccountName);

			var parentBAccountCache = Caches[typeof(BAccountParent)];
			parentBAccountCache.DisplayName = Messages.ParentAccount;
			PXUIFieldAttribute.SetDisplayName<BAccountParent.acctCD>(parentBAccountCache, Messages.ParentAccountID);
			PXUIFieldAttribute.SetDisplayName<BAccountParent.acctName>(parentBAccountCache, Messages.ParentAccountName);


			var stateCache = Caches[typeof(State)];
			PXUIFieldAttribute.SetDisplayName<State.name>(stateCache, "State");

		}

		#endregion

		#region Actions

		public PXCancel<BAccountFilter> Cancel;

		#endregion

		#region Event Handlers

		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Type")]
		[BAccountType.List]
		[PXDefault(BAccountType.ProspectType)]
		protected virtual void BAccount_Type_CacheAttached(PXCache sender)
		{
			
		}

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
