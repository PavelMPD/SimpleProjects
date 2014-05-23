using System.Collections;
using PX.Common;
using PX.Objects.AP;
using PX.Objects.GL;
using PX.SM;
using PX.TM;
using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CA;
using PX.Objects.CM;
using System.Collections.Generic;
using PX.Objects.AR;
using PX.Objects.PM;

namespace PX.Objects.EP
{
	public class PXSelectCompanyTreeEmployee : PXSelectCompanyTreeUsers
	{
		public PXSelectCompanyTreeEmployee(PXGraph graph)
			: base(graph)
		{
		}
		public PXSelectCompanyTreeEmployee(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
		}

		protected override bool OnInitItem(EPCompanyTreeUsers item)
		{
			if (item.UserID == null)
			{
				item.Icon = PX.Web.UI.Sprite.Main.GetFullUrl(PX.Web.UI.Sprite.Main.Roles);
				return true;
			}
			EPEmployee emp =
				PXSelect<EPEmployee,
				Where<EPEmployee.userID, Equal<EPEmployee.userID>>>.Select(this.View.Graph, item.UserID);
			if (emp != null)
			{
				item.Description = emp.AcctName;
				item.Icon = PX.Web.UI.Sprite.Main.GetFullUrl(PX.Web.UI.Sprite.Main.Users);
			}
			return false;
		}
	}

	[Serializable]
	public partial class EPEmployeeCompanyTreeMember : EPCompanyTreeMember
	{
		#region WorkGroupID
		public new abstract class workGroupID : PX.Data.IBqlField
		{
		}
		[PXDBInt(IsKey = true)]
		[PXParent(typeof(Select<EPEmployee,
		 Where<EPEmployee.userID, Equal<Current<EPEmployeeCompanyTreeMember.userID>>>>))]
		[PXSelector(typeof(EPCompanyTree.workGroupID), SubstituteKey = typeof(EPCompanyTree.description))]
		[PXUIField(DisplayName = "Workgroup ID")]
		public override int? WorkGroupID
		{
			get
			{
				return this._WorkGroupID;
			}
			set
			{
				this._WorkGroupID = value;
			}
		}
		#endregion

		#region UserID
		public new abstract class userID : IBqlField { }
		[PXDBGuid(IsKey = true)]
		[PXDefault(typeof(EPEmployee.userID))]
		[PXUIField(DisplayName = "User", Visible = false)]
		public override Guid? UserID
		{
			get
			{
				return this._UserID;
			}
			set
			{
				this._UserID = value;
			}
		}
		#endregion
	}

	public class EmployeeMaint : PXGraph<EmployeeMaint>
	{
		#region Selects Declartion

		public PXSelectJoin<EPEmployee, LeftJoin<GL.Branch, On<GL.Branch.bAccountID, Equal<EPEmployee.parentBAccountID>>>, Where<EPEmployee.parentBAccountID, IsNull, Or<MatchWithBranch<GL.Branch.branchID>>>> Employee;
		public PXSelect<BAccount2> BAccountParent;
		public PXSelect<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPEmployee.bAccountID>>>> CurrentEmployee;
		public PXSetup<EPEmployeeClass, Where<EPEmployeeClass.vendorClassID, Equal<Optional<EPEmployee.vendorClassID>>>> EmployeeClass;

		public PXSelect<Location, Where<Location.bAccountID, Equal<Current<EPEmployee.bAccountID>>,
			And<Location.locationID, Equal<Current<EPEmployee.defLocationID>>>>> DefLocation;

		public PXSelect<Address, Where<Address.bAccountID, Equal<Optional<EPEmployee.parentBAccountID>>,
			And<Address.addressID, Equal<Optional<EPEmployee.defAddressID>>>>> Address;
		public SelectContactEmailSync<Where<Contact.bAccountID, Equal<Optional<EPEmployee.parentBAccountID>>,
			And<Contact.contactID, Equal<Optional<EPEmployee.defContactID>>>>> Contact;
		public PXSelectJoin<VendorPaymentMethodDetail, 
                    InnerJoin<PaymentMethodDetail, On<PaymentMethodDetail.paymentMethodID, Equal<VendorPaymentMethodDetail.paymentMethodID>,
							    And<PaymentMethodDetail.detailID, Equal<VendorPaymentMethodDetail.detailID>,
                                    And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
                                                Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>>,
                    Where<VendorPaymentMethodDetail.bAccountID, Equal<Current<Location.bAccountID>>,
			            And<VendorPaymentMethodDetail.locationID, Equal<Current<Location.locationID>>,
    			            And<VendorPaymentMethodDetail.paymentMethodID, Equal<Current<Location.vPaymentMethodID>>>>>> PaymentDetails;

		public PXSelect<PaymentMethodDetail, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<Location.vPaymentMethodID>>,
                                And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForVendor>,
                                                Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>> PaymentTypeDetails;
		public PXSelect<BAccount2, Where<BAccount2.acctCD, Equal<Required<BAccount2.acctCD>>>> BAccount;
		public PXSelectJoin<ContactNotification,
			InnerJoin<NotificationSetup, On<NotificationSetup.setupID, Equal<ContactNotification.setupID>>>,
			Where<ContactNotification.contactID, Equal<Optional<EPEmployee.defContactID>>>> NWatchers;

		public PXSelect<EPEmployeeCompanyTreeMember,
			Where<EPEmployeeCompanyTreeMember.userID, Equal<Current<EPEmployee.userID>>>> CompanyTree;

		public PXSelect<EPEmployeeRate, 
			Where<EPEmployeeRate.employeeID, Equal<Current<EPEmployee.bAccountID>>>,
			OrderBy<Asc<EPEmployeeRate.effectiveDate>>> EmployeeRates;

		public PXSelect<EPEmployeeRateByProject,
			Where<EPEmployeeRateByProject.rateID, Equal<Current<EPEmployeeRate.rateID>>>> 
			EmployeeRatesByProject;

		public PXSelectJoin<EPEmployeeClassLaborMatrix
				, LeftJoin<IN.InventoryItem, On<IN.InventoryItem.inventoryID, Equal<EPEmployeeClassLaborMatrix.labourItemID>>
					, LeftJoin<EPEarningType, On<EPEarningType.typeCD, Equal<EPEmployeeClassLaborMatrix.earningType>>>
					>
					, Where<EPEmployeeClassLaborMatrix.employeeID, Equal<Current<EPEmployee.bAccountID>>>>
	
			LaborMatrix;

		public CMSetupSelect cmsetup;
		public PXSetup<GL.Branch, Where<GL.Branch.bAccountID, Equal<Optional<EPEmployee.parentBAccountID>>>> company;
		public PXSelect<Users> User; // for correct redirect to UserMaint (#34819). See also appropriate CacheAttached.

		public EmployeeMaint()
		{
			PXUIFieldAttribute.SetDisplayName<ContactNotification.classID>(this.NWatchers.Cache, Messages.CustomerVendorClass);
		}

		public override void Clear()
		{
			base.Clear();
			isPaymentMergedFlag = false;
		}

		private bool isPaymentMergedFlag = false;

		#endregion

		#region Buttons Definition

		public PXSave<EPEmployee> Save;
		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected virtual System.Collections.IEnumerable Cancel(PXAdapter a)
		{
			foreach (PXResult<EPEmployee> res in (new PXCancel<EPEmployee>(this, "Cancel")).Press(a))
			{
				EPEmployee e = res;
				if (Employee.Cache.GetStatus(e) == PXEntryStatus.Inserted)
				{
					BAccount e1 = BAccount.Select(e.AcctCD);
					if (e1 != null)
					{
						Employee.Cache.RaiseExceptionHandling<EPEmployee.acctCD>(e, e.AcctCD, new PXSetPropertyException(Messages.BAccountExists));
					}
				}
				yield return e;
			}
		}
		public PXAction<EPEmployee> cancel;
		public PXInsert<EPEmployee> Insert;
		public PXCopyPasteAction<EPEmployee> Edit; 
		public PXDelete<EPEmployee> Delete;
		public PXFirst<EPEmployee> First;
		public PXPrevious<EPEmployee> Prev;
		public PXNext<EPEmployee> Next;
		public PXLast<EPEmployee> Last;

		#endregion

		#region EPEmployee Events

		[PXDBGuid()]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.Invisible)]
		protected virtual void BAccount2_OwnerID_CacheAttached(PXCache sender)
		{
			
		}

		[PXSelector(typeof(Users.pKID), SubstituteKey = typeof(Users.username), DescriptionField = typeof(Users.fullName), CacheGlobal = true)]
		[PXUIField(DisplayName = "Employee Login", Visibility = PXUIVisibility.Visible)]
		[PXGuid]
		[PXDBScalar(typeof(Search2<Contact.userID, InnerJoin<BAccount, On<Contact.contactID, Equal<BAccount.defContactID>, And<Contact.bAccountID, Equal<BAccount.parentBAccountID>>>>,
			Where<BAccount.bAccountID, Equal<EPEmployee.bAccountID>>>))]
		protected virtual void EPEmployee_UserID_CacheAttached(PXCache cache)
		{
			
		}

		protected virtual void EPEmployee_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			EPEmployee row = (EPEmployee) e.Row;
			PXUIFieldAttribute.SetVisible<EPEmployee.curyID>(cache, null, (bool) cmsetup.Current.MCActivated);
			PXUIFieldAttribute.SetVisible<EPEmployee.curyRateTypeID>(cache, null, (bool) cmsetup.Current.MCActivated);
			PXUIFieldAttribute.SetVisible<EPEmployee.allowOverrideCury>(cache, null, (bool) cmsetup.Current.MCActivated);
			PXUIFieldAttribute.SetVisible<EPEmployee.allowOverrideRate>(cache, null, (bool) cmsetup.Current.MCActivated);
			PXUIFieldAttribute.SetEnabled<EPEmployee.terminationDate>(cache, null, row != null && (row.Terminated ?? false));
			PXUIFieldAttribute.SetRequired<EPEmployeeClass.termsID>(cache, true);
			PXUIFieldAttribute.SetDisplayName<Contact.displayName>(Contact.Cache, Messages.EmployeeContact);

			if (e.Row != null && cache.GetStatus(e.Row) != PXEntryStatus.Inserted)
			{
				this.FillPaymentDetails();
			}

			this.CompanyTree.Cache.AllowInsert =
				this.CompanyTree.Cache.AllowUpdate =
				this.CompanyTree.Cache.AllowDelete = row != null && row.UserID != null;
		}

		protected virtual void EPEmployee_DefLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{

		}

		protected virtual void EPEmployee_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			EPEmployee row = (EPEmployee)e.Row;

			using (new ReadOnlyScope(Address.Cache, Contact.Cache, DefLocation.Cache))
			{
				Address addr = new Address();
				addr.BAccountID = Employee.Current.ParentBAccountID;
				addr = (Address)Address.Cache.Insert(addr);

				Contact cont = new Contact();
				cont.BAccountID = Employee.Current.ParentBAccountID;
				cont = (Contact)Contact.Cache.Insert(cont);

				cont.Phone1Type = "H1";
				cont.Phone2Type = "C";
				cont.Phone3Type = "B1";
				cont.FaxType = "HF";
				cont.DefAddressID = addr.AddressID;

				Employee.Current.DefContactID = cont.ContactID;
				Employee.Current.DefAddressID = addr.AddressID;
				Employee.Current.AcctName = cont.DisplayName;

                foreach (PXResult<Location, GL.Branch> parent in PXSelectJoin<Location,
                    InnerJoin<GL.Branch, On<GL.Branch.bAccountID, Equal<Location.bAccountID>>,
                    InnerJoin<BAccount2, On<BAccount2.bAccountID, Equal<Location.bAccountID>, And<BAccount2.defLocationID, Equal<Location.locationID>>>>>,
                    Where<BAccount2.bAccountID, Equal<Current<EPEmployee.parentBAccountID>>>>.Select(this))
                {

                    Location defLoc = new Location();

                    defLoc.BAccountID = ((EPEmployee)e.Row).BAccountID;
                    defLoc.LocType = LocTypeList.EmployeeLoc;
                    object newValue = PXMessages.LocalizeNoPrefix(CR.Messages.DefaultLocationCD);
                    DefLocation.Cache.RaiseFieldUpdating<Location.locationCD>(defLoc, ref newValue);
                    defLoc.LocationCD = (string)newValue;
                    defLoc.Descr = PXMessages.LocalizeNoPrefix(CR.Messages.DefaultLocationDescription);
                    defLoc.VTaxZoneID = ((Location)parent).VTaxZoneID;
                    defLoc.VBranchID = ((GL.Branch)parent).BranchID;


                    defLoc = DefLocation.Insert(defLoc);
                        
                    defLoc.VAPAccountLocationID = defLoc.LocationID;
                    defLoc.VPaymentInfoLocationID = defLoc.LocationID;
                    defLoc.CARAccountLocationID = defLoc.LocationID;

                    defLoc.VDefAddressID = addr.AddressID;
                    defLoc.VDefContactID = cont.ContactID;
                    defLoc.DefAddressID = addr.AddressID;
                    defLoc.DefContactID = cont.ContactID;
                    defLoc.VRemitAddressID = addr.AddressID;
                    defLoc.VRemitContactID = cont.ContactID;

                    cache.SetValue<EPEmployee.defLocationID>(e.Row, defLoc.LocationID);
                }

				this.FillPaymentDetails();
			}
		}

		protected virtual void EPEmployee_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<EPEmployee.parentBAccountID>(e.Row, e.OldRow))
			{
				bool found = false;
				foreach (Address addr in Address.Cache.Inserted)
				{
					addr.BAccountID = ((EPEmployee)e.Row).ParentBAccountID;
					addr.CountryID = company.Current.CountryID;
					found = true;
				}

				if (!found)
				{
					Address addr = (Address)Address.View.SelectSingleBound(new object[] { e.OldRow }) ?? new Address();

					addr.BAccountID = ((EPEmployee)e.Row).ParentBAccountID;
					addr.CountryID = company.Current.CountryID;
					Address.Cache.Update(addr);

				}

				found = false;
				foreach (Contact cont in Contact.Cache.Inserted)
				{
					cont.BAccountID = ((EPEmployee)e.Row).ParentBAccountID;
					cont.DefAddressID = null;
					foreach (Address record in Address.Cache.Inserted)
					{
						cont.DefAddressID = record.AddressID;
					} 
					found = true;
				}

				if (!found)
				{
					Contact cont = (Contact)Contact.View.SelectSingleBound(new object[] { e.OldRow }) ?? new Contact();

					cont.BAccountID = ((EPEmployee)e.Row).ParentBAccountID;
					cont.DefAddressID = null;
					foreach (Address record in Address.Cache.Inserted)
					{
						cont.DefAddressID = record.AddressID;
					} 
					Contact.Cache.Update(cont);
				}

				found = false;
				foreach (Location loc in DefLocation.Cache.Inserted)
				{
					loc.VBranchID = company.Current.BranchID;
					foreach (Address record in Address.Cache.Inserted)
					{
						loc.DefAddressID = record.AddressID;
					}

					foreach (Contact record in Contact.Cache.Inserted)
					{
						loc.DefContactID = record.ContactID;
					}
					found = true;
				}

				if (!found)
				{
					Location loc = (Location)DefLocation.View.SelectSingleBound(new object[] { e.Row });

					loc.VBranchID = company.Current.BranchID;
					foreach (Address record in Address.Cache.Inserted)
					{
						loc.DefAddressID = record.AddressID;
						loc.VRemitAddressID = record.AddressID;
					}

					foreach (Contact record in Contact.Cache.Inserted)
					{
						loc.DefContactID = record.ContactID;
						loc.VRemitContactID = record.ContactID;
					}
					DefLocation.Cache.Update(loc);
				}
			}
		}

		protected virtual void EPEmployee_VendorClassID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EmployeeClass.RaiseFieldUpdated(sender, e.Row);

			EPEmployee row = (EPEmployee)e.Row;
			if (row.VendorClassID != null)
			{
				Location defLoc = DefLocation.Current ?? DefLocation.Select();

				sender.SetDefaultExt<EPEmployee.curyID>(e.Row);
				sender.SetDefaultExt<EPEmployee.curyRateTypeID>(e.Row);
				sender.SetDefaultExt<EPEmployee.allowOverrideCury>(e.Row);
				sender.SetDefaultExt<EPEmployee.allowOverrideRate>(e.Row);
				sender.SetDefaultExt<EPEmployee.calendarID>(e.Row);
				sender.SetDefaultExt<EPEmployee.termsID>(e.Row);

				sender.SetDefaultExt<EPEmployee.salesAcctID>(e.Row);
				sender.SetDefaultExt<EPEmployee.salesSubID>(e.Row);
				sender.SetDefaultExt<EPEmployee.expenseAcctID>(e.Row);
				sender.SetDefaultExt<EPEmployee.expenseSubID>(e.Row);
				sender.SetDefaultExt<EPEmployee.prepaymentAcctID>(e.Row);
				sender.SetDefaultExt<EPEmployee.prepaymentSubID>(e.Row);
				sender.SetDefaultExt<EPEmployee.discTakenAcctID>(e.Row);
				sender.SetDefaultExt<EPEmployee.discTakenSubID>(e.Row);
				sender.SetDefaultExt<EPEmployee.hoursValidation>(e.Row);

				DefLocation.Cache.SetDefaultExt<Location.vAPAccountID>(defLoc);
				DefLocation.Cache.SetDefaultExt<Location.vAPSubID>(defLoc);
				DefLocation.Cache.SetDefaultExt<Location.vTaxZoneID>(defLoc);
				DefLocation.Cache.SetDefaultExt<Location.vCashAccountID>(defLoc);
				DefLocation.Cache.SetDefaultExt<Location.vPaymentMethodID>(defLoc);
			}
		}

		protected virtual void EPEmployee_UserID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue != null)
			{
				EPEmployee row = (EPEmployee)e.Row;
				EPEmployee ep = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(this, e.NewValue);
				if (ep != null && ep.AcctCD != row.AcctCD)
				{
					Users us = PXSelect<Users, Where<Users.pKID, Equal<Required<Users.pKID>>>>.Select(this, e.NewValue);

					Employee.Cache.RaiseExceptionHandling<EPEmployee.userID>(e.Row, us.Username, new PXSetPropertyException(Messages.EmployeeLoginExists, ep.AcctCD, ep.AcctName));

					e.NewValue = null;
				}
				else
				{
					this.CompanyTree.Cache.Clear();
				}
			}
		}

		protected virtual void EPEmployee_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			EPEmployee doc = (EPEmployee)e.Row;

			if (string.IsNullOrEmpty(doc.TermsID) == false)
			{
				Terms terms = (Terms)PXSelectorAttribute.Select<EPEmployee.termsID>(Employee.Cache, doc);

				if (terms != null && terms.DiscPercent.HasValue && terms.DiscPercent.Value != decimal.Zero)
				{
					if (sender.RaiseExceptionHandling<EPEmployee.termsID>(e.Row, doc.TermsID, new PXSetPropertyException(Messages.EmployeeTermsCannotHaveCashDiscounts, typeof(EPEmployee.termsID).Name)))
					{
						throw new PXRowPersistingException(typeof(EPEmployee.termsID).Name, doc.TermsID, Messages.EmployeeTermsCannotHaveCashDiscounts, typeof(EPEmployee.termsID).Name);
					}
				}

				if (terms != null && terms.InstallmentType == CS.TermsInstallmentType.Multiple)
				{
					if (sender.RaiseExceptionHandling<EPEmployee.termsID>(e.Row, doc.TermsID, new PXSetPropertyException(Messages.EmployeeTermsCannotHaveMultiplyInstallments, typeof(EPEmployee.termsID).Name)))
					{
						throw new PXRowPersistingException(typeof(EPEmployee.termsID).Name, doc.TermsID, Messages.EmployeeTermsCannotHaveMultiplyInstallments, typeof(EPEmployee.termsID).Name);
					}
				}
			}
		}

		protected virtual void EPEmployee_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			EPEmployee employee = e.Row as EPEmployee;
			if (!string.IsNullOrEmpty(employee.AcctCD) && employee.Status != EPEmployee.status.Inactive)
			{
				e.Cancel = true;
				throw new PXException(EP.Messages.DisableEmployeeBeforeDeleting);
			}
		}

		protected virtual void EPEmployee_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			EPEmployee employee = e.Row as EPEmployee;

			Users user = PXSelect<Users, Where<Users.pKID, Equal<Required<EPEmployee.userID>>>>.Select(this, employee.UserID);
			if (user != null)
			{
				user.LoginTypeID = null;
				user.IsApproved = false;
				User.Update(user);
			}
		}

		#endregion

		#region Location Events

		protected virtual void Location_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Employee.Cache.GetValue<EPEmployee.bAccountID>(Employee.Current);
			e.Cancel = true;
		}

		protected virtual void Location_LocType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Location parent = PXSelect<Location, Where<Location.bAccountID, Equal<Current<EPEmployee.parentBAccountID>>, And<Location.locationCD, Equal<Current<Location.locationCD>>>>>.SelectMultiBound(this, new object[] { e.Row });

			if (parent != null)
			{
				e.NewValue = LocTypeList.EmployeeLoc;
				e.Cancel = true;
			}
		}

		protected virtual void Location_CBranchID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void Location_CARAccountLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = sender.GetValue<Location.locationID>(e.Row);
		}

		protected virtual void Location_VAPAccountLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = sender.GetValue<Location.locationID>(e.Row);
		}

		protected virtual void Location_VPaymentInfoLocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = sender.GetValue<Location.locationID>(e.Row);
		}

		protected virtual void Location_VDefAddressID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Employee.Cache.GetValue<EPEmployee.defAddressID>(Employee.Current);
			e.Cancel = true;
		}

		protected virtual void Location_VDefContactID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Employee.Cache.GetValue<EPEmployee.defContactID>(Employee.Current);
			e.Cancel = true;
		}

		protected virtual void Location_VRemitAddressID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Employee.Cache.GetValue<EPEmployee.defAddressID>(Employee.Current);
			e.Cancel = true;
		}

		protected virtual void Location_VRemitContactID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = Employee.Cache.GetValue<EPEmployee.defContactID>(Employee.Current);
			e.Cancel = true;
		}

		protected virtual void Location_VAPAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = EmployeeClass.Cache.GetValue<EPEmployeeClass.aPAcctID>(EmployeeClass.Current);
			e.Cancel = (EmployeeClass.Current != null);
		}

		protected virtual void Location_VAPSubID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = EmployeeClass.Cache.GetValue<EPEmployeeClass.aPSubID>(EmployeeClass.Current);
			e.Cancel = (EmployeeClass.Current != null);
		}

		protected virtual void Location_VTaxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = EmployeeClass.Cache.GetValue<EPEmployeeClass.taxZoneID>(EmployeeClass.Current);
			e.Cancel = (EmployeeClass.Current != null);
		}

		protected virtual void Location_VCashAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			Location row = (Location)e.Row;
			if (row != null )
			{
				EPEmployeeClass epClass = this.EmployeeClass.Current;
				if (epClass!= null && epClass.CashAcctID.HasValue 
					&& row.VPaymentMethodID == epClass.PaymentMethodID)
				{
					e.NewValue = epClass.CashAcctID;
					e.Cancel = true;
				}
				else
				{
					e.NewValue = null;
					e.Cancel = true;
				}
			}			
		}

		protected virtual void Location_VPaymentMethodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = EmployeeClass.Cache.GetValue<EPEmployeeClass.paymentMethodID>(EmployeeClass.Current);
			e.Cancel = (EmployeeClass.Current != null);
		}

		protected virtual void Location_VPaymentMethodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Location row = (Location)e.Row;
			this.isPaymentMergedFlag = false;
			this.FillPaymentDetails(row);
			cache.SetDefaultExt<Location.vCashAccountID>(e.Row);
			this.PaymentDetails.View.RequestRefresh();
		}

		protected virtual void Location_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Location row = (Location)e.Row;
			if (row != null) 
			{
				FillPaymentDetails(row);				
				PXUIFieldAttribute.SetEnabled<Location.vCashAccountID>(sender, e.Row, (String.IsNullOrEmpty(row.VPaymentMethodID) == false));
			}

		}

		object _KeyToAbort = null;
		protected virtual void Location_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				if (e.TranStatus == PXTranStatus.Open)
				{
					if ((int?)sender.GetValue<Location.vAPAccountLocationID>(e.Row) < 0)
					{
						_KeyToAbort = sender.GetValue<Location.locationID>(e.Row);

						PXDatabase.Update<Location>(
							new PXDataFieldAssign("VAPAccountLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<Location.vAPAccountLocationID>(e.Row, _KeyToAbort);
					}

					if ((int?)sender.GetValue<Location.vPaymentInfoLocationID>(e.Row) < 0)
					{
						_KeyToAbort = sender.GetValue<Location.locationID>(e.Row);

						PXDatabase.Update<Location>(
							new PXDataFieldAssign("VPaymentInfoLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<Location.vPaymentInfoLocationID>(e.Row, _KeyToAbort);
					}

					if ((int?)sender.GetValue<Location.cARAccountLocationID>(e.Row) < 0)
					{
						_KeyToAbort = sender.GetValue<Location.locationID>(e.Row);

						PXDatabase.Update<Location>(
							new PXDataFieldAssign("CARAccountLocationID", _KeyToAbort),
							new PXDataFieldRestrict("LocationID", _KeyToAbort),
							PXDataFieldRestrict.OperationSwitchAllowed);

						sender.SetValue<Location.cARAccountLocationID>(e.Row, _KeyToAbort);
					}
				}
				else
				{
					if (e.TranStatus == PXTranStatus.Aborted)
					{
						if (object.Equals(_KeyToAbort, sender.GetValue<Location.vAPAccountLocationID>(e.Row)))
						{
							object KeyAborted = sender.GetValue<Location.locationID>(e.Row);
							sender.SetValue<Location.vAPAccountLocationID>(e.Row, KeyAborted);
						}

						if (object.Equals(_KeyToAbort, sender.GetValue<Location.vPaymentInfoLocationID>(e.Row)))
						{
							object KeyAborted = sender.GetValue<Location.locationID>(e.Row);
							sender.SetValue<Location.vPaymentInfoLocationID>(e.Row, KeyAborted);
						}

						if (object.Equals(_KeyToAbort, sender.GetValue<Location.cARAccountLocationID>(e.Row)))
						{
							object KeyAborted = sender.GetValue<Location.locationID>(e.Row);
							sender.SetValue<Location.cARAccountLocationID>(e.Row, KeyAborted);
						}
					}
					_KeyToAbort = null;
				}
			}
		}

		#endregion

		#region Other Events
		
		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(Location.bAccountID))]
		[PXUIField(DisplayName = "BAccountID", Visible = false, Enabled = false)]
		[PXParent(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<VendorPaymentMethodDetail.bAccountID>>>>))]
		protected virtual void VendorPaymentMethodDetail_BAccountID_CacheAttached(PXCache sender)
		{

		}

		[PXDBInt(IsKey = true)]
		[PXDBLiteDefault(typeof(Location.locationID))]
		[PXUIField(Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible)]
		[PXParent(typeof(Select<Location, Where<Location.bAccountID, Equal<Current<VendorPaymentMethodDetail.bAccountID>>, And<Location.locationID, Equal<Current<VendorPaymentMethodDetail.locationID>>>>>))]
		protected virtual void VendorPaymentMethodDetail_LocationID_CacheAttached(PXCache sender)
		{

		}

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault(typeof(Search<Location.vPaymentMethodID, Where<Location.bAccountID, Equal<Current<VendorPaymentMethodDetail.bAccountID>>, And<Location.locationID, Equal<Current<VendorPaymentMethodDetail.locationID>>>>>))]
		[PXUIField(DisplayName = "Payment Method", Visible = false)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID>))]
		protected virtual void VendorPaymentMethodDetail_PaymentMethodID_CacheAttached(PXCache sender)
		{

		}
		
        [PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "ID", Visible = false, Enabled = false)]
		[PXSelector(typeof(Search<PaymentMethodDetail.detailID, Where<PaymentMethodDetail.paymentMethodID, Equal<Current<VendorPaymentMethodDetail.paymentMethodID>>,
                        And<Where<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForCashAccount>,
                        Or<PaymentMethodDetail.useFor, Equal<PaymentMethodDetailUsage.useForAll>>>>>>), DescriptionField = typeof(PaymentMethodDetail.descr))]
		protected virtual void VendorPaymentMethodDetail_DetailID_CacheAttached(PXCache sender)
		{

		}

		protected virtual void EPEmployeeCompanyTreeMember_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			EPEmployeeCompanyTreeMember row = e.Row as EPEmployeeCompanyTreeMember;
			EPEmployeeCompanyTreeMember oldRow = e.OldRow as EPEmployeeCompanyTreeMember;
			if (row != null && oldRow != null && row.IsOwner == true && oldRow.IsOwner != true)
			{
				foreach (EPEmployeeCompanyTreeMember member in
				PXSelect<EPEmployeeCompanyTreeMember,
					Where<EPEmployeeCompanyTreeMember.workGroupID, Equal<Required<EPEmployeeCompanyTreeMember.workGroupID>>,
						And<EPEmployeeCompanyTreeMember.isOwner, Equal<boolTrue>>>>.Select(this, row.WorkGroupID))
				{
					if (member.UserID != row.UserID)
					{
						EPEmployeeCompanyTreeMember upd = PXCache<EPEmployeeCompanyTreeMember>.CreateCopy(member);
						upd.IsOwner = false;
						this.CompanyTree.Update(upd);
					}
				}
			}
		}

		#endregion

		#region Contact Events

        [PXDBIdentity(IsKey = true)]
        [PXUIField(DisplayName = "Contact ID", Visibility = PXUIVisibility.Invisible)]
        [PXParent(typeof(Select<BAccount, Where<BAccount.defContactID, Equal<Current<Contact.contactID>>>>))]
		protected virtual void Contact_ContactID_CacheAttached(PXCache sender)
		{

		}

		[PXDBGuid]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.Invisible)]
		protected virtual void Contact_OwnerID_CacheAttached(PXCache sender)
		{

		}

		[PXDBString(2, IsFixed = true)]
		[PXDefault(ContactTypesAttribute.Employee)]
		protected virtual void Contact_ContactType_CacheAttached(PXCache sender)
		{

		}

        [PXDBString(100, IsUnicode = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Last Name", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void Contact_LastName_CacheAttached(PXCache sender)
        {

        }

		protected virtual void Contact_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void Contact_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.Employee.Current != null)
			{
				e.NewValue = this.Employee.Current.ParentBAccountID;
				e.Cancel = true;
			}
		}

		protected virtual void Contact_Salutation_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = ((EPPosition)PXSelectorAttribute.Select<EPEmployee.positionID>(Employee.Cache, Employee.Current)).With(_=>_.Description);
		}

		protected virtual void EPEmployee_PositionID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Contact.Cache.SetDefaultExt<Contact.salutation>(Contact.Current);
		}

		protected virtual void Contact_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Contact cont = e.Row as Contact;
			if (cont == null) return;

			Employee.Current.DefContactID = cont.ContactID;
		}

		protected virtual void Contact_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Contact cont = (Contact)e.Row;
			if (cont == null) return;

			PXUIFieldAttribute.SetWarning<Contact.eMail>(Contact.Cache, Contact.Cache.Current, string.IsNullOrEmpty(cont.EMail) ? Messages.EmailIsEmpty : null);
		}

		protected virtual void Contact_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			Contact cont = (Contact)e.Row;
			cont.FullName = cont.DisplayName;
			Employee.SetValueExt<EPEmployee.acctName>(Employee.Current, cont.DisplayName);
		}

		protected virtual void Contact_DefAddressID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		#endregion

		#region Address Envents

		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.bAccountID, Equal<Current<EPEmployee.parentBAccountID>>>>))]
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Country")]
		[PXSelector(typeof(Search<Country.countryID>), DescriptionField = typeof(Country.description), CacheGlobal = true)]
		protected virtual void Address_CountryID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void Address_BAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void Address_BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.Employee.Current != null)
			{
				e.NewValue = this.Employee.Current.ParentBAccountID;
				e.Cancel = true;
			}
		}

		protected virtual void Address_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Address addr = e.Row as Address;
			if (addr != null)
			{
				Employee.Current.DefAddressID = addr.AddressID;
			}
		}

        protected virtual void Address_CountryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            Address addr = (Address)e.Row;
            if ((string)e.OldValue != addr.CountryID)
            {
                addr.State = null;
            }
        }


		#endregion

		#region Location Events
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Address.addressID))]
		protected virtual void Location_DefAddressID_CacheAttached(PXCache sender)
		{ 
		}

		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		protected virtual void Location_DefContactID_CacheAttached(PXCache sender)
		{ 
		}
		#endregion

		// for correct redirect to UserMaint (#34819).
		[PXInt]
		[PXUIField(DisplayName = "Contact")]
		[PXSelector(typeof(Search2<Contact.contactID, LeftJoin<Users, On<Contact.userID, Equal<Users.pKID>>>,
			Where<Current<Users.guest>, Equal<True>, And<Contact.contactType, Equal<ContactTypesAttribute.person>,
			Or<Current<Users.guest>, NotEqual<True>, And<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>>>),
			typeof(Contact.displayName),
			typeof(Contact.salutation),
			typeof(Contact.fullName),
			typeof(Contact.eMail),
			typeof(Users.username),
			SubstituteKey = typeof(Contact.displayName))]
		[PXRestrictor(typeof(Where<Contact.eMail, IsNotNull, Or<Current<Users.source>, Equal<PXUsersSourceListAttribute.activeDirectory>>>), PX.Objects.CR.Messages.ContactWithoutEmail, typeof(Contact.displayName))]
		[PXDBScalar(typeof(Search<Contact.contactID, Where<Contact.userID, Equal<Users.pKID>>>))]
		protected virtual void Users_ContactID_CacheAttached(PXCache sender)
		{
		}


		#region EPEmployeeRate

		protected virtual void EPEmployeeRate_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var rate = (EPEmployeeRate) e.Row;
			if (rate == null) 
				return;

			PMTran lastTran = PXSelect<PMTran, Where<PMTran.resourceID, Equal<Current<EPEmployee.bAccountID>>>, OrderBy<Desc<PMTran.date>>>.SelectSingleBound(this, new object[] { Employee.Current }, null);
			if (lastTran != null && lastTran.Date >= rate.EffectiveDate && cache.GetStatus(rate) != PXEntryStatus.Inserted && cache.GetStatus(rate) != PXEntryStatus.Updated)
			{
				PXUIFieldAttribute.SetEnabled(cache, rate, false);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, rate, true);
				bool isHourly = rate.RateType == RateTypesAttribute.Hourly;
				PXUIFieldAttribute.SetEnabled<EPEmployeeRate.annualSalary>(cache, rate, !isHourly);
				PXUIFieldAttribute.SetEnabled<EPEmployeeRate.hourlyRate>(cache, rate, isHourly);
				if (isHourly)
					rate.AnnualSalary = null;
			}
		}

		protected virtual void EPEmployeeRate_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			var rate = (EPEmployeeRate)e.Row;
			if (rate == null) return;

			PMTran lastTran = PXSelect<PMTran, Where<PMTran.resourceID, Equal<Current<EPEmployee.bAccountID>>>, OrderBy<Desc<PMTran.date>>>.SelectSingleBound(this, new object[] { Employee.Current }, null);
			if (lastTran != null && lastTran.Date >= rate.EffectiveDate && cache.GetStatus(rate) != PXEntryStatus.Inserted && cache.GetStatus(rate) != PXEntryStatus.Updated && cache.GetStatus(rate) != PXEntryStatus.InsertedDeleted)
			{
				throw new PXException(Messages.CostInUse);
			}
		}

		protected virtual void EPEmployeeRate_EffectiveDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			EPEmployeeRate row = (EPEmployeeRate)e.Row;
			if (row == null) return;
			DateTime? newVal = (DateTime?)e.NewValue;
			DateTime? nextEffectiveDate = GetNextEffectiveDate(row);
			if (newVal == null || newVal < nextEffectiveDate)
				e.NewValue = nextEffectiveDate;
		}

		protected virtual void EPEmployeeRate_EffectiveDate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			EPEmployeeRate row = (EPEmployeeRate) e.Row;
			if (row == null) return;

			DateTime? newVal = (DateTime?)e.NewValue;
			DateTime? nextEffectiveDate = GetNextEffectiveDate(row);
			if (newVal != null && nextEffectiveDate != null && newVal < nextEffectiveDate)
			{
				e.NewValue = null;
				sender.RaiseExceptionHandling<EPEmployeeRate.effectiveDate>(row, null, new PXSetPropertyException(ErrorMessages.EndDateLessThanStartDate, PXUIFieldAttribute.GetDisplayName<EPEmployeeRate.effectiveDate>(sender), nextEffectiveDate));
			}
		}

		protected virtual void EPEmployeeRate_AnnualSalary_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ChangeHourlyRate((EPEmployeeRate)e.Row);
		}

		protected virtual void EPEmployeeRate_RegularHours_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ChangeHourlyRate((EPEmployeeRate)e.Row);
		}

		protected virtual void ChangeHourlyRate(EPEmployeeRate row)
		{
			const decimal weeksInYear = 52;
			if (row != null && row.RateType != RateTypesAttribute.Hourly && row.AnnualSalary != null && row.RegularHours != null)
				row.HourlyRate = Math.Round((decimal)row.AnnualSalary / weeksInYear / ((decimal)row.RegularHours), 2, MidpointRounding.ToEven);
		}

		public virtual void EPEmployeeRate_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			EPEmployeeRate row = (EPEmployeeRate)e.Row;
			if (row == null)
				return;

			if (row.RateType == RateTypesAttribute.Hourly)
			{
				if(row.HourlyRate == null || (decimal)(row.HourlyRate) <= 0m)
					sender.RaiseExceptionHandling<EPEmployeeRate.hourlyRate>(row, row.HourlyRate, new PXSetPropertyException(Messages.ValueMustBeGreaterThanZero));
				if (row.RegularHours == null || (int)(row.RegularHours) <= 0)
					sender.RaiseExceptionHandling<EPEmployeeRate.regularHours>(row, row.RegularHours, new PXSetPropertyException(Messages.ValueMustBeGreaterThanZero));
			}
			else
			{
				if (row.AnnualSalary == null || (decimal)(row.AnnualSalary) <= 0m)
					sender.RaiseExceptionHandling<EPEmployeeRate.annualSalary>(row, row.AnnualSalary, new PXSetPropertyException(Messages.ValueMustBeGreaterThanZero));
			}
		}


		#endregion

		#region EPEmployeeRateByProject

		protected virtual void EPEmployeeRateByProject_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			var row = e.Row as EPEmployeeRateByProject;
			if (row == null) return;

			if (row.HourlyRate == null)
			{
				row.HourlyRate = GetParentEmployeeRate(row.RateID).
					With(_ => _.HourlyRate);
			}

			if (row.CompensationCode == null)
			{
				row.CompensationCode = GetParentEmployeeRate(row.RateID).
					With(_ => _.CompensationCode);
			}
		}

		#endregion

		#region Utility Functions

		private EPEmployeeRate GetParentEmployeeRate(object rateId)
		{
			return (EPEmployeeRate)PXSelect<EPEmployeeRate,
				Where<EPEmployeeRate.rateID, Equal<Required<EPEmployeeRate.rateID>>>>.
				Select(this, rateId);
		}

		public static Guid? GetCurrentEmployeeID(PXGraph graph)
		{
			return GetCurrentEmployee(graph).With(_ => _.UserID);
		}

		public static EPEmployee GetCurrentEmployee(PXGraph graph)
		{
			PXSelectReadonly<EPEmployee,
				Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.
				Clear(graph);
			var set = PXSelectReadonly<EPEmployee,
				Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>.
				Select(graph);
			return set == null || set.Count == 0 ? null : (EPEmployee)set[0][typeof(EPEmployee)];
		}

		protected virtual void FillPaymentDetails()
		{
			Location account = this.DefLocation.Current ?? DefLocation.Select();
		}

		protected virtual void FillPaymentDetails(Location account)
		{
			if (account != null)
			{
				if (!isPaymentMergedFlag)
				{
					if (!string.IsNullOrEmpty(account.VPaymentMethodID))
					{
						List<PaymentMethodDetail> toAdd = new List<PaymentMethodDetail>();
						foreach (PaymentMethodDetail it in this.PaymentTypeDetails.Select())
						{
							VendorPaymentMethodDetail detail = null;
							foreach (VendorPaymentMethodDetail iPDet in this.PaymentDetails.Select())
							{
								if (iPDet.DetailID == it.DetailID)
								{
									detail = iPDet;
									break;
								}
							}
							if (detail == null)
							{
								toAdd.Add(it);
							}
						}
						using (ReadOnlyScope rs = new ReadOnlyScope(this.PaymentDetails.Cache))
						{
							foreach (PaymentMethodDetail it in toAdd)
							{
								VendorPaymentMethodDetail detail = new VendorPaymentMethodDetail();
								detail.BAccountID = account.BAccountID;
								detail.LocationID = account.LocationID;
								detail.DetailID = it.DetailID;
								this.PaymentDetails.Insert(detail);
							}
							if (toAdd.Count > 0)
							{
								this.PaymentDetails.View.RequestRefresh();
							}
						}
					}
					this.isPaymentMergedFlag = true;
				}
			}
		}

		protected DateTime? GetNextEffectiveDate(EPEmployeeRate currentRow)
		{
			DateTime? ret = null;
			PMTran lastTran =
				PXSelect<PMTran, Where<PMTran.resourceID, Equal<Current<EPEmployee.bAccountID>>>, OrderBy<Desc<PMTran.date>>>
					.SelectSingleBound(this, new object[] {Employee.Current}, null);
			EPEmployeeRate lastRate =
				PXSelect
					<EPEmployeeRate, Where<EPEmployeeRate.employeeID, Equal<Current<EPEmployee.bAccountID>>, And<EPEmployeeRate.rateID, NotEqual<Required<EPEmployeeRate.rateID>>>>,
						OrderBy<Desc<EPEmployeeRate.effectiveDate>>>.SelectSingleBound(this, new object[] { Employee.Current }, currentRow.RateID);
			if (lastRate != null && lastRate.EffectiveDate != null)
			{
				ret = lastRate.EffectiveDate.Value.AddDays(1);
			}
			if (lastTran != null && (ret == null || lastTran.Date > ret) && lastTran.Date != null)
			{
				ret = lastTran.Date.Value.AddDays(1);
			}
			return ret;
		}

		#endregion

		public PXAction<EPEmployee> viewContact;
		[PXUIField(DisplayName = "View Contact", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		public virtual IEnumerable ViewContact(PXAdapter adapter)
		{
			Contact contact = Contact.Current;
			if (contact != null)
			{
				Save.Press();
				ContactMaint graph = CreateInstance<ContactMaint>();
				graph.Contact.Current = graph.Contact.Search<Contact.contactID>(contact.ContactID);
				throw new PXRedirectRequiredException(graph, "View Contact");
			}
			return adapter.Get();
		}

	}
}
