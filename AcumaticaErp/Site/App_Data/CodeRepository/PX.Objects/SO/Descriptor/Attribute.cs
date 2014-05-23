using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.TX;
using PX.Objects.CS;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.IN;
using PX.Objects.GL;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using LocationStatus = PX.Objects.IN.Overrides.INDocumentRelease.LocationStatus;
using LotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.LotSerialStatus;
using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
using System.ComponentModel;
using System.Diagnostics;
using PX.Objects.CM;
using PX.Objects.CA;

namespace PX.Objects.SO
{
	public class Round<Field, CuryKeyField> : BqlFormula<Field, CuryKeyField>, IBqlOperand
		where Field : IBqlOperand
		where CuryKeyField : IBqlField
	{
		public override void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			decimal? val = (decimal?)Calculate<Field>(cache, item);

			if (val != null)
			{
				value = PXDBCurrencyAttribute.RoundCury<CuryKeyField>(cache, item, (decimal)val);
			}
		}
	}


	public static class Constants
	{
		public const string NoShipmentNbr = "<NEW>";

		public class noShipmentNbr : Constant<string>
		{
			public noShipmentNbr() : base(NoShipmentNbr) { ;}
		}
	}

	public class SOSetupSelect : PXSetupSelect<SOSetup>
	{
		public SOSetupSelect(PXGraph graph)
			:base(graph)
		{
		}

		protected override void FillDefaultValues(SOSetup record)
		{
			record.MinGrossProfitValidation = MinGrossProfitValidationType.Warning;
		}
	}

	public class OrderSiteSelectorAttribute : PXSelectorAttribute
	{
		protected string _InputMask = null;

		public OrderSiteSelectorAttribute()
			: base(typeof(Search2<SOOrderSite.siteID, 
				InnerJoin<INSite, On<INSite.siteID, Equal<SOOrderSite.siteID>>>,
				Where<SOOrderSite.orderType, Equal<Current<SOOrder.orderType>>, And<SOOrderSite.orderNbr, Equal<Current<SOOrder.orderNbr>>, 
				And<Match<INSite, Current<AccessInfo.userName>>>>>>),
				typeof(INSite.siteCD), typeof(INSite.descr), typeof(INSite.replenishmentClassID)
			)
		{
			this.DirtyRead = true;
			this.SubstituteKey = typeof(INSite.siteCD);
			this.DescriptionField = typeof(INSite.descr);
			this._UnconditionalSelect = BqlCommand.CreateInstance(typeof(Search<INSite.siteID, Where<INSite.siteID, Equal<Required<INSite.siteID>>>>));
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			PXDimensionAttribute attr = new PXDimensionAttribute(SiteAttribute.DimensionName);
			attr.CacheAttached(sender);
			PXFieldSelectingEventArgs e = new PXFieldSelectingEventArgs(null, null, true, false);
			attr.FieldSelecting(sender, e);

			_InputMask = ((PXSegmentedState)e.ReturnState).InputMask;
		}

		public override void SubstituteKeyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.SubstituteKeyFieldSelecting(sender, e);
			if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
			{
				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, null, null, null, null, null, _InputMask, null, null, null, null);
			}
		}
	}

	/// <summary>
	/// Specialized for SOInvoice version of the InvoiceNbrAttribute.<br/>
	/// The main purpose of the attribute is poviding of the uniqueness of the RefNbr <br/>
	/// amoung  a set of  documents of the specifyed types (for example, each RefNbr of the ARInvoice <br/>
	/// the ARInvoices must be unique across all ARInvoices and AR Debit memos)<br/>
	/// This may be useful, if user has configured a manual numberin for SOInvoices  <br/>
	/// or needs  to create SOInvoice from another document (like SOOrder) allowing to type RefNbr <br/>
	/// for the to-be-created Invoice manually. To store the numbers, system using ARInvoiceNbr table, <br/>
	/// keyed uniquelly by DocType and RefNbr. A source document is linked to a number by NoteID.<br/>
	/// Attributes checks a number for uniqueness on FieldVerifying and RowPersisting events.<br/>
	/// </summary>
	public class SOInvoiceNbrAttribute : InvoiceNbrAttribute
	{
		public SOInvoiceNbrAttribute()
			: base(typeof(SOOrder.aRDocType), typeof(SOOrder.noteID))
		{
		}

		protected override bool DeleteOnUpdate(PXCache sender, PXRowPersistedEventArgs e)
		{
			return base.DeleteOnUpdate(sender, e) || (bool?)sender.GetValue<SOOrder.cancelled>(e.Row) == true;
		}
	}

	/// <summary>
	/// Automatically tracks and Updates Cash discounts in accordance with Customer's Credit Terms. 
	/// </summary>
	public class SOInvoiceTermsAttribute : TermsAttribute
	{
		public SOInvoiceTermsAttribute()
			: base(typeof(ARInvoice.docDate), typeof(ARInvoice.dueDate), typeof(ARInvoice.discDate), null, null)
		{ 
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdated.AddHandler(typeof(ARInvoice), typeof(ARInvoice.curyOrigDocAmt).Name, CalcDisc<ARInvoice.curyOrigDocAmt>);
			sender.Graph.FieldUpdated.AddHandler(typeof(ARInvoice), typeof(ARInvoice.curyDocBal).Name, CalcDisc<ARInvoice.curyDocBal>);
			sender.Graph.FieldVerifying.AddHandler(typeof(ARInvoice), typeof(ARInvoice.curyOrigDiscAmt).Name, VerifyDiscount<ARInvoice.curyOrigDocAmt>);
			sender.Graph.FieldVerifying.AddHandler(typeof(ARInvoice), typeof(ARInvoice.curyOrigDiscAmt).Name, VerifyDiscount<ARInvoice.curyDocBal>);

			_CuryDiscBal = typeof(ARInvoice.curyOrigDiscAmt);
		}

		public override void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			base.FieldUpdated(sender, e);
			CalcDisc<ARInvoice.curyOrigDocAmt>(sender, e);
			CalcDisc<ARInvoice.curyDocBal>(sender, e);
		}

		public void CalcDisc<Field>(PXCache sender, PXFieldUpdatedEventArgs e)
			where Field : IBqlField
		{
			if (((ARInvoice)e.Row).DocType == ARDocType.CashSale && typeof(Field) == typeof(ARInvoice.curyDocBal) ||
				((ARInvoice)e.Row).DocType != ARDocType.CashSale && typeof(Field) == typeof(ARInvoice.curyOrigDocAmt))
			{
				_CuryDocBal = typeof(Field);
			}

			try
			{
				base.CalcDisc(sender, e);
			}
			finally
			{
				_CuryDocBal = null;
			}
		}

		public void VerifyDiscount<Field>(PXCache sender, PXFieldVerifyingEventArgs e)
			where Field : IBqlField
		{
			if (((ARInvoice)e.Row).DocType == ARDocType.CashSale && typeof(Field) == typeof(ARInvoice.curyDocBal) ||
				((ARInvoice)e.Row).DocType != ARDocType.CashSale && typeof(Field) == typeof(ARInvoice.curyOrigDocAmt))
			{
				_CuryDocBal = typeof(Field);
			}

			try
			{
				base.VerifyDiscount(sender, e);
			}
			finally
			{
				_CuryDocBal = null;
			}
		}
	}


	public class SOShipExpireDateAttribute : INExpireDateAttribute
	{
		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((ILSMaster)e.Row).InventoryID);
			if (item == null) return;

			if (((INLotSerClass)item).LotSerTrack != INLotSerTrack.NotNumbered && (((INLotSerClass)item).LotSerAssign != INLotSerAssign.WhenUsed || ((SOShipLineSplit)e.Row).Confirmed == true))
			{
				base.RowPersisting(sender, e);
			}
		}
	}

	public class DirtyFormulaAttribute : PXAggregateAttribute, IPXRowInsertedSubscriber, IPXRowUpdatedSubscriber
	{
		protected Dictionary<object, object> inserted = null;
		protected Dictionary<object, object> updated = null;

		public DirtyFormulaAttribute(Type formulaType, Type aggregateType)
		{
			this._Attributes.Add(new PXFormulaAttribute(formulaType, aggregateType));
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			inserted = new Dictionary<object, object>();
			updated = new Dictionary<object, object>();
		}

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			object copy;
			if (!inserted.TryGetValue(e.Row, out copy))
			{
				inserted[e.Row] = sender.CreateCopy(e.Row);
			} 
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			object copy;
			if (!updated.TryGetValue(e.Row, out copy))
			{
				updated[e.Row] = sender.CreateCopy(e.Row);
			}
		}

		public static void RaiseRowInserted<Field>(PXCache sender, PXRowInsertedEventArgs e)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributes<Field>(e.Row))
			{
				if (attr is DirtyFormulaAttribute)
				{
					object copy;
					if (((DirtyFormulaAttribute)attr).inserted.TryGetValue(e.Row, out copy))
					{
						List<IPXRowUpdatedSubscriber> subs = new List<IPXRowUpdatedSubscriber>();
						((PXAggregateAttribute)attr).GetSubscriber<IPXRowUpdatedSubscriber>(subs);
						foreach (IPXRowUpdatedSubscriber ru in subs)
						{
							ru.RowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, copy, false));
						}
						((DirtyFormulaAttribute)attr).inserted.Remove(e.Row);
					}
				}
			}
		}

		public static void RaiseRowUpdated<Field>(PXCache sender, PXRowUpdatedEventArgs e)
			where Field : IBqlField
		{
			foreach (PXEventSubscriberAttribute attr in sender.GetAttributes<Field>(e.Row))
			{
				if (attr is DirtyFormulaAttribute)
				{
					object copy;
					if (((DirtyFormulaAttribute)attr).updated.TryGetValue(e.Row, out copy))
					{
						List<IPXRowUpdatedSubscriber> subs = new List<IPXRowUpdatedSubscriber>();
						((PXAggregateAttribute)attr).GetSubscriber<IPXRowUpdatedSubscriber>(subs);
						foreach (IPXRowUpdatedSubscriber ru in subs)
						{
							ru.RowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, copy, false));
						}
						((DirtyFormulaAttribute)attr).updated.Remove(e.Row);
					}
				}
			}
		}
	}

	public sealed class OpenLineCalc<Field> : IBqlAggregateCalculator
	where Field : IBqlField
	{
		#region IBqlAggregateCalculator Members

		public object Calculate(PXCache cache, object row, object oldrow, int fieldordinal, int digit)
		{
			if (row != null && !typeof(SOLine).IsAssignableFrom(row.GetType()) ||
				oldrow != null && !typeof(SOLine).IsAssignableFrom(oldrow.GetType()))
			{
				return null;
			}

			if (object.ReferenceEquals(row, oldrow))
			{
				return null;
			}

			if (row != null && ((SOLine)row).Completed != true && (((SOLine)row).IsFree != true || ((SOLine)row).ManualDisc == true) && (oldrow == null || ((SOLine)oldrow).Completed == true || ((SOLine)oldrow).IsFree == true && ((SOLine)oldrow).ManualDisc == false))
			{
				return 1;
			}

			if (oldrow != null && ((SOLine)oldrow).Completed != true && (((SOLine)oldrow).IsFree != true || ((SOLine)oldrow).ManualDisc == true) && (row == null || ((SOLine)row).Completed == true || ((SOLine)row).IsFree == true && ((SOLine)row).ManualDisc == false))
			{
				return -1;
			}

			return 0;
		}

		public object Calculate(PXCache cache, object row, int fieldordinal, object[] records, int digit)
		{
			short count = 0;
			foreach (object record in records)
			{
				if (((SOLine)record).Completed != true && (((SOLine)record).IsFree != true || ((SOLine)record).ManualDisc == true))
				{
					count++;
				}
			}
			return count;
		}

		#endregion
	}

	public class DateMinusDaysNotLessThenDate<V1, V2, V3> : IBqlCreator
		where V1 : IBqlOperand
		where V2 : IBqlOperand
		where V3 : IBqlOperand
	{
		IBqlCreator _formula = new Switch<Case<Where<Sub<V1,V2>, Less<V3>, Or<Sub<V1,V2>, IsNull>>, V3>, Sub<V1,V2>>();

		public void Parse(PXGraph graph, List<IBqlParameter> pars, List<Type> tables, List<Type> fields, List<IBqlSortColumn> sortColumns, StringBuilder text, PX.Data.BqlCommand.Selection selection)
		{
			_formula.Parse(graph, pars, tables, fields, sortColumns, text, selection);
		}
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			_formula.Verify(cache, item, pars, ref result, ref value);
		}	
	}

	public interface IFreightBase
	{
		string ShipTermsID { get; }
		string ShipVia { get; }
		string ShipZoneID { get; }
		decimal? LineTotal { get; }
		decimal? OrderWeight { get; }
		decimal? PackageWeight { get; }
		decimal? OrderVolume { get; }
		decimal? FreightAmt { get; set;}
		decimal? FreightCost { get; set; }
	}

	/// <summary>
	/// Calculates Freight Cost and Freight Terms
	/// </summary>
	public class FreightCalculator
	{
		protected PXGraph graph;

		public FreightCalculator(PXGraph graph)
		{
			if (graph == null)
				throw new ArgumentNullException("graph");

			this.graph = graph;
		}

		/*
		Freight Calculation

		1. Per Unit

		FreightInBase = BaseRate + Rate * Weight

		Select FreightRate.Rate 
		FreightRate.Volume <= Order.Volume && 
		FreightRate.Weight <= Order.Weight && 
		FreightRate.Zone = Order.ShippingZone 
		ORDER BY FreightRate.Volume Asc, FreightRate.Weight Asc, FreightRate.Rate Asc

		IF not found, then

		Select ShipTerms.Rate 
		FreightRate.Volume <= Order.Volume && 
		FreightRate.Weight <= Order.Weight 
		ORDER BY FreightRate.Volume Asc, FreightRate.Weight Asc, FreightRate.Rate Desc
		(MAX Rate for search criteria)

		2. Net

		FreightInBase = BaseRate + Rate
		--------------------------------------------------------------------------------
											 FreightCost%			   InvoiceAmount%
		FreightFinalInBase = FreightInBase * ------------ + LineTotal * --------------
												 100						  100
		*/

		/// <summary>
		/// First calculate and sets CuryFreightCost, then applies the Terms and updates CuryFreightAmt.
		/// </summary>
		public virtual void CalcFreight<T, CuryFreightCostField, CuryFreightField>(PXCache sender, T data, int? linesCount)
			where T : class, IFreightBase, new()
			where CuryFreightField : IBqlField
			where CuryFreightCostField : IBqlField
		{
			decimal cost = CalculateFreightCost<T>(data);
			data.FreightCost = cost;
			data.FreightAmt = cost;

			ShipTermsDetail shipTermsDetail = GetFreightTerms(data.ShipTermsID, data.LineTotal);
			if (shipTermsDetail != null)
			{
				data.FreightAmt = cost * (shipTermsDetail.FreightCostPercent) / 100 +
					(data.LineTotal) * (shipTermsDetail.InvoiceAmountPercent) / 100 + shipTermsDetail.ShippingHandling + (linesCount * shipTermsDetail.LineHandling);
			}
			CM.PXCurrencyAttribute.CuryConvCury<CuryFreightCostField>(sender, data);
			CM.PXCurrencyAttribute.CuryConvCury<CuryFreightField>(sender, data);
		}

		/// <summary>
		/// Applies the Terms and updates CuryFreightAmt.
		/// </summary>
		public virtual void ApplyFreightTerms<T, CuryFreightField>(PXCache sender, T data, int? linesCount)
			where T : class, IFreightBase, new()
			where CuryFreightField : IBqlField
		{
			ShipTermsDetail shipTermsDetail = GetFreightTerms(data.ShipTermsID, data.LineTotal);

			if (shipTermsDetail != null)
			{
				data.FreightAmt = ((data.FreightCost * (shipTermsDetail.FreightCostPercent) / 100) ?? 0) +
						(((data.LineTotal) * (shipTermsDetail.InvoiceAmountPercent) / 100) ?? 0) + (shipTermsDetail.ShippingHandling ?? 0) + ((linesCount * shipTermsDetail.LineHandling) ?? 0);
			}
			else
			{
				data.FreightAmt = data.FreightCost;
			}

			CM.PXCurrencyAttribute.CuryConvCury<CuryFreightField>(sender, data);
		}


		protected virtual decimal CalculateFreightCost<T>(T data)
			where T : class, IFreightBase, new()
		{
			Carrier carrier = PXSelect<Carrier,
				Where<Carrier.carrierID, Equal<Required<SOOrder.shipVia>>>>.
				Select(graph, data.ShipVia);

			if (carrier == null)
				return 0;

			decimal freightCostAmt = 0;
			if (data.OrderVolume == null || data.OrderVolume == 0)
			{
				//Get Freight Rate based only on weight.
				FreightRate freightRateOnWeight = GetFreightRateBasedOnWeight(carrier.CarrierID, data.ShipZoneID, data.OrderWeight);

				if (carrier.CalcMethod == "N")
					freightCostAmt = freightRateOnWeight.Rate ?? 0;
				else
                    if(data.PackageWeight == null || data.PackageWeight == 0)
                        freightCostAmt = (freightRateOnWeight.Rate ?? 0m) * (data.OrderWeight ?? 0m);
                    else
                        freightCostAmt = (freightRateOnWeight.Rate ?? 0m) * (data.PackageWeight ?? 0m);
			}
			else if (data.PackageWeight == null || data.PackageWeight == 0)
			{
				//Get Freight Rate based only on Volume
				FreightRate freightRateOnVolume = GetFreightRateBasedOnVolume(carrier.CarrierID, data.ShipZoneID, data.OrderVolume);

				if (carrier.CalcMethod == "N")
					freightCostAmt = freightRateOnVolume.Rate ?? 0;
				else
					freightCostAmt = (freightRateOnVolume.Rate ?? 0m) * (data.OrderVolume ?? 0m);
			}
			else
			{
				FreightRate freightRateOnWeight = GetFreightRateBasedOnWeight(carrier.CarrierID, data.ShipZoneID, data.OrderWeight);
				FreightRate freightRateOnVolume = GetFreightRateBasedOnVolume(carrier.CarrierID, data.ShipZoneID, data.OrderVolume);

				decimal freightCostByWeight = 0;
				decimal freightCostByVolume = 0;


				if (carrier.CalcMethod == "N")
				{
					freightCostByWeight = freightRateOnWeight.Rate ?? 0;
					freightCostByVolume = freightRateOnVolume.Rate ?? 0;
				}
				else
				{
					freightCostByWeight = (freightRateOnWeight.Rate ?? 0m) * (data.PackageWeight ?? 0m);
					freightCostByVolume = (freightRateOnVolume.Rate ?? 0m) * (data.OrderVolume ?? 0m);
				}

				freightCostAmt = Math.Max(freightCostByWeight, freightCostByVolume);
			}

			return (carrier.BaseRate ?? 0m) + freightCostAmt;
		}

		protected virtual ShipTermsDetail GetFreightTerms(string shipTermsID, decimal? lineTotal)
		{
			return PXSelect<ShipTermsDetail,
				Where<ShipTermsDetail.shipTermsID, Equal<Required<SOOrder.shipTermsID>>,
				And<ShipTermsDetail.breakAmount, LessEqual<Required<SOOrder.lineTotal>>>>,
				OrderBy<Desc<ShipTermsDetail.breakAmount>>>.Select(graph, shipTermsID, lineTotal);

		}

		protected virtual FreightRate GetFreightRateBasedOnWeight(string carrierID, string shipZoneID, decimal? weight)
		{
			FreightRate freightRate = PXSelect<FreightRate,
				Where<FreightRate.carrierID, Equal<Required<FreightRate.carrierID>>,
				And<FreightRate.weight, LessEqual<Required<SOOrder.orderWeight>>,
				And<FreightRate.zoneID, Equal<Required<SOOrder.shipZoneID>>>>>,
				OrderBy<Desc<FreightRate.volume, Desc<FreightRate.weight, Asc<FreightRate.rate>>>>>.
				Select(graph, carrierID, weight, shipZoneID);

			if (freightRate == null)
			{
				freightRate = PXSelect<FreightRate,
					Where<FreightRate.carrierID, Equal<Required<FreightRate.carrierID>>,
					And<FreightRate.weight, LessEqual<Required<FreightRate.weight>>>>,
					OrderBy<Desc<FreightRate.volume, Desc<FreightRate.weight, Desc<FreightRate.rate>>>>>.
					Select(graph, weight);
			}

			return freightRate ?? new FreightRate();
		}

		protected virtual FreightRate GetFreightRateBasedOnVolume(string carrierID, string shipZoneID, decimal? volume)
		{
			FreightRate freightRate = PXSelect<FreightRate,
				Where<FreightRate.carrierID, Equal<Required<FreightRate.carrierID>>,
				And<FreightRate.volume, LessEqual<Required<FreightRate.volume>>,
				And<FreightRate.zoneID, Equal<Required<FreightRate.zoneID>>>>>,
				OrderBy<Desc<FreightRate.volume, Desc<FreightRate.weight, Asc<FreightRate.rate>>>>>.
				Select(graph, carrierID, volume, shipZoneID);

			if (freightRate == null)
			{
				freightRate = PXSelect<FreightRate,
					Where<FreightRate.carrierID, Equal<Required<FreightRate.carrierID>>,
					And<FreightRate.weight, LessEqual<Required<FreightRate.weight>>>>,
					OrderBy<Desc<FreightRate.volume, Desc<FreightRate.weight, Desc<FreightRate.rate>>>>>.
					Select(graph, volume);
			}

			return freightRate ?? new FreightRate();
		}

	}

	/// <summary>
	/// This is a specialized version of the <see cref="AR.CustomerCreditAttribute"/> attribute.<br/>
	/// See CustomerAttribute for detailed description. <br/>
	/// </summary>
	/// <remarks>
	/// Graph must contain a APSetup cache.
	/// </remarks>
	/// <example>
	/// [SOCustomerCredit(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Filterable = true)]
	/// </example>		
	[PXDBInt()]
	[PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<Customer.status, IsNull,
						Or<Customer.status, Equal<BAccount.status.active>,
						Or<Customer.status, Equal<BAccount.status.oneTime>>>>), AR.Messages.CustomerIsInStatus, typeof(Customer.status), ReplaceInherited = true)]
	public class SOCustomerCreditAttribute : AR.CustomerCreditAttribute
	{
		#region Ctor
		public SOCustomerCreditAttribute()
			: this(typeof(
				Search<BAccountR.bAccountID,
					Where<Customer.type, IsNotNull,
			      Or<Current<SOOrder.aRDocType>, Equal<ARDocType.noUpdate>,
					And<BAccountR.type, Equal<BAccountType.companyType>>>>>))
		{
		}

		public SOCustomerCreditAttribute(Type search)
			: base(search, typeof(SOOrder.creditHold), typeof(SOOrder.cancelled))
		{
		}
		#endregion
		#region Implementation

		protected override bool? GetReleasedValue(PXCache sender, object Row)
		{
			return (bool?)sender.GetValue<SOOrder.cancelled>(Row) == true && (bool?)sender.GetValue<SOOrder.completed>(Row) == true;
		}

		protected override bool? GetHoldValue(PXCache sender, object Row)
		{
			return ((bool?)sender.GetValue<SOOrder.hold>(Row) == true || (bool?)sender.GetValue<SOOrder.creditHold>(Row) == true || (bool?)sender.GetValue<SOOrder.inclCustOpenOrders>(Row) == false);
		}

		protected override bool? GetCreditCheckError(PXCache sender)
		{
			PXCache ordertypecache = sender.Graph.Caches[typeof(SOOrderType)];
			return ordertypecache.Current != null ? ((SOOrderType)ordertypecache.Current).CreditHoldEntry : false;
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (_InternalCall)
			{
				return;
			}

			if (e.OldRow != null && !sender.ObjectsEqual<SOOrder.creditHold>(e.Row, e.OldRow) && (bool?)sender.GetValue<SOOrder.creditHold>(e.Row) == false && (bool?)sender.GetValue<SOOrder.hold>(e.Row) == false ||
				e.OldRow != null && !sender.ObjectsEqual<SOOrder.isCCAuthorized>(e.Row, e.OldRow) && (bool?)sender.GetValue<SOOrder.isCCAuthorized>(e.Row) == true)
			{
				object DocumentBal = sender.GetValue<SOOrder.orderTotal>(e.Row);

				sender.SetValue<SOOrder.approvedCredit>(e.Row, true);
				sender.SetValue<SOOrder.approvedCreditAmt>(e.Row, DocumentBal);
			}

			base.RowUpdated(sender, e);
		}

		protected override decimal? GetDocumentBalance(PXCache cache, object Row)
		{
			decimal? DocumentBal = base.GetDocumentBalance(cache, Row);
			PXCache sender = cache.Graph.Caches[typeof(SOOrder)];

			if (DocumentBal > 0m && (bool?)sender.GetValue<SOOrder.approvedCredit>(Row) == true)
			{
				if ((decimal?)sender.GetValue<SOOrder.approvedCreditAmt>(Row) >= (decimal?)sender.GetValue<SOOrder.orderTotal>(Row))
				{
					DocumentBal = 0m;
				}
			}

			return DocumentBal;
		}

		protected override void PlaceOnHold(PXCache sender, object Row, bool OnAdminHold)
		{
			if (OnAdminHold)
			{
				sender.RaiseExceptionHandling<SOOrder.hold>(Row, true, new PXSetPropertyException(AR.Messages.AdminHoldEntry, PXErrorLevel.Warning));

				object OldRow = sender.CreateCopy(Row);
				sender.SetValueExt<SOOrder.status>(Row, null);
				sender.SetValueExt<SOOrder.creditHold>(Row, false);
				sender.SetValueExt<SOOrder.hold>(Row, true);
				sender.RaiseRowUpdated(Row, OldRow);
			}
			else
			{
				sender.SetValueExt<SOOrder.status>(Row, null);
				base.PlaceOnHold(sender, Row, false);
			}

			sender.SetValue<SOOrder.approvedCredit>(Row, false);
			sender.SetValue<SOOrder.approvedCreditAmt>(Row, 0m);
		}
		#endregion
	}

	[PXDBInt()]
	[PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.Visible)]
	public class ARCustomerCreditAttribute : AR.CustomerCreditAttribute
	{
		#region Ctor
		public ARCustomerCreditAttribute()
			: base(typeof(ARInvoice.creditHold), typeof(ARInvoice.released))
		{
		}
		#endregion

		#region Initialization
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.RowUpdated.AddHandler<SOInvoice>(SOInvoice_RowUpdated);
		}
		#endregion

		#region Implementation

		protected override bool? GetHoldValue(PXCache sender, object Row)
		{
			return ((bool?)sender.GetValue<ARInvoice.hold>(Row) == true || (bool?)sender.GetValue<ARInvoice.creditHold>(Row) == true);
		}

		protected override bool? GetCreditCheckError(PXCache sender)
		{
			PXCache sosetupcache = sender.Graph.Caches[typeof(SOSetup)];
			return ((SOSetup)sosetupcache.Current).CreditCheckError;
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (_InternalCall)
			{
				return;
			}

			if (e.OldRow != null && !sender.ObjectsEqual<ARInvoice.creditHold>(e.Row, e.OldRow) && (bool?)sender.GetValue<ARInvoice.creditHold>(e.Row) == false && (bool?)sender.GetValue<ARInvoice.hold>(e.Row) == false)
			{
				object DocumentBal = sender.GetValue<ARInvoice.origDocAmt>(e.Row);

				sender.SetValue<ARInvoice.approvedCredit>(e.Row, true);
				sender.SetValue<ARInvoice.approvedCreditAmt>(e.Row, DocumentBal);
			}

			base.RowUpdated(sender, e);
		}

		public virtual void SOInvoice_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (e.OldRow != null && !sender.ObjectsEqual<SOInvoice.isCCCaptured>(e.Row, e.OldRow) && (bool?)sender.GetValue<SOInvoice.isCCCaptured>(e.Row) == true)
			{
				ARInvoice ardoc = (ARInvoice)sender.Graph.Caches[typeof(ARInvoice)].Current;

				if (ardoc != null)
				{
					object DocumentBal = sender.GetValue<ARInvoice.origDocAmt>(ardoc);

					sender.SetValue<ARInvoice.approvedCredit>(ardoc, true);
					sender.SetValue<ARInvoice.approvedCreditAmt>(ardoc, DocumentBal);
				}
			}
		}

		protected override decimal? GetDocumentBalance(PXCache cache, object Row)
		{
			decimal? DocumentBal = 0m;
			ARBalances accumbal = cache.Current as ARBalances;
			if (accumbal != null && cache.GetStatus(accumbal) == PXEntryStatus.Inserted)
			{
				//get balance only from PXAccumulator
				DocumentBal = accumbal.UnreleasedBal;
			}

			PXCache sender = cache.Graph.Caches[typeof(ARInvoice)];

			if (DocumentBal > 0m && (bool?)sender.GetValue<ARInvoice.approvedCredit>(Row) == true)
			{
				if ((decimal?)sender.GetValue<ARInvoice.approvedCreditAmt>(Row) >= (decimal?)sender.GetValue<ARInvoice.origDocAmt>(Row))
				{
					DocumentBal = 0m;
				}
			}

			return DocumentBal;
		}

		protected override void PlaceOnHold(PXCache sender, object Row, bool OnAdminHold)
		{
			if (OnAdminHold)
			{
				sender.RaiseExceptionHandling<ARInvoice.hold>(Row, true, new PXSetPropertyException(AR.Messages.AdminHoldEntry, PXErrorLevel.Warning));

				object OldRow = sender.CreateCopy(Row);
				sender.SetValueExt<ARInvoice.status>(Row, null);
				sender.SetValueExt<ARInvoice.creditHold>(Row, false);
				sender.SetValueExt<ARInvoice.hold>(Row, true);
				sender.RaiseRowUpdated(Row, OldRow);
			}
			else
			{
				base.PlaceOnHold(sender, Row, false);
			}

			sender.SetValue<ARInvoice.approvedCredit>(Row, false);
			sender.SetValue<ARInvoice.approvedCreditAmt>(Row, 0m);
		}
		#endregion
	}	


	/// <summary>
	/// Creates Plan statistics for SOLine.
	/// </summary>
	public class SOLinePlanIDAttribute : INItemPlanIDAttribute
	{
		#region State
		protected Type _ParentOrderDate;
		#endregion
		#region Ctor
		public SOLinePlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry, Type ParentOrderDate)
			: base(ParentNoteID, ParentHoldEntry)
		{
			_ParentOrderDate = ParentOrderDate;
		}
		#endregion
		#region Implementation
		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PXView view;
			WebDialogResult answer = sender.Graph.Views.TryGetValue("Document", out view) ? view.Answer : WebDialogResult.None;

			bool DatesUpdated = !sender.ObjectsEqual<SOOrder.shipDate>(e.Row, e.OldRow) && (answer == WebDialogResult.Yes || ((SOOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed);
			bool RequestOnUpdated = !sender.ObjectsEqual<SOOrder.requestDate>(e.Row, e.OldRow) && (answer == WebDialogResult.Yes || ((SOOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed);

			if (DatesUpdated || !sender.ObjectsEqual<SOOrder.hold, SOOrder.cancelled, SOOrder.backOrdered, SOOrder.shipComplete>(e.Row, e.OldRow))
			{
				bool Cancelled = (bool)sender.GetValue<SOOrder.cancelled>(e.Row);
				bool? BackOrdered = (bool?)sender.GetValue<SOOrder.backOrdered>(e.Row);
				DatesUpdated |=	!sender.ObjectsEqual<SOOrder.shipComplete>(e.Row, e.OldRow) && ((SOOrder) e.Row).ShipComplete != SOShipComplete.BackOrderAllowed;
				RequestOnUpdated |= !sender.ObjectsEqual<SOOrder.shipComplete>(e.Row, e.OldRow) && ((SOOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed;

				PXCache plancache = sender.Graph.Caches[typeof(INItemPlan)];
				PXCache linecache = sender.Graph.Caches[typeof(SOLine)];

				foreach (SOLine line in PXSelect<SOLine, Where<SOLine.orderType, Equal<Current<SOOrder.orderType>>, And<SOLine.orderNbr, Equal<Current<SOOrder.orderNbr>>, And<SOLine.lineType, NotEqual<SOLineType.miscCharge>>>>>.Select(sender.Graph))
				{
					foreach (INItemPlan plan in plancache.Inserted)
					{
						if (object.Equals(plan.PlanID, line.PlanID))
						{
							if (Cancelled)
							{
								plancache.Delete(plan);
							}
							else
							{
								INItemPlan copy = PXCache<INItemPlan>.CreateCopy(plan);

								plan.Hold = (bool?)sender.GetValue<SOOrder.hold>(e.Row);
								if (DatesUpdated)
								{
									plan.PlanDate = (DateTime?)sender.GetValue<SOOrder.shipDate>(e.Row);
								}

								if (BackOrdered != null)
								{
									if (!INPlanConstants.IsPlan66X(plan.PlanType))
									{
										plan.PlanType = BackOrdered == true ? INPlanConstants.Plan68 : ((SOOrderType)sender.Graph.Caches[typeof(SOOrderType)].Current).OrderPlanType;
									}
								}

								if (!string.Equals(copy.PlanType, plan.PlanType))
								{
									plancache.RaiseRowUpdated(plan, copy);
								}
							}
							break;
						}
					}

					if (Cancelled)
					{
						SOLine old_row = PXCache<SOLine>.CreateCopy(line);
						line.UnbilledQty -= line.OpenQty;
						line.OpenQty = 0m;
						linecache.RaiseFieldUpdated<SOLine.unbilledQty>(line, 0m);
						linecache.RaiseFieldUpdated<SOLine.openQty>(line, 0m);

						line.Cancelled = true;
						line.PlanID = null;

						//SOOrderEntry_SOOrder_RowUpdated should execute later to correctly update balances
						TaxAttribute.Calculate<SOLine.taxCategoryID>(linecache, new PXRowUpdatedEventArgs(line, old_row, false));

						if (linecache.GetStatus(line) == PXEntryStatus.Notchanged)
						{
							linecache.SetStatus(line, PXEntryStatus.Updated);
						}
					}
					else
					{
						if ((bool)sender.GetValue<SOOrder.cancelled>(e.OldRow))
						{
							SOLine old_row = PXCache<SOLine>.CreateCopy(line);
							line.OpenQty = line.OrderQty - line.ShippedQty;
							line.UnbilledQty += line.OpenQty;
							object value = line.UnbilledQty;
							linecache.RaiseFieldVerifying<SOLine.unbilledQty>(line, ref value);
							linecache.RaiseFieldUpdated<SOLine.unbilledQty>(line, value);

							value = line.OpenQty;
							linecache.RaiseFieldVerifying<SOLine.openQty>(line, ref value);
							linecache.RaiseFieldUpdated<SOLine.openQty>(line, value);

							line.Cancelled = false;
							
							TaxAttribute.Calculate<SOLine.taxCategoryID>(linecache, new PXRowUpdatedEventArgs(line, old_row, false));

							INItemPlan plan = DefaultValues(linecache, line);
							if (plan != null)
							{
								plan = (INItemPlan) sender.Graph.Caches[typeof (INItemPlan)].Insert(plan);
								line.PlanID = plan.PlanID;								
							}
							if (linecache.GetStatus(line) == PXEntryStatus.Notchanged)
								linecache.SetStatus(line, PXEntryStatus.Updated);				
						}
						if (DatesUpdated)
						{
							line.ShipDate = (DateTime?) sender.GetValue<SOOrder.shipDate>(e.Row);
							if (linecache.GetStatus(line) == PXEntryStatus.Notchanged)
							{
								linecache.SetStatus(line, PXEntryStatus.Updated);
							}
						}
						if (RequestOnUpdated)
						{
							line.RequestDate = (DateTime?)sender.GetValue<SOOrder.requestDate>(e.Row);
							if (linecache.GetStatus(line) == PXEntryStatus.Notchanged)
							{
								linecache.SetStatus(line, PXEntryStatus.Updated);
							}
						}
					}
				}

				if (Cancelled)
				{
					PXFormulaAttribute.CalcAggregate<SOLine.unbilledQty>(linecache, e.Row);
					PXFormulaAttribute.CalcAggregate<SOLine.openQty>(linecache, e.Row);
				}

				PXSelectBase<INItemPlan> cmd = new PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<SOOrder.noteID>>>>(sender.Graph);

				//BackOrdered is tri-state
				if (BackOrdered == true && sender.GetValue<SOOrder.lastSiteID>(e.Row) != null && sender.GetValue<SOOrder.lastShipDate>(e.Row) != null)
				{
					cmd.WhereAnd<Where<INItemPlan.siteID, Equal<Current<SOOrder.lastSiteID>>, And<INItemPlan.planDate, LessEqual<Current<SOOrder.lastShipDate>>>>>();
				}

				if (BackOrdered == false)
				{
					sender.SetValue<SOOrder.lastSiteID>(e.Row, null);
					sender.SetValue<SOOrder.lastShipDate>(e.Row, null);
				}

				foreach (INItemPlan plan in cmd.Select())
				{
					if (Cancelled)
					{
						plancache.Delete(plan);
					}
					else
					{
						INItemPlan copy = PXCache<INItemPlan>.CreateCopy(plan);

						plan.Hold = (bool?)sender.GetValue<SOOrder.hold>(e.Row);
						if (DatesUpdated)
						{
							plan.PlanDate = (DateTime?)sender.GetValue<SOOrder.shipDate>(e.Row);
						}

						if (BackOrdered != null && ((SOOrderType)sender.Graph.Caches[typeof(SOOrderType)].Current).RequireAllocation != true)
						{
							if (!INPlanConstants.IsPlan66X(plan.PlanType))
							{
								plan.PlanType = BackOrdered == true ? INPlanConstants.Plan68 : ((SOOrderType)sender.Graph.Caches[typeof(SOOrderType)].Current).OrderPlanType;
							}
						}

						if (!string.Equals(copy.PlanType, plan.PlanType))
						{
							plancache.RaiseRowUpdated(plan, copy);
						}
						
						if (plancache.GetStatus(plan) == PXEntryStatus.Notchanged || plancache.GetStatus(plan) == PXEntryStatus.Held)
						{
							plancache.SetStatus(plan, PXEntryStatus.Updated);
						}
					}
				}
			}
		}

		public override void Parent_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				_KeyToAbort = sender.GetValue<SOOrder.orderNbr>(e.Row);
			}
		}

		public override void Parent_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open && _KeyToAbort != null)
			{
				foreach (SOLine split in PXSelect<SOLine, Where<SOLine.orderType, Equal<Required<SOOrder.orderType>>, And<SOLine.orderNbr, Equal<Required<SOOrder.orderNbr>>, And<SOLine.lineType, NotEqual<SOLineType.miscCharge>>>>>.Select(sender.Graph, ((SOOrder)e.Row).OrderType, _KeyToAbort))
				{
					foreach (INItemPlan plan in sender.Graph.Caches[typeof(INItemPlan)].Inserted)
					{
						if (object.Equals(plan.PlanID, split.PlanID))
						{
							plan.RefNoteID = (long?)sender.GetValue(e.Row, _ParentNoteID.Name);
						}
					}
				}
			}
			_KeyToAbort = null;
		}

		bool InitPlan = false;
		bool ResetSupplyPlanID = false;
		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			//respond only to GUI operations
			InitPlan = !sender.ObjectsEqual<SOLine.pOCreate, SOLine.pOSource, SOLine.operation>(e.Row, e.OldRow);
			ResetSupplyPlanID = ((SOLine)e.Row).PONbr == null;

			try
			{
				base.RowUpdated(sender, e);
			}
			finally
			{
				InitPlan = false;
				ResetSupplyPlanID = false;
			}
		}

		public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
		{
			if ((((SOLine)orig_Row).RequireAllocation == true || ((SOLine)orig_Row).RequireLocation == true) && ((SOLine)orig_Row).LineType == SOLineType.Inventory || ((SOLine)orig_Row).RequireShipping == false)
			{
				return null;
			}

			if (((SOLine)orig_Row).Cancelled == true)
			{
				return null;
			}

			if (((SOLine)orig_Row).LineType == SOLineType.MiscCharge)
			{
				return null;
			}

			SOLine split_Row = (SOLine)orig_Row;

			if (string.IsNullOrEmpty(plan_Row.PlanType) || InitPlan)
			{
				if (split_Row.POCreate == true)
				{
					//SOOrderType.FixedPlanType
					if (split_Row.POType == PO.POOrderType.Blanket)
						plan_Row.PlanType = split_Row.POSource == INReplenishmentSource.DropShip
						? INPlanConstants.Plan6E
						: INPlanConstants.Plan6B;
					else if (split_Row.POSource == INReplenishmentSource.TransferToOrder)
						plan_Row.PlanType = INPlanConstants.Plan6T;
					else
						plan_Row.PlanType = split_Row.POSource == INReplenishmentSource.DropShip
						? INPlanConstants.Plan6D
						: INPlanConstants.Plan66;
					plan_Row.FixedSource = INReplenishmentSource.Purchased;
				}
				else
				{
					plan_Row.PlanType = split_Row.PlanType;
					plan_Row.Reverse  = split_Row.Operation == SOOperation.Receipt;
				}
			}

			if (ResetSupplyPlanID)
			{
				plan_Row.SupplyPlanID = null;
			}
			plan_Row.VendorID = split_Row.VendorID;
			plan_Row.VendorLocationID =
				PX.Objects.PO.POItemCostManager.FetchLocation(
				sender.Graph,
				split_Row.VendorID,
				split_Row.InventoryID,
				split_Row.SubItemID,
				split_Row.SiteID);

			plan_Row.BAccountID = ((SOLine)orig_Row).CustomerID;
			plan_Row.InventoryID = split_Row.InventoryID;
			plan_Row.SubItemID = split_Row.SubItemID;
			plan_Row.SiteID = split_Row.SiteID;			
			plan_Row.PlanDate = split_Row.ShipDate;
			plan_Row.PlanQty = split_Row.BaseOpenQty;

			PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];
			plan_Row.RefNoteID = (long?)cache.GetValue(cache.Current, _ParentNoteID.Name);
			plan_Row.Hold = (bool?)cache.GetValue(cache.Current, _ParentHoldEntry.Name);

			if (plan_Row.RefNoteID < 0L)
			{
				plan_Row.RefNoteID = null;
			}

			if (string.IsNullOrEmpty(plan_Row.PlanType))
			{
				return null;
			}
			return plan_Row;
		}
		#endregion
	}

	public class SOLine2PlanIDAttribute : INItemPlanIDAttribute
	{
		#region Ctor
		public SOLine2PlanIDAttribute()
			: base(typeof(SOOrder.noteID), typeof(SOOrder.hold))
		{
		}
		#endregion
		#region Implementation
		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
		}

		public override void Parent_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
		}

		public override void Parent_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
		}

		public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
		{
			if (((SOLine2)orig_Row).RequireLocation == true)
			{
				return null;
			}

			plan_Row.PlanType = ((SOLine2)orig_Row).PlanType;
			plan_Row.Reverse = ((SOLine2)orig_Row).Operation == SOOperation.Receipt;
			return plan_Row;
		}		

		protected ObjectRef<SOLine2> _Current;
		protected SOLine2 Current
		{
			get
			{
				return _Current.Value;
			}
			set
			{
				_Current.Value = value;
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			//should precede base method invocation
			if (sender.Graph.Views.Caches.Contains(typeof(INItemPlan)))
			{
				sender.Graph.RowInserted.AddHandler<INItemPlan>(Plan_RowInserted);
				sender.Graph.RowUpdated.AddHandler<INItemPlan>(Plan_RowUpdated);
				sender.Graph.RowDeleted.AddHandler<INItemPlan>(Plan_RowDeleted);
			}

			base.CacheAttached(sender);

			_Current = new ObjectRef<SOLine2>();
		}

		public override void Plan_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
            bool CurrentIsNull = (Current == null);
			Current = Current ?? PXSelect<SOLine2, Where<SOLine2.planID, Equal<Required<SOLine2.planID>>>>.Select(sender.Graph, ((INItemPlan)e.Row).PlanID);
			try
			{
				base.Plan_RowUpdated(sender, e);
			}
			finally
			{
                if (CurrentIsNull)
                {
                    Current = null;
                }
			}
		}

		public override void Plan_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
            bool CurrentIsNull = (Current == null);
            Current = Current ?? PXSelect<SOLine2, Where<SOLine2.planID, Equal<Required<SOLine2.planID>>>>.Select(sender.Graph, ((INItemPlan)e.Row).PlanID);
            try
			{
				base.Plan_RowDeleted(sender, e);
			}
			finally
			{
                if (CurrentIsNull)
                {
                    Current = null;
                }
			}
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Current = (SOLine2)e.Row;
			try
			{
				base.RowInserted(sender, e);
			}
			finally
			{
				Current = null;
			}
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Current = (SOLine2)e.Row;
			try
			{
				base.RowUpdated(sender, e);
			}
			finally
			{
				Current = null;
			}
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			Current = (SOLine2)e.Row;
			try
			{
				base.RowDeleted(sender, e);
			}
			finally
			{
				Current = null;
			}
		}

		protected override INPlanType GetTargetPlanType<TNode>(PXGraph graph, INItemPlan plan, INPlanType plantype)
		{
			if (Current == null)
			{
				INPlanType ret = new INPlanType();
				return ret ^ ret;
			}
			return base.GetTargetPlanType<TNode>(graph, plan, plantype);
		}
		#endregion
	}

	public class SOLineSplitPlanIDAttribute : INItemPlanIDAttribute
	{
		#region State
		protected Type _ParentOrderDate;
		#endregion
		#region Ctor
		public SOLineSplitPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry, Type ParentOrderDate)
			: base(ParentNoteID, ParentHoldEntry)
		{
			_ParentOrderDate = ParentOrderDate;
		}
		#endregion
		#region Implementation
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldDefaulting.AddHandler<SiteStatus.negAvailQty>(SiteStatus_NegAvailQty_FieldDefaulting);
		}

		protected virtual void SiteStatus_NegAvailQty_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Cancel == false && sender.Graph.Caches[typeof(SOOrderType)].Current != null && ((SOOrderType)sender.Graph.Caches[typeof(SOOrderType)].Current).RequireAllocation == true)
			{
				e.NewValue = false;
				e.Cancel = true;
			}
		}

		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			PXView view;
			WebDialogResult answer = sender.Graph.Views.TryGetValue("Document", out view) ? view.Answer : WebDialogResult.None;

			bool DatesUpdated = !sender.ObjectsEqual<SOOrder.shipDate>(e.Row, e.OldRow) && (answer == WebDialogResult.Yes || ((SOOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed);

			if (DatesUpdated || !sender.ObjectsEqual<SOOrder.hold, SOOrder.cancelled, SOOrder.shipComplete>(e.Row, e.OldRow))
			{
				DatesUpdated |= !sender.ObjectsEqual<SOOrder.shipComplete>(e.Row, e.OldRow) && ((SOOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed;

				bool Cancelled = (bool)sender.GetValue<SOOrder.cancelled>(e.Row);

				PXCache plancache = sender.Graph.Caches[typeof(INItemPlan)];
				PXCache splitcache = sender.Graph.Caches[typeof(SOLineSplit)];

				foreach (SOLineSplit split in PXSelect<SOLineSplit, Where<SOLineSplit.orderType, Equal<Current<SOOrder.orderType>>, And<SOLineSplit.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>.Select(sender.Graph))
				{
					foreach (INItemPlan plan in plancache.Inserted)
					{
						if (object.Equals(plan.PlanID, split.PlanID))
						{
							if (Cancelled)
							{
								plancache.Delete(plan);
							}
							else
							{
								plan.Hold = (bool?)sender.GetValue<SOOrder.hold>(e.Row);
								if (DatesUpdated)
								{
									plan.PlanDate = (DateTime?)sender.GetValue<SOOrder.shipDate>(e.Row);
								}
							}
						}
					}

					if (Cancelled)
					{
						split.PlanID = null;

						if (splitcache.GetStatus(split) == PXEntryStatus.Notchanged)
						{
							splitcache.SetStatus(split, PXEntryStatus.Updated);
						}
					}
					else
					{
						if ((bool)sender.GetValue<SOOrder.cancelled>(e.OldRow))
						{
							INItemPlan plan = DefaultValues(splitcache, split);
							if (plan != null)
							{
								plan = (INItemPlan)sender.Graph.Caches[typeof(INItemPlan)].Insert(plan);
								split.PlanID = plan.PlanID;
								if (splitcache.GetStatus(split) == PXEntryStatus.Notchanged)
									splitcache.SetStatus(split, PXEntryStatus.Updated);
							}
							splitcache.SetStatus(split, PXEntryStatus.Updated);
						}

						if (DatesUpdated)
						{
							split.ShipDate = (DateTime?)sender.GetValue<SOOrder.shipDate>(e.Row);
							if (splitcache.GetStatus(split) == PXEntryStatus.Notchanged)
							{
								splitcache.SetStatus(split, PXEntryStatus.Updated);
							}
						}
					}
				}

				foreach (INItemPlan plan in PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<SOOrder.noteID>>>>.Select(sender.Graph))
				{
					if (Cancelled)
					{
						plancache.Delete(plan);
					}
					else
					{
						plan.Hold = (bool?)sender.GetValue<SOOrder.hold>(e.Row);
						if (DatesUpdated)
						{
							plan.PlanDate = (DateTime?)sender.GetValue<SOOrder.shipDate>(e.Row);
						}

						if (plancache.GetStatus(plan) == PXEntryStatus.Notchanged || plancache.GetStatus(plan) == PXEntryStatus.Held)
						{
							plancache.SetStatus(plan, PXEntryStatus.Updated);
						}
					}
				}
			}
		}

		public override void Parent_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				_KeyToAbort = sender.GetValue<SOOrder.orderNbr>(e.Row);
			}
		}

		public override void Parent_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open && _KeyToAbort != null)
			{
				foreach (SOLineSplit split in PXSelect<SOLineSplit, Where<SOLineSplit.orderType, Equal<Required<SOOrder.orderType>>, And<SOLineSplit.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>.Select(sender.Graph, ((SOOrder)e.Row).OrderType, _KeyToAbort))
				{
					foreach (INItemPlan plan in sender.Graph.Caches[typeof(INItemPlan)].Inserted)
					{
						if (object.Equals(plan.PlanID, split.PlanID))
						{
							plan.RefNoteID = (long?)sender.GetValue(e.Row, _ParentNoteID.Name);
						}
					}
				}
			}
			_KeyToAbort = null;
		}

		public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
		{
			if (((SOLineSplit)orig_Row).Cancelled == true || ((SOLineSplit)orig_Row).Released == true || ((SOLineSplit)orig_Row).IsStockItem == false)
			{
				return null;
			}
			SOLine parent = (SOLine)PXParentAttribute.SelectParent(sender, orig_Row);

			SOLineSplit split_Row = (SOLineSplit)orig_Row;			
			plan_Row.PlanType = (split_Row.RequireAllocation != true || split_Row.IsAllocated == true) ? split_Row.PlanType : split_Row.BackOrderPlanType;
			plan_Row.Reverse = parent != null ? parent.Operation == SOOperation.Receipt : false;
			plan_Row.BAccountID = parent == null ? null : parent.CustomerID;
			//plan_Row.BAccountID = parent.CustomerID;
			plan_Row.InventoryID = split_Row.InventoryID;
			plan_Row.SubItemID = split_Row.SubItemID;
			plan_Row.SiteID = split_Row.SiteID;
			plan_Row.LocationID = split_Row.LocationID;
			plan_Row.LotSerialNbr = split_Row.LotSerialNbr;
			if (string.IsNullOrEmpty(split_Row.AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(split_Row.AssignedNbr, split_Row.LotSerialNbr))
			{
				plan_Row.LotSerialNbr = null;
			}
			plan_Row.PlanDate = split_Row.ShipDate;
			plan_Row.PlanQty = split_Row.BaseQty;

			PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];
			plan_Row.RefNoteID = (long?)cache.GetValue(cache.Current, _ParentNoteID.Name);
			plan_Row.Hold = (bool?)cache.GetValue(cache.Current, _ParentHoldEntry.Name);

			if (plan_Row.RefNoteID < 0L)
			{
				plan_Row.RefNoteID = null;
			}

			if (string.IsNullOrEmpty(plan_Row.PlanType))
			{
				return null;
			}
			return plan_Row;
		}
		#endregion
	}

	public class SOShipLineSplitPlanIDAttribute : INItemPlanIDAttribute
	{
		#region State
		protected Type _ParentOrderDate;
		#endregion
		#region Ctor
		public SOShipLineSplitPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry, Type ParentOrderDate)
			: base(ParentNoteID, ParentHoldEntry)
		{
			_ParentOrderDate = ParentOrderDate;
		}
		#endregion
		#region Implementation
		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<SOShipment.shipDate, SOShipment.hold>(e.Row, e.OldRow))
			{
				PXCache plancache = sender.Graph.Caches[typeof(INItemPlan)];
				foreach (SOShipLineSplit split in PXSelect<SOShipLineSplit, Where<SOShipLineSplit.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>>.Select(sender.Graph))
				{
					foreach (INItemPlan plan in plancache.Inserted)
					{
						if (object.Equals(plan.PlanID, split.PlanID))
						{
							plan.Hold = (bool?)sender.GetValue<SOShipment.hold>(e.Row);
							plan.PlanDate = (DateTime?)sender.GetValue<SOShipment.shipDate>(e.Row);
						}
					}
				}

				foreach (INItemPlan plan in PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<SOShipment.noteID>>>>.Select(sender.Graph))
				{
					plan.Hold = (bool?)sender.GetValue<SOShipment.hold>(e.Row);
					plan.PlanDate = (DateTime?)sender.GetValue<SOShipment.shipDate>(e.Row);

					if (plancache.GetStatus(plan) == PXEntryStatus.Notchanged || plancache.GetStatus(plan) == PXEntryStatus.Held)
					{
						plancache.SetStatus(plan, PXEntryStatus.Updated);
					}
				}
			}
		}

		public override void Parent_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				_KeyToAbort = sender.GetValue<SOShipment.shipmentNbr>(e.Row);
			}
		}

		public override void Parent_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open && _KeyToAbort != null)
			{
				foreach (SOShipLineSplit split in PXSelect<SOShipLineSplit, Where<SOShipLineSplit.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>>>.Select(sender.Graph,  _KeyToAbort))
				{
					foreach (INItemPlan plan in sender.Graph.Caches[typeof(INItemPlan)].Inserted)
					{
						if (object.Equals(plan.PlanID, split.PlanID))
						{
							plan.RefNoteID = (long?)sender.GetValue(e.Row, _ParentNoteID.Name);
						}
					}
				}
			}
			_KeyToAbort = null;
		}

		public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
		{
			if (((SOShipLineSplit)orig_Row).Released == true || ((SOShipLineSplit)orig_Row).IsStockItem == false)
			{
				return null;
			}
			SOShipLine parent = (SOShipLine)PXParentAttribute.SelectParent(sender, orig_Row);

			if (parent == null) return null;

			SOShipLineSplit split_Row = (SOShipLineSplit)orig_Row;
			plan_Row.BAccountID = parent.CustomerID;
			plan_Row.PlanType = split_Row.PlanType;
			plan_Row.InventoryID = split_Row.InventoryID;
			plan_Row.Reverse = split_Row.Operation == SOOperation.Receipt;
			plan_Row.SubItemID = split_Row.SubItemID;
			plan_Row.SiteID = split_Row.SiteID;
			plan_Row.LocationID = split_Row.LocationID;
			plan_Row.LotSerialNbr = split_Row.LotSerialNbr;
			if (string.IsNullOrEmpty(split_Row.AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(split_Row.AssignedNbr, split_Row.LotSerialNbr))
			{
				plan_Row.LotSerialNbr = null;
			}
			plan_Row.PlanDate = split_Row.ShipDate;
			plan_Row.PlanQty = split_Row.BaseQty;

			PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];
			plan_Row.RefNoteID = (long?)cache.GetValue(cache.Current, _ParentNoteID.Name);
			plan_Row.Hold = (bool?)cache.GetValue(cache.Current, _ParentHoldEntry.Name);

			if (plan_Row.RefNoteID < 0L)
			{
				plan_Row.RefNoteID = null;
			}

			if (string.IsNullOrEmpty(plan_Row.PlanType))
			{
				return null;
			}
			return plan_Row;
		}	

		protected ObjectRef<SOShipLineSplit> _Current;
		protected SOShipLineSplit Current
		{
			get
			{
				return _Current.Value;
			}
			set
			{
				_Current.Value = value;
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			//should precede base method invocation
			if (sender.Graph.Views.Caches.Contains(typeof(INItemPlan)))
			{
				sender.Graph.RowInserted.AddHandler<INItemPlan>(Plan_RowInserted);
				sender.Graph.RowUpdated.AddHandler<INItemPlan>(Plan_RowUpdated);
				sender.Graph.RowDeleted.AddHandler<INItemPlan>(Plan_RowDeleted);
			}

			base.CacheAttached(sender);

			_Current = new ObjectRef<SOShipLineSplit>();
			sender.Graph.FieldDefaulting.AddHandler<SiteStatus.negAvailQty>(SiteStatus_NegAvailQty_FieldDefaulting);
		}

		protected virtual void SiteStatus_NegAvailQty_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			INItemClass itemclass = PXSelect<INItemClass, Where<INItemClass.itemClassID, Equal<Current<SiteStatus.itemClassID>>>>.SelectMultiBound(sender.Graph, new object[] { e.Row });
			if (itemclass != null)
			{
				e.NewValue = itemclass.NegQty;
				e.Cancel = true;
			}
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Current = (SOShipLineSplit)e.Row;
			try
			{
				base.RowInserted(sender, e);
			}
			finally
			{
				Current = null;
			}
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Current = (SOShipLineSplit)e.Row;
			try
			{
				base.RowUpdated(sender, e);
			}
			finally
			{
				Current = null;
			}
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			Current = (SOShipLineSplit)e.Row;
			try
			{
				base.RowDeleted(sender, e);
			}
			finally
			{
				Current = null;
			}
		}

		public override void Plan_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
            bool CurrentIsNull = (Current == null);
            Current = Current ?? PXSelect<SOShipLineSplit, Where<SOShipLineSplit.planID, Equal<Required<SOShipLineSplit.planID>>>>.Select(sender.Graph, ((INItemPlan)e.Row).PlanID);
			try
			{
				base.Plan_RowUpdated(sender, e);
			}
			finally
			{
                if (CurrentIsNull)
                {
                    Current = null;
                }
			}
		}

		public override void Plan_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			//Current can be set by RowDeleted
            bool CurrentIsNull = (Current == null);
            Current = Current ?? PXSelect<SOShipLineSplit, Where<SOShipLineSplit.planID, Equal<Required<SOShipLineSplit.planID>>>>.Select(sender.Graph, ((INItemPlan)e.Row).PlanID);
			try
			{
				base.Plan_RowDeleted(sender, e);
			}
			finally
			{
                if (CurrentIsNull)
                {
                    Current = null;
                }
			}
		}

		public override decimal GetInclQtyAvail<TNode>(PXGraph graph, object data)
		{
			Current = (SOShipLineSplit)data;
			try
			{
				return base.GetInclQtyAvail<TNode>(graph, data);
			}
			finally
			{
				Current = null;
			}
		}

		public override decimal GetInclQtyHardAvail<TNode>(PXGraph graph, object data)
		{
			Current = (SOShipLineSplit)data;
			try
			{
				return base.GetInclQtyHardAvail<TNode>(graph, data);
			}
			finally
			{
				Current = null;
			}
		}

		protected override INPlanType GetTargetPlanType<TNode>(PXGraph graph, INItemPlan plan, INPlanType plantype)
		{
			if (Current == null)
			{
				INPlanType ret = new INPlanType();
				return ret ^ ret;
			}

            PXResult<INPlanType> res = PXSelectJoin<INPlanType,
                InnerJoin<SOOrderTypeOperation, On<SOOrderTypeOperation.orderPlanType, Equal<INPlanType.planType>>,
                InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrderTypeOperation.orderType>>>>,
			Where<SOOrderTypeOperation.orderType, Equal<Current<SOShipLineSplit.origOrderType>>,
				And<SOOrderTypeOperation.operation, Equal<Current<SOShipLineSplit.operation>>>>>
				.SelectSingleBound(graph, new object[] { Current });

            INPlanType orderplantype = res;

			if (orderplantype == plantype || orderplantype == -plantype)
			{
				if (typeof(TNode) != typeof(SiteStatus))
				{
					return plantype;
				}
				else
				{
					return plantype * (orderplantype ^ plantype);
				}
			}

			if (typeof(TNode) != typeof(SiteStatus) || Current.IsComponentItem == true)
			{
                if (res.GetItem<SOOrderType>().RequireLocation == false || orderplantype == plantype)
				{
					return plantype * (orderplantype ^ plantype);
				}
			}

			return plantype;
		}
		#endregion
	}

	public class LSSOLine : LSSelect<SOLine, SOLineSplit, 
		Where<SOLineSplit.orderType, Equal<Current<SOOrder.orderType>>,
        And<SOLineSplit.orderNbr, Equal<Current<SOOrder.orderNbr>>>>>
	{
		#region State
		public bool IsLSEntryEnabled
		{
			get
			{
				return (this._Graph.Caches[typeof(SOOrderType)].Current == null || ((SOOrderType)this._Graph.Caches[typeof(SOOrderType)].Current).RequireLocation == true);
			}
		}
		public bool IsAllocationEntryEnabled
		{
			get
			{
				return (this._Graph.Caches[typeof(SOOrderType)].Current == null || ((SOOrderType)this._Graph.Caches[typeof(SOOrderType)].Current).RequireAllocation == true);
			}
		}
		#endregion
		#region Ctor
		public LSSOLine(PXGraph graph)
			: base(graph)
		{
			MasterQtyField = typeof(SOLine.orderQty);
			graph.FieldDefaulting.AddHandler<SOLineSplit.subItemID>(SOLineSplit_SubItemID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<SOLineSplit.locationID>(SOLineSplit_LocationID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<SOLineSplit.invtMult>(SOLineSplit_InvtMult_FieldDefaulting);
			graph.RowSelected.AddHandler<SOOrder>(Parent_RowSelected);
			graph.RowUpdated.AddHandler<SOOrder>(SOOrder_RowUpdated);
			graph.RowSelected.AddHandler<SOLine>(SOLine_RowSelected);			
			graph.RowSelected.AddHandler<SOLineSplit>(SOLineSplit_RowSelected);
			graph.RowPersisting.AddHandler<SOLineSplit>(SOLineSplit_RowPersisting);
		}
		#endregion

		#region Implementation
		public override IEnumerable BinLotSerial(PXAdapter adapter)
		{
			if (IsLSEntryEnabled || IsAllocationEntryEnabled)
			{
				if (MasterCache.Current != null && ((SOLine)MasterCache.Current).PlanID != null)
				{
					throw new PXSetPropertyException(Messages.BinLotSerialInvalid);
				}

				View.AskExt(true);
			}
			return adapter.Get();
		}

		protected virtual void SOOrder_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (IsLSEntryEnabled && !sender.ObjectsEqual<SOOrder.hold>(e.Row, e.OldRow) && (bool?)sender.GetValue<SOOrder.hold>(e.Row) == false)
			{
				PXCache cache = sender.Graph.Caches[typeof(SOLine)];

				foreach (SOLine item in PXParentAttribute.SelectSiblings(cache, null, typeof(SOOrder)))
				{
					if (Math.Abs((decimal)item.BaseQty) >= 0.0000005m && item.UnassignedQty >= 0.0000005m)
					{
						cache.RaiseExceptionHandling<SOLine.orderQty>(item, item.Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned));

						if (cache.GetStatus(item) == PXEntryStatus.Notchanged)
						{
							cache.SetStatus(item, PXEntryStatus.Updated);
						}
					}
				}
			}
		}

		protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				if ((((SOLine)e.Row).LineType == SOLineType.Inventory || ((SOLine)e.Row).LineType == SOLineType.NonInventory) && ((SOLine)e.Row).TranType != INTranType.NoUpdate && ((SOLine)e.Row).InvtMult == (short)-1 && ((SOLine)e.Row).BaseQty < 0m)
				{
					if (sender.RaiseExceptionHandling<SOLine.orderQty>(e.Row, ((SOLine)e.Row).Qty, new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString())))
					{
						throw new PXRowPersistingException(typeof(SOLine.orderQty).Name, ((SOLine)e.Row).Qty, CS.Messages.Entry_GE, ((int)0).ToString());
					}
				}

				if (IsLSEntryEnabled)
				{
					PXCache cache = sender.Graph.Caches[typeof(SOOrder)];
					object doc = PXParentAttribute.SelectParent(sender, e.Row, typeof(SOOrder)) ?? cache.Current;

					bool? OnHold = (bool?)cache.GetValue<SOOrder.hold>(doc);

					if (OnHold == false && Math.Abs((decimal)((SOLine)e.Row).BaseQty) >= 0.0000005m && ((SOLine)e.Row).UnassignedQty >= 0.0000005m)
					{
						if (sender.RaiseExceptionHandling<SOLine.orderQty>(e.Row, ((SOLine)e.Row).Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned)))
						{
							throw new PXRowPersistingException(typeof(SOLine.orderQty).Name, ((SOLine)e.Row).Qty, Messages.BinLotSerialNotAssigned);
						}
					}
				}
			}
			base.Master_RowPersisting(sender, e);
		}
		
		public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			IQtyAllocated availability = AvailabilityFetch(sender, (SOLine)e.Row, true);

			if (availability != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((SOLine)e.Row).InventoryID);

				availability.QtyOnHand = INUnitAttribute.ConvertFromBase<SOLine.inventoryID, SOLine.uOM>(sender, e.Row, (decimal)availability.QtyOnHand, INPrecision.QUANTITY);
				availability.QtyAvail = INUnitAttribute.ConvertFromBase<SOLine.inventoryID, SOLine.uOM>(sender, e.Row, (decimal)availability.QtyAvail, INPrecision.QUANTITY);
				availability.QtyNotAvail = INUnitAttribute.ConvertFromBase<SOLine.inventoryID, SOLine.uOM>(sender, e.Row, (decimal)availability.QtyNotAvail, INPrecision.QUANTITY);
				availability.QtyHardAvail = INUnitAttribute.ConvertFromBase<SOLine.inventoryID, SOLine.uOM>(sender, e.Row, (decimal)availability.QtyHardAvail, INPrecision.QUANTITY);
				if(IsAllocationEntryEnabled)
				{
					Decimal? allocated = ((SOLine)e.Row).BaseOrderQty - ((SOLine)e.Row).BaseShippedQty;
					foreach (SOLineSplit splint in this.SelectDetailReversed(this.DetailCache, (SOLine)e.Row))
					{
						if (splint.IsAllocated == true && splint.Cancelled != true) break;
						if (splint.BaseShippedQty == 0)
							allocated -= splint.BaseQty;
					}
					if(allocated != null)
						allocated = INUnitAttribute.ConvertFromBase<SOLine.inventoryID, SOLine.uOM>(sender, e.Row, (decimal)allocated,INPrecision.QUANTITY);

					e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(Messages.Availability_AllocatedInfo,
							sender.GetValue<SOLine.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail), FormatQty(allocated));
				}
				else
					e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(Messages.Availability_Info,
							sender.GetValue<SOLine.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail));


				AvailabilityCheck(sender, (SOLine)e.Row, availability);
			}
			else
			{
				//handle missing UOM
				INUnitAttribute.ConvertFromBase<SOLine.inventoryID, SOLine.uOM>(sender, e.Row, 0m, INPrecision.QUANTITY);
				e.ReturnValue = string.Empty;
			}

			base.Availability_FieldSelecting(sender, e);
		}

		protected SOOrder _LastSelected;

		protected virtual void Parent_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (_LastSelected == null || !object.ReferenceEquals(_LastSelected, e.Row))
			{
				PXUIFieldAttribute.SetRequired<SOLine.locationID>(this.MasterCache, IsLSEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLine.locationID>(this.MasterCache, null, IsLSEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLine.lotSerialNbr>(this.MasterCache, null, IsLSEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLine.expireDate>(this.MasterCache, null, IsLSEntryEnabled);

				PXUIFieldAttribute.SetVisible<SOLineSplit.locationID>(this.DetailCache, null, IsLSEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.lotSerialNbr>(this.DetailCache, null, IsLSEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.expireDate>(this.DetailCache, null, IsLSEntryEnabled);

				PXUIFieldAttribute.SetVisible<SOLineSplit.shipDate>(this.DetailCache, null, IsAllocationEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.isAllocated>(this.DetailCache, null, IsAllocationEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.cancelled>(this.DetailCache, null, IsAllocationEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.shippedQty>(this.DetailCache, null, IsAllocationEntryEnabled);
				PXUIFieldAttribute.SetVisible<SOLineSplit.shipmentNbr>(this.DetailCache, null, IsAllocationEntryEnabled);

				if (e.Row is SOOrder)
				{
					_LastSelected = (SOOrder)e.Row;
				}
			}
			this.SetEnabled(IsLSEntryEnabled || IsAllocationEntryEnabled);
		}

		protected virtual void IssueAvailable(PXCache sender, SOLine Row, decimal? BaseQty)
		{
			DetailCounters.Remove(Row);
			foreach (INSiteStatus avail in PXSelectReadonly<INSiteStatus,
				Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>,
				And<INSiteStatus.subItemID, Equal<Required<INSiteStatus.subItemID>>,
				And<INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>>>>,
				OrderBy<Asc<INLocation.pickPriority>>>.Select(this._Graph, Row.InventoryID, Row.SubItemID, Row.SiteID))
			{
				SOLineSplit split = (SOLineSplit)Row;
				split.SplitLineNbr = null;
				split.IsAllocated = true;

				decimal InclQtyHardAvail = INItemPlanIDAttribute.GetInclQtyHardAvail<SiteStatus>(DetailCache, split);

				if (InclQtyHardAvail < 0m)
				{
					SiteStatus accumavail = new SiteStatus();
					PXCache<INSiteStatus>.RestoreCopy(accumavail, avail);

					accumavail = (SiteStatus)this._Graph.Caches[typeof(SiteStatus)].Insert(accumavail);

					decimal? AvailableQty = avail.QtyHardAvail + accumavail.QtyHardAvail;

					if (AvailableQty <= 0m)
					{
						continue;
					}

					if (AvailableQty < BaseQty)
					{
						split.BaseQty = AvailableQty;
						split.Qty = INUnitAttribute.ConvertFromBase(MasterCache, split.InventoryID, split.UOM, (decimal)AvailableQty, INPrecision.QUANTITY);
						DetailCache.Insert(split);

						BaseQty -= AvailableQty;
					}
					else
					{
						split.BaseQty = BaseQty;
						split.Qty = INUnitAttribute.ConvertFromBase(MasterCache, split.InventoryID, split.UOM, (decimal)BaseQty, INPrecision.QUANTITY);
						DetailCache.Insert(split);

						BaseQty = 0m;
						break;
					}
				}
			}

			if (BaseQty > 0m && Row.InventoryID != null && Row.SubItemID != null && Row.SiteID != null)
			{
				SOLineSplit split = (SOLineSplit)Row;
				split.SplitLineNbr = null;
				split.IsAllocated = false;
				split.BaseQty = BaseQty;
				split.Qty = INUnitAttribute.ConvertFromBase(MasterCache, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);

				BaseQty = 0m;

				DetailCache.Insert(PXCache<SOLineSplit>.CreateCopy(split));
			}
		}

		public override void UpdateParent(PXCache sender, SOLine Row)
		{
			if (Row != null && Row.RequireAllocation == true)
			{
				decimal BaseQty;
				UpdateParent(sender, Row, null, null, out BaseQty);
			}
			else
			{
				base.UpdateParent(sender, Row);
			}
		}

		public override void UpdateParent(PXCache sender, SOLineSplit Row, SOLineSplit OldRow)
		{
			SOLine parent = (SOLine)LSParentAttribute.SelectParent(sender, Row ?? OldRow, typeof(SOLine));

			if (parent != null && parent.RequireAllocation == true)
			{
				if ((Row ?? OldRow) != null && SameInventoryItem((ILSMaster)(Row ?? OldRow), (ILSMaster)parent))
				{
					SOLine oldrow = PXCache<SOLine>.CreateCopy(parent);
					decimal BaseQty;

					UpdateParent(sender, parent, (Row != null && Row.Cancelled == false ? Row : null), (OldRow != null && OldRow.Cancelled == false ? OldRow : null), out BaseQty);

					using (InvtMultScope<SOLine> ms = new InvtMultScope<SOLine>(parent))
					{
						parent.BaseQty = BaseQty + parent.BaseClosedQty;
						parent.Qty = INUnitAttribute.ConvertFromBase(sender, parent.InventoryID, parent.UOM, (decimal)parent.BaseQty, INPrecision.QUANTITY);
					}

					if (sender.Graph.Caches[typeof(SOLine)].GetStatus(parent) == PXEntryStatus.Notchanged)
					{
						sender.Graph.Caches[typeof(SOLine)].SetStatus(parent, PXEntryStatus.Updated);
					}

					sender.Graph.Caches[typeof(SOLine)].RaiseFieldUpdated(_MasterQtyField, parent, oldrow.Qty);
					if (sender.Graph.Caches[typeof(SOLine)].RaiseRowUpdating(parent, oldrow))
					{
						sender.Graph.Caches[typeof(SOLine)].RaiseRowUpdated(parent, oldrow);
					}
					else
					{
						sender.Graph.Caches[typeof(SOLine)].RestoreCopy(parent, oldrow);
					}
				}
			}
			else
			{
				base.UpdateParent(sender, Row, OldRow);
			}
		}

		protected override object[] SelectDetail(PXCache sender, SOLineSplit row)
		{
			object[] ret = base.SelectDetail(sender, row);

			return Array.FindAll<object>(ret, new Predicate<object>(delegate(object a)
			{
				return ((SOLineSplit)a).Cancelled == false;
			}));
		}

		protected override object[] SelectDetail(PXCache sender, SOLine row)
		{
			object[] ret = base.SelectDetail(sender, row);

			return Array.FindAll<object>(ret, new Predicate<object>(delegate(object a)
			{
				return ((SOLineSplit)a).Cancelled == false;
			}));
		}

		protected override object[] SelectDetailOrdered(PXCache sender, SOLineSplit row)
		{
			object[] ret = SelectDetail(sender, row);

			Array.Sort<object>(ret, new Comparison<object>(delegate(object a, object b)
			{
				object aIsAllocated = ((SOLineSplit)a).Cancelled == true ? 0 : ((SOLineSplit)a).IsAllocated == true ? 1 : 2;
				object bIsAllocated = ((SOLineSplit)b).Cancelled == true ? 0 : ((SOLineSplit)b).IsAllocated == true ? 1 : 2;

				int res = ((IComparable)aIsAllocated).CompareTo(bIsAllocated);

				if (res != 0)
				{
					return res;
				}

				object aSplitLineNbr = ((SOLineSplit)a).SplitLineNbr;
				object bSplitLineNbr = ((SOLineSplit)b).SplitLineNbr;

				return ((IComparable)aSplitLineNbr).CompareTo(bSplitLineNbr);
			}));

			return ret;
		}

		protected override object[] SelectDetailReversed(PXCache sender, SOLineSplit row)
		{
			object[] ret = SelectDetail(sender, row);

			Array.Sort<object>(ret, new Comparison<object>(delegate(object a, object b)
			{
				object aIsAllocated = ((SOLineSplit)a).Cancelled == true ? 0 : ((SOLineSplit)a).IsAllocated == true ? 1 : 2;
				object bIsAllocated = ((SOLineSplit)b).Cancelled == true ? 0 : ((SOLineSplit)b).IsAllocated == true ? 1 : 2;

				int res = -((IComparable)aIsAllocated).CompareTo(bIsAllocated);

				if (res != 0)
				{
					return res;
				}

				object aSplitLineNbr = ((SOLineSplit)a).SplitLineNbr;
				object bSplitLineNbr = ((SOLineSplit)b).SplitLineNbr;

				return -((IComparable)aSplitLineNbr).CompareTo(bSplitLineNbr);
			}));

			return ret;
		}

		public virtual void TruncateSchedules(PXCache sender, SOLine Row, decimal BaseQty)
		{
			DetailCounters.Remove(Row);
			PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);

			foreach (object detail in SelectDetailReversed(DetailCache, Row))
			{
				if (BaseQty >= ((ILSDetail)detail).BaseQty)
				{
					BaseQty -= (decimal)((ILSDetail)detail).BaseQty;
					DetailCache.Delete(detail);
				}
				else
				{
					SOLineSplit newdetail = PXCache<SOLineSplit>.CreateCopy((SOLineSplit)detail);
					newdetail.BaseQty -= BaseQty;
					newdetail.Qty = INUnitAttribute.ConvertFromBase(sender, newdetail.InventoryID, newdetail.UOM, (decimal)newdetail.BaseQty, INPrecision.QUANTITY);

					DetailCache.Update(newdetail);
					break;
				}
			}
		}

		public virtual void UpdateSchedules(PXCache sender, SOLine Row)
		{
			foreach (object detail in SelectDetail(DetailCache, Row))
			{
				SOLineSplit copy = PXCache<SOLineSplit>.CreateCopy((SOLineSplit)detail);
				copy.ShipDate = Row.ShipDate;

				DetailCache.Update(copy);
			}
		}

		protected virtual void IssueAvailable(PXCache sender, SOLine Row)
		{
			IssueAvailable(sender, Row, Row.BaseOpenQty);
		}

		protected override void _Master_RowInserted(PXCache sender, PXRowInsertedEventArgs<SOLine> e)
		{
			if (IsLSEntryEnabled)
			{
				base._Master_RowInserted(sender, e);
			}
			else
			{
				sender.SetValue<SOLine.locationID>(e.Row, null);
				sender.SetValue<SOLine.lotSerialNbr>(e.Row, null);
				sender.SetValue<SOLine.expireDate>(e.Row, null);

				if (IsAllocationEntryEnabled)
				{

					PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, e.Row.InventoryID);

					if (e.Row.InvtMult == -1 && item != null && ((InventoryItem)item).StkItem == true)
					{
						IssueAvailable(sender, e.Row);

					}
				}
				AvailabilityCheck(sender, e.Row);
			}
		}

		protected override void _Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs<SOLine> e)
		{
			if (IsLSEntryEnabled)
			{
				base._Master_RowUpdated(sender, e);
			}
			else 
			{
				sender.SetValue<SOLine.locationID>(e.Row, null);
				sender.SetValue<SOLine.lotSerialNbr>(e.Row, null);
				sender.SetValue<SOLine.expireDate>(e.Row, null);

				if (IsAllocationEntryEnabled)
				{
					PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, e.Row.InventoryID);

					if (e.OldRow != null && (e.OldRow.InventoryID != e.Row.InventoryID || e.OldRow.SiteID != e.Row.SiteID || e.OldRow.SubItemID != e.Row.SubItemID || e.OldRow.InvtMult != e.Row.InvtMult || e.OldRow.UOM != e.Row.UOM))
					{
						RaiseRowDeleted(sender, e.OldRow);
						RaiseRowInserted(sender, e.Row);
					}
					else if (e.Row.InvtMult == -1 && item != null && ((InventoryItem)item).StkItem == true)
					{
						//respond only to GUI, not on changes to openqty in ConfirmShipment(), CorrectShipment()
						//OpenQty is calculated via formulae, ExternalCall is used to eliminate duplicating formula arguments here
						//direct OrderQty for AddItem()
						if (e.Row.OrderQty != e.OldRow.OrderQty || e.ExternalCall)
						{
							e.Row.BaseOpenQty = INUnitAttribute.ConvertToBase(sender, e.Row.InventoryID, e.Row.UOM, (decimal)e.Row.OpenQty,
																															INPrecision.QUANTITY);

							if (e.Row.BaseOpenQty > e.OldRow.BaseOpenQty)
							{
								IssueAvailable(sender, e.Row, (decimal)e.Row.BaseOpenQty - (decimal)e.OldRow.BaseOpenQty);
								UpdateParent(sender, e.Row);
							}
							else if (e.Row.BaseOpenQty < e.OldRow.BaseOpenQty)
							{
								TruncateSchedules(sender, e.Row, (decimal)e.OldRow.BaseOpenQty - (decimal)e.Row.BaseOpenQty);
								UpdateParent(sender, e.Row);
							}
						}
						
						if (e.OldRow.ShipDate != e.Row.ShipDate)
						{
							UpdateSchedules(sender, e.Row);
						}
					}
				}
				AvailabilityCheck(sender, (SOLine)e.Row);
			}
		}

		protected virtual bool SchedulesEqual(SOLineSplit a, SOLineSplit b)
		{
			if (a != null && b != null)
			{
				return (a.InventoryID == b.InventoryID &&
								a.SubItemID == b.SubItemID &&
								a.SiteID == b.SiteID &&
								a.ShipDate == b.ShipDate &&
								a.IsAllocated == b.IsAllocated &&
								a.Cancelled == b.Cancelled);
			}
			else
			{
				return (a != null);
			}
		}

		protected override void Detail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (IsLSEntryEnabled)
			{
				if (e.ExternalCall)
				{
					SOLine parent = (SOLine)LSParentAttribute.SelectParent(sender, e.Row, typeof(SOLine));

					if (parent != null && parent.PlanID != null)
					{
						throw new PXSetPropertyException(ErrorMessages.CantInsertRecord);
					}
				}

				base.Detail_RowInserting(sender, e);
			}
			else if (IsAllocationEntryEnabled)
			{
				SOLineSplit a = (SOLineSplit)e.Row;

				if (!e.ExternalCall)
				{
					foreach (object item in SelectDetail(sender, (SOLineSplit)e.Row))
					{
						SOLineSplit detailitem = (SOLineSplit)item;

						if (SchedulesEqual((SOLineSplit)e.Row, detailitem))
						{
							object old_item = PXCache<SOLineSplit>.CreateCopy(detailitem);
							detailitem.BaseQty += ((SOLineSplit)e.Row).BaseQty;
							detailitem.Qty = INUnitAttribute.ConvertFromBase(sender, detailitem.InventoryID, detailitem.UOM, (decimal)detailitem.BaseQty, INPrecision.QUANTITY);

							sender.RaiseRowUpdated(detailitem, old_item);
							if (sender.GetStatus(detailitem) == PXEntryStatus.Notchanged)
							{
								sender.SetStatus(detailitem, PXEntryStatus.Updated);
							}

							e.Cancel = true;
							break;
						}
					}
				}
				else
				{
					SOLine parent = (SOLine)LSParentAttribute.SelectParent(sender, e.Row, typeof(SOLine));

					if (parent != null && parent.PlanID != null)
					{
						throw new PXSetPropertyException(ErrorMessages.CantInsertRecord);
					}
				}

				if (((SOLineSplit)e.Row).InventoryID == null || string.IsNullOrEmpty(((SOLineSplit)e.Row).UOM))
				{
					e.Cancel = true;
				}

				if (!e.Cancel)
				{
				}
			}
		}

		protected virtual bool Allocated_Updated(PXCache sender, EventArgs e)
		{
			SOLineSplit split = (SOLineSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).Row : ((PXRowInsertedEventArgs)e).Row);
			SiteStatus accum = new SiteStatus();
			accum.InventoryID = split.InventoryID;
			accum.SiteID = split.SiteID;
			accum.SubItemID = split.SubItemID;

			accum = (SiteStatus)sender.Graph.Caches[typeof(SiteStatus)].Insert(accum);
			accum = PXCache<SiteStatus>.CreateCopy(accum);

			INSiteStatus stat = PXSelectReadonly<INSiteStatus, Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>, And<INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>, And<INSiteStatus.subItemID, Equal<Required<INSiteStatus.subItemID>>>>>>.Select(sender.Graph, split.InventoryID, split.SiteID, split.SubItemID);
			if (stat != null)
			{
				accum.QtyAvail += stat.QtyAvail;
				accum.QtyHardAvail += stat.QtyHardAvail;
			}

			if (accum.QtyHardAvail < 0m)
			{
				SOLineSplit copy = PXCache<SOLineSplit>.CreateCopy(split);
				if (split.BaseQty + accum.QtyHardAvail > 0m)
				{
					split.BaseQty += accum.QtyHardAvail;
					split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
				}
				else
				{
					split.IsAllocated = false;
					sender.RaiseExceptionHandling<SOLineSplit.isAllocated>(split, true, new PXSetPropertyException(IN.Messages.Inventory_Negative2));
				}

				sender.RaiseRowUpdated(split, copy);

				if (split.IsAllocated == true)
				{
					copy.SplitLineNbr = null;
					copy.PlanID = null;
					copy.IsAllocated = false;
					copy.BaseQty = -accum.QtyHardAvail;
					copy.Qty = INUnitAttribute.ConvertFromBase(MasterCache, copy.InventoryID, copy.UOM, (decimal)copy.BaseQty, INPrecision.QUANTITY);

					sender.Insert(copy);
				}
				RefreshView(sender);
				
				return true;
			}
			return false;
		}
		private void RefreshView(PXCache sender)
		{
			foreach (KeyValuePair<string, PXView> pair in sender.Graph.Views)
			{
				PXView view = pair.Value;
				if (view.IsReadOnly == false && view.GetItemType() == sender.GetItemType())
				{
					view.RequestRefresh();
				}
			}
		}

		protected override void Detail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			base.Detail_RowInserted(sender, e);

			if (_InternallCall == false && IsAllocationEntryEnabled)
			{
				if (((SOLineSplit)e.Row).IsAllocated == true)
				{
					Allocated_Updated(sender, e);

					sender.RaiseExceptionHandling<SOLineSplit.qty>(e.Row, null, null);
					AvailabilityCheck(sender, (SOLineSplit)e.Row);
				}
			}
		}

		protected override void Detail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.Detail_RowUpdated(sender, e);

			if (_InternallCall == false && IsAllocationEntryEnabled)
			{
				if (!sender.ObjectsEqual<SOLineSplit.isAllocated>(e.Row, e.OldRow))
				{
					if (((SOLineSplit)e.Row).IsAllocated == true)
					{
						Allocated_Updated(sender, e);

						sender.RaiseExceptionHandling<SOLineSplit.qty>(e.Row, null, null);
						AvailabilityCheck(sender, (SOLineSplit) e.Row);
					}
					else
					{
						foreach(SOLineSplit s in this.SelectDetailReversed(sender,(SOLineSplit) e.Row))
						{
							if(s.SplitLineNbr != ((SOLineSplit) e.Row).SplitLineNbr &&
								s.IsAllocated == false && s.ShipDate == ((SOLineSplit)e.Row).ShipDate)
							{
								((SOLineSplit)e.Row).Qty += s.Qty;
								((SOLineSplit)e.Row).BaseQty += s.BaseQty;
								sender.SetStatus(s, sender.GetStatus(s) == PXEntryStatus.Inserted ? PXEntryStatus.InsertedDeleted : PXEntryStatus.Deleted);
                                sender.ClearQueryCache();

								PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
								INItemPlan plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(sender.Graph, ((SOLineSplit)e.Row).PlanID);
								if (plan != null)
								{
									plan.PlanQty += s.BaseQty;
									if (cache.GetStatus(plan) != PXEntryStatus.Inserted)
									{
										cache.SetStatus(plan, PXEntryStatus.Updated);
									}
								}

								INItemPlan old_plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(sender.Graph, s.PlanID);
								if (old_plan != null)
								{
									cache.SetStatus(old_plan, cache.GetStatus(old_plan) == PXEntryStatus.Inserted ? PXEntryStatus.InsertedDeleted : PXEntryStatus.Deleted);
                                    cache.ClearQueryCache();

								}
								RefreshView(sender);								
							}
						}
					}
				}
			}
		}

		public override void Detail_UOM_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (!IsAllocationEntryEnabled)
			{
				base.Detail_UOM_FieldDefaulting(sender, e);
			}
		}

		public override void Detail_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!IsAllocationEntryEnabled)
			{
				base.Detail_Qty_FieldVerifying(sender, e);
			}
		}

		public override SOLineSplit Convert(SOLine item)
		{
			using (InvtMultScope<SOLine> ms = new InvtMultScope<SOLine>(item))
			{
				SOLineSplit ret = (SOLineSplit)item;
				//baseqty will be overriden in all cases but AvailabilityFetch
				ret.BaseQty = item.BaseQty - item.UnassignedQty;

				if (item.PlanID != null)
				{
					INItemPlan plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(this._Graph, item.PlanID);
					if (plan != null)
					{
						ret.PlanType = plan.PlanType;
					}
				}
				return ret;
			}
		}

		public void ThrowFieldIsEmpty<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Field).Name)))
			{
				throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, typeof(Field).Name);
			}
		}
		public virtual void SOLine_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOLine line = e.Row as SOLine;
			if (line != null)			
				PXUIFieldAttribute.SetEnabled<SOLine.shipDate>(sender, line, line.ShipComplete == SOShipComplete.BackOrderAllowed);
		}

		public virtual void SOLineSplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOLineSplit split = e.Row as SOLineSplit;

			if (split != null)
			{
				SOLine parent = (SOLine)PXParentAttribute.SelectParent(sender, split);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.cancelled>(sender, e.Row, false);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.shippedQty>(sender, e.Row, false);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.shipmentNbr>(sender, e.Row, false);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.isAllocated>(sender, e.Row, split.Cancelled == false);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.qty>(sender, e.Row, split.Cancelled == false);
				PXUIFieldAttribute.SetEnabled<SOLineSplit.shipDate>(sender, e.Row, split.Cancelled == false && parent.ShipComplete == SOShipComplete.BackOrderAllowed);
			}
		}

		public virtual void SOLineSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				bool RequireLocationAndSubItem = ((SOLineSplit)e.Row).RequireAllocation != true && ((SOLineSplit)e.Row).IsStockItem == true && ((SOLineSplit)e.Row).BaseQty != 0m;

				PXDefaultAttribute.SetPersistingCheck<SOLineSplit.subItemID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<SOLineSplit.locationID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
			}
		}

		public virtual void SOLineSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(SOLine)];
			if (cache.Current != null && (e.Row == null || ((SOLine)cache.Current).LineNbr == ((SOLineSplit)e.Row).LineNbr && ((SOLineSplit)e.Row).IsStockItem == true))
			{
				e.NewValue = ((SOLine)cache.Current).SubItemID;
				e.Cancel = true;
			}
		}

		public virtual void SOLineSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(SOLine)];
			if (cache.Current != null && (e.Row == null || ((SOLine)cache.Current).LineNbr == ((SOLineSplit)e.Row).LineNbr && ((SOLineSplit)e.Row).IsStockItem == true))
			{
				e.NewValue = ((SOLine)cache.Current).LocationID;
                e.Cancel = (_InternallCall == true || e.NewValue != null);
			}
		}

		public virtual void SOLineSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(SOLine)];
			if (cache.Current != null && (e.Row == null || ((SOLine)cache.Current).LineNbr == ((SOLineSplit)e.Row).LineNbr))
			{
				using (InvtMultScope<SOLine> ms = new InvtMultScope<SOLine>((SOLine)cache.Current))
				{
					e.NewValue = ((SOLine)cache.Current).InvtMult;
					e.Cancel = true;
				}
			}
		}

		protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXSetPropertyException e)
		{
			if (row is SOLine)
			{
				sender.RaiseExceptionHandling<SOLine.orderQty>(row, newValue, new PXSetPropertyException(e.Message, PXErrorLevel.Warning, sender.GetStateExt<SOLine.inventoryID>(row), sender.GetStateExt<SOLine.subItemID>(row), sender.GetStateExt<SOLine.siteID>(row), sender.GetStateExt<SOLine.locationID>(row), sender.GetValue<SOLine.lotSerialNbr>(row)));
			}
			else
			{
				sender.RaiseExceptionHandling<SOLineSplit.qty>(row, newValue, new PXSetPropertyException(e.Message, PXErrorLevel.Warning, sender.GetStateExt<SOLineSplit.inventoryID>(row), sender.GetStateExt<SOLineSplit.subItemID>(row), sender.GetStateExt<SOLineSplit.siteID>(row), sender.GetStateExt<SOLineSplit.locationID>(row), sender.GetValue<SOLineSplit.lotSerialNbr>(row)));
			}
		}

		protected void RaiseMemoQtyExceptionHanding<Target>(PXCache sender, SOLine Row, ILSMaster Split, Exception e)
			where Target : class, ILSMaster
		{
			if (typeof(Target) == typeof(SOLine))
			{
				sender.Graph.Caches[typeof(SOLine)].RaiseExceptionHandling<SOLine.orderQty>(Row, Row.OrderQty, new PXSetPropertyException(e.Message, sender.GetValueExt<SOLine.inventoryID>(Row), sender.GetValueExt<SOLine.subItemID>(Row), sender.GetValueExt<SOLine.invoiceNbr>(Row), sender.GetValueExt<SOLine.lotSerialNbr>(Row)));
			}
			else
			{
				PXCache cache = sender.Graph.Caches[typeof(SOLineSplit)];
				cache.RaiseExceptionHandling<SOLineSplit.qty>(Split, ((SOLineSplit)Split).Qty, new PXSetPropertyException(e.Message, sender.GetValueExt<SOLine.inventoryID>(Row), cache.GetValueExt<SOLineSplit.subItemID>(Split), sender.GetValueExt<SOLine.invoiceNbr>(Row), cache.GetValueExt<SOLineSplit.lotSerialNbr>(Split)));
			}
		}

		public class Comparer<T> : IEqualityComparer<T>
		{
			protected PXCache _cache;
			public Comparer(PXGraph graph)
			{
				_cache = graph.Caches[typeof(T)];
			}

			public bool Equals(T a, T b)
			{
				return _cache.ObjectsEqual(a, b);
			}

			public int GetHashCode(T a)
			{
				return _cache.GetObjectHashCode(a);
			}
		}

		protected virtual void MemoAvailabilityCheck(PXCache sender, SOLine Row)
		{
			if (Row.InvoiceNbr != null)
			{
				//credit memo contains only one reference to each original invoice line
				PXResultset<ARTran> orig_line = PXSelectJoin<ARTran,
					LeftJoin<SOMemoLine, On<SOMemoLine.origOrderType, Equal<ARTran.sOOrderType>, And<SOMemoLine.origOrderNbr, Equal<ARTran.sOOrderNbr>, And<SOMemoLine.origLineNbr, Equal<ARTran.sOOrderLineNbr>, And<SOMemoLine.invoiceNbr, Equal<ARTran.refNbr>, And<SOMemoLine.cancelled, Equal<boolFalse>>>>>>>,
					Where<ARTran.sOOrderType, Equal<Optional<SOLine.origOrderType>>, And<ARTran.sOOrderNbr, Equal<Optional<SOLine.origOrderNbr>>, And<ARTran.sOOrderLineNbr, Equal<Optional<SOLine.origLineNbr>>, And<ARTran.refNbr, Equal<Optional<SOLine.invoiceNbr>>>>>>>.SelectMultiBound(sender.Graph, new object[] { Row });

				if (orig_line == null || orig_line.Count == 0)
				{
					throw new PXException();
				}

				decimal? QtyInvoicedBase = -INUnitAttribute.ConvertToBase<SOLine.inventoryID>(sender, Row, Row.UOM, (decimal)Row.OrderQty, INPrecision.QUANTITY);

				Dictionary<ARTran, bool> invoice_lines_distinct = new Dictionary<ARTran, bool>(new Comparer<ARTran>(sender.Graph));
				Dictionary<SOMemoLine, bool> return_lines_distinct = new Dictionary<SOMemoLine, bool>(new Comparer<SOMemoLine>(sender.Graph));

				PXCache cache = sender.Graph.Caches[typeof(ARTran)];

				foreach (PXResult<ARTran, SOMemoLine> res in orig_line)
				{
					ARTran invoice_line = (ARTran)res;
					SOMemoLine return_line = (SOMemoLine)res;

					bool exists;
					if (!invoice_lines_distinct.TryGetValue(invoice_line, out exists))
					{
						invoice_lines_distinct[invoice_line] = true;
						QtyInvoicedBase += INUnitAttribute.ConvertToBase<ARTran.inventoryID>(cache, invoice_line, invoice_line.UOM, (decimal)invoice_line.Qty, INPrecision.QUANTITY);
					}

					if (return_line.OrderNbr != null && !return_lines_distinct.TryGetValue(return_line, out exists))
					{
						return_lines_distinct[return_line] = true;

						if (sender.Locate(return_line) == null)
						{
							QtyInvoicedBase -= INUnitAttribute.ConvertToBase<SOLine.inventoryID>(sender, return_line, return_line.UOM, (decimal)return_line.OrderQty, INPrecision.QUANTITY);
						}
					}
				}

				if (QtyInvoicedBase < 0m)
				{
					sender.RaiseExceptionHandling<SOLine.orderQty>(Row, Row.OrderQty, new PXSetPropertyException(Messages.InvoiceCheck_QtyNegative, sender.GetValueExt<SOLine.inventoryID>(Row), sender.GetValueExt<SOLine.subItemID>(Row), sender.GetValueExt<SOLine.invoiceNbr>(Row)));
				}
			} 
		}

		protected void MemoAvailabilityCheck<Target>(PXCache sender, SOLine Row, ILSMaster Split)
			where Target : class, ILSMaster
		{
			if (Row.InvoiceNbr != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Split.InventoryID);

				if (item != null && ((INLotSerClass)item).LotSerTrack != INLotSerTrack.NotNumbered && Split.SubItemID != null && string.IsNullOrEmpty(Split.LotSerialNbr) == false)
				{
					PXResult<INTran> orig_line = PXSelectJoinGroupBy<INTran,
					InnerJoin<INTranSplit, On<INTranSplit.tranType, Equal<INTran.tranType>, And<INTranSplit.refNbr, Equal<INTran.refNbr>, And<INTranSplit.lineNbr, Equal<INTran.lineNbr>>>>>,
					Where<INTran.sOOrderType, Equal<Optional<SOLine.origOrderType>>, And<INTran.sOOrderNbr, Equal<Optional<SOLine.origOrderNbr>>, And<INTran.sOOrderLineNbr, Equal<Optional<SOLine.origLineNbr>>, And<INTran.aRRefNbr, Equal<Optional<SOLine.invoiceNbr>>,
					And<INTranSplit.subItemID, Equal<Optional<SOLineSplit.subItemID>>, And<INTranSplit.lotSerialNbr, Equal<Optional<SOLineSplit.lotSerialNbr>>>>>>>>,
					Aggregate<GroupBy<INTranSplit.subItemID, GroupBy<INTranSplit.lotSerialNbr, Sum<INTranSplit.baseQty>>>>>.SelectSingleBound(sender.Graph, new object[] { Row, Split as SOLineSplit });

					PXResult<SOLine> memo_line = PXSelectJoinGroupBy<SOLine,
					InnerJoin<SOLineSplit, On<SOLineSplit.orderType, Equal<SOLine.orderType>, And<SOLineSplit.orderNbr, Equal<SOLine.orderNbr>, And<SOLineSplit.lineNbr, Equal<SOLine.lineNbr>>>>>,
					Where2<Where<SOLine.orderType, NotEqual<Optional<SOLine.orderType>>, Or<SOLine.orderNbr, NotEqual<Optional<SOLine.orderNbr>>>>, And<SOLine.origOrderType, Equal<Optional<SOLine.origOrderType>>, And<SOLine.origOrderNbr, Equal<Optional<SOLine.origOrderNbr>>, And<SOLine.origLineNbr, Equal<Optional<SOLine.origLineNbr>>, And<SOLine.invoiceNbr, Equal<Optional<SOLine.invoiceNbr>>, And<SOLine.cancelled, Equal<boolFalse>,
					And<SOLineSplit.subItemID, Equal<Optional<SOLineSplit.subItemID>>, And<SOLineSplit.lotSerialNbr, Equal<Optional<SOLineSplit.lotSerialNbr>>>>>>>>>>,
					Aggregate<GroupBy<SOLineSplit.subItemID, GroupBy<SOLineSplit.lotSerialNbr, Sum<SOLineSplit.baseQty>>>>>.SelectSingleBound(sender.Graph, new object[] { Row, Split as SOLineSplit });

					if (orig_line == null)
					{
						if (Split is SOLineSplit && string.IsNullOrEmpty(((SOLineSplit)Split).AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(((SOLineSplit)Split).AssignedNbr, ((SOLineSplit)Split).LotSerialNbr))
						{
							((SOLineSplit)Split).AssignedNbr = null;
							((SOLineSplit)Split).LotSerialNbr = null;
						}
						else
						{
							RaiseMemoQtyExceptionHanding<Target>(sender, Row, Split, new PXSetPropertyException(Messages.InvoiceCheck_LotSerialInvalid));
						}
						return;
					}

					decimal? QtyInvoicedLotBase = ((INTranSplit)(PXResult<INTran, INTranSplit>)orig_line).BaseQty;

					if (memo_line != null)
					{
						if (((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
						{
							RaiseMemoQtyExceptionHanding<Target>(sender, Row, Split, new PXSetPropertyException(Messages.InvoiceCheck_SerialAlreadyReturned));
						}
						else
						{
							QtyInvoicedLotBase -= ((SOLineSplit)(PXResult<SOLine, SOLineSplit>)memo_line).BaseQty;
						}
					}

					if (Split is SOLine)
					{
						QtyInvoicedLotBase -= Split.BaseQty;
					}
					else
					{
						foreach (SOLineSplit split in PXParentAttribute.SelectSiblings(sender.Graph.Caches[typeof(SOLineSplit)], Split))
						{
							if (object.Equals(split.SubItemID, Split.SubItemID) && object.Equals(split.LotSerialNbr, Split.LotSerialNbr))
							{
								QtyInvoicedLotBase -= split.BaseQty;
							}
						}
					}

					if (QtyInvoicedLotBase < 0m)
					{
						RaiseMemoQtyExceptionHanding<Target>(sender, Row, Split, new PXSetPropertyException(Messages.InvoiceCheck_QtyLotSerialNegative));
					}
				}
			}
		}

		public override void AvailabilityCheck(PXCache sender, ILSMaster Row)
		{
			base.AvailabilityCheck(sender, Row);

			if (Row is SOLine)
			{
				MemoAvailabilityCheck(sender, (SOLine)Row);

				SOLineSplit copy = Convert(Row as SOLine);

				if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
				{
					DefaultLotSerialNbr(sender.Graph.Caches[typeof(SOLineSplit)], copy);
				}

				MemoAvailabilityCheck<SOLine>(sender, (SOLine)Row, copy);

				if (copy.LotSerialNbr == null)
				{
					Row.LotSerialNbr = null;
				}
			}
			else
			{
				object parent = PXParentAttribute.SelectParent(sender, Row, typeof(SOLine));
				MemoAvailabilityCheck(sender.Graph.Caches[typeof(SOLine)], (SOLine)parent);
				MemoAvailabilityCheck<SOLineSplit>(sender.Graph.Caches[typeof(SOLine)], (SOLine)parent, Row);
			}
		}
		#endregion

		public override PXSelectBase<INLotSerialStatus> GetSerialStatusCmd(PXCache sender, SOLine Row, PXResult<InventoryItem, INLotSerClass> item)
		{
			PXSelectBase<INLotSerialStatus> cmd = new PXSelectJoin<INLotSerialStatus, InnerJoin<INLocation, On<INLocation.locationID, Equal<INLotSerialStatus.locationID>>>, Where<INLotSerialStatus.inventoryID, Equal<Current<INLotSerialStatus.inventoryID>>, And<INLotSerialStatus.siteID, Equal<Current<INLotSerialStatus.siteID>>, And<INLotSerialStatus.qtyOnHand, Greater<decimal0>>>>>(sender.Graph);

			if (Row.SubItemID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.subItemID, Equal<Current<INLotSerialStatus.subItemID>>>>();
			}
			if (Row.LocationID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.locationID, Equal<Current<INLotSerialStatus.locationID>>>>();
			}
			else
			{
				switch (Row.TranType)
				{
					case INTranType.Transfer:
						cmd.WhereAnd<Where<INLocation.transfersValid, Equal<boolTrue>>>();
						break;
					default:
						cmd.WhereAnd<Where<INLocation.salesValid, Equal<boolTrue>>>();
						break;
				}
			}
			if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.lotSerialNbr, Equal<Current<INLotSerialStatus.lotSerialNbr>>>>();
			}

			switch (((INLotSerClass)item).LotSerIssueMethod)
			{
				case INLotSerIssueMethod.FIFO:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.receiptDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
					break;
				case INLotSerIssueMethod.LIFO:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Desc<INLotSerialStatus.receiptDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
					break;
				case INLotSerIssueMethod.Expiration:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.expireDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
					break;
				case INLotSerIssueMethod.Sequential:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.lotSerialNbr>>>>();
					break;
				case INLotSerIssueMethod.UserEnterable:
					cmd.WhereAnd<Where<boolTrue, Equal<boolFalse>>>();
					break;
				default:
					throw new PXException();
			}

			return cmd;
		}

	}

	public class LSSOShipLine : LSSelect<SOShipLine, SOShipLineSplit,
		Where<SOShipLineSplit.shipmentNbr, Equal<Current<SOShipment.shipmentNbr>>>>
	{
		#region State
		protected string _OrigOrderQtyField = "OrigOrderQty";
		protected string _OpenOrderQtyField = "OpenOrderQty";
		#endregion
		#region Ctor
		public LSSOShipLine(PXGraph graph)
			: base(graph)
		{
			MasterQtyField = typeof(SOShipLine.shippedQty);
			graph.FieldDefaulting.AddHandler<SOShipLineSplit.subItemID>(SOShipLineSplit_SubItemID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<SOShipLineSplit.locationID>(SOShipLineSplit_LocationID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<SOShipLineSplit.invtMult>(SOShipLineSplit_InvtMult_FieldDefaulting);
			graph.RowPersisting.AddHandler<SOShipLine>(SOShipLine_RowPersisting);
			graph.RowPersisting.AddHandler<SOShipLineSplit>(SOShipLineSplit_RowPersisting);

			graph.Caches[typeof(SOShipLine)].Fields.Add(_OrigOrderQtyField);
			graph.Caches[typeof(SOShipLine)].Fields.Add(_OpenOrderQtyField);
			graph.FieldSelecting.AddHandler(typeof(SOShipLine), _OrigOrderQtyField, OrigOrderQty_FieldSelecting);
			graph.FieldSelecting.AddHandler(typeof(SOShipLine), _OpenOrderQtyField, OpenOrderQty_FieldSelecting);
			graph.RowUpdated.AddHandler<SOShipment>(SOShipment_RowUpdated);
		}
		#endregion

		#region Implementation
		protected virtual void SOShipment_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (!sender.ObjectsEqual<SOShipment.confirmed>(e.Row, e.OldRow) && (bool?)sender.GetValue<SOShipment.confirmed>(e.Row) == true)
			{
				PXCache cache = sender.Graph.Caches[typeof(SOShipLine)];

				foreach (SOShipLine item in PXParentAttribute.SelectSiblings(cache, null, typeof(SOShipment)))
				{
					if (Math.Abs((decimal)item.BaseQty) >= 0.0000005m && item.UnassignedQty >= 0.0000005m)
					{
						cache.RaiseExceptionHandling<SOShipLine.shippedQty>(item, item.Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned));

						if (cache.GetStatus(item) == PXEntryStatus.Notchanged)
						{
							cache.SetStatus(item, PXEntryStatus.Updated);
						}
					}
				}
			}
		}

		protected virtual void OrigOrderQty_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				SOLine2 orig_line = PXSelect<SOLine2, Where<SOLine2.orderType, Equal<Current<SOShipLine.origOrderType>>, And<SOLine2.orderNbr, Equal<Current<SOShipLine.origOrderNbr>>, And<SOLine2.lineNbr, Equal<Current<SOShipLine.origLineNbr>>>>>>.SelectSingleBound(_Graph, new object[] { (SOShipLine)e.Row });

				if (orig_line != null)
				{
					if (string.Equals(((SOShipLine)e.Row).UOM, orig_line.UOM) == false)
					{
						decimal BaseOrderQty = INUnitAttribute.ConvertToBase<SOShipLine.inventoryID>(sender, e.Row, orig_line.UOM, (decimal)orig_line.OrderQty, INPrecision.QUANTITY);
						e.ReturnValue = INUnitAttribute.ConvertFromBase<SOShipLine.inventoryID>(sender, e.Row, ((SOShipLine)e.Row).UOM, BaseOrderQty, INPrecision.QUANTITY);
					}
					else
					{
						e.ReturnValue = orig_line.OrderQty;
					}
				}
			}
			e.ReturnState = PXDecimalState.CreateInstance(e.ReturnState, INSetupDecPl.Qty, _OrigOrderQtyField, false, 0, decimal.MinValue, decimal.MaxValue);
			((PXFieldState)e.ReturnState).DisplayName = PXMessages.LocalizeNoPrefix(Messages.OrigOrderQty);
			((PXFieldState)e.ReturnState).Enabled = false;
		}
		protected virtual void OpenOrderQty_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row != null)
			{
				SOLine2 orig_line = PXSelect<SOLine2, Where<SOLine2.orderType, Equal<Current<SOShipLine.origOrderType>>, And<SOLine2.orderNbr, Equal<Current<SOShipLine.origOrderNbr>>, And<SOLine2.lineNbr, Equal<Current<SOShipLine.origLineNbr>>>>>>.SelectSingleBound(_Graph, new object[] { (SOShipLine)e.Row });

				if (orig_line != null)
				{
					if (string.Equals(((SOShipLine)e.Row).UOM, orig_line.UOM) == false)
					{
						decimal BaseOpenQty = INUnitAttribute.ConvertToBase<SOShipLine.inventoryID>(sender, e.Row, orig_line.UOM, (decimal)orig_line.OrderQty - (decimal)orig_line.ShippedQty, INPrecision.QUANTITY);
						e.ReturnValue = INUnitAttribute.ConvertFromBase<SOShipLine.inventoryID>(sender, e.Row, ((SOShipLine)e.Row).UOM, BaseOpenQty, INPrecision.QUANTITY);
					}
					else
					{
						e.ReturnValue = orig_line.OrderQty - orig_line.ShippedQty;
					}
				}
			}
			e.ReturnState = PXDecimalState.CreateInstance(e.ReturnState, INSetupDecPl.Qty, _OpenOrderQtyField, false, 0, decimal.MinValue, decimal.MaxValue);
			((PXFieldState)e.ReturnState).DisplayName = PXMessages.LocalizeNoPrefix(Messages.OpenOrderQty);
			((PXFieldState)e.ReturnState).Enabled = false;
		}

		protected virtual void OrderAvailabilityCheck(PXCache sender, SOShipLine Row)
		{
			if (Row.OrigOrderNbr != null)
			{
				SOLineSplit2 split = PXSelect<SOLineSplit2,
					Where<SOLineSplit2.orderType, Equal<Current<SOShipLine.origOrderType>>,
						And<SOLineSplit2.orderNbr, Equal<Current<SOShipLine.origOrderNbr>>,
						And<SOLineSplit2.lineNbr, Equal<Current<SOShipLine.origLineNbr>>,
						And<SOLineSplit2.splitLineNbr, Equal<Current<SOShipLine.origSplitLineNbr>>>>>>>
					.SelectSingleBound(_Graph, new object[] {Row});
				if (split != null)
				{
					if(split.Qty < split.ShippedQty)
						throw new PXSetPropertyException(Messages.OrderSplitCheck_QtyNegative, 
							sender.GetValueExt<SOShipLine.inventoryID>(Row), 
							sender.GetValueExt<SOShipLine.subItemID>(Row), 
							sender.GetValueExt<SOShipLine.origOrderType>(Row), 
							sender.GetValueExt<SOShipLine.origOrderNbr>(Row));
				}
				

				SOLine2 orig_line = PXSelect<SOLine2, Where<SOLine2.orderType, Equal<Current<SOShipLine.origOrderType>>, And<SOLine2.orderNbr, Equal<Current<SOShipLine.origOrderNbr>>, And<SOLine2.lineNbr, Equal<Current<SOShipLine.origLineNbr>>>>>>.SelectSingleBound(_Graph, new object[] { Row });

				if (orig_line != null)
				{
					if (PXDBPriceCostAttribute.Round(sender, (decimal)(orig_line.OrderQty * orig_line.CompleteQtyMax / 100m - orig_line.ShippedQty)) < 0m)
					{
						throw new PXSetPropertyException(Messages.OrderCheck_QtyNegative, sender.GetValueExt<SOShipLine.inventoryID>(Row), sender.GetValueExt<SOShipLine.subItemID>(Row), sender.GetValueExt<SOShipLine.origOrderType>(Row), sender.GetValueExt<SOShipLine.origOrderNbr>(Row));
					}
				}				
			}
		}

		public override void AvailabilityCheck(PXCache sender, ILSMaster Row)
		{
			base.AvailabilityCheck(sender, Row);

			if (Row is SOShipLine)
			{
				try
				{
					OrderAvailabilityCheck(sender, (SOShipLine)Row);
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<SOShipLine.shippedQty>(Row, ((SOShipLine)Row).ShippedQty, ex);
				}
			}
			else
			{
				object parent = PXParentAttribute.SelectParent(sender, Row, typeof(SOShipLine));
				try
				{
					OrderAvailabilityCheck(sender.Graph.Caches[typeof(SOShipLine)], (SOShipLine)parent);
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<SOShipLineSplit.qty>(Row, ((SOShipLineSplit)Row).Qty, ex);
				}
			}
		}

		protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				PXCache cache = sender.Graph.Caches[typeof(SOShipment)];
				object doc = PXParentAttribute.SelectParent(sender, e.Row, typeof(SOShipment)) ?? cache.Current;

				bool? Confirmed = (bool?)cache.GetValue<SOShipment.confirmed>(doc);

				if (Confirmed == true && Math.Abs((decimal)((SOShipLine)e.Row).BaseQty) >= 0.0000005m && ((SOShipLine)e.Row).UnassignedQty >= 0.0000005m)
				{
					if (sender.RaiseExceptionHandling<SOShipLine.shippedQty>(e.Row, ((SOShipLine)e.Row).Qty, new PXSetPropertyException(Messages.BinLotSerialNotAssigned)))
					{
						throw new PXRowPersistingException(typeof(SOShipLine.shippedQty).Name, ((SOShipLine)e.Row).Qty, Messages.BinLotSerialNotAssigned);
					}
				}
			}

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				try
				{
					OrderAvailabilityCheck(sender, (SOShipLine)e.Row);
				}
				catch (PXSetPropertyException ex)
				{
					sender.RaiseExceptionHandling<SOShipLine.shippedQty>(e.Row, ((SOShipLine)e.Row).ShippedQty, ex);
				}
			}
			base.Master_RowPersisting(sender, e);
		}

		public int? lastComponentID = null;
		protected override void _Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs<SOShipLine> e)
		{
			if (lastComponentID == e.Row.InventoryID)
			{
				return;
			}
			base._Master_RowUpdated(sender, e);
		}

		protected virtual void UpdateKit(PXCache sender, SOShipLineSplit row)
		{
			SOShipLine newline = SelectMaster(sender, row);

			if (newline == null)
			{
				return;
			}

			decimal KitQty = (decimal)newline.BaseQty;
			if (newline.InventoryID != row.InventoryID && row.IsStockItem == true)
			{
				foreach (PXResult<INKitSpecStkDet, InventoryItem> res in PXSelectJoin<INKitSpecStkDet, InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INKitSpecStkDet.compInventoryID>>>, Where<INKitSpecStkDet.kitInventoryID, Equal<Required<INKitSpecStkDet.kitInventoryID>>>>.Search<INKitSpecStkDet.compInventoryID>(sender.Graph, row.InventoryID, newline.InventoryID))
				{
					INKitSpecStkDet kititem = res;
					decimal ComponentQty = INUnitAttribute.ConvertToBase<SOShipLineSplit.inventoryID>(sender, row, kititem.UOM, KitQty * (decimal)kititem.DfltCompQty, INPrecision.QUANTITY);

					Counters counters;
					if (!DetailCounters.TryGetValue(newline, out counters))
					{
						DetailCounters[newline] = counters = new Counters();
						foreach (SOShipLineSplit detail in SelectDetail(sender, row))
						{
							UpdateCounters(sender, counters, detail);
						}
					}

					if (ComponentQty != 0m && (decimal)counters.BaseQty != ComponentQty)
					{
						KitQty = PXDBQuantityAttribute.Round(sender, KitQty * (decimal)counters.BaseQty / ComponentQty);
						lastComponentID = kititem.CompInventoryID;
					}
				}
			}
			else if (newline.InventoryID != row.InventoryID)
			{
				foreach (PXResult<INKitSpecNonStkDet, InventoryItem> res in PXSelectJoin<INKitSpecNonStkDet, InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<INKitSpecNonStkDet.compInventoryID>>>, Where<INKitSpecNonStkDet.kitInventoryID, Equal<Required<INKitSpecNonStkDet.kitInventoryID>>>>.Search<INKitSpecNonStkDet.compInventoryID>(sender.Graph, row.InventoryID, newline.InventoryID))
				{
					INKitSpecNonStkDet kititem = res;

					decimal ComponentQty = INUnitAttribute.ConvertToBase<SOShipLineSplit.inventoryID>(sender, row, kititem.UOM, (decimal)kititem.DfltCompQty, INPrecision.QUANTITY);

					if (ComponentQty != 0m && row.BaseQty != ComponentQty)
					{
						KitQty = PXDBQuantityAttribute.Round(sender, KitQty * (decimal)row.BaseQty / ComponentQty);
						lastComponentID = kititem.CompInventoryID;
					}
				}
			}

			if (lastComponentID != null)
			{
				SOShipLine copy = PXCache<SOShipLine>.CreateCopy(newline);
				copy.ShippedQty = INUnitAttribute.ConvertFromBase<SOShipLine.inventoryID>(MasterCache, newline, newline.UOM, KitQty, INPrecision.QUANTITY);
				try
				{
					MasterCache.Update(copy);
				}
				finally
				{
					lastComponentID = null;
				}

				if (sender.Graph is SOShipmentEntry)
				{
					((SOShipmentEntry)sender.Graph).splits.View.RequestRefresh();
				}
			}
		}

		protected override void Detail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.Detail_RowUpdated(sender, e);

			SOShipLineSplit row = (SOShipLineSplit)e.Row;

			if (!_InternallCall && !sender.Graph.UnattendedMode)
			{
				UpdateKit(sender, row);
			}
		}

		protected override void Detail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			base.Detail_RowInserted(sender, e);

			SOShipLineSplit row = (SOShipLineSplit)e.Row;

			if (!_InternallCall && !sender.Graph.UnattendedMode)
			{
				UpdateKit(sender, row);
			}
		}

		protected override void Detail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			base.Detail_RowDeleted(sender, e);

			SOShipLineSplit row = (SOShipLineSplit)e.Row;

			if (!_InternallCall && !sender.Graph.UnattendedMode)
			{
				UpdateKit(sender, row);
			}
		}


		public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			IQtyAllocated availability = AvailabilityFetch(sender, (SOShipLine)e.Row, true);

			if (availability != null)
			{
				PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((SOShipLine)e.Row).InventoryID);

				availability.QtyOnHand = INUnitAttribute.ConvertFromBase<SOShipLine.inventoryID, SOShipLine.uOM>(sender, e.Row, (decimal)availability.QtyOnHand, INPrecision.QUANTITY);
				availability.QtyAvail = INUnitAttribute.ConvertFromBase<SOShipLine.inventoryID, SOShipLine.uOM>(sender, e.Row,  (decimal)availability.QtyAvail, INPrecision.QUANTITY);
				availability.QtyNotAvail = INUnitAttribute.ConvertFromBase<SOShipLine.inventoryID, SOShipLine.uOM>(sender, e.Row, (decimal)availability.QtyNotAvail, INPrecision.QUANTITY);
				availability.QtyHardAvail = INUnitAttribute.ConvertFromBase<SOShipLine.inventoryID, SOShipLine.uOM>(sender, e.Row, (decimal)availability.QtyHardAvail, INPrecision.QUANTITY);
				
				e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(IN.Messages.Availability_Info,
						sender.GetValue<SOShipLine.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail));

				AvailabilityCheck(sender, (SOShipLine)e.Row, availability);
			}
			else
			{
				e.ReturnValue = string.Empty;
			}

			base.Availability_FieldSelecting(sender, e);
		}
		protected override IQtyAllocated AvailabilityFetch<TNode>(ILSDetail Row, IQtyAllocated allocated, IStatus status, bool ExcludeCurrent)			
		{
			if (status != null)
			{
				allocated.QtyOnHand += status.QtyOnHand;
				allocated.QtyHardAvail += status.QtyHardAvail;
			}
			allocated.QtyAvail = allocated.QtyHardAvail;

			if (ExcludeCurrent)
			{
				decimal SignQtyAvail = INItemPlanIDAttribute.GetInclQtyHardAvail<TNode>
					(_Graph.Caches[typeof (SOShipLineSplit)],
					 Row);
				if (SignQtyAvail != 0)
				{
					allocated.QtyAvail     -= SignQtyAvail * (Row.BaseQty ?? 0m);
					allocated.QtyNotAvail  += SignQtyAvail * (Row.BaseQty ?? 0m);
					allocated.QtyHardAvail -= SignQtyAvail * (Row.BaseQty ?? 0m);
				}
				

			}
			return allocated;
		}


		public override SOShipLineSplit Convert(SOShipLine item)
		{
			using (InvtMultScope<SOShipLine> ms = new InvtMultScope<SOShipLine>(item))
			{
				SOShipLineSplit ret = item;
				//baseqty will be overriden in all cases but AvailabilityFetch
				ret.BaseQty = item.BaseQty - item.UnassignedQty;
				return ret;
			}
		}

		public void ThrowFieldIsEmpty<Field>(PXCache sender, object data)
			where Field : IBqlField
		{
			if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Field).Name)))
			{
				throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, typeof(Field).Name);
			}
		}
		public virtual void SOShipLine_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{			
			if (e.Row != null && AdvancedAvailCheck(sender, e.Row) &&
				((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				if (((SOShipLine)e.Row).BaseQty != 0m)
				{
					AvailabilityCheck(sender, (SOShipLine)e.Row);
				}
			}
		}

		public virtual void SOShipLineSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
			{
				bool RequireLocationAndSubItem = ((SOShipLineSplit)e.Row).IsStockItem == true && ((SOShipLineSplit)e.Row).BaseQty != 0m;

				PXDefaultAttribute.SetPersistingCheck<SOShipLineSplit.subItemID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
				PXDefaultAttribute.SetPersistingCheck<SOShipLineSplit.locationID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

				if (AdvancedAvailCheck(sender, e.Row) && ((SOShipLineSplit)e.Row).BaseQty != 0m)
				{
					AvailabilityCheck(sender, (SOShipLineSplit)e.Row);
				}
			}
		}

		public virtual void SOShipLineSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(SOShipLine)];
			if (cache.Current != null && (e.Row == null || ((SOShipLine)cache.Current).LineNbr == ((SOShipLineSplit)e.Row).LineNbr && ((SOShipLineSplit)e.Row).IsStockItem == true))
			{
				e.NewValue = ((SOShipLine)cache.Current).SubItemID;
				e.Cancel = true;
			}
		}

		public virtual void SOShipLineSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(SOShipLine)];
			if (cache.Current != null && (e.Row == null || ((SOShipLine)cache.Current).LineNbr == ((SOShipLineSplit)e.Row).LineNbr && ((SOShipLineSplit)e.Row).IsStockItem == true))
			{
				e.NewValue = ((SOShipLine)cache.Current).LocationID;
                e.Cancel = (_InternallCall == true || e.NewValue != null);
			}
		}

		public virtual void SOShipLineSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			PXCache cache = sender.Graph.Caches[typeof(SOShipLine)];
			if (cache.Current != null && (e.Row == null || ((SOShipLine)cache.Current).LineNbr == ((SOShipLineSplit)e.Row).LineNbr))
			{
				using (InvtMultScope<SOShipLine> ms = new InvtMultScope<SOShipLine>((SOShipLine)cache.Current))
				{
					e.NewValue = ((SOShipLine)cache.Current).InvtMult;
					e.Cancel = true;
				}
			}
		}

		protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXSetPropertyException e)
		{
			PXErrorLevel level = AdvancedAvailCheck(sender, row) ? PXErrorLevel.Error : PXErrorLevel.Warning;
			if (row is SOShipLine)
			{
				sender.RaiseExceptionHandling<SOShipLine.shippedQty>(row, newValue, new PXSetPropertyException(e.Message, level, sender.GetStateExt<SOShipLine.inventoryID>(row), sender.GetStateExt<SOShipLine.subItemID>(row), sender.GetStateExt<SOShipLine.siteID>(row), sender.GetStateExt<SOShipLine.locationID>(row), sender.GetValue<SOShipLine.lotSerialNbr>(row)));
			}
			else
			{
				sender.RaiseExceptionHandling<SOShipLineSplit.qty>(row, newValue, new PXSetPropertyException(e.Message, level, sender.GetStateExt<SOShipLineSplit.inventoryID>(row), sender.GetStateExt<SOShipLineSplit.subItemID>(row), sender.GetStateExt<SOShipLineSplit.siteID>(row), sender.GetStateExt<SOShipLineSplit.locationID>(row), sender.GetValue<SOShipLineSplit.lotSerialNbr>(row)));
			}
		}

		protected bool AdvancedAvailCheck(PXCache sender, object row)
		{			
			SOSetup setup = (SOSetup)sender.Graph.Caches[typeof(SOSetup)].Current;
			if (setup != null && setup.AdvancedAvailCheck == true)
			{				
				if (_advancedAvailCheck != null) return _advancedAvailCheck == true;				
			}
			return false;
		}

		public void OverrideAdvancedAvailCheck(bool checkRequired)
		{
			_advancedAvailCheck = checkRequired;
		}
		private bool? _advancedAvailCheck;
		#endregion

		public override PXSelectBase<INLotSerialStatus> GetSerialStatusCmd(PXCache sender, SOShipLine Row, PXResult<InventoryItem, INLotSerClass> item)
		{
			PXSelectBase<INLotSerialStatus> cmd = new PXSelectJoin<INLotSerialStatus, InnerJoin<INLocation, On<INLocation.locationID, Equal<INLotSerialStatus.locationID>>>, Where<INLotSerialStatus.inventoryID, Equal<Current<INLotSerialStatus.inventoryID>>, And<INLotSerialStatus.siteID, Equal<Current<INLotSerialStatus.siteID>>, And<INLotSerialStatus.qtyOnHand, Greater<decimal0>>>>>(sender.Graph);

			if (Row.SubItemID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.subItemID, Equal<Current<INLotSerialStatus.subItemID>>>>();
			}
			if (Row.LocationID != null)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.locationID, Equal<Current<INLotSerialStatus.locationID>>>>();
			}
			else
			{
				switch (Row.TranType)
				{
					case INTranType.Transfer:
						cmd.WhereAnd<Where<INLocation.transfersValid, Equal<boolTrue>>>();
						break;
					default:
						cmd.WhereAnd<Where<INLocation.salesValid, Equal<boolTrue>>>();
						break;
				}
			}
			if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
			{
				cmd.WhereAnd<Where<INLotSerialStatus.lotSerialNbr, Equal<Current<INLotSerialStatus.lotSerialNbr>>>>();
			}

			switch (((INLotSerClass)item).LotSerIssueMethod)
			{
				case INLotSerIssueMethod.FIFO:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.receiptDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
					break;
				case INLotSerIssueMethod.LIFO:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Desc<INLotSerialStatus.receiptDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
					break;
				case INLotSerIssueMethod.Expiration:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.expireDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
					break;
				case INLotSerIssueMethod.Sequential:
					cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.lotSerialNbr>>>>();
					break;
				case INLotSerIssueMethod.UserEnterable:
					cmd.WhereAnd<Where<boolTrue, Equal<boolFalse>>>();
					break;
				default:
					throw new PXException();
			}

			return cmd;
		}


		public override void AvailabilityCheck(PXCache sender, ILSMaster Row, IQtyAllocated availability)
		{
			base.AvailabilityCheck(sender, Row, availability);
			if (Row.InvtMult == (short)-1 && Row.BaseQty > 0m && availability != null)
			{
				SOShipment doc = (SOShipment)sender.Graph.Caches[typeof(SOShipment)].Current;
				if (availability.QtyOnHand - Row.Qty < 0m && doc != null && doc.Confirmed == false)
				{
					if (availability is LotSerialStatus)
					{
						RaiseQtyRowExceptionHandling(sender, Row, Row.Qty, new PXSetPropertyException(IN.Messages.StatusCheck_QtyLotSerialOnHandNegative));
					}
					else if (availability is LocationStatus)
					{
						RaiseQtyRowExceptionHandling(sender, Row, Row.Qty, new PXSetPropertyException(IN.Messages.StatusCheck_QtyLocationOnHandNegative));
					}
					else if (availability is SiteStatus)
					{
						RaiseQtyRowExceptionHandling(sender, Row, Row.Qty, new PXSetPropertyException(IN.Messages.StatusCheck_QtyOnHandNegative));
					}
				}
			}
		}
		private void RaiseQtyRowExceptionHandling(PXCache sender, object row, object newValue, PXSetPropertyException e)
		{
			PXErrorLevel level = AdvancedAvailCheck(sender, row) ? PXErrorLevel.Error : PXErrorLevel.Warning;
			if (row is SOShipLine)
			{
				sender.RaiseExceptionHandling<SOShipLine.shippedQty>(row, newValue, 
					e == null ? e : new PXSetPropertyException(e.Message, level, sender.GetStateExt<SOShipLine.inventoryID>(row), sender.GetStateExt<SOShipLine.subItemID>(row), sender.GetStateExt<SOShipLine.siteID>(row), sender.GetStateExt<SOShipLine.locationID>(row), sender.GetValue<SOShipLine.lotSerialNbr>(row)));
			}
			else
			{
				sender.RaiseExceptionHandling<SOShipLineSplit.qty>(row, newValue,
					e == null ? e : new PXSetPropertyException(e.Message, level, sender.GetStateExt<SOShipLineSplit.inventoryID>(row), sender.GetStateExt<SOShipLineSplit.subItemID>(row), sender.GetStateExt<SOShipLineSplit.siteID>(row), sender.GetStateExt<INTranSplit.locationID>(row), sender.GetValue<SOShipLineSplit.lotSerialNbr>(row)));
			}
		}
		
	}

	public abstract class SOContactAttribute : ContactAttribute
	{
		#region State
		BqlCommand _DuplicateSelect = BqlCommand.CreateInstance(typeof(Select<SOContact, Where<SOContact.customerID, Equal<Required<SOContact.customerID>>, And<SOContact.customerContactID, Equal<Required<SOContact.customerContactID>>, And<SOContact.revisionID, Equal<Required<SOContact.revisionID>>, And<SOContact.isDefaultContact, Equal<boolTrue>>>>>>));
		#endregion
		#region Ctor
		public SOContactAttribute(Type AddressIDType, Type IsDefaultAddressType, Type SelectType)
			: base(AddressIDType, IsDefaultAddressType, SelectType)
		{
		}
		#endregion
		#region Implementation
		public override void Record_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && ((SOContact)e.Row).IsDefaultContact == true)
			{
				PXView view = sender.Graph.TypedViews.GetView(_DuplicateSelect, true);
				view.Clear();

				SOContact prev_address = (SOContact)view.SelectSingle(((SOContact)e.Row).CustomerID, ((SOContact)e.Row).CustomerContactID, ((SOContact)e.Row).RevisionID);
				if (prev_address != null)
				{
					_KeyToAbort = sender.GetValue(e.Row, _RecordID);
					object newkey = sender.Graph.Caches[typeof(SOContact)].GetValue(prev_address, _RecordID);

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
					foreach (object data in cache.Inserted)
					{
						object datakey = cache.GetValue(data, _RecordID);
						if (Equals(key, datakey))
						{
							if (((SOContact)data).IsDefaultContact == true)
							{
								PXView view = sender.Graph.TypedViews.GetView(_DuplicateSelect, true);
								view.Clear();

								SOContact prev_address = (SOContact)view.SelectSingle(((SOContact)data).CustomerID, ((SOContact)data).CustomerContactID, ((SOContact)data).RevisionID);

								if (prev_address != null)
								{
									_KeyToAbort = sender.GetValue(e.Row, _FieldOrdinal);
									object id = sender.Graph.Caches[typeof(SOContact)].GetValue(prev_address, _RecordID);
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

	public class SOBillingContactAttribute : SOContactAttribute
	{
		public SOBillingContactAttribute(Type SelectType)
			: base(typeof(SOBillingContact.contactID), typeof(SOBillingContact.isDefaultContact), SelectType)
		{ 
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<SOBillingContact.overrideContact>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<SOBillingContact, SOBillingContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<SOBillingContact, SOBillingContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<SOBillingContact>(sender, e);
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
				PXUIFieldAttribute.SetEnabled<SOBillingContact.overrideContact>(sender, e.Row, true);
			}
		}
	}

	/// <summary>
	/// Shipping contact for the Sales Order document.
	/// </summary>
	public class SOShippingContactAttribute : SOContactAttribute
	{
		public SOShippingContactAttribute(Type SelectType)
			: base(typeof(SOShippingContact.contactID), typeof(SOShippingContact.isDefaultContact), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<SOShippingContact.overrideContact>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<SOShippingContact, SOShippingContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<SOShippingContact, SOShippingContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<SOShippingContact>(sender, e);
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
				PXUIFieldAttribute.SetEnabled<SOShippingContact.overrideContact>(sender, e.Row, true);
			}
		}
	}

	public class SOShipmentContactAttribute : ContactAttribute
	{
		public SOShipmentContactAttribute(Type SelectType)
			: base(typeof(SOShipmentContact.contactID), typeof(SOShipmentContact.isDefaultContact), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<SOShipmentContact.overrideContact>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultContact<SOShipmentContact, SOShipmentContact.contactID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyContact<SOShipmentContact, SOShipmentContact.contactID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Contact_IsDefaultContact_FieldVerifying<SOShipmentContact>(sender, e);
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
				PXUIFieldAttribute.SetEnabled<SOShipmentContact.overrideContact>(sender, e.Row, true);
			}
		}
	}

	public abstract class SOAddressAttribute : AddressAttribute
	{
		#region State
		BqlCommand _DuplicateSelect = BqlCommand.CreateInstance(typeof(Select<SOAddress, Where<SOAddress.customerID, Equal<Required<SOAddress.customerID>>, And<SOAddress.customerAddressID, Equal<Required<SOAddress.customerAddressID>>, And<SOAddress.revisionID, Equal<Required<SOAddress.revisionID>>, And<SOAddress.isDefaultAddress, Equal<boolTrue>>>>>>));
		#endregion
		#region Ctor
		public SOAddressAttribute(Type AddressIDType, Type IsDefaultAddressType, Type SelectType)
			: base(AddressIDType, IsDefaultAddressType, SelectType)
		{
		}
		#endregion
		#region Implementation
		public override void Record_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && ((SOAddress)e.Row).IsDefaultAddress == true)
			{
				PXView view = sender.Graph.TypedViews.GetView(_DuplicateSelect, true);
				view.Clear();

				SOAddress prev_address = (SOAddress)view.SelectSingle(((SOAddress)e.Row).CustomerID, ((SOAddress)e.Row).CustomerAddressID, ((SOAddress)e.Row).RevisionID);
				if (prev_address != null)
				{
					_KeyToAbort = sender.GetValue(e.Row, _RecordID);
					object newkey = sender.Graph.Caches[typeof(SOAddress)].GetValue(prev_address, _RecordID);

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
					foreach (object data in cache.Inserted)
					{
						object datakey = cache.GetValue(data, _RecordID);
						if (Equals(key, datakey))
						{
							if (((SOAddress)data).IsDefaultAddress == true)
							{
								PXView view = sender.Graph.TypedViews.GetView(_DuplicateSelect, true);
								view.Clear();

								SOAddress prev_address = (SOAddress)view.SelectSingle(((SOAddress)data).CustomerID, ((SOAddress)data).CustomerAddressID, ((SOAddress)data).RevisionID);

								if (prev_address != null)
								{
									_KeyToAbort = sender.GetValue(e.Row, _FieldOrdinal);
									object id = sender.Graph.Caches[typeof(SOAddress)].GetValue(prev_address, _RecordID);
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

	public class SOBillingAddressAttribute : SOAddressAttribute
	{
		public SOBillingAddressAttribute(Type SelectType)
			: base(typeof(SOBillingAddress.addressID), typeof(SOBillingAddress.isDefaultAddress), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<SOBillingAddress.overrideAddress>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<SOBillingAddress, SOBillingAddress.addressID>(sender, DocumentRow, Row);
		}

		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<SOBillingAddress, SOBillingAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}

		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<SOBillingAddress>(sender, e);
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
				PXUIFieldAttribute.SetEnabled<SOBillingAddress.overrideAddress>(sender, e.Row, true);
				PXUIFieldAttribute.SetEnabled<SOBillingAddress.isValidated>(sender, e.Row, false);
			}
		}
	}

	public class SOShippingAddressAttribute : SOAddressAttribute
	{
		public SOShippingAddressAttribute(Type SelectType)
			: base(typeof(SOShippingAddress.addressID), typeof(SOShippingAddress.isDefaultAddress), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<SOShippingAddress.overrideAddress>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<SOShippingAddress, SOShippingAddress.addressID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<SOShippingAddress, SOShippingAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<SOShippingAddress>(sender, e);
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
				PXUIFieldAttribute.SetEnabled<SOShippingAddress.overrideAddress>(sender, e.Row, true);
			}
		}
	}

	public class SOShipmentAddressAttribute : AddressAttribute
	{
		public SOShipmentAddressAttribute(Type SelectType)
			: base(typeof(SOShipmentAddress.addressID), typeof(SOShipmentAddress.isDefaultAddress), SelectType)
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			sender.Graph.FieldVerifying.AddHandler<SOShipmentAddress.overrideAddress>(Record_Override_FieldVerifying);
		}

		public override void DefaultRecord(PXCache sender, object DocumentRow, object Row)
		{
			DefaultAddress<SOShipmentAddress, SOShipmentAddress.addressID>(sender, DocumentRow, Row);
		}
		public override void CopyRecord(PXCache sender, object DocumentRow, object SourceRow, bool clone)
		{
			CopyAddress<SOShipmentAddress, SOShipmentAddress.addressID>(sender, DocumentRow, SourceRow, clone);
		}
		public override void Record_IsDefault_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		public virtual void Record_Override_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			e.NewValue = (e.NewValue == null ? e.NewValue : (bool?)e.NewValue == false);
			try
			{
				Address_IsDefaultAddress_FieldVerifying<SOShipmentAddress>(sender, e);
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
				PXUIFieldAttribute.SetEnabled<SOShipmentAddress.overrideAddress>(sender, e.Row, true);
				PXUIFieldAttribute.SetEnabled<SOShipmentAddress.isValidated>(sender, e.Row, false);
			}
		}
	}

	public class SOUnbilledTax4Attribute : SOUnbilledTax2Attribute
	{
		public SOUnbilledTax4Attribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryTranAmt = typeof(SOLine4.curyUnbilledAmt);

			this._Attributes.Clear();
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<SOLine4.operation, Equal<Parent<SOOrder.defaultOperation>>>, SOLine4.curyUnbilledAmt>, Minus<SOLine4.curyUnbilledAmt>>), typeof(SumCalc<SOOrder.curyUnbilledLineTotal>)));
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<SOLine4.lineNbr>, short2>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}
	}

	public class SOUnbilledTax2Attribute : SOUnbilledTaxAttribute
	{
		public SOUnbilledTax2Attribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryTranAmt = typeof(SOLine2.curyUnbilledAmt);

			this._Attributes.Clear();
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<SOLine2.operation, Equal<Parent<SOOrder.defaultOperation>>>, SOLine2.curyUnbilledAmt>, Minus<SOLine2.curyUnbilledAmt>>), typeof(SumCalc<SOOrder.curyUnbilledLineTotal>)));
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<SOLine2.lineNbr>, short2>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_ChildType = sender.GetItemType();
			_TaxCalc = TaxCalc.Calc;
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _CuryTranAmt, CuryUnbilledAmt_FieldUpdated);
		}

		public virtual void CuryUnbilledAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			CalcTaxes(sender, e.Row, PXTaxCheck.Line);
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowInserted(sender, e);
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowUpdated(sender, e);
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowDeleted(sender, e);
		}
	}

	/// <summary>
	/// Extends <see cref="SOTaxAttribute"/> and calculates CuryUnbilledOrderTotal and OpenDoc for the Parent(Header) SOOrder.
	/// </summary>
	/// <example>
	/// [SOUnbilledTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), TaxCalc = TaxCalc.ManualLineCalc)]
	/// </example>
	public class SOUnbilledTaxAttribute : SOTaxAttribute
	{
		protected override short SortOrder
		{
			get
			{
				return 2;
			}
		}

		public SOUnbilledTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryDocBal = null;
			this.CuryLineTotal = typeof(SOOrder.curyUnbilledLineTotal);
			this.CuryTaxTotal = typeof(SOOrder.curyUnbilledTaxTotal);
			this.DocDate = typeof(SOOrder.orderDate);
			this.CuryTranAmt = typeof(SOLine.curyUnbilledAmt);

            this._Attributes.Clear();
            this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<SOLine.lineType, NotEqual<SOLineType.miscCharge>>, Switch<Case<Where<SOLine.operation, Equal<Current<SOOrder.defaultOperation>>>, SOLine.curyUnbilledAmt>, Minus<SOLine.curyUnbilledAmt>>>, decimal0>), typeof(SumCalc<SOOrder.curyUnbilledLineTotal>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<SOLine.lineType, Equal<SOLineType.miscCharge>>, Switch<Case<Where<SOLine.operation, Equal<Current<SOOrder.defaultOperation>>>, SOLine.curyUnbilledAmt>, Minus<SOLine.curyUnbilledAmt>>>, decimal0>), typeof(SumCalc<SOOrder.curyUnbilledMiscTot>)));
		}

		protected override void CalcDocTotals(PXCache sender, object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
		{
			_CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);

			decimal CuryLineTotal = (decimal)(ParentGetValue<SOOrder.curyLineTotal>(sender.Graph) ?? 0m);
			decimal CuryMiscTotal = (decimal)(ParentGetValue<SOOrder.curyMiscTot>(sender.Graph) ?? 0m);
			decimal CuryFreightTotal = (decimal)(ParentGetValue<SOOrder.curyFreightTot>(sender.Graph) ?? 0m);
			decimal CuryDiscTotal = (decimal)(ParentGetValue<SOOrder.curyDiscTot>(sender.Graph) ?? 0m);

			CuryLineTotal += CuryMiscTotal + CuryFreightTotal;

			decimal CuryUnbilledLineTotal = (decimal)(ParentGetValue<SOOrder.curyUnbilledLineTotal>(sender.Graph) ?? 0m);
			decimal CuryUnbilledMiscTotal = (decimal)(ParentGetValue<SOOrder.curyUnbilledMiscTot>(sender.Graph) ?? 0m);

			CuryUnbilledLineTotal += CuryUnbilledMiscTotal;

			decimal CuryUnbilledDiscTotal = (Math.Abs(CuryLineTotal - CuryUnbilledLineTotal) < 0.00005m) ? CuryDiscTotal : ( CuryLineTotal == 0 ? 0 : PXCurrencyAttribute.RoundCury(ParentCache(sender.Graph), ParentRow(sender.Graph), CuryUnbilledLineTotal * CuryDiscTotal / CuryLineTotal));
			decimal CuryUnbilledDocTotal = CuryUnbilledLineTotal + CuryTaxTotal - CuryInclTaxTotal - CuryUnbilledDiscTotal;

			if (object.Equals(CuryUnbilledDocTotal, (decimal)(ParentGetValue<SOOrder.curyUnbilledOrderTotal>(sender.Graph) ?? 0m)) == false)
			{
				ParentSetValue<SOOrder.curyUnbilledOrderTotal>(sender.Graph, CuryUnbilledDocTotal);
				ParentSetValue<SOOrder.openDoc>(sender.Graph, (CuryUnbilledDocTotal > 0m)); 
			}
		}

		protected override bool IsFreightTaxable(PXCache sender, List<object> taxitems)
		{
			if (taxitems.Count > 0)
			{
				List<object> items = base.SelectTaxes<Where<Tax.taxID, Equal<Required<Tax.taxID>>>>(sender.Graph, null, PXTaxCheck.RecalcLine, ((SOTax)(PXResult<SOTax>)taxitems[0]).TaxID);

				return base.IsFreightTaxable(sender, items);
			}
			else
			{
				return false;
			}
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<SOLine.lineNbr>, short2>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}
	}

	public class SOOpenTax4Attribute : SOOpenTaxAttribute
	{
		public SOOpenTax4Attribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryTranAmt = typeof(SOLine4.curyOpenAmt);

			this._Attributes.Clear();
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<SOLine4.operation, Equal<Parent<SOOrder.defaultOperation>>>, SOLine4.curyOpenAmt>, Minus<SOLine4.curyOpenAmt>>), typeof(SumCalc<SOOrder.curyOpenLineTotal>)));
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<SOLine4.lineNbr>, short1>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_ChildType = sender.GetItemType();
			_TaxCalc = TaxCalc.Calc;
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _CuryTranAmt, CuryUnbilledAmt_FieldUpdated);
		}

		public virtual void CuryUnbilledAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			CalcTaxes(sender, e.Row, PXTaxCheck.Line);
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowInserted(sender, e);
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowUpdated(sender, e);
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowDeleted(sender, e);
		}
	}


	/// <summary>
	/// Extends <see cref="SOTaxAttribute"/> and calculates CuryOpenOrderTotal for the Parent(Header) SOOrder.
	/// </summary>
	/// <example>
	/// [SOOpenTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), TaxCalc = TaxCalc.ManualLineCalc)]
	/// </example>
	public class SOOpenTaxAttribute : SOTaxAttribute
	{
		protected override short SortOrder
		{
			get
			{
				return 1;
			}
		}

		public SOOpenTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryDocBal = typeof(SOOrder.curyOpenOrderTotal);
			this.CuryLineTotal = typeof(SOOrder.curyOpenLineTotal);
			this.CuryTaxTotal = typeof(SOOrder.curyOpenTaxTotal);
			this.DocDate = typeof(SOOrder.orderDate);
			this.CuryTranAmt = typeof(SOLine.curyOpenAmt);

			this._Attributes.Clear();
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<SOLine.operation, Equal<Current<SOOrder.defaultOperation>>>, SOLine.curyOpenAmt>, Minus<SOLine.curyOpenAmt>>), typeof(SumCalc<SOOrder.curyOpenLineTotal>)));
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<SOLine.lineNbr>, short1>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		protected override void CalcDocTotals(PXCache sender, object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
		{
			_CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);

			decimal CuryLineTotal = (decimal)(ParentGetValue<SOOrder.curyLineTotal>(sender.Graph) ?? 0m);
			decimal CuryMiscTotal = (decimal)(ParentGetValue<SOOrder.curyMiscTot>(sender.Graph) ?? 0m);
			decimal CuryFreightTotal = (decimal)(ParentGetValue<SOOrder.curyFreightTot>(sender.Graph) ?? 0m);
			decimal CuryDiscTotal = (decimal)(ParentGetValue<SOOrder.curyDiscTot>(sender.Graph) ?? 0m);

			CuryLineTotal += CuryMiscTotal + CuryFreightTotal;

			decimal CuryOpenLineTotal = (decimal)(ParentGetValue<SOOrder.curyOpenLineTotal>(sender.Graph) ?? 0m);
			decimal CuryOpenDiscTotal = (Math.Abs(CuryLineTotal - CuryOpenLineTotal) < 0.00005m) ? CuryDiscTotal : ( CuryLineTotal == 0 ? 0 : PXCurrencyAttribute.RoundCury(ParentCache(sender.Graph), ParentRow(sender.Graph), CuryOpenLineTotal * CuryDiscTotal / CuryLineTotal));
			decimal CuryOpenDocTotal = CuryOpenLineTotal + CuryTaxTotal - CuryInclTaxTotal - CuryOpenDiscTotal;

			if (object.Equals(CuryOpenDocTotal, (decimal)(ParentGetValue<SOOrder.curyOpenOrderTotal>(sender.Graph) ?? 0m)) == false)
			{
				ParentSetValue<SOOrder.curyOpenOrderTotal>(sender.Graph, CuryOpenDocTotal);
			}
		}

		protected override bool IsFreightTaxable(PXCache sender, List<object> taxitems)
		{
			if (taxitems.Count > 0)
			{
				List<object> items = base.SelectTaxes<Where<Tax.taxID, Equal<Required<Tax.taxID>>>>(sender.Graph, null, PXTaxCheck.RecalcLine, ((SOTax)(PXResult<SOTax>)taxitems[0]).TaxID);

				return base.IsFreightTaxable(sender, items);
			}
			else
			{
				return false;
			}
		}
	}

	/// <summary>
	/// Extends <see cref="SOTaxAttribute"/> and calculates CuryOrderTotal and CuryTaxTotal for the Parent(Header) SOOrder.
	/// This Attribute overrides some of functionality of <see cref="SOTaxAttribute"/>. 
	/// This Attribute is applied to the TaxCategoryField of SOOrder instead of SO Line.
	/// </summary>
	/// <example>
	/// [SOOrderTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), TaxCalc = TaxCalc.ManualLineCalc)]
	/// </example>
	public class SOOrderTaxAttribute : SOTaxAttribute
	{
		public SOOrderTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			CuryTranAmt = typeof(SOOrder.curyFreightTot);
			TaxCategoryID = typeof(SOOrder.freightTaxCategoryID);

			this._Attributes.Clear();
		}

		protected override object InitializeTaxDet(object data)
		{
			object new_data =  base.InitializeTaxDet(data);
			if (new_data.GetType() == _TaxType)
			{
				((SOTax)new_data).LineNbr = 32000;
			}

			return new_data;
		}

        protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType)
		{
                return (decimal?)sender.GetValue(row, _CuryTranAmt);
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, int32000, short0>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		protected override void CalcDocTotals(PXCache sender, object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
		{
			decimal CuryLineTotal = (decimal)(ParentGetValue<SOOrder.curyLineTotal>(sender.Graph) ?? 0m);
			decimal CuryMiscTotal = (decimal)(ParentGetValue<SOOrder.curyMiscTot>(sender.Graph) ?? 0m);
			decimal CuryFreightTotal = (decimal)(ParentGetValue<SOOrder.curyFreightTot>(sender.Graph) ?? 0m);
			decimal CuryDiscountTotal = (decimal)(ParentGetValue<SOOrder.curyDiscTot>(sender.Graph) ?? 0m);

			decimal CuryDocTotal = CuryLineTotal + CuryMiscTotal + CuryFreightTotal + CuryTaxTotal - CuryInclTaxTotal - CuryDiscountTotal;

			if (object.Equals(CuryDocTotal, (decimal)(ParentGetValue<SOOrder.curyOrderTotal>(sender.Graph) ?? 0m)) == false ||
				object.Equals(CuryTaxTotal, (decimal)(ParentGetValue<SOOrder.curyTaxTotal>(sender.Graph) ?? 0m)) == false)
			{
				ParentSetValue<SOOrder.curyOrderTotal>(sender.Graph, CuryDocTotal);
				ParentSetValue<SOOrder.curyTaxTotal>(sender.Graph, CuryTaxTotal);
			}
		}

		protected override void Tax_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
		}

		protected override void Tax_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if ((_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc) && e.ExternalCall || _TaxCalc == TaxCalc.ManualCalc)
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				PXCache taxcache = sender.Graph.Caches[_TaxType];

				object det = ParentRow(sender.Graph);
				{
					ITaxDetail taxzonedet = MatchesCategory(cache, det, (ITaxDetail)e.Row);
					AddOneTax(taxcache, det, taxzonedet);
				}
				_NoSumTotals = (_TaxCalc == TaxCalc.ManualCalc && e.ExternalCall == false);

				PXRowDeleting del = delegate(PXCache _sender, PXRowDeletingEventArgs _e) { _e.Cancel |= object.ReferenceEquals(e.Row, _e.Row); };
				sender.Graph.RowDeleting.AddHandler(_TaxSumType, del);
				try
				{
					CalcTaxes(cache, null);
				}
				finally
				{
					sender.Graph.RowDeleting.RemoveHandler(_TaxSumType, del);
				}
				_NoSumTotals = false;
			}
		}

		protected override void Tax_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if ((_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc) && e.ExternalCall || _TaxCalc == TaxCalc.ManualCalc)
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				PXCache taxcache = sender.Graph.Caches[_TaxType];

				object det = ParentRow(sender.Graph);
				{
					DelOneTax(taxcache, det, e.Row);
				}
				CalcTaxes(cache, null);
			}
		}
		
		protected override void ZoneUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				if (!CompareZone(sender.Graph, (string)e.OldValue, (string)sender.GetValue(e.Row, _TaxZoneID)))
				{
					Preload(sender);

					ClearTaxes(sender, e.Row);
					DefaultTaxes(sender, e.Row, false);

					_ParentRow = e.Row;
					CalcTaxes(sender, e.Row);
					_ParentRow = null;
				}
			}
		}

		protected override void DateUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc)
			{
				Preload(sender);

				ClearTaxes(sender, e.Row);
				DefaultTaxes(sender, e.Row, false);

				_ParentRow = e.Row;
				CalcTaxes(sender, e.Row);
				_ParentRow = null;
			}
		}

        protected override void PeriodUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
        }

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			_ParentRow = e.Row;
			base.RowUpdated(sender, e);
			_ParentRow = null;
		}
	}

	public class SOUnbilledMiscTax2Attribute : SOUnbilledTaxAttribute
	{
		public SOUnbilledMiscTax2Attribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryDocBal = null;
			this.CuryLineTotal = typeof(SOOrder.curyUnbilledMiscTot);
			this.CuryTaxTotal = typeof(SOOrder.curyUnbilledTaxTotal);
			this.DocDate = typeof(SOOrder.orderDate);
			this.CuryTranAmt = typeof(SOMiscLine2.curyUnbilledAmt);

			this._Attributes.Clear();
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<SOMiscLine2.operation, Equal<Parent<SOOrder.defaultOperation>>>, SOMiscLine2.curyUnbilledAmt>, Minus<SOMiscLine2.curyUnbilledAmt>>), typeof(SumCalc<SOOrder.curyUnbilledMiscTot>)));
		}

		protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType)
		{
			return (decimal?)sender.GetValue(row, _CuryTranAmt);
		}

		protected override object InitializeTaxDet(object data)
		{
			((SOTax)data).BONbr = (short)2;

			return data;
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<SOMiscLine2.lineNbr>, short2>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);
			_ChildType = sender.GetItemType();
			_TaxCalc = TaxCalc.Calc;
			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _CuryTranAmt, CuryUnbilledAmt_FieldUpdated);
		}

		public virtual void CuryUnbilledAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			CalcTaxes(sender, e.Row, PXTaxCheck.Line);
		}

		public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowInserted(sender, e);
		}

		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowUpdated(sender, e);
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
			base.RowDeleted(sender, e);
		}

	}

	/// <summary>
	/// Specialized for SO version of the TaxAttribute. <br/>
	/// Provides Tax calculation for SOLine, by default is attached to SOLine (details) and SOOrder (header) <br/>
	/// Normally, should be placed on the TaxCategoryID field. <br/>
	/// Internally, it uses SOOrder graphs, otherwise taxes are not calculated (tax calc Level is set to NoCalc).<br/>
	/// As a result of this attribute work a set of SOTax tran related to each SOLine  and to their parent will created <br/>
	/// May be combined with other attrbibutes with similar type - for example, SOOpenTaxAttribute <br/>
	/// </summary>
	/// <example>
	/// [SOTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), TaxCalc = TaxCalc.ManualLineCalc)]
	/// </example>    
	public class SOTaxAttribute : TaxAttribute
	{
		protected virtual short SortOrder
		{
			get
			{
				return 0;
			}
		}
		
		public SOTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
			: base(ParentType, TaxType, TaxSumType)
		{
			this.CuryDocBal = null;
			this.CuryLineTotal = typeof(SOOrder.curyLineTotal);
			this.CuryTaxTotal = typeof(SOOrder.curyTaxTotal);
			this.DocDate = typeof(SOOrder.orderDate);
            this.CuryTranAmt = typeof(SOLine.curyLineAmt);
            this.GroupDiscountRate = typeof(SOLine.groupDiscountRate);

			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<SOLine.lineType, NotEqual<SOLineType.miscCharge>>, Switch<Case<Where<SOLine.operation, Equal<Current<SOOrder.defaultOperation>>>, SOLine.curyLineAmt>, Minus<SOLine.curyLineAmt>>>, decimal0>), typeof(SumCalc<SOOrder.curyLineTotal>)));
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<SOLine.lineType, Equal<SOLineType.miscCharge>>, Switch<Case<Where<SOLine.operation, Equal<Current<SOOrder.defaultOperation>>>, SOLine.curyLineAmt>, Minus<SOLine.curyLineAmt>>>, decimal0>), typeof(SumCalc<SOOrder.curyMiscTot>)));
            this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<SOLine.commissionable, Equal<True>>, Mult<SOLine.curyLineAmt, SOLine.groupDiscountRate>>, decimal0>), typeof(SumCalc<SOSalesPerTran.curyCommnblAmt>)));
		}

		public override int CompareTo(object other)
		{
 			 return this.SortOrder.CompareTo(((SOTaxAttribute)other).SortOrder);
		}

		protected override decimal CalcLineTotal(PXCache sender, object row)
		{
			return (decimal?)ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m;
		}
		
		protected override object InitializeTaxDet(object data)
		{
			((SOTax)data).BONbr = SortOrder;

			return data;
		}

        protected override decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType)
		{
            decimal val = 0m;
            if (TaxCalcType == "I")
                val = (base.GetCuryTranAmt(sender, row) ?? 0m) * ((decimal?)sender.GetValue(row, _GroupDiscountRate) ?? 1m);
            else
                val = base.GetCuryTranAmt(sender, row) ?? 0m;

			PXCache cache = sender.Graph.Caches[typeof(SOOrder)];
			
			if (row != null && !object.Equals(sender.GetValue<SOLine.operation>(row), cache.GetValue<SOOrder.defaultOperation>(cache.Current)))
			{
				return -val;
			}

			return val;
		}

		protected override void CalcDocTotals(PXCache sender, object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
		{
			base.CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);

			decimal CuryLineTotal = (decimal)(ParentGetValue<SOOrder.curyLineTotal>(sender.Graph) ?? 0m);
			decimal CuryMiscTotal = (decimal)(ParentGetValue<SOOrder.curyMiscTot>(sender.Graph) ?? 0m);
			decimal CuryFreightTotal = (decimal)(ParentGetValue<SOOrder.curyFreightTot>(sender.Graph) ?? 0m);
			decimal CuryDiscountTotal = (decimal)(ParentGetValue<SOOrder.curyDiscTot>(sender.Graph) ?? 0m);
			
			//if (this.GetType() == typeof(SOTaxAttribute) && CuryLineTotal < 0m)
			//{
			//	CuryLineTotal = -CuryLineTotal;
			//	CuryTaxTotal = -CuryTaxTotal;
			//	CuryDiscountTotal = -CuryDiscountTotal;
			//}

			decimal CuryDocTotal = CuryLineTotal + CuryMiscTotal + CuryFreightTotal + CuryTaxTotal - CuryInclTaxTotal - CuryDiscountTotal;

			if (object.Equals(CuryDocTotal, (decimal)(ParentGetValue<SOOrder.curyOrderTotal>(sender.Graph) ?? 0m)) == false)
			{
				ParentSetValue<SOOrder.curyOrderTotal>(sender.Graph, CuryDocTotal);
			}
		}

		protected virtual bool IsFreightTaxable(PXCache sender, List<object> taxitems)
		{
			for (int i = 0; i < taxitems.Count; i++)
			{
				if (((SOTax)(PXResult<SOTax>)taxitems[i]).LineNbr == 32000)
				{
					return true;
				}
			}
			return false;
		}

		protected override void AdjustTaxableAmount(PXCache sender, object row, List<object> taxitems, ref decimal CuryTaxableAmt)
		{
			decimal CuryLineTotal = (decimal?)ParentGetValue<SOOrder.curyLineTotal>(sender.Graph) ?? 0m;
			decimal CuryMiscTotal = (decimal)(ParentGetValue<SOOrder.curyMiscTot>(sender.Graph) ?? 0m);
			decimal CuryFreightTotal = (decimal)(ParentGetValue<SOOrder.curyFreightTot>(sender.Graph) ?? 0m);
			decimal CuryDiscountTotal = (decimal)(ParentGetValue<SOOrder.curyDiscTot>(sender.Graph) ?? 0m);

			CuryLineTotal += CuryMiscTotal;

            //24565 do not protate discount if lineamt+freightamt = taxableamt
            //27214 do not prorate discount on freight separated from document lines, i.e. taxable by different tax than document lines
            decimal CuryTaxableFreight = IsFreightTaxable(sender, taxitems) ? CuryFreightTotal : 0m;

			if (CuryLineTotal != 0m && CuryTaxableAmt != 0m)
			{
				if (Math.Abs(CuryTaxableAmt - CuryTaxableFreight - CuryLineTotal) < 0.00005m)
				{
					CuryTaxableAmt -= CuryDiscountTotal;
				}
				else
				{
					CuryTaxableAmt = PXCurrencyAttribute.RoundCury(ParentCache(sender.Graph), ParentRow(sender.Graph), CuryTaxableFreight + (CuryTaxableAmt - CuryTaxableFreight) * (1 - CuryDiscountTotal / CuryLineTotal));
				}
			}
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			return SelectTaxes<Where, Current<SOLine.lineNbr>, short0>(graph, new object[] { row, graph.Caches[_ParentType].Current }, taxchk, parameters);
		}

		protected List<object> SelectTaxes<Where, LineNbr, BONbr>(PXGraph graph, object[] currents, PXTaxCheck taxchk, params object[] parameters)
			where Where : IBqlWhere, new()
			where LineNbr : IBqlOperand
			where BONbr : IBqlOperand
		{
			Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
			foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
					And<TaxRev.outdated, Equal<boolFalse>,
					And<TaxRev.taxType, Equal<TaxType.sales>,
					And<Tax.taxType, NotEqual<CSTaxType.withholding>,
					And<Tax.taxType, NotEqual<CSTaxType.use>,
					And<Tax.reverseTax, Equal<boolFalse>,
					And<Current<SOOrder.orderDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
				Where>
				.SelectMultiBound(graph, currents, parameters))
			{
				tail[((Tax)record).TaxID] = record;
			}
			List<object> ret = new List<object>();
			Type fieldLineNbr;
			switch (taxchk)
			{
				case PXTaxCheck.Line:
					if (currents.Length > 0
						&& currents[0] != null
						&& typeof(LineNbr).IsGenericType
						&& typeof(LineNbr).GetGenericTypeDefinition() == typeof(Current<>)
						&& (fieldLineNbr = typeof(LineNbr).GetGenericArguments()[0]).IsNested
						&& currents[0].GetType() == BqlCommand.GetItemType(fieldLineNbr)
						)
					{
						int? linenbr = (int?)graph.Caches[BqlCommand.GetItemType(fieldLineNbr)].GetValue(currents[0], fieldLineNbr.Name);
						foreach (SOTax record in PXSelect<SOTax,
							Where<SOTax.orderType, Equal<Current<SOOrder.orderType>>,
								And<SOTax.orderNbr, Equal<Current<SOOrder.orderNbr>>,
								And<SOTax.bONbr, Equal<BONbr>>>>>
							.SelectMultiBound(graph, currents))
						{
							if (record.LineNbr == linenbr)
							{
								PXResult<Tax, TaxRev> line;
								if (tail.TryGetValue(record.TaxID, out line))
								{
									int idx;
									for (idx = ret.Count;
										(idx > 0)
										&& String.Compare(((Tax)(PXResult<SOTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
										idx--) ;
									ret.Insert(idx, new PXResult<SOTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
								}
							}
							//resultset is always sorted by LineNbr
							if (record.LineNbr > linenbr)
							{
								break;
							}
						}
					}
					else
					{
						foreach (SOTax record in PXSelect<SOTax,
							Where<SOTax.orderType, Equal<Current<SOOrder.orderType>>,
								And<SOTax.orderNbr, Equal<Current<SOOrder.orderNbr>>,
								And<SOTax.bONbr, Equal<BONbr>,
								And<SOTax.lineNbr, Equal<LineNbr>>>>>>
							.SelectMultiBound(graph, currents))
						{
							PXResult<Tax, TaxRev> line;
							if (tail.TryGetValue(record.TaxID, out line))
							{
								int idx;
								for (idx = ret.Count;
									(idx > 0)
									&& String.Compare(((Tax)(PXResult<SOTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
									idx--) ;
								ret.Insert(idx, new PXResult<SOTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
							}
						}
					}
					return ret;
				case PXTaxCheck.RecalcLine:
					foreach (SOTax record in PXSelect<SOTax,
						Where<SOTax.orderType, Equal<Current<SOOrder.orderType>>,
							And<SOTax.orderNbr, Equal<Current<SOOrder.orderNbr>>,
							And<SOTax.bONbr, Equal<BONbr>,
							And<SOTax.lineNbr, Less<intMax>>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& ((SOTax)(PXResult<SOTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
								&& String.Compare(((Tax)(PXResult<SOTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<SOTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				case PXTaxCheck.RecalcTotals:
					foreach (SOTaxTran record in PXSelect<SOTaxTran,
						Where<SOTaxTran.orderType, Equal<Current<SOOrder.orderType>>,
							And<SOTaxTran.orderNbr, Equal<Current<SOOrder.orderNbr>>,
							And<SOTaxTran.bONbr, Equal<BONbr>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (record.TaxID != null && tail.TryGetValue(record.TaxID, out line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0)
								&& String.Compare(((Tax)(PXResult<SOTaxTran, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
								idx--) ;
							ret.Insert(idx, new PXResult<SOTaxTran, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;
				default:
					return ret;
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			_ChildType = sender.GetItemType();

            inserted = new Dictionary<object, object>();
            updated = new Dictionary<object, object>();

			if (sender.Graph is SOOrderEntry)
			{
                base.CacheAttached(sender);

				sender.Graph.RowInserting.AddHandler(_TaxSumType, Tax_RowInserting);

				sender.Graph.FieldUpdated.AddHandler(typeof(SOOrder), typeof(SOOrder.curyDiscTot).Name, SOOrder_CuryDiscTot_FieldUpdated);
				sender.Graph.FieldUpdated.AddHandler(typeof(SOOrder), typeof(SOOrder.curyFreightTot).Name, SOOrder_CuryFreightTot_FieldUpdated);
				sender.Graph.FieldUpdated.AddHandler(typeof(SOOrder), _CuryTaxTotal, SOOrder_CuryTaxTot_FieldUpdated);
			}
			else
			{
				this.TaxCalc = TaxCalc.NoCalc;
			}
		}

		protected virtual void SOOrder_CuryDiscTot_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			bool calc = true;
			TaxZone taxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID));
			if (taxZone != null && taxZone.IsExternal == true)
				calc = false;
			
			this._ParentRow = e.Row;
			CalcTotals(sender, e.Row, calc);
			this._ParentRow = null;
		}

		protected virtual void SOOrder_CuryFreightTot_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			bool calc = true;
			TaxZone taxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID));
			if (taxZone != null && taxZone.IsExternal == true)
				calc = false;

			this._ParentRow = e.Row;
			CalcTotals(sender, e.Row, calc);
			this._ParentRow = null;
		}

		protected virtual void SOOrder_CuryTaxTot_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			decimal? curyTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
			decimal? curyWhTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryWhTaxTotal);
			CalcDocTotals(sender, e.Row, curyTaxTotal.GetValueOrDefault(), 0, curyWhTaxTotal.GetValueOrDefault());
		}

		protected virtual void Tax_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (e.ExternalCall)
			{
				((SOTax)e.Row).BONbr = (short)0;
			}
		}
    }

	public class SOInvoiceTaxAttribute : ARTaxAttribute
	{
		public SOInvoiceTaxAttribute()
			:base(typeof(PX.Objects.AR.ARInvoice), typeof(ARTax), typeof(ARTaxTran))
		{
			_Attributes.Clear();
			_Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<ARTran.lineType, NotEqual<SOLineType.miscCharge>, And<ARTran.lineType, NotEqual<SOLineType.freight>, And<ARTran.lineType, NotEqual<SOLineType.discount>>>>, ARTran.curyTranAmt>, decimal0>), typeof(SumCalc<SOInvoice.curyLineTotal>)));
			_Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<ARTran.lineType, Equal<SOLineType.miscCharge>>, ARTran.curyTranAmt>, decimal0>), typeof(SumCalc<SOInvoice.curyMiscTot>)));
			_Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<ARTran.lineType, Equal<SOLineType.freight>>, ARTran.curyTranAmt>, decimal0>), typeof(SumCalc<SOInvoice.curyFreightTot>)));
		}

		protected override decimal CalcLineTotal(PXCache sender, object row)
		{
			return (decimal?)ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m;
		}

		protected override void AdjustTaxableAmount(PXCache sender, object row, List<object> taxitems, ref decimal CuryTaxableAmt)
		{
			PXCache cache = sender.Graph.Caches[typeof(SOInvoice)];
			SOInvoice current = (SOInvoice)PXParentAttribute.SelectParent(sender, row, typeof(SOInvoice)); 

			decimal CuryLineTotal = (decimal?)cache.GetValue<SOInvoice.curyLineTotal>(current) ?? 0m;
			decimal CuryMiscTotal = (decimal?)cache.GetValue<SOInvoice.curyMiscTot>(current) ?? 0m;
			decimal CuryFreightTotal = (decimal?)cache.GetValue<SOInvoice.curyFreightTot>(current) ?? 0m;
			decimal CuryDiscountTotal = (decimal?)cache.GetValue<SOInvoice.curyDiscTot>(current) ?? 0m;

			if (CuryLineTotal + CuryMiscTotal + CuryFreightTotal != 0m && CuryTaxableAmt != 0m)
			{
				if (Math.Abs(CuryTaxableAmt - CuryLineTotal - CuryMiscTotal - CuryFreightTotal) < 0.00005m)
				{
					CuryTaxableAmt -= CuryDiscountTotal;
				}
				else
				{
					CuryTaxableAmt = CM.PXCurrencyAttribute.Round(ParentCache(sender.Graph), ParentRow(sender.Graph), CuryTaxableAmt * (1 - CuryDiscountTotal / (CuryLineTotal + CuryMiscTotal + CuryFreightTotal)), PX.Objects.CM.CMPrecision.TRANCURY);
				}
			}
		}

		protected override void CalcDocTotals(PXCache sender, object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
		{
			PXCache cache = sender.Graph.Caches[typeof(SOInvoice)];

			SOInvoice doc = null;
			if ( row is SOInvoice )
			{
				doc = (SOInvoice) row;
			}
			else
			{
				doc = (SOInvoice)PXParentAttribute.SelectParent(sender, row, typeof(SOInvoice));	
			}
			

			decimal CuryLineTotal = ((decimal?)cache.GetValue<SOInvoice.curyLineTotal>(doc) ?? 0m)
									+((decimal?)cache.GetValue<SOInvoice.curyMiscTot>(doc) ?? 0m)
									+ ((decimal?)cache.GetValue<SOInvoice.curyFreightTot>(doc) ?? 0m)
									- ((decimal?)cache.GetValue<SOInvoice.curyDiscTot>(doc) ?? 0m);

			decimal CuryDocTotal = CuryLineTotal + CuryTaxTotal - CuryInclTaxTotal;

			decimal doc_CuryLineTotal = (decimal)(ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m);
			decimal doc_CuryTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryTaxTotal) ?? 0m);

			if (object.Equals(CuryLineTotal, doc_CuryLineTotal) == false ||
				object.Equals(CuryTaxTotal, doc_CuryTaxTotal) == false)
			{
				ParentSetValue(sender.Graph, _CuryLineTotal, CuryLineTotal);
				ParentSetValue(sender.Graph, _CuryTaxTotal, CuryTaxTotal);
				if (!string.IsNullOrEmpty(_CuryDocBal))
				{
					ParentSetValue(sender.Graph, _CuryDocBal, CuryDocTotal);
					return;
				}
			}

			if (!string.IsNullOrEmpty(_CuryDocBal))
			{
				decimal doc_CuryDocBal = (decimal)(ParentGetValue(sender.Graph, _CuryDocBal) ?? 0m);

				if (object.Equals(CuryDocTotal, doc_CuryDocBal) == false)
				{
					ParentSetValue(sender.Graph, _CuryDocBal, CuryDocTotal);
				}
			}
		}

		protected override void IsTaxSavedFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARInvoice invoice = e.Row as ARInvoice;

			if (invoice != null)
			{
				SOInvoice soInvoice = PXSelect<SOInvoice, Where<SOInvoice.docType, Equal<Required<SOInvoice.docType>>, And<SOInvoice.refNbr, Equal<Required<SOInvoice.refNbr>>>>>.Select(sender.Graph, invoice.DocType, invoice.RefNbr);
				decimal? curyTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
				decimal? curyWhTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryWhTaxTotal);
				CalcDocTotals(sender, soInvoice, curyTaxTotal.GetValueOrDefault(), 0, curyWhTaxTotal.GetValueOrDefault());
			}
		}
	}

	public class SOSalesAcctSubDefault
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

		public class AcctListAttribute : CustomListAttribute
		{
			public AcctListAttribute()
				: base(new string[] { MaskItem, MaskSite, MaskClass, MaskLocation, MaskReasonCode }, new string[] { IN.Messages.MaskItem, IN.Messages.MaskSite, IN.Messages.MaskClass, AR.Messages.MaskLocation, IN.Messages.MaskReasonCode })
			{
			}
		}

		public class SubListAttribute : CustomListAttribute
		{
			public SubListAttribute()
				: base(new string[] { MaskItem, MaskSite, MaskClass, MaskLocation, MaskEmployee, MaskCompany, MaskSalesPerson, MaskReasonCode }, new string[] { IN.Messages.MaskItem, IN.Messages.MaskSite, IN.Messages.MaskClass, AR.Messages.MaskLocation, AR.Messages.MaskEmployee, AR.Messages.MaskCompany, AR.Messages.MaskSalesPerson, IN.Messages.MaskReasonCode })
			{
			}
		}

		public const string MaskItem = "I";
		public const string MaskSite = "W";
		public const string MaskClass = "P";
		public const string MaskReasonCode = "R";

		public const string MaskLocation = "L";
		public const string MaskEmployee = "E";
		public const string MaskCompany = "C";
		public const string MaskSalesPerson = "S";
	}

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class SOSalesSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ZZZZZZZZZA";
		public SOSalesSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, SOSalesAcctSubDefault.MaskItem, new SOSalesAcctSubDefault.SubListAttribute().AllowedValues, new SOSalesAcctSubDefault.SubListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new SOSalesAcctSubDefault.SubListAttribute().AllowedValues, 3, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new SOSalesAcctSubDefault.SubListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}


	public class SOMiscAcctSubDefault
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

		public class AcctListAttribute : CustomListAttribute
		{
			public AcctListAttribute()
				: base(new string[] { MaskLocation, MaskItem }, new string[] { AR.Messages.MaskLocation, AR.Messages.MaskItem })
			{
			}
		}

		public class SubListAttribute : CustomListAttribute
		{
			public SubListAttribute()
				: base(new string[] { MaskLocation, MaskItem, MaskEmployee, MaskCompany }, new string[] { AR.Messages.MaskLocation, AR.Messages.MaskItem, AR.Messages.MaskEmployee, AR.Messages.MaskCompany })
			{
			}
		}

		public const string MaskItem = "I";
		public const string MaskLocation = "L";
		public const string MaskEmployee = "E";
		public const string MaskCompany = "C";
	}

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class SOMiscSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ZZZZZZZZZB";
		public SOMiscSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, SOMiscAcctSubDefault.MaskItem, new SOMiscAcctSubDefault.SubListAttribute().AllowedValues, new SOMiscAcctSubDefault.SubListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new SOMiscAcctSubDefault.SubListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new SOMiscAcctSubDefault.SubListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}


	public class SOFreightAcctSubDefault
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

		public class AcctListAttribute : CustomListAttribute
		{
			public AcctListAttribute()
                : base(new string[] { OrderType, MaskLocation, MaskShipVia }, new string[] { Messages.OrderType, AR.Messages.MaskLocation, Messages.ShipVia })
			{
			}
		}

		public class SubListAttribute : CustomListAttribute
		{
			public SubListAttribute()
                : base(new string[] { OrderType, MaskLocation, MaskShipVia, MaskCompany }, new string[] { Messages.OrderType, AR.Messages.MaskLocation, Messages.ShipVia, AR.Messages.MaskCompany})
			{
			}
		}

		public const string MaskShipVia = "V";
		public const string MaskLocation = "L";
		public const string OrderType = "T";
        public const string MaskCompany = "C";
	}

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class SOFreightSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ZZZZZZZZZC";
		public SOFreightSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, SOFreightAcctSubDefault.MaskLocation, new SOFreightAcctSubDefault.SubListAttribute().AllowedValues, new SOFreightAcctSubDefault.SubListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField 
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new SOFreightAcctSubDefault.SubListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new SOFreightAcctSubDefault.SubListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}




	public class SODiscAcctSubDefault
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

		public class AcctListAttribute : CustomListAttribute
		{
			public AcctListAttribute()
				: base(new string[] { OrderType, MaskLocation }, new string[] { Messages.OrderType, AR.Messages.MaskLocation })
			{
			}
		}

		public class SubListAttribute : CustomListAttribute
		{
			public SubListAttribute()
				: base(new string[] { OrderType, MaskLocation, MaskCompany }, new string[] { Messages.OrderType, AR.Messages.MaskLocation, AR.Messages.MaskCompany })
			{
			}
		}

		public const string OrderType = "T";
		public const string MaskLocation = "L";
        public const string MaskCompany = "C";
	}

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class SODiscSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ZZZZZZZZZD";
		public SODiscSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, SODiscAcctSubDefault.MaskLocation, new SODiscAcctSubDefault.SubListAttribute().AllowedValues, new SODiscAcctSubDefault.SubListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField 
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new SODiscAcctSubDefault.SubListAttribute().AllowedValues, 0, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new SODiscAcctSubDefault.SubListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	public class SOCOGSAcctSubDefault
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

		public class AcctListAttribute : CustomListAttribute
		{
			public AcctListAttribute()
				: base(new string[] { MaskItem, MaskSite, MaskClass, MaskLocation }, new string[] { IN.Messages.MaskItem, IN.Messages.MaskSite, IN.Messages.MaskClass, AR.Messages.MaskLocation })
			{
			}
		}

		public class SubListAttribute : CustomListAttribute
		{
			public SubListAttribute()
				: base(new string[] { MaskItem, MaskSite, MaskClass, MaskLocation, MaskEmployee, MaskCompany, MaskSalesPerson }, new string[] { IN.Messages.MaskItem, IN.Messages.MaskSite, IN.Messages.MaskClass, AR.Messages.MaskLocation, AR.Messages.MaskEmployee, AR.Messages.MaskCompany, AR.Messages.MaskSalesPerson })
			{
			}
		}

		public const string MaskItem = "I";
		public const string MaskSite = "W";
		public const string MaskClass = "P";

		public const string MaskLocation = "L";
		public const string MaskEmployee = "E";
		public const string MaskCompany = "C";
		public const string MaskSalesPerson = "S";
	}

	[PXDBString(30, IsUnicode = true, InputMask = "")]
	[PXUIField(DisplayName = "Subaccount Mask", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
	public sealed class SOCOGSSubAccountMaskAttribute : AcctSubAttribute
	{
		private const string _DimensionName = "SUBACCOUNT";
		private const string _MaskName = "ZZZZZZZZZE";
		public SOCOGSSubAccountMaskAttribute()
			: base()
		{
			PXDimensionMaskAttribute attr = new PXDimensionMaskAttribute(_DimensionName, _MaskName, SOCOGSAcctSubDefault.MaskItem, new SOCOGSAcctSubDefault.SubListAttribute().AllowedValues, new SOCOGSAcctSubDefault.SubListAttribute().AllowedLabels);
			attr.ValidComboRequired = false;
			_Attributes.Add(attr);
			_SelAttrIndex = _Attributes.Count - 1;
		}

		public static string MakeSub<Field>(PXGraph graph, string mask, object[] sources, Type[] fields)
			where Field : IBqlField
		{
			try
			{
				return PXDimensionMaskAttribute.MakeSub<Field>(graph, mask, new SOCOGSAcctSubDefault.SubListAttribute().AllowedValues, 3, sources);
			}
			catch (PXMaskArgumentException ex)
			{
				PXCache cache = graph.Caches[BqlCommand.GetItemType(fields[ex.SourceIdx])];
				string fieldName = fields[ex.SourceIdx].Name;
				throw new PXMaskArgumentException(new SOCOGSAcctSubDefault.SubListAttribute().AllowedLabels[ex.SourceIdx], PXUIFieldAttribute.GetDisplayName(cache, fieldName));
			}
		}
	}

	public class SOSiteStatusLookup<Status, StatusFilter> : INSiteStatusLookup<Status, StatusFilter>
		where Status : class, IBqlTable, new()
		where StatusFilter:  SOSiteStatusFilter, new()
	{
		#region Ctor
		public SOSiteStatusLookup(PXGraph graph)
			:base(graph)
		{
			graph.RowSelecting.AddHandler(typeof(SOSiteStatusSelected), OnRowSelecting);
		}

		public SOSiteStatusLookup(PXGraph graph, Delegate handler)
			:base(graph, handler)
		{
			graph.RowSelecting.AddHandler(typeof(SOSiteStatusSelected), OnRowSelecting);
		}
		#endregion
		protected virtual void OnRowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			if (sender.Fields.Contains(typeof(SOSiteStatusSelected.curyID).Name) &&
					sender.GetValue<SOSiteStatusSelected.curyID>(e.Row) == null)
			{
				PXCache orderCache = sender.Graph.Caches[typeof(SOOrder)];
				sender.SetValue<SOSiteStatusSelected.curyID>(e.Row,
					orderCache.GetValue<SOOrder.curyID>(orderCache.Current));
				sender.SetValue<SOSiteStatusSelected.curyInfoID>(e.Row,
					orderCache.GetValue<SOOrder.curyInfoID>(orderCache.Current));				
			}
		}

		protected override void OnFilterSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			base.OnFilterSelected(sender, e);
			SOSiteStatusFilter filter = (SOSiteStatusFilter)e.Row;
			PXUIFieldAttribute.SetVisible<SOSiteStatusFilter.historyDate>(sender, null, filter.Mode == SOAddItemMode.ByCustomer);
			PXCache status = sender.Graph.Caches[typeof (SOSiteStatusSelected)];
			PXUIFieldAttribute.SetVisible<SOSiteStatusSelected.qtyLastSale>(status, null, filter.Mode == SOAddItemMode.ByCustomer);
			PXUIFieldAttribute.SetVisible<SOSiteStatusSelected.curyID>(status, null, filter.Mode == SOAddItemMode.ByCustomer);
			PXUIFieldAttribute.SetVisible<SOSiteStatusSelected.curyUnitPrice>(status, null, filter.Mode == SOAddItemMode.ByCustomer);
			PXUIFieldAttribute.SetVisible<SOSiteStatusSelected.lastSalesDate>(status, null, filter.Mode == SOAddItemMode.ByCustomer);
			if (filter.HistoryDate == null)
				filter.HistoryDate = DateTime.Today.AddMonths(-3);
		}
	}
	#region SOOpenPeriod
	/// <summary>
	/// Specialized version of the selector for SO Open Financial Periods.<br/>
	/// Displays a list of FinPeriods, having flags Active = true and  ARClosed = false and INClosed = false.<br/>
	/// </summary>
	public class SOOpenPeriodAttribute : OpenPeriodAttribute
	{
		#region Ctor
		public SOOpenPeriodAttribute(Type SourceType)
			: base(typeof(Search<FinPeriod.finPeriodID, Where<FinPeriod.iNClosed, Equal<False>, And<FinPeriod.aRClosed, Equal<False>, And<FinPeriod.active, Equal<True>>>>>), SourceType)
		{
		}

		public SOOpenPeriodAttribute()
			: this(null)
		{
		}
		#endregion
		
		#region Implementation
		public override void IsValidPeriod(PXCache sender, object Row, object NewValue)
		{
			string OldValue = (string)sender.GetValue(Row, _FieldName);
			base.IsValidPeriod(sender, Row, NewValue);			

			if (NewValue != null && _ValidatePeriod != PeriodValidation.Nothing)
			{
				FinPeriod p = (FinPeriod)PXSelect<FinPeriod, Where<FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>>>.Select(sender.Graph, (string)NewValue);
				if (p.Closed == true)
				{
                    if (PostClosedPeriods(sender.Graph))
                    {
                        sender.RaiseExceptionHandling(_FieldName, Row, null, new FiscalPeriodClosedException(p.FinPeriodID, PXErrorLevel.Warning));
                        return;
                    }
                    else
                    {
                        throw new FiscalPeriodClosedException(p.FinPeriodID);
                    }
                }
				if (p.ARClosed == true)
				{
                    if (PostClosedPeriods(sender.Graph))
                    {
                        sender.RaiseExceptionHandling(_FieldName, Row, null, new FiscalPeriodClosedException(p.FinPeriodID, PXErrorLevel.Warning));
                        return;
                    }
                    else
                    {
                        throw new FiscalPeriodClosedException(p.FinPeriodID);
                    }
                }
				if (p.INClosed == true)
				{
                    if (PostClosedPeriods(sender.Graph))
                    {
                        sender.RaiseExceptionHandling(_FieldName, Row, null, new FiscalPeriodClosedException(p.FinPeriodID, PXErrorLevel.Warning));
                        return;
                    }
                    else
                    {
                        throw new FiscalPeriodClosedException(p.FinPeriodID);
                    }
                }
			}
			return;
		}
		#endregion
	}
	#endregion




	/// <summary>
	/// Sets ManualDisc flag based on the values of the depending fields. Manual Flag is set when user
	/// overrides the discount values.
	/// This attribute also updates the relative fields. Ex: Updates Discount Amount when Discount Pct is modified.
	/// </summary>
	public class SOManualDiscMode : PXEventSubscriberAttribute, IPXRowUpdatedSubscriber, IPXRowInsertedSubscriber
	{
		private Type curyDiscAmt;
		private Type curyTranAmt;
		private Type freezeDisc;
		private Type discPct;

		public SOManualDiscMode(Type curyDiscAmt, Type curyTranAmt, Type discPct, Type freezeManualDisc):this(curyDiscAmt, discPct)
		{
			if (freezeManualDisc == null)
				throw new ArgumentNullException();

			if (curyTranAmt == null)
				throw new ArgumentNullException();

			this.freezeDisc = freezeManualDisc;
			this.curyTranAmt = curyTranAmt;
		}

		public SOManualDiscMode(Type curyDiscAmt, Type discPct)
		{
			if (curyDiscAmt == null)
				throw new ArgumentNullException("curyDiscAmt");
			if (discPct == null)
				throw new ArgumentNullException("discPct");
						
			this.curyDiscAmt = curyDiscAmt;
			this.discPct = discPct;
		}

		#region IPXRowUpdatedSubscriber Members

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{            
            IDiscountable row = (IDiscountable)e.Row;
			IDiscountable oldRow = (IDiscountable)e.OldRow;

			bool manualMode = false;//by default AutoMode.
			bool useDiscPct = false;//by default value in DiscAmt has higher priority then DiscPct when both are modified. 

			//Force Auto Mode 
			if (row.ManualDisc == false && oldRow.ManualDisc == true)
				manualMode = false;

			//Change to Manual Mode based on fields changed:
			if (row.ManualDisc == true)
				manualMode = true;

			if (row.IsFree == true && oldRow.IsFree != true)
				manualMode = true;

			if (row.DiscPct != oldRow.DiscPct && row.InventoryID == oldRow.InventoryID)
			{
				manualMode = true;
				useDiscPct = true;
			}

            if (row.Qty != oldRow.Qty && row.DiscPct == oldRow.DiscPct && row.CuryDiscAmt == oldRow.CuryDiscAmt)//if only qty was changed use DiscPct
            {
                useDiscPct = true;
            }

            if (row.CuryUnitPrice != oldRow.CuryUnitPrice && row.DiscPct == oldRow.DiscPct && row.CuryDiscAmt == oldRow.CuryDiscAmt)//if only unitPrice was changed use DiscPct
            {
                useDiscPct = true;
            }

			if (row.CuryExtPrice != oldRow.CuryExtPrice && row.DiscPct == oldRow.DiscPct && row.CuryDiscAmt == oldRow.CuryDiscAmt)//if only extPrice was changed use DiscPct
			{
				useDiscPct = true;
			}

			if (row.CuryDiscAmt != oldRow.CuryDiscAmt)
			{
				manualMode = true;
				useDiscPct = false;
			}

			//if only CuryLineAmt (Ext.Price) was changed for a line with DiscoutAmt<>0
			//for Contracts Qty * UnitPrice * Prorate(<>1) = ExtPrice
			if (row.CuryLineAmt != oldRow.CuryLineAmt && row.Qty == oldRow.Qty && row.CuryUnitPrice == oldRow.CuryUnitPrice && row.CuryExtPrice == oldRow.CuryExtPrice && row.DiscPct == oldRow.DiscPct && row.CuryDiscAmt == oldRow.CuryDiscAmt && row.CuryDiscAmt != 0)
			{
				manualMode = true;
			}

            decimal? validLineAmtRaw;
			decimal? validLineAmt = null;
			if (row.CuryLineAmt != oldRow.CuryLineAmt)
			{
				if (useDiscPct)
				{
					decimal val = row.CuryExtPrice ?? 0;

					decimal disctAmt;
					ARSetup arsetup = ARSetupSelect.Select(sender.Graph);
                    if (arsetup.LineDiscountTarget == LineDiscountTargetType.SalesPrice)
					{
						disctAmt = PXCurrencyAttribute.Round(sender, row, (row.Qty ?? 0m) * (row.CuryUnitPrice ?? 0m), CMPrecision.TRANCURY)
							- PXCurrencyAttribute.Round(sender, row, (row.Qty ?? 0m) * PXDBPriceCostAttribute.Round(sender, (row.CuryUnitPrice ?? 0m) * (1 - (row.DiscPct ?? 0m) * 0.01m)), CMPrecision.TRANCURY);
					}
					else
					{
						disctAmt = val * (row.DiscPct ?? 0m) * 0.01m;
						disctAmt = PXCurrencyAttribute.Round(sender, row, disctAmt, CMPrecision.TRANCURY);
					}

					validLineAmtRaw = row.CuryExtPrice - disctAmt;
					validLineAmt = PXCurrencyAttribute.Round(sender, row, validLineAmtRaw ?? 0, CMPrecision.TRANCURY);

				}
				else
				{
					validLineAmtRaw = row.CuryExtPrice - row.CuryDiscAmt;
					validLineAmt = PXCurrencyAttribute.Round(sender, row, validLineAmtRaw ?? 0, CMPrecision.TRANCURY);
				}

				

                if (row.CuryLineAmt != validLineAmt && row.DiscPct != oldRow.DiscPct)
					manualMode = true;
			}

            sender.SetValue(e.Row, this.FieldName, manualMode);

			//Process only Manual Mode:
            if (manualMode || sender.Graph.IsImport)
			{
                if (manualMode)
                    SODiscountEngine.ClearDetDiscComponents(row);
				//Update related fields:

				if (row.Qty == 0 && oldRow.Qty > 0)
				{
					sender.SetValueExt(row, sender.GetField(curyDiscAmt), 0m);
					sender.SetValueExt(row, sender.GetField(discPct), 0m);
				}
				else if (row.CuryLineAmt != oldRow.CuryLineAmt && !useDiscPct)
				{
					decimal? extAmt = row.CuryExtPrice ?? 0;
					if (extAmt - row.CuryLineAmt >= 0)
					{
						sender.SetValueExt(row, sender.GetField(curyDiscAmt), extAmt - row.CuryLineAmt);
						if (extAmt > 0)
						{
							decimal? pct = 100 * row.CuryDiscAmt / extAmt;
							sender.SetValueExt(row, sender.GetField(discPct), pct);
						}
					}
				}
				else if (row.CuryDiscAmt != oldRow.CuryDiscAmt)
				{
					if (row.CuryExtPrice > 0)
					{
						decimal? pct = (row.CuryDiscAmt ?? 0) * 100 / row.CuryExtPrice;
						sender.SetValueExt(row, sender.GetField(discPct), pct);
					}
				}
				else if (row.DiscPct != oldRow.DiscPct)
				{
					decimal val = row.CuryExtPrice ?? 0;

					decimal amt;
                    ARSetup arsetup = ARSetupSelect.Select(sender.Graph);
                    if (arsetup.LineDiscountTarget == LineDiscountTargetType.SalesPrice)
					{
						if (row.CuryUnitPrice != 0 && row.Qty != 0)//if sales price is available
						{
							amt = PXCurrencyAttribute.Round(sender, row, (row.Qty ?? 0m) * (row.CuryUnitPrice ?? 0m), CMPrecision.TRANCURY)
								- PXCurrencyAttribute.Round(sender, row, (row.Qty ?? 0m) * PXDBPriceCostAttribute.Round(sender, (row.CuryUnitPrice ?? 0m) * (1 - (row.DiscPct ?? 0m) * 0.01m)), CMPrecision.TRANCURY);
						}
						else
						{
							amt = val * (row.DiscPct ?? 0m) * 0.01m;
						}
					}
					else
					{
						amt = val * (row.DiscPct ?? 0m) * 0.01m;
					}

					sender.SetValueExt(row, sender.GetField(curyDiscAmt), amt);
				}
                else if (validLineAmt != null && row.CuryLineAmt != validLineAmt)
                {
                    decimal val = row.CuryExtPrice ?? 0;

                    decimal amt;
                    ARSetup arsetup = ARSetupSelect.Select(sender.Graph);
                    if (arsetup.LineDiscountTarget == LineDiscountTargetType.SalesPrice)
                    {
						if (row.CuryUnitPrice != 0 && row.Qty != 0)//if sales price is available
						{
							amt = PXCurrencyAttribute.Round(sender, row, (row.Qty ?? 0m) * (row.CuryUnitPrice ?? 0m), CMPrecision.TRANCURY)
								- PXCurrencyAttribute.Round(sender, row, (row.Qty ?? 0m) * PXDBPriceCostAttribute.Round(sender, (row.CuryUnitPrice ?? 0m) * (1 - (row.DiscPct ?? 0m) * 0.01m)), CMPrecision.TRANCURY);
						}
						else
						{
							amt = val * (row.DiscPct ?? 0m) * 0.01m;
						}
                    }
                    else
                    {
                        amt = val * (row.DiscPct ?? 0m) * 0.01m;
                    }

                    sender.SetValueExt(row, sender.GetField(curyDiscAmt), amt);
                }
				
				if (row.IsFree == true)
				{
					//certain fields must be disabled/enabled:
					sender.RaiseRowSelected(e.Row);
				}
			}
		}

		#endregion

		#region IPXRowInsertedSubscriber Members

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (freezeDisc == null)
				return;

			bool freeze = ((bool?)sender.GetValue(e.Row, sender.GetField(freezeDisc))) == true;

			//When a new row is inserted there is 2 possible ways of handling it:
			//1. Sync the Discounts fields DiscAmt and DiscPct and calculate LineAmt as ExtPrice - DiscAmt. If DiscAmt <> 0 then ManualDisc flag is set.
			//2. Add line as is without changing any of the fields.
			//First Mode is typically executed when a user adds a line to Invoice from UI. Moreover the user enters Only Ext.Amt on the UI.
			//Second Mode is when a line from SOOrder is added to SOInvoice - in this case all discounts must be freezed and line must be added as is.
						
			if (!freeze && !sender.Graph.IsImport)
			{
				IDiscountable row = (IDiscountable)e.Row;

				if (row.CuryDiscAmt != null && row.CuryDiscAmt != 0 && row.CuryExtPrice != 0)
				{
					row.DiscPct = 100 * row.CuryDiscAmt / row.CuryExtPrice;
					sender.SetValue(row, sender.GetField(curyTranAmt), row.CuryExtPrice - row.CuryDiscAmt);
					sender.SetValue(e.Row, this.FieldName, true);
				}
				else if (row.DiscPct != null && row.DiscPct != 0)
				{
					decimal val = row.CuryExtPrice ?? 0;
					decimal amt = val * (row.DiscPct ?? 0) * 0.01m;
					row.CuryDiscAmt = amt;
					sender.SetValue(row, sender.GetField(curyTranAmt), row.CuryExtPrice - row.CuryDiscAmt);
					sender.SetValue(e.Row, this.FieldName, true);
				}
				else if (row.CuryExtPrice != null && row.CuryExtPrice != 0m)
				{
					sender.SetValue(row, sender.GetField(curyTranAmt), row.CuryExtPrice);
				}
			}
		}

		#endregion
	}

	public class SOCashSaleCashTranIDAttribute : CashTranIDAttribute
	{
		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			SOInvoice soinvoice = (SOInvoice)orig_Row;
			if (soinvoice.Released == true || soinvoice.CuryPaymentAmt == null || soinvoice.CuryPaymentAmt == 0m)
			{
				return null;
			}

			catran_Row.OrigModule = BatchModule.AR;
			catran_Row.OrigTranType = soinvoice.DocType;
			catran_Row.OrigRefNbr = soinvoice.RefNbr;
			catran_Row.ExtRefNbr = soinvoice.ExtRefNbr ?? string.Format(AR.Messages.ARAutoPaymentRefNbrFormat, soinvoice.AdjDate);
			catran_Row.CashAccountID = soinvoice.CashAccountID;
			catran_Row.CuryInfoID = soinvoice.CuryInfoID;
			catran_Row.CuryID = soinvoice.CuryID;

			switch (soinvoice.DocType)
			{
				case ARDocType.CashSale:
					catran_Row.CuryTranAmt = soinvoice.CuryPaymentAmt;
					catran_Row.DrCr = "D";
					break;
				case ARDocType.CashReturn:
					catran_Row.CuryTranAmt = -soinvoice.CuryPaymentAmt;
					catran_Row.DrCr = "C";
					break;
				default:
					return null;
			}

			catran_Row.TranDate = soinvoice.AdjDate;
			catran_Row.TranDesc = string.Empty;
			catran_Row.FinPeriodID = soinvoice.AdjFinPeriodID;
			catran_Row.ReferenceID = soinvoice.CustomerID;
			catran_Row.Released = false;
			catran_Row.Hold = false;
			catran_Row.Cleared = soinvoice.Cleared;
			catran_Row.ClearDate = soinvoice.ClearDate;

			return catran_Row;
		}
	}

	/// <summary>
	/// Extends <see cref="LocationAvailAttribute"/> and shows the availability of Inventory Item for the given location.
	/// </summary>
	/// <example>
	/// [SOLocationAvail(typeof(SOLine.inventoryID), typeof(SOLine.subItemID), typeof(SOLine.siteID), typeof(SOLine.tranType), typeof(SOLine.invtMult))]
	/// </example>
	public class SOLocationAvailAttribute : LocationAvailAttribute
	{ 
		public SOLocationAvailAttribute(Type InventoryType, Type SubItemType, Type SiteIDType, Type TranType, Type InvtMultType)
			: base(InventoryType, SubItemType, SiteIDType, null, null, null)
		{
			_IsSalesType = BqlCommand.Compose(
				typeof(Where<,,>),
				TranType,
				typeof(Equal<INTranType.issue>),
				typeof(Or<,,>),
				TranType,
				typeof(Equal<INTranType.invoice>),
				typeof(Or<,>),
				TranType,
				typeof(Equal<INTranType.debitMemo>)
				);

			_IsReceiptType = BqlCommand.Compose(
				typeof(Where<,,>),
				TranType,
				typeof(Equal<INTranType.receipt>),
				typeof(Or<,,>),
				TranType,
				typeof(Equal<INTranType.return_>),
				typeof(Or<,>),
				TranType,
				typeof(Equal<INTranType.creditMemo>)
				);

			_IsTransferType = BqlCommand.Compose(
				typeof(Where<,,>),
				TranType,
				typeof(Equal<INTranType.transfer>),
				typeof(And<,,>),
				InvtMultType,
				typeof(Equal<short1>),
				typeof(Or<,,>),
				TranType,
				typeof(Equal<INTranType.transfer>),
				typeof(And<,>),
				InvtMultType,
				typeof(Equal<shortMinus1>)
				);

			_IsStandardCostAdjType = BqlCommand.Compose(
				typeof(Where<,,>),
				TranType,
				typeof(Equal<INTranType.standardCostAdjustment>),
				typeof(Or<,>),
				TranType,
				typeof(Equal<INTranType.negativeCostAdjustment>)
				);
		}
	}

	/// <summary>
	/// Specialized for SOLine version of the CrossItemAttribute.<br/> 
	/// Providing an Inventory ID selector for the field, it allows also user <br/>
	/// to select both InventoryID and SubItemID by typing AlternateID in the control<br/>
	/// As a result, if user type a correct Alternate id, values for InventoryID, SubItemID, <br/>
	/// and AlternateID fields in the row will be set.<br/>
	/// In this attribute, InventoryItems with a status inactive, markedForDeletion,<br/>
	/// noSale and noRequest are filtered out. It also fixes  INPrimaryAlternateType parameter to CPN <br/>    
	/// This attribute may be used in combination with AlternativeItemAttribute on the AlternateID field of the row <br/>
	/// <example>
	/// [SOLineInventoryItem(Filterable = true)]
	/// </example>
	/// </summary>
	[PXDBInt()]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where2<Match<Current<AccessInfo.userName>>,
							 And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
									And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
								And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noSales>,
									And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noRequest>>>>>>), IN.Messages.ItemCannotSale)]
	public class SOLineInventoryItemAttribute : CrossItemAttribute
	{

		/// <summary>
		/// Default ctor
		/// </summary>
		public SOLineInventoryItemAttribute()
			: base(typeof(Where<boolTrue, Equal<boolTrue>>), INPrimaryAlternateType.CPN)
		{
		}

		/// <summary>
		/// Extended ctor. User may specified additional Where clause for the InventoryItem Selector, which will be combined with the default one.
		/// </summary>		
		/// <param name="Where"> Must be IBqlWhere type. Allows to specify additional where criteria for select.</param>
		public SOLineInventoryItemAttribute(Type Where)
			: base(BqlCommand.Compose(typeof(Where<>), Where), INPrimaryAlternateType.CPN)
		{
		}

		/// <summary>
		/// Extended ctor. User may specified both Where  and Join clause for the InventoryItem Selector, which will be combined with the default one.
		/// </summary>	
		/// <param name="JoinType">Must be IBqlJoin type. Allows to join other tables to select.</param>
		/// <param name="Where"> Must be IBqlWhere type. Allows to specify additional where criteria for select.</param>
		public SOLineInventoryItemAttribute(Type JoinType, Type Where)
			: base(JoinType, BqlCommand.Compose(typeof(Where<>), Where), INPrimaryAlternateType.CPN)
		{

		}
	}
}
