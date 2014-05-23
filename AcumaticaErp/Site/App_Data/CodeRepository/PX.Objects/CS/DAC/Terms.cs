namespace PX.Objects.CS
{
	using System;
	using PX.Data;

    //namespace Constants
    //{
    //    //public static class TermsDueType
    //    //{
    //    //    public const string FixedNumberOfDays = "N";
    //    //    public const string DayOfNextMonth = "D";
    //    //    public const string EndOfMonth = "E";
    //    //    public const string EndOfNextMonth = "M";
    //    //    public const string Custom = "C";
    //    //}
    //    //public static class TermsInstallmentFrequency
    //    //{
    //    //    public const string Weekly = "W";
    //    //    public const string Monthly = "M";
    //    //    public const string BeMonthly = "B";
    //    //}

    //    //public static class TermsInstallmentType
    //    //{
    //    //    public const string Single = "S";
    //    //    public const string Multiple = "M";
    //    //}

    //    //public static class TermsInstallmentMethod
    //    //{
    //    //    public const string EqualParts = "E";
    //    //    public const string AllTaxInFirst = "A";
    //    //    public const string SplitByPercents = "S";
    //    //}
    //}

	[System.SerializableAttribute()]
	[PXPrimaryGraph(
		new Type[] { typeof(TermsMaint)},
		new Type[] { typeof(Select<Terms, 
			Where<Terms.termsID, Equal<Current<Terms.termsID>>>>)
		})]
	public partial class Terms : PX.Data.IBqlTable
	{
		#region TermsID
		public abstract class termsID : PX.Data.IBqlField
		{
		}
		protected String _TermsID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Terms ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search3<Terms.termsID, OrderBy<Asc<Terms.termsID>>>), CacheGlobal = true)]
		public virtual String TermsID
		{
			get
			{
				return this._TermsID;
			}
			set
			{
				this._TermsID = value;
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
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
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
		#endregion		#region DueType
		#region VisibleTo
		public abstract class visibleTo : PX.Data.IBqlField
		{
		}
		protected String _VisibleTo;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(TermsVisibleTo.All)]
		[PXUIField(DisplayName = "Visible To")]
        [TermsVisibleTo.List()]
		public virtual String VisibleTo
		{
			get
			{
				return this._VisibleTo;
			}
			set
			{
				this._VisibleTo = value;
			}
		}
				#endregion
		#region DueType
		public abstract class dueType : PX.Data.IBqlField
		{
		}
		protected String _DueType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TermsDueType.FixedNumberOfDays)]
		[PXUIField(DisplayName = "Due Date Type")]
		[TermsDueType.List()]
		public virtual String DueType
		{
			get
			{
				return this._DueType;
			}
			set
			{
				this._DueType = value;
			}
		}
		#endregion
		#region DayDue00
		public abstract class dayDue00 : PX.Data.IBqlField
		{
		}
		protected Int16? _DayDue00;
		[PXDBShort(MinValue = 0)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Due Day 1")]
		public virtual Int16? DayDue00
		{
			get
			{
				return this._DayDue00;
			}
			set
			{
				this._DayDue00 = value;
			}
		}
		#endregion
		#region DayFrom00
		public abstract class dayFrom00 : PX.Data.IBqlField
		{
		}
		protected Int16? _DayFrom00;
		[PXDBShort(MinValue =0,MaxValue=31)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Day From 1")]
		public virtual Int16? DayFrom00
		{
			get
			{
				return this._DayFrom00;
			}
			set
			{
				this._DayFrom00 = value;
			}
		}
		#endregion
		#region DayTo00
		public abstract class dayTo00 : PX.Data.IBqlField
		{
		}
		protected Int16? _DayTo00;
		[PXDBShort(MinValue = 0, MaxValue = 31)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Day To 1")]
		public virtual Int16? DayTo00
		{
			get
			{
				return this._DayTo00;
			}
			set
			{
				this._DayTo00 = value;
			}
		}
		#endregion
		#region DayDue01
		public abstract class dayDue01 : PX.Data.IBqlField
		{
		}
		protected Int16? _DayDue01;
		[PXDBShort(MinValue = 0, MaxValue = 31)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Due Day 2")]
		public virtual Int16? DayDue01
		{
			get
			{
				return this._DayDue01;
			}
			set
			{
				this._DayDue01 = value;
			}
		}
			#endregion
		#region DayFrom01
		public abstract class dayFrom01 : PX.Data.IBqlField
		{
		}
		protected Int16? _DayFrom01;
		[PXDBShort(MinValue = 0, MaxValue = 31)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Day From 2")]
		public virtual Int16? DayFrom01
		{
			get
			{
				return this._DayFrom01;
			}
			set
			{
				this._DayFrom01 = value;
			}
		}
		#endregion
		#region DayTo01
		public abstract class dayTo01 : PX.Data.IBqlField
		{
		}
		protected Int16? _DayTo01;
		[PXDBShort(MinValue = 0, MaxValue = 31)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Day To 2")]
		public virtual Int16? DayTo01
		{
			get
			{
				return this._DayTo01;
			}
			set
			{
				this._DayTo01 = value;
			}
		}
		#endregion
		#region DiscType
		public abstract class discType : PX.Data.IBqlField
		{
		}
		protected String _DiscType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TermsDueType.FixedNumberOfDays)]
		[PXUIField(DisplayName = "Discount Type")]
		[TermsDiscType.List()]
		public virtual String DiscType
		{
			get
			{
				return this._DiscType;
			}
			set
			{
				this._DiscType = value;
			}
		}
		#endregion
		#region DayDisc
		public abstract class dayDisc : PX.Data.IBqlField
		{
		}
		protected Int16? _DayDisc;
		[PXDBShort(MinValue=0)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Discount Day")]
		public virtual Int16? DayDisc
		{
			get
			{
				return this._DayDisc;
			}
			set
			{
				this._DayDisc = value;
			}
		}
		#endregion
		#region DiscPercent
		public abstract class discPercent : PX.Data.IBqlField
		{
		}
		protected Decimal? _DiscPercent;
		[PXDBDecimal(2,MinValue=0.0, MaxValue=100.0)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Discount %")]
		public virtual Decimal? DiscPercent
		{
			get
			{
				return this._DiscPercent;
			}
			set
			{
				this._DiscPercent = value;
			}
		}
		#endregion
		#region TermsInstallmentType
		public abstract class installmentType : PX.Data.IBqlField
		{
		}
		protected String _InstallmentType;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Installment Type")]
		[PXDefault(TermsInstallmentType.Single)]
        [TermsInstallmentType.List()]
		public virtual String InstallmentType
		{
			get
			{
				return this._InstallmentType;
			}
			set
			{
				this._InstallmentType = value;
			}
		}
		#endregion
		#region InstallmentCntr
		public abstract class installmentCntr : PX.Data.IBqlField
		{
		}
		protected Int16? _InstallmentCntr;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Number of Installments")]
		public virtual Int16? InstallmentCntr
		{
			get
			{
				return this._InstallmentCntr;
			}
			set
			{
				this._InstallmentCntr = value;
			}
		}
		#endregion
		#region InstallmentFreq
		public abstract class installmentFreq : PX.Data.IBqlField
		{
		}
		protected String _InstallmentFreq;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TermsInstallmentFrequency.Weekly)]
		[PXUIField(DisplayName = "Installment Frequency")]
        [TermsInstallmentFrequency.List()]
		public virtual String InstallmentFreq
		{
			get
			{
				return this._InstallmentFreq;
			}
			set
			{
				this._InstallmentFreq = value;
			}
		}
		#endregion
		#region InstallmentMthd
		public abstract class installmentMthd : PX.Data.IBqlField
		{
		}
		protected String _InstallmentMthd;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(TermsInstallmentMethod.EqualParts)]
		[PXUIField(DisplayName = "Installment Method")]
        [TermsInstallmentMethod.List()]
		public virtual String InstallmentMthd
		{
			get
			{
				return this._InstallmentMthd;
			}
			set
			{
				this._InstallmentMthd = value;
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
    #region Attributes
    public class TermsVisibleTo
	{
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { All, Vendor, Customer, Disabled },
                new string[] { Messages.All, Messages.Vendor, Messages.Customer, Messages.Disabled }) { ; }
        }
		public const string All = "AL";
		public const string Vendor = "VE";
		public const string Customer = "CU";
		public const string Disabled = "DS";

		public class all : Constant<string>
		{
			public all() : base(All) { ;}
		}
		public class vendor : Constant<string>
		{
			public vendor() : base(Vendor) { ;}
		}
		public class customer : Constant<string>
		{
			public customer() : base(Customer) { ;}
		}
		public class disabled : Constant<string>
		{
			public disabled() : base(Disabled) { ;}
		}
	}

    public class TermsDueType
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { FixedNumberOfDays, DayOfNextMonth, EndOfMonth, EndOfNextMonth,DayOfTheMonth, Prox, Custom},
				new string[] { Messages.FixedNumberOfDays, Messages.DayOfNextMonth, Messages.EndOfMonth, Messages.EndOfNextMonth, Messages.DayOfTheMonth, Messages.Prox, Messages.Custom }) { ; }
        }
        public const string FixedNumberOfDays = "N";
        public const string DayOfNextMonth = "D";
        public const string EndOfMonth = "E";
        public const string EndOfNextMonth = "M";
		public const string DayOfTheMonth = "T";
		public const string Prox = "P";
        public const string Custom = "C";

        public class fixedNumberOfDays : Constant<string>
        {
            public fixedNumberOfDays() : base(FixedNumberOfDays) { ;}
        }
        public class dayOfNextMonth : Constant<string>
        {
            public dayOfNextMonth() : base(DayOfNextMonth) { ;}
        }
        public class endOfMonth : Constant<string>
        {
            public endOfMonth() : base(EndOfMonth) { ;}
        }
        public class endOfNextMonth : Constant<string>
        {
            public endOfNextMonth() : base(EndOfNextMonth) { ;}
        }

		public class dayOfTheMonth: Constant<string>
		{
			public dayOfTheMonth() : base(DayOfTheMonth) { ;}
		}
		public class prox : Constant<string>
		{
			public prox() : base(Prox) { ;}
		}
        public class custom : Constant<string>
        {
            public custom() : base(Custom) { ;}
        }
    }

	public class TermsDiscType : TermsDueType 
	{
		public new class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { FixedNumberOfDays, DayOfNextMonth, EndOfMonth, EndOfNextMonth, DayOfTheMonth, Prox},
				new string[] { Messages.FixedNumberOfDays, Messages.DayOfNextMonth, Messages.EndOfMonth, Messages.EndOfNextMonth, Messages.DayOfTheMonth, Messages.Prox}) { ; }
		}

	}

    public class TermsInstallmentFrequency
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { Weekly, Monthly, SemiMonthly },
                new string[] { Messages.Weekly, Messages.Monthly, Messages.SemiMonthly }) { ; }
        }

        public const string Weekly = "W";
        public const string Monthly = "M";
        public const string SemiMonthly = "B";
        
        public class weekly : Constant<string>
        {
            public weekly() : base(Weekly) { ;}
        }
        public class monthly : Constant<string>
        {
            public monthly() : base(Monthly) { ;}
        }
        public class semiMonthly : Constant<string>
        {
            public semiMonthly() : base(SemiMonthly) { ;}
        }

    
    }

    public class TermsInstallmentType
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { Single, Multiple },
                new string[] { Messages.Single, Messages.Multiple}) { ; }
        }
    
        public const string Single = "S";
        public const string Multiple = "M";

        public class single : Constant<string>
        {
            public single() : base(Single) { ;}
        }
        public class multiple : Constant<string>
        {
            public multiple() : base(Multiple) { ;}
        }
    }

    public class TermsInstallmentMethod
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { EqualParts, AllTaxInFirst, SplitByPercents},
                new string[] { Messages.EqualParts, Messages.AllTaxInFirst, Messages.SplitByPercents }) { ; }
        }
        public const string EqualParts = "E";
        public const string AllTaxInFirst = "A";
        public const string SplitByPercents = "S";

        public class equalParts : Constant<string>
        {
            public equalParts() : base(EqualParts) { ;}
        }
        public class allTaxInFirst : Constant<string>
        {
            public allTaxInFirst() : base(AllTaxInFirst) { ;}
        }
        public class splitByPercents : Constant<string>
        {
            public splitByPercents() : base(SplitByPercents) { ;}
        }
    }
    #endregion
}
