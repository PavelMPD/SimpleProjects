namespace PX.Objects.CS
{
	using System;
	using PX.Data;
	using PX.Objects.AP;
	using PX.Objects.GL;
	using PX.Objects.TX;
using System.Collections.Generic;
	using PX.Objects.IN;
using PX.Objects.CA;
	
	[System.SerializableAttribute()]
	[PXPrimaryGraph(
		new Type[] { typeof(CarrierMaint)},
		new Type[] { typeof(Select<Carrier, 
			Where<Carrier.carrierID, Equal<Current<Carrier.carrierID>>>>)
		})]
	public partial class Carrier : PX.Data.IBqlTable
	{
		#region CarrierID
		public abstract class carrierID : PX.Data.IBqlField
		{
		}
		protected String _CarrierID;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXDefault()]
		[PXUIField(DisplayName = "Ship Via", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Carrier.carrierID>), CacheGlobal = true)]
		public virtual String CarrierID
		{
			get
			{
				return this._CarrierID;
			}
			set
			{
				this._CarrierID = value;
			}
		}
		#endregion
		#region CalcMethod
		public abstract class calcMethod : PX.Data.IBqlField
		{
		}
		protected String _CalcMethod;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("M")]
		[PXStringList(new string[] { "P", "N","M" }, new string[] { "Per Unit", "Net", "Manual" })]
		[PXUIField(DisplayName = "Calculation Method")]
		public virtual String CalcMethod
		{
			get
			{
				return this._CalcMethod;
			}
			set
			{
				this._CalcMethod = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.IBqlField
		{
		}
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.IBqlField
		{
		}
		protected String _TaxCategoryID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category")]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
        public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region CalendarID
		public abstract class calendarID : PX.Data.IBqlField
		{
		}
		protected String _CalendarID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Calendar")]
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
		#region FreightSalesAcctID
		public abstract class freightSalesAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _FreightSalesAcctID;
		[Account(DisplayName = "Freight Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault()]
		public virtual Int32? FreightSalesAcctID
		{
			get
			{
				return this._FreightSalesAcctID;
			}
			set
			{
				this._FreightSalesAcctID = value;
			}
		}
		#endregion
		#region FreightSalesSubID
		public abstract class freightSalesSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _FreightSalesSubID;
		[SubAccount(typeof(Carrier.freightSalesAcctID), DisplayName = "Freight Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault()]
		public virtual Int32? FreightSalesSubID
		{
			get
			{
				return this._FreightSalesSubID;
			}
			set
			{
				this._FreightSalesSubID = value;
			}
		}
		#endregion
		#region FreightExpenseAcctID
		public abstract class freightExpenseAcctID : PX.Data.IBqlField
		{
		}
		protected Int32? _FreightExpenseAcctID;
		[Account(DisplayName = "Freight Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault()]
		public virtual Int32? FreightExpenseAcctID
		{
			get
			{
				return this._FreightExpenseAcctID;
			}
			set
			{
				this._FreightExpenseAcctID = value;
			}
		}
		#endregion
		#region FreightExpenseSubID
		public abstract class freightExpenseSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _FreightExpenseSubID;
		[SubAccount(typeof(Carrier.freightExpenseAcctID), DisplayName = "Freight Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault()]
		public virtual Int32? FreightExpenseSubID
		{
			get
			{
				return this._FreightExpenseSubID;
			}
			set
			{
				this._FreightExpenseSubID = value;
			}
		}
		#endregion
		#region BaseRate
		public abstract class baseRate : PX.Data.IBqlField
		{
		}
		protected Decimal? _BaseRate;
		[PXDBDecimal(2)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Base Rate")]
		public virtual Decimal? BaseRate
		{
			get
			{
				return this._BaseRate;
			}
			set
			{
				this._BaseRate = value;
			}
		}
		#endregion
		#region IsExternal
		public abstract class isExternal : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsExternal;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "External Plug-in")]
		public virtual Boolean? IsExternal
		{
			get
			{
				return this._IsExternal;
			}
			set
			{
				this._IsExternal = value;
			}
		}
		#endregion
		#region CarrierPluginID
		public abstract class carrierPluginID : PX.Data.IBqlField
		{
		}
		protected String _CarrierPluginID;
		[PXDBString(15, IsUnicode = true, InputMask = ">aaaaaaaaaaaaaaa")]
		[PXUIField(DisplayName = "Carrier")]
		[PXSelector(typeof(Search<CarrierPlugin.carrierPluginID>), CacheGlobal = true)]
		public virtual String CarrierPluginID
		{
			get
			{
				return this._CarrierPluginID;
			}
			set
			{
				this._CarrierPluginID = value;
			}
		}
		#endregion
		#region PluginMethod
		public abstract class pluginMethod : PX.Data.IBqlField
		{
		}
		protected String _PluginMethod;
		[PXDBString(50)]
		[PXUIField(DisplayName = "Service Method")]
		[CarrierMethodSelector]
		public virtual String PluginMethod
		{
			get
			{
				return this._PluginMethod;
			}
			set
			{
				this._PluginMethod = value;
			}
		}
		#endregion

		#region ConfirmationRequired
		public abstract class confirmationRequired : PX.Data.IBqlField
		{
		}
		protected Boolean? _ConfirmationRequired;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Confirmation for Each Box Is Required", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? ConfirmationRequired
		{
			get
			{
				return this._ConfirmationRequired;
			}
			set
			{
				this._ConfirmationRequired = value;
			}
		}
		#endregion
		#region PackageRequired
		public abstract class packageRequired : PX.Data.IBqlField
		{
		}
		protected Boolean? _PackageRequired;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "At least one Package Is Required", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Boolean? PackageRequired
		{
			get
			{
				return this._PackageRequired;
			}
			set
			{
				this._PackageRequired = value;
			}
		}
		#endregion
        #region IsCommonCarrier
        public abstract class isCommonCarrier : PX.Data.IBqlField
        {
        }
        protected Boolean? _IsCommonCarrier;
        [PXDBBool()]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Common Carrier", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Boolean? IsCommonCarrier
        {
            get
            {
                return this._IsCommonCarrier;
            }
            set
            {
                this._IsCommonCarrier = value;
            }
        }
        #endregion

		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote()]
		public virtual Int64? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
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
	}

	public static class CarrierUnitsType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { SI, US },
				new string[] { Messages.SIUnits, Messages.USUnits }) { ; }
		}
		public const string SI = "S";
		public const string US = "U";
	}
}
