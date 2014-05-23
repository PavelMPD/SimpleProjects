using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.CA;
using PX.Objects.GL;


namespace PX.Objects.AR
{
    public class ARDunningLetterProcess : PXGraph<ARDunningLetterProcess>
    {
        /// <summary>
        /// Filter
        /// </summary>
        [System.SerializableAttribute()]
        public partial class ARDunningLetterRecordsParameters : IBqlTable
        {
            #region CustomerClass
            public abstract class customerClassID : PX.Data.IBqlField
            {
            }
            protected String _CustomerClassID;
            [PXDBString(10, IsUnicode = true)]
            [PXUIField(DisplayName = "Customer Class", Visibility = PXUIVisibility.Visible)]
            [PXSelector(typeof(CustomerClass.customerClassID))]
            public virtual String CustomerClassID
            {
                get
                {
                    return this._CustomerClassID;
                }
                set
                {
                    this._CustomerClassID = value;
                }
            }
            #endregion
            #region DocDate
            public abstract class docDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _DocDate;
            [PXDate()]
            [PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "Dunning Letter Date", Visibility = PXUIVisibility.Visible)]
            public virtual DateTime? DocDate
            {
                get
                {
                    return this._DocDate;
                }
                set
                {
                    this._DocDate = value;
                }
            }
            #endregion
            #region DueDate
            public abstract class dueDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _DueDate;
            [PXDate()]
            //[PXDefault(typeof(AccessInfo.businessDate))]
            [PXUIField(DisplayName = "Process Date", Visibility = PXUIVisibility.Visible)]
            public virtual DateTime? DueDate
            {
                get
                {
                    return (this._DocDate.HasValue) ? this._DocDate.Value.AddDays(-30) : this._DocDate;
                }
            }
            #endregion
        }

        /// <summary>
        /// Items for processing
        /// </summary>
        [Serializable]
        public partial class ARDunningLetterList : IBqlTable
        {
            #region Selected
            public abstract class selected : PX.Data.IBqlField
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
            #region CustomerClass
            public abstract class customerClassID : PX.Data.IBqlField
            {
            }
            protected String _CustomerClassID;
            [PXDBString(10, IsUnicode = true)]
            [PXUIField(DisplayName = "Customer Class", Visibility = PXUIVisibility.Visible)]
            public virtual String CustomerClassID
            {
                get
                {
                    return this._CustomerClassID;
                }
                set
                {
                    this._CustomerClassID = value;
                }
            }
            #endregion
            #region BranchID
            public abstract class branchID : PX.Data.IBqlField
            {
            }
            protected Int32? _BranchID;
            [PXDBInt(IsKey=true)]
            [PXDefault()]
            [Branch(typeof(PX.Objects.GL.Branch.branchID))]
            [PXUIField(DisplayName = "Branch ID")]
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
            #region BAccountID
            public abstract class bAccountID : PX.Data.IBqlField
            {
            }
            protected Int32? _BAccountID;
            [PXDBInt(IsKey = true)]
            [PXDefault()]
            [Customer(DescriptionField = typeof(Customer.acctName))]
            [PXUIField(DisplayName = "Customer ID")]
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
            #region DocDate
            public abstract class docDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _DocDate;
            [PXDBDate(IsKey = true)]
            [PXDefault()]
            [PXUIField(DisplayName = "Doc Date")]
            public virtual DateTime? DocDate
            {
                get
                {
                    return this._DocDate;
                }
                set
                {
                    this._DocDate = value;
                }
            }
            #endregion
            
            #region DueDate
            public abstract class dueDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _DueDate;
            [PXDBDate(IsKey = true)]
            [PXDefault()]
            [PXUIField(DisplayName = "Earliest Due Date")]
            public virtual DateTime? DueDate
            {
                get
                {
                    return this._DueDate;
                }
                set
                {
                    this._DueDate = value;
                }
            }
            #endregion
            #region NumberOfDocuments
            public abstract class numberOfDocuments : PX.Data.IBqlField
            {
            }
            protected int? _NumberOfDocuments;
            [PXInt()]
            [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Number Of Documents", Visible=false)]
            public virtual int? NumberOfDocuments
            {
                get
                {
                    return this._NumberOfDocuments;
                }
                set
                {
                    this._NumberOfDocuments = value;
                }
            }
            #endregion
            #region NumberOfOverdueDocuments
            public abstract class numberOfOverdueDocuments : PX.Data.IBqlField
            {
            }
            protected int? _NumberOfOverdueDocuments;
            [PXInt()]
            [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Number Of Overdue Documents")]
            public virtual int? NumberOfOverdueDocuments
            {
                get
                {
                    return this._NumberOfOverdueDocuments;
                }
                set
                {
                    this._NumberOfOverdueDocuments = value;
                }
            }
            #endregion
            #region OrigDocAmt
            public abstract class origDocAmt : PX.Data.IBqlField
            {
            }
            protected Decimal? _OrigDocAmt;
            [PXDBBaseCury()]
            [PXUIField(DisplayName = "Customer Balance")]
            public virtual Decimal? OrigDocAmt
            {
                get
                {
                    return this._OrigDocAmt;
                }
                set
                {
                    this._OrigDocAmt = value;
                }
            }
            #endregion
            #region DocBal
            public abstract class docBal : PX.Data.IBqlField
            {
            }
            protected Decimal? _DocBal;
            [PXDBBaseCury()]
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

            #region DunningLetterLevel
            public abstract class dunningLetterLevel : PX.Data.IBqlField
            {
            }
            protected int? _DunningLetterLevel;
            [PXInt()]
            [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Dunning Letter Level")]
            public virtual int? DunningLetterLevel
            {
                get
                {
                    return this._DunningLetterLevel;
                }
                set
                {
                    this._DunningLetterLevel = value;
                }
            }
            #endregion
            #region LastDunningLetterDate
            public abstract class lastDunningLetterDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _LastDunningLetterDate;
            [PXDBDate(IsKey = true)]
            [PXDefault()]
            [PXUIField(DisplayName = "Last Dunning Letter Date")]
            public virtual DateTime? LastDunningLetterDate
            {
                get
                {
                    return this._LastDunningLetterDate;
                }
                set
                {
                    this._LastDunningLetterDate = value;
                }
            }
            #endregion
            #region DueDays
            public abstract class dueDays : PX.Data.IBqlField
            {
            }
            protected Int32? _DueDays;
            [PXDBInt()]
            [PXDefault()]
            [PXUIField(DisplayName = "Due Days")]
            public virtual Int32? DueDays
            {
                get
                {
                    return this._DueDays;
                }
                set
                {
                    this._DueDays = value;
                }
            }
            #endregion

        }


        
        public PXFilter<ARDunningLetterRecordsParameters> Filter;
        public PXCancel<ARDunningLetterRecordsParameters> Cancel;

        [PXFilterable]
        public PXFilteredProcessing<ARDunningLetterList, ARDunningLetterRecordsParameters> DunningLetterList;

        public List<int> D_DueDays=new List<int>();
        public List<ARDunningSetup> DunningSetupList = new List<ARDunningSetup>(3);
        public int  D_MaxLevel = 0;

        public ARDunningLetterProcess()
        {
            DunningLetterList.Cache.AllowDelete = false;
            DunningLetterList.Cache.AllowInsert = false;
            DunningLetterList.Cache.AllowUpdate = true;

            int prevDays=0;
            foreach (ARDunningSetup xx in PXSelectOrderBy<ARDunningSetup, OrderBy<Asc<ARDunningSetup.dunningLetterLevel>>>.Select(this))
            {
                this.DunningSetupList.Add(xx);
                if (xx.DueDays.HasValue)
                {
                    D_DueDays.Add(xx.DueDays.Value - prevDays);
                    prevDays = xx.DueDays.Value;
                    D_MaxLevel++;
                }
            }

            DunningLetterList.SetProcessDelegate<DunningLetterMassProcess>(DunningLetterProc);
            PXUIFieldAttribute.SetEnabled(DunningLetterList.Cache, null, false);
            PXUIFieldAttribute.SetEnabled<ARDunningLetterList.selected>(DunningLetterList.Cache, null, true);
            DunningLetterList.SetSelected<ARDunningLetterList.selected>();
        }

        #region Delegate for select
        protected virtual IEnumerable dunningLetterList()
        {
            this.DunningLetterList.Cache.Clear();

            ARDunningLetterRecordsParameters header = Filter.Current;
            if (header == null || header.DocDate == null)
            {
                yield break;
            }


            bool Cons = false;
            int? DBranch = null;
            bool canAccessToBranch = false;

            try
            {
                ARSetup SetupV = PXSelect<ARSetup, Where<MatchWithBranch<ARSetup.dunningLetterBranchID>>>.Select(this);
                if (SetupV != null)
                {
                    Cons = SetupV.ConsolidatedDunningLetter ?? false;
                    DBranch = SetupV.DunningLetterBranchID;
                    canAccessToBranch=true;
                }
            }
            catch {}

            if (!canAccessToBranch)
            {
                yield break;
            }

            //Select due customers. (ARInvoice.dueDate < "current date")
            int? OldBAccount = null;
            int? OldBranch = null;
            foreach (PXResult<Customer, ARInvoice> item in
                PXSelectJoinGroupBy<Customer,
                 InnerJoin<ARInvoice, On<ARInvoice.customerID, Equal<Customer.bAccountID>>>,
                Where<ARInvoice.released, Equal<boolTrue>,
                    And<ARInvoice.openDoc, Equal<boolTrue>,
                    And<ARInvoice.voided, Equal<boolFalse>,
                    And<Where<ARInvoice.docType, Equal<ARDocType.invoice>, Or<ARInvoice.docType, Equal<ARDocType.finCharge>,
                    And<ARInvoice.dueDate, Less<Required<ARDunningLetterRecordsParameters.docDate>>>>
                            >>>>>, Aggregate<GroupBy<Customer.bAccountID,GroupBy<ARInvoice.branchID>>>>
                    .Select(this, header.DocDate))
            {
                Customer cust = item;
                ARInvoice br = item;

                if (header.CustomerClassID != null)
                    if (cust.CustomerClassID != header.CustomerClassID) continue;   // CustomerClassID filter

                int? BranchID = (Cons ? DBranch : br.BranchID);

                if ((BranchID == OldBranch)&&(OldBAccount == cust.BAccountID)) continue;    // Group by BranchID
                if (OldBAccount != cust.BAccountID)
                {
                    OldBranch = null;
                    OldBAccount = cust.BAccountID;
                }

                OldBranch = BranchID;

                ARDunningLetterList rec = new ARDunningLetterList();
                rec.CustomerClassID = cust.CustomerClassID;
                rec.BAccountID = cust.BAccountID;
                rec.BranchID = BranchID;
                rec.DocDate = header.DocDate;

                bool Show = true;
                DateTime maxRD = DateTime.MinValue;
                int rlevel = 0;

                //Find last active (not closed) Dunning Letter for each Customer
                foreach (PXResult<ARDunningLetter> ii in
                    PXSelectJoinGroupBy<ARDunningLetter,
                    InnerJoin<ARDunningLetterDetail, On<ARDunningLetterDetail.dunningLetterID, Equal<ARDunningLetter.dunningLetterID>,
                        And<ARDunningLetterDetail.overdue,Equal<boolTrue>>>,
                    InnerJoin<ARInvoice, On<ARInvoice.docType, Equal<ARDunningLetterDetail.docType>, 
                        And<ARInvoice.refNbr, Equal<ARDunningLetterDetail.refNbr>,
                        And<ARInvoice.released, Equal<boolTrue>,
                        And<ARInvoice.openDoc, Equal<boolTrue>,
                        And<ARInvoice.voided, Equal<boolFalse>>>>>>>>,
                    Where<ARDunningLetter.bAccountID, Equal<Required<ARDunningLetter.bAccountID>>,
                        And<ARDunningLetter.branchID, Equal<Required<ARDunningLetter.branchID>>>
                    >,
                    Aggregate<GroupBy<ARDunningLetter.dunningLetterLevel, GroupBy<ARDunningLetter.dunningLetterDate>>
                    >>.Select(this, cust.BAccountID, BranchID))
                {
                    ARDunningLetter RR = ii;
                    if (maxRD < RR.DunningLetterDate.Value)
                    {
                        maxRD = RR.DunningLetterDate.Value;
                        rlevel = RR.DunningLetterLevel.Value;
                    }
                }
                if (rlevel >= D_MaxLevel) Show = false; // Dunning Letter max level is exceeded
                else if (rlevel > 0) // has active Dunning Letter
                {
                    if (maxRD.AddDays(D_DueDays[rlevel]) > header.DocDate.Value)
                        Show = false;   //Letter is not overdue, not process
                }
                if (Show)   // need to process
                {

                    rec.NumberOfDocuments = 0;
                    rec.NumberOfOverdueDocuments = 0;
                    rec.OrigDocAmt = 0;
                    rec.DocBal = 0;
                    DateTime minD = DateTime.Today;

                    // Selecl all invoices 
                    foreach (PXResult<ARInvoice> ix in
                        PXSelect<ARInvoice,
                        Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>,
                            And<ARInvoice.released, Equal<boolTrue>,
                            And<ARInvoice.openDoc, Equal<boolTrue>,
                            And<ARInvoice.voided, Equal<boolFalse>,
                            And<Where<ARInvoice.docType, Equal<ARDocType.invoice>, Or<ARInvoice.docType, Equal<ARDocType.finCharge>,
                            And<ARInvoice.docDate, Less<Required<ARInvoice.docDate>>>>>>>>>>
                        >.Select(this, cust.BAccountID, header.DocDate))
                    {
                        ARInvoice dl = ix;

                        if (!Cons && dl.BranchID != BranchID) continue; // alien branch

                        minD = minD < dl.DueDate.Value ? minD : dl.DueDate.Value; 
                        rec.NumberOfDocuments++;
                        rec.NumberOfOverdueDocuments += (dl.DueDate > header.DocDate ? 0 : 1);
                        rec.OrigDocAmt += dl.DocBal;
                        rec.DocBal += (dl.DueDate > header.DocDate ? 0 : dl.DocBal);
                    }
                    rec.DunningLetterLevel = rlevel + 1;
                    rec.DueDate = minD;
                    ARDunningSetup settings = this.DunningSetupList[rlevel];
                    rec.DueDays = (settings.DaysToSettle??0);
                    //rec.DueDays = D_DueDays[rlevel];
                    
                    if (rlevel>0) rec.LastDunningLetterDate = maxRD;
                    else rec.LastDunningLetterDate = null;

                    if (minD.AddDays(D_DueDays[rlevel]) < header.DocDate) //Has at least one overdue invoice
                    {
                        try
                        {
                            this.DunningLetterList.Insert(rec);
                        }
                        catch {}
                    }
                }
                
            }

            this.DunningLetterList.Cache.IsDirty = false;
            foreach (ARDunningLetterList ret in this.DunningLetterList.Cache.Inserted)
                yield return ret;

        }
        #endregion

        #region Processing
        public static void DunningLetterProc(DunningLetterMassProcess aGraph, ARDunningLetterList aCard)
        {
            aGraph.DunningLetterProc(aCard);
        }
        #endregion
    }

    [PXHidden()]
    public class DunningLetterMassProcess : PXGraph<DunningLetterMassProcess>
    {

        public int? D_MaxLevel = null;
        public bool Cons = false;

        #region Selects
        [PXViewNameAttribute("DunningLetter")]
        public PXSelect<ARDunningLetter> docs;
        [PXViewNameAttribute("DunningLetterDetail")]
        public PXSelect<ARDunningLetterDetail, Where<ARDunningLetterDetail.dunningLetterID, Equal<Required<ARDunningLetter.dunningLetterID>>>> docsDet;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public DunningLetterMassProcess()
        {

            try
            {
                ARSetup SetupV = PXSelect<ARSetup>.Select(this);
                Cons = SetupV.ConsolidatedDunningLetter.Value;
            }
            catch { }

            PXResult<ARDunningSetup> r = PXSelectGroupBy<ARDunningSetup, Aggregate<Max<ARDunningSetup.dunningLetterLevel>>>.Select(this);
            try
            {
                D_MaxLevel = ((ARDunningSetup)r).DunningLetterLevel;
            }
            catch
            {
                D_MaxLevel = 0;
            }
        }

        /// <summary>
        /// Process selected rows
        /// </summary>
        /// <param name="aDoc">Row of selection</param>
        public virtual void DunningLetterProc(PX.Objects.AR.ARDunningLetterProcess.ARDunningLetterList aDoc)
        {
            this.Clear();
            ARDunningLetter doc = new ARDunningLetter();
            
            doc.BAccountID = aDoc.BAccountID;
            doc.BranchID = aDoc.BranchID;
            doc.DunningLetterDate = aDoc.DocDate;
            doc.Deadline = aDoc.DocDate.Value.AddDays(aDoc.DueDays.Value);
            doc.DunningLetterLevel = aDoc.DunningLetterLevel;
            doc.Consolidated = Cons;
            doc.Printed = false;
            doc.Emailed = false;
            doc.DontPrint = false;
            doc.DontEmail = false;
            doc.LastLevel = aDoc.DunningLetterLevel >= D_MaxLevel;
            doc = docs.Insert(doc);
            int? R_ID = doc.DunningLetterID;
            foreach (PXResult<ARInvoice> ii in
                PXSelect<ARInvoice,
                Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>,
                    And<ARInvoice.branchID, Equal<Required<ARInvoice.branchID>>,
                    And<ARInvoice.released, Equal<boolTrue>,
                    And<ARInvoice.openDoc, Equal<boolTrue>,
                    And<ARInvoice.voided, Equal<boolFalse>,
                    And<Where<ARInvoice.docType, Equal<ARDocType.invoice>, Or<ARInvoice.docType, Equal<ARDocType.finCharge>,
                    And<ARInvoice.docDate, Less<Required<ARInvoice.docDate>>>>>>>>>>>
                >.Select(this, aDoc.BAccountID, aDoc.BranchID, aDoc.DocDate))
            {
                ARInvoice dl = ii;
                ARDunningLetterDetail docDet = new ARDunningLetterDetail();

                docDet.DunningLetterID = R_ID;
                docDet.CuryOrigDocAmt = dl.CuryOrigDocAmt;
                docDet.CuryDocBal = dl.CuryDocBal;
                docDet.CuryID = dl.CuryID;
                docDet.OrigDocAmt = dl.OrigDocAmt;
                docDet.DocBal = dl.DocBal;
                docDet.DueDate = dl.DueDate;
                docDet.DocType = dl.DocType;
                docDet.RefNbr = dl.RefNbr;
                docDet.DocDate = dl.DocDate;
                docDet.Overdue = dl.DueDate < aDoc.DocDate;
                docDet = docsDet.Insert(docDet);
            }

            this.Actions.PressSave();

        }


        public ARDunningLetter DunningLetterDoc(int Letter)
        {
            PXResult<ARDunningLetter> RD = PXSelect<ARDunningLetter, 
                Where<ARDunningLetter.dunningLetterID, Equal<Required<ARDunningLetter.dunningLetterID>>>>
            .Select(this, Letter);

            ARDunningLetter doc = RD;
            return (doc);
        }

    }
}
