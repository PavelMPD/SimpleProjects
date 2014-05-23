using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.CR;

namespace PX.Objects.AR
{
    public class ARCustomerCreditHoldProcess : PXGraph<ARCustomerCreditHoldProcess>
    {
        /// <summary>
        /// Filter
        /// </summary>
        [System.SerializableAttribute()]
        public partial class CreditHoldParameters : IBqlTable
        {
            #region Action
            public abstract class action : PX.Data.IBqlField
            {
            }
            protected Int32? _Action;
            [PXDBInt]
            [PXDefault(0)]
            [PXUIField(DisplayName = "Action")]
            [PXIntList(new int[] { 0, 1 },
                new string[] { "Apply Credit Hold", "Release Credit Hold" })]
            public virtual Int32? Action
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

            #region BeginDate
            public abstract class beginDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _BeginDate;
            [PXDate]
            [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible)]
            public virtual DateTime? BeginDate
            {
                get
                {
                    return this._BeginDate;
                }
                set
                {
                    this._BeginDate = value;
                }
            }
            #endregion
            #region EndDate
            public abstract class endDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _EndDate;
            [PXDate]
            [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "End Date", Visibility = PXUIVisibility.Visible)]
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

            #region ShowAll
            public abstract class showAll : IBqlField
            {
            }
            protected bool? _ShowAll = false;
            [PXDBBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Show All")]
            public virtual bool? ShowAll
            {
                get
                {
                    return _ShowAll;
                }
                set
                {
                    _ShowAll = value;
                }
            }
            #endregion

        }

        #region DetailsResult
        /// <summary>
        /// Data for view
        /// </summary>
        [Serializable]
        public partial class DetailsResult : IBqlTable
        {
            #region Selected
            public abstract class selected : IBqlField
            {
            }
            protected bool? _Selected = false;
            [PXBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Selected")]
            public virtual bool? Selected
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

            #region DunningLetterID
            public abstract class dunningLetterID : PX.Data.IBqlField
            {
            }
            protected Int32? _DunningLetterID;
            [PXUIField(Enabled = false)]
            public virtual Int32? DunningLetterID
            {
                get
                {
                    return this._DunningLetterID;
                }
                set
                {
                    this._DunningLetterID = value;
                }
            }
            #endregion

            #region CustomerId
            public abstract class customerId : PX.Data.IBqlField
            {
            }
            protected Int32? _CustomerId;
            [PXInt(IsKey = true)]
            [Customer(DescriptionField = typeof(Customer.acctName))]
            [PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.Visible)]
            public virtual Int32? CustomerId
            {
                get
                {
                    return this._CustomerId;
                }
                set
                {
                    this._CustomerId = value;
                }
            }
            #endregion

            #region DunningLetterDate
            public abstract class dunningLetterDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _DunningLetterDate;
            [PXDBDate()]
            [PXDefault(TypeCode.DateTime, "01/01/1900")]
            [PXUIField(DisplayName = "Dunning Letter Date")]
            public virtual DateTime? DunningLetterDate
            {
                get
                {
                    return this._DunningLetterDate;
                }
                set
                {
                    this._DunningLetterDate = value;
                }
            }
            #endregion

            #region DocBal
            public abstract class docBal : PX.Data.IBqlField
            {
            }
            protected Decimal? _DocBal;
            [PXDBBaseCury()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Overdue Balance")]
            public virtual Decimal? DocBal
            {
                get
                {
                    return this._DocBal;
                }
                set
                {
                    this._DocBal = value;
                }
            }
            #endregion
            #region InvBal
            public abstract class invBal : PX.Data.IBqlField
            {
            }
            protected Decimal? _InvBal;
            [PXDBBaseCury()]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Customer Balance")]
            public virtual Decimal? InvBal
            {
                get
                {
                    return this._InvBal;
                }
                set
                {
                    this._InvBal = value;
                }
            }
            #endregion
            #region Status
            public abstract class status : BAccount.status
            {
            }
            protected String _Status;
            [PXUIField(DisplayName = "Status")]
            [status.List()]
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



            /// <summary>
            /// Copy field values from selection and aggregate details
            /// </summary>
            /// <param name="G">Graph</param>
            /// <param name="aSrc">Selected DunningLetter</param>
            /// <param name="cust">Selected Customer</param>
            public virtual void Copy(PXGraph G, ARDunningLetter aSrc, Customer cust)
            {
                this.CustomerId = cust.BAccountID;
                this.DunningLetterID = aSrc.DunningLetterID;
                this.DunningLetterDate = aSrc.DunningLetterDate;
                this.DocBal = 0;
                this.InvBal = 0;
                this.Status = cust.Status;

                foreach (PXResult<ARDunningLetterDetail> it in PXSelect<ARDunningLetterDetail,
                    Where<ARDunningLetterDetail.dunningLetterID,
                        Equal<Required<ARDunningLetterDetail.dunningLetterID>>>>
                    .Select(G, aSrc.DunningLetterID))
                {
                    ARDunningLetterDetail d = it;
                    this.DocBal += d.DocBal;
                }

            }

        }
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public ARCustomerCreditHoldProcess()
        {
            try
            {
                Details.Cache.AllowDelete = false;
                Details.Cache.AllowInsert = false;
                Details.SetSelected<ARInvoice.selected>();  //Field for process
                Details.SetProcessCaption(IN.Messages.Process);
                Details.SetProcessAllCaption(IN.Messages.ProcessAll);
            }
            catch { }
        }
        #endregion

        public PXCancel<CreditHoldParameters> Cancel;

        public PXFilter<CreditHoldParameters> Filter;

        [PXFilterable]
        public PXFilteredProcessing<DetailsResult, CreditHoldParameters> Details;

        #region Delegates

        /// <summary>
        /// Generates a list of documents that meet the filter criteria.
        /// This list is used for display in the processing screen
        /// </summary>
        /// <returns>List of Customers with Dunning Letters</returns>
        protected virtual IEnumerable details()
        {
            CreditHoldParameters header = Filter.Current;
            List<DetailsResult> result = new List<DetailsResult>();
            if (header == null)
                yield break;

            bool AllShow = header.ShowAll ?? false;

            if (this.Filter.Current.Action == 0)
            {
                foreach (PXResult<Customer, ARDunningLetter> it in PXSelectJoin<Customer,
                         InnerJoin<ARDunningLetter, On<Customer.bAccountID, Equal<ARDunningLetter.bAccountID>,
                             And<ARDunningLetter.lastLevel, Equal<boolTrue>>>>,
                         Where<ARDunningLetter.dunningLetterDate,
                                 Between<Required<ARDunningLetter.dunningLetterDate>, Required<ARDunningLetter.dunningLetterDate>>>,
                         OrderBy<Asc<ARDunningLetter.bAccountID>>>
                         .Select(this, header.BeginDate, header.EndDate))
                {
                    ARDunningLetter dl = it;
                    Customer cust = it;
                    if (!AllShow && cust.Status != BAccount.status.Active) continue;

                    DetailsResult res = new DetailsResult();
                    res.Copy(this, dl, cust);

                    //==============================================================================
                    foreach (PXResult<ARInvoice> ix in
                        PXSelect<ARInvoice,
                        Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>,
                            And<ARInvoice.released, Equal<boolTrue>,
                            And<ARInvoice.openDoc, Equal<boolTrue>,
                            And<ARInvoice.voided, Equal<boolFalse>,
                            And<ARInvoice.docType, Equal<AP.APDocType.invoice>,
                            And<ARInvoice.docDate, Between<Required<ARInvoice.docDate>, Required<ARInvoice.docDate>>>>>>>>
                        >.Select(this, cust.BAccountID, header.BeginDate, header.EndDate))
                    {
                        ARInvoice inv = ix;

                        if (inv.BranchID != dl.BranchID) continue; // alien branch

                        res.InvBal += inv.DocBal;
                    }
                    result.Add(res);
                }
            }
            else if (this.Filter.Current.Action == 1)
            {
                foreach (PXResult<Customer, ARDunningLetter> it in PXSelectJoin<Customer,
                         LeftJoin<ARDunningLetter, On<Customer.bAccountID, Equal<ARDunningLetter.bAccountID>,
                             And<ARDunningLetter.lastLevel, Equal<boolTrue>>>>>
                         .Select(this))
                {
                    ARDunningLetter dl = it;
                    Customer cust = it;
                    if (!AllShow && cust.Status != BAccount.status.CreditHold) continue;

                    DetailsResult res = new DetailsResult();
                    res.Copy(this, dl, cust);

                    //==============================================================================
                    foreach (PXResult<ARInvoice> ix in
                        PXSelect<ARInvoice,
                        Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>,
                            And<ARInvoice.released, Equal<boolTrue>,
                            And<ARInvoice.openDoc, Equal<boolTrue>,
                            And<ARInvoice.voided, Equal<boolFalse>,
                            And<ARInvoice.docType, Equal<AP.APDocType.invoice>,
                            And<ARInvoice.docDate, Less<Required<ARInvoice.docDate>>>>>>>>
                        >.Select(this, cust.BAccountID, header.EndDate))
                    {
                        ARInvoice inv = ix;

                        //if (inv.BranchID != dl.BranchID) continue; // alien branch

                        res.InvBal += inv.DocBal;
                    }
                    result.Add(res);
                }
            }
            else yield break;
            foreach (var item in result)
            {
                Details.Cache.SetStatus(item, PXEntryStatus.Held);
                yield return item;
            }
            Details.Cache.IsDirty = false;
        }
        #endregion

        #region Filter Events

        protected virtual void CreditHoldParameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CreditHoldParameters o = (CreditHoldParameters)e.Row;
            if (o != null)
            {
                CreditHoldParameters filter = (CreditHoldParameters)this.Filter.Cache.CreateCopy(o);
                switch (o.Action)
                {
                    case 0:
                        Details.SetProcessDelegate<DLCustomerUpdate>(Proc0);
                        break;
                    case 1:
                        Details.SetProcessDelegate<DLCustomerUpdate>(Proc1);
                        break;
                }
            }
        }

        protected virtual void CreditHoldParameters_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            Details.Cache.Clear();
        }

        protected virtual void CreditHoldParameters_RowPersisting(PXCache sedner, PXRowPersistingEventArgs e)
        {
            e.Cancel = true;
        }
        #endregion

        #region Process Delegates

        public static void Proc0(DLCustomerUpdate graph, DetailsResult aDoc)
        {
                Customer C = graph.Customers.Search<Customer.bAccountID>(aDoc.CustomerId);
                if(C != null)
                {
                    graph.Customers.Current = C;
                    Customer copy= (Customer) graph.Customers.Cache.CreateCopy(C);
                    copy.Status = BAccount.status.CreditHold;
                    graph.Customers.Cache.Update(copy);
                }
                graph.Actions.PressSave();       
        }
        public static void Proc1(DLCustomerUpdate graph, DetailsResult aDoc)
        {
            Customer C = graph.Customers.Search<Customer.bAccountID>(aDoc.CustomerId);
            if (C != null)
            {
                graph.Customers.Current = C;
                Customer copy = (Customer)graph.Customers.Cache.CreateCopy(C);
                copy.Status = BAccount.status.Active;
                graph.Customers.Cache.Update(copy);
            }
            graph.Actions.PressSave();
        }


        #endregion
    }

    [PXHidden()]
    public class DLCustomerUpdate : PXGraph<DLCustomerUpdate>
    {

        public PXSelect<Customer> Customers;
      

    }
}
