using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.TX;

namespace PX.Objects.SO
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class SOReleaseInvoice : PXGraph<SOReleaseInvoice>
	{
		public PXCancel<SOInvoiceFilter> Cancel;
		public PXFilter<SOInvoiceFilter> Filter;
		[PXFilterable]
		public PXFilteredProcessing<ARInvoice, SOInvoiceFilter> SOInvoiceList;

		#region Cache Attached
		#region ARInvoice
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault()]
		[ARDocType.SOEntryList()]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		protected virtual void ARInvoice_DocType_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
        [ARInvoiceType.RefNbr(typeof(Search2<SOInvoice.refNbr,
                    InnerJoin<ARInvoice, On<ARInvoice.docType, Equal<SOInvoice.docType>, And<ARInvoice.refNbr, Equal<SOInvoice.refNbr>>>,
					InnerJoin<Customer, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>>,
					Where<SOInvoice.docType, Equal<Optional<SOInvoice.docType>>,
					And<Match<Customer, Current<AccessInfo.userName>>>>, OrderBy<Desc<SOInvoice.refNbr>>>), Filterable = true)]
		[ARInvoiceType.Numbering()]
		[ARInvoiceNbr()]
		protected virtual void ARInvoice_RefNbr_CacheAttached(PXCache sender)
		{
		}
		[ARCustomerCredit(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Filterable = true)]
		[PXDefault()]
		protected virtual void ARInvoice_CustomerID_CacheAttached(PXCache sender)
		{
		}
		[SOOpenPeriod(typeof(ARRegister.docDate))]
		[PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_FinPeriodID_CacheAttached(PXCache sender)
		{
		}
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<Customer.termsID, Where<Customer.bAccountID, Equal<Current<ARInvoice.customerID>>, And<Current<ARInvoice.docType>, NotEqual<ARDocType.creditMemo>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		[SOInvoiceTerms()]
		protected virtual void ARInvoice_TermsID_CacheAttached(PXCache sender)
		{
		}
		[PXDBDate()]
		[PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_DueDate_CacheAttached(PXCache sender)
		{
		}
		[PXDBDate()]
		[PXUIField(DisplayName = "Cash Discount Date", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_DiscDate_CacheAttached(PXCache sender)
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.origDocAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_CuryOrigDocAmt_CacheAttached(PXCache sender)
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.docBal), BaseCalc = false)]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		protected virtual void ARInvoice_CuryDocBal_CacheAttached(PXCache sender)
		{
		}
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(ARInvoice.curyInfoID), typeof(ARInvoice.origDiscAmt))]
		[PXUIField(DisplayName = "Cash Discount", Visibility = PXUIVisibility.SelectorVisible)]
		protected virtual void ARInvoice_CuryOrigDiscAmt_CacheAttached(PXCache sender)
		{
		}
		#endregion		
		#endregion

		public SOReleaseInvoice()
		{
			SOInvoiceList.SetSelected<ARInvoice.selected>();
			PXUIFieldAttribute.SetVisible<SOInvoice.cCCapturedAmt>(Caches[typeof(SOInvoice)], null, false);
			PXUIFieldAttribute.SetVisible<SOInvoice.paymentTotal>(Caches[typeof(SOInvoice)], null, false);
		}

		public PXAction<SOInvoiceFilter> viewDocument;
		[PXUIField(DisplayName = Messages.ViewDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable ViewDocument(PXAdapter adapter)
		{
			if (SOInvoiceList.Current != null)
			{
				SOInvoiceEntry docgraph = PXGraph.CreateInstance<SOInvoiceEntry>();
				docgraph.Document.Current = docgraph.Document.Search<ARInvoice.refNbr>(SOInvoiceList.Current.RefNbr, SOInvoiceList.Current.DocType);
				throw new PXRedirectRequiredException(docgraph, true, "Order") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
			}
			return adapter.Get();
		}

		public virtual void SOInvoiceFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			SOInvoiceFilter filter = e.Row as SOInvoiceFilter;
			if (filter != null && !String.IsNullOrEmpty(filter.Action))
			{
				Dictionary<string, object> parameters = Filter.Cache.ToDictionary(filter);
				SOInvoiceList.SetProcessTarget(null, null, null, filter.Action, parameters);

                string actionID = (string)SOInvoiceList.GetTargetFill(null, null, null, Filter.Current.Action, "@ActionName");
                PXUIFieldAttribute.SetVisible<SOInvoiceFilter.showFailedCCCapture>(sender, e.Row, actionID == "CaptureCCPayment");
			}
		}

		protected bool _ActionChanged = false;

		public virtual void SOInvoiceFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			_ActionChanged = !sender.ObjectsEqual<SOInvoiceFilter.action>(e.Row, e.OldRow);
		}

		public virtual IEnumerable soInvoiceList()
		{
			if (Filter.Current.Action == "<SELECT>")
			{
				yield break;
			}

			SOInvoiceFilter filter = Filter.Current;


			string actionID = (string)SOInvoiceList.GetTargetFill(null, null, null, Filter.Current.Action, "@ActionName");

			if (_ActionChanged)
			{
				SOInvoiceList.Cache.Clear();
			}

			foreach (ARInvoice item in SOInvoiceList.Cache.Updated)
			{
				yield return item;
			}

			PXSelectBase<ARInvoice> cmd;

			switch (actionID)
			{
				case "Release":
					cmd = new PXSelectJoin<ARInvoice, 
						InnerJoin<SOInvoice, On<SOInvoice.docType, Equal<ARInvoice.docType>, And<SOInvoice.refNbr, Equal<ARInvoice.refNbr>>>>, 
						Where<ARInvoice.hold, Equal<boolFalse>, And<ARInvoice.released, Equal<boolFalse>>>>(this);
					break;
				case "Post":
					cmd = new PXSelectJoinGroupBy<ARInvoice, 
						InnerJoin<SOInvoice, On<SOInvoice.docType, Equal<ARInvoice.docType>, And<SOInvoice.refNbr, Equal<ARInvoice.refNbr>>>,
						InnerJoin<SOOrderShipment, On<SOOrderShipment.invoiceType, Equal<ARInvoice.docType>, And<SOOrderShipment.invoiceNbr, Equal<ARInvoice.refNbr>>>,
						InnerJoin<SOOrderType, On<SOOrderType.orderType, Equal<SOOrderShipment.orderType>>,
						InnerJoin<ARTran, On<ARTran.tranType, Equal<ARInvoice.docType>, And<ARTran.refNbr, Equal<ARInvoice.refNbr>>>,
						LeftJoin<INTran, On<INTran.aRDocType, Equal<ARTran.tranType>, And<INTran.aRRefNbr, Equal<ARTran.refNbr>, And<INTran.aRLineNbr, Equal<ARTran.lineNbr>>>>>>>>>,
						Where<ARInvoice.released, Equal<boolTrue>, And<SOOrderType.iNDocType, NotEqual<INTranType.noUpdate>, And<SOOrderShipment.invtRefNbr, IsNull, And<ARTran.lineType, Equal<SOLineType.inventory>, And<INTran.refNbr, IsNull>>>>>,
						Aggregate<
							GroupBy<ARInvoice.docType, 
							GroupBy<ARInvoice.refNbr,
							GroupBy<ARInvoice.released>>>>>(this);
					break;
				case "CaptureCCPayment":
					cmd = new PXSelectJoin<ARInvoice, 
						InnerJoin<SOInvoice, On<SOInvoice.docType, Equal<ARInvoice.docType>, And<SOInvoice.refNbr, Equal<ARInvoice.refNbr>>>,
						InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<SOInvoice.pMInstanceID>>,
						InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>, 
							And<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>, 
							And<PaymentMethod.aRIsProcessingRequired, Equal<True>>>>>>>,
						Where<SOInvoice.isCCCaptureFailed, Equal<Current<SOInvoiceFilter.showFailedCCCapture>>>>(this);
					break;
				case "CreditHold":
					cmd = new PXSelectJoin<ARInvoice,
						InnerJoin<SOInvoice, On<SOInvoice.docType, Equal<ARInvoice.docType>, And<SOInvoice.refNbr, Equal<ARInvoice.refNbr>>>,
						LeftJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<SOInvoice.pMInstanceID>>,
						LeftJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>,
							And<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>,
							And<PaymentMethod.aRIsProcessingRequired, Equal<True>>>>>>>,
						Where<PaymentMethod.paymentMethodID, IsNull>>(this); 
						break;
				default:
					cmd = new PXSelectJoin<ARInvoice,
						InnerJoin<SOInvoice, On<SOInvoice.docType, Equal<ARInvoice.docType>, And<SOInvoice.refNbr, Equal<ARInvoice.refNbr>>>>>(this);
					break;
			}

			cmd.WhereAnd<Where<ARInvoice.docDate, LessEqual<Current<SOInvoiceFilter.endDate>>>>();

			if (filter.StartDate != null)
			{
				cmd.WhereAnd<Where<ARInvoice.docDate, GreaterEqual<Current<SOInvoiceFilter.startDate>>>>();
			}

			if (filter.CustomerID != null)
			{
				cmd.WhereAnd<Where<ARInvoice.customerID, Equal<Current<SOInvoiceFilter.customerID>>>>();
			}

            int startRow = PXView.StartRow;
            int totalRows = 0;

			foreach (PXResult<ARInvoice, SOInvoice> res in cmd.View.Select(PXView.Currents, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				ARInvoice item = res;
				SOInvoice ext = res;

				object paymentmethod;
				if ((paymentmethod = res[typeof(PaymentMethod)]) != null)
				{
					item.IsCCPayment = ((PaymentMethod)paymentmethod).ARIsProcessingRequired != null;
				}
				else
				{
					item.IsCCPayment = false;
				}

				if ((item = (ARInvoice)SOInvoiceList.Cache.Locate(item)) == null || SOInvoiceList.Cache.GetStatus(item) == PXEntryStatus.Notchanged)
				{
					yield return (ARInvoice)res;
				}

                PXView.StartRow = 0;
			}
		}
	}

    [Serializable]
	public partial class SOInvoiceFilter : IBqlTable
	{
		#region Action
		public abstract class action : PX.Data.IBqlField
		{
		}
		protected string _Action;
		[PXAutomationMenu]
		public virtual string Action
		{
			get
			{
				return this._Action;
			}
			set
			{
				this._Action = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _StartDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
		[PXDefault()]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _EndDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : IBqlField
		{
		}
		protected int? _CustomerID;
		[PXUIField(DisplayName = "Customer ID")]
		[Customer(DescriptionField = typeof(Customer.acctName))]
		public virtual int? CustomerID
		{
			get
			{
				return _CustomerID;
			}
			set
			{
				_CustomerID = value;
			}
		}
		#endregion
        #region ShowFailedCCCapture
        public abstract class showFailedCCCapture : IBqlField { }
        [PXUIField(DisplayName = "Show Failed CC Capture")]
        [PXDBBool()]
        [PXDefault(false)]
        public bool? ShowFailedCCCapture
        {
            get;
            set;
        }
        #endregion
    }
}
