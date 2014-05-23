using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.FA
{	
	#region FARecordType Attribute
	public class FARecordType
	{
		public class CustomListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues
			{
				get
				{
					return _AllowedValues;
				}
			}

			public string[] AllowedLabels
			{
				get
				{
					return _AllowedLabels;
				}
			}

			public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels)
				: base(AllowedValues, AllowedLabels)
			{
			}
		}

		public class MethodListAttribute : CustomListAttribute
		{
			public MethodListAttribute()
				: base(
					new string[] { ClassType, AssetType, BothType},
					new string[] { Messages.ClassType, Messages.AssetType, Messages.BothType}) { ; }
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { ClassType, AssetType, ElementType},
				new string[] { Messages.ClassType, Messages.AssetType, Messages.ElementType}) { ; }
		}

		public class NumberingAttribute : AutoNumberAttribute
		{
			public NumberingAttribute()
				: base(typeof(FixedAsset.recordType), typeof(FixedAsset.createdDateTime),
				new string[] { ClassType, AssetType, ElementType },
				new Type[] { null, typeof(Search<FASetup.assetNumberingID>), typeof(Search<FASetup.assetNumberingID>) })
			{
				NullMode = NullNumberingMode.UserNumbering;
			}
		}

		public const string ClassType = "C";
		public const string AssetType = "A";
		public const string ElementType = "E";
		public const string BothType = "B";

		public class classType : Constant<string>
		{
			public classType() : base(ClassType) { ;}
		}
		public class assetType : Constant<string>
		{
			public assetType() : base(AssetType) { ;}
		}
		public class elementType : Constant<string>
		{
			public elementType() : base(ElementType) { ;}
		}
		public class bothType : Constant<string>
		{
			public bothType() : base(BothType) { ;}
		}
	}
	#endregion
	#region FixedAssetStatus Attribute
	public class FixedAssetStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Active, Hold, Suspended, FullyDepreciated, Disposed, UnderConstruction, Reversed },
				new string[] { Messages.Active, Messages.Hold, Messages.Suspended, Messages.FullyDepreciated, Messages.Disposed, Messages.UnderConstruction, Messages.Reversed }) { ; }
		}

		public const string Active = "A";
		public const string Hold = "H";
		public const string Suspended = "S";
		public const string FullyDepreciated = "F";
		public const string Disposed = "D";
		public const string UnderConstruction = "C";
		public const string Dekitting = "K";
        public const string Reversed = "R";

        public class active : Constant<string>
		{
			public active() : base(Active) { ;}
		}
		public class hold : Constant<string>
		{
			public hold() : base(Hold) { ;}
		}
		public class suspended : Constant<string>
		{
			public suspended() : base(Suspended) { ;}
		}
		public class fullyDepreciated : Constant<string>
		{
			public fullyDepreciated() : base(FullyDepreciated) { ;}
		}
		public class disposed : Constant<string>
		{
			public disposed() : base(Disposed) { ;}
		}
		public class underConstruction : Constant<string>
		{
			public underConstruction() : base(UnderConstruction) { ;}
		}
		public class dekitting : Constant<string>
		{
			public dekitting() : base(Dekitting) { ;}
		}
        public class reversed : Constant<string>
        {
            public reversed() : base(Reversed) { ;}
        }
    }
	#endregion

	[System.SerializableAttribute()]
    [PXPrimaryGraph(new Type[]{
        typeof(AssetClassMaint),
        typeof(AssetClassMaint),
        typeof(AssetMaint),
        typeof(AssetMaint)},
        new Type[]{
            typeof(Select<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FixedAsset.assetID>>, And<FixedAsset.recordType, Equal<FARecordType.classType>>>>),
            typeof(Where<FAClass.assetID, Less<Zero>, And<FAClass.recordType, Equal<FARecordType.classType>>>),
            typeof(Select<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FixedAsset.assetID>>, And<FixedAsset.recordType, Equal<FARecordType.assetType>>>>),
            typeof(Where<FixedAsset.assetID, Less<Zero>, And<FixedAsset.recordType, Equal<FARecordType.assetType>>>)

        })]
	[PXCacheName(Messages.FixedAsset)]
	public partial class FixedAsset : IBqlTable
	{
		#region Selected
		public abstract class selected : IBqlField
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
		#region AssetID
		public abstract class assetID : PX.Data.IBqlField
		{
		}
		protected Int32? _AssetID;
		[PXDBIdentity()]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? AssetID
		{
			get
			{
				return this._AssetID;
			}
			set
			{
				this._AssetID = value;
			}
		}
		#endregion
		#region RecordType
		public abstract class recordType : PX.Data.IBqlField
		{
		}
		protected String _RecordType;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Record Type", TabOrder = 0)]
		[FARecordType.List()]
		public virtual String RecordType
		{
			get
			{
				return this._RecordType;
			}
			set
			{
				this._RecordType = value;
			}
		}
		#endregion
		#region AssetCD
		public abstract class assetCD : IBqlField
		{
		}
		protected String _AssetCD;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Asset ID", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(Search2<assetCD,
			LeftJoin<FADetails, On<FADetails.assetID, Equal<FixedAsset.assetID>>,
			LeftJoin<FALocationHistory, On<FALocationHistory.assetID, Equal<FixedAsset.assetID>, 
										And<FALocationHistory.revisionID, Equal<FADetails.locationRevID>>>,
			LeftJoin<Branch, On<Branch.branchID, Equal<FALocationHistory.locationID>>,
			LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<FALocationHistory.custodian>>,
            LeftJoin<FAClass, On<FAClass.assetID, Equal<FixedAsset.classID>>>>>>>, 
			Where<recordType, Equal<Current<recordType>>>>),
			typeof(assetCD),
			typeof(description),
			typeof(classID),
            typeof(FAClass.description),
            typeof(usefulLife),
			typeof(assetType),
			typeof(FADetails.status),
			typeof(Branch.branchCD),
			typeof(EPEmployee.acctName),
			typeof(FALocationHistory.department), 
			Filterable = true)]
		[FARecordType.Numbering]
		[PX.Data.EP.PXFieldDescription]
		public virtual String AssetCD
		{
			get
			{
				return _AssetCD;
			}
			set
			{
				_AssetCD = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.IBqlField
		{
		}
		protected Int32? _BranchID;
		[Branch()]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region ClassID
		public abstract class classID : PX.Data.IBqlField
		{
		}
		protected Int32? _ClassID;
		[PXDBInt]
        [PXSelector(typeof(Search<FAClass.assetID, Where<FAClass.recordType, Equal<FARecordType.classType>>>),
                    typeof(FAClass.assetCD), typeof(FAClass.assetType), typeof(FAClass.description), typeof(FAClass.usefulLife),
                    SubstituteKey = typeof(FAClass.assetCD),
                    DescriptionField = typeof(FAClass.description), CacheGlobal = true)]
        [PXUIField(DisplayName = "Asset Class", Visibility = PXUIVisibility.Visible, TabOrder = 3)]
		public virtual Int32? ClassID
		{
			get
			{
				return this._ClassID;
			}
			set
			{
				this._ClassID = value;
			}
		}
		#endregion
		#region OldClassID
		public abstract class oldClassID : PX.Data.IBqlField
		{
		}
		[PXInt()]
		[PXDBCalced(typeof(FixedAsset.classID), typeof(Int32), Persistent = true)]
		public virtual Int32? OldClassID
		{
			get;
			set;
		}
		#endregion
		#region ParentAssetID
		public abstract class parentAssetID : PX.Data.IBqlField
		{
		}
		protected Int32? _ParentAssetID;
		[PXDBInt()]
		[PXParent(typeof(Select<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FixedAsset.parentAssetID>>>>), UseCurrent = true, LeaveChildren = true)]
		[PXSelector(typeof(Search<FixedAsset.assetID, Where<FixedAsset.assetID, NotEqual<Current<FixedAsset.assetID>>,
													 And<Where<FixedAsset.recordType, Equal<Current<FixedAsset.recordType>>,
														   And<Current<FixedAsset.recordType>, NotEqual<FARecordType.elementType>,
														   Or<Current<FixedAsset.recordType>, Equal<FARecordType.elementType>,
														   And<FixedAsset.recordType, Equal<FARecordType.assetType>>>>>>>>),
					typeof(FixedAsset.assetCD), typeof(FixedAsset.assetType), typeof(FixedAsset.description), typeof(FixedAsset.usefulLife),
					SubstituteKey = typeof(assetCD),
					DescriptionField = typeof(description))]
		[PXUIField(DisplayName = "Parent Asset", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 2)]
		public virtual Int32? ParentAssetID
		{
			get
			{
				return this._ParentAssetID;
			}
			set
			{
				this._ParentAssetID = value;
			}
		}
		#endregion
		#region AssetType
		public abstract class assetType : PX.Data.IBqlField
		{
			public class CustomListAttribute : PXStringListAttribute
			{
				public string[] AllowedValues
				{
					get
					{
						return _AllowedValues;
					}
				}

				public string[] AllowedLabels
				{
					get
					{
						return _AllowedLabels;
					}
				}

				public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels)
					: base(AllowedValues, AllowedLabels)
				{
				}
			}

			public class TangibleListAttribute : CustomListAttribute
			{
				public TangibleListAttribute()
					: base(
						new string[] { Building, Ground, Vehicle, Machinery, Equipment, Computers, Furniture },
						new string[] { Messages.Building, Messages.Ground, Messages.Vehicle, Messages.Machinery, Messages.Equipment, Messages.Computers, Messages.Furniture }) { ; }
			}

			public class NonTangibleListAttribute : CustomListAttribute
			{
				public NonTangibleListAttribute()
					: base(
						new string[] { Software, Goodwill, Patents, Copyrights },
						new string[] { Messages.Software, Messages.Goodwill, Messages.Patents, Messages.Copyrights }) { ; }
			}

			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Building, Ground, Vehicle, Machinery, Equipment, Computers, Furniture, Software, Goodwill, Patents, Copyrights },
					new string[] { Messages.Building, Messages.Ground, Messages.Vehicle, Messages.Machinery, Messages.Equipment, Messages.Computers, Messages.Furniture, Messages.Software, Messages.Goodwill, Messages.Patents, Messages.Copyrights }) { ; }
			}

			public const string Building = "B";
			public const string Ground = "G";
			public const string Vehicle = "V";
			public const string Machinery = "M";
			public const string Equipment = "E";
			public const string Computers = "C";
			public const string Furniture = "F";


			public const string Software = "S";
			public const string Goodwill = "W";
			public const string Patents = "P";
			public const string Copyrights = "R";

			public class machinery : Constant<string>
			{
				public machinery() : base(Machinery) { ;}
			}
			public class building : Constant<string>
			{
				public building() : base(Building) { ;}
			}
			public class ground : Constant<string>
			{
				public ground() : base(Ground) { ;}
			}
			public class vehicle : Constant<string>
			{
				public vehicle() : base(Vehicle) { ;}
			}
			public class equipment : Constant<string>
			{
				public equipment() : base(Equipment) { ;}
			}
			public class computers : Constant<string>
			{
				public computers() : base(Computers) { ;}
			}
			public class furniture : Constant<string>
			{
				public furniture() : base(Furniture) { ;}
			}

			public class software : Constant<string>
			{
				public software() : base(Software) { ;}
			}
			public class goodwill : Constant<string>
			{
				public goodwill() : base(Goodwill) { ;}
			}
			public class patents : Constant<string>
			{
				public patents() : base(Patents) { ;}
			}
			public class copyrights : Constant<string>
			{
				public copyrights() : base(Copyrights) { ;}
			}
		}
		protected String _AssetType;
		[PXDBString(1, IsFixed = true)]
		[assetType.List]
		[PXUIField(DisplayName = "Asset Type", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 4, Required = true)]
		[PXDefault(typeof(Search<FixedAsset.assetType, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		public virtual String AssetType
		{
			get
			{
				return this._AssetType;
			}
			set
			{
				this._AssetType = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.IBqlField
		{
		}
		protected String _Status;
		[PXString(1, IsFixed = true)]
		[PXDBScalar(typeof(Search<FADetails.status, Where<FADetails.assetID, Equal<FixedAsset.assetID>>>))]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		[FixedAssetStatus.List()]
		[PXDefault(FixedAssetStatus.Active, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region Path
		public abstract class path : PX.Data.IBqlField
		{
		}
		protected String _Path;
		[PXDBString(250, IsUnicode = true)]
		[PXUIField(DisplayName = "Path")]
		[PXDefault("")]
		public virtual String Path
		{
			get
			{
				return this._Path;
			}
			set
			{
				this._Path = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.IBqlField
		{
		}
		protected String _Description;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 2)]
		[PX.Data.EP.PXFieldDescription]
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
		#region ConstructionAccountID
		public abstract class constructionAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _ConstructionAccountID;
		[Account(DisplayName = "Construction Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<FixedAsset.constructionAccountID, 
		                	Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>), 
		        	PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? ConstructionAccountID
		{
			get
			{
				return this._ConstructionAccountID;
			}
			set
			{
				this._ConstructionAccountID = value;
			}
		}
		#endregion
		#region ConstructionSubID
		public abstract class constructionSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _ConstructionSubID;
		[SubAccount(typeof(FixedAsset.constructionAccountID), DisplayName = "Construction Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<FixedAsset.constructionSubID,
							Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>),
							PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? ConstructionSubID
		{
			get
			{
				return this._ConstructionSubID;
			}
			set
			{
				this._ConstructionSubID = value;
			}
		}
		#endregion
		#region FAAccountID
		public abstract class fAAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _FAAccountID;
		[PXDefault(typeof(Search<FixedAsset.fAAccountID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		[Account(DisplayName = "Fixed Assets Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		public virtual Int32? FAAccountID
		{
			get
			{
				return this._FAAccountID;
			}
			set
			{
				this._FAAccountID = value;
			}
		}
		#endregion
		#region FASubID
		public abstract class fASubID : PX.Data.IBqlField
		{
		}
		protected Int32? _FASubID;
		[PXDefault]
		[SubAccount(typeof(FixedAsset.fAAccountID), DisplayName = "Fixed Assets Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? FASubID
		{
			get
			{
				return this._FASubID;
			}
			set
			{
				this._FASubID = value;
			}
		}
		#endregion
        #region FAAccrualAcctID
        public abstract class fAAccrualAcctID : PX.Data.IBqlField
        {
        }
        protected Int32? _FAAccrualAcctID;
        [PXDefault(typeof(FASetup.fAAccrualAcctID))]
        [Account(DisplayName = "FA Accrual Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
        public virtual Int32? FAAccrualAcctID
        {
            get
            {
                return this._FAAccrualAcctID;
            }
            set
            {
                this._FAAccrualAcctID = value;
            }
        }
        #endregion
        #region FAAccrualSubID
        public abstract class fAAccrualSubID : PX.Data.IBqlField
        {
        }
        protected Int32? _FAAccrualSubID;
        [PXDefault(typeof(FASetup.fAAccrualSubID))]
        [SubAccount(typeof(FixedAsset.fAAccrualAcctID), Visibility = PXUIVisibility.Visible, DisplayName = "FA Accrual Sub.", DescriptionField = typeof(Sub.description))]
        public virtual Int32? FAAccrualSubID
        {
            get
            {
                return this._FAAccrualSubID;
            }
            set
            {
                this._FAAccrualSubID = value;
            }
        }
        #endregion
		#region AccumulatedDepreciationAccountID
		public abstract class accumulatedDepreciationAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccumulatedDepreciationAccountID;
		[PXDefault(typeof(Search<FixedAsset.accumulatedDepreciationAccountID, 
			                     Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		[Account(DisplayName = "Accumulated Depreciation Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		public virtual Int32? AccumulatedDepreciationAccountID
		{
			get
			{
				return this._AccumulatedDepreciationAccountID;
			}
			set
			{
				this._AccumulatedDepreciationAccountID = value;
			}
		}
		#endregion
		#region AccumulatedDepreciationSubID
		public abstract class accumulatedDepreciationSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _AccumulatedDepreciationSubID;
		[PXDefault]
		[SubAccount(typeof(FixedAsset.accumulatedDepreciationAccountID), DisplayName = "Accumulated Depreciation Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? AccumulatedDepreciationSubID
		{
			get
			{
				return this._AccumulatedDepreciationSubID;
			}
			set
			{
				this._AccumulatedDepreciationSubID = value;
			}
		}
		#endregion
		#region DepreciatedExpenseAccountID
		public abstract class depreciatedExpenseAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _DepreciatedExpenseAccountID;
		[PXDefault(typeof(Search<FixedAsset.depreciatedExpenseAccountID,
								 Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		[Account(DisplayName = "Depreciation Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		public virtual Int32? DepreciatedExpenseAccountID
		{
			get
			{
				return this._DepreciatedExpenseAccountID;
			}
			set
			{
				this._DepreciatedExpenseAccountID = value;
			}
		}
		#endregion
		#region DepreciatedExpenseSubID
		public abstract class depreciatedExpenseSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _DepreciatedExpenseSubID;
		[PXDefault]
		[SubAccount(typeof(FixedAsset.depreciatedExpenseAccountID), DisplayName = "Depreciation Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? DepreciatedExpenseSubID
		{
			get
			{
				return this._DepreciatedExpenseSubID;
			}
			set
			{
				this._DepreciatedExpenseSubID = value;
			}
		}
		#endregion
		#region DisposalAccountID
		public abstract class disposalAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _DisposalAccountID;
		[PXDefault(typeof(Search<FixedAsset.disposalAccountID, 
							Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>), 
					PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Proceeds Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		public virtual Int32? DisposalAccountID
		{
			get
			{
				return this._DisposalAccountID;
			}
			set
			{
				this._DisposalAccountID = value;
			}
		}
		#endregion
		#region DisposalSubID
		public abstract class disposalSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _DisposalSubID;
		[PXDefault(typeof(Search<FixedAsset.disposalSubID, 
							Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>),
					PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(FixedAsset.disposalAccountID), DisplayName = "Proceeds Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		public virtual Int32? DisposalSubID
		{
			get
			{
				return this._DisposalSubID;
			}
			set
			{
				this._DisposalSubID = value;
			}
		}
		#endregion
		#region RentAccountID
		public abstract class rentAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _RentAccountID;
		[Account(DisplayName = "Rent Account", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Account.description), Visible = false)]
		public virtual Int32? RentAccountID
		{
			get
			{
				return this._RentAccountID;
			}
			set
			{
				this._RentAccountID = value;
			}
		}
		#endregion
		#region RentSubID
		public abstract class rentSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _RentSubID;
		[SubAccount(typeof(FixedAsset.rentAccountID), DisplayName = "Rent Sub.", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Sub.description), Visible = false)]
		public virtual Int32? RentSubID
		{
			get
			{
				return this._RentSubID;
			}
			set
			{
				this._RentSubID = value;
			}
		}
		#endregion
		#region LeaseAccountID
		public abstract class leaseAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _LeaseAccountID;
		[Account(DisplayName = "Lease Account", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Account.description), Visible = false)]
		public virtual Int32? LeaseAccountID
		{
			get
			{
				return this._LeaseAccountID;
			}
			set
			{
				this._LeaseAccountID = value;
			}
		}
		#endregion
		#region LeaseSubID
		public abstract class leaseSubID : PX.Data.IBqlField
		{
		}
		protected Int32? _LeaseSubID;
		[SubAccount(typeof(FixedAsset.leaseAccountID), DisplayName = "Lease Sub.", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Sub.description), Visible = false)]
		public virtual Int32? LeaseSubID
		{
			get
			{
				return this._LeaseSubID;
			}
			set
			{
				this._LeaseSubID = value;
			}
		}
		#endregion
        #region GainAcctID
        public abstract class gainAcctID : PX.Data.IBqlField
        {
        }
        protected Int32? _GainAcctID;
        [PXDefault(typeof(Search<FixedAsset.gainAcctID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
        [Account(DisplayName = "Gain Account", DescriptionField = typeof(Account.description))]
        public virtual Int32? GainAcctID
        {
            get
            {
                return this._GainAcctID;
            }
            set
            {
                this._GainAcctID = value;
            }
        }
        #endregion
        #region GainSubID
        public abstract class gainSubID : PX.Data.IBqlField
        {
        }
        protected Int32? _GainSubID;
        [PXDefault(typeof(Search<FixedAsset.gainSubID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
        [SubAccount(typeof(FixedAsset.gainAcctID),
            DescriptionField = typeof(Sub.description),
            DisplayName = "Gain Sub.")]
        public virtual Int32? GainSubID
        {
            get
            {
                return this._GainSubID;
            }
            set
            {
                this._GainSubID = value;
            }
        }
        #endregion
        #region LossAcctID
        public abstract class lossAcctID : PX.Data.IBqlField
        {
        }
        protected Int32? _LossAcctID;
        [PXDefault(typeof(Search<FixedAsset.lossAcctID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
        [Account(DisplayName = "Loss Account", DescriptionField = typeof(Account.description))]
        public virtual Int32? LossAcctID
        {
            get
            {
                return this._LossAcctID;
            }
            set
            {
                this._LossAcctID = value;
            }
        }
        #endregion
        #region LossSubID
        public abstract class lossSubID : PX.Data.IBqlField
        {
        }
        protected Int32? _LossSubID;
        [PXDefault(typeof(Search<FixedAsset.lossSubID, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
        [SubAccount(typeof(FixedAsset.lossAcctID),
            DescriptionField = typeof(Sub.description),
            DisplayName = "Loss Sub.")]
        public virtual Int32? LossSubID
        {
            get
            {
                return this._LossSubID;
            }
            set
            {
                this._LossSubID = value;
            }
        }
        #endregion

		#region FASubMask
		public abstract class fASubMask : IBqlField
		{
		}
		protected string _FASubMask;
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        public virtual string FASubMask
		{
			get
			{
				return _FASubMask;
			}
			set
			{
				_FASubMask = value;
			}
		}
		#endregion
		#region AccumDeprSubMask
		public abstract class accumDeprSubMask : IBqlField
		{
		}
		protected string _AccumDeprSubMask;
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        public virtual string AccumDeprSubMask
		{
			get
			{
				return _AccumDeprSubMask;
			}
			set
			{
				_AccumDeprSubMask = value;
			}
		}
		#endregion
		#region DeprExpenceSubMask
		public abstract class deprExpenceSubMask : IBqlField
		{
		}
		protected string _DeprExpenceSubMask;
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        public virtual string DeprExpenceSubMask
		{
			get
			{
				return _DeprExpenceSubMask;
			}
			set
			{
				_DeprExpenceSubMask = value;
			}
		}
		#endregion
		#region UseFASubMask
		public abstract class useFASubMask : IBqlField
		{
		}
		protected bool? _UseFASubMask;
		[PXDBBool]
		[PXDefault(true)]
        [PXUIField(DisplayName = "Use Fixed Asset Sub. Mask", FieldClass = SubAccountAttribute.DimensionName)]
		public virtual Boolean? UseFASubMask
		{
			get
			{
				return _UseFASubMask;
			}
			set
			{
				_UseFASubMask = value;
			}
		}
		#endregion
        #region ProceedsSubMask
        public abstract class proceedsSubMask : IBqlField
        {
        }
        protected string _ProceedsSubMask;
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        public virtual string ProceedsSubMask
        {
            get
            {
                return _ProceedsSubMask;
            }
            set
            {
                _ProceedsSubMask = value;
            }
        }
        #endregion
        #region GainLossSubMask
        public abstract class gainLossSubMask : IBqlField
        {
        }
        protected string _GainLossSubMask;
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        public virtual string GainLossSubMask
        {
            get
            {
                return _GainLossSubMask;
            }
            set
            {
                _GainLossSubMask = value;
            }
        }
        #endregion

		#region UsefulLife
		public abstract class usefulLife : PX.Data.IBqlField
		{
		}
		protected Decimal? _UsefulLife;
		[PXDBDecimal(4)]
		[PXDefault(typeof(Search<FixedAsset.usefulLife, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		[PXUIField(DisplayName = "Useful Life, Years", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? UsefulLife
		{
			get
			{
				return this._UsefulLife;
			}
			set
			{
				this._UsefulLife = value;
			}
		}
		#endregion
		#region InsuranceRequired
		public abstract class insuranceRequired : PX.Data.IBqlField
		{
		}
		protected Boolean? _InsuranceRequired;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Insurance Required")]
		public virtual Boolean? InsuranceRequired
		{
			get
			{
				return this._InsuranceRequired;
			}
			set
			{
				this._InsuranceRequired = value;
			}
		}
		#endregion
		#region IsTangible
		public abstract class isTangible : PX.Data.IBqlField
		{
		}
		protected Boolean? _IsTangible;
		[PXDBBool]
		[PXDefault(true, typeof(Search<FixedAsset.isTangible, Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>))]
		[PXUIField(DisplayName = "Tangible")]
		public virtual Boolean? IsTangible
		{
			get
			{
				return this._IsTangible;
			}
			set
			{
				this._IsTangible = value;
			}
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.IBqlField
		{
		}
		protected Boolean? _Active;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion
        #region Suspended
        public abstract class suspended : PX.Data.IBqlField
        {
        }
        protected Boolean? _Suspended;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Suspended")]
        public virtual Boolean? Suspended
        {
            get
            {
                return this._Suspended;
            }
            set
            {
                this._Suspended = value;
            }
        }
        #endregion
		#region HoldEntry
		public abstract class holdEntry : IBqlField
		{
		}
		protected Boolean? _HoldEntry;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Hold on Entry")]
		public virtual Boolean? HoldEntry
		{
			get
			{
				return _HoldEntry;
			}
			set
			{
				_HoldEntry = value;
			}
		}
		#endregion
		#region ServiceScheduleID
		public abstract class serviceScheduleID : PX.Data.IBqlField
		{
		}
		protected Int32? _ServiceScheduleID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Service Schedule", Visibility = PXUIVisibility.Invisible, Visible = false)]
		[PXSelector(typeof(Search<FAServiceSchedule.scheduleID>), 
					typeof(FAServiceSchedule.scheduleCD), typeof(FAServiceSchedule.serviceEveryPeriod), typeof(FAServiceSchedule.serviceEveryValue), typeof(FAServiceSchedule.serviceAfterUsageValue), typeof(FAServiceSchedule.serviceAfterUsageUOM), typeof(FAServiceSchedule.description),
					SubstituteKey = typeof(FAServiceSchedule.scheduleCD), DescriptionField = typeof(FAServiceSchedule.description))]
		[PXDefault(typeof(Search<FixedAsset.serviceScheduleID, 
							Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>), 
					PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? ServiceScheduleID
		{
			get
			{
				return this._ServiceScheduleID;
			}
			set
			{
				this._ServiceScheduleID = value;
			}
		}
		#endregion
		#region UsageScheduleID
		public abstract class usageScheduleID : PX.Data.IBqlField
		{
		}
		protected Int32? _UsageScheduleID;
		[PXDBInt()]
        [PXUIField(DisplayName = "Usage Measurement Schedule", Visibility = PXUIVisibility.Invisible, Visible = false)]
		[PXSelector(typeof(Search<FAUsageSchedule.scheduleID>), 
					typeof(FAUsageSchedule.scheduleCD), typeof(FAUsageSchedule.readUsageEveryPeriod), typeof(FAUsageSchedule.readUsageEveryValue), typeof(FAUsageSchedule.usageUOM), typeof(FAUsageSchedule.description),
					SubstituteKey = typeof(FAUsageSchedule.scheduleCD), DescriptionField = typeof(FAUsageSchedule.usageUOM))]
		[PXDefault(typeof(Search<FixedAsset.usageScheduleID,
							Where<FixedAsset.assetID, Equal<Current<FixedAsset.classID>>>>),
					PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? UsageScheduleID
		{
			get
			{
				return this._UsageScheduleID;
			}
			set
			{
				this._UsageScheduleID = value;
			}
		}
		#endregion
		#region RequiredRecalculation
		public abstract class requiredRecalculation : PX.Data.IBqlField
		{
		}
		protected Boolean? _RequiredRecalculation;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Required Recalculation", Enabled = false)]
		public virtual Boolean? RequiredRecalculation
		{
			get
			{
				return this._RequiredRecalculation;
			}
			set
			{
				this._RequiredRecalculation = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote(DescriptionField = typeof(FixedAsset.assetCD), 
			Selector = typeof(FixedAsset.assetCD))]
		public virtual Int64? NoteID
		{
			get
			{
				return _NoteID;
			}
			set
			{
				_NoteID = value;
			}
		}
		#endregion
        #region Qty
        public abstract class qty : IBqlField
        {
        }
        protected Decimal? _Qty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "1.0")]
        [PXUIField(DisplayName = "Quantity")]
        public virtual Decimal? Qty
        {
            get
            {
                return _Qty;
            }
            set
            {
                _Qty = value;
            }
        }
        #endregion
        #region SplittedFrom
        public abstract class splittedFrom : PX.Data.IBqlField
        {
        }
        protected int? _SplittedFrom;
        [PXDBInt]
        public virtual int? SplittedFrom
        {
            get
            {
                return _SplittedFrom;
            }
            set
            {
                _SplittedFrom = value;
            }
        }
        #endregion

        #region DisposalAmt
        public abstract class disposalAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _DisposalAmt;
        [PXBaseCury]
        [PXUIField(DisplayName = "Proceeds Amount")]
        public virtual Decimal? DisposalAmt
        {
            get
            {
                return this._DisposalAmt;
            }
            set
            {
                this._DisposalAmt = value;
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
        #region FADetails
        [Serializable()]
        public class FADetails : IBqlTable
        {
            #region AssetID
            public abstract class assetID : PX.Data.IBqlField
            {
            }
            protected Int32? _AssetID;
            [PXDBInt(IsKey = true)]
            public virtual Int32? AssetID
            {
                get
                {
                    return this._AssetID;
                }
                set
                {
                    this._AssetID = value;
                }
            }
            #endregion
            #region Status
            public abstract class status : PX.Data.IBqlField
            {
            }
            protected String _Status;
            [PXDBString(1, IsFixed = true)]
            public virtual String Status
            {
                get
                {
                    return this._Status;
                }
                set
                {
                    this._Status = value;
                }
            }
            #endregion
            #region LocationRevID
            public abstract class locationRevID : IBqlField
            {
            }
            protected Int32? _LocationRevID;
            [PXDBInt]
            public virtual Int32? LocationRevID
            {
                get
                {
                    return _LocationRevID;
                }
                set
                {
                    _LocationRevID = value;
                }
            }
            #endregion
        }
        #endregion
    }

    [Serializable]
    public partial class FAClass : FixedAsset
    {
        public new abstract class assetID : IBqlField {}
        public new abstract class recordType : IBqlField { }
        public new abstract class assetCD : IBqlField { }
        public new abstract class assetType : IBqlField { }
        public new abstract class description : IBqlField { }
        public new abstract class usefulLife : IBqlField { }
    }

    [PXCacheName(Messages.FAComponent)]
    [Serializable]
	public partial class FAComponent : FixedAsset
	{
		#region RecordType
		public new abstract class recordType : PX.Data.IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(FARecordType.ElementType)]
		[PXUIField(DisplayName = "Record Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, TabOrder = 0)]
		[FARecordType.List()]
		public override String RecordType
		{
			get
			{
				return this._RecordType;
			}
			set
			{
				this._RecordType = value;
			}
		}
		#endregion
		#region AssetCD
		public new abstract class assetCD : PX.Data.IBqlField
		{
		}
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Asset ID", Visibility = PXUIVisibility.SelectorVisible, TabOrder=1)]
		[PXSelector(typeof(Search<FAComponent.assetCD, Where<FAComponent.recordType, Equal<FARecordType.elementType>, And<Where<FAComponent.parentAssetID, IsNull, Or<FAComponent.assetCD, Equal<Current<FAComponent.assetCD>>>>>>>))]
		public override String AssetCD
		{
			get
			{
				return this._AssetCD;
			}
			set
			{
				this._AssetCD = value;
			}
		}
		#endregion	
	}
}
