using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.CR.MassProcess;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.TM;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.SM;
using PX.Objects.PM;

namespace PX.TM
{
	#region PXOwnerSelectorAttribute

	using Objects.EP;
	/// <summary>
	/// Allows show employees for specified work group.
	/// </summary>
	/// <example>
	/// [PXOwnerSelector(typeof(MyDac.myField)]
	/// </example>
	public class PXOwnerSelectorAttribute : PXAggregateAttribute
	{
		#region State
		protected readonly int _SelAttrIndex;
		protected Type _workgroupType;
		#endregion

		#region DAC
		[PXProjection(typeof(Select2<Users,
			LeftJoin<Contact, On<Users.pKID, Equal<Contact.userID>>,
			LeftJoin<PX.Objects.EP.EPEmployee, On<Contact.contactID, Equal<PX.Objects.EP.EPEmployee.defContactID>>>>>), Persistent = false)]
		[CRCacheIndependentPrimaryGraph(
			typeof(EmployeeMaint),
			typeof(Select<PX.Objects.EP.EPEmployee,
				Where<PX.Objects.EP.EPEmployee.bAccountID, Equal<Current<EPEmployee.bAccountID>>>>))]
		[CRCacheIndependentPrimaryGraph(
			typeof(AccessUsers),
			typeof(Select<Users,
				Where<Current<EPEmployee.bAccountID>, IsNull, And<Users.pKID, Equal<Current<EPEmployee.pKID>>>>>))]
		public class EPEmployee : IBqlTable
		{
			#region PKID
			public abstract class pKID : PX.Data.IBqlField
			{
			}
			protected Guid? _PKID;
			[PXDBGuidMaintainDeleted(BqlTable = typeof(Users))]
			[PXDefault]
			[PXUIField(Visibility = PXUIVisibility.Invisible)]
			public virtual Guid? PKID
			{
				get
				{
					return this._PKID;
				}
				set
				{
					this._PKID = value;
				}
			}
			#endregion
			#region BAccountID
			public abstract class bAccountID : PX.Data.IBqlField
			{
			}
			protected Int32? _BAccountID;
			[PXDBIdentity(BqlTable = typeof(PX.Objects.EP.EPEmployee))]
			[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
			public virtual Int32? BAccountID
			{
				get
				{
					return this._BAccountID;
				}
				set
				{
					this._BAccountID = value;
				}
			}
			#endregion
			#region AcctCD
			public abstract class acctCD : PX.Data.IBqlField
			{
			}
			protected String _AcctCD;
			[PXDBString(30, IsUnicode = true, InputMask = "", BqlTable = typeof(PX.Objects.EP.EPEmployee))]
			[PXUIField(DisplayName = "Employee ID", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String AcctCD
			{
				get
				{
					return this._AcctCD;
				}
				set
				{
					this._AcctCD = value;
				}
			}
			#endregion
			#region AcctName
			public abstract class acctName : PX.Data.IBqlField
			{
			}
			protected String _AcctName;
			[PXDBString(60, IsUnicode = true, BqlTable = typeof(PX.Objects.EP.EPEmployee))]
			[PXUIField(DisplayName = "Employee Name")]
			public virtual String AcctName
			{
				get
				{
					return this._AcctName;
				}
				set
				{
					this._AcctName = value;
				}
			}
			#endregion
			#region PositionID
			public abstract class positionID : PX.Data.IBqlField
			{
			}
			protected String _PositionID;
			[PXDBString(30, IsUnicode = true, BqlTable = typeof(PX.Objects.EP.EPEmployee))]			
			[PXSelector(typeof(EPPosition.positionID), DescriptionField = typeof(EPPosition.description))]
			[PXUIField(DisplayName = "Position", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String PositionID
			{
				get
				{
					return this._PositionID;
				}
				set
				{
					this._PositionID = value;
				}
			}
			#endregion
			#region DepartmentID
			public abstract class departmentID : PX.Data.IBqlField
			{
			}
			protected String _DepartmentID;
			[PXDBString(30, IsUnicode = true, BqlTable = typeof(PX.Objects.EP.EPEmployee))]
			[PXSelector(typeof(EPDepartment.departmentID), DescriptionField = typeof(EPDepartment.description))]
			[PXUIField(DisplayName = "Department", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String DepartmentID
			{
				get
				{
					return this._DepartmentID;
				}
				set
				{
					this._DepartmentID = value;
				}
			}
			#endregion			
			#region Username
			public abstract class username : IBqlField { }
			[PXDBString(64, IsUnicode = true, InputMask = "",BqlTable = typeof(Users))]
			[PXDefault]
			[PXUIField(DisplayName = "Username", Visibility = PXUIVisibility.SelectorVisible)]			
			public virtual String Username { get; set; }
			#endregion
			#region DisplayName
			public abstract class displayName : IBqlField { }
			[PXString(40)]
			[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.SelectorVisible)]
			[PXDBCalced(typeof(Switch<
				Case<Where<IsNull<Users.firstName,Empty>, NotEqual<Empty>,
							And<IsNull<Users.lastName,Empty>, NotEqual<Empty>>>,
							Add<Users.firstName, Add<Space,Users.lastName>>>,
				IsNull<IsNull<Users.firstName, Users.lastName>,PX.Objects.EP.EPEmployee.acctName>>), typeof(string))]
			public virtual String DisplayName { get; set; }
			#endregion			
			#region ExtKey
			public abstract class extKey : IBqlField { }
			[PXString(IsKey = true)]			
			[PXDBCalced(typeof(PX.Objects.EP.EPEmployee.acctCD), typeof(string))]
			[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
			public virtual String ExtKey { get; set; }
			#endregion			
			#region Status
			public abstract class status : PX.Data.IBqlField{}
			protected String _Status;
			[PXDBString(1, IsFixed = true, BqlField = typeof(PX.Objects.EP.EPEmployee.status))]
			[PXUIField(DisplayName = "Status")]
			[BAccount.status.List()]
			[PXDefault(BAccount.status.Active)]
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
			#region IsApproved
			public abstract class isApproved : IBqlField { }
			[PXDBBool(BqlField = typeof(Users.isApproved))]
			[PXUIField(DisplayName = "Activated")]
			[PXDefault(true)]			
			public virtual bool? IsApproved { get; set; }
			#endregion
			#region DefContactID
			public abstract class defContactID : IBqlField { }
			[PXDBInt()]
			[PXUIField(DisplayName = "Default Contact")]
			[PXSelector(typeof(Search<Contact.contactID, Where<Contact.bAccountID, Equal<Current<EPEmployee.parentBAccountID>>>>))]
			public virtual Int32? DefContactID
			{
				get;
				set;
			}
			#endregion
			#region ParentBAccountID
			public abstract class parentBAccountID : IBqlField { }
			[PXDBInt()]
			[PXUIField(DisplayName = "Branch")]
			public virtual Int32? ParentBAccountID
			{
				get;
				set;
			}
			#endregion
		}
		#endregion

		public PXOwnerSelectorAttribute():this(null)
		{
			
		}
		public PXOwnerSelectorAttribute(Type workgroupType)
			: this(workgroupType,null)
		{
		}

		protected PXOwnerSelectorAttribute(Type workgroupType, Type search, bool validateValue = true)
		{
			PXSelectorAttribute selector;
			_Attributes.Add(selector = new PXSelectorAttribute(search ?? CreateSelect(workgroupType),
			typeof(EPEmployee.displayName), typeof(EPEmployee.acctCD), typeof(EPEmployee.positionID),
			typeof(EPEmployee.departmentID), typeof(EPEmployee.username)));
			_SelAttrIndex = _Attributes.Count - 1;

			selector.DescriptionField = typeof(EPEmployee.displayName);
			selector.SubstituteKey = typeof(EPEmployee.extKey);
			selector.ValidateValue = validateValue;
			_workgroupType = workgroupType;

			_Attributes.Add(new PXRestrictorAttribute(typeof(Where<EPEmployee.status, IsNull, Or<EPEmployee.status, NotEqual<BAccount.status.inactive>>>), Objects.EP.Messages.InactiveEpmloyee, typeof(EPEmployee.acctCD), typeof(EPEmployee.status)));
			_Attributes.Add(new PXRestrictorAttribute(typeof(Where<EPEmployee.isApproved, Equal<True>>), Objects.EP.Messages.InactiveUser, typeof(EPEmployee.username)));
		}		
		
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (_workgroupType != null)
			{
				sender.Graph.RowUpdated.AddHandler(sender.GetItemType(), RowUpdated);
			}			
		}

		private static Type CreateSelect(Type workgroupType)
		{
			if( workgroupType == null )
				return typeof(Search<EPEmployee.pKID,Where<EPEmployee.acctCD, IsNotNull>>);

			return BqlCommand.Compose(
							typeof(Search2<,,>), typeof(EPEmployee.pKID),
							typeof(LeftJoin<,>), typeof(EPCompanyTreeMember),
							typeof(On<,,>), typeof(EPCompanyTreeMember.userID), typeof(Equal<EPEmployee.pKID>),
							typeof(And<,>), typeof(EPCompanyTreeMember.workGroupID), typeof(Equal<>), typeof(Optional<>), workgroupType,
							typeof(Where<,,>), typeof(EPEmployee.acctCD), typeof(IsNotNull),
							typeof(And<>), typeof(Where<,,>),
							typeof(Optional<>), workgroupType, typeof(IsNull),
							typeof(Or<EPCompanyTreeMember.userID, IsNotNull>)
							);			
		}

		protected virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (e.Row != null)
			{
				int? WorkGroupID = (int?)sender.GetValue(e.Row, _workgroupType.Name);
				Guid? OwnerID = (Guid?)sender.GetValue(e.Row, _FieldOrdinal);

				if (!BelongsToWorkGroup(sender.Graph, WorkGroupID, OwnerID))
				{
					sender.SetValue(e.Row, _FieldOrdinal, OwnerWorkGroup(sender.Graph, WorkGroupID));
				}
			}
		}
		public static bool BelongsToWorkGroup(PXGraph graph, int? WorkGroupID, Guid? OwnerID)
		{
			if (WorkGroupID == null && OwnerID != null || OwnerID == null) return true;

			return PXSelect<EPCompanyTreeMember,
					Where<EPCompanyTreeMember.workGroupID, Equal<Required<EPCompanyTreeMember.workGroupID>>,
					And<EPCompanyTreeMember.userID, Equal<Required<EPCompanyTreeMember.userID>>>>>
					.Select(graph, WorkGroupID, OwnerID).Count > 0;
		}

		public static Guid? OwnerWorkGroup(PXGraph graph, int? WorkGroupID)
		{
			EPCompanyTreeMember member = PXSelect<EPCompanyTreeMember,
				Where<EPCompanyTreeMember.workGroupID, Equal<Required<EPCompanyTreeMember.workGroupID>>,
					And<EPCompanyTreeMember.isOwner, Equal<Required<EPCompanyTreeMember.isOwner>>>>>
				.Select(graph, WorkGroupID, 1);
			return member != null ? member.UserID : null;
		}
	}

	#endregion
	
	#region PXSubordinateOwnerSelectorAttribute

	/// <summary>
	/// Allows show employees which are subordinated or coworkers for current logined employee.
	/// </summary>
	/// <example>
	/// [PXSubordinateOwnerSelector]
	/// </example>
	public class PXSubordinateOwnerSelectorAttribute : PXOwnerSelectorAttribute
	{
		public PXSubordinateOwnerSelectorAttribute()
			: base(null, typeof(Search5<EPEmployee.pKID,
				LeftJoin<EPCompanyTreeMember, On<EPCompanyTreeMember.userID, Equal<EPEmployee.pKID>>>,
				Where<EPEmployee.pKID, Equal<Current<AccessInfo.userID>>,
										Or<EPCompanyTreeMember.workGroupID, Owned<Current<AccessInfo.userID>>>>,
				Aggregate<GroupBy<EPEmployee.pKID>>>), false)
		{			
		}
	}
	#endregion
}

namespace PX.Objects.EP
{
	#region PXSubordinateSelectorAttribute
	/// <summary>
	/// Allow show employees which are subordinated of coworkers for logined employee.
	/// You can specify additional filter for EPEmployee records.
	/// It's a 'BIZACCT' dimension selector.
	/// </summary>
	/// <example>
	/// [PXSubordinateSelector]
	/// </example>
	public class PXSubordinateSelectorAttribute : PXAggregateAttribute
	{		
		public Type DescriptionField
		{
			get
			{
				return this.GetAttribute<PXSelectorAttribute>().DescriptionField;
			}
			set
			{
				this.GetAttribute<PXSelectorAttribute>().DescriptionField = value;
			}
		}

		public Type SubstituteKey
		{
			get
			{
				return this.GetAttribute<PXSelectorAttribute>().SubstituteKey;
			}
			set
			{
				this.GetAttribute<PXSelectorAttribute>().SubstituteKey = value;
			}
		}

		public PXSubordinateSelectorAttribute(Type where)
		{
			PXDimensionAttribute attr = new PXDimensionAttribute("BIZACCT");
			attr.ValidComboRequired = true;
			_Attributes.Add(attr);

			PXSelectorAttribute selattr = new PXSelectorAttribute(GetCommand(where),
				typeof(EPEmployee.acctCD),
				typeof(EPEmployee.bAccountID), typeof(EPEmployee.acctName),
				typeof(EPEmployee.classID), typeof(EPEmployee.positionID), typeof(EPEmployee.departmentID),
				typeof(EPEmployee.defLocationID), typeof(Users.username), typeof(Users.displayName));
			selattr.SubstituteKey = typeof(EPEmployee.acctCD);
			selattr.DescriptionField = typeof (EPEmployee.acctName);

			_Attributes.Add(selattr);
			_Attributes.Add(new PXRestrictorAttribute(typeof(Where<EPEmployee.status, NotEqual<BAccount.status.inactive>>), Objects.EP.Messages.InactiveEpmloyee, typeof(EPEmployee.acctCD), typeof(EPEmployee.status)));			
		}

		public PXSubordinateSelectorAttribute()
			: this(null)
		{
		}

		private static Type GetCommand(Type where)
		{
			var whereType = typeof(Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
						Or<EPCompanyTreeMember.workGroupID, Owned<Current<AccessInfo.userID>>>>);
			if (where != null)
				whereType = BqlCommand.Compose(typeof(Where2<,>), 
					typeof(Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>,
						Or<EPCompanyTreeMember.workGroupID, Owned<Current<AccessInfo.userID>>>>),
					typeof(And<>), where);
			return BqlCommand.Compose(typeof(Search5<,,,>), typeof(EPEmployee.bAccountID),
				typeof(LeftJoin<Users, On<Users.pKID, Equal<EPEmployee.userID>>,
					LeftJoin<EPCompanyTreeMember, On<EPCompanyTreeMember.userID, Equal<EPEmployee.userID>>>>),
				whereType,
				typeof(Aggregate<GroupBy<EPEmployee.acctCD>>));
		}

		
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdating.RemoveHandler(sender.GetItemType(), _FieldName, this.GetAttribute<PXSelectorAttribute>().SubstituteKeyFieldUpdating);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, FieldUpdating);
		}

		protected virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.acctCD, Equal<Required<EPEmployee.acctCD>>>>
				.SelectWindowed(sender.Graph, 0, 1, e.NewValue);
			if (employee != null)
			{
				e.NewValue = employee.BAccountID;
				e.Cancel = true;
			}
			else
			{
				PXFieldUpdating fu = this.GetAttribute<PXDimensionAttribute>().FieldUpdating;
				fu(sender, e);
				e.Cancel = false;

				fu = this.GetAttribute<PXSelectorAttribute>().SubstituteKeyFieldUpdating;
				fu(sender, e);
				
			}
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) != typeof(IPXFieldUpdatingSubscriber) && 
				  typeof(ISubscriber) != typeof(IPXRowPersistingSubscriber) &&
					typeof(ISubscriber) != typeof(IPXFieldDefaultingSubscriber))
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}

        public static bool IsSubordinated(PXGraph graph, object employeeId)
        {
            var command = BqlCommand.CreateInstance(GetCommand(null));
            var filter = new[] { new PXFilterRow
							{
								DataField = typeof(EPEmployee.bAccountID).Name,
								Condition = PXCondition.EQ,
								Value = employeeId
							}};
            var view = new PXView(graph, true, command);
            int startRow = 0;
            int totalRows = 0;
            var res = view.Select(null, null, null, null, null, filter, ref startRow, 1, ref totalRows);
            return res.Return(_ => _.Count(), 0) > 0;
        }
	}

	#endregion

	#region PXEPEmployeeSelectorAttribute
	/// <summary>
	/// Dimension selector for EPEmployee.
	/// </summary>
	/// <example>
	/// [PXEPEmployeeSelector]
	/// </example>
	public class PXEPEmployeeSelectorAttribute : PXDimensionSelectorAttribute
	{
		public PXEPEmployeeSelectorAttribute()
			: base("BIZACCT",
			typeof(Search2<EPEmployee.bAccountID, LeftJoin<Users, On<Users.pKID, Equal<EPEmployee.userID>>>>),
			typeof(EPEmployee.acctCD),
			typeof(EPEmployee.bAccountID), typeof(EPEmployee.acctCD), typeof(EPEmployee.acctName),
			typeof(EPEmployee.positionID), typeof(EPEmployee.departmentID),
			typeof(EPEmployee.defLocationID), typeof(Users.username), typeof(Users.displayName))
		{ 
			DescriptionField = typeof(EPEmployee.acctName);
		}
	}
	#endregion

	public class PXWorkgroupSelectorAttribute : PXSelectorAttribute
	{
		public PXWorkgroupSelectorAttribute()
			:this(null)
		{
			
		}
		public PXWorkgroupSelectorAttribute(Type rootWorkgroupID)
			: base(rootWorkgroupID == null
							? typeof(Search3<EPCompanyTree.workGroupID, 
								 LeftJoin<EPCompanyTreeMember, On<EPCompanyTreeMember.workGroupID, Equal<EPCompanyTree.workGroupID>, And<EPCompanyTreeMember.isOwner, Equal<True>>>,
							   LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<EPCompanyTreeMember.userID>>>>, 
								 OrderBy<Asc<EPCompanyTree.description, Asc<EPCompanyTree.workGroupID>>>>)
			       	: BqlCommand.Compose(
			       		typeof (Search2<,,,>), typeof (EPCompanyTree.workGroupID),
								typeof(InnerJoin<EPCompanyTreeH, On<EPCompanyTreeH.workGroupID, Equal<EPCompanyTree.workGroupID>>,
											 LeftJoin<EPCompanyTreeMember, On<EPCompanyTreeMember.workGroupID, Equal<EPCompanyTree.workGroupID>, And<EPCompanyTreeMember.isOwner, Equal<True>>>,
											 LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<EPCompanyTreeMember.userID>>>>>),
								typeof(Where2<,>),
									typeof(Where<,,>), typeof(Current<>), rootWorkgroupID, typeof(IsNotNull), typeof(And<,>), typeof(EPCompanyTreeH.parentWGID), typeof(Equal<>), typeof(Current<>), rootWorkgroupID,
								typeof(Or<>),
									typeof(Where<,,>), typeof(Current<>), rootWorkgroupID, typeof(IsNull), typeof(And<,>), typeof(EPCompanyTreeH.parentWGID), typeof(Equal<>), typeof(EPCompanyTreeH.workGroupID),
			       		typeof (OrderBy<
			       			Asc<EPCompanyTree.description,
									Asc<EPCompanyTree.workGroupID>>>)), typeof(EPCompanyTree.description), typeof(EPEmployee.acctCD), typeof(EPEmployee.acctName))
		{
			SubstituteKey = typeof (EPCompanyTree.description);
		}
	}

	#region EPExpenceClaimSelectorAttribute

	/// <summary>
	/// Allow show expence claim records.
	/// </summary>
	/// <example>
	/// [EPExpenceClaimSelector]
	/// </example>
	public class EPExpenceClaimSelectorAttribute : PXSelectorAttribute
	{
		public EPExpenceClaimSelectorAttribute()
			: base(typeof(Search2<EPExpenseClaim.refNbr,
					InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPExpenseClaim.employeeID>>>,
					Where<EPExpenseClaim.createdByID, Equal<Current<AccessInfo.userID>>,
						 Or<EPEmployee.userID, Equal<Current<AccessInfo.userID>>, 
						 Or<EPEmployee.userID, OwnedUser<Current<AccessInfo.userID>>,
						 Or<EPExpenseClaim.noteID, Approver<Current<AccessInfo.userID>>>>>>>)
				)
		{
		}
	}

	#endregion

	#region EPTaxAttribute

	public class EPTaxAttribute : TaxAttribute
	{
		public EPTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryDocBal = typeof(EPExpenseClaim.curyDocBal);
			this.CuryLineTotal = typeof(EPExpenseClaim.curyLineTotal);
			this.DocDate = typeof(EPExpenseClaim.docDate);
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
			object[] currents = new object[] { row, ((ExpenseClaimEntry)graph).ExpenseClaim.Current };
			foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
					And<TaxRev.outdated, Equal<boolFalse>,
					And2<Where<TaxRev.taxType, Equal<TaxType.purchase>, And<Tax.reverseTax, Equal<boolFalse>,
						Or<TaxRev.taxType, Equal<TaxType.sales>, And<Tax.reverseTax, Equal<boolTrue>,
						Or<Tax.taxType, Equal<CSTaxType.use>>>>>>,
					And<Current<EPExpenseClaim.docDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>,
				Where>
				.SelectMultiBound(graph, currents, parameters))
			{
				tail[((Tax)record).TaxID] = record;
			}
			List<object> ret = new List<object>();
			switch (taxchk)
			{
				case PXTaxCheck.Line:
					foreach (EPTax record in PXSelect<EPTax,
						Where<EPTax.refNbr, Equal<Current<EPExpenseClaim.refNbr>>,
							And<EPTax.lineNbr, Equal<Current<EPExpenseClaimDetails.lineNbr>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<EPTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<EPTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcLine:
					foreach (EPTax record in PXSelect<EPTax,
						Where<EPTax.refNbr, Equal<Current<EPExpenseClaim.refNbr>>,
							And<EPTax.lineNbr, Less<intMax>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& ((EPTax)(PXResult<EPTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
								&& String.Compare(((Tax)(PXResult<EPTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<EPTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcTotals:
					foreach (EPTaxTran record in PXSelect<EPTaxTran,
						Where<EPTaxTran.refNbr, Equal<Current<EPExpenseClaim.refNbr>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (record.TaxID != null && tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<EPTaxTran, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<EPTaxTran, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				default:
					return ret;
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			if (sender.Graph is ExpenseClaimEntry)
			{
				base.CacheAttached(sender);
			}
			else
			{
				this.TaxCalc = TaxCalc.NoCalc;
			}
		}
	}

	#endregion

	#region EPSetupSelect

	public class EPSetupSelect : PXSetupOptional<EPSetup>
	{
		public EPSetupSelect(PXGraph graph)
			: base(graph)
		{

		}
	}

	#endregion

	#region EPAcctSubDefault

	public class EPAcctSubExpenseDefault
	{
        public static string[] GetAllowedValues()
		{
            List<string> list = new List<string>();
            list.Add(MaskEmployee);
            list.Add(MaskItem);
            list.Add(MaskCompany);

		    if (PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
		    {
                list.Add(MaskProject);
                list.Add(MaskTask);
		    }

		    return list.ToArray();
		}

        public static string[] GetAllowedLabels()
        {
            List<string> list = new List<string>();
            list.Add(Messages.MaskEmployee);
            list.Add(Messages.MaskItem);
            list.Add(Messages.MaskCompany);

            if (PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
            {
                list.Add(PM.Messages.Project);
                list.Add(Messages.Task);
            }

			return list.ToArray();
        }
        
        public const string MaskEmployee = "E";
		public const string MaskItem = "I";
		public const string MaskCompany = "C";
        public const string MaskProject = "P";
        public const string MaskTask = "T";
	}

	#endregion

	#region EPAcctSubSalesDefault

	public class EPAcctSubSalesDefault
	{
		public static string[] GetAllowedValues()
		{
			List<string> list = new List<string>();
			list.Add(MaskEmployee);
			list.Add(MaskItem);
			list.Add(MaskCompany);

			if (PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
			{
				list.Add(MaskProject);
				list.Add(MaskTask);
			}
			list.Add(MaskLocation);

			return list.ToArray();
		}

		public static string[] GetAllowedLabels()
		{
			List<string> list = new List<string>();
			list.Add(Messages.MaskEmployee);
			list.Add(Messages.MaskItem);
			list.Add(Messages.MaskCompany);

			if (PXAccess.FeatureInstalled<FeaturesSet.projectModule>())
			{
				list.Add(PM.Messages.Project);
				list.Add(Messages.Task);
			}
			list.Add(PO.Messages.CustomerLocationID);

			return list.ToArray();
		}

		public const string MaskEmployee = "E";
		public const string MaskItem = "I";
		public const string MaskCompany = "C";
		public const string MaskProject = "P";
		public const string MaskTask = "T";
		public const string MaskLocation = "L";
	}

	#endregion


	#region SubAccountMaskAttribute

	/// <summary>
	/// Determine mask for sub accounts used in EP module.
	/// </summary>
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class SubAccountSalesMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "EPSETUPSALE";
		public SubAccountSalesMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, EPAcctSubSalesDefault.MaskEmployee, EPAcctSubSalesDefault.GetAllowedValues(), EPAcctSubSalesDefault.GetAllowedLabels());
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField 
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, EPAcctSubSalesDefault.GetAllowedValues(), 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(EPAcctSubSalesDefault.GetAllowedValues()[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	#endregion

	#region SubAccountMaskAttribute

	/// <summary>
	/// Determine mask for sub accounts used in EP module.
	/// </summary>
	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class SubAccountExpenseMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "EPSETUPEXPENSE";
		public SubAccountExpenseMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, EPAcctSubExpenseDefault.MaskEmployee, EPAcctSubExpenseDefault.GetAllowedValues(), EPAcctSubExpenseDefault.GetAllowedLabels());
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, EPAcctSubExpenseDefault.GetAllowedValues(), 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(EPAcctSubExpenseDefault.GetAllowedValues()[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	#endregion

	#region EPMessageType

	public sealed class EPMessageType
	{
		/// <summary>
		/// List of attendee message types.
		/// </summary>
		/// <example>
		/// [EPMessageTypeList]
		/// </example>
		public sealed class EPMessageTypeListAttribute : PXIntListAttribute
		{
			public EPMessageTypeListAttribute()
				: base(new int[] { Invitation, CancelInvitation },
						 new string[] { Messages.Invitation, Messages.CancelInvitation })
			{

			}
		}

		public const int Invitation = 1;
		public const int CancelInvitation = 2;

		public class invitation : Constant<int>
		{
			public invitation() : base(Invitation) { }
		}

		public class cancelInvitation : Constant<int>
		{
			public cancelInvitation() : base(CancelInvitation) { }
		}
	}

	#endregion

	#region EPNotificationTemplateAttribute

	public sealed class EPNotificationTemplateAttribute : PXEventSubscriberAttribute, IPXRowSelectedSubscriber
	{
		private readonly Type _wikiIDBqlField;
		private readonly Type _pageIDBqlField;
		private string _wikiIDField;
		private string _pageIDField;

		public EPNotificationTemplateAttribute(Type wikiIdBqlField, Type pageIdBqlField)
		{
			_wikiIDBqlField = wikiIdBqlField;
			_pageIDBqlField = pageIdBqlField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_wikiIDField = sender.GetField(_wikiIDBqlField);
			_pageIDField = sender.GetField(_pageIDBqlField);
		}

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			object wikiID = sender.GetValue(e.Row, _wikiIDField);
			object pageID = sender.GetValue(e.Row, _pageIDField);

			object newvalue = null;
			if (wikiID != null && pageID != null)
			{
				PXResultset<WikiDescriptor> wikiSet = PXSelect<WikiDescriptor>.Search<WikiDescriptor.pageID>(sender.Graph, wikiID);
				if (wikiSet != null && wikiSet.Count > 0)
				{
					PXResultset<WikiPageSimple> pageSet = PXSelect<WikiPageSimple>.
						Search<WikiPageSimple.pageID, WikiPageSimple.wikiID>(sender.Graph, pageID, wikiID);
					if (pageSet != null && pageSet.Count > 0)
					{
						PXSiteMapNode nodeWiki = PXSiteMap.WikiProvider.FindSiteMapNodeFromKey(((WikiDescriptor)wikiSet[0]).PageID.Value);
						PXSiteMapNode nodePage = PXSiteMap.WikiProvider.FindSiteMapNodeFromKey(((WikiPageSimple)pageSet[0]).PageID.Value);
						if (pageSet != null && pageSet.Count > 0)
							newvalue = string.Format("{0}/{1}", nodeWiki.Title, nodePage.Title);
					}
				}
			}
			sender.SetValue(e.Row, _FieldOrdinal, newvalue);
		}
	}

	#endregion

	#region EPWikiPageSelectorAttribute
	/// <summary>
	/// Allow show articles of certain wiki.
	/// </summary>
	/// <example>
	/// [EPWikiPageSelector]
	/// </example>
	public sealed class EPWikiPageSelectorAttribute : PXCustomSelectorAttribute
	{
		private readonly Type _wiki;

		public EPWikiPageSelectorAttribute() : this(null) { }

		public EPWikiPageSelectorAttribute(Type wiki) :
			base(typeof(WikiPageSimple.pageID))
		{
			_wiki = wiki;
			_ViewName = GenerateViewName();
		}

		protected override string GenerateViewName()
		{
			return string.Concat(base.GenerateViewName(), "_", _wiki == null ? null : _wiki.Name);
		}

		public IEnumerable GetRecords([PXDBGuid] Guid? wikiId)
		{
			if (wikiId != null || _wiki != null && BqlCommand.GetItemType(_wiki) != null)
			{
				var wikiCache = _Graph.Caches[BqlCommand.GetItemType(_wiki)];
				var id = wikiId;
				if (id == null && _wiki != null)
				{
					var idValue = wikiCache.GetValue(wikiCache.Current, _wiki.Name);
					if (idValue != null) id = GUID.CreateGuid(idValue.ToString());
				}
				if (id != null)
					foreach (WikiPageSimple page in
						PXSelect<WikiPageSimple, Where<WikiPageSimple.wikiID, Equal<Required<WikiPageSimple.wikiID>>>>.
							Select(_Graph, id))
					{
						if (PXSiteMap.WikiProvider.GetAccessRights(page.PageID.Value) >= PXWikiRights.Select)
						{
							PXSiteMapNode node = PXSiteMap.WikiProvider.FindSiteMapNodeFromKey(page.PageID.Value);
							page.Title = (node != null && !string.IsNullOrEmpty(node.Title)) ? node.Title : page.Name;
							yield return page;
						}
					}
			}
		}
	}

	#endregion

	#region EPStartDateAttribute
	public sealed class EPStartDateAttribute : PXDBDateAndTimeAttribute
	{
		private string _DisplayFieldName;
		

		public EPStartDateAttribute()
		{
			_PreserveTime = true;
			base.UseTimeZone = true;
			base.WithoutDisplayNames = true;
		}

		public string DisplayName { get; set; }

		public override bool PreserveTime
		{
			get { return base.PreserveTime; }
			set { }
		}

		public override bool UseTimeZone
		{
			get { return base.UseTimeZone; }
			set { }
		}

		public override void CacheAttached(PXCache sender)
		{
			if (!typeof(EPActivity).IsAssignableFrom(sender.GetItemType()))
				throw new ArgumentException(Messages.EPActivityIsExpected);

			_DisplayFieldName = _FieldName + "_Display";
			sender.Fields.Add(_DisplayFieldName);
			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _DisplayFieldName, _DisplayFieldName_FieldSelecting);
			base.CacheAttached(sender);
		}

		private void _DisplayFieldName_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs args)
		{
			var info = string.Format("{0:g}", sender.GetValue(args.Row, _FieldOrdinal));
			string localDispName = DisplayName;
			if (!CultureInfo.InvariantCulture.Equals(System.Threading.Thread.CurrentThread.CurrentCulture))
				localDispName = PXLocalizer.Localize(DisplayName, sender.GetItemType().FullName);

			args.ReturnState = PXFieldState.CreateInstance(info, typeof(string), null, null, null, null, null,
						null, _DisplayFieldName, null, localDispName, null, PXErrorLevel.Undefined, false,
						true, true, PXUIVisibility.Visible, null, null, null);
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			this.InputMask = this.DisplayMask = RequireTimeOnActivity(sender) ? "g" : "d";
			base.FieldSelecting(sender, e);
		}

		public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			EPSetup setup = sender.Graph.Caches[typeof(EPSetup)].Current as EPSetup ?? new PXSetupSelect<EPSetup>(sender.Graph).SelectSingle();

			if (setup == null || setup.RequireTimes != true)
			{
				DateTime date = (e.NewValue as DateTime?) ?? PXTimeZoneInfo.Now;
				e.NewValue = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
			}
			base.FieldUpdating(sender, e);
		}		

		private bool RequireTimeOnActivity(PXCache sender)
		{
			EPSetup setup = null;
			try
			{
				setup = sender.Graph.Caches[typeof(EPSetup)].Current as EPSetup ?? new PXSetupSelect<EPSetup>(sender.Graph).SelectSingle();
			}
			catch {/* SKIP */}			
			return (setup != null ? setup.RequireTimes : null ) ?? false;			
		}
	}

	#endregion

	#region EPEndDayAttribute
	public sealed class EPEndDateAttribute : PXDBDateAndTimeAttribute, IPXRowUpdatedSubscriber
	{
		private readonly Type ActivityClass;
		private readonly Type StartDate;
		private readonly Type TimeSpent;

		public EPEndDateAttribute(Type activityClass, Type startDate, Type timeSpent)
		{
			this.ActivityClass = activityClass;
			this.StartDate = startDate;
			this.TimeSpent = timeSpent;
			this.InputMask = "g";
			this.PreserveTime = true;
			this.WithoutDisplayNames = true;
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			this.InputMask = this.DisplayMask = RequireTimeOnActivity(sender) ? "g" : "d";
			base.FieldSelecting(sender, e);
		}

		private bool RequireTimeOnActivity(PXCache sender)
		{
			EPSetup setup = null;
			try
			{
				setup = sender.Graph.Caches[typeof(EPSetup)].Current as EPSetup ?? new PXSetup<EPSetup>(sender.Graph).SelectSingle();
			}
			catch {/* SKIP */}
			return (setup != null ? setup.RequireTimes : null) ?? false;
		}

		#region IPXRowUpdatingSubscriber Members
		public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var side = (int?)sender.GetValue(e.Row, ActivityClass.Name);
			var startDate = (DateTime?)sender.GetValue(e.Row, StartDate.Name);			
			var endDate = (DateTime?)sender.GetValue(e.Row, _FieldName);
			var timeSpent = (int?)sender.GetValue(e.Row, TimeSpent.Name);
			DateTime? value = null;

			if (side != null && startDate != null)
			{
				value = endDate;
				switch ((int) side)
				{
					case CRActivityClass.Task:
						if (startDate > endDate)
							value = (DateTime) startDate;
						break;
					case CRActivityClass.Event:
						if (startDate > endDate || endDate == null)
							value = ((DateTime) startDate).AddMinutes(30);
						else if (Object.Equals(sender.GetValue(e.OldRow, _FieldName), endDate))
						{
							var oldStartDate = (DateTime?) sender.GetValue(e.OldRow, StartDate.Name);
							if (oldStartDate != null)
								value = ((DateTime) endDate).AddTicks(((DateTime) startDate - (DateTime) oldStartDate).Ticks);
						}
						break;
					default:
						value = timeSpent != null ? (DateTime?) ((DateTime) startDate).AddMinutes((int) timeSpent) : null;
						break;
				}
			}
			sender.SetValue(e.Row, _FieldName, value);
		}
		#endregion
	}
	#endregion

	#region EPAllDayAttribute
	public sealed class EPAllDayAttribute  : PXDBBoolAttribute, IPXRowUpdatedSubscriber, IPXRowSelectedSubscriber
	{
		private readonly Type StartDate;
		private readonly Type EndDate;
		public EPAllDayAttribute(Type startDate, Type endDate)
		{			
			this.StartDate = startDate;
			this.EndDate = endDate;
		}

		#region IPXRowUpdatingSubscriber Members
		public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			bool? allDay = (bool?)sender.GetValue(e.Row, _FieldName);
			var startDate = (DateTime?)sender.GetValue(e.Row, StartDate.Name);
			var endDate = (DateTime?)sender.GetValue(e.Row, EndDate.Name);
			if (allDay == true && startDate != null)
			{				
					sender.SetValue(e.Row, StartDate.Name, ((DateTime) startDate).Date);

					if (endDate == null || ((DateTime)endDate).Date <= ((DateTime)startDate).Date)
						endDate = ((DateTime) startDate).AddDays(1D);

					sender.SetValue(e.Row, EndDate.Name, ((DateTime) endDate).Date);									
			}
		}
		#endregion

		#region IPXRowSelectedSubscriber Members

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			bool? allDay = (bool?)sender.GetValue(e.Row, _FieldName);
			PXFieldState startState = sender.GetStateExt(e.Row, typeof(EPActivity.startDate) + PXDBDateAndTimeAttribute.DATE_FIELD_POSTFIX) as PXFieldState;
			PXFieldState endState = sender.GetStateExt(e.Row, typeof(EPActivity.endDate) + PXDBDateAndTimeAttribute.DATE_FIELD_POSTFIX) as PXFieldState;
			if(startState != null)
				PXDBDateAndTimeAttribute.SetTimeEnabled<EPActivity.startDate>(sender, e.Row, startState.Enabled && allDay != true );
			if (endState != null)
				PXDBDateAndTimeAttribute.SetTimeEnabled<EPActivity.endDate>(sender, e.Row, endState.Enabled &&  allDay != true );
	}

	#endregion
	}
	#endregion

	#region PXInvitationStatusAttribute

	/// <summary>
	/// List of invitation statuses.
	/// </summary>
	/// <example>
	/// [PXInvitationStatus]
	/// </example>
	public class PXInvitationStatusAttribute : PXIntListAttribute
	{
		public const int NOTINVITED = 0;
		public const int INVITED = 1;
		public const int ACCEPTED = 2;
		public const int REJECTED = 3;
		public const int RESCHEDULED = 4;
		public const int CANCELED = 5;

		public PXInvitationStatusAttribute()
			: base(
				new[] { INVITED, ACCEPTED, REJECTED, RESCHEDULED, CANCELED },
				new[] { Messages.InvitationInvited, 
				  Messages.InvitationAccepted,
				  Messages.InvitationRejected,
				  Messages.InvitationRescheduled,
				  Messages.InvitationCanceled })
		{
		}
	}

	#endregion

	#region EPDependNoteList

	public class EPDependNoteList<Table, FRefNoteID, ParentTable> : PXSelect<Table>
		where Table : class, IBqlTable, new()
		where ParentTable : class, IBqlTable
		where FRefNoteID : class, IBqlField
	{
		protected PXView _History;
		
		public EPDependNoteList(PXGraph graph)
			: base(graph)
		{
			PXDBDefaultAttribute.SetSourceType<FRefNoteID>(graph.Caches[typeof(Table)], SourceNoteID);
			this.View = new PXView(graph, false, BqlCommand.CreateInstance(BqlCommand.Compose(typeof(Select<,>), typeof(Table), ComposeWhere)));
			Init(graph);
		}
		public EPDependNoteList(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{			
			Init(graph);
		}

		protected Type SourceNoteID
		{
			get { return typeof(ParentTable).GetNestedType(EntityHelper.GetNoteField(typeof(ParentTable))); }
		}
		protected Type RefNoteID
		{
			get { return typeof (FRefNoteID); }
		}

		protected Type ComposeWhere
		{
			get { return BqlCommand.Compose(typeof (Where<,>), RefNoteID, typeof (Equal<>), typeof (Current<>), SourceNoteID); }
		}

		private void Init(PXGraph graph)
		{
			graph.RowInserted.AddHandler(BqlCommand.GetItemType(SourceNoteID), Source_RowInserted);
			graph.RowDeleted.AddHandler(BqlCommand.GetItemType(SourceNoteID), Source_RowDeleted);

			if(!graph.Views.Caches.Contains(typeof(Note)))
				graph.Views.Caches.Add(typeof(Note));

			PXCache source = graph.Caches[typeof(Table)];
			_History = CreateView(graph, BqlCommand.Compose(
				typeof(Select<,>), typeof(Table),
				typeof(Where<,>), RefNoteID,
				typeof(Equal<>), typeof(Required<>), SourceNoteID));		
		}

		protected virtual PXView CreateView(PXGraph graph, Type command)
		{
			PXView view = new PXView(graph, false, BqlCommand.CreateInstance(command));
			graph.Views.Add(Guid.NewGuid().ToString(), view);
			return view;
		}

		protected virtual void Source_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			foreach (Table item in this._History.SelectMulti(GetSourceNoteID(e.Row)))
				this.Cache.Delete(item);
		}

		protected virtual void Source_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PXNoteAttribute.GetNoteID(sender, e.Row, SourceNoteID.Name);			
		}

		protected long? GetSourceNoteID(object source)
		{
			PXCache cache = this._Graph.Caches[source.GetType()];
			var noteCache = _Graph.Caches[typeof(Note)];
			var oldDirty = noteCache.IsDirty;
			var res = (long?)PXNoteAttribute.GetNoteID(cache, source, SourceNoteID.Name);
			noteCache.IsDirty = oldDirty;
			return res;
		}

		protected long? GetRefNoteID(object source)
		{
			PXCache cache = this._Graph.Caches[source.GetType()];
			return (long?)cache.GetValue(source, RefNoteID.Name);
		}
	}
	#endregion

	#region EPApprovalList

	public class EPApprovalList<SourceAssign> : EPDependNoteList<EPApproval, EPApproval.refNoteID, SourceAssign>
		where SourceAssign : class, IAssign,  IBqlTable
	{
		#region State
		protected PXView _Pending;
		protected PXView _Find;
		protected PXView _Rejected;
		protected PXAction _Activity;
		#endregion

		#region Ctor
		public EPApprovalList(PXGraph graph, Delegate @delegate)
			: base(graph, @delegate)
		{
			Initialize(graph);
		}

		public EPApprovalList(PXGraph graph)
			: base(graph)
		{
			Initialize(graph);
		}

		private void Initialize(PXGraph graph)
		{
			PXCache source = graph.Caches[typeof(EPApproval)];
			
			_Activity = graph.Actions["RegisterActivity"];


			this.View = CreateView(graph, BqlCommand.Compose(
				typeof (Select2<,,>), typeof (EPApproval),
				typeof (LeftJoin<ApproverEmployee, On<ApproverEmployee.userID, Equal<EPApproval.ownerID>>,
					LeftJoin<ApprovedByEmployee, On<ApprovedByEmployee.userID, Equal<EPApproval.approvedByID>>>>),
				typeof(Where<,>), typeof(EPApproval.refNoteID), typeof(Equal<>), typeof(Current<>), SourceNoteID));

			_Pending = CreateView(graph, BqlCommand.Compose(
											typeof(Select<,>), typeof(EPApproval),
											typeof(Where<,,>), source.GetBqlField(typeof(EPApproval.refNoteID).Name),
											typeof(Equal<>), typeof(Required<>), SourceNoteID,
											typeof(And<,>), source.GetBqlField(typeof(EPApproval.status).Name),
											typeof(Equal<>), typeof(EPApprovalStatus.pending)));

			_Find = CreateView(graph, BqlCommand.Compose(
										typeof(Select<,>), typeof(EPApproval),
										typeof(Where<,,>), source.GetBqlField(typeof(EPApproval.refNoteID).Name),
										typeof(Equal<>), typeof(Required<>), SourceNoteID,
										typeof(And<,,>), source.GetBqlField(typeof(EPApproval.assignmentMapID).Name),
										typeof(Equal<>), typeof(Required<>), source.GetBqlField(typeof(EPApproval.assignmentMapID).Name),
										typeof(And<,>), source.GetBqlField(typeof(EPApproval.workgroupID).Name),
										typeof(Equal<>), typeof(Required<>), source.GetBqlField(typeof(EPApproval.workgroupID).Name)));

			_Rejected = CreateView(graph, BqlCommand.Compose(
										typeof(Select<,>), typeof(EPApproval),
										typeof(Where<,,>), source.GetBqlField(typeof(EPApproval.refNoteID).Name),
										typeof(Equal<>), typeof(Required<>), SourceNoteID,
										typeof(And<,>), source.GetBqlField(typeof(EPApproval.status).Name),
										typeof(Equal<>), typeof(EPApprovalStatus.rejected)));
		}


		#endregion

		#region Implementation
		public virtual bool Assign(SourceAssign source, params int?[] maps)
		{
			this.Reset(source);
			return Assign(source, null, maps);
		}

		public virtual void Reset(SourceAssign source)
		{
			foreach (EPApproval item in this._History.SelectMulti(GetSourceNoteID(source)))
				this.Cache.Delete(item);
		}

		public virtual bool Approve(SourceAssign source)
		{
			EPApproval item = (EPApproval)this._Rejected.SelectSingle(GetSourceNoteID(source));
			if (item != null) throw new PXException(Messages.CannotApproveRejectedItem);
			if (UpdateApproval(source, EPApprovalStatus.Approved))
			{
				this.RegisterActivity(source, PO.Messages.Approved);
				return true;
			}
			return false;
		}

		public virtual bool Reject(SourceAssign source)
		{
			if(UpdateApproval(source, EPApprovalStatus.Rejected))
			{
				this.RegisterActivity(source, PO.Messages.Rejected);
				return true;
			}
			return false;
		}

		public virtual void ClearPendingApproval(SourceAssign source)
		{
			foreach (EPApproval item in this._Pending.SelectMulti(GetSourceNoteID(source)))
			{
				this.Cache.Delete(item);
			}
		}

		public virtual bool IsApprover(SourceAssign source)
		{
			bool result = true;
			foreach (EPApproval item in this._Pending.SelectMulti(GetSourceNoteID(source)))
			{
				result = false;
				if (ValidateAccess(item.WorkgroupID))
					return true;
			}
			return result;
		}

		public virtual bool IsApproved(SourceAssign source)
		{
			var sourceNoteId = GetSourceNoteID(source);
			EPApproval item = (EPApproval)this._Pending.SelectSingle(sourceNoteId);
			if (item == null)
			{
				item = (EPApproval)this._Rejected.SelectSingle(sourceNoteId);
				return item == null;
			}
			return false;
		}

		private bool UpdateApproval(SourceAssign source, string status)
		{
			bool result = false;
			foreach (EPApproval item in this._Pending.SelectMulti(GetSourceNoteID(source)))
			{
				if (ValidateAccess(item.WorkgroupID))
				{
					item.ApprovedByID = PXAccess.GetUserID();
					item.ApproveDate = DateTime.Now;
					item.Status = status;
					if (this.Cache.Update(item) != null)
					{
						result = true;
						if (status == EPApprovalStatus.Approved)
							Assign(source, item, item.AssignmentMapID);
					}
				}
			}
			return result;
		}

		public bool ValidateAccess(SourceAssign source)
		{
			foreach (EPApproval item in this._Pending.SelectMulti(GetSourceNoteID(source)))
				if (ValidateAccess(item.WorkgroupID))
					return true;
			return false;
		}

		private bool Assign(SourceAssign source, EPApproval aproved, params int?[] maps)			
		{
			EPAssignmentProcessHelper<SourceAssign> helper = new EPAssignmentProcessHelper<SourceAssign>(this._Graph);
			PXCache cache = this.Cache.Graph.Caches[BqlCommand.GetItemType(SourceNoteID)];

			if (source == null) return false;

			bool result = false;
			foreach (int? map in maps)
			{
				if (map == null) continue;
				using (new CRAssigmentScope(source))
				{
					source.WorkgroupID = aproved == null ? null : aproved.WorkgroupID;
					source.OwnerID     = aproved == null ? null : aproved.OwnerID;

					helper.Assign(source, map.Value);
					if (source.WorkgroupID == null) continue;

					EPApproval exists = (EPApproval)this._Find.SelectSingle(GetSourceNoteID(source), map, source.WorkgroupID);

					if (exists == null)
					{
						result = true;
						EPApproval item = new EPApproval();
						item.RefNoteID = (long?)GetSourceNoteID(source);
						item.WorkgroupID = source.WorkgroupID;
						item.OwnerID = source.OwnerID;
						item.Status = EPApprovalStatus.Pending;
						item.AssignmentMapID = map;
						item = (EPApproval)this.Cache.Insert(item);
						if (aproved != null && item != null && ValidateAccess(item.WorkgroupID))
						{
							item.ApprovedByID = PXAccess.GetUserID();
							item.ApproveDate = DateTime.Now;
							item.Status = EPApprovalStatus.Approved;
							if (this.Cache.Update(item) != null)
								Assign(source, item, item.AssignmentMapID);
						}
					}
				}			
			}
			return result;
		}

		public virtual bool ValidateAccess(int? workgroup)
		{
			EPCompanyTree wg =
				PXSelect<EPCompanyTree,
					Where<EPCompanyTree.workGroupID, Equal<Required<EPCompanyTree.workGroupID>>,
						And<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>>.SelectWindowed(this._Graph, 0, 1, workgroup);
			return wg != null;
		}

		public virtual bool ValidateAccess(int? workgroup, Guid? ownerID)
		{
			if (workgroup == null && ownerID == null) return true;

			if (PXAccess.GetUserID() == ownerID) return true;			

			EPCompanyTree wg =
				PXSelect<EPCompanyTree,
					Where<EPCompanyTree.workGroupID, Equal<Required<EPCompanyTree.workGroupID>>,
						And<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>>.SelectWindowed(this._Graph, 0, 1, workgroup);
			return wg != null;
		}

		protected void RegisterActivity(object item, string summary)
		{
			if(_Activity != null)
			{
				PXAdapter adapter = new PXAdapter(new DummyView(this._Graph, new Select<SourceAssign>(), new List<object>{item}));
				adapter.Parameters = new object[] {summary};
				foreach (object r in _Activity.Press(adapter))
				{					
				}
			}
		}
		protected virtual void OnApprovalPersisted(PXCache cache, PXRowPersistedEventArgs e)
		{
			if(e.TranStatus == PXTranStatus.Open && e.Operation != PXDBOperation.Delete)
			{
				PXNotification.ProcessPersisted(cache.Graph, typeof(EPApproval), new List<object>(){e.Row});
			}
		}

		private sealed class DummyView : PXView
		{
			private readonly List<object>_records;
			internal DummyView(PXGraph graph, BqlCommand command, List<object> records)
				: base(graph, true, command)
			{
				_records = records;
			}
			public override List<object> Select(object[] currents, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
			{
				return _records;
			}
		}
		#endregion
	}	
	#endregion

	#region EPApprovalActionList
	public class EPApprovalAction<SourceAssign, Approved, Rejected> : EPApprovalList<SourceAssign>
		where SourceAssign : class, IAssign,  IBqlTable
		where Approved : class, IBqlField
		where Rejected : class, IBqlField
	{
		#region Ctor
		public EPApprovalAction(PXGraph graph, Delegate @delegate)
			: base(graph, @delegate)
		{
			Initialize(graph);
		}

		public EPApprovalAction(PXGraph graph)
			: base(graph)
		{
			Initialize(graph);
		}
		private void Initialize(PXGraph graph)
		{
			AddAction(graph, "Approve", Approve);
			AddAction(graph, "Reject" , Reject);		
		}
		#endregion

		public virtual void SetEnabledActions(PXCache sender, SourceAssign row, bool enable)
		{
			if (enable && !IsApprover(row))
				enable = false;

			sender.Graph.Actions["Approve"].SetEnabled(enable);
			sender.Graph.Actions["Approve"].SetEnabled(enable);
		}
		protected virtual void AddAction(PXGraph graph, string name, PXButtonDelegate handler)
		{
			graph.Actions[name] =
				(PXAction)Activator.CreateInstance(
				typeof(PXNamedAction<>).MakeGenericType(
				new Type[] { BqlCommand.GetItemType(SourceNoteID) }),
				new object[] { graph, name, handler }
				);
		}	

		[PXUIField(DisplayName = Messages.Approve)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Complete)]
		protected virtual IEnumerable Approve(PXAdapter adapter)
		{
			PXCache cache = _Graph.Caches[typeof (SourceAssign)];
			foreach (SourceAssign item in adapter.Get<SourceAssign>())
			{
				this.Approve(item);
				cache.SetValue<Approved>(item, IsApproved(item));
				yield return cache.Update(item);
			}
		}

		[PXUIField(DisplayName = Messages.Reject)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Remove)]
		protected virtual IEnumerable Reject(PXAdapter adapter)
		{
			PXCache cache = _Graph.Caches[typeof (SourceAssign)];
			foreach (SourceAssign item in adapter.Get<SourceAssign>())
			{
				if (Reject(item))
					cache.SetValue<Rejected>(item, true);
				cache.Update(item);
				yield return cache.Update(item);
			}
		}
	}
	#endregion

	#region EPApprovalAutomation
	public class EPApprovalAutomation<SourceAssign, Approved, Rejected, Hold> : EPApprovalList<SourceAssign>
		where SourceAssign : class, IAssign, IBqlTable
		where Approved : class, IBqlField
		where Rejected : class, IBqlField
		where Hold : class, IBqlField
	{
		#region Ctor
		public EPApprovalAutomation(PXGraph graph, Delegate @delegate)
			: base(graph, @delegate)
		{
			Initialize(graph);
		}

		public EPApprovalAutomation(PXGraph graph)
			: base(graph)
		{
			Initialize(graph);
		}

		private void Initialize(PXGraph graph) 
		{
			graph.FieldVerifying.AddHandler(BqlCommand.GetItemType(typeof(Approved)), typeof(Approved).Name, Approved_FieldVerifying);
			graph.FieldVerifying.AddHandler(BqlCommand.GetItemType(typeof(Rejected)), typeof(Rejected).Name, Rejected_FieldVerifying);
			graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(typeof(Approved)), typeof(Approved).Name, Approved_FieldUpdated);
			graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(typeof(Rejected)), typeof(Rejected).Name, Rejected_FieldUpdated);
			graph.FieldUpdated.AddHandler(BqlCommand.GetItemType(typeof(Hold)), typeof(Hold).Name, Hold_FieldUpdated);
		}
		#endregion
		#region Implementation
		protected virtual void Approved_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			if ((bool?)e.NewValue == true && !this.IsApprover((SourceAssign)e.Row))
			{
				throw new PXSetPropertyException(PO.Messages.DontHaveAppoveRights);
			}
		}
		protected virtual void Rejected_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((bool?)e.NewValue == true && !this.IsApprover((SourceAssign)e.Row))
			{
				PXUIFieldAttribute.SetError<Approved>(sender, e.Row, PO.Messages.DontHaveRejectRights);
				throw new PXSetPropertyException(PO.Messages.DontHaveRejectRights);
			}
		}
		protected virtual void Approved_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			SourceAssign doc = (SourceAssign)e.Row;
			PXFieldState state = (PXFieldState)cache.GetStateExt<Approved>(doc);
			if (e.Row != null &&
					(bool?)(cache.GetValue<Approved>(doc)) == true && (bool?)e.OldValue != true)
			{
				cache.SetValue<Approved>(doc, false);
				if (this.Approve(doc))
				{
					cache.SetValue<Approved>(doc, this.IsApproved(doc));				
				}
			}
		}
		protected virtual void Rejected_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			SourceAssign doc = (SourceAssign)e.Row;
			if (!((bool?)cache.GetValue<Rejected>(doc) == true && this.Reject(doc)))			
				cache.SetValue<Rejected>(doc, false);
		}
		protected virtual void Hold_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			SourceAssign doc = (SourceAssign)e.Row;
			bool? hold = (bool?) cache.GetValue<Hold>(doc);
			if (hold == true && e.OldValue != null && ((bool)e.OldValue) == false)
			{
				this.ClearPendingApproval(doc);
			}
		}
		#endregion
	}	
	#endregion

	#region DayOfWeekAttribute

	/// <summary>
	/// List days of week.
	/// </summary>
	/// <example>
	/// [DayOfWeek]
	/// </example>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class DayOfWeekAttribute : PXIntListAttribute
	{
		public DayOfWeekAttribute()
			: base(new[] { (int)DayOfWeek.Sunday, 
			               (int)DayOfWeek.Monday, 
			               (int)DayOfWeek.Tuesday, 
			               (int)DayOfWeek.Wednesday, 
			               (int)DayOfWeek.Thursday, 
			               (int)DayOfWeek.Friday, 
			               (int)DayOfWeek.Saturday },
			       new[] { GetDayName(DayOfWeek.Sunday), 
			               GetDayName(DayOfWeek.Monday), 
			               GetDayName(DayOfWeek.Tuesday), 
			               GetDayName(DayOfWeek.Wednesday), 
			               GetDayName(DayOfWeek.Thursday), 
			               GetDayName(DayOfWeek.Friday), 
			               GetDayName(DayOfWeek.Saturday) })
		{
		}

		private static string GetDayName(DayOfWeek day)
		{
			return CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(day);
		}
	}

	#endregion

	#region WorkTimeRemindDateAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class WorkTimeRemindDateAttribute : PXRemindDateAttribute
	{
		private readonly Type _activityEmployeeBqlField;
		private int _activityEmployeeFieldOrigin;
		private PXGraph _graph;

		public WorkTimeRemindDateAttribute(Type isReminderOnBqlField, Type startDateBqlField, Type activityEmployeeBqlField) 
			: base(isReminderOnBqlField, startDateBqlField)
		{
			_activityEmployeeBqlField = activityEmployeeBqlField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_activityEmployeeFieldOrigin = sender.GetFieldOrdinal(sender.GetField(_activityEmployeeBqlField));
			_graph = sender.Graph;
		}

		protected override DateTime? CalcCorrectValue(PXCache sender, object row)
		{
			_reversedRemindAt = 0;
			DateTime? result = base.CalcCorrectValue(sender, row);
			if (result != null && object.Equals(true, sender.GetValue(row, _isReminderOnFieldOrigin)))
			{
				var searchedCalendar = PXSelectJoin<CSCalendar,
					InnerJoin<EPEmployee, On<EPEmployee.calendarID, Equal<CSCalendar.calendarID>>>,
					Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.
					Select(_graph, sender.GetValue(row, _activityEmployeeFieldOrigin));
				if (searchedCalendar != null && searchedCalendar.Count != 0)
				{
					var calendar = (CSCalendar)searchedCalendar[0][typeof(CSCalendar)];
					var calendarTimeZone = string.IsNullOrEmpty(calendar.TimeZone) ? 
						PXTimeZoneInfo.Invariant :
						PXTimeZoneInfo.FindSystemTimeZoneById(calendar.TimeZone);
					var addTicks = 0L;
					switch (result.Value.DayOfWeek)
					{
						case DayOfWeek.Sunday:
							if (calendar.SunWorkDay == true) addTicks = calendar.SunStartTime.Value.TimeOfDay.Ticks;
							break;
						case DayOfWeek.Monday:
							if (calendar.MonWorkDay == true) addTicks = calendar.MonStartTime.Value.TimeOfDay.Ticks;
							break;
						case DayOfWeek.Tuesday:
							if (calendar.TueWorkDay == true) addTicks = calendar.TueStartTime.Value.TimeOfDay.Ticks;
							break;
						case DayOfWeek.Wednesday:
							if (calendar.WedWorkDay == true) addTicks = calendar.WedStartTime.Value.TimeOfDay.Ticks;
							break;
						case DayOfWeek.Thursday:
							if (calendar.ThuWorkDay == true) addTicks = calendar.ThuStartTime.Value.TimeOfDay.Ticks;
							break;
						case DayOfWeek.Friday:
							if (calendar.FriWorkDay == true) addTicks = calendar.FriStartTime.Value.TimeOfDay.Ticks;
							break;
						case DayOfWeek.Saturday:
							if (calendar.SatWorkDay == true) addTicks = calendar.SatStartTime.Value.TimeOfDay.Ticks;
							break;
					}
					var utcTime = result.Value.Date.AddTicks(addTicks - calendarTimeZone.BaseUtcOffset.Ticks);
					result = PXTimeZoneInfo.ConvertTimeFromUtc(utcTime, LocaleInfo.GetTimeZone());
				}
			}
			return result;
		}
	}

	#endregion

	#region EmployeeRawAttribute

	/// <summary>
	/// 'EMPLOYEE' dimension selector.
	/// </summary>
	/// <example>
	/// [EmployeeRaw]
	/// </example>
	public class EmployeeRawAttribute : AcctSubAttribute
	{
		#region EmployeeLogin

		[Serializable]
		[PXBreakInheritance]
		public sealed class EmployeeLogin : Users
		{
			#region PKID

			public new abstract class pKID : IBqlField { }

			#endregion

			#region Username

			public new abstract class username : IBqlField { }

			[PXDBString(64, IsKey = true, IsUnicode = true)]
			[PXUIField(Visible = false)]
			public override string Username
			{
				get
				{
					return base.Username;
				}
				set
				{
					base.Username = value;
				}
			}

			#endregion

		}

		#endregion

		public const string DimensionName = "EMPLOYEE";

		public EmployeeRawAttribute()
		{
			Type searchType = typeof(Search2<EPEmployee.acctCD, 
				LeftJoin<EmployeeLogin, On<EmployeeLogin.pKID, Equal<EPEmployee.userID>>,
				InnerJoin<GL.Branch, On<GL.Branch.bAccountID, Equal<EPEmployee.parentBAccountID>>>>, Where<MatchWithBranch<GL.Branch.branchID>>>);

			PXDimensionSelectorAttribute attr;
			_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, searchType, typeof(EPEmployee.acctCD),
									typeof(EPEmployee.bAccountID), typeof(EPEmployee.acctCD), typeof(EPEmployee.acctName),
									typeof(EPEmployee.status), typeof(EPEmployee.positionID), typeof(EPEmployee.departmentID),
									typeof(EPEmployee.defLocationID), typeof(EmployeeLogin.username), typeof(EmployeeLogin.displayName)));
			attr.DescriptionField = typeof(EPEmployee.acctName);
			_SelAttrIndex = _Attributes.Count - 1;
			this.Filterable = true;
		}
	}

	#endregion

	#region PXParentActivityInfoAttribute

	public class PXParentActivityInfoAttribute : PXEntityInfoAttribute
	{
		public PXParentActivityInfoAttribute(Type refNoteID) : base(refNoteID)
		{
		}

		protected override long GetEntityNoteID(PXCache cache, object row)
		{
			var activity = row as EPActivity;
			if (activity != null)
			{
				var resultSet = PXSelect<EPActivity,
					Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.
					Select(cache.Graph, activity.ParentTaskID);
				if (resultSet != null && resultSet.Count > 0)
					return Convert.ToInt64(((EPActivity)resultSet[0]).NoteID);
			}
			return 0;
		}
	}

	#endregion

	#region PXHistoryAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class PXHistoryAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber
	{
		private const string _VIEW_PREFIX = "_HISTORY_";

		private PXView _view;
		private string _noteField;

		private readonly Dictionary<long, EPActivity> _persistHistoryRowIds;

		public PXHistoryAttribute()
		{
			_persistHistoryRowIds = new Dictionary<long, EPActivity>();
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			ReadDacInfo();
			EnshureEventHandlers(sender.Graph);
			EnshureView(sender.Graph);

			sender.Graph.Initialized += GraphInitialized;
		}

		private void GraphInitialized(PXGraph sender)
		{
			EnshureEventHandlers(sender);
		}

		private void ReadDacInfo()
		{
			_noteField = EntityHelper.GetNoteField(_BqlTable);
			if (string.IsNullOrEmpty(_noteField))
				throw new PXException(string.Format(Messages.NoteIdFieldIsAbsent, _BqlTable.Name));
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert ||
				e.Operation == PXDBOperation.Delete)
			{
				return;
			}

			var oldRow = GetOldRow(e.Row);
			if (oldRow != null && !CompareField(oldRow, e.Row))
				AddHistoryRow(e.Row, oldRow);
		}

		private void AddHistoryRow(object row, object oldRow)
		{
			var cache = Graph.Caches[typeof(EPActivity)];
			var historyRow = GetHistoryRows(row).FirstOrDefault();
			if (historyRow == null)
			{
				var oldDirty = cache.IsDirty;
				historyRow = (EPActivity)cache.Insert();
				historyRow.ClassID = CRActivityClass.History;
				historyRow.RefNoteID = GetRefNoteID(row);
				historyRow.StartDate = PXTimeZoneInfo.Now;
				historyRow.UIStatus = ActivityStatusListAttribute.Completed;
				historyRow.Hold = true;
				historyRow.IsBillable = false;
				cache.IsDirty = oldDirty;
			}
			historyRow.Subject = "Changeset";

			var oldValue = TargetCache.GetValue(oldRow, _FieldOrdinal);
			var newValue = TargetCache.GetValue(row, _FieldOrdinal);
			var detailsCache = Graph.Caches[typeof(EPChangesetDetail)];
			var newDetail = (EPChangesetDetail)detailsCache.Insert();
			newDetail.ChangesetID = historyRow.TaskID;
			newDetail.SourceField = _FieldName;
			newDetail.OldValue = oldValue.With(_ => PXDatabase.Serialize(new[] { _ }));
			newDetail.NewValue = newValue.With(_ => PXDatabase.Serialize(new[] { _ }));
			detailsCache.IsDirty = false;
		}

		private object GetOldRow(object row)
		{
			var keyFields = TargetCache.Keys;
			if (keyFields.Count > 0)
			{
				var parameters = new object[keyFields.Count];
				for (int i = 0; i < keyFields.Count; i++)
				{
					var keyField = keyFields[i];
					var val = TargetCache.GetValue(row, keyField);
					parameters[i] = val;
				}
				return _view.SelectSingle(parameters);
			}
			return null;
		}

		private bool CompareField(object oldRow, object row)
		{
			var oldVal = TargetCache.GetValue(oldRow, _FieldOrdinal);
			var val = TargetCache.GetValue(row, _FieldOrdinal);
			return object.Equals(oldVal, val);
		}

		private void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			switch (e.TranStatus)
			{
				case PXTranStatus.Open:
					UnholdHistoryRow(e.Row);
					break;
				case PXTranStatus.Completed:
					PersistHistoryRow(e.Row);
					break;
				case PXTranStatus.Aborted:
					RemoveHistoryRow(e.Row);
					break;
			}
		}

		private void PersistHistoryRow(object row)
		{
			var persistList = new List<EPActivity>();
			var persistListDetails = new List<EPChangesetDetail>();
			var cache = Graph.Caches[typeof(EPActivity)];
				var detailsCache = Graph.Caches[typeof(EPChangesetDetail)];
			foreach (EPActivity historyRow in GetHistoryRows(row))
			{
				persistList.Add(historyRow);
				persistListDetails.AddRange(GetHistoryDetailsRows(historyRow));
			}
			using (var ts = new PXTransactionScope())
			{
				foreach (EPActivity historyRow in persistList)
					cache.PersistInserted(historyRow);
				foreach (EPChangesetDetail detail in persistListDetails)
					detailsCache.PersistInserted(detail);
				foreach (EPActivity historyRow in persistList)
				{
					cache.RaiseRowPersisted(historyRow, PXDBOperation.Insert, PXTranStatus.Completed, null);
					cache.Remove(historyRow);
				}
				foreach (EPChangesetDetail detail in persistListDetails)
				{
					detailsCache.RaiseRowPersisted(detail, PXDBOperation.Insert, PXTranStatus.Completed, null);
					detailsCache.Remove(detail);
				}
				ts.Complete();
			}
			cache.Normalize();
			detailsCache.Normalize();
			_persistHistoryRowIds.Clear();
		}

		private void RemoveHistoryRow(object row)
		{
			var removeList = new List<EPActivity>();
			var removeListDetails = new List<EPChangesetDetail>();
			var cache = Graph.Caches[typeof(EPActivity)];
			foreach (EPActivity historyRow in GetHistoryRows(row))
			{
				removeList.Add(historyRow);
				removeListDetails.AddRange(GetHistoryDetailsRows(historyRow));
			}
			foreach (EPActivity historyRow in removeList)
			{
				cache.RaiseRowPersisted(historyRow, PXDBOperation.Insert, PXTranStatus.Aborted, null);
				cache.Remove(historyRow);
			}
			var detailsCache = Graph.Caches[typeof(EPChangesetDetail)];
			foreach (EPChangesetDetail detail in removeListDetails)
			{
				detailsCache.RaiseRowPersisted(detail, PXDBOperation.Insert, PXTranStatus.Aborted, null);
				detailsCache.Remove(detail);
			}
		}

		private void UnholdHistoryRow(object row)
		{
			foreach (EPActivity historyRow in GetHistoryRows(row))
				historyRow.Hold = false;
		}

		private IEnumerable<EPChangesetDetail> GetHistoryDetailsRows(EPActivity row)
		{
			if (!row.TaskID.HasValue) yield break;

			var taskID = (int)row.TaskID;
			var detailsCache = Graph.Caches[typeof(EPChangesetDetail)];
			foreach (EPChangesetDetail detail in detailsCache.Cached)
				if (detail.ChangesetID == taskID)
					yield return detail;
		}

		private IEnumerable<EPActivity> GetHistoryRows(object row)
		{
			var status = TargetCache.GetStatus(row);
			if (status == PXEntryStatus.Inserted || status == PXEntryStatus.InsertedDeleted)
				yield break;

			var refNoteID = GetRefNoteID(row);
			var cache = Graph.Caches[typeof(EPActivity)];
			foreach (EPActivity historyRow in cache.Inserted)
				if (object.Equals(historyRow.RefNoteID, refNoteID))
					yield return historyRow;
		}

		private long GetRefNoteID(object row)
		{
			return PXNoteAttribute.GetNoteID(TargetCache, row, _noteField);
		}

		private PXCache TargetCache
		{
			get { return _view.Cache; }
		}

		private PXGraph Graph
		{
			get { return _view.Graph; }
		}

		private void EnshureView(PXGraph sender)
		{
			var viewName = _VIEW_PREFIX + _BqlTable.GetType().GetLongName();
			if (!sender.Views.TryGetValue(viewName, out _view))
			{
				var cmd = BqlCommand.CreateInstance(typeof(Select<>), _BqlTable);
				var cache = sender.Caches[_BqlTable];
				foreach (Type bqlField in cache.BqlKeys)
				{
					var newCondition = BqlCommand.Compose(
						typeof(Where<,>),
						bqlField,
						typeof(Equal<>),
						typeof(Required<>),
						bqlField);
					cmd = cmd.WhereAnd(newCondition);
				}

				_view = new PXView(sender, true, cmd);
				sender.Views.Add(viewName, _view);
			}
		}

		private void EnshureEventHandlers(PXGraph sender)
		{
			if (_view == null)
			{
				var cache = sender.Caches[_BqlTable];
				cache.Fields.Add("$history$hold");
				sender.RowPersisted.AddHandler(_BqlTable, RowPersisted);
				sender.RowPersisting.AddHandler(typeof(EPActivity), HistoryRowPersisting);
				sender.RowPersisting.AddHandler(typeof(EPChangesetDetail), HistoryRowDetailPersisting);
			}
		}

		private void HistoryRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = (EPActivity)e.Row;
			if (row.Hold == true) 
				e.Cancel = true;
			else 
				if (row.TaskID.HasValue)
					_persistHistoryRowIds[(int)row.TaskID] = row;
		}

		private void HistoryRowDetailPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = (EPChangesetDetail)e.Row;
			/*if (row == null || row.ChangesetID == null) return;

			var cache = Graph.Caches[typeof(EPActivity)];
			var parentRow = (EPActivity)cache.CreateInstance();
			parentRow.TaskID = row.ChangesetID;
			parentRow = (EPActivity)cache.Locate(parentRow);
			if (parentRow != null && parentRow.Hold == true) e.Cancel = true;*/

			if (row == null || row.ChangesetID == null || row.ChangesetID > -1) return;

			EPActivity parentRow;
			if (!_persistHistoryRowIds.TryGetValue((int)row.ChangesetID, out parentRow) || parentRow.Hold == true)
			{
				e.Cancel = true;
			}
			else
			{
				row.ChangesetID = parentRow.TaskID;
			}
		}
	}

	#endregion

	#region EPViewStatusAttribute

	public class EPViewStatusAttribute : PXIntListAttribute
	{
		public const int NOTVIEWED = 0;
		public const int VIEWED = 1;

		public sealed class NotViewed : Constant<int>
		{
			public NotViewed() : base(NOTVIEWED) { }
		}

		public sealed class Viewed : Constant<int>
		{
			public Viewed() : base(VIEWED) { }
		}

		public EPViewStatusAttribute()
			: base(
			new[] { NOTVIEWED, VIEWED },
			new[] { Messages.EntityIsNotViewed, 
				  Messages.EntityIsViewed })
		{
		}

		public static bool MarkAsViewed(PXGraph graph, object row, bool persist)
		{
			var res = MarkAsViewed(graph, row);
			if (res && persist)
			{
				var ts = new PXTransactionScope();
				try
				{
					var cache = graph.Caches[typeof(EPView)];
					cache.Persist(PXDBOperation.Insert);
					cache.Persist(PXDBOperation.Update);
					cache.Persist(PXDBOperation.Delete);
					ts.Complete(graph);
					cache.Clear();
				}
				catch (StackOverflowException) { throw; }
				catch (OutOfMemoryException) { throw; }
				catch (Exception)
				{
					res = false;
				}
				finally
				{
					ts.Dispose();
				}
			}
			return res;
		}

		public static bool MarkAsViewed(PXGraph graph, object row)
		{
			var activityType = row.GetType();
			var noteID = PXNoteAttribute.GetNoteID(graph.Caches[activityType], row, EntityHelper.GetNoteField(activityType));
			var result = false;
			if (noteID >= 0L)
			{
				var userID = PXAccess.GetUserID();
				PXSelect<EPView,
					Where<EPView.noteID, Equal<Required<EPView.noteID>>,
					And<EPView.userID, Equal<Required<EPView.userID>>>>>.Clear(graph);
				var item = (EPView)PXSelect<EPView,
									Where<EPView.noteID, Equal<Required<EPView.noteID>>,
									And<EPView.userID, Equal<Required<EPView.userID>>>>>.
									Select(graph, noteID, userID);
				PXCache cache = graph.Caches[typeof(EPView)];
				if (item == null)
				{
					item = (EPView)cache.Insert();
					item.NoteID = noteID;
					item.UserID = userID;
					cache.Normalize();
					result = true;
				}
				if (item.Status != EPViewStatusAttribute.VIEWED)
				{
					item.Status = EPViewStatusAttribute.VIEWED;
					cache.Update(item);
					result = true;
				}
				var persistableCaches = graph.Views.Caches;
				if (!persistableCaches.Contains(typeof(EPView))) persistableCaches.Add(typeof(EPView));
			}
			return result;
		}
	}

	#endregion

	#region CurrentEmployeeByDefaultAttribute

	/// <summary>
	/// Allow determine current logined employee on 'field default' event.
	/// </summary>
	/// <example>
	/// [CurrentEmployeeByDefault]
	/// </example>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class CurrentEmployeeByDefaultAttribute : PXDefaultAttribute
	{
		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var employee = EmployeeMaint.GetCurrentEmployee(sender.Graph);
			if (employee != null && employee.TimeCardRequired == true)
				e.NewValue = employee.BAccountID;
		}
	}

	#endregion

	#region PXWeekSelectorAttribute

	/// <summary>
	/// Allow select weeks.</br>
	/// Shows start and end date of week.
	/// </summary>
	/// <example>
	/// [PXWeekSelector]
	/// </example>
    [Serializable]
	public class PXWeekSelectorAttribute : PXCustomSelectorAttribute, IPXFieldDefaultingSubscriber
	{
		[PXVirtual]
        [Serializable]
		public partial class EPWeek : IBqlTable
		{
			#region WeekID

			public abstract class weekID : IBqlField { }

			[PXDBInt(IsKey = true)]
			[PXUIField(Visible = false)]
			public virtual Int32? WeekID { get; set; }

			#endregion

			#region FullNumber

			public abstract class fullNumber : IBqlField { }

			private String _fullNumber;
			[PXString]
			[PXUIField(DisplayName = "Week")]
			public virtual String FullNumber
			{
				get
				{
					Initialize();
					return _fullNumber;
				}
			}

			#endregion

			#region Number

			public abstract class number : IBqlField { }

			private Int32? _number;
			[PXInt]
			[PXUIField(DisplayName = "Number", Visible = false)]
			public virtual Int32? Number
			{
				get
				{
					Initialize();
					return _number;
				}
			}

			#endregion

			#region StartDate

			public abstract class startDate : IBqlField { }

			private DateTime? _startDate;
			[PXDate]
			[PXUIField(DisplayName = "Start")]
			public virtual DateTime? StartDate
			{
				get
				{
					Initialize();
					return _startDate;
				}
			}

			#endregion

			#region EndDate

			public abstract class endDate : IBqlField { }

			private DateTime? _endDate;
			[PXDate]
			[PXUIField(DisplayName = "End", Visibility = PXUIVisibility.Visible)]
			public virtual DateTime? EndDate
			{
				get
				{
					Initialize();
					return _endDate;
				}
			}

			#endregion

			#region Description

			public abstract class description : IBqlField { }

			[PXString]
			[PXUIField(DisplayName = "Description")]
			public virtual String Description
			{
				[PXDependsOnFields(typeof(fullNumber),typeof(startDate),typeof(endDate))]
				get
				{
                   
				    return string.Format("{0} ({1:MM/dd} - {2:MM/dd})", FullNumber, StartDate, EndDate);
				}
			}

			#endregion

			#region ShortDescription

			public abstract class shortDescription : IBqlField { }

			[PXString]
			[PXUIField(DisplayName = "Description", Visible = false)]
			public virtual String ShortDescription
			{
                [PXDependsOnFields(typeof(fullNumber), typeof(startDate), typeof(endDate))]
				get
				{
                    return string.Format("{0} ({1:MM/dd} - {2:MM/dd})",FullNumber , StartDate, EndDate);
				}
			}

			#endregion

			private void Initialize()
			{
				if (_number != null && _fullNumber != null && 
					_startDate != null && _endDate != null || WeekID == null)
				{
					return;
				}

				var weekId = (int)WeekID;
				var year = weekId / 100;
				var week = weekId % 100;
				_number = week;
				_fullNumber = string.Format("{0}-{1:00}", year, week);

				var startDate = PX.Data.Maintenance.PXDateTimeInfo.GetWeekStart(year, week);
				_startDate = startDate;
				_endDate = startDate.AddDays(7).AddTicks(-1L);
			}
		}

		private readonly Type _startDateBqlField;
		private readonly Type _timeSpentBqlField;

		private readonly bool _limited;
		private int _startDateOrdinal;
		private int _timeSpentOrdinal;

		public PXWeekSelectorAttribute()
			: base(typeof(EPWeek.weekID), 
				typeof(EPWeek.number), typeof(EPWeek.startDate), typeof(EPWeek.endDate))
		{
			
		}


		public PXWeekSelectorAttribute(Type type, Type[] fieldList)
			: base(type, fieldList)
		{
			
		}

		public PXWeekSelectorAttribute(Type startDateBqlField, Type timeSpentBqlField)
			: this()
		{
			_startDateBqlField = startDateBqlField;
			_timeSpentBqlField = timeSpentBqlField;
			_limited = true;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (_limited)
			{
				_startDateOrdinal = sender.GetFieldOrdinal(sender.GetField(_startDateBqlField));
				_timeSpentOrdinal = sender.GetFieldOrdinal(sender.GetField(_timeSpentBqlField));
			}
		}

		protected IEnumerable GetRecords()
		{
			if (!_limited) return GetAllRecords();

			var cache = _Graph.Caches[_CacheType];
			var startDate = cache.GetValue(cache.Current, _startDateOrdinal);
			return GetRecordsByDate((DateTime?)startDate);
		}

		protected virtual IEnumerable GetAllRecords()
		{
			var pageSize = PXView.MaximumRows;
			if (pageSize < 1) pageSize = 52;

			var startIndex = PXView.StartRow;
			/*var pageNumber = pageSize / */
			PXView.StartRow = 0;

			int year = -1;
			int week = -1;
			if (PXView.Searches != null && PXView.Searches.Length > 0)
				for (int i = 0; i < PXView.Searches.Length; i++)
				{
					var name = PXView.SortColumns[i].With(_ => _.ToLower());
					if (string.IsNullOrEmpty(name)) continue;

					var searchValue = PXView.Searches[i];
					if (searchValue == null) continue;

					if (typeof(EPWeek.weekID).Name.Equals(name, StringComparison.OrdinalIgnoreCase))
					{
						var weekId = searchValue is int ? (int)searchValue : int.Parse((string)searchValue);
						year = weekId / 100;
						week = weekId % 100;
						break;
					}

					var str = searchValue as string;
					if (str != null && typeof(EPWeek.fullNumber).Name.Equals(name, StringComparison.OrdinalIgnoreCase) && str.Length > 6)
					{
						year = int.Parse(str.Substring(0, 4));
						week = int.Parse(str.Substring(5, 2));
						break;
					}
				}

			if ((year == -1 || week == -1) && PXView.Filters != null && PXView.Filters.Length > 0)
			{
				var condArr = new[]
								{
									PXCondition.EQ, PXCondition.BETWEEN, PXCondition.GE, PXCondition.IN,
									PXCondition.LE, PXCondition.LLIKE, PXCondition.LIKE, PXCondition.RLIKE
								};
				foreach (PXFilterRow item in PXView.Filters)
				{
					var name = item.DataField;
					if (string.IsNullOrEmpty(name)) continue;
					var searchValue = item.Value;
					if (searchValue == null) continue;
					var condition = item.Condition;
					if (Array.IndexOf(condArr, condition) < 0) continue;

					if (typeof(EPWeek.weekID).Name.Equals(name, StringComparison.OrdinalIgnoreCase))
					{
						var weekId = searchValue is int ? (int)searchValue : int.Parse((string)searchValue);
						year = weekId / 100;
						week = weekId % 100;
						break;
					}

					var str = searchValue as string;
					if (str != null && typeof(EPWeek.fullNumber).Name.Equals(name, StringComparison.OrdinalIgnoreCase) && str.Length > 6)
					{
						year = int.Parse(str.Substring(0, 4));
						week = int.Parse(str.Substring(5, 2));
						break;
					}
				}
			}

			if (year == -1 || week == -1)
			{
				var toDay = PXTimeZoneInfo.Now.Date;
				year = toDay.Year;
				week = PX.Data.Maintenance.PXDateTimeInfo.GetWeekNumber(toDay);
			}

			var currentDate = PX.Data.Maintenance.PXDateTimeInfo.GetWeekStart(year, week).AddDays(startIndex * 7);
			if (currentDate.Year < 1901) yield break;

			var currentWeek = PX.Data.Maintenance.PXDateTimeInfo.GetWeekNumber(currentDate);
			int currentYear = currentDate.Year;

			if (currentWeek >52 )
			{
				if (currentDate.Day > 28)
				{
					currentYear++;
					currentWeek = 1;
				}
			}

			
			for (int i = 0; i < pageSize; i++)
			{
				yield return new EPWeek { WeekID = currentYear * 100 + currentWeek };

				currentDate = currentDate.AddDays(7D);
				if (currentWeek < 52) 
					currentWeek++;
				else
				{
					currentWeek = PX.Data.Maintenance.PXDateTimeInfo.GetWeekNumber(currentDate);
					if (currentWeek == 1)
					{
						currentYear++;
					}
				}

				if (currentDate.Year > 9998) yield break;
			}
		}

		public static IEnumerable GetRecordsByDate(DateTime? startDate)
		{
			if (startDate == null) yield break;

			var date = (DateTime)startDate;
			var dateWeek = PX.Data.Maintenance.PXDateTimeInfo.GetWeekNumber(date);
			var utcDate = PXTimeZoneInfo.ConvertTimeToUtc(date, LocaleInfo.GetTimeZone());
			var utcDateWeek = PX.Data.Maintenance.PXDateTimeInfo.GetWeekNumber(utcDate);
			if (dateWeek != utcDateWeek)
			{
				if (date > utcDate)
				{
					yield return new EPWeek { WeekID = utcDate.Year * 100 + utcDateWeek };
					yield return new EPWeek { WeekID = date.Year * 100 + dateWeek };
				}
				else
				{
					yield return new EPWeek { WeekID = date.Year * 100 + dateWeek };
					yield return new EPWeek { WeekID = utcDate.Year * 100 + utcDateWeek };
				}
			}
			else
				yield return new EPWeek { WeekID = date.Year * 100 + dateWeek };
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (_limited)
			{
				var oldCurrent = sender.Current;
				sender.Current = e.Row;
				base.FieldVerifying(sender, e);
				sender.Current = oldCurrent;
			}
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!_limited) return;

			var startDate = sender.GetValue(e.Row, _startDateOrdinal);
			if (startDate != null)
			{
				var date = (DateTime)startDate;
				e.NewValue = GetWeekID(date);
			}
		}

		public static int GetWeekID(DateTime date)
		{
			int year = date.Year;
			int week = PX.Data.Maintenance.PXDateTimeInfo.GetWeekNumber(date);

			if (date.Day > 28 && date.Month == 12)
			{
				year++;
				week = 1;
			}

			return year * 100 + week;
		}

		public static DateTime GetWeekSrartDate(int weekId)
		{
			var year = weekId / 100;
			var weekNumber = weekId % 100;
			return PX.Data.Maintenance.PXDateTimeInfo.GetWeekStart(year, weekNumber);
		}
	}

	#endregion

	#region PXWeekSelector2Attribute

	/// <summary>
	/// Allow select weeks.</br>
	/// Shows start and end date of week, fixed diapason only.
	/// </summary>
	/// <example>
	/// [PXWeekSelector2]
	/// </example>
	[Serializable]
	public class PXWeekSelector2Attribute : PXWeekSelectorAttribute
	{
		public static class FullWeekList
		{
			private static DateTime _stratDay = new DateTime(2005, 1, 1);
			private static int weekListCount = 800;
			private static List<EPWeekRaw> _weeks = new List<EPWeekRaw>(weekListCount);

			static FullWeekList()
			{
				DateTime curentDay = _stratDay;
				for (int i = 0; i < weekListCount; i++)
				{
					EPWeek epWeek = new EPWeek { WeekID = curentDay.Year * 100 + PX.Data.Maintenance.PXDateTimeInfo.GetWeekNumber(curentDay) };
					EPWeekRaw rawWeek = EPWeekRaw.ToEPWeekRaw(epWeek);
					_weeks.Add(rawWeek);
					curentDay = curentDay.AddDays(7);
				}
			}

			public static List<EPWeekRaw> Weeks
			{
				get { return _weeks; }
			}

		}

		private bool isCustomWeek = false;

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			EPSetup setup = PXSetup<EPSetup>.SelectSingleBound(_Graph, null);
			if (setup != null && setup.CustomWeek == true)
				isCustomWeek = true;
		}

		public PXWeekSelector2Attribute() : base(typeof (EPWeekRaw.weekID),
		                                         new Type[]
													{
														typeof (EPWeekRaw.fullNumber),
														typeof (EPWeekRaw.startDate),
														typeof (EPWeekRaw.endDate)
													}
												)
		{
		}

		protected override IEnumerable GetAllRecords()
		{
			if (isCustomWeek)
			{
				var res = (PXResultset<EPCustomWeek>) PXSelect<EPCustomWeek>.Select(_Graph, null);
				return res.Select(_ => EPWeekRaw.ToEPWeekRaw(_)).ToList();
			}
			else
				return FullWeekList.Weeks;
		}

		public static bool IsCustomWeek(PXGraph graph)
		{
			EPSetup setup = PXSetup<EPSetup>.SelectSingleBound(graph, null);
			return setup != null && setup.CustomWeek == true;
		}

		private static EPCustomWeek GetCustomWeek(PXGraph graph, DateTime date)
		{
			EPCustomWeek customWeek = PXSelect<EPCustomWeek, Where<Required<EPCustomWeek.startDate>, Between<EPCustomWeek.startDate, EPCustomWeek.endDate>>>.SelectSingleBound(graph, null, date);
			if (customWeek == null)
				throw new PXException(Messages.CustomWeekNotFoundByDate, date);
			return customWeek;
		}

		private static EPCustomWeek GetCustomWeek(PXGraph graph, int weekID)
		{
			EPCustomWeek customWeek = PXSelect<EPCustomWeek, Where<EPCustomWeek.weekID, Equal<Required<EPCustomWeek.weekID>>>>.SelectSingleBound(graph, null, weekID);
			if (customWeek == null)
				throw new PXException(Messages.CustomWeekNotFound);
			return customWeek;
		}

        private static EPWeekRaw GetStandartWeek(PXGraph graph, DateTime date)
        {
            EPWeek epWeek = new EPWeek { WeekID = date.Year * 100 + PX.Data.Maintenance.PXDateTimeInfo.GetWeekNumber(date) };
            return EPWeekRaw.ToEPWeekRaw(epWeek);
        }

		public static int GetWeekID(PXGraph graph, DateTime date)
		{
			if (IsCustomWeek(graph))
				return GetCustomWeek(graph, date).WeekID.Value;
			else
				return PXWeekSelectorAttribute.GetWeekID(date);
		}

		public static int GetNextWeekID(PXGraph graph, int weekID)
		{
			if (IsCustomWeek(graph))
				return GetWeekID(graph, GetCustomWeek(graph, weekID).EndDate.Value.AddDays(1d));
			else
				return PXWeekSelectorAttribute.GetWeekID(PXWeekSelectorAttribute.GetWeekSrartDate(weekID).AddDays(7d));
		}

		public static int GetNextWeekID(PXGraph graph, DateTime date)
		{
			if (IsCustomWeek(graph))
				return GetWeekID(graph, GetCustomWeek(graph, date).EndDate.Value.AddDays(1d));
			else
				return PXWeekSelectorAttribute.GetWeekID(date.AddDays(7));
		}

		public static DateTime GetWeekSrartDate(PXGraph graph, int weekId)
		{
			if (IsCustomWeek(graph))
				return GetCustomWeek(graph, weekId).StartDate.Value;
			else
				return PXWeekSelectorAttribute.GetWeekSrartDate(weekId);
		}

        public static DateTime GetWeekEndDate(PXGraph graph, int weekId)
        {
            if (IsCustomWeek(graph))
                return GetCustomWeek(graph, weekId).EndDate.Value;
            else
                return PXWeekSelectorAttribute.GetWeekSrartDate(weekId).AddDays(6d);
        }


        public class WeekInfo
        {
            private Dictionary<DayOfWeek, DayInfo> _days = new Dictionary<DayOfWeek, DayInfo>();

            public DayInfo Mon { get { return GetDayInfo(DayOfWeek.Monday); } }
            public DayInfo Tue { get { return GetDayInfo(DayOfWeek.Tuesday); } }
            public DayInfo Wed { get { return GetDayInfo(DayOfWeek.Wednesday); } }
            public DayInfo Thu { get { return GetDayInfo(DayOfWeek.Thursday); } }
            public DayInfo Fri { get { return GetDayInfo(DayOfWeek.Friday); } }
            public DayInfo Sat { get { return GetDayInfo(DayOfWeek.Saturday); } }
            public DayInfo Sun { get { return GetDayInfo(DayOfWeek.Sunday); } }

            private DayInfo GetDayInfo(DayOfWeek mDay)
            {
                if (_days.ContainsKey(mDay))
                    return _days[mDay];
                else
                    return new DayInfo(null);
            }

            public void AddDayInfo(DateTime date)
            {
                _days[date.DayOfWeek] = new DayInfo(date);
            }

            public bool IsValid(DateTime date)
            {
                foreach (DayInfo info in _days.Values)
                {
                    if (info.Enabled && info.Date.Value.Date == date.Date.Date)
                        return true;
                }

                return false;
            }
        }

        public class DayInfo
        {
            public DayInfo(DateTime? date)
            {
                _date = date;
            }

            private DateTime? _date;
            public DateTime? Date { get { return _date; } }
            public bool Enabled { get { return (_date != null); } }
        }

        public static WeekInfo GetWeekInfo(PXGraph graph, int weekId)
        {
            WeekInfo ret = new WeekInfo();
            for (DateTime date = GetWeekSrartDate(graph, weekId); date <= GetWeekEndDate(graph, weekId); date = date.AddDays(1))
            {
                ret.AddDayInfo(date);
            }
            return ret;
        }

	}

	#endregion

	#region DefaultWeekAttribute
	public class PXDefaultWeekAttribute : PXDefaultAttribute
	{
		protected readonly Type _DateField;
		protected PXGraph _graph;

		public PXDefaultWeekAttribute(Type DateField)
		{
			_DateField = DateField;
			this.PersistingCheck = PXPersistingCheck.Nothing;
		}

		public override void CacheAttached(PXCache sender)
		{
			_graph = sender.Graph;
		}

		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			DateTime? dateField = (DateTime?)sender.GetValue(e.Row, _DateField.Name);
			if (dateField != null)
				e.NewValue = PXWeekSelector2Attribute.GetWeekID(_graph, (DateTime)dateField);
		}
	}
	#endregion


	#region EPActivityProjectDefaultAttribute
	public class EPActivityProjectDefaultAttribute : PM.ProjectDefaultAttribute
	{
		public EPActivityProjectDefaultAttribute()
			: base(BatchModule.EP, typeof(Search<PM.PMProject.contractID, Where<PM.PMProject.nonProject, Equal<True>>>))
		{
		}

		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as EPActivity;
			if (row == null || row.IsBillable != true || !PM.ProjectAttribute.IsPMVisible(sender.Graph, BatchModule.EP)) 
				base.FieldDefaulting(sender, e);
		}

	}
	#endregion

	#region EPActiveProjectAttribute
	[PXDBInt()]
	[PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible)]
	public class EPActiveProjectAttribute : AcctSubAttribute
	{
		public EPActiveProjectAttribute():this(false)
		{
		}

		public EPActiveProjectAttribute(bool ignoreRestrictionList)
		{
			Type searchType;
			if (ignoreRestrictionList)
			{
				searchType = typeof(Search<PMProject.contractID,
				Where<PMProject.isTemplate, Equal<False>,
				And<PMProject.isActive, Equal<True>,
				And<PMProject.baseType, Equal<PMProject.ProjectBaseType>,
				And<Where<
					Current<EPActivity.customerID>, IsNull,
					Or<PMProject.customerID, Equal<Current<EPActivity.customerID>>,
					And<PMProject.locationID, Equal<Current<EPActivity.locationID>>>>>>>>>,
				OrderBy<Desc<PMProject.contractCD>>>);
			}
			else
			{
				searchType = typeof(Search2<PMProject.contractID,
				LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<Current<EPActivity.owner>>>,
				LeftJoin<EPEmployeeContract, On<EPEmployeeContract.contractID, Equal<PMProject.contractID>, And<EPEmployeeContract.employeeID, Equal<EPEmployee.bAccountID>>>>>,
				Where<PMProject.isTemplate, Equal<False>,
				And<PMProject.isActive, Equal<True>,
				And<PMProject.baseType, Equal<PMProject.ProjectBaseType>,
				And2<Where<PMProject.restrictToEmployeeList, Equal<False>, Or<EPEmployeeContract.employeeID, IsNotNull>>,
				And<Where<
					Current<EPActivity.customerID>, IsNull,
					Or<PMProject.customerID, Equal<Current<EPActivity.customerID>>,
					And<PMProject.locationID, Equal<Current<EPActivity.locationID>>>>>>>>>>,
				OrderBy<Desc<PMProject.contractCD>>>);
			}

			PXDimensionSelectorAttribute select = new PXDimensionSelectorAttribute(ProjectAttribute.DimensionName, searchType, typeof(PMProject.contractCD),
			typeof(PMProject.contractCD), typeof(PMProject.description), typeof(PMProject.status));
			select.DescriptionField = typeof(PMProject.description);
			select.ValidComboRequired = true;
			select.CacheGlobal = true;

			_Attributes.Add(select);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
            /*Visible =*/
            Enabled = ProjectAttribute.IsPMVisible(sender.Graph, BatchModule.EP);
		}
	}

	#endregion   

	#region RateTypesAttribute
	public class RateTypesAttribute : PXStringListAttribute
	{
		public const string Hourly = "H";
		public const string Salary = "S";
		public const string SalaryWithExemption = "E";

		public RateTypesAttribute()
			: base(
			new[] { Hourly, Salary, SalaryWithExemption },
			new[] { Messages.Hourly, Messages.Salary, Messages.SalaryWithExemption }) { }

		public class hourly : Constant<string>
		{
			public hourly() : base(Hourly) { }
		}

		public class salary : Constant<string>
		{
			public salary() : base(Salary) { }
		}

		public class salaryWithExemption : Constant<string>
		{
			public salaryWithExemption() : base(SalaryWithExemption) { }
		}
	}
	#endregion

	#region FilterHeaderDescriptionAttribute

	public sealed class FilterHeaderDescriptionAttribute : PXDACDescriptionAttribute
	{
		public FilterHeaderDescriptionAttribute()
			: base(typeof(FilterHeader), new PXPrimaryGraphAttribute(typeof(PX.Objects.CS.CSFilterMaint)))
		{ }
	}

	#endregion

    #region PXDBTimeSpanAttribute
    public class PXDBTimeSpan2Attribute : PXDBIntAttribute
    {
        #region State
        protected string _InputMask = "HH:mm";
        protected string _DisplayMask = "HH:mm";
        protected new DateTime? _MinValue;
        protected new DateTime? _MaxValue;

        public string InputMask
        {
            get
            {
                return _InputMask;
            }
            set
            {
                _InputMask = value;
            }
        }
        public string DisplayMask
        {
            get
            {
                return _DisplayMask;
            }
            set
            {
                _DisplayMask = value;
            }
        }
        public new string MinValue
        {
            get
            {
                if (_MinValue != null)
                {
                    return _MinValue.ToString();
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    _MinValue = DateTime.Parse(value);
                }
                else
                {
                    _MinValue = null;
                }
            }
        }
        public new string MaxValue
        {
            get
            {
                if (_MaxValue != null)
                {
                    return _MaxValue.ToString();
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    _MaxValue = DateTime.Parse(value);
                }
                else
                {
                    _MaxValue = null;
                }
            }
        }

        public const string Zero = "00:00";
        public sealed class zero : Constant<string> { public zero() : base(Zero) { } }
        #endregion

        #region Ctor
        public PXDBTimeSpan2Attribute()
        {
        }
        #endregion

        #region Initialization
        #endregion

        #region Implementation
        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
            {
                e.ReturnState = PXDateState.CreateInstance(e.ReturnState, _FieldName, _IsKey, null, _InputMask, _DisplayMask, _MinValue, _MaxValue);
            }

            if (e.ReturnValue != null && (e.ReturnValue is int || e.ReturnValue is int?))
            {
                int minutes = (int) e.ReturnValue;

                TimeSpan span = new TimeSpan(0, 0, Math.Abs(minutes), 0);

                //if (minutes < 0)
                //{
                //    e.ReturnValue 
                //}
                //else
                //{
                    
                //}
                e.ReturnValue = new DateTime(1900, 1, 1).Add(span);
                //e.ReturnValue = new DateTime(1900, 1, 1, span.Hours, span.Minutes, 0);
            }
        }

        public override void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (e.NewValue == null || e.NewValue is int)
            {
            }
            else if (e.NewValue is string)
            {
                DateTime val;
                if (DateTime.TryParse((string)e.NewValue, sender.Graph.Culture, DateTimeStyles.None, out val))
                {
                    TimeSpan span = new TimeSpan(val.Hour, val.Minute, 0);
                    e.NewValue = (int)span.TotalMinutes;
                }
                else
                {
                    e.NewValue = null;
                }
            }
            else if (e.NewValue is DateTime)
            {
                DateTime val = (DateTime)e.NewValue;
                TimeSpan span = new TimeSpan(val.Hour, val.Minute, 0);
                e.NewValue = (int)span.TotalMinutes;
            }
        }
        #endregion

        public static DateTime FromMinutes(int minutes)
        {
            TimeSpan span = new TimeSpan(0, 0, minutes, 0);
            return new DateTime(1900, 1, 1).Add(span);
        }
    }
    #endregion

    public class PXTimeListAttribute : PXIntListAttribute
    {
        public PXTimeListAttribute()
			: base(GetValues(30, 47), GetLabels(GetValues(30, 47)))
        {   
        }

		public PXTimeListAttribute(int step, int count)
			: base(GetValues(step, count), GetLabels(GetValues(step, count)))
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, FieldUpdating);
		}

		protected void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (!sender.Graph.IsImport || e.NewValue is Int32 || String.IsNullOrEmpty((String)e.NewValue))
				return;
			String value = e.NewValue as String;
			if (value == null)
				return;
			Regex minuteRegex = new Regex("^[0-9]+$", RegexOptions.Singleline | RegexOptions.Compiled);
			if (minuteRegex.IsMatch(value))
				return;
			Regex timeRegex = new Regex("^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$", RegexOptions.Singleline | RegexOptions.Compiled);
			if (!timeRegex.IsMatch(value))
				throw new PXException(Messages.UnableConvertToTime, e.NewValue);
			DateTime time = DateTime.ParseExact(value, "hh:mm", CultureInfo.InvariantCulture);
			e.NewValue = time.Hour * 60 + time.Minute;
		}

	    public static int[] GetValues(int step, int count)
		{
			List<int> list = new List<int>(100);
			for (int i = 0; i <= count; i++)
			{
				list.Add(i * step);
			}

			return list.ToArray();
		}

        public static string[] GetLabels(int[] values)
        {
            List<string> list = new List<string>();

            foreach (int val in values)
            {
                list.Add(GetString(val));
            }

            return list.ToArray();
        }

        public static string GetString(int totalMinutes)
        {
            TimeSpan ts = TimeSpan.FromMinutes(Math.Abs(totalMinutes));

            if (totalMinutes < 0)
            {
                return string.Format("-{0:hh}:{0:mm}", ts);
            }
            else
            {
                return string.Format("{0:hh}:{0:mm}", ts);
            }
        }
        
    }

	/// <summary>
	/// Time is displayed and modified in the timezone of the user/Employee.
	/// </summary>
	public class EPDBDateAndTimeAttribute : PXDBDateAndTimeAttribute
	{
		protected Type typeUserID;
		protected PXTimeZoneInfo timezone;
		
		public EPDBDateAndTimeAttribute(Type userID)
		{
			this.typeUserID = userID;
		}
		
		public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			InitTimeZone(sender, e.Row);

			base.CommandPreparing(sender, e);
		}
		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			using (new PXConnectionScope())
			{
				InitTimeZone(sender, e.Row);
			}

			Debug.Print("{0}", GetTimeZone().DisplayName);
			base.RowSelecting(sender, e);
		}

		protected virtual void InitTimeZone(PXCache sender, object row)
		{
			if (row == null) return;

			Guid? userID = (Guid?)sender.GetValue(row, sender.GetField(typeUserID));
			if (userID != null)
			{
				UserPreferences pref = PXSelect<UserPreferences, Where<UserPreferences.userID, Equal<Required<UserPreferences.userID>>>>.Select(sender.Graph, userID);
				if (pref != null && !string.IsNullOrEmpty(pref.TimeZone))
				{
					timezone = PXTimeZoneInfo.FindSystemTimeZoneById(pref.TimeZone);
					return;
				}
				EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.Select(sender.Graph, userID);
				if (employee != null && employee.CalendarID != null)
				{
					CSCalendar cal = PXSelect<CSCalendar, Where<CSCalendar.calendarID, Equal<Required<CSCalendar.calendarID>>>>.Select(sender.Graph, employee.CalendarID);
					if (cal != null && !string.IsNullOrEmpty(cal.TimeZone))
					{
						timezone = PXTimeZoneInfo.FindSystemTimeZoneById(cal.TimeZone);
					}
				}
			}
		}

		protected override PXTimeZoneInfo GetTimeZone()
		{
			if (timezone != null)
				return timezone;

			return base.GetTimeZone();
		}
	}
}
