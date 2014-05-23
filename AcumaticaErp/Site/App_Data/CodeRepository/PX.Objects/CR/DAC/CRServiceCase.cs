using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.TM;

namespace PX.Objects.CR
{
	[PXPrimaryGraph(typeof(ServiceCaseMaint))]
	[PXCacheName(Messages.ServiceCall)]
	[Serializable]
	public partial class CRServiceCase : IBqlTable
	{
		#region ServiceCaseID
		public abstract class serviceCaseID : IBqlField { }

		[PXDBIdentity]
		[PXNavigateSelector(typeof(CRServiceCase.serviceCaseID))]
		public virtual Int32? ServiceCaseID { get; set; }
		#endregion

		#region ServiceCaseCD
		public abstract class serviceCaseCD : IBqlField { }

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Service Call ID", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(CRSetup.serviceCaseNumberingID), typeof(AccessInfo.businessDate))]
		[PXSelector(typeof(Search3<CRServiceCase.serviceCaseCD, LeftJoin<Customer, On<CRServiceCase.customerID, Equal<Customer.bAccountID>>>, OrderBy<Desc<CRServiceCase.serviceCaseCD>>>),
			typeof(CRServiceCase.serviceCaseCD),
			typeof(CRServiceCase.status),
			typeof(Customer.acctName),
			typeof(CRServiceCase.subject))]
		[PXFieldDescription]
		public virtual String ServiceCaseCD { get; set; }
		#endregion

		#region Subject
		public abstract class subject : IBqlField { }

		[PXDBString(IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Subject", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String Subject { get; set; }
		#endregion

		#region Description
		public abstract class description : IBqlField { }

		[PXDBText(IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual String Description { get; set; }
		#endregion

		#region Hold
		public abstract class hold : IBqlField { }

		[PXDBBool]
		[PXUIField(DisplayName = "Open", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true)]
		public virtual Boolean? Hold { get; set; }
		#endregion

		#region Status
		public abstract class status : IBqlField { }

		[PXDBString(1, IsFixed = true)]
		[PXDefault("N")]
		[PXStringList(new [] { "A", "D", "H", "J", "N", "O", "S", "W", "B", "C", "L", "R", "U" },
			new [] { "Assigned", "Deferred", "On Hold", "Rejected", "New", "Open", "Resolved", "Waiting", "Billed", "Completed", "Released", "Reopen", "Unbilled" })]
		[PXUIField(DisplayName = "Service Call Status")]
		[PXChildUpdatable]
		public virtual String Status { get; set; }
		#endregion

		#region WorkgroupID
		public abstract class workgroupID : IBqlField { }

		[PXDBInt]
		[PXChildUpdatable(UpdateRequest = true)]
		[PXUIField(DisplayName = "Workgroup")]
		[PXCompanyTreeSelector]
		public virtual Int32? WorkgroupID { get; set; }
		#endregion

		#region OwnerID
		public abstract class ownerID : IBqlField { }

		[PXDBGuid]
		[PXOwnerSelector(typeof(CRCase.workgroupID))]
		[PXChildUpdatable(AutoRefresh = true, TextField = "AcctName", ShowHint = false)]
		[PXUIField(DisplayName = "Owner")]
		public virtual Guid? OwnerID { get; set; }
		#endregion

		#region CustomerID
		public abstract class customerID : IBqlField { }

		[Customer]
		[PXDefault]
		public virtual Int32? CustomerID { get; set; }
		#endregion

		#region LocationID
		public abstract class locationID : IBqlField { }

		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<CRServiceCase.customerID>>>), 
			DescriptionField = typeof(Location.descr))]
		[PXDefault(typeof(Search<BAccountR.defLocationID, 
			Where<BAccountR.bAccountID, Equal<Current<CRServiceCase.customerID>>>>))]
		public virtual Int32? LocationID { get; set; }
		#endregion

		#region ContactID
		public abstract class contactID : IBqlField { }

		[PXDBInt()]
		[PXUIField(DisplayName = "Contact", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search2<Contact.contactID,
			InnerJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
			Where<Contact.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>,
				And<Where2<Where<BAccount.type, Equal<BAccountType.customerType>,
				Or<BAccount.type, Equal<BAccountType.prospectType>>>,
					And<Where<BAccount.bAccountID, Equal<Current<CRServiceCase.customerID>>,
				Or<Current<CRServiceCase.customerID>, IsNull>>>>>>>),
			DescriptionField = typeof(Contact.displayName), Filterable = true)]
		[PXDefault(typeof(Search2<BAccount.defContactID,
			InnerJoin<Contact, On<Contact.contactID, Equal<BAccount.defContactID>>>,
			Where<BAccount.bAccountID, Equal<Current<CRServiceCase.customerID>>,
				And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>),
			SearchOnDefault = false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? ContactID { get; set; }
		#endregion

		#region TaxZoneID
		public abstract class taxZoneID : IBqlField { }

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Customer Tax Zone",
			Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TX.TaxZone.taxZoneID),
			DescriptionField = typeof(TX.TaxZone.descr),
			Filterable = true)]
		public virtual String TaxZoneID { get; set; }
		#endregion

		#region CuryID
		public abstract class curyID : IBqlField { }

		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID { get; set; }
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : IBqlField { }

		[PXDBLong]
		[CurrencyInfo(ModuleCode = "CR")]
		public virtual Int64? CuryInfoID { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : IBqlField { }

		[PXNote(typeof(CRServiceCase.serviceCaseCD),
			typeof(CRServiceCase.status),
			typeof(CRServiceCase.description),
			typeof(Customer.acctCD),
			typeof(Customer.acctName),
			typeof(Contact.displayName),
			ForeignRelations = new [] { typeof(CRServiceCase.customerID), typeof(CRServiceCase.contactID) },
			ExtraSearchResultColumns = new[] { typeof(Contact) },
			DescriptionField = typeof(CRServiceCase.serviceCaseCD),
			Selector = typeof(CRServiceCase.serviceCaseCD))]
		public virtual Int64? NoteID { get; set; }
		#endregion

		#region LastLineNbr
		public abstract class lineCntr : IBqlField { }

		[PXDBInt]
		[PXDefault(0)]
		public virtual Int32? LineCntr { get; set; }
		#endregion

		#region ServiceItemID
		public abstract class serviceItemID : IBqlField { }

		[PXDBInt]
		[PXDefault]
		[PXUIField(DisplayName = "Equipment ID")]
		[PXSelector(typeof(Search3<INServiceItem.serviceItemID,
			LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INServiceItem.inventoryID>>,
			LeftJoin<Customer, On<INServiceItem.customerID, Equal<Customer.bAccountID>>>>, 
			OrderBy<Desc<INServiceItem.serviceItemCD>>>))]
		public virtual Int32? ServiceItemID { get; set; }
		#endregion

		#region Location
		public abstract class location : IBqlField { }

		[PXDBString(IsUnicode = true)]
		[PXUIField(DisplayName = "Location", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Location { get; set; }
		#endregion

		#region StartDate
		public abstract class startDate : IBqlField { }

		[PXDBDateAndTime]
		[PXUIField(DisplayName = "Start Time")]
		public virtual DateTime? StartDate { get; set; }
		#endregion

		#region AllDay
		public abstract class allDay : PX.Data.IBqlField { }

		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "All-Day Event")]
		public virtual Boolean? AllDay { get; set; }
		#endregion

		#region EndDate
		public abstract class endDate : IBqlField { }

		[PXDBDateAndTime]
		[PXUIField(DisplayName = "End Time")]
		public virtual DateTime? EndDate { get; set; }
		#endregion

		#region IsReminderOn
		public abstract class isReminderOn : IBqlField { }

		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Reminder")]
		public virtual bool? IsReminderOn { get; set; }
		#endregion

		#region ReminderDate
		public abstract class reminderDate : IBqlField { }

		[PXRemindDate(typeof(isReminderOn), typeof(startDate))]
		[PXUIField(DisplayName = "Remind at")]
		public virtual DateTime? ReminderDate { get; set; }
		#endregion

		#region DefAddressID
		public abstract class destinationAddressID : IBqlField { }

		[PXDBInt]
		[CRServiceCaseDestinationAddress]
		public virtual Int32? DestinationAddressID { get; set; }
		#endregion

		#region BillAddressID
		public abstract class billAddressID : IBqlField { }

		[PXDBInt]
		[CRServiceCaseBillingAddress]
		public virtual Int32? BillAddressID { get; set; }
		#endregion

		#region EstimatedCuryItemAmount
		public abstract class estimatedCuryItemAmount : IBqlField { }

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(CRServiceCase.curyInfoID), typeof(CRServiceCase.estimatedItemAmount))]
		[PXUIField(DisplayName = "Estimated Total", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? EstimatedCuryItemAmount { get; set; }
		#endregion

		#region EstimatedItemAmount
		public abstract class estimatedItemAmount : IBqlField { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? EstimatedItemAmount { get; set; }
		#endregion

		#region ActualCuryItemAmount
		public abstract class actualCuryItemAmount : IBqlField { }

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(CRServiceCase.curyInfoID), typeof(CRServiceCase.actualItemAmount))]
		[PXUIField(DisplayName = "Actual Total", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? ActualCuryItemAmount { get; set; }
		#endregion

		#region ActualItemAmount
		public abstract class actualItemAmount : IBqlField { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ActualItemAmount { get; set; }
		#endregion

		#region EstimatedCuryTaxTotal
		public abstract class estimatedCuryTaxTotal : IBqlField { }

		[PXDBCurrency(typeof(CRServiceCase.curyInfoID), typeof(CRServiceCase.estimatedTaxTotal))]
		[PXUIField(DisplayName = "Tax Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? EstimatedCuryTaxTotal { get; set; }
		#endregion

		#region EstimatedTaxTotal
		public abstract class estimatedTaxTotal : IBqlField { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? EstimatedTaxTotal { get; set; }
		#endregion

		#region ActualCuryTaxTotal
		public abstract class actualCuryTaxTotal : IBqlField { }

		[PXDBCurrency(typeof(CRServiceCase.curyInfoID), typeof(CRServiceCase.actualTaxTotal))]
		[PXUIField(DisplayName = "Tax Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ActualCuryTaxTotal { get; set; }
		#endregion

		#region ActualTaxTotal
		public abstract class actualTaxTotal : IBqlField { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ActualTaxTotal { get; set; }
		#endregion

		#region EstimatedCuryLineTotal
		public abstract class estimatedCuryLineTotal : IBqlField { }

		[PXDBCurrency(typeof(CRServiceCase.curyInfoID), typeof(CRServiceCase.estimatedLineTotal))]
		[PXUIField(DisplayName = "Detail Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? EstimatedCuryLineTotal { get; set; }
		#endregion

		#region EstimatedLineTotal
		public abstract class estimatedLineTotal : IBqlField { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? EstimatedLineTotal { get; set; }
		#endregion

		#region ActualCuryLineTotal
		public abstract class actualCuryLineTotal : IBqlField { }

		[PXDBCurrency(typeof(CRServiceCase.curyInfoID), typeof(CRServiceCase.actualLineTotal))]
		[PXUIField(DisplayName = "Detail Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ActualCuryLineTotal { get; set; }
		#endregion

		#region ActualLineTotal
		public abstract class actualLineTotal : IBqlField { }

		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ActualLineTotal { get; set; }
		#endregion


		#region EstimatedCost

		public abstract class estimatedCost : IBqlField { }

		[PXDecimal(2)]
		[PXUIField(DisplayName = "Estimated Cost", Enabled = false)]
		public virtual Decimal? EstimatedCost { get; set; }

		#endregion

		#region FinalCost

		public abstract class finalCost : IBqlField { }

		[PXDecimal(2)]
		[PXUIField(DisplayName = "Final Cost", Enabled = false)]
		public virtual Decimal? FinalCost { get; set; }

		#endregion

		#region Duration
		public abstract class duration : IBqlField { }

		[PXTimeSpanLong]
		[PXUIField(DisplayName = "Duration", Enabled = false)]
		public virtual Int32? Duration { get; set; }
		#endregion


		#region CreatedByID
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.IBqlField
		{
		}
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTimeUtc]
		[PXUIField(DisplayName = "Service Call Date", Enabled = false)]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.IBqlField
		{
		}
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTimeUtc]
		[PXUIField(DisplayName = "Last Activity", Enabled = false)]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.IBqlField
		{
		}
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}
}
