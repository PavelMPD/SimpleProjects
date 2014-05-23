using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.TX;
using PX.Objects.AR;
using PX.Objects.EP;
using PX.TM;
using PX.Common;

namespace PX.Objects.CR
{
	#region BAccountAttribute

	[PXDBInt()]
	[PXUIField(DisplayName = "Business Account")]
	[PXDimensionSelector(DimensionName, typeof(Search2<BAccount.bAccountID,
						LeftJoin<Contact,
							On<Contact.bAccountID, Equal<BAccount.bAccountID>,
								And<Contact.contactID, Equal<BAccount.defContactID>>>,
						LeftJoin<Address,
							On<Address.bAccountID, Equal<BAccount.bAccountID>,
								And<Address.addressID, Equal<BAccount.defAddressID>>>>>,
						Where<Match<Current<AccessInfo.userName>>>>), typeof(BAccount.acctCD),
																 typeof(BAccount.acctCD),
																 typeof(BAccount.acctName),
																 typeof(BAccount.type),
																 typeof(BAccount.classID),
																 typeof(BAccount.status),
																 typeof(Contact.phone1),
																 typeof(Address.city),
																 typeof(Address.countryID),
																 typeof(Contact.eMail))]
	public class BAccountAttribute : AcctSubAttribute
	{
		public const string DimensionName = "BIZACCT";

		public BAccountAttribute()
		{
			this.DescriptionField = typeof (BAccount.acctName);
			Initialize();
		}
	}
	#endregion

	#region CustomerAndProspectAttribute

	[PXRestrictor(typeof(Where<BAccount.type, Equal<BAccountType.prospectType>,
			Or<BAccount.type, Equal<BAccountType.customerType>,
			Or<BAccount.type, Equal<BAccountType.combinedType>>>>), Messages.BAccountIsType, typeof(BAccount.type))]	
	public class CustomerAndProspectAttribute : BAccountAttribute
	{ 
		public CustomerAndProspectAttribute()
			: base()
		{
			this.Filterable = true;
			this.DisplayName = "Customer";
		}

	}

	#endregion

	#region CustomerProspectVendorAttribute

	[PXRestrictor(typeof(Where<BAccount.type, Equal<BAccountType.prospectType>,
			Or<BAccount.type, Equal<BAccountType.customerType>,
			Or<BAccount.type, Equal<BAccountType.vendorType>,
			Or<BAccount.type, Equal<BAccountType.combinedType>>>>>), Messages.BAccountIsType, typeof(BAccount.type))]
	public class CustomerProspectVendorAttribute : BAccountAttribute 
	{ 
		public CustomerProspectVendorAttribute()
			: base()
		{
			this.Filterable = true;
		}
	}

	#endregion

	#region AddressRevisionIDAttribute

	public class AddressRevisionIDAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber
	{
		public virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				int? revision = (int?)sender.GetValue(e.Row, _FieldOrdinal) ?? 0;
				revision++;
				sender.SetValue(e.Row, _FieldOrdinal, revision);
			}
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Aborted && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update))
			{
				int? revision = (int?)sender.GetValue(e.Row, _FieldOrdinal);
				revision--;
				sender.SetValue(e.Row, _FieldOrdinal, revision);
			}
		}
	}

	#endregion

	#region StateAttribute

	public class StateAttribute : PXAggregateAttribute, IPXFieldVerifyingSubscriber
	{
		#region Protected Members

		protected int _SelAttrIndex = -1;
		protected Type _CountryID;

		#endregion

		#region Ctor+Overrides

		public StateAttribute(Type aCountryID)
		{
			_CountryID = aCountryID;
			Filterable = true;

			Type SearchType =
				BqlCommand.Compose(
					typeof(Search<,>),
					typeof(State.stateID),
					typeof(Where<,>),
					typeof(State.countryID),
					typeof(Equal<>),
					typeof(Optional<>),
					_CountryID);

			_Attributes.Add(new PXSelectorAttribute(SearchType));
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			base.GetSubscriber(subscribers);
			if (subscribers.Count > 0)
			{
				if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber) && (_SelAttrIndex >= 0))
				{
					subscribers.Remove(_Attributes[_SelAttrIndex] as ISubscriber);
				}
			}
		}

		#endregion

		#region Property Accessors

		public virtual bool Filterable
		{
			get { return (_SelAttrIndex == -1) ? false : ((PXSelectorAttribute)_Attributes[_SelAttrIndex]).Filterable; }
			set
			{
				if (_SelAttrIndex != -1)
					((PXSelectorAttribute)_Attributes[_SelAttrIndex]).Filterable = value;
			}
		}

		public virtual Type DescriptionField
		{
			get { return (_SelAttrIndex == -1) ? null : ((PXSelectorAttribute)_Attributes[_SelAttrIndex]).DescriptionField; }
			set
			{
				if (_SelAttrIndex != -1)
					((PXSelectorAttribute)_Attributes[_SelAttrIndex]).DescriptionField = value;
			}
		}

		public virtual bool CacheGlobal
		{
			get { return (_SelAttrIndex == -1) ? false : ((PXSelectorAttribute)_Attributes[_SelAttrIndex]).CacheGlobal; }
			set
			{
				if (_SelAttrIndex != -1)
					((PXSelectorAttribute)_Attributes[_SelAttrIndex]).CacheGlobal = value;
			}
		}

		#endregion

		#region Events

		void IPXFieldVerifyingSubscriber.FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			Country def = FindCountry(sender, e.Row);
			if (def == null || (bool)def.AllowStateEdit)
			{
				e.Cancel = true;
			}
			if (!e.Cancel)
			{
				((PXSelectorAttribute)_Attributes[_SelAttrIndex]).FieldVerifying(sender, e);
			}
		}

		#endregion

		#region Utility

		protected virtual Country FindCountry(PXCache sender, Object row)
		{
			object countryID = sender.GetValue(row, _CountryID.Name);
			if (countryID != null)
			{
				Country result = PXSelect<Country, Where<Country.countryID, Equal<Required<Country.countryID>>>>.Select(
					sender.Graph, countryID);
				return result;
			}
			return null;
		}

		#endregion
	}

	#endregion

	#region CR Calculation Attributes

	/// <summary>
	/// Base attribute class for dinamic calculation values of DAC fields.
	/// </summary>
	public abstract class CRCalculationAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		#region Fields

		private readonly BqlCommand _select;

		#endregion

		protected CRCalculationAttribute(Type valueSelect)
		{
			if (valueSelect == null)
			{
				throw new PXArgumentException(ErrorMessages.ArgumentNullException, "valueSelect");
			}
			_select = BqlCommand.CreateInstance(valueSelect);
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				PXView view = sender.Graph.TypedViews.GetView(_select, true);

				object value = CalculateValue(view, e.Row);

				sender.SetValue(e.Row, base._FieldName, value);

				var state = PXFieldState.CreateInstance(e.ReturnState, null, false, null,
														-1, null, null, value, base._FieldName, null, null, null,
														PXErrorLevel.Undefined, false, null, null, PXUIVisibility.Undefined, null, null,
														null);
				state.Value = value;
				e.ReturnState = state;
			}
		}

		protected abstract object CalculateValue(PXView view, object row);
	}

	/// <summary>
	/// Dinamicaly calculates count of rows returned by given Bql command.
	/// </summary>
	public class CRCountCalculationAttribute : CRCalculationAttribute
	{
		public CRCountCalculationAttribute(Type valueSelect)
			: base(valueSelect)
		{
		}

		protected override object CalculateValue(PXView view, object row)
		{
			PXResult result = view.SelectSingle() as PXResult;
			return result == null ? 0 : result.RowCount;
		}
	}

	/// <summary>
	/// Dinamicaly calculates field summ of rows returned by given Bql command.
	/// </summary>
	public class CRSummCalculationAttribute : CRCalculationAttribute
	{
		private readonly Type _summField;

		public CRSummCalculationAttribute(Type valueSelect, Type summField)
			: base(valueSelect)
		{
			_summField = summField;
		}

		protected override object CalculateValue(PXView view, object row)
		{
			return view.Cache.GetValue(view.SelectSingle(), _summField.Name);
		}
	}

	#endregion

	#region CRTaxAttribute

	public class CRTaxAttribute : TaxAttribute
	{
		public CRTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			CuryDocBal = typeof(CROpportunity.curyProductsAmount);
			CuryLineTotal = typeof(CROpportunity.curyLineTotal);
			DocDate = typeof(CROpportunity.closeDate);
			CuryTranAmt = typeof(CROpportunityProducts.curyAmount);
		}

		protected override void CalcDocTotals(PXCache sender, object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
		{
			base.CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);

			decimal CuryLineTotal = (decimal)(ParentGetValue<CROpportunity.curyLineTotal>(sender.Graph) ?? 0m);
			decimal CuryDiscountTotal = (decimal)(ParentGetValue<CROpportunity.curyDiscTot>(sender.Graph) ?? 0m);

			decimal CuryDocTotal = CuryLineTotal - CuryDiscountTotal;

			if (object.Equals(CuryDocTotal, (decimal)(ParentGetValue<CROpportunity.curyProductsAmount>(sender.Graph) ?? 0m)) == false)
			{
				ParentSetValue<CROpportunity.curyProductsAmount>(sender.Graph, CuryDocTotal);
			}
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
			object[] currents = new object[] { row, ((OpportunityMaint)graph).Opportunity.Current };
			foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
					And<TaxRev.outdated, Equal<boolFalse>,
						And<TaxRev.taxType, Equal<TaxType.sales>,
						And<Tax.taxType, NotEqual<CSTaxType.withholding>,
						And<Tax.taxType, NotEqual<CSTaxType.use>,
						And<Tax.reverseTax, Equal<boolFalse>,
						And<Current<CROpportunity.closeDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
				Where>
				.SelectMultiBound(graph, currents, parameters))
			{
				tail[((Tax)record).TaxID] = record;
			}
			List<object> ret = new List<object>();
			switch (taxchk)
			{
				case PXTaxCheck.Line:
					foreach (CROpportunityTax record in PXSelect<CROpportunityTax,
						Where<CROpportunityTax.opportunityID, Equal<Current<CROpportunity.opportunityID>>,
							And<CROpportunityTax.opportunityProductID, Equal<Current<CROpportunityProducts.cROpportunityProductID>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<CROpportunityTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcLine:
					foreach (CROpportunityTax record in PXSelect<CROpportunityTax,
						Where<CROpportunityTax.opportunityID, Equal<Current<CROpportunity.opportunityID>>,
							And<CROpportunityTax.opportunityProductID, Less<intMax>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& ((CROpportunityTax)(PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1]).OpportunityProductID == record.OpportunityProductID
								&& String.Compare(((Tax)(PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<CROpportunityTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcTotals:
					foreach (CRTaxTran record in PXSelect<CRTaxTran,
						Where<CRTaxTran.opportunityID, Equal<Current<CROpportunity.opportunityID>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (record.TaxID != null && tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<CRTaxTran, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<CRTaxTran, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				default:
					return ret;
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			if (sender.Graph is OpportunityMaint)
			{
				base.CacheAttached(sender);
			}
			else
			{
				TaxCalc = TaxCalc.NoCalc;
			}
		}
	}

	#endregion

	#region CRServiceCaseTaxAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class CRServiceCaseTaxAttribute : TaxAttribute
	{
		public CRServiceCaseTaxAttribute()
			: base(typeof(CRServiceCase), typeof(CRServiceCaseTax), typeof(CRServiceCaseTaxTran))
		{
			DocDate = typeof(CRServiceCase.createdDateTime);
			CuryDocBal = typeof(CRServiceCase.actualCuryItemAmount);
			CuryLineTotal = typeof(CRServiceCase.actualCuryLineTotal);
			CuryTranAmt = typeof(CRServiceCaseItem.actualCuryAmount);
			CuryTaxTotal = typeof(CRServiceCase.actualCuryTaxTotal);
			TaxCalc = TX.TaxCalc.ManualLineCalc;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
			object[] currents = new object[] { row, graph.Caches[typeof(CRServiceCase)].Current };
			foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
					And<TaxRev.outdated, Equal<False>,
						And<TaxRev.taxType, Equal<TaxType.sales>,
						And<Tax.taxType, NotEqual<CSTaxType.withholding>,
						And<Tax.taxType, NotEqual<CSTaxType.use>,
						And<Tax.reverseTax, Equal<boolFalse>,
						And<Current<CRServiceCase.createdDateTime>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
				Where>
				.SelectMultiBound(graph, currents, parameters))
			{
				tail[((Tax)record).TaxID] = record;
			}
			List<object> ret = new List<object>();
			switch (taxchk)
			{
				case PXTaxCheck.Line:
					foreach (CRServiceCaseTax record in PXSelect<CRServiceCaseTax,
						Where<CRServiceCaseTax.serviceCaseID, Equal<Current<CRServiceCase.serviceCaseID>>,
							And<CRServiceCaseTax.lineNbr, Equal<Current<CRServiceCaseItem.lineNbr>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<CRServiceCaseTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<CRServiceCaseTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcLine:
					foreach (CRServiceCaseTax record in PXSelect<CRServiceCaseTax,
						Where<CRServiceCaseTax.serviceCaseID, Equal<Current<CRServiceCase.serviceCaseID>>,
							And<CRServiceCaseTax.lineNbr, Less<intMax>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& ((CRServiceCaseTax)(PXResult<CRServiceCaseTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
								&& String.Compare(((Tax)(PXResult<CRServiceCaseTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<CRServiceCaseTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcTotals:
					foreach (CRServiceCaseTaxTran record in PXSelect<CRServiceCaseTaxTran,
						Where<CRServiceCaseTaxTran.serviceCaseID, Equal<Current<CRServiceCase.serviceCaseID>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (record.TaxID != null && tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<CRServiceCaseTaxTran, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<CRServiceCaseTaxTran, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				default:
					return ret;
			}
		}
	}

	#endregion

	#region CRNowDefaultAttribute

	/// <summary>
	/// Set DateTime.Now as default value
	/// </summary>
	public class CRNowDefaultAttribute : PXDefaultAttribute
	{
		public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			base.FieldDefaulting(sender, e);
			e.NewValue = PXTimeZoneInfo.Now;
		}
	}

	#endregion

	#region OperationParam

	[Serializable]
	public partial class OperationParam : IBqlTable
	{
		public abstract class action : IBqlField
		{
		}
		protected string _Action;

		[PXStringList("")]
		public string Action
		{
			get
			{
				return _Action;
			}
			set
			{
				_Action = value;
			}
		}

		public abstract class assignmentMapID : IBqlField { }

		[PXInt]
		[PXUIField(DisplayName = "Assignment Map")]
		public virtual Int32? AssignmentMapID { get; set; }

	}

	#endregion

	#region OwnedFilter
	[Serializable]
	public partial class OwnedFilter : IBqlTable, PX.TM.IOwnedFilter
	{
		#region CurrentOwnerID
		public abstract class currentOwnerID : PX.Data.IBqlField
		{
		}

		[PXDBGuid]
		[CRCurrentOwnerID]
		public virtual Guid? CurrentOwnerID { get; set; }
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.IBqlField
		{
		}
		protected Guid? _OwnerID;
		[PXDBGuid]
		[PXUIField(DisplayName = "Owner")]
		[PX.TM.PXSubordinateOwnerSelector]
		public virtual Guid? OwnerID
		{
			get
			{
				return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
			}
			set
			{
				_OwnerID = value;
			}
		}
		#endregion
		#region MyOwner
		public abstract class myOwner : PX.Data.IBqlField
		{
		}
		protected Boolean? _MyOwner;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Me")]
		public virtual Boolean? MyOwner
		{
			get
			{
				return _MyOwner;
			}
			set
			{
				_MyOwner = value;
			}
		}
		#endregion
		#region WorkGroupID
		public abstract class workGroupID : PX.Data.IBqlField
		{
		}
		protected Int32? _WorkGroupID;
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>),
		 SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual Int32? WorkGroupID
		{
			get
			{
				return (_MyWorkGroup == true) ? null : _WorkGroupID;
			}
			set
			{
				_WorkGroupID = value;
			}
		}
		#endregion
		#region MyWorkGroup
		public abstract class myWorkGroup : PX.Data.IBqlField
		{
		}
		protected Boolean? _MyWorkGroup;
		[PXDefault(false)]
		[PXDBBool]
		[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyWorkGroup
		{
			get
			{
				return _MyWorkGroup;
			}
			set
			{
				_MyWorkGroup = value;
			}
		}
		#endregion		
		#region FilterSet
		public abstract class filterSet : PX.Data.IBqlField
		{
		}
		[PXDefault(false)]
		[PXDBBool]
        public virtual Boolean? FilterSet
		{
			get
			{
				return 
					this.OwnerID != null ||
					this.WorkGroupID != null || 
					this.MyWorkGroup == true;
			}
		}
		#endregion
	}
	#endregion

	#region SubordinateOwnedFilter
	[Serializable]
	public partial class SubordinateOwnedFilter : PX.TM.OwnedFilter
	{
		#region CurrentOwnerID
		public new abstract class currentOwnerID : PX.Data.IBqlField
		{
		}
		#endregion
		#region OwnerID
		public new abstract class ownerID : PX.Data.IBqlField
		{
		}
		[PXDBGuid]
		[PXUIField(DisplayName = "Assigned to")]
		[PX.TM.PXSubordinateOwnerSelector]
		public override Guid? OwnerID
		{
			get
			{
				return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
			}
			set
			{
				_OwnerID = value;
			}
		}
		#endregion
		#region MyOwner
		public new abstract class myOwner : PX.Data.IBqlField
		{
		}
		#endregion
		#region WorkGroupID
		public new abstract class workGroupID : PX.Data.IBqlField
		{
		}		
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PX.TM.PXSubordinateGroupSelector]
		public override Int32? WorkGroupID
		{
			get
			{
				return (_MyWorkGroup == true) ? null : _WorkGroupID;
			}
			set
			{
				_WorkGroupID = value;
			}
		}
		#endregion
		#region WorkGroup
		public new abstract class myWorkGroup : PX.Data.IBqlField
		{
		}
		#endregion
	}
	#endregion

	#region OwnedEscalatedFilter
	[Serializable]
	public partial class OwnedEscalatedFilter : IBqlTable
	{
		#region CurrentOwnerID
		public abstract class currentOwnerID : PX.Data.IBqlField
		{
		}

		[PXDBGuid]
		[CRCurrentOwnerID]
		public virtual Guid? CurrentOwnerID { get; set; }
		#endregion
		#region OwnerID
		public abstract class ownerID : PX.Data.IBqlField
		{
		}
		protected Guid? _OwnerID;
		[PXDBGuid]
		[PXUIField(DisplayName = "Assigned To")]
		[PX.TM.PXSubordinateOwnerSelector]
		public virtual Guid? OwnerID
		{
			get
			{
				return (_MyOwner == true) ? CurrentOwnerID : _OwnerID;
			}
			set
			{
				_OwnerID = value;
			}
		}
		#endregion
		#region MyOwner
		public abstract class myOwner : PX.Data.IBqlField
		{
		}
		protected Boolean? _MyOwner;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Me")]
		public virtual Boolean? MyOwner
		{
			get
			{
				return _MyOwner;
			}
			set
			{
				_MyOwner = value;
			}
		}
		#endregion
		#region WorkGroupID
		public abstract class workGroupID : PX.Data.IBqlField
		{
		}
		protected Int32? _WorkGroupID;
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXSelector(typeof(Search<EPCompanyTree.workGroupID,
			Where<EPCompanyTree.workGroupID, Owned<Current<AccessInfo.userID>>>>),
		 SubstituteKey = typeof(EPCompanyTree.description))]
		public virtual Int32? WorkGroupID
		{
			get
			{
				return (_MyWorkGroup == true) ? null : _WorkGroupID;
			}
			set
			{
				_WorkGroupID = value;
			}
		}
		#endregion
		#region MyWorkGroup
		public abstract class myWorkGroup : PX.Data.IBqlField
		{
		}
		protected Boolean? _MyWorkGroup;
		[PXDefault(false)]
		[PXDBBool]
		[PXUIField(DisplayName = "My", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyWorkGroup
		{
			get
			{
				return _MyWorkGroup;
			}
			set
			{
				_MyWorkGroup = value;
			}
		}
		#endregion		
		#region MyEscalated
		public abstract class myEscalated : PX.Data.IBqlField
		{
		}
		protected Boolean? _MyEscalated;
		[PXDefault(true)]
		[PXDBBool]
		[PXUIField(DisplayName = "Display Escalated", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? MyEscalated
		{
			get
			{
				return _MyEscalated;
			}
			set
			{
				_MyEscalated = value;
			}
		}
		#endregion
		#region FilterSet
		public abstract class filterSet : PX.Data.IBqlField
		{
		}
		[PXDefault(false)]
		[PXDBBool]
        public virtual Boolean? FilterSet
		{
			get
			{
				return
					this.OwnerID != null ||
					this.WorkGroupID != null ||
					this.MyWorkGroup == true ||
					this.MyEscalated == true;
			}
		}
		#endregion
	}
	#endregion

	#region ContactDisplayNameAttribute

	public class ContactDisplayNameAttribute : PXDBStringAttribute, IPXRowUpdatedSubscriber, IPXRowInsertedSubscriber/*, IPXRowSelectedSubscriber*/
	{
		private readonly Type _lastNameBqlField;
		private readonly Type _firstNameBqlField;
		private readonly Type _midNameBqlField;
		private readonly Type _titleBqlField;
		private readonly bool _reversed;

		private int _lastNameFieldOrdinal;
		private int _firstNameFieldOrdinal;
		private int _midNameFieldOrdinal;
		private int _titleFieldOrdinal;

		public ContactDisplayNameAttribute(Type lastNameBqlField, Type firstNameBqlField,
			Type midNameBqlField, Type titleBqlField, bool reversed)
			: base(255)
		{
			if (lastNameBqlField == null) throw new ArgumentNullException("lastNameBqlField");
			if (firstNameBqlField == null) throw new ArgumentNullException("firstNameBqlField");
			if (midNameBqlField == null) throw new ArgumentNullException("midNameBqlField");
			if (titleBqlField == null) throw new ArgumentNullException("titleBqlField");

			_lastNameBqlField = lastNameBqlField;
			_firstNameBqlField = firstNameBqlField;
			_midNameBqlField = midNameBqlField;
			_titleBqlField = titleBqlField;
			_reversed = reversed;

			IsUnicode = true;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_lastNameFieldOrdinal = GetFieldOrdinal(sender, _lastNameBqlField);
			_firstNameFieldOrdinal = GetFieldOrdinal(sender, _firstNameBqlField);
			_midNameFieldOrdinal = GetFieldOrdinal(sender, _midNameBqlField);
			_titleFieldOrdinal = GetFieldOrdinal(sender, _titleBqlField);
		}

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Handler(sender, e.Row);
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Handler(sender, e.Row);
		}

		/*public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (sender.GetValue(e.Row, _FieldOrdinal) == null) 
				Handler(sender, e.Row);
		}*/

		private string FormatDisplayName(PXCache sender, string aLastName, string aFirstName, string aMidName, string aTitle, bool aReversed)
		{
			if (aLastName == null) aLastName = string.Empty;
			if (aFirstName == null) aFirstName = string.Empty;
			if (aMidName == null) aMidName = string.Empty;
			var locolizedTitle = GetLocolizedValue(sender, aTitle);
			if (locolizedTitle == null) locolizedTitle = string.Empty;

			if (string.IsNullOrEmpty(locolizedTitle))
				return Concat(aLastName, ",", aFirstName, aMidName);

			if (aReversed)
				return Concat(aLastName, aFirstName, aMidName, ",", locolizedTitle);

			return Concat(locolizedTitle, aFirstName, aMidName, aLastName);
		}

		private static string Concat(params string[] args)
		{
			var res = new System.Text.StringBuilder();
			foreach (string item in args)
			{
				var s = item.Trim();
				if (s.Length == 0) continue;

				if (s == ",")
				{
					if (res.Length > 0) res.Append(s);
				}
				else
				{
					if (res.Length > 0) res.Append(" ");
					res.Append(s);
				}
			}
			return res.ToString().TrimEnd(',');
		}

		private string GetLocolizedValue(PXCache cache, string message)
		{
			var value = PXUIFieldAttribute.GetNeutralDisplayName(cache, _titleBqlField.Name) + " -> " + message;
			var temp = PXLocalizer.Localize(value, _BqlTable.FullName);
			if (!string.IsNullOrEmpty(temp) && temp != value)
				return temp;
			return message;
		}

		private static int GetFieldOrdinal(PXCache sender, Type bqlField)
		{
			return sender.GetFieldOrdinal(sender.GetField(bqlField));
		}

		private void Handler(PXCache sender, object data)
		{
			var newValue = FormatDisplayName(sender, 
				sender.GetValue(data, _lastNameFieldOrdinal) as string,
				sender.GetValue(data, _firstNameFieldOrdinal) as string,
				sender.GetValue(data, _midNameFieldOrdinal) as string,
				sender.GetValue(data, _titleFieldOrdinal) as string,
				_reversed);
			sender.SetValue(data, _FieldOrdinal, newValue);
		}
	}

	#endregion

	#region LeadSourcesAttribute

	public class CRMSourcesAttribute : PXStringListAttribute
	{
		public const string _WEB = "W";
		public const string _PHONE_INQ = "H";
		public const string _REFERRAL = "R";
		public const string _PURCHASED_LIST = "L";
		public const string _OTHER = "O";

		public CRMSourcesAttribute() : 
			base(new[] { _WEB, _PHONE_INQ, _REFERRAL, _PURCHASED_LIST, _OTHER },
						new[] { Messages.Web, Messages.PhoneInq, Messages.Referral, Messages.PurchasedList, Messages.Other })
		{
		}
	}

	#endregion

	#region CampaignStatusesAttribute

	public class CampaignStatusesAttribute : PXStringListAttribute
	{
		public const string _PREPARED = "P";
		public const string _STARTED = "S";
		public const string _FINISHED = "F";
		public const string _CANCELED = "X";

		public CampaignStatusesAttribute()
			: base(new[] { _PREPARED, _STARTED, _FINISHED, _CANCELED },
			       new[] { Messages.Prepared, Messages.Started, Messages.Finished, Messages.Canceled })
		{}
	}

	#endregion

	#region LeadMajorStatusesAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class LeadMajorStatusesAttribute : PXIntListAttribute
	{
		public const int _RECORD = -2;
		public const int _JUST_CREATED = -1;
		public const int _HOLD = 0;
		public const int _OPEN = 1;
		public const int _SUSPENDED = 2;
		public const int _CLOSED = 3;
		public const int _CONVERTED = 4;

		public LeadMajorStatusesAttribute()
			: base(new[] { _RECORD, _JUST_CREATED, _HOLD, _OPEN, _CLOSED, _SUSPENDED, _CONVERTED},
			new[] { Messages.RecordFlag, Messages.JustCreatedFlag, Messages.HoldFlag, Messages.OpenFlag, Messages.ClosedFlag, Messages.SuspendedFlag, Messages.ConvertedFlag }) 
		{ }

		public class closed : Constant<int>
		{
			public closed():base(_CLOSED)
			{
			}
		}
	}

	#endregion

	#region ActivityMajorStatuses

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class ActivityMajorStatusesAttribute : PXIntListAttribute
	{
		public const int _JUST_CREATED = -1;
		public const int _OPEN = 0;
		public const int _PREPROCESS = 2;
		public const int _PROCESSING = 3;
		public const int _PROCESSED = 4;
		public const int _FAILED = 5;
		public const int _CANCELED = 6;
		public const int _COMPLETED = 7;
		public const int _DELETED = 8;
		public const int _RELEASED = 9;

		public ActivityMajorStatusesAttribute()
			: base(new[] { _JUST_CREATED, _OPEN, _PREPROCESS, _PROCESSING, _PROCESSED, _FAILED, _CANCELED, _COMPLETED, _DELETED, _RELEASED },
			new[] { Messages.JustCreatedFlag, Messages.OpenFlag, Messages.PreprocessFlag, Messages.ProcessingFlag, Messages.ProcessedFlag, Messages.FailedFlag, 
				Messages.CanceledFlag, Messages.CompletedFlag, Messages.DeletedFlag, Messages.ReleasedFlag }) 
		{ }

		public class open : Constant<int>
		{
			public open() : base(_OPEN) { }
		}

		public class preProcess : Constant<int>
		{
			public preProcess() : base(_PREPROCESS) { }
		}

		public class processing : Constant<int>
		{
			public processing() : base(_PROCESSING) { }
		}

		public class processed : Constant<int>
		{
			public processed() : base(_PROCESSED) { }
		}

		public class failed : Constant<int>
		{
			public failed() : base(_FAILED) { }
		}

		public class canceled : Constant<int>
		{
			public canceled() : base(_CANCELED) { }
		}

		public class completed : Constant<int>
		{
			public completed() : base(_COMPLETED) { }
		}

		public class deleted : Constant<int>
		{
			public deleted() : base(_DELETED) { }
		}

		public class released : Constant<int>
		{
			public released() : base(_RELEASED) { }
		}
	}

	#endregion

	#region AnnouncementMajorStatusesAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class AnnouncementMajorStatusesAttribute : PXIntListAttribute
	{
		public const int _DRAFT = 0;
		public const int _PUBLISHED = 1;
		public const int _ARCHIVED = 2;

		public AnnouncementMajorStatusesAttribute()
			: base(new[] { _DRAFT, _PUBLISHED, _ARCHIVED },
			new[] { "Draft", "Published", "Archived" })
		{ }
	}
	#endregion

	#region LeadStatusesAttribute

	public class LeadStatusesAttribute : PXStringListAttribute
	{
		public const string New = "H";
		public const string Open = "O";
		public const string Suspend = "S";
		public const string Closed = "L";
		public const string Converted = "C";

		public LeadStatusesAttribute()
			: base(new[] { New, Open, Suspend, Closed, Converted },
					new[] { Messages.LeadNew, Messages.LeadOpen, Messages.LeadSuspend, Messages.Closed, Messages.LeadConverted })
		{}

		public sealed class @new : Constant<string>
		{
			public @new() : base(New) { }
		}

		public sealed class open : Constant<string>
		{
			public open() : base(Open) { }
		}

		public sealed class suspend : Constant<string>
		{
			public suspend() : base(Suspend) { }
		}

		public sealed class closed : Constant<string>
		{
			public closed() : base(Closed) { }
		}
	}

	#endregion

	#region DuplicateStatusAttribute

	public class DuplicateStatusAttribute : PXStringListAttribute
	{
		public const string NotValidated = "NV";
		public const string PossibleDuplicated = "PD";
		public const string Validated = "VA";
		public const string Duplicated = "DD";

		public DuplicateStatusAttribute() :
			base(new String[0],new String[0])
		{
		}

		public sealed class notValidated : Constant<string>
		{
			public notValidated() : base(NotValidated) {}
		}
		public sealed class possibleDuplicated : Constant<string>
		{
			public possibleDuplicated() : base(PossibleDuplicated) { }
		}
		public sealed class duplicated : Constant<string>
		{
			public duplicated() : base(Duplicated) { }
		}
	}

	#endregion

	#region LeadResolutionsAttribute

	public class LeadResolutionsAttribute : PXStringListAttribute
	{
		public const string CONVERTED = "CC";

		public LeadResolutionsAttribute()
			: base(new string[0], new string[0])
		{
			
		}		
		public sealed class converted : Constant<string>
		{
			public converted() : base(CONVERTED) { }
		}
	}

	#endregion

	#region ValidationTypesAttribute
	public class ValidationTypesAttribute : PXStringListAttribute
	{
		public const string LeadContact = "LC";
		public const string Account = "AC";
		public const string LeadAccount = "LA";

		public ValidationTypesAttribute(): base(new[] { LeadContact, LeadAccount, Account },
												new[] { Messages.LeadToContactValidation, Messages.LeadToAccountValidation, Messages.AccountValidation })
		{}

		public sealed class leadContact : Constant<string>
		{
			public leadContact() : base(LeadContact) { }
		}

		public sealed class account : Constant<string>
		{
			public account() : base(Account) { }
		}

		public sealed class leadAccount : Constant<string>
		{
			public leadAccount() : base(LeadAccount) { }
		}
	}

	#endregion

	#region TransformationRulesAttribute
	public class TransformationRulesAttribute : PXStringListAttribute
	{
		public const string DomainName = "DN";
		public const string None = "NO";
		public const string SplitWords = "SW";

		public TransformationRulesAttribute(): base(new[] {DomainName, None, SplitWords},
													new[] {Messages.DomainName, Messages.None, Messages.SplitWords})
		{}

		public sealed class domainName : Constant<string>
		{
			public domainName() : base(DomainName) { }
		}

		public sealed class none : Constant<string>
		{
			public none() : base(None) { }
		}

		public sealed class splitWords : Constant<string>
		{
			public splitWords() : base(SplitWords) { }
		}
	}

	#endregion

	#region LanguageDBStringAttribute

	public sealed class LanguageDBStringAttribute : PXDBStringAttribute
	{
		private const string _FIELD_POSTFIX = "_DisplayName";

		private string _displayNameFieldName;

		public LanguageDBStringAttribute()
		{
		}

		public LanguageDBStringAttribute(int length) : base(length)
		{
		}

		public string DisplayName { get; set; }

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_displayNameFieldName = _FieldName + _FIELD_POSTFIX;
			sender.Fields.Add(_displayNameFieldName);
			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _displayNameFieldName, _FieldName_DisplayName_FieldSelecting);
		}

		private void _FieldName_DisplayName_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs args)
		{
			var langVal = sender.GetValue(args.Row, _FieldOrdinal) as string;
			var displayNameVal = string.Empty;
			if (!string.IsNullOrEmpty(langVal))
				try
				{
					displayNameVal = CultureInfo.GetCultureInfo(langVal).DisplayName;
				}
				catch (ArgumentException) { } //NOTE: incorrect language value
			args.ReturnState = PXFieldState.CreateInstance(displayNameVal, typeof(string), null, null, null, null, null,
                                   null, _displayNameFieldName, null, DisplayName, null, PXErrorLevel.Undefined, false,
                                   true, null, PXUIVisibility.Visible, null, null, null);
		}
	}

	#endregion

	#region PXNotificationContactSelectorAttribute	
	public class PXNotificationContactSelectorAttribute : PXSelectorAttribute
	{
		public PXNotificationContactSelectorAttribute()
			: this(null)
		{
		}

		public PXNotificationContactSelectorAttribute(Type contactType)
					:base(typeof(Search2<Contact.contactID,
				LeftJoin<BAccountR, On<BAccountR.bAccountID, Equal<Contact.bAccountID>>,			
				LeftJoin<EPEmployee, 
				      On<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>,
				      And<EPEmployee.defContactID, Equal<Contact.contactID>>>>>,
				Where2<
						Where<Current<NotificationRecipient.contactType>, Equal<NotificationContactType.employee>,
              And<EPEmployee.acctCD, IsNotNull>>,
					 Or<Where<Current<NotificationRecipient.contactType>, Equal<NotificationContactType.contact>,
								And<BAccountR.noteID, Equal<Current<NotificationRecipient.refNoteID>>,
								And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>>>>))
		{
			SubstituteKey = typeof(Contact.displayName);
			this.contactType = contactType;
		}

		public PXNotificationContactSelectorAttribute(Type contactType, Type search)
			:base(search)
		{
			SubstituteKey = typeof(Contact.displayName);
			this.contactType = contactType;
		}

		private Type contactType;
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			if(contactType != null)
				sender.Graph.RowSelected.AddHandler(sender.GetItemType(), OnRowSelected);
		}

		protected virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null || contactType == null) return;
			
			PXCache sourceCache = sender.Graph.Caches[BqlCommand.GetItemType(contactType)];
			object value = 
				(sourceCache == sender) ?
				sender.GetValue(e.Row, contactType.Name) :
				sourceCache.GetValue(sourceCache.Current, contactType.Name);

			string type = value != null ? value.ToString() : null;				

			bool enabled = 
				type == NotificationContactType.Contact ||
				 type == NotificationContactType.Employee;
			PXUIFieldAttribute.SetEnabled(sender, e.Row, _FieldName, enabled);
			PXDefaultAttribute.SetPersistingCheck(sender, _FieldName, e.Row, enabled ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}
	}
	#endregion

	public class String<ListField> : BqlFormula<ListField>, IBqlOperand
		where ListField : IBqlField
	{
		public override void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			PXFieldState state = cache.GetStateExt<ListField>(item) as PXFieldState;
			value = PXFieldState.GetStringValue(state, null, null);
		}
	}

	#region PXLocationID
	public class PXLocationIDAttribute : PXAggregateAttribute
	{
		private string _DimensionName = "LOCATION";
		public PXLocationIDAttribute(Type type, params Type[] fieldList):
			base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(_DimensionName);
			attr.ValidComboRequired = true;
			_Attributes.Add(attr);

			PXNavigateSelectorAttribute selectorattr = new PXNavigateSelectorAttribute(type, fieldList);
			_Attributes.Add(selectorattr);
		}

		public PXLocationIDAttribute(Type type):
			base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(_DimensionName);
			attr.ValidComboRequired = true;
			_Attributes.Add(attr);
			
			PXNavigateSelectorAttribute selectorattr = new PXNavigateSelectorAttribute(type);
			_Attributes.Add(selectorattr);
		}
	}
	#endregion

	#region DefLocationID
	public class DefLocationIDAttribute : PXAggregateAttribute
	{
		private string _DimensionName = "LOCATION";

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

		public DefLocationIDAttribute(Type type, params Type[] fieldList) 
			: base()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute(_DimensionName);
			attr.ValidComboRequired = true;
			_Attributes.Add(attr);

			PXSelectorAttribute selattr = new PXSelectorAttribute(type, fieldList);
			selattr.DirtyRead = true;
			selattr.CacheGlobal = false;
			_Attributes.Add(selattr);
		}

		public DefLocationIDAttribute(Type type) 
			: this(type, typeof(Location.locationCD), typeof(Location.descr))
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdating.RemoveHandler(sender.GetItemType(), _FieldName, this.GetAttribute<PXSelectorAttribute>().SubstituteKeyFieldUpdating);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, FieldUpdating);
		}

		protected virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			PXFieldUpdating fu = this.GetAttribute<PXDimensionAttribute>().FieldUpdating;
			fu(sender, e);
			e.Cancel = false;

			fu = this.GetAttribute<PXSelectorAttribute>().SubstituteKeyFieldUpdating;
			fu(sender, e);
		}

		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) != typeof(IPXFieldUpdatingSubscriber))
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
            
            if (SubstituteKey == null || String.Compare(SubstituteKey.Name, _FieldName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                if (typeof(ISubscriber) == typeof(IPXFieldDefaultingSubscriber))
                {
                    subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
                }
                if (typeof(ISubscriber) == typeof(IPXRowPersistingSubscriber))
                {
                    subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
                }
                else if (typeof(ISubscriber) == typeof(IPXRowPersistedSubscriber))
                {
                    subscribers.Remove(_Attributes[_Attributes.Count - 2] as ISubscriber);
                }
            }
        }
	}
	#endregion

	#region PXNavigateSelector

	public class PXNavigateSelectorAttribute : PXSelectorAttribute
	{
		public PXNavigateSelectorAttribute(Type type) : base(type)
		{
		}

		public PXNavigateSelectorAttribute(Type type, params Type[] fieldList)
			: base(type, fieldList)
		{
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			
		}

		protected override bool IsReadDeletedSupported
		{
			get
			{
				return false;
			}
		}

		protected override void SetBqlTable(Type bqlTable)
		{
			base.SetBqlTable(bqlTable);
			lock (((ICollection)_SelectorFields).SyncRoot)
			{
				List<KeyValuePair<string, Type>> list;
				if (_SelectorFields.TryGetValue(bqlTable, out list) && list.Count > 0 && list[list.Count - 1].Key == base.FieldName)
				{
					list.RemoveAt(list.Count - 1);
				}
			}
		}
	}

	#endregion

	#region CaseRelationTypeAttribute

	public class CaseRelationTypeAttribute : PXStringListAttribute
	{
		public const string _BLOCKS_VALUE = "P";
		public const string _DEPENDS_ON_VALUE = "C";
		public const string _RELATED_VALUE = "R";
		public const string _DUBLICATE_OF_VALUE = "D";

		public CaseRelationTypeAttribute()
			: base(new[] { _BLOCKS_VALUE, _DEPENDS_ON_VALUE, _RELATED_VALUE, _DUBLICATE_OF_VALUE }, new[] { "Blocks", "Depends On", "Related", "Duplicate Of" })
		{
			
		}
	}

	#endregion

	#region CRPreviewAttribute

	public class CRPreviewAttribute : PXPreviewAttribute
	{
		private readonly PXSelectDelegate _select;
		private GeneratePreview _handler;
		private object _current;

		public delegate object GeneratePreview(object row);

		public CRPreviewAttribute(Type primaryViewType, Type previewType)
			: base(primaryViewType, previewType)
		{
			_select = new PXSelectDelegate(() => new [] { _current });
		}

		protected override PXSelectDelegate SelectHandler
		{
			get
			{
				return _select;
			}
		}

		protected override IEnumerable GetPreview()
		{
			if (_handler != null)
			{
				var row = Graph.Caches[CacheType].Current;
				yield return _handler(row);
			}
		}

		protected override void PerformRefresh()
		{
			foreach(object row in GetPreview())
				_current = row;
		}

		public virtual void Attach(PXGraph graph, string viewName, GeneratePreview getPreviewHandler)
		{
			if (_handler != null) throw new InvalidOperationException("Attributes is already attached.");

			_handler = getPreviewHandler ?? (o => o);
			ViewCreated(graph, viewName);
		}
	}

	#endregion

	#region LeadRawAttribute

	public class LeadRawAttribute : AcctSubAttribute
	{
		public const string DimensionName = "LEAD";

		public LeadRawAttribute()
		{
			Type searchType = typeof(Search<Contact.contactID, Where<Contact.contactType, Equal<ContactTypesAttribute.lead>>>);

			PXDimensionSelectorAttribute attr;
			_Attributes.Add(attr = new PXDimensionSelectorAttribute(DimensionName, searchType, typeof(EPEmployee.acctCD))); //TODO: need implementation (substituteKey)
			attr.DescriptionField = typeof(Contact.displayName);
			_SelAttrIndex = _Attributes.Count - 1;
			this.Filterable = true;
		}
	}

	#endregion

	#region CRContactsViewAttribute

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class CREmailContactsViewAttribute : Attribute
	{
		private readonly BqlCommand _select;

		public CREmailContactsViewAttribute(Type select)
		{
			if (select == null) throw new ArgumentNullException("select");
			if (!typeof(BqlCommand).IsAssignableFrom(select)) 
				throw new ArgumentException(string.Format("type '{0}' must inherit PX.Data.BqlCommand", select.Name), "select");
			_select = BqlCommand.CreateInstance(select);
		}

		public BqlCommand Select
		{
			get { return _select; }
		}

		public static PXView GetView(PXGraph graph, Type objType)
		{
			if (graph == null || objType == null) return null;

			var contactsViewAtt = GetCustomAttribute(objType, typeof(CREmailContactsViewAttribute), true) as CREmailContactsViewAttribute;
			if (contactsViewAtt != null && contactsViewAtt.Select != null)
				return new PXView(graph, true, contactsViewAtt.Select);
			return null;
		}
	}

	#endregion

	#region CREmailSelectorAttribute

	public sealed class CREmailSelectorAttribute : PXCustomSelectorAttribute
	{
		private readonly bool _all;
		private readonly ContactSelectorAttribute _allContactsSelector;

		public CREmailSelectorAttribute() : this(false) { }

		public CREmailSelectorAttribute(bool all)
			: base(typeof(Search<Contact.eMail>),
					new[] { typeof(Contact.displayName), typeof(Contact.eMail) })
		{
			_all = all;
			_allContactsSelector = new ContactSelectorAttribute(typeof(ContactTypesAttribute.lead), typeof(ContactTypesAttribute.person), typeof(ContactTypesAttribute.employee));
			_ViewName = GenerateViewName();
			base.DescriptionField = typeof(Contact.displayName);
			CacheGlobal = false;
		}

		public override Type DescriptionField
		{
			get
			{
				return base.DescriptionField;
			}
			set { }
		}

		protected override bool IsReadDeletedSupported
		{
			get
			{
				return false;
			}
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public override void ReadDeletedFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			
		}

		protected override string GenerateViewName()
		{
			var name = base.GenerateViewName() + "Email_";
			if (_all) name += "All_";
			return name;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_allContactsSelector.CacheAttached(sender);
		}

		public IEnumerable GetRecords()
		{
			var cache = _Graph.Caches[typeof(EPActivity)];
			var row = cache.Current;
			var refEntity = row.
				With(_ => cache.GetValue(row, typeof(EPActivity.refNoteID).Name)).
				With(_ => new EntityHelper(_Graph).GetEntityRow((long?)_));
			PXView view;
			if (_all || refEntity == null || (view = CREmailContactsViewAttribute.GetView(_Graph, refEntity.GetType())) == null)
				return _allContactsSelector.GetRecords();
			
			var startRow = PXView.StartRow;
			var totalRows = 0;
			var res = view.Select(new [] { refEntity }, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
							ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			return res;
		}
	}

	#endregion

	#region EPActivityDescriptionAttribute

	public sealed class EPActivityDescriptionAttribute : PXDACDescriptionAttribute
	{
		public EPActivityDescriptionAttribute(Type type)
			: base(type, new EPActivityPrimaryGraphAttribute())
		{}
	}

	#endregion

	#region EPActivityPrimaryGraphAttribute

	public sealed class EPActivityPrimaryGraphAttribute : PXPrimaryGraphBaseAttribute
	{
		public override Type GetGraphType(PXCache cache, ref object row, bool checkRights, Type preferedType)
		{
			var act = row as EPActivity;

			return GetGraphType(act);
		}

		public static Type GetGraphType(EPActivity act)
		{
			if (act == null || act.ClassID == null) 
				return null;

			return GetGraphType((int)act.ClassID);
		}

		public static Type GetGraphType(int classID)
		{
			switch (classID)
			{
				case CRActivityClass.Task:
					return typeof(CRTaskMaint);
				case CRActivityClass.Event:
					return typeof(EPEventMaint);
				case CRActivityClass.Activity:
					return typeof(CRActivityMaint);
				case CRActivityClass.History:
					return typeof(ChangesetMaint);
				case CRActivityClass.Email:
					return typeof(CREmailActivityMaint);
				case CRActivityClass.EmailRouting:
					return typeof(EmailRoutingMaint);
				default:
					return null;
			}
		}
	}

	#endregion

	#region CRContactMethodsAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class CRContactMethodsAttribute : PXStringListAttribute
	{
		public const string Any = "A";
		public const string Email = "E";
		public const string Mail = "M";
		public const string Fax = "F";
		public const string Phone = "P";

		public CRContactMethodsAttribute()
			: base(new [] { Any, Email, Mail, Fax, Phone },
					new [] { Messages.MethodAny, Messages.MethodEmail, Messages.MethodMail, Messages.MethodFax, Messages.MethodPhone })
		{
			
		}

		public class any : Constant<string>
		{
			public any() : base(Any) { }
		}

		public class email : Constant<string>
		{
			public email() : base(Email) { }
		}

		public class mail : Constant<string>
		{
			public mail() : base(Mail) { }
		}

		public class fax : Constant<string>
		{
			public fax() : base(Fax) { }
		}

		public class phone : Constant<string>
		{
			public phone() : base(Phone) { }
		}
	}

	#endregion

	#region MaritalStatusesAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class MaritalStatusesAttribute : PXStringListAttribute
	{
		public const string Single = "S";
		public const string Married = "M";
		public const string Divorced = "D";
		public const string Widowed = "W";

		public MaritalStatusesAttribute()
			: base(new [] { Single, Married, Divorced, Widowed }, 
					new [] { Messages.Single, Messages.Married, Messages.Divorced, Messages.Widowed })
		{
			
		}

		public class single : Constant<string>
		{
			public single() : base(Single) { }
		}

		public class married : Constant<string>
		{
			public married() : base(Married) { }
		}

		public class divorced : Constant<string>
		{
			public divorced() : base(Divorced) { }
		}

		public class widowed : Constant<string>
		{
			public widowed() : base(Widowed) { }
		}
	}

	#endregion

	#region GendersAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class GendersAttribute : PXStringListAttribute
	{
		public const string Male = "M";
		public const string Female = "F";

		private readonly Type _titleField;
		private int _titleFieldOrdinal;

		public GendersAttribute(Type titleField)
			: this()
		{
			if (titleField == null) throw new ArgumentNullException("titleField");
			_titleField = titleField;
		}

		public GendersAttribute()
			: base(new [] { Male, Female }, new [] { Messages.Male, Messages.Female })
		{
			
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (_titleField != null)
			{
				_titleFieldOrdinal = sender.GetFieldOrdinal(sender.GetField(_titleField));
				sender.Graph.RowInserted.AddHandler(_BqlTable, RowInsertedHandler);
				sender.Graph.RowUpdated.AddHandler(_BqlTable, RowUpdatedHandler);
			}
		}

		private void RowInsertedHandler(PXCache sender, PXRowInsertedEventArgs e)
		{
			var gender = sender.GetValue(e.Row, _FieldName);
			if (gender == null)
			{
				var title = sender.GetValue(e.Row, _titleFieldOrdinal) as string;
				if (title != null)
				{
					object newVal = null;
					switch (title)
					{
						case TitlesAttribute.Mr:
							newVal = Male;
							break;
						case TitlesAttribute.Ms:
						case TitlesAttribute.Miss:
						case TitlesAttribute.Mrs:
							newVal = Female;
							break;
					}
					sender.SetValue(e.Row, _FieldName, newVal);
				}
			}
		}

		private void RowUpdatedHandler(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var gender = sender.GetValue(e.Row, _FieldOrdinal);
			var oldGender = sender.GetValue(e.OldRow, _FieldOrdinal);
			var title = sender.GetValue(e.Row, _titleFieldOrdinal) as string;
			var oldlTitle = sender.GetValue(e.OldRow, _titleFieldOrdinal) as string;
			if (gender == oldGender && title != null && title != oldlTitle)
			{
				object newVal = null;
				switch (title)
				{
					case TitlesAttribute.Mr:
						newVal = Male;
						break;
					case TitlesAttribute.Ms:
					case TitlesAttribute.Miss:
					case TitlesAttribute.Mrs:
						newVal = Female;
						break;
				}
				if (newVal != null) sender.SetValue(e.Row, _FieldOrdinal, newVal);
			}
		}

		public class male : Constant<string>
		{
			public male() : base(Male) { }
		}

		public class female : Constant<string>
		{
			public female() : base(Female) { }
		}
	}

	#endregion

	#region PXPrimaryGraphAttribute
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class CRCacheIndependentPrimaryGraphAttribute : PXPrimaryGraphBaseAttribute
	{
		private readonly CRCacheIndependentPrimaryGraphListAttribute _att;

		public CRCacheIndependentPrimaryGraphAttribute(Type graphType, Type condition)
		{
			_att = new CRCacheIndependentPrimaryGraphListAttribute { { graphType, condition } };
		}

		public override Type GetGraphType(PXCache cache, ref object row, bool checkRights, Type preferedType)
		{
			return _att.GetGraphType(cache, ref row, checkRights, preferedType);
		}
	}
	#endregion

	#region CRCacheIndependantPrimaryGraphListAttribute

	public class CRCacheIndependentPrimaryGraphListAttribute : PXPrimaryGraphBaseAttribute, IEnumerable
	{
		#region PrimaryGraph

		private sealed class PrimaryGraph
		{
			private readonly Type _graphType;
			private readonly Type _condition;

			public PrimaryGraph(Type graphType, Type condition)
			{
				if (graphType == null) throw new ArgumentNullException("graphType");
				if (!typeof(PXGraph).IsAssignableFrom(graphType))
					throw new ArgumentException(string.Format(Messages.NeedGraphType, graphType.GetLongName()));
				if (condition == null) throw new ArgumentNullException("condition");
				if (!typeof(BqlCommand).IsAssignableFrom(condition) && !typeof(IBqlWhere).IsAssignableFrom(condition))
					throw new ArgumentException(string.Format(Messages.NeedBqlCommandType, condition.GetLongName()));

				_graphType = graphType;
				_condition = condition;
			}

			public Type GraphType
			{
				get { return _graphType; }
			}

			public Type Condition
			{
				get { return _condition; }
			}
		}

		#endregion

		private readonly IList<PrimaryGraph> _items;

		public CRCacheIndependentPrimaryGraphListAttribute()
		{
			_items = new List<PrimaryGraph>();
		}

		public CRCacheIndependentPrimaryGraphListAttribute(Type[] graphTypes, Type[] conditions) 
			: this()
		{
			if (graphTypes == null) throw new ArgumentNullException("graphTypes");
			if (conditions == null) throw new ArgumentNullException("conditions");
			if (graphTypes.Length != conditions.Length)
				throw new ArgumentException(Messages.GraphTypesAndConditionsLengthException);

			for (int index = 0; index < conditions.Length; index++)
			{
				var graphType = graphTypes[index];
				var condition = conditions[index];
				Add(graphType, condition);
			}
		}

		public virtual void Add(Type graphType, Type condition)
		{
			_items.Add(new PrimaryGraph(graphType, condition));
		}

		public override Type GetGraphType(PXCache cache, ref object row, bool checkRights, Type preferedType)
		{
			PXGraph graph = null;
			foreach (PrimaryGraph pg in _items)
			{
				if (PXAccess.VerifyRights(pg.GraphType))
				{
					var itemType = cache.GetItemType();
					if (graph == null)
					{
						graph = new PXGraph();
						var rowStatus = cache.GetStatus(row);
						graph.Caches[itemType].SetStatus(row, rowStatus);
					}
					if (typeof(IBqlWhere).IsAssignableFrom(pg.Condition))
					{
						IBqlWhere where = (IBqlWhere)Activator.CreateInstance(pg.Condition);
						bool? result = null;
						object value = null;
						where.Verify(graph.Caches[itemType], row, new List<object>(), ref result, ref value);
						if (result == true && (preferedType == null || preferedType.IsAssignableFrom(pg.GraphType)))
						{
							return pg.GraphType;
						}
					}
					else
					{
						var command = BqlCommand.CreateInstance(pg.Condition);
						var view = new PXView(graph, false, command);
						var item = view.SelectSingleBound(new[] { row });
						if (item != null && (preferedType == null || preferedType.IsAssignableFrom(pg.GraphType)))
						{
							row = row is PXResult ? ((PXResult)row)[0] : item;
							return pg.GraphType;
						}
					}
				}
			}
			return null;
		}

		public virtual IEnumerator GetEnumerator()
		{
			return _items.GetEnumerator();
		}
	}

	#endregion

	#region CRMassMailStatusesAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class CRMassMailStatusesAttribute : PXStringListAttribute
	{
		public const string Hold = "H";
		public const string Prepared = "P";
		public const string Approved = "A";
		public const string Send = "S";

		public CRMassMailStatusesAttribute()
			: base(
				new[] { Hold, Prepared, Approved, Send },
				new[] { Messages.Hold_MassMailStatus, Messages.Prepared_MassMailStatus, 
					Messages.Approved_MassMailStatus, Messages.Sent_MassMailStatus })
		{
			
		}

		public class hold : Constant<string>
		{
			public hold() : base(Hold) { }
		}

		public class prepared : Constant<string>
		{
			public prepared() : base(Prepared) { }
		}

		public class approved : Constant<string>
		{
			public approved() : base(Approved) { }
		}
		public class send : Constant<string>
		{
			public send() : base(Send) { }
		}

	}

	#endregion

	#region CRMassMailSourcesAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class CRMassMailSourcesAttribute : PXIntListAttribute
	{
		public const int MailList = 0;
		public const int Campaign = 1;
		public const int Lead = 2;

		public CRMassMailSourcesAttribute()
			: base(new[] { MailList, Campaign, Lead },
				new[] { Messages.MailList_MassMailSource, Messages.Campaign_MassMailSource, Messages.LeadContacts_MassMailSource })
		{
			
		}

		public class hold : Constant<int>
		{
			public hold() : base(Campaign) { }
		}

		public class pending : Constant<int>
		{
			public pending() : base(Lead) { }
		}

		public class rejected : Constant<int>
		{
			public rejected() : base(MailList) { }
		}
	}
	#endregion

	#region ContactTypesAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class ContactTypesAttribute : PXStringListAttribute
	{
		public const string Person = "PN";
		public const string SalesPerson = "SP";
		public const string BAccountProperty = "AP";
		public const string Employee = "EP";
		public const string Lead = "LD";

		public ContactTypesAttribute()
			: base(
			new string[] { Person, SalesPerson, BAccountProperty, Employee, Lead },
			new string[] { Messages.Person, Messages.SalesPerson, Messages.BAccountProperty, Messages.Employee, Messages.Lead })
		{

		}

		public class person : Constant<string>
		{
			public person() : base(Person) { ; }
		}
		public class bAccountProperty : Constant<string>
		{
			public bAccountProperty() : base(BAccountProperty) { ; }
		}
		public class salesPerson : Constant<string>
		{
			public salesPerson() : base(SalesPerson) { ; }
		}
		public class employee : Constant<string>
		{
			public employee() : base(Employee) { ; }
		}
		public class lead : Constant<string>
		{
			public lead() : base(Lead) { ; }
		}

	}

	#endregion

	#region PhoneTypesAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class PhoneTypesAttribute : PXStringListAttribute
	{
		public const string Business1 = "B1";
		public const string Business2 = "B2";
		public const string Business3 = "B3";
		public const string BusinessAssistant1 = "BA1";
		public const string BusinessFax = "BF";
		public const string Home = "H1";
		public const string HomeFax = "HF";
		public const string Cell = "C";

		public PhoneTypesAttribute()
			: base(
				new string[] { Business1, Business2, Business3, BusinessAssistant1, BusinessFax, Home, HomeFax, Cell },
				new string[] { Messages.Business1, Messages.Business2, Messages.Business3, Messages.BusinessAssistant1, Messages.BusinessFax, Messages.Home, Messages.HomeFax, Messages.Cell })
		{
		}

		public class business1 : Constant<string>
		{
			public business1() : base(Business1) { ;}
		}
		public class business2 : Constant<string>
		{
			public business2() : base(Business2) { ;}
		}
		public class business3 : Constant<string>
		{
			public business3() : base(Business3) { ;}
		}
		public class businessAssistant1 : Constant<string>
		{
			public businessAssistant1() : base(BusinessAssistant1) { ;}
		}
		public class businessFax : Constant<string>
		{
			public businessFax() : base(BusinessFax) { ;}
		}
		public class home : Constant<string>
		{
			public home() : base(Home) { ;}
		}
		public class homeFax : Constant<string>
		{
			public homeFax() : base(HomeFax) { ;}
		}
		public class cell : Constant<string>
		{
			public cell() : base(Cell) { ;}
		}
	}

	#endregion

	#region TitlesAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class TitlesAttribute : PXStringListAttribute
	{
		public TitlesAttribute()
			: base(
				new string[] { Doctor, Miss, Mr, Mrs, Ms, Prof },
				new string[] { Messages.Doctor, Messages.Miss, Messages.Mr, Messages.Mrs, Messages.Ms, Messages.Prof })
		{
		}

		public const string Doctor = "Dr.";
		public const string Miss = "Miss";
		public const string Mr = "Mr.";
		public const string Mrs = "Mrs.";
		public const string Ms = "Ms.";
		public const string Prof = "Prof.";
	}

	#endregion

	#region PhoneValidationAttribute
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class PhoneValidationAttribute : PX.SM.PXPhoneValidationAttribute
	{
		private class Definition : IPrefetchable
		{
			public string PhoneMask;
			void IPrefetchable.Prefetch()
			{
				using (PXDataRecord pref =
						PXDatabase.SelectSingle<GL.Company>(
						new PXDataField(typeof(GL.Company.phoneMask).Name)))
				{
					if (pref != null)
					{
						PhoneMask = pref.GetString(0);
					}
				}
			}
		}
		public PhoneValidationAttribute() : base("") { }
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			Definition def = PXDatabase.GetSlot<Definition>("CompanyPhoneMask", typeof(GL.Company));
			if (def != null)
			{
				_mask = def.PhoneMask ?? "";
			}
		}
	}
	#endregion

	#region CRMassEmailLoadTemplateAttribute

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class CRMassEmailLoadTemplateAttribute : PXEmailLoadTemplateAttribute
	{
		private const string _MAILTO_PARAM_NAME = "mailTo";
		private const string _MAILCC_PARAM_NAME = "mailCc";
		private const string _MAILBCC_PARAM_NAME = "mailBcc";
		private const string _MAILSUBJECT_PARAM_NAME = "mailSubject";

		public CRMassEmailLoadTemplateAttribute(Type primaryView) : base(primaryView)
		{
		}

		protected override void ProcessParameter(string column, ref string value)
		{
			base.ProcessParameter(column, ref value);

			var cache = Graph.Caches[PrimaryView];
			var current = cache.Current;
			if (current == null || string.IsNullOrEmpty(value)) return;

			Type field = null;
			if (string.Compare(column, _MAILTO_PARAM_NAME, true) == 0)
			{
				field = typeof(CRMassMail.mailTo);
			}
			else if (string.Compare(column, _MAILCC_PARAM_NAME, true) == 0)
			{
				field = typeof(CRMassMail.mailCc);
			}
			else if (string.Compare(column, _MAILBCC_PARAM_NAME, true) == 0)
			{
				field = typeof(CRMassMail.mailBcc);
			}
			else if (string.Compare(column, _MAILSUBJECT_PARAM_NAME, true) == 0)
			{
				field = typeof(CRMassMail.mailSubject);
			}

			if (field != null)
			{
				cache.SetValue(current, field.Name, value);
				cache.Update(current);
			}
		}
	}

	#endregion

	#region CRTimeSpanCalcedAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class CRTimeSpanCalcedAttribute : PXDBCalcedAttribute
	{
		// private static readonly long _1900YEAR_TICKS = new DateTime(1900, 1, 1).Ticks;

		public CRTimeSpanCalcedAttribute(Type operand) 
			: base(operand, typeof(int))
		{
		}

		public override void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			// excepting DATEDIFF(mi, d1, d2)
			object value = e.Record.GetValue(e.Position);
			if (value != null)
			{
				int v = Convert.ToInt32(value);
				value = v > 0 ? value : 0;
				// new TimeSpan(ticks > _1900YEAR_TICKS ? ticks - _1900YEAR_TICKS : 0).TotalMinutes;
			}
			sender.SetValue(e.Row, _FieldOrdinal, Convert.ToInt32(value));
			e.Position++;
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			
		}
	}

	#endregion

	/*public class CRFieldsListAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		private readonly Type[] _tables;

		public CRFieldsListAttribute(Type[] tables)
		{
			_tables = tables;
		}

		public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (attributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				ValueLabelPairs pairs = Data;
				e.ReturnState = CreateState(sender, e, pairs.Values, Localize(pairs.DescriptionFieldName, pairs.Labels), fieldName, pairs.DefaultValue);
			}
		}
	}*/

	#region CRUnsafeUIFieldAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public sealed class CRUnsafeUIFieldAttribute : PXUIFieldAttribute
	{
		public override void CacheAttached(PXCache sender)
		{
			TryLocalize(sender);
		}

		protected override bool EnableRights
		{
			get
			{
				return true;
			}
			set
			{
				
			}
		}
	}

	#endregion

	#region CRFixedFilterableAttribute

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class CRFixedFilterableAttribute : PXViewExtensionAttribute
	{
		public const string FilterRowName = "$FilterRow";

		private readonly Type _refNoteID;

		public CRFixedFilterableAttribute(Type refNoteID)
		{
			if (refNoteID == null) 
				throw new ArgumentNullException("refNoteID");
			if (!typeof(IBqlField).IsAssignableFrom(refNoteID))
				throw new ArgumentException(typeof(IBqlField).GetLongName() + " expected.", "refNoteID");

			_refNoteID = refNoteID;
		}

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			var command = BqlCommand.CreateInstance(
				typeof(Select<,>), typeof(CRFixedFilterRow), 
				typeof(Where<,>), typeof(CRFixedFilterRow.refNoteID), 
				typeof(Equal<>), typeof(Optional<>), _refNoteID);
			var detailView = new PXView(graph, false, command);

			graph.Views[viewName + FilterRowName] = detailView;
			detailView.Cache.AllowSelect = true;
			detailView.Cache.AllowInsert = true;
			detailView.Cache.AllowUpdate = true;
			detailView.Cache.AllowDelete = true;

			graph.EnshureCachePersistance(typeof(CRFixedFilterRow));

			var mainCacheType = BqlCommand.GetItemType(_refNoteID);
			graph.FieldDefaulting.AddHandler(typeof(CRFixedFilterRow), typeof(CRFixedFilterRow.refNoteID).Name, 
				(sender, args) =>
					{
						var cache = graph.Caches[mainCacheType];
						var noteID = (long?)cache.GetValue(cache.Current, cache.GetField(_refNoteID));
						args.NewValue = noteID;
					});
			graph.RowPersisted.AddHandler(mainCacheType, 
				(sender, args) =>
					{
						if (args.Row != null && 
							args.Operation == PXDBOperation.Insert && 
							args.TranStatus == PXTranStatus.Open)
						{
							var cache = graph.Caches[mainCacheType];
							var noteID = cache.GetValue(args.Row, cache.GetField(_refNoteID));
							foreach (object item in detailView.SelectMulti(noteID))
							{
								var refNoteID = (long?)detailView.Cache.GetValue(item, typeof(CRFixedFilterRow.refNoteID).Name);
								if (refNoteID == null || refNoteID.Value < 0)
									detailView.Cache.SetValue(item, typeof(CRFixedFilterRow.refNoteID).Name, noteID);
							}
						}
					});

			PXUIFieldAttribute.SetEnabled(detailView.Cache, null, true);
			PXUIFieldAttribute.SetReadOnly(detailView.Cache, null, false);
		}
	}

	#endregion

	#region CRServiceCaseAddressAttribute

	public abstract class CRServiceCaseAddressAttribute : AddressAttribute
	{
		#region Fields

		private readonly BqlCommand _duplicateSelect;

		#endregion

		#region Ctor
		protected CRServiceCaseAddressAttribute(Type AddressIDType, Type IsDefaultAddressType, Type SelectType)
			: base(AddressIDType, IsDefaultAddressType, SelectType)
		{
			_duplicateSelect = 
				BqlCommand.CreateInstance(typeof(Select<CRServiceCaseAddress, 
					Where<CRServiceCaseAddress.customerID, Equal<Required<CRServiceCaseAddress.customerID>>, 
						And<CRServiceCaseAddress.customerAddressID, Equal<Required<CRServiceCaseAddress.customerAddressID>>, 
						And<CRServiceCaseAddress.isDefaultAddress, Equal<True>>>>>));
		}
		#endregion

		#region Implementation
		public override void Record_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = (CRServiceCaseAddress)e.Row;
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && row.IsDefaultAddress == true)
			{
				PXView view = sender.Graph.TypedViews.GetView(_duplicateSelect, true);
				view.Clear();

				var prevAddress = (CRServiceCaseAddress)view.SelectSingle(row.CustomerID, row.CustomerAddressID);
				if (prevAddress != null)
				{
					_KeyToAbort = sender.GetValue(e.Row, _RecordID);
					object newkey = sender.Graph.Caches[typeof(CRServiceCaseAddress)].GetValue(prevAddress, _RecordID);

					PXCache cache = sender.Graph.Caches[_ItemType];

					foreach (object data in cache.Updated)
					{
						object datakey = cache.GetValue(data, _FieldOrdinal);
						if (Equals(_KeyToAbort, datakey))
						{
							cache.SetValue(data, _FieldOrdinal, newkey);
						}
					}

					_KeyToAbort = null;
					e.Cancel = true;
					return;
				}
			}
			base.Record_RowPersisting(sender, e);
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			object key = sender.GetValue(e.Row, _FieldOrdinal);
			if (key != null)
			{
				PXCache cache = sender.Graph.Caches[_RecordType];
				if (Convert.ToInt32(key) < 0)
				{
					var addressCache = sender.Graph.Caches[typeof(CRServiceCaseAddress)];
					foreach (object data in cache.Inserted)
					{
						object datakey = cache.GetValue(data, _RecordID);
						if (Equals(key, datakey))
						{
							var address = (CRServiceCaseAddress)data;
							if (address.IsDefaultAddress == true)
							{
								PXView view = sender.Graph.TypedViews.GetView(_duplicateSelect, true);
								view.Clear();

								var prevAddress = (CRServiceCaseAddress)view.SelectSingle(address.CustomerID, address.CustomerAddressID);

								if (prevAddress != null)
								{
									_KeyToAbort = sender.GetValue(e.Row, _FieldOrdinal);
									object id = addressCache.GetValue(prevAddress, _RecordID);
									sender.SetValue(e.Row, _FieldOrdinal, id);
								}
							}
							break;
						}
					}
				}
			}
			base.RowPersisting(sender, e);
		}
		#endregion
	}

	#endregion

	#region CRServiceCaseBillingAddressAttribute

	public class CRServiceCaseBillingAddressAttribute : CRServiceCaseAddressAttribute
	{
		public CRServiceCaseBillingAddressAttribute()
			: base(typeof(CRServiceCaseBillingAddress.addressID), typeof(CRServiceCaseBillingAddress.isDefaultAddress), SelectType)
		{
		}

		private static Type SelectType
		{
			get
			{
				return typeof (Select2<BAccountR,
					InnerJoin<Location,
						On<Location.bAccountID, Equal<BAccountR.bAccountID>,
							And<Location.locationID, Equal<BAccountR.defLocationID>>>,
						LeftJoin<Customer,
							On<Customer.bAccountID, Equal<BAccountR.bAccountID>>,
						InnerJoin<Address,
							On<Address.bAccountID, Equal<BAccountR.bAccountID>,
								And<Where2<
									Where<Customer.bAccountID, IsNotNull,
										And<Address.addressID, Equal<Customer.defBillAddressID>>>,
									Or<Where<Customer.bAccountID, IsNull,
										And<Address.addressID, Equal<BAccountR.defAddressID>>>>>>>,
						LeftJoin<CRServiceCaseBillingAddress,
							On<CRServiceCaseBillingAddress.customerID, Equal<Address.bAccountID>,
								And<CRServiceCaseBillingAddress.customerAddressID, Equal<Address.addressID>,
								And<CRServiceCaseBillingAddress.isDefaultAddress, Equal<True>>>>>>>>,
					Where<BAccountR.bAccountID, Equal<Current<CRServiceCase.customerID>>>>);
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<CRServiceCaseBillingAddress.overrideAddress>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<CRServiceCaseBillingAddress, CRServiceCaseBillingAddress.addressID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<CRServiceCaseBillingAddress, CRServiceCaseBillingAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<CRServiceCaseBillingAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<CRServiceCaseBillingAddress.overrideAddress>(sender, e.Row, true);
			}
		}
	}

	#endregion

	#region CRServiceCaseDestinationAddressAttribute

	public class CRServiceCaseDestinationAddressAttribute : CRServiceCaseAddressAttribute
	{
		public CRServiceCaseDestinationAddressAttribute()
			: base(typeof(CRServiceCaseDestinationAddress.addressID), typeof(CRServiceCaseDestinationAddress.isDefaultAddress), SelectType)
		{
		}

		private static Type SelectType
		{
			get
			{
				return typeof (Select2<BAccountR,
					InnerJoin<Location,
						On<Location.bAccountID, Equal<BAccountR.bAccountID>,
							And<Location.locationID, Equal<BAccountR.defLocationID>>>,
						InnerJoin<Address,
							On<Address.bAccountID, Equal<BAccountR.bAccountID>,
								And<Address.addressID, Equal<BAccountR.defAddressID>>>,
						LeftJoin<CRServiceCaseDestinationAddress,
							On<CRServiceCaseDestinationAddress.customerID, Equal<Address.bAccountID>,
								And<CRServiceCaseDestinationAddress.customerAddressID, Equal<Address.addressID>,
								And<CRServiceCaseDestinationAddress.isDefaultAddress, Equal<True>>>>>>>,
					Where<BAccountR.bAccountID, Equal<Current<CRServiceCase.customerID>>>>);
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<CRServiceCaseDestinationAddress.overrideAddress>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<CRServiceCaseDestinationAddress, CRServiceCaseDestinationAddress.addressID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<CRServiceCaseDestinationAddress, CRServiceCaseDestinationAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<CRServiceCaseDestinationAddress>(sender, e);
			}
			finally
			{
				e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			}
		}

		protected override void Record_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.Record_RowSelected(sender, e);

			if (e.Row != null)
			{
				PXUIFieldAttribute.SetEnabled<CRServiceCaseDestinationAddress.overrideAddress>(sender, e.Row, true);
			}
		}
	}

	#endregion

	#region CRContactCacheNameAttribute

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class CRContactCacheNameAttribute : PX.Data.PXCacheNameAttribute
	{
		public CRContactCacheNameAttribute(string name) 
			: base(name)
		{
		}

		public override string GetName(object row)
		{
			var contact = row as Contact;
			if (contact == null) return base.GetName();
			
			var result = Messages.ContactType;
			switch (contact.ContactType)
			{
				case ContactTypesAttribute.Lead:
					result = Messages.Lead;
					break;
				/*case ContactTypesAttribute.Person:
					result = Messages.Person;
					break;
				case ContactTypesAttribute.SalesPerson:
					result = Messages.SalesPerson;
					break;
				case ContactTypesAttribute.BAccountProperty:
					result = Messages.BAccountProperty;
					break;*/
				case ContactTypesAttribute.Employee:
					result = Messages.Employee;
					break;
			}
			return result;
		}
	}

	#endregion

	#region CRCaseMajorStatusesAttribute

	public class CRCaseMajorStatusesAttribute : PXIntListAttribute
	{
		public const int _JUST_CREATED = -1;
		public const int _NEW = 0;
		public const int _PENDING_CUSTOMER = 1;
		public const int _OPEN = 2;
		public const int _CLOSED = 4;
		public const int _RELEASED = 5;
		public const int _PENDING_CLOSURE = 6;

		public CRCaseMajorStatusesAttribute()
			: base(new[] { _JUST_CREATED, _NEW, _PENDING_CUSTOMER, _OPEN, _CLOSED, _RELEASED, _PENDING_CLOSURE},
			new[] { "Just Created", "New", "Pending Customer", "Open", "Closed", "Released", "Pending Closure" })
		{

		}

		public sealed class justCreated : Constant<int>
		{
			public justCreated() : base(_JUST_CREATED) { }
		}

		public sealed class @new : Constant<int>
		{
			public @new() : base(_NEW) { }
		}

		public sealed class pendingCustomer : Constant<int>
		{
			public pendingCustomer() : base(_PENDING_CUSTOMER) { }
		}

		public class open : Constant<int>
		{
			public open() : base(_OPEN) { }
		}

		public class closed : Constant<int>
		{
			public closed(): base(_CLOSED){ }
		}

		public class released : Constant<int>
		{
			public released(): base(_RELEASED){ }
		}

		public class pendingClosure : Constant<int>
		{
			public pendingClosure(): base(_PENDING_CLOSURE){ }
		}
	}

	#endregion

	#region CRCaseStatusesAttribute

	public class CRCaseStatusesAttribute : PXStringListAttribute
	{
		public const string _NEW = "N";
		public const string _PENDING_CUSTOMER = "P";
		public const string _OPEN = "O";
		public const string _CLOSED = "C";
		public const string _RELSEASE = "R";

		public CRCaseStatusesAttribute()
			: base(new[] { _NEW, _PENDING_CUSTOMER, _OPEN, _CLOSED, _RELSEASE },
			new [] { "New", "Pending Customer", "Open", "Closed", "Released" })
		{
		}

		public sealed class Open : Constant<string>
		{
			public Open() : base(_OPEN) { }
		}

		public sealed class PendingCustomer : Constant<string>
		{
			public PendingCustomer() : base(_PENDING_CUSTOMER) { }
		}
		
		public sealed class New : Constant<string>
		{
			public New() : base(_NEW) { }
		}

		public sealed class Closed : Constant<string>
		{
			public Closed() : base(_CLOSED) { }
		}

		public sealed class Released : Constant<string>
		{
			public Released() : base(_RELSEASE) { }
		}
	}
	#endregion

	#region CRCaseResolutionsAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public sealed class CRCaseResolutionsAttribute : PXStringListAttribute
	{
		public const string _CUSTOMER_PRECLOSED = "CC";
		public const string _CUSTOMER_REPLIED = "AD";
		public const string _RESOLVED = "RD";
		public const string _ASSIGNED = "AS";
		public const string _UNASSIGNED = "UA";
		public const string _UPDATED = "AD";
		
		public CRCaseResolutionsAttribute()
			: base(new string[0], new string[0]) 			
		{
			
		}

		public sealed class CustomerPreclosed : Constant<string>
		{
			public CustomerPreclosed() : base(_CUSTOMER_PRECLOSED) { }
		}

		public sealed class CustomerReplied : Constant<string>
		{
			public CustomerReplied() : base(_CUSTOMER_REPLIED) { }
		}

		public sealed class Resolved : Constant<string>
		{
			public Resolved() : base(_RESOLVED) { }
		}

		public sealed class Assigned : Constant<string>
		{
			public Assigned() : base(_ASSIGNED) { }
		}

		public sealed class Unassigned : Constant<string>
		{
			public Unassigned() : base(_UNASSIGNED) { }
		}

		public sealed class Updated : Constant<string>
		{
			public Updated() : base(_UPDATED) { }
		}
	}

	#endregion

	#region OpportunityStatusAttribute
	public class OpportunityStatusAttribute : PXStringListAttribute
	{
		public OpportunityStatusAttribute()
			: base(new[] { "N", "O", "W", "L" },
			new[] { "New", "Open", "Won", "Lost" })
		{

		}

		public const string _NEW = "N";
		public const string _OPEN = "O";
		public const string _WON = "W";
		public const string _LOST = "L";

		public sealed class New : Constant<string>
		{
			public New() : base(_NEW) { }
		}

		public sealed class Open : Constant<string>
		{
			public Open() : base(_OPEN) { }
		}

		public sealed class Won : Constant<string>
		{
			public Won() : base(_WON) { }
		}

		public sealed class Lost : Constant<string>
		{
			public Lost() : base(_LOST) { }
		}
	}
	#endregion

	#region OpportunityStatusAttribute
	public class OpportunityResolusionAttribute : PXStringListAttribute
	{
		public OpportunityResolusionAttribute()
			: base(new[] { "NA", "AS", "RG", "IP", "FC", "CM", "PR", "OT", "CL", "TH", "RL" },
			new[] { "Unassign", "Assign", "Rejected", "In Process", "Functionality", "Company Maturity", "Price", "Other", "Cancelled", "Technology", "Relationship"})
		{

		}

		public const string _UNASSIGN = "NA";
		public const string _ASSIGN = "AS";
		public const string _REJECTED = "RG";
		public const string _INPROCESS = "IP";
		public const string _FUNCTIONALITY= "FC";
		public const string _COMPANYMATURITY = "CM";
		public const string _PRICE = "PR";
		public const string _OTHER = "OT";
		public const string _CANCELLED = "CL";
		public const string _TECHNOLOGY = "TH";
		public const string _RELATIONSHIP = "RL";

		public sealed class Unassign : Constant<string>
		{
			public Unassign() : base(_UNASSIGN) { }
		}

		public sealed class Assign : Constant<string>
		{
			public Assign() : base(_ASSIGN) { }
		}

		public sealed class Rejected : Constant<string>
		{
			public Rejected() : base(_REJECTED) { }
		}

		public sealed class InProcess : Constant<string>
		{
			public InProcess() : base(_INPROCESS) { }
		}

		public sealed class Functionality : Constant<string>
		{
			public Functionality() : base(_FUNCTIONALITY) { }
		}

		public sealed class CompanyMaturity : Constant<string>
		{
			public CompanyMaturity() : base(_COMPANYMATURITY) { }
		}

		public sealed class Price : Constant<string>
		{
			public Price() : base(_PRICE) { }
		}

		public sealed class Other : Constant<string>
		{
			public Other() : base(_OTHER) { }
		}

		public sealed class Cancelled : Constant<string>
		{
			public Cancelled() : base(_CANCELLED) { }
		}

		public sealed class Technology : Constant<string>
		{
			public Technology() : base(_TECHNOLOGY) { }
		}

		public sealed class Relationship : Constant<string>
		{
			public Relationship() : base(_RELATIONSHIP) { }
		}
	}
	#endregion

	#region OpportunityMajorStatusesAttribute
	public class OpportunityMajorStatusesAttribute : PXIntListAttribute
	{
		public OpportunityMajorStatusesAttribute()
			: base(new[] { -1, 0, 1, 2, 3, 4, 5, 6, 7, 8},
			new[] { "Just Created", "New", "Pending Customer", "Open", "Suspend", "Closed", "New Assigned", "New Rejected", "New Assigned To Owner", "New Rejected To Owner" })
		{

		}

		public const int _JUSTCREATED = -1;
		public const int _NEW = 0;
		public const int _PENDINGCUSTOMER = 1;
		public const int _OPEN = 2;
		public const int _SUSPEND = 3;
		public const int _CLOSED = 4;
		public const int _NEWASSIGNED = 5;
		public const int _NEWREJECTED = 6;
		public const int _NEWASSIGNEDTOOWNER = 7;
		public const int _NEWREGECTEDEDTOOWNER = 8;

		public sealed class justCreated : Constant<int>
		{
			public justCreated() : base(_JUSTCREATED) { }
		}

		public sealed class @new : Constant<int>
		{
			public @new() : base(_NEW) { }
		}

		public sealed class pendingCustomer : Constant<int>
		{
			public pendingCustomer() : base(_PENDINGCUSTOMER) { }
		}

		public sealed class open : Constant<int>
		{
			public open() : base(_OPEN) { }
		}

		public sealed class suspend : Constant<int>
		{
			public suspend() : base(_SUSPEND) { }
		}

		public sealed class closed : Constant<int>
		{
			public closed() : base(_CLOSED) { }
		}

		public sealed class newAssign : Constant<int>
		{
			public newAssign() : base(_NEWASSIGNED) { }
		}

		public sealed class newReject : Constant<int>
		{
			public newReject() : base(_NEWREJECTED) { }
		}

		public sealed class newAssignedToOwner : Constant<int>
		{
			public newAssignedToOwner() : base(_NEWASSIGNEDTOOWNER) { }
		}

		public sealed class NewRegectedToOwner : Constant<int>
		{
			public NewRegectedToOwner() : base(_NEWREGECTEDEDTOOWNER) { }
		}

	}
	#endregion
	
	#region CRCurrentOwnerIDAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class CRCurrentOwnerIDAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber
	{
		public virtual void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = EmployeeMaint.GetCurrentEmployeeID(sender.Graph);
		}
	}

	#endregion

	#region PXActivityApplicationList
	public class PXActivityApplicationAttribute : PXIntListAttribute
	{
		public const int Portal = 0;
		public const int Backend = 1;
		public PXActivityApplicationAttribute()
			: base(new int[] {Portal, Backend},
			       new string[] {Messages.Portal, Messages.Backend})
		{			
		}

		public class portal : Constant<int>{public portal() : base(Portal){}}
		public class backend : Constant<int> { public backend() : base(Backend) { } }
	}
	#endregion

	#region ActivityStatusListAttribute

	public class ActivityStatusListAttribute : PXStringListAttribute
	{
		public const string Draft = "DR";
		public const string Open = "OP";
		public const string Completed = "CD";
		public const string Approved = "AP";
		public const string Rejected = "RJ";
		public const string Canceled = "CL";
		public const string InProcess = "IP";

		public const string PendingApproval = "PA";
		public const string Released = "RL";


		public ActivityStatusListAttribute()
			: base(new[] { Draft, Open, InProcess, Completed, Approved, Rejected, Canceled, PendingApproval, Released },
				new[] { EP.Messages.Draft, EP.Messages.Open, EP.Messages.InProcess, EP.Messages.Complete, EP.Messages.Approved, EP.Messages.Rejected, EP.Messages.Canceled, EP.Messages.Balanced, EP.Messages.Released })
		{

		}

		public class draft : Constant<string>
		{
			public draft() : base(Draft) { }
		}

		public class open : Constant<string>
		{
			public open() : base(Open) { }
		}

		public class completed : Constant<string>
		{
			public completed() : base(Completed) { }
		}		
		public class approved : Constant<string>
		{
			public approved() : base(Approved) { }
		}

		public class rejected : Constant<string>
		{
			public rejected() : base(Rejected) { }
		}

		public class canceled : Constant<string>
		{
			public canceled() : base(Canceled) { }
		}

		public class pendingApproval : Constant<string>
		{
			public pendingApproval() : base(PendingApproval) { }
		}

		public class released : Constant<string>
		{
			public released() : base(Released) { }
		}

		public class inprocess : Constant<string>
		{
			public inprocess() : base(InProcess) { }
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
 			 base.FieldSelecting(sender, e);
			 PXStringState state = e.ReturnState as PXStringState;
			if (state == null) return;

			string[] allowedValues = AllowedState(sender, e.Row);
			if (allowedValues != null)
			{
				string[] allowedLabels = new string[allowedValues.Length];
				for (int i = 0; i < allowedValues.Length; i++)
				{
					int index = Array.IndexOf(state.AllowedValues, allowedValues[i]);
					allowedLabels[i] = state.AllowedLabels[index];
				}
				state.AllowedValues = allowedValues;
				state.AllowedLabels = allowedLabels;
			}			
		}

		protected virtual string[] AllowedState(PXCache sender, object row)
		{
			return null;
		}
		}

	#endregion

	#region ApprovalStatusListAttribute

	public class ApprovalStatusListAttribute : PXStringListAttribute
	{
		public const string NotRequired = "NR";
		public const string Approved = "AP";
		public const string Rejected = "RJ";
		public const string PartiallyApprove = "PR";
		public const string PendingApproval = "PA";

		public ApprovalStatusListAttribute()
			: base(new[] { NotRequired, Approved, Rejected, PartiallyApprove, PendingApproval },
				new[] { EP.Messages.NotRequired, EP.Messages.Approved, EP.Messages.Rejected, EP.Messages.PartiallyApprove, EP.Messages.PendingApproval })
		{
		}


		public class notRequired : Constant<string>
		{
			public notRequired() : base(NotRequired) { }
		}

		public class approved : Constant<string>
		{
			public approved() : base(Approved) { }
		}

		public class rejected : Constant<string>
		{
			public rejected() : base(Rejected) { }
		}

		public class partiallyApprove : Constant<string>
		{
			public partiallyApprove() : base(PartiallyApprove) { }
		}

		public class pendingApproval : Constant<string>
		{
			public pendingApproval() : base(PendingApproval) { }
		}

	}

	#endregion


	#region ActivityStatusAttribute
	public class ActivityStatusAttribute : ActivityStatusListAttribute
	{
		protected override string[] AllowedState(PXCache sender, object row)
		{
			var status = sender.GetValueOriginal(row, _FieldName);
			
			if (row != null && status == null)
				status = Open;

			switch ((string)status)
			{
				case Draft:
					return new string[] { Draft, Open, Canceled, Completed };
				case Open:
					return new string[] { Open, Canceled, Completed };
				case Canceled:
					return new string[] { Open, Canceled };
				case Completed:
					return new string[] { Open, Completed };
				case Approved:
					return new string[] { Open, Canceled, Completed, Approved };
				case Rejected:
					return new string[] { Open, Canceled, Completed, Rejected };
			}
			return base.AllowedState(sender, row);
		}
		}
	#endregion

	#region TaskStatusAttribute
	public class TaskStatusAttribute : ActivityStatusListAttribute
	{
		protected override string[] AllowedState(PXCache sender, object row)
		{
			var status = sender.GetValueOriginal(row, _FieldName);

			if (row != null && status == null)
				status = Open;

			switch ((string)status)
			{
				case Draft:
					return new string[] { Draft, Open, Canceled };
				case Open:
					return new string[] { Open, Draft, InProcess, Canceled, Completed };
				case Canceled:
					return new string[] { Open, InProcess, Canceled };
				case Completed:
					return new string[] { Open, InProcess, Completed };
			}
			return base.AllowedState(sender, row);
		}
	}
	#endregion

	#region MailStatusListAttribute

	public class MailStatusListAttribute : PXStringListAttribute
	{
		public const string Draft = "DR";
		public const string PreProcess = "PP";
		public const string InProcess = "IP";
		public const string Processed = "PD";
		public const string Failed = "FL";
		public const string Canceled = "CL";
		public const string Deleted = "DL";

		public MailStatusListAttribute()
			: base(new[] { Draft, PreProcess, InProcess, Processed, Canceled, Failed, Deleted },
				new[] { EP.Messages.Draft, EP.Messages.PreProcess, EP.Messages.InProcess, 
					EP.Messages.Processed, EP.Messages.Canceled, EP.Messages.Failed, 
					EP.Messages.EmailDeleted})
		{
		}


		public class draft : Constant<string>
		{
			public draft() : base(Draft) { }
		}
		public class preProcess : Constant<string>
		{
			public preProcess() : base(PreProcess) { }
		}
		public class inProcess : Constant<string>
		{
			public inProcess() : base(InProcess) { }
		}

		public class processed : Constant<string>
		{
			public processed() : base(Processed) { }
		}

		public class canceled : Constant<string>
		{
			public canceled() : base(Canceled) { }
		}

		public class failed : Constant<string>
		{
			public failed() : base(Failed) { }
		}

		public class deleted : Constant<string>
		{
			public deleted() : base(Deleted) { }
		}
	}

	#endregion

	#region RefTaskSelectorAttribute

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RefTaskSelectorAttribute : PXSelectorAttribute
    {
        public RefTaskSelectorAttribute() :
            base(typeof(Search<EPActivity.taskID, Where<EPActivity.classID, Equal<CRActivityClass.task>>>),
                typeof(EPActivity.taskID), typeof(EPActivity.subject),
                typeof(EPActivity.startDate), typeof(EPActivity.endDate),
                typeof(EPActivity.owner))
        {
        }

        public RefTaskSelectorAttribute(Type Field) :
            base(BqlCommand.Compose(typeof(Search5<,,,>),
                                        typeof(EPActivity.taskID),
                                        typeof(LeftJoin<,>),
                                            typeof(CRActivityRelation),
                                            typeof(On<,>),
                                                typeof(CRActivityRelation.refTaskID),
                                                typeof(Equal<>),
                                                    typeof(EPActivity.taskID),
                                        typeof(Where<,,>),
                                            typeof(EPActivity.classID),
                                            typeof(Equal<>),
                                                typeof(CRActivityClass.task),
                                            typeof(And<,,>),
                                                typeof(EPActivity.taskID),
                                                typeof(NotEqual<>),
                                                   typeof(Current<>),
                                                       Field,
                                                typeof(And<>),
                                                    typeof(Where<,,>),
                                                        typeof(CRActivityRelation.taskID),
                                                        typeof(NotEqual<>),
                                                           typeof(Current<>),
                                                               Field,
                                                        typeof(Or<,>),
                                                            typeof(CRActivityRelation.taskID),
                                                            typeof(IsNull),
                                        typeof(Aggregate<>),
                                            typeof(GroupBy<,>),
                                                typeof(EPActivity.taskID),
                                                typeof(GroupBy<,>),
                                                    typeof(EPActivity.subject),
                                                    typeof(GroupBy<,>),
                                                        typeof(EPActivity.startDate),
                                                        typeof(GroupBy<,>),
                                                            typeof(EPActivity.endDate),
                                                            typeof(GroupBy<>),
                                                                typeof(EPActivity.owner)
            ),
                typeof(EPActivity.taskID), typeof(EPActivity.subject),
                typeof(EPActivity.startDate), typeof(EPActivity.endDate),
                typeof(EPActivity.owner))
        {
        }
    }

	#endregion

	#region MergeMethodsAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class MergeMethodsAttribute : PXIntListAttribute
	{
		public const int _FIRST = 0;
		public const int _SELECT = 1;
		public const int _CONCAT = 2;
		public const int _SUM = 3;
		public const int _MAX = 4;
		public const int _MIN = 5;
		public const int _COUNT = 6;

		private const string _FIRST_LABEL = "First";
		private const string _SELECT_LABEL = "Select";
		private const string _CONCAT_LABEL = "Concat";
		private const string _SUM_LABEL = "Sum";
		private const string _MAX_LABEL = "Max";
		private const string _MIN_LABEL = "Min";
		private const string _COUNT_LABEL = "Count";

		private static readonly int[] _NUMBER_VALUES = 
			new[] { _FIRST, _SELECT, _SUM, _MAX, _MIN, _COUNT };
		private static readonly string[] _NUMBER_LABELS = 
			new[] { _FIRST_LABEL, _SELECT_LABEL, _SUM_LABEL, _MAX_LABEL, _MIN_LABEL, _COUNT_LABEL };

		private static readonly int[] _STRING_VALUES =
			new[] { _FIRST, _SELECT, _CONCAT };
		private static readonly string[] _STRING_LABELS =
			new[] { _FIRST_LABEL, _SELECT_LABEL, _CONCAT_LABEL };

		private static readonly int[] _DATE_VALUES =
			new[] { _FIRST, _SELECT, _MAX, _MIN };
		private static readonly string[] _DATE_LABELS =
			new[] { _FIRST_LABEL, _SELECT_LABEL, _MAX_LABEL, _MIN_LABEL };

		private static readonly int[] _COMMON_VALUES =
			new[] { _FIRST, _SELECT };
		private static readonly string[] _COMMON_LABELS =
			new[] { _FIRST_LABEL, _SELECT_LABEL };

		public MergeMethodsAttribute()
			: base(new [] { _FIRST, _SELECT, _CONCAT, _SUM, _MAX, _MIN, _COUNT }, 
			new [] { _FIRST_LABEL, _SELECT_LABEL, _CONCAT_LABEL, _SUM_LABEL, _MAX_LABEL, _MIN_LABEL, _COUNT_LABEL })
		{
			
		}

		public static void SetNumberList<TField>(PXCache cache, object row) 
			where TField : IBqlField
		{
			PXIntListAttribute.SetList<TField>(cache, row, _NUMBER_VALUES, _NUMBER_LABELS);
		}

		public static void SetStringList<TField>(PXCache cache, object row) 
			where TField : IBqlField
		{
			PXIntListAttribute.SetList<TField>(cache, row, _STRING_VALUES, _STRING_LABELS);
		}

		public static void SetDateList<TField>(PXCache cache, object row) 
			where TField : IBqlField
		{
			PXIntListAttribute.SetList<TField>(cache, row, _DATE_VALUES, _DATE_LABELS);
		}

		public static void SetCommonList<TField>(PXCache cache, object row) 
			where TField : IBqlField
		{
			PXIntListAttribute.SetList<TField>(cache, row, _COMMON_VALUES, _COMMON_LABELS);
		}
	}

	#endregion

	#region MergableTypesSelectorAttribute

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [Serializable]
	public sealed class MergableTypesSelectorAttribute : PXCustomSelectorAttribute
	{
		#region EntityInfo

        [Serializable]
		public class EntityInfo : IBqlTable
		{
			#region Key

			public abstract class key : IBqlField
			{
			}

			[PXString(IsKey = true)]
			[PXUIField(Visible = false)]
			public virtual String Key { get; set; }

			#endregion

			#region Name

			public abstract class name : IBqlField
			{
			}

			[PXString(IsUnicode = true)]
			[PXUIField(DisplayName = "Name")]
			public virtual String Name { get; set; }

			#endregion
		}

		#endregion

		public MergableTypesSelectorAttribute()
			: base(typeof(EntityInfo.key), typeof(EntityInfo.name))
		{
			base.DescriptionField = typeof (EntityInfo.name);
		}

		public override Type DescriptionField
		{
			get
			{
				return base.DescriptionField;
			}
			set
			{
			}
		}

		public IEnumerable GetRecords()
		{
			//TODO: need implement collection types by special marker attribute
			yield return GenerateInfo(typeof(Contact));
			yield return GenerateInfo(typeof(BAccount));
			yield return GenerateInfo(typeof(CRCampaign));
		}

		public override void DescriptionFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, string alias)
		{
			var deleted = false;
			if (e.Row != null)
			{
				var infoKey =  sender.GetValue(e.Row, _FieldOrdinal) as string;
				var infoType = string.IsNullOrEmpty(infoKey) ? null : System.Web.Compilation.BuildManager.GetType(infoKey, false);
				if (infoType == null)
				{
					deleted = true;
					e.ReturnValue = infoKey;
				}
				else
					e.ReturnValue = GenerateInfo(infoType).Name;
			}

			if (e.Row == null || e.IsAltered)
			{
				int? length;
				string displayname = getDescriptionName(sender, out length);
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(string), false, true, null, null, length, null,
					alias, null, displayname, deleted ? ErrorMessages.ForeignRecordDeleted : null, 
					deleted ? PXErrorLevel.Warning : PXErrorLevel.Undefined, false, true, null, PXUIVisibility.Invisible, null, null, null);
			}
		}

		private EntityInfo GenerateInfo(Type type)
		{
			var displayName = EntityHelper.GetFriendlyEntityName(type);
			return new EntityInfo { Key = type.GetLongName(), Name = displayName };
		}
	}

	#endregion

	#region MergeMatchingTypesAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class MergeMatchingTypesAttribute : PXIntListAttribute
	{
		public const int _EQUALS_TO = 0;
		public const int _LIKE = 1;
		public const int _THE_SAME = 2;
		public const int _GREATER_THAN = 3;
		public const int _LESS_THAN = 4;

		private static readonly int[] _commonValues;
		private static readonly string[] _commonLabels;

		private static readonly int[] _comparableValues;
		private static readonly string[] _comparableLabels;

		static MergeMatchingTypesAttribute()
		{
			_commonValues = new[] { _EQUALS_TO, _LIKE, _THE_SAME };
			_commonLabels = new[] { "Equals To", "Like", "The Same" };
			_comparableValues = new[] { _EQUALS_TO, _LIKE, _THE_SAME, _GREATER_THAN, _LESS_THAN };
			_comparableLabels = new[] { "Equals To", "Like", "The Same", "Greater Than", "Less Than" };
		}

		public MergeMatchingTypesAttribute()
			: base(CommonValues, CommonLabels)
		{
			
		}

		public static int[] CommonValues
		{
			get { return _commonValues; }
		}

		public static string[] CommonLabels
		{
			get { return _commonLabels; }
		}

		public static int[] ComparableValues
		{
			get { return _comparableValues; }
		}

		public static string[] ComparableLabels
		{
			get { return _comparableLabels; }
		}
	}

	#endregion

	#region CaseSourcesAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public sealed class CaseSourcesAttribute : PXStringListAttribute
	{
		public const string _EMAIL = "EM";
		public const string _PHONE = "PH";
		public const string _WEB = "WB";
		public const string _CHAT = "CH";

		public CaseSourcesAttribute()
			: base(new [] { _EMAIL, _PHONE, _WEB, _CHAT }, 
					new [] { Messages.CaseSourceEmail, Messages.CaseSourcePhone, Messages.CaseSourceWeb, Messages.CaseSourceChat })
		{}

		public sealed class Email : Constant<string>
		{
			public Email() : base(_EMAIL) { }
		}

		public sealed class Phone : Constant<string>
		{
			public Phone() : base(_PHONE) { }
		}

		public sealed class Web : Constant<string>
		{
			public Web() : base(_WEB) { }
		}

		public sealed class Chat : Constant<string>
		{
			public Chat() : base(_CHAT) { }
		}
	}

	#endregion

	#region PXCheckCurrentAttribute

	public class PXCheckCurrentAttribute : PXViewExtensionAttribute
	{
		private string _hostViewName;

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			_hostViewName = viewName;

			graph.Initialized += sender =>
			{
				var cache = GetCache(sender);
				var record = cache.Current;
				if (record == null)
				{
					var itemType = cache.GetItemType();
					string name = itemType.Name;
					if (itemType.IsDefined(typeof(PXCacheNameAttribute), true))
					{
						PXCacheNameAttribute attr = (PXCacheNameAttribute)(itemType.GetCustomAttributes(typeof(PXCacheNameAttribute), true)[0]);
						name = attr.GetName();
					}
                    throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, itemType, name);
				}
			};
		}

		private PXCache GetCache(PXGraph graph)
		{
			return graph.Views[_hostViewName].Cache;
		}
	}

	#endregion

	#region CheckActivitiesOnDeleteAttribute

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class CheckActivitiesOnDeleteAttribute : CRActivityListBaseAttribute
	{
		protected override void AttachHandlers(PXGraph graph, Type selectorDAC, Type commandDAC)
		{
			var primaryDAC = ((IActivityList)GraphSelector).PrimaryViewType;
			graph.RowDeleting.AddHandler(primaryDAC, Handler);
		}

		private void Handler(PXCache sender, PXRowDeletingEventArgs e)
		{
			if (!((IActivityList)GraphSelector).CheckActivitesForDelete(e.Row))
			{
				e.Cancel = true;
				throw new PXException(Messages.LeadCannotBeDeleted);
			}
		}
	}

	#endregion

	#region CRLeadFullNameAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	[PXDBString(255, IsUnicode = true)]
	public class CRLeadFullNameAttribute : PXAggregateAttribute
	{
		private readonly Type _accountIdBqlField;

		private int _accountIdfieldOrdinal;

		public CRLeadFullNameAttribute(Type accountIdBqlField)
		{
			if (accountIdBqlField == null) throw new ArgumentNullException("accountIdBqlField");

			_accountIdBqlField = accountIdBqlField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			var itemType = sender.GetItemType();
			if (!typeof(Contact).IsAssignableFrom(itemType))
				throw new Exception(string.Format("Attribute '{0}' can be used only with DAC '{1}' or its inheritors",
					GetType().Name, typeof(Contact).Name));

			_accountIdfieldOrdinal = GetFieldOrdinal(sender, itemType, _accountIdBqlField);

			sender.Graph.RowSelected.AddHandler(itemType, Handler);
		}

		private int GetFieldOrdinal(PXCache sender, Type itemType, Type bqlField)
		{
			var fieldName = sender.GetField(bqlField);
			if (string.IsNullOrEmpty(fieldName))
				throw new Exception(string.Format("Field '{0}' can not be not found in table '{1}'",
					bqlField.Name, itemType.Name));
			return sender.GetFieldOrdinal(fieldName);
		}

		private void Handler(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			var accountId = sender.GetValue(e.Row, _accountIdfieldOrdinal);
			if (accountId != null)
			{
				PXUIFieldAttribute.SetEnabled<Contact.fullName>(sender, e.Row, false);

				var account = (BAccount)PXSelect<BAccount,
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(sender.Graph, accountId);
				var newValue = account.With(_ => _.AcctName);
				sender.SetValue(e.Row, _FieldOrdinal, newValue);
			}
		}
	}

	#endregion

	#region ContactSelectorAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class ContactSelectorAttribute : PXCustomSelectorAttribute
	{
		private readonly Type[] _contactTypes;

		private PXView _view;

		public ContactSelectorAttribute(params Type[] contactTypes)
			: base(typeof(Contact.contactID))
		{
			if (contactTypes == null) throw new ArgumentNullException("contactTypes");
			if (contactTypes.Length == 0) throw new ArgumentOutOfRangeException("contactTypes");
			_contactTypes = contactTypes;

			base.DescriptionField = typeof(Contact.displayName);
			base.Filterable = true;
		}

		public override Type DescriptionField
		{
			get
			{
				return base.DescriptionField;
			}
			set
			{
			}
		}

		public override bool Filterable
		{
			get
			{
				return base.Filterable;
			}
			set
			{
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			var command = BqlCommand.CreateInstance(
				typeof(Select2<,>), typeof(Contact),
				typeof(LeftJoin<,>), typeof(BAccount),
					typeof(On<,>), typeof(BAccount.bAccountID), typeof(Equal<>), typeof(Contact.bAccountID));
			foreach (Type contactType in _contactTypes)
			{
				command = command.WhereOr(BqlCommand.Compose(typeof(Where<,>), typeof(Contact.contactType), typeof(Equal<>), contactType));
			}
			_view = new PXView(sender.Graph, true, command);
			_Select = command;
		}

		public IEnumerable GetRecords()
		{
			var startRow = PXView.StartRow;
			int totalRows = 0;
			var list = _view.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns,
				PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;

			foreach (PXResult<Contact, BAccount> row in list)
			{
				var contact = (Contact)row;
				var account = (BAccount)row;
				if (contact.BAccountID != null)
					contact.FullName = account.AcctName;

				yield return row;
			}
		}
	}

	#endregion

	#region AssignedDateAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	[PXDBDate(PreserveTime = true)]
	public class AssignedDateAttribute : PXAggregateAttribute
	{
		private readonly Type _ownerBqlField;

		private int _ownerFieldOrdinal;

		public AssignedDateAttribute(Type ownerBqlField)
		{
			if (ownerBqlField == null) throw new ArgumentNullException("ownerBqlField");
			_ownerBqlField = ownerBqlField;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			var itemType = sender.GetItemType();
			var fieldName = sender.GetField(_ownerBqlField);
			if (string.IsNullOrEmpty(fieldName))
				throw new Exception(string.Format("Field '{0}' can not be not found in table '{1}'",
					_ownerBqlField.Name, itemType.Name));
			_ownerFieldOrdinal = sender.GetFieldOrdinal(fieldName);

			sender.Graph.RowInserted.AddHandler(itemType, RowInsertedHandler);
			sender.Graph.RowUpdated.AddHandler(itemType, RowUpdatedHandler);
		}

		private void RowUpdatedHandler(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var owner = sender.GetValue(e.Row, _ownerFieldOrdinal);
			if (owner == null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, null);
			}
			else
			{
				var oldOwner = sender.GetValue(e.OldRow, _ownerFieldOrdinal);
				if (oldOwner != owner)
					sender.SetValue(e.Row, _FieldOrdinal, PXTimeZoneInfo.Now);
			}
		}

		private void RowInsertedHandler(PXCache sender, PXRowInsertedEventArgs e)
		{
			object newValue = null;
			var owner = sender.GetValue(e.Row, _ownerFieldOrdinal);
			if (owner != null)
				newValue = PXTimeZoneInfo.Now;
			sender.SetValue(e.Row, _FieldOrdinal, newValue);
		}
	}

	#endregion

	#region CRAttributesFieldAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public sealed class CRAttributesFieldAttribute : PXDBAttributeAttribute
	{
		public CRAttributesFieldAttribute(Type entityType, Type entityIdField, Type classIdField)
			: base(GetValueSearchCommand(entityType, entityIdField),
				typeof(CSAnswers.attributeID),
				GetAttributesSearchCommand(entityType, classIdField))
		{
			
		}

		private static Type GetAttributesSearchCommand(Type entityType, Type classIdField)
		{
			return BqlCommand.Compose(typeof(Search2<,,>), typeof(CSAttribute.attributeID),
				typeof(InnerJoin<,>), typeof(CSAttributeGroup), 
					typeof(On<,>), typeof(CSAttributeGroup.attributeID), typeof(Equal<>), typeof(CSAttribute.attributeID),
				typeof(Where<,,>), typeof(CSAttributeGroup.type), typeof(Equal<>), entityType,
					typeof(And<,>), typeof(CSAttributeGroup.entityClassID), typeof(Equal<>), typeof(Current<>), classIdField);
		}

		private static Type GetValueSearchCommand(Type entityType, Type entityIdField)
		{
			return BqlCommand.Compose(typeof(Search<,>), typeof(CSAnswers.value),
				typeof(Where<,,>), typeof(CSAnswers.entityType), typeof(Equal<>), entityType, 
					typeof(And<,>), typeof(CSAnswers.entityID), typeof(Equal<>), entityIdField);
		}
	}

	#endregion

	#region CROpportunityStagesAttribute

	public class CROpportunityStagesAttribute : PXStringListAttribute
	{
		public const string _PROSPECT = "L";
		public const string _NURTURE = "N";
		public const string _QUALIFY = "P";
		public const string _DEVELOP = "Q";
		public const string _SOLUTION = "V";
		public const string _PROOF = "A";
		public const string _CLOSE = "R";
		public const string _DEPLOY = "W";

		public CROpportunityStagesAttribute()
			: base(new string[] { _PROSPECT,_NURTURE, _QUALIFY, _DEVELOP, _SOLUTION, _PROOF, _CLOSE, _DEPLOY },
			new string[] { "Prospect", "Nurture", "Qualify", "Develop", "Solution", "Proof", "Close", "Deploy" })
		{
			
		}

		public sealed class Deploy : Constant<string>
		{
			public Deploy()
				: base(_DEPLOY)
			{
				
			}
		}
	}

	#endregion	

	#region CRCaseBillableTimeAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	internal sealed class CRCaseBillableTimeAttribute : PXEventSubscriberAttribute
	{
		private PXGraph _graph;

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_graph = sender.Graph;
			_graph.RowSelected.AddHandler<CRCase>((s, e) => CalculateBillableTime(e.Row as CRCase));
		}

		private void CalculateBillableTime(CRCase caseRow)
		{
			if (caseRow == null) return;

			var billPerActivities = false;
			CRCaseClass caseClass = (CRCaseClass)PXSelectorAttribute.Select<CRCase.caseClassID>(_graph.Caches[typeof(CRCase)], caseRow);
			if (caseClass != null)
				billPerActivities = caseClass.PerItemBilling == BillingTypeListAttribute.PerActivity;

			var recalcBillableTimes = billPerActivities || caseRow.ManualBillableTimes != true;
			if (!recalcBillableTimes) return;

			caseRow.TimeSpent = 0;
			caseRow.OvertimeSpent = 0;
			caseRow.TimeBillable = 0;
			caseRow.OvertimeBillable = 0;

			foreach (EPActivity activity in PXSelectReadonly<EPActivity,
				Where<EPActivity.refNoteID, Equal<Required<EPActivity.refNoteID>>, And<EPActivity.isCorrected, NotEqual<True>, 
					And<Where<EPActivity.uistatus, Equal<ActivityStatusAttribute.completed>, Or<EPActivity.uistatus, Equal<ActivityStatusAttribute.approved>>>>>>>.
				Select(_graph, caseRow.NoteID))
			{
				caseRow.TimeSpent += activity.TimeSpent.GetValueOrDefault();
				caseRow.OvertimeSpent += activity.OvertimeSpent.GetValueOrDefault();

				var isBillable = activity.IsBillable == true;
				if (isBillable)
				{
					int timeBillable = activity.TimeBillable ?? 0;
					int overtimeBillable = activity.OvertimeBillable ?? 0;
					if (caseClass != null && caseClass.RoundingInMinutes > 1)
					{
						if (timeBillable > 0)
						{
							decimal fraction = Convert.ToDecimal(timeBillable)/Convert.ToDecimal(caseClass.RoundingInMinutes);
							int points = Convert.ToInt32(Math.Ceiling(fraction));
							timeBillable = points * (caseClass.RoundingInMinutes ?? 0);

							if (caseClass.MinBillTimeInMinutes > 0)
							{
								timeBillable = Math.Max(timeBillable, (int)caseClass.MinBillTimeInMinutes);
							}
						}

						if (overtimeBillable > 0)
						{
							decimal fraction = Convert.ToDecimal(overtimeBillable)/Convert.ToDecimal(caseClass.RoundingInMinutes);
							int points = Convert.ToInt32(Math.Ceiling(fraction));
							overtimeBillable = points * (caseClass.RoundingInMinutes ?? 0);

							if (caseClass.MinBillTimeInMinutes > 0)
							{
								overtimeBillable = Math.Max(overtimeBillable, (int)caseClass.MinBillTimeInMinutes);
							}
						}
					}

					caseRow.TimeBillable += timeBillable;
					caseRow.OvertimeBillable += overtimeBillable;
				}
			}
		}
	}

	#endregion

	#region CRDropDownAutoValueAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public sealed class CRDropDownAutoValueAttribute : PXEventSubscriberAttribute
	{
		private readonly Type _refField;
		private int _refFieldOrdinal;
		private BqlCommand _bqlCommand;

		public CRDropDownAutoValueAttribute(Type dependsOnField)
		{
			if (dependsOnField == null)
				throw new ArgumentNullException("dependsOnField");
			if (!typeof(IBqlField).IsAssignableFrom(dependsOnField))
				throw new ArgumentException(string.Format("'{0}' is expected.", typeof(IBqlField).GetLongName()));

			_refField = dependsOnField;
		}

		public bool CheckOnInsert { get; set; }

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_refFieldOrdinal = sender.GetFieldOrdinal(sender.GetField(_refField));
			if (CheckOnInsert) sender.Graph.RowInserting.AddHandler(_BqlTable, RowInsertingHandler);
			sender.Graph.RowUpdating.AddHandler(_BqlTable, RowUpdatingHandler);

			_bqlCommand = BqlCommand.CreateInstance(typeof(Select<>), _BqlTable);
		}

		private void RowInsertingHandler(PXCache sender, PXRowInsertingEventArgs e)
		{
			var currentValue = sender.GetValue(e.Row, _FieldOrdinal);

			var graph = sender.Graph;
		    if (!string.IsNullOrEmpty(graph.AutomationView))
		    {
		        if (graph.Views[graph.AutomationView].GetItemType() != sender.GetItemType()) return;
		    }

		    var oldStep = graph.AutomationStep;
			PXAutomation.GetStep(graph, new List<object> { e.Row }, _bqlCommand);

			sender.SetValue(e.Row, _FieldOrdinal, null);
			PXAutomation.ApplyStep(graph, e.Row, false);
		    
			var state = sender.GetStateExt(e.Row, _FieldName) as PXStringState;
			sender.SetValue(e.Row, _FieldOrdinal, currentValue);
			if (state != null)
			{
				var allowedValues = state.AllowedValues;
				if (allowedValues != null && (string.IsNullOrEmpty(currentValue as string) || Array.IndexOf(allowedValues, (string)currentValue) < 0))
					sender.SetValue(e.Row, _FieldOrdinal, allowedValues.FirstOrDefault());
			}
			else if (currentValue != null)
			{
				sender.SetValue(e.Row, _FieldOrdinal, null);
			}

			graph.AutomationStep = oldStep;
		}

		private void RowUpdatingHandler(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var oldRefValue = sender.GetValue(e.Row, _refFieldOrdinal);
			var newRefValue = sender.GetValue(e.NewRow, _refFieldOrdinal);

			if (!Object.Equals(oldRefValue ,newRefValue))
			{
				var graph = sender.Graph;

				var oldStep = graph.AutomationStep;
				PXAutomation.GetStep(graph, new List<object> { e.NewRow }, _bqlCommand);

				var currentValue = sender.GetValue(e.NewRow, _FieldOrdinal);
				sender.SetValue(e.NewRow, _FieldOrdinal, null);

				PXAutomation.ApplyStep(graph, e.NewRow, false);

				var state = sender.GetStateExt(e.NewRow, _FieldName) as PXStringState;
				sender.SetValue(e.NewRow, _FieldOrdinal, currentValue);
				if (state != null)
				{
					var allowedValues = state.AllowedValues;
					if (allowedValues != null && (string.IsNullOrEmpty(currentValue as string) || Array.IndexOf(allowedValues, (string)currentValue) < 0))
						sender.SetValue(e.NewRow, _FieldOrdinal, allowedValues.FirstOrDefault());
				}
				else if (sender.GetValue(e.NewRow, _FieldOrdinal) == sender.GetValue(e.Row, _FieldOrdinal))
				{
					sender.SetValue(e.NewRow, _FieldOrdinal, null);
				}

				graph.AutomationStep = oldStep;
			}
		}
	}
	#endregion

	#region PXUIBaseStateAttribute
	public class PXBaseConditionAttribute : PXEventSubscriberAttribute
	{
		protected IBqlCreator _Condition;

		public virtual Type Condition
		{
			get
			{
				return _Condition.GetType();
			}
			set
			{
				_Condition = PXFormulaAttribute.InitFormula(value);
			}
		}

		public PXBaseConditionAttribute()
			: base()
		{
		}

		public PXBaseConditionAttribute(Type conditionType)
			: this()
		{
			_Condition = PXFormulaAttribute.InitFormula(conditionType);
		}

		public override void CacheAttached(PXCache sender)
		{
			sender.DisableCloneAttributes = true;
		}

		protected static bool GetConditionResult(PXCache sender, object row, Type conditionType)
		{
			IBqlCreator condition = PXFormulaAttribute.InitFormula(conditionType);
			bool? result = null;
			object value = null;
			BqlFormula.Verify(sender, row, condition, ref result, ref value);
			return (value as bool?) == true;
		}

		protected static Type GetCondition<AttrType, Field>(PXCache sender, object row)
			where AttrType : PXBaseConditionAttribute
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributes<Field>())
			{
				if (attr is AttrType)
				{
					return ((AttrType)attr).Condition;
				}
			}
			return null;
		}

		protected static Type GetCondition<AttrType>(PXCache sender, object row, string fieldName)
			where AttrType:PXBaseConditionAttribute
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributes(row, fieldName))
			{
				if (attr is AttrType)
				{
					return ((AttrType)attr).Condition;
				}
			}
			return null;
		}

	}
	#endregion

	public sealed class CRDuplicateContactsSelectorAttribute : PXCustomSelectorAttribute
	{
		private readonly Type ContactID;
		public CRDuplicateContactsSelectorAttribute(Type contactID)
			: base(typeof(Search2<Contact.contactID, 
				InnerJoin<CRDuplicateRecord, On<Contact.contactID, Equal<CRDuplicateRecord.duplicateContactID>>>,				 
				Where<CRDuplicateRecord.contactID, Equal<Optional<Contact.contactID>>,
					And<CRDuplicateRecord.score, GreaterEqual<Current<CRSetup.leadValidationThreshold>>,
					And<CRDuplicateRecord.validationType, Equal<ValidationTypesAttribute.leadContact>,
					And<Contact.duplicateStatus, NotEqual<DuplicateStatusAttribute.duplicated>,
				  And<Contact.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>>>>>>>),
			typeof(Contact.displayName),
			typeof(Contact.contactType),
			typeof(Contact.salutation),
			typeof(Contact.eMail),
			typeof(Contact.phone1)
			)
		{
			DirtyRead = true;
			DescriptionField = typeof (Contact.displayName);
			this.ContactID = contactID;
			ValidateValue = false;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			View = new PXView(_Graph, false, _Select);
		}

		private PXView View;

		public IEnumerable GetRecords()
		{
			PXCache sourceCache = _Graph.Caches[ContactID.DeclaringType];
			int? currentID = (int?) sourceCache.GetValue(sourceCache.Current, ContactID.Name);
			foreach (PXResult<Contact, CRDuplicateRecord> result in View.SelectMulti(currentID))
			{
				Contact c = result;
				CRDuplicateRecord d = result;
				d = (CRDuplicateRecord)_Graph.Caches[typeof(CRDuplicateRecord)].Locate(d);

				if ((d != null && d.Selected == true) || c.ContactID == currentID)
					yield return c;
			}
		}
	}

	public sealed class CRDuplicateBAccountSelectorAttribute : PXCustomSelectorAttribute
	{
		private readonly Type BAccountID;
		public CRDuplicateBAccountSelectorAttribute(Type bAccountID)
			:base(typeof(Search2<BAccount.bAccountID,
			  InnerJoin<CRLeadContactValidationProcess.Contact2,
			         On<BAccount.bAccountID, Equal<CRLeadContactValidationProcess.Contact2.bAccountID>,
 							And<BAccount.defContactID, Equal<CRLeadContactValidationProcess.Contact2.contactID>>>,			  
				InnerJoin<CRDuplicateRecord,
							On<CRDuplicateRecord.duplicateContactID, Equal<CRLeadContactValidationProcess.Contact2.contactID>>,
 			  InnerJoin<BAccountR,
							On<BAccountR.defContactID, Equal<CRDuplicateRecord.contactID>>>>>,
				Where<BAccountR.bAccountID, Equal<Optional<BAccount.bAccountID>>,				 
				 And<CRLeadContactValidationProcess.Contact2.duplicateStatus, NotEqual<DuplicateStatusAttribute.duplicated>, 
				 And<CRDuplicateRecord.score, GreaterEqual<Current<CRSetup.accountValidationThreshold>>,
				 And<CRDuplicateRecord.validationType, Equal<ValidationTypesAttribute.account>>>>>>),
			typeof(BAccount.acctCD),
			typeof(BAccount.acctName),
			typeof(BAccount.classID)
			)
		{
			DirtyRead = true;
			DescriptionField = typeof(BAccount.acctName);
			SubstituteKey = typeof (BAccount.acctCD);
			BAccountID = bAccountID;
			ValidateValue = false;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			View = new PXView(_Graph, false, _Select);
		}

		private PXView View;

		public IEnumerable GetRecords()
		{
			PXCache sourceCache = _Graph.Caches[BAccountID.DeclaringType];
			int? currentID = (int?)sourceCache.GetValue(sourceCache.Current, BAccountID.Name);
			foreach (PXResult<BAccount, CRLeadContactValidationProcess.Contact2, CRDuplicateRecord> result in View.SelectMulti(currentID))
			{
				BAccount b = result;
				CRDuplicateRecord d = result;
				d = (CRDuplicateRecord)_Graph.Caches[typeof(CRDuplicateRecord)].Locate(d);

				if ((d != null && d.Selected == true) || b.BAccountID == currentID)
					yield return b;
			}
		}
	}


	#region PXUIRequiredAttribute
	public class PXUIRequiredAttribute : PXBaseConditionAttribute
	{

		public PXUIRequiredAttribute(Type conditionType)
			: base(conditionType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			sender.Graph.RowPersisting.AddHandler(sender.GetItemType(), GraphRowPersisting);
			sender.Graph.RowSelecting.AddHandler(sender.GetItemType(), GraphRowSelecting);
			base.CacheAttached(sender);
		}

		public virtual void GraphRowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			SetPersistingCheck(sender, e.Row, _FieldName, Condition);
		}

		public virtual void GraphRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			SetPersistingCheck(sender, e.Row, _FieldName, Condition);
		}

		public static void SetPersistingCheck(PXCache sender, object row, string fieldName, Type conditionType)
		{
			if (row == null || conditionType == null) 
				return;
			PXDefaultAttribute.SetPersistingCheck(sender, fieldName, row, GetConditionResult(sender, row, conditionType) ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}

		public static void SetPersistingCheck(PXCache sender, object row, string fieldName)
		{
			if (row == null)
				return;
			SetPersistingCheck(sender, row, fieldName, GetCondition<PXUIRequiredAttribute>(sender, row, fieldName));
		}

		public static void SetPersistingCheck<Field>(PXCache sender, object row, Type conditionType)
			where Field : IBqlField
		{
			if (row == null || conditionType == null)
				return;
			SetPersistingCheck(sender, row, typeof(Field).Name, conditionType);
		}

		public static void SetPersistingCheck<Field>(PXCache sender, object row)
			where Field : IBqlField
		{
			if (row == null)
				return;
			SetPersistingCheck<Field>(sender, row, GetCondition<PXUIRequiredAttribute, Field>(sender, row));
		}




	}
	#endregion

	#region PXUIEnabledAttribute
	public class PXUIEnabledAttribute : PXBaseConditionAttribute, IPXRowSelectedSubscriber
	{
		public PXUIEnabledAttribute(Type conditionType)
			: base(conditionType)
		{
		}

		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null || _Condition == null) 
				return;
			SetEnabled(sender, e.Row, _FieldName, Condition);
		}

		public static void SetEnabled(PXCache sender, object row, string fieldName, Type conditionType)
		{
			if (row == null || conditionType == null) 
				return;
			PXUIFieldAttribute.SetEnabled(sender, row, fieldName, GetConditionResult(sender, row, conditionType));
		}

		public static void SetEnabled(PXCache sender, object row, string fieldName)
		{
			if (row == null)
				return;
			SetEnabled(sender, row, fieldName, GetCondition<PXUIEnabledAttribute>(sender, row, fieldName));
		}
	
		public static void SetEnabled<Field>(PXCache sender, object row, Type conditionType)
			where Field : IBqlField
		{
			if (row == null || conditionType == null) 
				return;
			SetEnabled(sender, row, typeof (Field).Name, conditionType);
		}

		public static void SetEnabled<Field>(PXCache sender, object row)
			where Field : IBqlField
		{
			if (row == null)
				return;
			SetEnabled<Field>(sender, row, GetCondition<PXUIEnabledAttribute, Field>(sender, row));
		}

	}
	#endregion

	#region CRValidateDateAttribute
	public sealed class CRValidateDateAttribute : PXDBDateAndTimeAttribute
	{
		public override void CacheAttached(PXCache sender)
		{
			sender.Graph.RowPersisting.AddHandler(sender.GetItemType(), (cache, e) =>
			{
				if (cache.GetValue(e.Row, _FieldName) == null)
					cache.SetValue(e.Row, _FieldName, new DateTime(1900,1,1));
			});
			base.CacheAttached(sender);
		}
	}
	#endregion

	#region Minutes
	public sealed class Minutes<Operand> : IBqlOperand, IBqlCreator
	where Operand : IBqlOperand
	{
		
		private IBqlCreator _operand = null;
		public void Parse(PXGraph graph, List<IBqlParameter> pars, List<Type> tables, List<Type> fields, List<IBqlSortColumn> sortColumns, StringBuilder text, PX.Data.BqlCommand.Selection selection)
		{
			if (typeof (IBqlField).IsAssignableFrom(typeof (Operand)))
			{				
				if (fields != null)				
					fields.Add(typeof(Operand));								
			}
			else
			{
				if (_operand == null)
				{
					_operand = Activator.CreateInstance<Operand>() as IBqlCreator;
				}
				if (_operand == null)
				{
					throw new PXArgumentException("Operand", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);
				}
				_operand.Parse(graph, pars, tables, fields, sortColumns, text, selection);
			}
		}

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			if (typeof(IBqlField).IsAssignableFrom(typeof(Operand)))
			{
				if (!(cache.GetItemType() == BqlCommand.GetItemType(typeof(Operand)) || BqlCommand.GetItemType(typeof(Operand)).IsAssignableFrom(cache.GetItemType())))
				{
					return;
				}
				value = cache.GetValue(item, typeof(Operand).Name);
			}
			else
			{
				if (_operand == null)
				{
					_operand = Activator.CreateInstance<Operand>() as IBqlCreator;
				}
				if (_operand == null)
				{
					throw new PXArgumentException("Operand", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);
				}
				_operand.Verify(cache, item, pars, ref result, ref value);				
			}

			if(value != null)
				value = TimeSpan.FromMinutes(Convert.ToDouble(value));
		}
	}

	#endregion
	#region Minutes
	public sealed class Round30Minutes<Operand> : IBqlOperand, IBqlCreator
	where Operand : IBqlOperand
	{

		private IBqlCreator _operand = null;
		public void Parse(PXGraph graph, List<IBqlParameter> pars, List<Type> tables, List<Type> fields, List<IBqlSortColumn> sortColumns, StringBuilder text, PX.Data.BqlCommand.Selection selection)
		{
			if (typeof(IBqlField).IsAssignableFrom(typeof(Operand)))
			{
				if (fields != null)
					fields.Add(typeof(Operand));
			}
			else
			{
				if (_operand == null)
				{
					_operand = Activator.CreateInstance<Operand>() as IBqlCreator;
				}
				if (_operand == null)
				{
					throw new PXArgumentException("Operand", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);
				}
				_operand.Parse(graph, pars, tables, fields, sortColumns, text, selection);
			}
		}

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			if (typeof(IBqlField).IsAssignableFrom(typeof(Operand)))
			{
				if (!(cache.GetItemType() == BqlCommand.GetItemType(typeof(Operand)) || BqlCommand.GetItemType(typeof(Operand)).IsAssignableFrom(cache.GetItemType())))
				{
					return;
				}
				value = cache.GetValue(item, typeof(Operand).Name);
			}
			else
			{
				if (_operand == null)
				{
					_operand = Activator.CreateInstance<Operand>() as IBqlCreator;
				}
				if (_operand == null)
				{
					throw new PXArgumentException("Operand", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);
				}
				_operand.Verify(cache, item, pars, ref result, ref value);
			}

			if (value != null)
			{				
				DateTime date = Convert.ToDateTime(value);
				var minutes = date.Minute;
				if (minutes != 0)
					minutes = minutes <= 30 ? 30 : 60;

				value = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0).AddMinutes(minutes);
			}
		}
	}

	#endregion

	public class PXViewSavedDetailsButtonAttribute : PX.SM.PXViewDetailsButtonAttribute
	{		
		public PXViewSavedDetailsButtonAttribute(Type primaryType):base(primaryType)
		{		
		}

		public PXViewSavedDetailsButtonAttribute(Type primaryType, Type select)
			:base(primaryType , select)
		{			
		}

		protected override void Redirect(PXAdapter adapter, object record)
		{
			var status = adapter.View.Graph.Caches[record.GetType()].GetStatus(record);
			if (status == PXEntryStatus.Deleted || status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated) return;

			base.Redirect(adapter, record);
		}
	}
}
