using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.IN
{
	using System;
	using PX.Data;
	using PX.Objects.CS;

	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(INReplenishmentPolicyMaint))]
	[PXCacheName(Messages.ReplenishmentPolicy)]
	public partial class INReplenishmentPolicy : PX.Data.IBqlTable
	{
		#region ReplenishmentPolicyID
		public abstract class replenishmentPolicyID : PX.Data.IBqlField
		{
		}
		protected String _ReplenishmentPolicyID;
		[PXDefault()]
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Seasonality ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INReplenishmentPolicy.replenishmentPolicyID>))]
		[PX.Data.EP.PXFieldDescription]
		public virtual String ReplenishmentPolicyID
		{
			get
			{
				return this._ReplenishmentPolicyID;
			}
			set
			{
				this._ReplenishmentPolicyID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.IBqlField
		{
		}
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, Required=true)]
		[PX.Data.EP.PXFieldDescription]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion					
		#region CalendarID
		public abstract class calendarID : PX.Data.IBqlField
		{
		}
		protected String _CalendarID;
		[PXDBString(10, IsUnicode = true)]		
		[PXUIField(DisplayName = "Calendar", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
		public virtual String CalendarID
		{
			get
			{
				return this._CalendarID;
			}
			set
			{
				this._CalendarID = value;
			}
		}
		#endregion

		#region NoteID
		public abstract class noteID : IBqlField { }

		[PX.Data.PXNote(DescriptionField = typeof(INReplenishmentPolicy.replenishmentPolicyID),
			Selector = typeof(Search<INReplenishmentPolicy.replenishmentPolicyID>), 
			FieldList = new [] { typeof(INReplenishmentPolicy.replenishmentPolicyID), typeof(INReplenishmentPolicy.descr) })]
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
		[PXDBCreatedDateTime()]
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
		[PXDBLastModifiedDateTime()]
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

	public class INReplenishmentMethod
	{
		public const string None = "N";
		public const string MinMax = "M";
		public const string FixedReorder = "F";
		public class List : PXStringListAttribute
		{
			public List()
				: base(
				new string[] { None, MinMax, FixedReorder },
				new string[] { Messages.None, Messages.MinMax, Messages.FixedReorder })
			{
			}
		}
	}
	public class INReplenishmentSource
	{
		public const string None = "N";
		public const string Purchased = "P";
		public const string Manufactured = "M";
		public const string Transfer = "T";
		public const string DropShip = "D";
		public const string PurchaseToOrder = "O";
		public const string TransferToOrder = "R";
		public class List : PXStringListAttribute
		{
			public List()
				:base(
				new string[] { None, Purchased, Manufactured, Transfer, DropShip, PurchaseToOrder, TransferToOrder },
				new string[] { Messages.None, Messages.Purchased, Messages.Manufactured, Messages.Transfer, Messages.DropShip, Messages.PurchaseToOrder, Messages.TransferToOrder })
			{
			}
		}
		public class INPlanList : PXStringListAttribute
		{
			public INPlanList()
				:base(
				new string[] { Purchased, Transfer, },
				new string[] { Messages.Purchased, Messages.Transfer, })
			{
			}
		}
		public class SOList : PXStringListAttribute
		{
			public SOList()
				: base(
				PXAccess.FeatureInstalled<FeaturesSet.dropShipments>() ? new string[] { DropShip, PurchaseToOrder, TransferToOrder } : new string[] { PurchaseToOrder, TransferToOrder },
				PXAccess.FeatureInstalled<FeaturesSet.dropShipments>() ? new string[] { Messages.DropShip, Messages.PurchaseToOrder, Messages.TransferToOrder } : new string[] { Messages.PurchaseToOrder, Messages.TransferToOrder })
			{
			}
		}
		public class none : Constant<string>
		{
			public none() : base(None) { }
		}
		public class purchased : Constant<string>
		{
			public purchased() : base(Purchased){}
		}
		public class transfer : Constant<string>
		{
			public transfer() : base(Transfer) { }
		}
		public class manufactured : Constant<string>
		{
			public manufactured() : base(Manufactured) { }
		}
		public class dropShip : Constant<string>
		{
			public dropShip() : base(DropShip) { }
		}
		public class purchaseToOrder : Constant<string>
		{
			public purchaseToOrder() : base(PurchaseToOrder) { }
		}
		public class transferToOrder : Constant<string>
		{
			public transferToOrder() : base(TransferToOrder) { }
		}
		public static bool IsTransfer(string value)
		{
			return value == Transfer || value == TransferToOrder;
		}
	}
}
