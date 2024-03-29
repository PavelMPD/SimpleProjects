using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	[PXPrimaryGraph(typeof(EquipmentMaint))]
	[PXCacheName(Messages.Equipment)]
	[Serializable]
	public partial class INServiceItem : IBqlTable
	{
		#region ServiceItemID
		public abstract class serviceItemID : IBqlField { }

		[PXDBIdentity]
		[PXNavigateSelector(typeof(INServiceItem.serviceItemID))]
		public virtual Int32? ServiceItemID { get; set; }
		#endregion

		#region ServiceItemCD
		public abstract class serviceItemCD : IBqlField { }

		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Equipment ID", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(INSetup.serviceItemNumberingID), typeof(AccessInfo.businessDate))]
		[PXSelector(typeof(Search3<INServiceItem.serviceItemCD,
			LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INServiceItem.inventoryID>>,
			LeftJoin<Customer, On<INServiceItem.customerID, Equal<Customer.bAccountID>>>>, 
			OrderBy<Desc<INServiceItem.serviceItemCD>>>),
			typeof(INServiceItem.serviceItemCD),
			typeof(Customer.acctName))]
		[PXFieldDescription]
		public virtual String ServiceItemCD { get; set; }
		#endregion

		#region InventoryID
		public abstract class inventoryID : IBqlField { }

		[PXDBInt]
		[PXDefault]
		[PXUIField(DisplayName = "Inventory ID")]
		[PXSelector(typeof(Search<InventoryItem.inventoryID,
			Where2<Match<Current<AccessInfo.userName>>,
				And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
				And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
				And<InventoryItem.stkItem, Equal<True>>>>>>),
			typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr),
			typeof(InventoryItem.itemClassID), typeof(InventoryItem.itemStatus),
			typeof(InventoryItem.itemType))]
		public virtual Int32? InventoryID { get; set; }
		#endregion

		#region LotSerialNbr
		public abstract class lotSerialNbr : IBqlField { }

		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Lot/Serial Nbr.")]
		[PXSelectorWithoutVerification(typeof(Search4<INTranSplit.lotSerialNbr,
			Where<INTranSplit.inventoryID, Equal<Current<INServiceItem.inventoryID>>,
				And<INTranSplit.lotSerialNbr, IsNotNull>>,
			Aggregate<GroupBy<INTranSplit.lotSerialNbr>>>))]
		public virtual String LotSerialNbr { get; set; }
		#endregion

		#region WarrantyNbr
		public abstract class warrantyNbr : IBqlField { }

		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Warranty Nbr.")]
		public virtual String WarrantyNbr { get; set; }
		#endregion

		#region CustomerID
		public abstract class customerID : IBqlField { }

		[CustomerAndProspect]
		public virtual Int32? CustomerID { get; set; }
		#endregion

		#region ContactID
		public abstract class contactID : IBqlField { }

		[PXDBInt]
		[PXUIField(DisplayName = "Contact", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search2<Contact.contactID,
			InnerJoin<BAccount2, On<BAccount2.bAccountID, Equal<Contact.bAccountID>>>,
			Where<Contact.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>,
				And<Where2<Where<BAccount2.type, Equal<BAccountType.customerType>,
				Or<BAccount2.type, Equal<BAccountType.prospectType>>>,
					And<Where<BAccount2.bAccountID, Equal<Current<INServiceItem.customerID>>,
				Or<Current<INServiceItem.customerID>, IsNull>>>>>>>),
			DescriptionField = typeof(Contact.displayName), Filterable = true)]
		[PXDefault(typeof(Search2<BAccount2.defContactID,
			InnerJoin<Contact, On<Contact.contactID, Equal<BAccount2.defContactID>>>,
			Where<BAccount2.bAccountID, Equal<Current<INServiceItem.customerID>>,
				And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>),
			SearchOnDefault = false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? ContactID { get; set; }
		#endregion

		#region StartDate
		public abstract class startDate : IBqlField { }

		[PXDBDate]
		[PXUIField(DisplayName = "Start Date")]
		public virtual DateTime? StartDate { get; set; }
		#endregion

		#region ExpireDate

		public abstract class expireDate : IBqlField { }

		[PXDBDate]
		[PXUIField(DisplayName = "Expire Date")]
		public virtual DateTime? ExpireDate { get; set; }

		#endregion

		#region LastServiceDate

		public abstract class lastServiceDate : IBqlField { }

		[PXDBDate]
		[PXUIField(DisplayName = "Last Service Date")]
		public virtual DateTime? LastServiceDate { get; set; }

		#endregion

		#region LastIncidentDate

		public abstract class lastIncidentDate : IBqlField { }

		[PXDBDate]
		[PXUIField(DisplayName = "Last Incident")]
		public virtual DateTime? LastIncidentDate { get; set; }

		#endregion

		#region NextServiceDate

		public abstract class nextServiceDate : IBqlField { }

		[PXDBDate]
		[PXUIField(DisplayName = "Next Service Date")]
		public virtual DateTime? NextServiceDate { get; set; }

		#endregion

		#region NextServiceCode

		public abstract class nextServiceCode : IBqlField { }

		[PXDBString]
		[PXUIField(DisplayName = "Next Service Code")]
		public virtual String NextServiceCode { get; set; }

		#endregion

		#region NoteID
		public abstract class noteID : IBqlField { }

		[PXNote(DescriptionField = typeof(INServiceItem.serviceItemCD),
			Selector = typeof(INServiceItem.serviceItemCD))]
		public virtual Int64? NoteID { get; set; }
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
		[PXUIField(DisplayName = "Date Reported", Enabled = false)]
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
