using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Api;
using PX.SM;
using PX.Data.EP;

namespace PX.Objects.CA
{    
    [Serializable]
	public class CABankStatementEntry : PXGraph<CABankStatementEntry, CABankStatement>, PXImportAttribute.IPXPrepareItems
	{

		#region Internal Types Declaration

        [Serializable]
		public partial class CABankStmtDetailDocRef : PX.Data.IBqlTable
		{
			#region Selected
			public abstract class selected : PX.Data.IBqlField
			{
			}
			protected Boolean? _Selected;
			[PXBool()]
			[PXUIField(DisplayName = "Selected")]
			public virtual Boolean? Selected
			{
				get
				{
					return this._Selected;
				}
				set
				{
					this._Selected = value;
				}
			}
			#endregion
			#region RefNbr
			public abstract class refNbr : PX.Data.IBqlField
			{
			}
			protected String _RefNbr;
			[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
			[PXDBDefault(typeof(CABankStatement.refNbr))]
			public virtual String RefNbr
			{
				get
				{
					return this._RefNbr;
				}
				set
				{
					this._RefNbr = value;
				}
			}
			#endregion
			#region LineNbr
			public abstract class lineNbr : PX.Data.IBqlField
			{
			}
			protected Int32? _LineNbr;
			[PXDBInt(IsKey = true)]
			[PXParent(typeof(Select<CABankStatementDetail, Where<CABankStatementDetail.refNbr, Equal<Current<CABankStmtDetailDocRef.refNbr>>,
										And<CABankStatementDetail.lineNbr, Equal<Current<CABankStmtDetailDocRef.lineNbr>>>>>))]
			public virtual Int32? LineNbr
			{
				get
				{
					return this._LineNbr;
				}
				set
				{
					this._LineNbr = value;
				}
			}
			#endregion
			#region CATranID
			public abstract class cATranID : PX.Data.IBqlField
			{
			}
			protected Int64? _CATranID;

			[PXDBLong(IsKey = true)]
			public virtual Int64? CATranID
			{
				get
				{
					return this._CATranID;
				}
				set
				{
					this._CATranID = value;
				}
			}
			#endregion
			#region CashAccountID
			public abstract class cashAccountID : PX.Data.IBqlField
			{
			}
			protected Int32? _CashAccountID;
			[PXDBInt()]
			[PXUIField(DisplayName = "Cash Account ID", Visible = false)]
			[PXDefault()]
			public virtual int? CashAccountID
			{
				get
				{
					return this._CashAccountID;
				}
				set
				{
					this._CashAccountID = value;
				}
			}
			#endregion
			#region MatchRelevance
			public abstract class matchRelevance : PX.Data.IBqlField
			{
			}
			protected Decimal? _MatchRelevance;
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXDBDecimal(6)]
			[PXUIField(DisplayName = "Match Relevance", Enabled = false)]
			public virtual Decimal? MatchRelevance
			{
				get
				{
					return this._MatchRelevance;
				}
				set
				{
					this._MatchRelevance = value;
				}
			}
			#endregion

			public void Copy(CATran aSrc)
			{
				this.CashAccountID = aSrc.CashAccountID;
				this.CATranID = aSrc.TranID;
			}
			public void Copy(CABankStatementDetail aSrc)
			{
				this.RefNbr = aSrc.RefNbr;
				this.LineNbr = aSrc.LineNbr;
				this.CashAccountID = aSrc.CashAccountID;
			}

		}  
        [Serializable]
		public partial class CATranExt : CATran
		{            
			#region IsMatched
			public abstract class isMatched : PX.Data.IBqlField
			{
			}
			protected Boolean? _IsMatched;
			[PXBool()]
			[PXUIField(DisplayName = "Matched")]
			public virtual Boolean? IsMatched
			{
				get
				{
					return this._IsMatched;
				}
				set
				{
					this._IsMatched = value;
				}
			}
			#endregion
			#region MatchRelevance
			public abstract class matchRelevance : PX.Data.IBqlField
			{
			}
			protected Decimal? _MatchRelevance;
			[PXDefault(TypeCode.Decimal, "0.0",PersistingCheck = PXPersistingCheck.Nothing)]
			[PXDecimal(6)]
			[PXUIField(DisplayName = "Match Relevance", Enabled = false)]
			public virtual Decimal? MatchRelevance
			{
				get
				{
					return this._MatchRelevance;
				}
				set
				{
					this._MatchRelevance = value;
				}
			}
			#endregion
			#region Released
			public new abstract class released : PX.Data.IBqlField
			{
			}
			
			[PXDBBool()]
			[PXUIField(DisplayName ="Released", Enabled=false)]
			[PXDefault(false)]
			public override Boolean? Released
			{
				get
				{
					return this._Released;
				}
				set
				{
					this._Released = value;
				}
			}
			#endregion
		}

        [Serializable]
        public partial class CATran2 : CATran
        {
            #region TranID
            public new abstract class tranID : PX.Data.IBqlField
            {
            }
            #endregion
            #region CashAccountID
            public new abstract class cashAccountID : PX.Data.IBqlField
            {
            }
            #endregion
            #region VoidedTranID
            public new abstract class voidedTranID : PX.Data.IBqlField
            {
            }
            #endregion
        }
		public partial class AdjustInfo : CA.ICADocAdjust 
		{
			private string _AdjdDocType;
			private string _AdjdRefNbr;
			private Decimal? _CuryAdjgAmount;
            private Decimal? _CuryAdjgWhTaxAmt;
            private Decimal? _CuryAdjgDiscAmt;
            private Decimal? _AdjdCuryRate;
            

			#region ICADocAdjust Members

			public string AdjdDocType
			{
				get
				{
					return this._AdjdDocType;
				}
				set
				{
					this._AdjdDocType = value;
				}
			}

			public string AdjdRefNbr
			{
				get
				{
					return this._AdjdRefNbr;
				}
				set
				{
					this._AdjdRefNbr= value;
				}
			}

			public Decimal? CuryAdjgAmount
			{
				get
				{
					return this._CuryAdjgAmount;
				}
				set
				{
					this._CuryAdjgAmount = value;
				}
			}
            public Decimal? CuryAdjgDiscAmt
            {
                get
                {
                    return this._CuryAdjgDiscAmt;
                }
                set
                {
                    this._CuryAdjgDiscAmt = value;
                }
            }
            public Decimal? CuryAdjgWhTaxAmt
            {
                get
                {
                    return this._CuryAdjgWhTaxAmt;
                }
                set
                {
                    this._CuryAdjgWhTaxAmt = value;
                }
            }
            public Decimal? AdjdCuryRate
            {
                get
                {
                    return this._AdjdCuryRate;
                }
                set
                {
                    this._AdjdCuryRate = value;
                }
            }

            #endregion

			public void Copy(APRegister src) 
			{
				this._AdjdDocType = src.DocType;
				this._AdjdRefNbr = src.RefNbr;
			}

			public void Copy(ARRegister src)
			{
				this._AdjdDocType = src.DocType;
				this._AdjdRefNbr = src.RefNbr;
			}
		}

		public interface IMatchSettings
		{
			int? DisbursementTranDaysBefore { get; set;}
			int? DisbursementTranDaysAfter { get; set; }

			int? ReceiptTranDaysBefore { get; set; }
			int? ReceiptTranDaysAfter { get; set; }

			Decimal? RefNbrCompareWeight { get; set; }
			Decimal? DateCompareWeight { get; set; }
			Decimal? PayeeCompareWeight { get; set; }

			Decimal? DateMeanOffset { get; set; }
			Decimal? DateSigma { get; set; }
			Boolean? MatchInSelection { get; set; }
            Boolean? SkipVoided { get; set; }
		}

        [Serializable]
		public partial class MatchSettings : IBqlTable, IMatchSettings
		{
			#region ReceiptTranDaysBefore
			public abstract class receiptTranDaysBefore : PX.Data.IBqlField
			{
			}
			protected Int32? _ReceiptTranDaysBefore;
			[PXInt(MinValue = 0, MaxValue =365)]
			[PXDefault(5, typeof(CASetup.receiptTranDaysBefore), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Days before bank transaction date")]
			public virtual Int32? ReceiptTranDaysBefore
			{
				get
				{
					return this._ReceiptTranDaysBefore;
				}
				set
				{
					this._ReceiptTranDaysBefore = value;
				}
			}
			#endregion
			#region ReceiptTranDaysAfter
			public abstract class receiptTranDaysAfter : PX.Data.IBqlField
			{
			}
			protected Int32? _ReceiptTranDaysAfter;
			[PXInt(MinValue = 0, MaxValue = 365)]
			[PXDefault(2, typeof(CASetup.receiptTranDaysAfter), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Days after bank transaction date")]
			public virtual Int32? ReceiptTranDaysAfter
			{
				get
				{
					return this._ReceiptTranDaysAfter;
				}
				set
				{
					this._ReceiptTranDaysAfter = value;
				}
			}
			#endregion
			#region DisbursementTranDaysBefore
			public abstract class disbursementTranDaysBefore : PX.Data.IBqlField
			{
			}
			protected Int32? _DisbursementTranDaysBefore;
			[PXInt(MinValue = 0,MaxValue=365)]
			[PXDefault(5, typeof(CASetup.disbursementTranDaysBefore), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Days before bank transaction date")]
			public virtual Int32? DisbursementTranDaysBefore
			{
				get
				{
					return this._DisbursementTranDaysBefore;
				}
				set
				{
					this._DisbursementTranDaysBefore = value;
				}
			}
			#endregion
			#region DisbursementTranDaysAfter
			public abstract class disbursementTranDaysAfter : PX.Data.IBqlField
			{
			}
			protected Int32? _DisbursementTranDaysAfter;
			[PXInt(MinValue = 0,MaxValue=365)]
			[PXDefault(2, typeof(CASetup.disbursementTranDaysAfter), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Days after bank transaction date")]
			public virtual Int32? DisbursementTranDaysAfter
			{
				get
				{
					return this._DisbursementTranDaysAfter;
				}
				set
				{
					this._DisbursementTranDaysAfter = value;
				}
			}
			#endregion

			#region RefNbrCompareWeight
			public abstract class refNbrCompareWeight : PX.Data.IBqlField
			{
			}
			protected Decimal? _RefNbrCompareWeight;
			[PXDecimal(MinValue = 0, MaxValue = 100.0)]
			[PXDefault(TypeCode.Decimal, "70.0", typeof(CASetup.refNbrCompareWeight), 
				PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Ref. Nbr. Weight")]
			public virtual Decimal? RefNbrCompareWeight
			{
				get
				{
					return this._RefNbrCompareWeight;
				}
				set
				{
					this._RefNbrCompareWeight = value;
				}
			}
			#endregion
			#region DateCompareWeight
			public abstract class dateCompareWeight : PX.Data.IBqlField
			{
			}
			protected Decimal? _DateCompareWeight;
			[PXDecimal(MinValue = 0, MaxValue = 100)]
			[PXDefault(TypeCode.Decimal, "20.0",
				typeof(CASetup.dateCompareWeight),
				PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Doc. Date Weight")]
			public virtual Decimal? DateCompareWeight
			{
				get
				{
					return this._DateCompareWeight;
				}
				set
				{
					this._DateCompareWeight = value;
				}
			}
			#endregion
			#region PayeeCompareWeight
			public abstract class payeeCompareWeight : PX.Data.IBqlField
			{
			}
			protected Decimal? _PayeeCompareWeight;
			[PXDecimal(MinValue = 0, MaxValue = 100)]
			[PXDefault(TypeCode.Decimal, "10.0",
					typeof(CASetup.payeeCompareWeight),
					PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Doc. Payee Weight")]
			public virtual Decimal? PayeeCompareWeight
			{
				get
				{
					return this._PayeeCompareWeight;
				}
				set
				{
					this._PayeeCompareWeight = value;
				}
			}
			#endregion

			protected  Decimal TotalWeight
			{
				get
				{
					decimal total = (this._DateCompareWeight ?? Decimal.Zero) 
									+ (this.RefNbrCompareWeight ?? Decimal.Zero)
									+ (this.PayeeCompareWeight ?? Decimal.Zero);
					return total;
				}
				
			}
			#region RefNbrComparePercent
			public abstract class refNbrComparePercent : PX.Data.IBqlField
			{
			}
			
			[PXDecimal()]
			[PXUIField(DisplayName = "%",Enabled=false)]
			public virtual Decimal? RefNbrComparePercent
			{
				get
				{
					Decimal total = this.TotalWeight;
					return ((total != Decimal.Zero ? (this.RefNbrCompareWeight / total) : Decimal.Zero) * 100.0m); 
				}
				set
				{
					
				}
			}
			#endregion
			#region DateComparePercent
			public abstract class dateComparePercent : PX.Data.IBqlField
			{
			}
			[PXDecimal()]
			[PXUIField(DisplayName = "%", Enabled = false)]
			public virtual Decimal? DateComparePercent
			{
				get
				{
					Decimal total = this.TotalWeight;
					return ((total != Decimal.Zero ? (this.DateCompareWeight / total) : Decimal.Zero) * 100.0m); 
				}
				set
				{
			
				}
			}
			#endregion
			#region PayeeComparePercent
			public abstract class payeeComparePercent : PX.Data.IBqlField
			{
			}
			
			[PXDecimal()]
			[PXUIField(DisplayName = "%", Enabled = false)]
			public virtual Decimal? PayeeComparePercent
			{
				get
				{
					Decimal total = this.TotalWeight;
					return ((total != Decimal.Zero ? (this.PayeeCompareWeight / total) : Decimal.Zero) * 100.0m); 
				}
				set
				{
			
				}
			}
			#endregion
			#region DateMeanOffset
			public abstract class dateMeanOffset: PX.Data.IBqlField
			{
			}
			protected Decimal? _DateMeanOffset;
			[PXDecimal(MinValue = -365, MaxValue = 365)]
			[PXDefault(TypeCode.Decimal, "10.0",
				typeof(CASetup.dateMeanOffset),
					PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Payment Clearing Average Delay")]
			public virtual Decimal? DateMeanOffset
			{
				get
				{
					return this._DateMeanOffset;
				}
				set
				{
					this._DateMeanOffset = value;
				}
			}
			#endregion
			#region DateSigma
			public abstract class dateSigma : PX.Data.IBqlField
			{
			}
			protected Decimal? _DateSigma;
			[PXDecimal(MinValue = 0, MaxValue = 365)]
			[PXDefault(TypeCode.Decimal, "5.0",
				typeof(CASetup.dateSigma),
					PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Estimated Deviation (days)")]
			public virtual Decimal? DateSigma
			{
				get
				{
					return this._DateSigma;
				}
				set
				{
					this._DateSigma = value;
				}
			}
			#endregion
			#region MatchInSelection
			public abstract class matchInSelection : PX.Data.IBqlField
			{
			}
			protected Boolean? _MatchInSelection;
			[PXBool()]
			[PXDefault(false,
					typeof(CASetup.matchInSelection),
					PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Match in the selection only")]
			public virtual Boolean? MatchInSelection
			{
				get
				{
					return this._MatchInSelection;
				}
				set
				{
					this._MatchInSelection = value;
				}
			}
			#endregion

            #region SkipVoided
            public abstract class skipVoided : PX.Data.IBqlField
            {
            }
            protected Boolean? _SkipVoided;
            [PXBool()]
            [PXDefault(false, typeof(CASetup.skipVoided),
                    PersistingCheck = PXPersistingCheck.Nothing)]
            [PXUIField(DisplayName = "Skip Voided transactions during matching")]
            public virtual Boolean? SkipVoided
            {
                get
                {
                    return this._SkipVoided;
                }
                set
                {
                    this._SkipVoided = value;
                }
            }
            #endregion

		}

		public static class ImportProviderParams
		{
			public const string FileName ="FileName";
			public const string StatementNbr ="RefNbr";
			public const string BatchSequenceStartingNbr = "BatchStartNumber";
		}

        public partial class GeneralInvoice : IBqlTable
        {            
            #region RefNbr
            public abstract class refNbr : PX.Data.IBqlField
            {
            }
            private String _RefNbr;
            [PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
            [PXString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]           
            public String RefNbr
            {
                get
                {
                    return this._RefNbr;
                }
                set
                {
                    this._RefNbr = value;
                }
            }
            #endregion
            #region OrigModule
            public abstract class origModule : PX.Data.IBqlField
            {
            }
            protected String _OrigModule;
            [PXString(2, IsFixed = true)]            
            [PXUIField(DisplayName = "Source", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
            [GL.BatchModule.FullList()]
            public virtual String OrigModule
            {
                get
                {
                    return this._OrigModule;
                }
                set
                {
                    this._OrigModule = value;
                }
            }
            #endregion
            #region DocType
            public abstract class docType : PX.Data.IBqlField
            {
            }
            protected String _DocType;
            [PXString(3, IsKey = true, IsFixed = true)]            
            [PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
            public virtual String DocType
            {
                get
                {
                    return this._DocType;
                }
                set
                {
                    this._DocType = value;
                }
            }
            #endregion
            #region DocDate
            public abstract class docDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _DocDate;
            [PXDate()]            
            [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
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
            #region FinPeriodID
            public abstract class finPeriodID : PX.Data.IBqlField
            {
            }
            protected String _FinPeriodID;
            [APOpenPeriod(typeof(APRegister.docDate))]
            [PXDefault()]
            [PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual String FinPeriodID
            {
                get
                {
                    return this._FinPeriodID;
                }
                set
                {
                    this._FinPeriodID = value;
                }
            }
            #endregion
            #region BAccountID
            public abstract class bAccountID : PX.Data.IBqlField
            {
            }
            protected Int32? _bAccountID;            
            [PXDefault()]
            public virtual Int32? BAccountID
            {
                get
                {
                    return this._bAccountID;
                }
                set
                {
                    this._bAccountID = value;
                }
            }
            #endregion
            #region LocationID
            public abstract class locationID : PX.Data.IBqlField
            {
            }
            protected Int32? _LocationID;
            [LocationID(
                typeof(Where<Location.bAccountID, Equal<Optional<GeneralInvoice.bAccountID>>,
                    And<Location.isActive, Equal<boolTrue>,
                    And<MatchWithBranch<Location.vBranchID>>>>),
                DescriptionField = typeof(Location.descr),
                Visibility = PXUIVisibility.SelectorVisible)]
            public virtual Int32? LocationID
            {
                get
                {
                    return this._LocationID;
                }
                set
                {
                    this._LocationID = value;
                }
            }
            #endregion
            #region CuryID
            public abstract class curyID : PX.Data.IBqlField
            {
            }
            protected String _CuryID;
            [PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
            [PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
            [PXDefault(typeof(Search<Company.baseCuryID>))]
            [PXSelector(typeof(Currency.curyID))]
            public virtual String CuryID
            {
                get
                {
                    return this._CuryID;
                }
                set
                {
                    this._CuryID = value;
                }
            }
            #endregion
            #region CuryOrigDocAmt
            public abstract class curyOrigDocAmt : PX.Data.IBqlField
            {
            }
            protected Decimal? _CuryOrigDocAmt;
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXDecimal(4)]
            [PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual Decimal? CuryOrigDocAmt
            {
                get
                {
                    return this._CuryOrigDocAmt;
                }
                set
                {
                    this._CuryOrigDocAmt = value;
                }
            }
            #endregion
            #region CuryDocBal
            public abstract class curyDocBal : PX.Data.IBqlField
            {
            }
            protected Decimal? _CuryDocBal;
            [PXDecimal(4)]
            [PXDefault(TypeCode.Decimal, "0.0")]            
            [PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
            public virtual Decimal? CuryDocBal
            {
                get
                {
                    return this._CuryDocBal;
                }
                set
                {
                    this._CuryDocBal = value;
                }
            }
            #endregion
            #region Status
            public abstract class status : PX.Data.IBqlField
            {
            }
            protected String _Status;
            [PXString(1, IsFixed = true)]            
            [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]            
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
            #region DueDate
            public abstract class dueDate : PX.Data.IBqlField
            {
            }
            protected DateTime? _DueDate;
            [PXDate()]
            [PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
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
            #endregion            
        }

        public class PXInvoiceSelectorAttribute : PXCustomSelectorAttribute
        {
            protected Type _BatchModule;

            public PXInvoiceSelectorAttribute(Type BatchModule)
                : base(typeof(GeneralInvoice.refNbr),
                   typeof(GeneralInvoice.refNbr),
                   typeof(GeneralInvoice.docDate),
                   typeof(GeneralInvoice.finPeriodID),
                   typeof(GeneralInvoice.locationID),
                   typeof(GeneralInvoice.curyID),
                   typeof(GeneralInvoice.curyOrigDocAmt),
                   typeof(GeneralInvoice.curyDocBal),
                   typeof(GeneralInvoice.status),
                   typeof(GeneralInvoice.dueDate))
                
            {                
                this._BatchModule = BatchModule;
            }

            protected virtual IEnumerable GetRecords()
            {
                PXCache cache = this._Graph.Caches[BqlCommand.GetItemType(this._BatchModule)];
                PXCache adjustments = this._Graph.Caches[typeof(CABankStatementAdjustment)];
                object current = null;
                foreach (object item in PXView.Currents)
                {
                    if (item != null && (item.GetType() == typeof(CABankStatementAdjustment) || item.GetType().IsSubclassOf(typeof(CABankStatementAdjustment))))
                    {
                        current = item;
                        break;
                    }
                }
                if (current == null)
                {
                    current = adjustments.Current;
                }
                
                if (cache.Current == null) yield break;
                CABankStatementAdjustment row = current as CABankStatementAdjustment;
                string tranModule = (string)cache.GetValue(cache.Current, this._BatchModule.Name);
                switch (tranModule)
                {   
                    case GL.BatchModule.AP:            
                         foreach (PX.Objects.AP.APAdjust.APInvoice apInvoice in PXSelectJoin<PX.Objects.AP.APAdjust.APInvoice,
                                LeftJoin<APAdjust, On<APAdjust.adjdDocType, Equal<PX.Objects.AP.APAdjust.APInvoice.docType>, 
                                    And<APAdjust.adjdRefNbr, Equal<PX.Objects.AP.APAdjust.APInvoice.refNbr>, And<APAdjust.released, Equal<boolFalse>>>>,
                                LeftJoin<CABankStatementAdjustment, On<CABankStatementAdjustment.adjdDocType, Equal<PX.Objects.AP.APAdjust.APInvoice.docType>, 
                                 And<CABankStatementAdjustment.adjdRefNbr, Equal<PX.Objects.AP.APAdjust.APInvoice.refNbr>, And<CABankStatementAdjustment.released, Equal<boolFalse>,
                                        And<Where<CABankStatementAdjustment.refNbr,
                            NotEqual<Current<CABankStatementAdjustment.refNbr>>, Or<CABankStatementAdjustment.lineNbr, NotEqual<Current<CABankStatementAdjustment.lineNbr>>>>>
                                     >>>,
                                LeftJoin<APPayment, On<APPayment.docType, Equal<PX.Objects.AP.APAdjust.APInvoice.docType>,
                                    And<APPayment.refNbr, Equal<PX.Objects.AP.APAdjust.APInvoice.refNbr>, And<
                                    Where<APPayment.docType, Equal<APDocType.prepayment>, Or<APPayment.docType, Equal<APDocType.debitAdj>>>>>>>>>,
                                Where<PX.Objects.AP.APAdjust.APInvoice.vendorID, Equal<Optional<CAApplicationStatementDetail.payeeBAccountID>>, And<PX.Objects.AP.APAdjust.APInvoice.docType, Equal<Optional<CABankStatementAdjustment.adjdDocType>>,
                                And2<Where<PX.Objects.AP.APAdjust.APInvoice.released, Equal<True>, Or<PX.Objects.AP.APAdjust.APInvoice.prebooked, Equal<True>>>, And<PX.Objects.AP.APAdjust.APInvoice.openDoc, Equal<boolTrue>, And<CABankStatementAdjustment.adjdRefNbr, IsNull, And<APAdjust.adjgRefNbr, IsNull,
                                   And2<Where<APPayment.refNbr, IsNull, And<Current<CAApplicationStatementDetail.docType>, NotEqual<APDocType.refund>,
                                    Or<APPayment.refNbr, IsNotNull, And<Current<CAApplicationStatementDetail.docType>, Equal<APDocType.refund>,
                                    Or<APPayment.docType, Equal<APDocType.debitAdj>, And<Current<CAApplicationStatementDetail.docType>, Equal<APDocType.check>,
                                    Or<APPayment.docType, Equal<APDocType.debitAdj>, And<Current<CAApplicationStatementDetail.docType>, Equal<APDocType.voidCheck>>>>>>>>>,
                               And<PX.Objects.AP.APAdjust.APInvoice.docDate, LessEqual<Current<CAApplicationStatementDetail.tranDate>>>>>>>>>>>
                               .SelectMultiBound(this._Graph, new object[]{current}))
                        {
                            CABankStatementAdjustment adjustment = null;
                            foreach (CABankStatementAdjustment adj in adjustments.Inserted)
                            {
                                if ( (adj.AdjdDocType == apInvoice.DocType && adj.AdjdRefNbr == apInvoice.RefNbr && row != null && (adj.RefNbr!= row.RefNbr || adj.LineNbr!= row.LineNbr))
                                    || (row == null && adj.AdjdDocType == apInvoice.DocType && adj.AdjdRefNbr == apInvoice.RefNbr))
                                {
                                    adjustment = adj;
                                    break;
                                }
                            }
                            if (adjustment != null) continue;
                            GeneralInvoice gInvoice = new GeneralInvoice();
                            gInvoice.RefNbr = apInvoice.RefNbr;
                            gInvoice.OrigModule = apInvoice.OrigModule;
                            gInvoice.DocType = apInvoice.DocType;
                            gInvoice.DocDate = apInvoice.DocDate;
                            gInvoice.FinPeriodID = apInvoice.FinPeriodID;                            
                            gInvoice.LocationID = apInvoice.VendorLocationID;
                            gInvoice.CuryID = apInvoice.CuryID;
                            gInvoice.CuryOrigDocAmt = apInvoice.CuryOrigDocAmt;
                            gInvoice.CuryDocBal = apInvoice.CuryDocBal;
                            gInvoice.Status = apInvoice.Status;
                            gInvoice.DueDate = apInvoice.DueDate;
                            yield return gInvoice;
                        }

                        break;
                    case GL.BatchModule.AR:
                        foreach (ARInvoice arInvoice in PXSelectJoin<ARInvoice, 
                            LeftJoin<ARAdjust, On<ARAdjust.adjdDocType, Equal<ARInvoice.docType>, And<ARAdjust.adjdRefNbr, Equal<ARInvoice.refNbr>,
                            And<ARAdjust.released, Equal<boolFalse>, And<ARAdjust.voided, Equal<boolFalse>, And<Where<ARAdjust.adjgDocType, NotEqual<Current<CABankStatementAdjustment.adjdDocType>>>>>>>>,
                            LeftJoin<CABankStatementAdjustment, On<CABankStatementAdjustment.adjdDocType, Equal<ARInvoice.docType>, 
                            And<CABankStatementAdjustment.adjdRefNbr, Equal<ARInvoice.refNbr>, 
                            And<CABankStatementAdjustment.released, Equal<boolFalse>,
                                    And<Where<CABankStatementAdjustment.refNbr,
                            NotEqual<Current<CABankStatementAdjustment.refNbr>>, Or<CABankStatementAdjustment.lineNbr, NotEqual<Current<CABankStatementAdjustment.lineNbr>>>>>
                            >>>>>,
                            Where<ARInvoice.customerID, Equal<Current<CAApplicationStatementDetail.payeeBAccountID>>, 
                            And<ARInvoice.docType, Equal<Current<CABankStatementAdjustment.adjdDocType>>, 
                            And<ARInvoice.released, Equal<boolTrue>, 
                            And<ARInvoice.openDoc, Equal<boolTrue>, 
                            And<ARAdjust.adjgRefNbr, IsNull,
                            And<CABankStatementAdjustment.adjdRefNbr, IsNull,
                            And<ARInvoice.docDate, LessEqual<Current<CAApplicationStatementDetail.tranDate>>>
                            >>>>>>>.SelectMultiBound(this._Graph, new object[]{current}))
                        {
                            CABankStatementAdjustment adjustment = null;
                            foreach (CABankStatementAdjustment adj in adjustments.Inserted)
                            {
                                if (adj.AdjdDocType == arInvoice.DocType && adj.AdjdRefNbr == arInvoice.RefNbr && row != null && (adj.RefNbr != row.RefNbr || adj.LineNbr != row.LineNbr)
                                || (row == null && adj.AdjdDocType == arInvoice.DocType && adj.AdjdRefNbr == arInvoice.RefNbr)
                                    )
                                {
                                    adjustment = adj;
                                    break;
                                }
                            }
                            if (adjustment != null) continue;
                            GeneralInvoice gInvoice = new GeneralInvoice();
                            gInvoice.RefNbr = arInvoice.RefNbr;
                            gInvoice.OrigModule = arInvoice.OrigModule;
                            gInvoice.DocType = arInvoice.DocType;
                            gInvoice.DocDate = arInvoice.DocDate;
                            gInvoice.FinPeriodID = arInvoice.FinPeriodID;                            
                            gInvoice.LocationID = arInvoice.CustomerLocationID;
                            gInvoice.CuryID = arInvoice.CuryID;
                            gInvoice.CuryOrigDocAmt = arInvoice.CuryOrigDocAmt;
                            gInvoice.CuryDocBal = arInvoice.CuryDocBal;
                            gInvoice.Status = arInvoice.Status;
                            gInvoice.DueDate = arInvoice.DueDate;
                            yield return gInvoice;
                        }

                        break;
                }
            }
        }

        

        public class CurrencyInfoConditional : CurrencyInfoAttribute
        {
            Type _conditionField = null;
            public CurrencyInfoConditional(Type conditionField) : base() 
            {
                this._conditionField = conditionField;
            }

            public override void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
            {
                if (this._conditionField != null)
                {
                    Boolean? condition = sender.GetValue(e.Row, this._conditionField.Name) as Boolean?;
                    if (condition ?? false)
                        base.RowInserting(sender, e);
                }
                else
                    base.RowInserting(sender, e);
            }
            public override void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
            {
                if (this._conditionField != null)
                {
                    Boolean? condition = sender.GetValue(e.Row, this._conditionField.Name) as Boolean?;
                    if (condition ?? false)
                        base.RowUpdating(sender, e);
                }
                else
                    base.RowUpdating(sender, e);
            }
        }
		#endregion

		#region Buttons Definition		
		

		#region Button ViewDoc
		public PXAction<CABankStatement> viewDoc;
		[PXUIField(DisplayName = Messages.ViewDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewDoc(PXAdapter adapter)
		{
			CATran tran = this.DetailMatches.Current;
			if(tran != null)
		    {
				CATran.Redirect(this.DetailMatches.Cache, tran);
			}
			return adapter.Get();
		}
		#endregion

		#region Button ViewDetailsDoc
		public PXAction<CABankStatement> viewDetailsDoc;
		[PXUIField(DisplayName = Messages.ViewDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewDetailsDoc(PXAdapter adapter)
		{
			CABankStatementDetail row = this.Details.Current;
			if (row != null && row.CATranID!=null)
			{
				CATran tran = PXSelect<CATran, Where<CATran.cashAccountID, Equal<Required<CATran.cashAccountID>>,
								And<CATran.tranID, Equal<Required<CATran.tranID>>>>>.Select(this, row.CashAccountID, row.CATranID);
				CATran.Redirect(this.DetailMatches.Cache, tran);
			}
			return adapter.Get();
		}
		#endregion
		#region Button Upload File
		public PXSelect<CABankStatement> NewRevisionPanel;
		public PXAction<CABankStatement> uploadFile;
		[PXUIField(DisplayName = Messages.UploadFile, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]		
		[PXButton()]
		public virtual IEnumerable UploadFile(PXAdapter adapter)
		{
            bool doImport = true;
			if (this.casetup.Current.ImportToSingleAccount == true)
			{
				CABankStatement row = this.BankStatement.Current;
				if (row == null || this.BankStatement.Current.CashAccountID == null)
				{
					throw new PXException(Messages.CashAccountMustBeSelectedToImportStatement);
				}
				else
				{
					CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, row.CashAccountID);
					if (acct != null && string.IsNullOrEmpty(acct.StatementImportTypeName))
					{
						throw new PXException(Messages.StatementImportServiceMustBeConfiguredForTheCashAccount);
					}
				}
			}
			else 
			{
				if (string.IsNullOrEmpty(this.casetup.Current.StatementImportTypeName))
				{
					throw new PXException(Messages.StatementImportServiceMustBeConfiguredInTheCASetup);
				}
			}

            if (this.BankStatement.Current != null && this.IsDirty == true)
            {
                if (this.casetup.Current.ImportToSingleAccount != true)
                {
                    if (this.BankStatement.Ask(Messages.ImportConfirmationTitle, Messages.UnsavedDataInThisScreenWillBeLostConfirmation, MessageButtons.YesNo) != WebDialogResult.Yes)
                    {
                        doImport = false;
                    }
                }
                else 
                {
                    doImport = true;
                }
            }

			if (doImport)
			{
				if (this.NewRevisionPanel.AskExt() == WebDialogResult.OK) 
				{
					const string PanelSessionKey = "ImportStatementFile";
					PX.SM.FileInfo info = PXContext.SessionTyped<PXSessionStatePXData>().FileInfo[PanelSessionKey] as PX.SM.FileInfo;
					System.Web.HttpContext.Current.Session.Remove(PanelSessionKey);
					this.ImportStatement(info);
					CABankStatement newRow = this.BankStatement.Current;
					List<CABankStatement> result = new List<CABankStatement>();
					result.Add(newRow);
					return result;
				}				
			}
			return adapter.Get();
		}

		#endregion

		#region Import
		public PXAction<CABankStatement> import;
		[PXUIField(DisplayName = Messages.ImportStatement, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
		[PXProcessButton]
		public virtual IEnumerable Import(PXAdapter adapter)
		{
			if (PXLongOperation.Exists(UID))
			{
				throw new ApplicationException(GL.Messages.PrevOperationNotCompleteYet);
			}

			CABankStatement doc = this.BankStatement.Current;
			if (doc != null && doc.Released == true && doc.Hold == false)
			{
				string importScenatioName = "Import Bank Statement MOM";
				SYMapping map = PXSelect<SYMapping, Where<SYMapping.name, Equal<Required<SYMapping.name>>>>.Select(this, importScenatioName);
				
				/*if (map!= null && map.MappingID!= null)
				{
					string defaultFileName = 
					PXLongOperation.StartOperation(this, delegate()
					{

						PX.Api.SYImportProcess.RunScenario(map.Name,
							new PX.Api.PXSYParameter(ExportProviderParams.FileName, defaultFileName),
							new PX.Api.PXSYParameter(ExportProviderParams.BatchNbr, doc.BatchNbr)
							//,
							//new PX.Api.PXSYParameter(ExportProviderParams.BatchSequenceStartingNbr, "0000")
							);
					});
				}
				else
				{
					throw new PXException(Messages.CABatchExportProviderIsNotConfigured);
				}*/
			}

			return adapter.Get();
		}
		
		#endregion
		#region AutoMatch
		public PXAction<CABankStatement> autoMatch;
		[PXUIField(DisplayName = "Auto Match", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		public virtual IEnumerable AutoMatch(PXAdapter adapter)
		{
			DoMatch();			
			return adapter.Get();
		}
		#endregion
		
		#region MatchTrans
		public PXAction<CABankStatement> matchTrans;

		[PXUIField(DisplayName = Messages.MatchSelected, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable MatchTrans(PXAdapter adapter)
		{
			CATranExt tran = this.DetailMatches.Current;
			CABankStatementDetail source = this.Details.Current;
			if (tran != null && source!=null)
			{
				
				if (source.CATranID.HasValue && source.CATranID != tran.TranID) 
				{
					throw new PXException(Messages.StatementDetailIsAlreadyMatched); 
				}
				else
				{
					source.CATranID = tran.TranID;
					source.DocumentMatched = true;
					this.Details.Update(source);
				}
			}
			return adapter.Get();
		}
		#endregion
		#region Clear Match
		public PXAction<CABankStatement> clearMatch;

		[PXUIField(DisplayName = Messages.ClearMatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXLookupButton]
		public virtual IEnumerable ClearMatch(PXAdapter adapter)
		{
			CABankStatementDetail source = this.Details.Current;
			if (source!=null)
			{
				source.CATranID = null;
				source.DocumentMatched = false;
				this.Details.Update(source);
			}
			return adapter.Get();
		}
		#endregion

		#region Clear All Matches
		public PXAction<CABankStatement> clearAllMatches;

		[PXUIField(DisplayName = "Clear All", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXProcessButton]
		public virtual IEnumerable ClearAllMatches(PXAdapter adapter)
		{ 
			foreach (CABankStatementDetail it in this.Details.Select())
			{
				if (it != null && it.CATranID!= null)
				{
					it.CATranID = null;
					it.DocumentMatched = false;
					this.Details.Update(it);
				}
			}
			return adapter.Get();
		}
		#endregion

		#region CreateAllDocuments
		public PXAction<CABankStatement> createAllDocuments;

		[PXUIField(DisplayName = Messages.CreateAllDocuments, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXProcessButton]
		public virtual IEnumerable CreateAllDocuments(PXAdapter adapter)
		{
			CABankStatement source = this.BankStatement.Current;
			if (source != null && source.Released == false)
			{
				foreach (CABankStatementDetail iDet in this.Details.Select()) 
				{
					if (iDet.CreateDocument == true && iDet.CATranID == null) 
					{
						CreateDocumentProc(iDet,false);
					}
				}
				this.Save.Press();
			}
			return adapter.Get();
		}
		#endregion
		#region CreateDocument
		public PXAction<CABankStatement> createDocument;

		[PXUIField(DisplayName =Messages.CreateDocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXProcessButton]
		public virtual IEnumerable CreateDocument(PXAdapter adapter)
		{
			CABankStatementDetail source = this.Details.Current;
			CABankStatement doc = this.BankStatement.Current;
			if (source != null 
				&& source.CATranID == null && (source.CreateDocument == true))
			{
				source.CashAccountID = doc.CashAccountID;
				this.CreateDocumentProc(source,false);
			}
			return adapter.Get();
		}
		#endregion

		#region Release
		public PXAction<CABankStatement> release;

		[PXUIField(DisplayName = Messages.Release, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXProcessButton]
		public virtual IEnumerable Release(PXAdapter adapter)
		{
			
			PXCache cache = this.BankStatement.Cache;
			List<CABankStatement> list = new List<CABankStatement>();
			foreach (CABankStatement doc in adapter.Get<CABankStatement>())
			{
				if (doc.CuryReconciledBalanceDiff != Decimal.Zero || doc.CuryDetailsBalanceDiff != Decimal.Zero
					|| doc.CuryCreditsTotal != doc.CuryReconciledCredits || doc.CuryDebitsTotal != doc.CuryReconciledDebits
					|| doc.CountCredit != doc.ReconciledCountCredit || doc.CountDebit != doc.ReconciledCountDebit)
				{
					
					throw new PXException(Messages.DocumentIsUnbalancedItCanNotBeReleased);
				}
				if (!(bool)doc.Hold)
				{
					cache.Update(doc);
					list.Add(doc);						
				}				
			}
			if (list.Count == 0)
			{
				throw new PXException(Messages.DocumentOnHoldCanNotBeReleased);
			}
			this.Save.Press();
            PXLongOperation.ClearStatus(this.UID);
			PXLongOperation.StartOperation(this, delegate() { CABankStatementEntry.ReleaseDoc(list); });
			return list;			
		}
		#endregion
		#region MatchSettingsPanel
		public PXAction<CABankStatement> matchSettingsPanel;

		[PXUIField(DisplayName = Messages.MatchSettings, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public virtual IEnumerable MatchSettingsPanel(PXAdapter adapter)
		{
			return adapter.Get();
		}
		#endregion

		#region ValidateMatches
		public PXAction<CABankStatement> validateMatches;
		[PXUIField(DisplayName = "Validate Matches", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
		[PXProcessButton()]
		public virtual IEnumerable ValidateMatches(PXAdapter adapter)
		{
			CABankStatement doc = this.BankStatement.Current;
			if( doc!= null && doc.Released == false)
			{
				PXCache cache = this.Details.Cache;
				PXCache matchesCache = this.DetailMatches.Cache;
				int problemsCount = 0;
				foreach (CABankStatementDetail iDet in this.Details.Select())
				{
					if (iDet.CATranID.HasValue) 
					{
						CATranExt tran = PXSelect<CATranExt, Where<CATranExt.tranID, Equal<Required<CATranExt.tranID>>>>.Select(this, iDet.CATranID);
						if (tran != null)
						{
							Decimal res = CompareRefNbr(iDet, tran, false);
							if (res < Decimal.One)
							{
								problemsCount++;
								cache.RaiseExceptionHandling<CABankStatementDetail.extRefNbr>(iDet, iDet.ExtRefNbr, new PXSetPropertyException( Messages.RowMatchesCATranWithDifferentExtRefNbr, PXErrorLevel.RowWarning));
								matchesCache.RaiseExceptionHandling<CATranExt.extRefNbr>(tran, iDet.ExtRefNbr, new PXSetPropertyException(Messages.RowMatchesCATranWithDifferentExtRefNbr, PXErrorLevel.RowWarning));
							}
							else 
							{
								cache.RaiseExceptionHandling<CABankStatementDetail.extRefNbr>(iDet, iDet.ExtRefNbr, null);
							}
						}
						else 
						{
							cache.RaiseExceptionHandling<CABankStatementDetail.documentMatched>(iDet, true, new PXSetPropertyException(Messages.MatchingCATransIsNotValid, PXErrorLevel.RowError));
						}
					}
				}
				if (problemsCount > 0)
					throw new PXException(Messages.RowsWithSuspiciousMatchingWereDetected, problemsCount);
			}
			return adapter.Get();
		}
		#endregion
		#endregion

		#region Ctor + Selects
		//public ToggleCurrency<CABankStatement> CurrencyView;

		public PXSelect<CABankStatement, Where<CABankStatement.cashAccountID, Equal<Optional<CABankStatement.cashAccountID>>>,OrderBy<Asc<CABankStatement.cashAccountID, Desc<CABankStatement.endBalanceDate>>>> BankStatement;

        public PXSelectJoin<CABankStatementDetail,
                        LeftJoin<CATran, On<CATran.cashAccountID, Equal<CABankStatementDetail.cashAccountID>,
                            And<CATran.tranID, Equal<CABankStatementDetail.cATranID>>>>,
                            Where<CABankStatementDetail.refNbr, Equal<Current<CABankStatement.refNbr>>>,
                                OrderBy<Asc<CABankStatementDetail.tranDate>>> PaymentData;

        [PXCopyPasteHiddenView]        
        public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>> CurrencyInfo_CuryInfoID;
        public PXSetup<Customer, Where<Customer.bAccountID, Equal<Optional<CAApplicationStatementDetail.payeeBAccountID>>>> customer;
        public PXSelect<ARInvoice, Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>, And<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>> ARInvoice_CustomerID_DocType_RefNbr;

        [PXCopyPasteHiddenView]
        public PXSelectJoin<CAApplicationStatementDetail,
                        LeftJoin<CATran, On<CATran.cashAccountID, Equal<CAApplicationStatementDetail.cashAccountID>,
                            And<CATran.tranID, Equal<CAApplicationStatementDetail.cATranID>>>>,
                            Where<CAApplicationStatementDetail.refNbr, Equal<Current<CABankStatement.refNbr>>,
                                And<CAApplicationStatementDetail.createDocument,Equal<True>>>,
                                OrderBy<Asc<CAApplicationStatementDetail.tranDate>>> ApplicationDetails;

        public PXSelect<CABankStatementAdjustment, Where<CABankStatementAdjustment.refNbr, Equal<Current<CABankStatement.refNbr>>,
            And<CABankStatementAdjustment.lineNbr,
            Equal<Current<CAApplicationStatementDetail.lineNbr>>>>> Adjustments;

		[PXFilterable]
		[PXImport(typeof(CABankStatement))]
		public PXSelectJoin<CABankStatementDetail, 
						LeftJoin<CATran,On<CATran.cashAccountID,Equal<CABankStatementDetail.cashAccountID>,
							And<CATran.tranID, Equal<CABankStatementDetail.cATranID>>>>,
							Where<CABankStatementDetail.refNbr,Equal<Current<CABankStatement.refNbr>>>,
								OrderBy<Asc<CABankStatementDetail.tranDate>>> Details;
 
		public PXSelectJoin<CATranExt, LeftJoin<BAccountR, On<CATranExt.referenceID, Equal<BAccountR.bAccountID>>>, 
					Where<CATranExt.tranID, Equal<Optional<CABankStatementDetail.cATranID>>>> DetailMatches;
		
		//public PXSelect<CurrencyInfo> currencyinfo;
        public PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Optional<CABankStatementDetail.curyInfoID>>>> currencyinfo;
		//public PXSetup<CashAccount, Where<CashAccount.reconcile, Equal<boolTrue>>> cashaccount;
        public PXSetup<CashAccount, Where<CashAccount.accountID, Equal<Optional<CABankStatement.cashAccountID>>>> cashaccount;
		public PXSetup<CASetup> casetup;
		public CMSetupSelect CMSetup;
		public PXFilter<MatchSettings> matchSettings;

		public PXSelectReadonly<CashAccount, Where<CashAccount.extRefNbr, Equal<Optional<CashAccount.extRefNbr>>>> cashAccountByExtRef;
		public PXSelect<APPayment, Where<APPayment.docType,IsNull>> apPayment;
		public PXSelect<ARPayment, Where<ARPayment.docType, IsNull>> arPayment;
		public PXSelect<CADeposit, Where<CADeposit.tranType,IsNull>> caDeposit;
		public PXSelect<CAAdj, Where<CAAdj.adjRefNbr,IsNull>> caAdjustment;
		public PXSelect<CATransfer, Where<CATransfer.transferNbr, IsNull>> caTransfer;
		
		public CABankStatementEntry()
		{
			CASetup setup = casetup.Current; 

			PXCache cache = this.BankStatement.Cache;
			PXDBCurrencyAttribute.SetBaseCalc<CABankStatement.curyReconciledBalanceDiff>(cache, null, false);
			PXDBCurrencyAttribute.SetBaseCalc<CABankStatement.curyReconciledCredits>(cache, null, false);
			PXDBCurrencyAttribute.SetBaseCalc<CABankStatement.curyReconciledDebits>(cache, null, false);
			//PXDBCurrencyAttribute.SetBaseCalc<CABankStatement.curyReconciledTurnover>(reconCache, null, false);
			PXUIFieldAttribute.SetVisible<CABankStatement.curyID>(cache, null, (bool)CMSetup.Current.MCActivated);

			PXCache detailCache = this.Details.Cache;
			PXUIFieldAttribute.SetVisible<CABankStatementDetail.tranEntryDate>(detailCache, null, false);
			PXUIFieldAttribute.SetVisible<CABankStatementDetail.extTranID>(detailCache, null, false);
			PXUIFieldAttribute.SetVisible<CABankStatementDetail.curyID>(detailCache, null, false);
			bool showAmountAsDrCr = true;
			PXUIFieldAttribute.SetVisible<CABankStatementDetail.drCr>(detailCache, null, !showAmountAsDrCr);
			PXUIFieldAttribute.SetVisible<CABankStatementDetail.curyTranAmt>(detailCache, null, !showAmountAsDrCr);

			PXUIFieldAttribute.SetVisible<CABankStatementDetail.curyDebitAmt>(detailCache, null, showAmountAsDrCr);
			PXUIFieldAttribute.SetVisible<CABankStatementDetail.curyCreditAmt>(detailCache, null, showAmountAsDrCr);
			PXUIFieldAttribute.SetVisible<CABankStatementDetail.payeeMatched>(detailCache, null, false);
			PXUIFieldAttribute.SetVisible<CABankStatementDetail.documentMatched>(detailCache, null, true);

			this.DetailMatches.Cache.AllowInsert = false;
			this.DetailMatches.Cache.AllowDelete = false;
			this.DetailMatches.Cache.AllowUpdate = false;

		}		
        #endregion


		#region Implement

		public virtual IEnumerable detailMatches()
		{
			CABankStatementDetail detail = this.Details.Current;
			if (detail == null) return null;
			List<CATranExt> res = new List<CATranExt>(1);
			if (detail.CATranID == null)
			{
				return this.FindDetailMatches(detail, this.matchSettings.Current, false);
			}
			else
			{
				PXResult<CATranExt,BAccountR> tranRes =(PXResult<CATranExt,BAccountR>) PXSelectReadonly2<CATranExt, LeftJoin<BAccountR, On<BAccountR.bAccountID,
														Equal<CATranExt.referenceID>>>, Where<CATranExt.tranID, Equal<Required<CATranExt.tranID>>>>.Select(this, detail.CATranID);
				CATranExt tran = tranRes;
				BAccount payee = tranRes;
				if (tran != null)
				{
					if (payee != null && payee.BAccountID != null)
						tran.ReferenceName = payee.AcctName;
					tran.MatchRelevance = EvaluateMatching(detail, tran, this.matchSettings.Current);
					tran.IsMatched = true;
					res.Add(tran);
				}
				else 
				{
					//this.Details.Cache.RaiseExceptionHandling<CABankStatementDetail.extRefNbr>(detail,detail.ExtRefNbr, new PXSetPropertyException("This line is matched to deleted Document",PXErrorLevel.Warning));
				}
			}
			return res;
		}
	
		#endregion

		#region Functions
		public virtual void DoMatch()
		{
			PXCache cache = this.Details.Cache;
			DoMatch(cache, this.Details.Select().RowCast<CABankStatementDetail>());
		} 

		public virtual void DoMatch(PXCache aUpdateCache, IEnumerable aRows)
		{
			Dictionary<long, List<CABankStmtDetailDocRef>> cross = new Dictionary<long, List<CABankStmtDetailDocRef>>();
			Dictionary<int, CABankStatementDetail> rows = new Dictionary<int, CABankStatementDetail>();
			int rowCount= 0;
			IMatchSettings setting = this.matchSettings.Current;
			const int daysTreshold = 30;
			bool useInternalCache = false;
			DateTime? minDate = null;
			DateTime? maxDate = null;

			if (((setting.DisbursementTranDaysAfter ?? 0) + (setting.DisbursementTranDaysBefore ?? 0)) > daysTreshold
				|| ((setting.ReceiptTranDaysAfter ?? 0) + (setting.ReceiptTranDaysBefore ?? 0)) > daysTreshold)
			{
				useInternalCache = true;
				foreach (object en in aRows)
				{					
					CABankStatementDetail iDet = en as CABankStatementDetail;
					if (setting.MatchInSelection == true && iDet.Selected != true) continue; 
					if (!minDate.HasValue || minDate.Value > iDet.TranDate.Value)
						minDate = iDet.TranDate;
					if (!maxDate.HasValue || maxDate.Value < iDet.TranDate.Value)
						maxDate = iDet.TranDate;
				}
			}
			foreach (object en in aRows) 
			{
				rowCount++;
				CABankStatementDetail iDet = en as CABankStatementDetail;
				if (iDet.CATranID.HasValue) continue; //Skip already matched records
				if (setting.MatchInSelection == true && iDet.Selected != true) continue; 
				if (!rows.ContainsKey(iDet.LineNbr.Value)) 
				{
					rows.Add(iDet.LineNbr.Value, iDet);
				}
				List<CATranExt> list= null;
				if(useInternalCache)
					list = (List<CATranExt>)this.FindDetailMatches(iDet, this.matchSettings.Current, minDate.Value, maxDate.Value);
				else
					list = (List<CATranExt>) this.FindDetailMatches(iDet,this.matchSettings.Current, true);
				int count = 0;
				CATranExt[] bestMatches = { null, null };				
				foreach (CATranExt iTran in list) 
				{
					if (bestMatches[0] == null || bestMatches[0].MatchRelevance < iTran.MatchRelevance)
					{						 
						bestMatches[1] = bestMatches[0];
						bestMatches[0] = iTran;
					}
					else 
					{
						if (bestMatches[1] == null || bestMatches[1].MatchRelevance < iTran.MatchRelevance)
							bestMatches[1] = iTran;
					}
					count++;
 				}
				if (bestMatches[0] != null 
					&& (( bestMatches[1] == null || (bestMatches[0].MatchRelevance - bestMatches[1].MatchRelevance) > 0.2m)|| bestMatches[0].MatchRelevance >0.75m)) 
				{
					CATranExt matchCandidate = bestMatches[0];
					CABankStmtDetailDocRef xref = new CABankStmtDetailDocRef();
					xref.Copy(iDet);
					xref.Copy(matchCandidate);
					xref.MatchRelevance = matchCandidate.MatchRelevance;
					if (cross.ContainsKey(matchCandidate.TranID.Value) == false)
					{
						cross.Add(matchCandidate.TranID.Value, new List<CABankStmtDetailDocRef>()); 
					}
					cross[matchCandidate.TranID.Value].Add(xref);					
				}
			}

			Dictionary<Int32, CABankStatementDetail> spareDetails = new Dictionary<Int32, CABankStatementDetail>();
			int updateCount = 0;
			int totalCount = 0;
			foreach(KeyValuePair<long,List<CABankStmtDetailDocRef>> iCandidate in cross)
			{ 
				CABankStmtDetailDocRef bestMatch = null;
				updateCount++;
				foreach(CABankStmtDetailDocRef iRef in iCandidate.Value)
				{
					totalCount++;
					if (bestMatch == null || bestMatch.MatchRelevance < iRef.MatchRelevance) 
					{
						bestMatch = iRef;
					} 
				}
				if (bestMatch != null && bestMatch.LineNbr!= null)  
				{
					CABankStatementDetail detail;
					if (!rows.TryGetValue(bestMatch.LineNbr.Value, out detail))
					{
						detail = this.Details.Search<CABankStatementDetail.lineNbr>(bestMatch.LineNbr.Value);
						rows.Add(detail.LineNbr.Value, detail);
					}
					 
					if (detail != null && detail.CATranID == null) 
					{
						detail.CATranID = bestMatch.CATranID;
						detail.DocumentMatched = true;
						aUpdateCache.Update(detail);
						//Clear CA transaction from the storage dictionary - it's not available anymore.
						if (this.caTransDict != null) 
						{
							List<CATranExt> storage;
							if (this.caTransDict.TryGetValue(detail.CuryTranAmt.Value, out storage) && storage!=null)
							{
								storage.RemoveAll(i=>(i.TranID == bestMatch.CATranID));
							}
						}
					}
					spareDetails.Remove(bestMatch.LineNbr.Value);
					foreach (CABankStmtDetailDocRef iMatch in iCandidate.Value) 
					{
						if (Object.ReferenceEquals(iMatch, bestMatch)) continue;
						spareDetails[iMatch.LineNbr.Value] = null;
					}
				}				
			}
			cross.Clear();
            List<CABankStatementDetail> spareDetails1 = new List<CABankStatementDetail>(spareDetails.Keys.Count);            
			foreach (KeyValuePair<Int32,CABankStatementDetail> iDet in spareDetails) 
			{
				CABankStatementDetail detail;
				if (!rows.TryGetValue(iDet.Key, out detail))
				{
					detail = this.Details.Search<CABankStatementDetail.lineNbr>(iDet.Key);
					rows.Add(detail.LineNbr.Value, detail);
				}
                //CABankStatementDetail detail = this.Details.Search<CABankStatementDetail.lineNbr>(iDet.Key);
                if(detail!=null)
                    spareDetails1.Add(detail);
			}
			if(spareDetails1.Count >0)
				DoMatch(aUpdateCache, spareDetails1);
		}

		protected static Pair<DateTime, DateTime> GetDateRangeForMatch(CABankStatementDetail aDetail, IMatchSettings aSettings) 
		{
			DateTime tranDateStart = aDetail.TranEntryDate ?? aDetail.TranDate.Value;
			DateTime tranDateEnd = aDetail.TranEntryDate ?? aDetail.TranDate.Value;
			bool isReceipt = (aDetail.DrCr == CADrCr.CADebit);
			tranDateStart = tranDateStart.AddDays(-(isReceipt ? aSettings.ReceiptTranDaysBefore.Value : aSettings.DisbursementTranDaysBefore.Value));
			tranDateEnd = tranDateEnd.AddDays((isReceipt ? aSettings.ReceiptTranDaysAfter.Value : aSettings.DisbursementTranDaysAfter.Value));
			if (tranDateEnd < tranDateStart)
			{
				DateTime swap = tranDateStart;
				tranDateStart = tranDateEnd;
				tranDateEnd = swap;
			}			
			return new Pair<DateTime,DateTime>(tranDateStart,tranDateEnd);
		}

		public virtual IEnumerable FindDetailMatches(CABankStatementDetail aDetail, IMatchSettings aSettings, bool skipLowRelevance)
		{
            List<CATranExt> matchList = new List<CATranExt>();
			bool hasBaccount = aDetail.PayeeBAccountID.HasValue;
			bool hasLocation = aDetail.PayeeLocationID.HasValue;
            if (!aDetail.TranEntryDate.HasValue && !aDetail.TranDate.HasValue) return matchList;
			Pair<DateTime,DateTime> tranDateRange = GetDateRangeForMatch(aDetail, aSettings);
			const decimal relevanceTreshhold = 0.2m;						
			string CuryID = aDetail.CuryID; //Need to reconsider.            
            foreach (PXResult<CATranExt, BAccountR, CABankStatementDetail, CATran2> iRes in PXSelectReadonly2<CATranExt, LeftJoin<BAccountR, On<BAccountR.bAccountID,
                                                        Equal<CATranExt.referenceID>>,
                                                        LeftJoin<CABankStatementDetail, On<CABankStatementDetail.cATranID, Equal<CATran.tranID>>,
                                                        LeftJoin<CATran2, On<CATran2.cashAccountID, Equal<CATran.cashAccountID>,
                                                                    And<CATran2.voidedTranID, Equal<CATran.tranID>>>>>>,
                                                        Where<CATranExt.cashAccountID, Equal<Current<CABankStatement.cashAccountID>>,
                                                            And2<Where<CABankStatementDetail.refNbr, IsNull,
                                                                Or<CABankStatementDetail.refNbr, Equal<Current<CABankStatement.refNbr>>>>,
                                                            And<CATranExt.tranDate, Between<Required<CATran.tranDate>, Required<CATran.tranDate>>,
                                                            And<CATranExt.curyID, Equal<Required<CATran.curyID>>,
                                                            And<CATranExt.curyTranAmt, Equal<Required<CATran.curyTranAmt>>,
                                                            And<CATranExt.drCr, Equal<Required<CATran.drCr>>>>>>>>>.
                                                        Select(this, tranDateRange.first, tranDateRange.second,
                                                        CuryID, aDetail.CuryTranAmt.Value, aDetail.DrCr))
            
            {
				CATranExt iTran = iRes;
				BAccount iPayee = iRes;
                CATran2 iVoidTran = iRes;
                
                if (aSettings.SkipVoided == true && (iTran.VoidedTranID.HasValue 
                    || (iVoidTran!=null && iVoidTran.TranID.HasValue)))
                {
                    continue;   
                }

				if (hasBaccount && iTran.ReferenceID != aDetail.PayeeBAccountID) continue;
				iTran.ReferenceName = iPayee.AcctName;
				CABankStatementDetail iLinkedDetail = iRes;
				//Check updated in cache
				CABankStatementDetail sourceRow = null;
				bool hasLinkedDetail = (iLinkedDetail.LineNbr != null 
										&& (iLinkedDetail.RefNbr != aDetail.RefNbr || iLinkedDetail.LineNbr!= aDetail.LineNbr));
				bool linkCleared = !hasLinkedDetail;
				if (hasLinkedDetail) 
				{
					foreach (CABankStatementDetail iDetail in this.Details.Cache.Deleted) 
					{
						if (iDetail.CashAccountID == iLinkedDetail.CashAccountID && iDetail.RefNbr == iLinkedDetail.RefNbr &&
							iDetail.LineNbr == iLinkedDetail.LineNbr)
						{
							linkCleared = true;
							break;
						}
					}
				}

				foreach (CABankStatementDetail iDetail in this.Details.Cache.Inserted)
				{
					if (iDetail.CATranID == iTran.TranID && (iDetail.RefNbr != aDetail.RefNbr || iDetail.LineNbr != aDetail.LineNbr))
					{
						sourceRow = iDetail;
						break;
					}
				}

				if (sourceRow != null) continue;
				foreach (CABankStatementDetail iDetail in this.Details.Cache.Updated)
				{
					if (iDetail.CATranID == iTran.TranID && (iDetail.RefNbr !=aDetail.RefNbr || iDetail.LineNbr!= aDetail.LineNbr))
					{
						sourceRow = iDetail;
						if(hasLinkedDetail == false || linkCleared == true)
							break;
					}
					if (hasLinkedDetail && !linkCleared) 
					{
						if(iDetail.RefNbr == iLinkedDetail.RefNbr &&
							iDetail.LineNbr == iLinkedDetail.LineNbr)
						{
							if (iDetail.CATranID == null || iDetail.CATranID != iTran.TranID) 
							{
								linkCleared = true;
								if (sourceRow != null)
									break;
							}
						}
					}					
				}
				if (sourceRow != null || (hasLinkedDetail && !linkCleared)) continue;				
				iTran.MatchRelevance = EvaluateMatching(aDetail, iTran, aSettings);
				iTran.IsMatched = (iTran.TranID == aDetail.CATranID);
				if (skipLowRelevance && (iTran.IsMatched == false && iTran.MatchRelevance < relevanceTreshhold))
					continue;
				matchList.Add(iTran);
			}
			return matchList;
		}

		public virtual IEnumerable FindDetailMatches(CABankStatementDetail aDetail, IMatchSettings aSettings, DateTime minDate, DateTime maxDate)
		{
            List<CATranExt> matchList = new List<CATranExt>();
			bool hasBaccount = aDetail.PayeeBAccountID.HasValue;
			bool hasLocation = aDetail.PayeeLocationID.HasValue;			
            if (!aDetail.TranEntryDate.HasValue && !aDetail.TranDate.HasValue) return matchList;			
			Pair<DateTime, DateTime> tranDateRange = GetDateRangeForMatch(aDetail, aSettings);			
			const decimal relevanceTreshhold = 0.2m;
			if(this.caTransDict == null)
				this.LoadTranDictionary(aSettings, minDate, maxDate);

			List<CATranExt> storage;
			if (!this.caTransDict.TryGetValue(aDetail.CuryTranAmt.Value, out storage))
			{
				return matchList;
			}
			
			string CuryID = aDetail.CuryID; //Need to reconsider.			
			foreach (CATranExt iRes in storage)
			{
				CATranExt iTran = iRes;				
				if (hasBaccount && iTran.ReferenceID != aDetail.PayeeBAccountID) continue;
				if (iTran.TranDate < tranDateRange.first || iTran.TranDate > tranDateRange.second) continue;
				iTran.MatchRelevance = EvaluateMatching(aDetail, iTran, aSettings);
				iTran.IsMatched = (iTran.TranID == aDetail.CATranID);
				if ((iTran.IsMatched == false && iTran.MatchRelevance < relevanceTreshhold))
					continue;				
				matchList.Add(iTran);
			}
			return matchList;
		}

		protected Dictionary<Decimal,List<CATranExt>> caTransDict = null;

		protected virtual void LoadTranDictionary(IMatchSettings aSettings, DateTime minDate, DateTime maxDate) 
		{
			CABankStatement stmt = this.BankStatement.Current;
			if (stmt == null) return;
			this.caTransDict = new Dictionary<decimal, List<CATranExt>>();
			DateTime disbDateFrom = minDate.AddDays(-(aSettings.DisbursementTranDaysBefore ?? 0));
			DateTime disbDateto = maxDate.AddDays((aSettings.DisbursementTranDaysAfter ?? 0));
			DateTime receiptDateFrom = minDate.AddDays(-(aSettings.ReceiptTranDaysBefore ?? 0));
			DateTime receiptDateto = maxDate.AddDays((aSettings.ReceiptTranDaysAfter ?? 0));

			foreach (PXResult<CATranExt, BAccountR, CABankStatementDetail> iRes in PXSelectReadonly2<CATranExt, LeftJoin<BAccountR, On<BAccountR.bAccountID,
														Equal<CATranExt.referenceID>>,
														LeftJoin<CABankStatementDetail, On<CABankStatementDetail.cATranID,Equal<CATran.tranID>>>>,
														Where<CATranExt.cashAccountID, Equal<Current<CABankStatement.cashAccountID>>,
															And<CABankStatementDetail.refNbr,IsNull,
															And<CATranExt.tranDate, Between<Required<CATran.tranDate>, Required<CATran.tranDate>>,
															And<CATranExt.drCr, Equal<Required<CATran.drCr>>>>>>>.
															Select(this, disbDateFrom, disbDateto, CADrCr.CACredit))
			{
				CATranExt iTran = iRes;
				BAccountR iBaccount = iRes;
				List<CATranExt> storage;
				if (!this.caTransDict.TryGetValue(iTran.CuryTranAmt.Value, out storage)) 
				{
					storage = new List<CATranExt>();
					this.caTransDict.Add(iTran.CuryTranAmt.Value, storage);
				}
				CABankStatementDetail sourceRow= null;
				//Need additional search for rows updated in the current graph
				foreach (CABankStatementDetail iDetail in this.Details.Cache.Updated)
				{
					if (iDetail.CATranID == iTran.TranID)
					{
						sourceRow = iDetail;
						break;
					}
				}
				if (sourceRow != null) continue;
				iTran.ReferenceName = iBaccount.AcctName;
				storage.Add(iTran);
			}

			foreach (PXResult<CATranExt, BAccountR, CABankStatementDetail> iRes in PXSelectReadonly2<CATranExt, LeftJoin<BAccountR, On<BAccountR.bAccountID,
														Equal<CATranExt.referenceID>>,
														LeftJoin<CABankStatementDetail, On<CABankStatementDetail.cATranID, Equal<CATran.tranID>>>>,
														Where<CATranExt.cashAccountID, Equal<Current<CABankStatement.cashAccountID>>,
															And<CABankStatementDetail.refNbr, IsNull,
															And<CATranExt.tranDate, Between<Required<CATran.tranDate>, Required<CATran.tranDate>>,
															And<CATranExt.drCr, Equal<Required<CATran.drCr>>>>>>>.
															Select(this, receiptDateFrom, receiptDateto, CADrCr.CADebit))
			{
				CATranExt iTran = iRes;
				BAccountR iBaccount = iRes;
				List<CATranExt> storage;
				if (!this.caTransDict.TryGetValue(iTran.CuryTranAmt.Value, out storage))
				{
					storage = new List<CATranExt>();
					this.caTransDict.Add(iTran.CuryTranAmt.Value, storage);
				}
				//Need additional search for rows updated in the current graph
				CABankStatementDetail sourceRow = null;
				foreach (CABankStatementDetail iDetail in this.Details.Cache.Updated)
				{
					if (iDetail.CATranID == iTran.TranID)
					{
						sourceRow = iDetail;
						break;
					}
				}
				if (sourceRow != null) continue;
				iTran.ReferenceName = iBaccount.AcctName;
				storage.Add(iTran);
			}
		}

		protected virtual decimal EvaluateMatching(CABankStatementDetail aDetail, CATran aTran, IMatchSettings aSettings)
		{
			decimal relevance = Decimal.Zero;
			decimal[] weights = { 0.1m, 0.7m, 0.2m };
			double sigma = 50.0;
			double meanValue = -7.0;
			if (aSettings != null) 
			{
				if (aSettings.DateCompareWeight.HasValue && aSettings.RefNbrCompareWeight.HasValue && aSettings.PayeeCompareWeight.HasValue)
				{
					Decimal totalWeight = (aSettings.DateCompareWeight.Value + aSettings.RefNbrCompareWeight.Value + aSettings.PayeeCompareWeight.Value);
					if (totalWeight != Decimal.Zero)
					{
						weights[0] = aSettings.DateCompareWeight.Value / totalWeight;
						weights[1] = aSettings.RefNbrCompareWeight.Value / totalWeight;
						weights[2] = aSettings.PayeeCompareWeight.Value / totalWeight;
					}
				}
				if (aSettings.DateMeanOffset.HasValue)
					meanValue = (double) aSettings.DateMeanOffset.Value;
				if (aSettings.DateSigma.HasValue)
					sigma = (double) aSettings.DateSigma.Value;
			}			
			bool looseCompare = false;
			relevance += CompareDate(aDetail, aTran, meanValue, sigma) * weights[0];
			relevance += CompareRefNbr(aDetail, aTran, looseCompare) * weights[1];
			relevance += ComparePayee(aDetail, aTran) * weights[2];
			return relevance;
		}

		protected virtual decimal CompareDate(CABankStatementDetail aDetail, CATran aTran, Double meanValue, Double sigma ) 
		{
			TimeSpan diff1 = (aDetail.TranDate.Value - aTran.TranDate.Value);
			TimeSpan diff2 = aDetail.TranEntryDate.HasValue? (aDetail.TranEntryDate.Value - aTran.TranDate.Value): diff1;
			TimeSpan diff = diff1.Duration() < diff2.Duration() ? diff1 : diff2;
			Double sigma2 = (sigma*sigma);
			if (sigma2 < 1.0) 
			{
				sigma2 = 0.25; //Corresponds to 0.5 day
			} 
			decimal res = (decimal) Math.Exp(-(Math.Pow(diff.TotalDays - meanValue,2.0)/(2*sigma2))); //Normal Distribution 
			return res > 0 ? res : 0.0m;
		}
		protected virtual decimal CompareRefNbr(CABankStatementDetail aDetail, CATran aTran, bool looseCompare)		
		{
			if (looseCompare)
				return EvaluateMatching(aDetail.ExtRefNbr, aTran.ExtRefNbr, false);
			else
				return EvaluateTideMatching(aDetail.ExtRefNbr, aTran.ExtRefNbr, false);
		}
		protected virtual decimal ComparePayee(CABankStatementDetail aDetail, CATran aTran) 
		{
			if(aDetail.PayeeBAccountID.HasValue)
			{
				return aDetail.PayeeBAccountID == aTran.ReferenceID ? Decimal.One : Decimal.Zero;
			}
			return EvaluateMatching(aDetail.PayeeName, aTran.ReferenceName, false);				
		}

		//Returns Degree of matching between two strings - as a percentage of matching characters 
		//in the correstponding positions of the aStr1 and aStr2
		//Return value - decimal between 0 and 1.
		public virtual decimal EvaluateMatching(string aStr1, string aStr2, bool aCaseSensitive) 
		{
			decimal result = decimal.Zero;
		    if (String.IsNullOrEmpty(aStr1) || String.IsNullOrEmpty(aStr2)) 
			{
				return (String.IsNullOrEmpty(aStr1) && String.IsNullOrEmpty(aStr2)) ? Decimal.One : Decimal.Zero;
			}  
			string str1 = aStr1.Trim();
			string str2 = aStr2.Trim();
			int length = str1.Length > str2.Length ? str1.Length : str2.Length;
			if (length == 0) return Decimal.One;
			Decimal charWeight = Decimal.One /(decimal)length;
			Decimal total = Decimal.Zero;			
			for (int i = 0; i < length; i++) 
			{
				if (i < str1.Length && i < str2.Length) 
				{
					bool match = (aCaseSensitive) ? (str2[i].CompareTo(str2[i]) == 0) : (Char.ToLower(str2[i]).CompareTo(Char.ToLower(str1[i]))==0);
					if (match)
						result += charWeight;
				}
				total += charWeight;
			}
			//Compencate rounding
			if (result > Decimal.Zero && total != Decimal.One) 
			{
				result += (Decimal.One - total); 
			}  
			return result;
		}

		public virtual decimal EvaluateTideMatching(string aStr1, string aStr2, bool aCaseSensitive)
		{
			decimal result = decimal.One;
			const int maxDiffCount = 3;
			decimal[] distr = { Decimal.One, 0.5m, 0.25m, 0.05m };
			if (String.IsNullOrEmpty(aStr1) || String.IsNullOrEmpty(aStr2))
			{
				return (String.IsNullOrEmpty(aStr1) && String.IsNullOrEmpty(aStr2)) ? Decimal.One : Decimal.Zero;
			}
			
			string str1 = aStr1.Trim();
			string str2 = aStr2.Trim();
			
			long strAsInt1, strAsInt2;
			if (Int64.TryParse(str1, out strAsInt1) && Int64.TryParse(str2, out strAsInt2)) 
			{
				return (strAsInt1 == strAsInt2 ? Decimal.One : Decimal.Zero);
			}

			int length = Math.Max(str1.Length,str2.Length);
			if (length == 0) return Decimal.One;
			int diff = Math.Abs(str1.Length- str2.Length);
			if (diff > maxDiffCount) return Decimal.Zero;
			int differentCount = 0;
			for (int i = 0; i < length; i++)
			{
				if (i < str1.Length && i < str2.Length)
				{
					bool match = (aCaseSensitive) ? (str2[i].CompareTo(str2[i]) == 0) : (Char.ToLower(str2[i]).CompareTo(Char.ToLower(str1[i])) == 0);
					if (!match)
						differentCount++;
				}
				else 
				{
					differentCount++;
				}
				if (differentCount > maxDiffCount) return Decimal.Zero;
			}
			//Compencate rounding
			
			result = distr[differentCount];
			return result;
		}

        private void SetDocTypeList(object Row)
        {
            CABankStatementDetail detail = (CABankStatementDetail)Row;

            List<string> AllowedValues = new List<string>();
            List<string> AllowedLabels = new List<string>();

            if (detail.OrigModule == GL.BatchModule.AP)
            {
                if (detail.DocType == APDocType.Refund)
                {
                    PXDefaultAttribute.SetDefault<CABankStatementAdjustment.adjdDocType>(Adjustments.Cache, APDocType.DebitAdj);
                    PXStringListAttribute.SetList<CABankStatementAdjustment.adjdDocType>(Adjustments.Cache, null, new string[] { APDocType.DebitAdj, APDocType.Prepayment }, new string[] { AP.Messages.DebitAdj, AP.Messages.Prepayment });
                }
                else if (detail.DocType == APDocType.Prepayment)
                {
                    PXDefaultAttribute.SetDefault<CABankStatementAdjustment.adjdDocType>(Adjustments.Cache, APDocType.Invoice);
                    PXStringListAttribute.SetList<CABankStatementAdjustment.adjdDocType>(Adjustments.Cache, null, new string[] { APDocType.Invoice, APDocType.CreditAdj }, new string[] { AP.Messages.Invoice, AP.Messages.CreditAdj });
                }
                else if (detail.DocType == APDocType.Check || ((APPayment)Row).DocType == APDocType.VoidCheck)
                {
                    PXDefaultAttribute.SetDefault<CABankStatementAdjustment.adjdDocType>(Adjustments.Cache, APDocType.Invoice);
                    PXStringListAttribute.SetList<CABankStatementAdjustment.adjdDocType>(Adjustments.Cache, null, new string[] { APDocType.Invoice, APDocType.DebitAdj, APDocType.CreditAdj, APDocType.Prepayment }, new string[] { AP.Messages.Invoice, AP.Messages.DebitAdj, AP.Messages.CreditAdj, AP.Messages.Prepayment });
                }
                else
                {
                    PXDefaultAttribute.SetDefault<CABankStatementAdjustment.adjdDocType>(Adjustments.Cache, APDocType.Invoice);
                    PXStringListAttribute.SetList<CABankStatementAdjustment.adjdDocType>(Adjustments.Cache, null, new string[] { APDocType.Invoice, APDocType.CreditAdj, APDocType.Prepayment }, new string[] { AP.Messages.Invoice, AP.Messages.CreditAdj, AP.Messages.Prepayment });
                }
            }
            else if (detail.OrigModule == GL.BatchModule.AR)
            {

                if (detail.DocType == ARDocType.Refund)
                {
                    PXDefaultAttribute.SetDefault<CABankStatementAdjustment.adjdDocType>(Adjustments.Cache, ARDocType.CreditMemo);
                    PXStringListAttribute.SetList<CABankStatementAdjustment.adjdDocType>(Adjustments.Cache, null, new string[] { ARDocType.CreditMemo, ARDocType.Payment, ARDocType.Prepayment }, new string[] { AR.Messages.CreditMemo, AR.Messages.Payment, AR.Messages.Prepayment });
                }
                else if (detail.DocType == ARDocType.Payment || detail.DocType == ARDocType.VoidPayment)
                {
                    PXDefaultAttribute.SetDefault<CABankStatementAdjustment.adjdDocType>(Adjustments.Cache, ARDocType.Invoice);
                    PXStringListAttribute.SetList<CABankStatementAdjustment.adjdDocType>(Adjustments.Cache, null,
                        new string[] { ARDocType.Invoice, ARDocType.DebitMemo, ARDocType.CreditMemo, ARDocType.FinCharge, ARDocType.SmallCreditWO },
                        new string[] { AR.Messages.Invoice, AR.Messages.DebitMemo, AR.Messages.CreditMemo, AR.Messages.FinCharge, AR.Messages.SmallCreditWO });
                }
                else
                {
                    PXDefaultAttribute.SetDefault<CABankStatementAdjustment.adjdDocType>(Adjustments.Cache, ARDocType.Invoice);
                    PXStringListAttribute.SetList<CABankStatementAdjustment.adjdDocType>(Adjustments.Cache, null,
                        new string[] { ARDocType.Invoice, ARDocType.DebitMemo, ARDocType.FinCharge, ARDocType.SmallCreditWO },
                        new string[] { AR.Messages.Invoice, AR.Messages.DebitMemo, AR.Messages.FinCharge, AR.Messages.SmallCreditWO });
                }
            }
        }

        public virtual void RecalcApplAmounts(PXCache sender, CAApplicationStatementDetail row)
        {
            using (new PXConnectionScope())
            {
                internalCall = true;
                try
                {
                    PXFormulaAttribute.CalcAggregate<CABankStatementAdjustment.curyAdjgAmt>(Adjustments.Cache, row, true);                    
                }
                finally
                {
                    internalCall = false;
                }
                sender.RaiseFieldUpdated<CAApplicationStatementDetail.curyApplAmt>(row, null);
                PXDBCurrencyAttribute.CalcBaseValues<CAApplicationStatementDetail.curyApplAmt>(sender, row);
                PXDBCurrencyAttribute.CalcBaseValues<CAApplicationStatementDetail.curyUnappliedBal>(sender, row);
            }
        }

        private void CalculateBalancesAR(ARAdjust adj, ARInvoice invoice, bool isCalcRGOL, bool DiscOnDiscDate)
        {
            PaymentEntry.CalcBalances<ARInvoice, ARAdjust>(CurrencyInfo_CuryInfoID, adj.AdjgCuryInfoID, adj.AdjdCuryInfoID, invoice, adj);
            if (DiscOnDiscDate)
            {
                PaymentEntry.CalcDiscount<ARInvoice, ARAdjust>(adj.AdjgDocDate, invoice, adj);
            }
            PaymentEntry.WarnDiscount<ARInvoice, ARAdjust>(this, adj.AdjgDocDate, invoice, adj);

            if (customer.Current != null && customer.Current.SmallBalanceAllow == true && adj.AdjgDocType != ARDocType.Refund && adj.AdjdDocType != ARDocType.CreditMemo)
            {
                decimal payment_smallbalancelimit;
                CurrencyInfo payment_info = CurrencyInfo_CuryInfoID.Select(adj.AdjgCuryInfoID);
                PXDBCurrencyAttribute.CuryConvCury(CurrencyInfo_CuryInfoID.Cache, payment_info, customer.Current.SmallBalanceLimit ?? 0m, out payment_smallbalancelimit);
                adj.CuryWOBal = payment_smallbalancelimit;
                adj.WOBal = customer.Current.SmallBalanceLimit;
            }
            else
            {
                adj.CuryWOBal = 0m;
                adj.WOBal = 0m;
            }

            PaymentEntry.AdjustBalance<ARAdjust>(CurrencyInfo_CuryInfoID, adj);
            if (isCalcRGOL && (adj.Voided != true))
            {
                PaymentEntry.CalcRGOL<ARInvoice, ARAdjust>(CurrencyInfo_CuryInfoID, invoice, adj);
                adj.RGOLAmt = (bool)adj.ReverseGainLoss ? -1.0m * adj.RGOLAmt : adj.RGOLAmt;
            }
        }

        private void UpdateBalance(CABankStatementAdjustment adj, bool isCalcRGOL)
        {
            if (ApplicationDetails.Current.OrigModule == GL.BatchModule.AP)
            {
                foreach (PXResult<APInvoice, CurrencyInfo> res in PXSelectJoin<APInvoice, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>>, Where<APInvoice.docType, Equal<Required<APInvoice.docType>>, And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>.Select(this, adj.AdjdDocType, adj.AdjdRefNbr))
                {
                    APInvoice invoice = (APInvoice)res;

                    APAdjust adjustment = new APAdjust();
                    adjustment.AdjdRefNbr = adj.AdjdRefNbr;
                    adjustment.AdjdDocType = adj.AdjdDocType;
                    CopyToAdjust(adjustment, adj);

                    APPaymentEntry.CalcBalances<APInvoice>(CurrencyInfo_CuryInfoID, adjustment, invoice, isCalcRGOL, true);

                    CopyToBankAdjustment(adj, adjustment);
                    adj.AdjdCuryRate = adjustment.AdjdCuryRate;

                }
            }
            else if (ApplicationDetails.Current.OrigModule == GL.BatchModule.AR)
            {
                foreach (ARInvoice invoice in ARInvoice_CustomerID_DocType_RefNbr.Select(adj.VendorID, adj.AdjdDocType, adj.AdjdRefNbr))
                {
                    ARAdjust adjustment = new ARAdjust();
                    CopyToAdjust(adjustment, adj);
                    adjustment.AdjdRefNbr = adj.AdjdRefNbr;
                    adjustment.AdjdDocType = adj.AdjdDocType;

                    CalculateBalancesAR(adjustment, invoice, isCalcRGOL, false);

                    CopyToBankAdjustment(adj, adjustment);
                }
            }
        }

        private static CABankStatementAdjustment CopyToBankAdjustment(CABankStatementAdjustment bankAdj, IAdjustment iAdjust)
        {
            bankAdj.AdjgCuryInfoID = iAdjust.AdjgCuryInfoID;
            bankAdj.AdjdCuryInfoID = iAdjust.AdjdCuryInfoID;
            bankAdj.AdjgDocDate = iAdjust.AdjgDocDate;
            bankAdj.DocBal = iAdjust.DocBal;
            bankAdj.CuryDocBal = iAdjust.CuryDocBal;
            bankAdj.CuryDiscBal = iAdjust.CuryDiscBal;
            bankAdj.CuryWhTaxBal = iAdjust.CuryWhTaxBal;
            bankAdj.CuryAdjgAmt = iAdjust.CuryAdjgAmt;
            bankAdj.CuryAdjgDiscAmt = iAdjust.CuryAdjgDiscAmt;
            bankAdj.CuryAdjgWhTaxAmt = iAdjust.CuryAdjgWhTaxAmt;
            bankAdj.AdjdOrigCuryInfoID = iAdjust.AdjdOrigCuryInfoID;
            return bankAdj;
        }
        private static IAdjustment CopyToAdjust(IAdjustment iAdjust, CABankStatementAdjustment bankAdj)
        {
            iAdjust.AdjgCuryInfoID = bankAdj.AdjgCuryInfoID;
            iAdjust.AdjdCuryInfoID = bankAdj.AdjdCuryInfoID;
            iAdjust.AdjgDocDate = bankAdj.AdjgDocDate;
            iAdjust.DocBal = bankAdj.DocBal;
            iAdjust.CuryDocBal = bankAdj.CuryDocBal;
            iAdjust.CuryDiscBal = bankAdj.CuryDiscBal;
            iAdjust.CuryWhTaxBal = bankAdj.CuryWhTaxBal;
            iAdjust.CuryAdjgAmt = bankAdj.CuryAdjgAmt;
            iAdjust.CuryAdjgDiscAmt = bankAdj.CuryAdjgDiscAmt;
            iAdjust.CuryAdjgWhTaxAmt = bankAdj.CuryAdjgWhTaxAmt;
            iAdjust.AdjdOrigCuryInfoID = bankAdj.AdjdOrigCuryInfoID;
            return iAdjust;
        }

		#endregion

		#region Import-Related functions
		public virtual void ImportStatement(PX.SM.FileInfo aFileInfo)
		{
			bool isFormatRecognized = false;
			IStatementReader reader = this.CreateReader();			
			if (reader !=null && reader.IsValidInput(aFileInfo.BinData))
			{				
				reader.Read(aFileInfo.BinData);
				List<CABankStatement> imported;
				reader.ExportTo(aFileInfo, this, out imported);
				if (imported != null)
				{
					CABankStatement last = (imported != null && imported.Count > 0) ? imported[imported.Count-1] : null; 
					if (this.BankStatement.Current == null || (last != null 
						&& (this.BankStatement.Current.CashAccountID!= last.CashAccountID || this.BankStatement.Current.RefNbr != last.RefNbr)))
					{
						this.BankStatement.Current = this.BankStatement.Search<CABankStatement.cashAccountID, CABankStatement.refNbr>(last.CashAccountID, last.RefNbr);
						throw new PXRedirectRequiredException(this, "Navigate to the uploaded record");
					}
				}
				isFormatRecognized = true;
			}
			if (!isFormatRecognized)
			{
				throw new PXException(Messages.UploadFileHasUnrecognizedBankStatementFormat);
			}
		}
		protected virtual IStatementReader CreateReader() 
		{
			IStatementReader processor = null;
			bool importToSingleAccount = this.casetup.Current.ImportToSingleAccount??false;
			string typeName = this.casetup.Current.StatementImportTypeName;
			if (importToSingleAccount)
			{
				CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Optional<CABankStatement.cashAccountID>>>>.Select(this);
				if (acct != null)
					typeName = acct.StatementImportTypeName;
			}			

			if (string.IsNullOrEmpty(typeName)) 
				return processor;
			try
			{
				Type processorType = System.Web.Compilation.BuildManager.GetType(typeName, true);
				processor = (IStatementReader)Activator.CreateInstance(processorType);
			}
			catch (Exception e)
			{
				throw new PXException(e, Messages.StatementServiceReaderCreationError, typeName);
			}
			return processor;
		}

        public bool IsAlreadyImported(int? aCashAccountID, string aExtTranID, out string aRefNbr)
        {
            aRefNbr = null;          
            CABankStatementDetail detail = PXSelectReadonly<CABankStatementDetail,
                                               Where<CABankStatementDetail.cashAccountID, Equal<Required<CABankStatementDetail.cashAccountID>>,
                                                And<CABankStatementDetail.extTranID, Equal<Required<CABankStatementDetail.extTranID>>>>>.Select(this, aCashAccountID, aExtTranID);
            if (detail != null)
                aRefNbr = detail.RefNbr;
            return (detail != null);
        }

		#endregion

		#region Events
		#region CABankStatement Events

		protected virtual void CABankStatement_EndBalanceDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CABankStatement row = (CABankStatement)e.Row;
			if (row.Hold != false && row.StatementDate.HasValue)
				e.NewValue = row.StatementDate;
		}

		protected virtual void CABankStatement_CuryEndBalance_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CABankStatement row = (CABankStatement)e.Row;
			if (row != null && row.Hold != false)
			{
				e.NewValue = (row.CuryBegBalance ?? Decimal.Zero);
			}
		}

		protected virtual void CABankStatement_CashAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankStatement header = (CABankStatement)e.Row;
			sender.SetDefaultExt<CABankStatement.curyID>(e.Row);
			sender.SetDefaultExt<CABankStatement.startBalanceDate>(e.Row);
			sender.SetDefaultExt<CABankStatement.endBalanceDate>(e.Row);
			sender.SetDefaultExt<CABankStatement.curyBegBalance>(e.Row);
			sender.SetDefaultExt<CABankStatement.curyEndBalance>(e.Row);
		}

		protected virtual void CABankStatement_StatementDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankStatement row = (CABankStatement)e.Row;
			if (row != null && row.Hold == true)
			{
				sender.SetDefaultExt<CABankStatement.startBalanceDate>(e.Row);
				sender.SetDefaultExt<CABankStatement.endBalanceDate>(e.Row);
				sender.SetDefaultExt<CABankStatement.curyBegBalance>(e.Row);
				sender.SetDefaultExt<CABankStatement.curyEndBalance>(e.Row);
			}
		}

		protected virtual void CABankStatement_Hold_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			CABankStatement row = (CABankStatement)e.Row;
			if ((bool)e.NewValue != true)
			{
				bool hasError = false;
				if (row.CuryDetailsBalanceDiff != null && (Decimal)row.CuryDetailsBalanceDiff != 0)
				{
				    sender.RaiseExceptionHandling<CABankStatement.curyEndBalance>(row, row.CuryEndBalance, new PXSetPropertyException(Messages.StatementIsOutOfBalanceEndBalanceDoesNotMatchDetailsTotal));
					hasError = true;
				}
				
				if (row.CuryCreditsTotal != row.CuryReconciledCredits || row.CountCredit != row.ReconciledCountCredit)
				{
					sender.RaiseExceptionHandling<CABankStatement.curyReconciledCredits>(row, row.CuryReconciledCredits, new PXSetPropertyException(Messages.StatementIsOutOfBalanceThereAreUnmatchedDetails));
					hasError = true;					
				}

				if (row.CuryDebitsTotal != row.CuryReconciledDebits || row.CountDebit != row.ReconciledCountDebit)
				{
					sender.RaiseExceptionHandling<CABankStatement.curyReconciledDebits>(row, row.CuryReconciledDebits, new PXSetPropertyException(Messages.StatementIsOutOfBalanceThereAreUnmatchedDetails));
					hasError = true;
				}

				if (!row.StartBalanceDate.HasValue)
				{
					sender.RaiseExceptionHandling<CABankStatement.startBalanceDate>(row, row.StartBalanceDate, new PXSetPropertyException(Messages.StatementStartBalanceDateIsRequired));
					hasError = true;
				}

				if (!row.EndBalanceDate.HasValue)
				{
					sender.RaiseExceptionHandling<CABankStatement.endBalanceDate>(row, row.EndBalanceDate, new PXSetPropertyException(Messages.StatementEndBalanceDateIsRequired));
					hasError=true;
				}

				if (row.EndBalanceDate < row.StartBalanceDate) 
				{
					sender.RaiseExceptionHandling<CABankStatement.endBalanceDate>(row, row.EndBalanceDate, new PXSetPropertyException(Messages.StatementEndDateMustBeGreaterThenStartDate));
					hasError = true;
				}
				if(hasError)
				{
					e.NewValue = row.Hold;
					e.Cancel = true;
				}
				
			}
		}

		protected virtual void CABankStatement_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            if (e.Row == null) return;
			bool importToSingleAccount = (this.casetup.Current.ImportToSingleAccount ?? false); 
            CABankStatement row = (CABankStatement)e.Row;
			bool isReleased = (row.Released == true);
			bool isHold = (row.Hold == true);
			bool isBalanced = !isHold && !isReleased;
			bool hasKeys = row.CashAccountID.HasValue;
			bool showSelection = false;
			
			if ((bool)row.Released)
			{
				PXUIFieldAttribute.SetEnabled(sender, null, false);				
				PXUIFieldAttribute.SetEnabled<CABankStatement.cashAccountID>(sender, row, true);
				PXUIFieldAttribute.SetEnabled<CABankStatement.refNbr>(sender, row, true);
				PXUIFieldAttribute.SetVisible<CABankStatement.hold>(sender, row, false);
				PXUIFieldAttribute.SetVisible<CABankStatement.released>(sender, row, true);
				sender.AllowDelete = false;
				sender.AllowUpdate = true;
				this.Details.Cache.AllowDelete = false;
				this.Details.Cache.AllowUpdate = false;
				this.Details.Cache.AllowInsert = false;
				this.matchSettingsPanel.SetEnabled(false);
			}
			else
			{
				showSelection = (this.matchSettings.Current.MatchInSelection == true);
				sender.AllowDelete = hasKeys;
				sender.AllowUpdate = true;
				this.Details.Cache.AllowDelete = isHold && hasKeys;
				this.Details.Cache.AllowUpdate = isHold && hasKeys;
				this.Details.Cache.AllowInsert = isHold && hasKeys;				
				if (hasKeys)
				{
					PXUIFieldAttribute.SetEnabled(sender, null, true);
					PXUIFieldAttribute.SetEnabled<CABankStatement.startBalanceDate>(sender, row, isHold);
					PXUIFieldAttribute.SetEnabled<CABankStatement.endBalanceDate>(sender, row, isHold);
					PXUIFieldAttribute.SetEnabled<CABankStatement.statementDate>(sender, row, isHold);
					PXUIFieldAttribute.SetEnabled<CABankStatement.curyBegBalance>(sender, row, isHold);
					PXUIFieldAttribute.SetEnabled<CABankStatement.curyEndBalance>(sender, row, isHold);
					PXUIFieldAttribute.SetVisible<CABankStatement.hold>(sender, row, true);
					PXUIFieldAttribute.SetVisible<CABankStatement.released>(sender, row, false);
				}
				else 
				{
					PXUIFieldAttribute.SetEnabled(sender, null, false);
					PXUIFieldAttribute.SetEnabled<CABankStatement.cashAccountID>(sender, row, true);
					PXUIFieldAttribute.SetEnabled<CABankStatement.refNbr>(sender, row, true);				
				}
			}

			PXUIFieldAttribute.SetVisible<MatchSettings.matchInSelection>(this.matchSettings.Cache, null, (this.casetup.Current.MatchInSelection == true));
			PXUIFieldAttribute.SetEnabled<CABankStatementDetail.selected>(this.Details.Cache, null, showSelection);
			PXUIFieldAttribute.SetVisible<CABankStatementDetail.selected>(this.Details.Cache, null, showSelection);
			if (importToSingleAccount)
			{
				PXEntryStatus status =  sender.GetStatus(e.Row);
				this.uploadFile.SetEnabled(isHold && hasKeys && status == PXEntryStatus.Inserted);
			}
			else 
			{
				//this.import.SetEnabled(isHold);
			}
			this.createAllDocuments.SetEnabled(isHold && hasKeys);
			this.createDocument.SetEnabled(isHold && hasKeys);
			this.matchTrans.SetEnabled(isHold && hasKeys);
			this.clearMatch.SetEnabled(isHold && hasKeys);
			this.autoMatch.SetEnabled(isHold && hasKeys);
			this.release.SetEnabled(isBalanced);
			this.validateMatches.SetEnabled((isReleased != true) && hasKeys);
			this.matchSettingsPanel.SetVisible(isHold && hasKeys);
			this.matchSettingsPanel.SetEnabled(isHold && hasKeys);
	
			PXUIFieldAttribute.SetEnabled<CABankStatement.status>(sender, row, false);
			PXUIFieldAttribute.SetEnabled<CABankStatement.curyID>(sender, row, false);

			PXUIFieldAttribute.SetEnabled<CABankStatement.countCredit>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<CABankStatement.countDebit>(sender, null, false);

			PXUIFieldAttribute.SetEnabled<CABankStatement.reconciledCountCredit>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<CABankStatement.reconciledCountDebit>(sender, null, false);

			PXUIFieldAttribute.SetEnabled<CABankStatement.curyCreditsTotal>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<CABankStatement.curyDebitsTotal>(sender, null, false);

			PXUIFieldAttribute.SetEnabled<CABankStatement.curyReconciledDebits>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<CABankStatement.curyReconciledCredits>(sender, null, false);

			PXUIFieldAttribute.SetEnabled<CABankStatement.curyDetailsEndBalance>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<CABankStatement.curyDetailsBalanceDiff>(sender, null, false);

			PXUIFieldAttribute.SetEnabled<CABankStatement.curyReconciledEndBalance>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<CABankStatement.curyReconciledBalanceDiff>(sender, null, false);

			PXUIFieldAttribute.SetEnabled<CABankStatement.bankStatementFormat>(sender, null, false);
			PXUIFieldAttribute.SetEnabled<CABankStatement.formatVerisionNbr>(sender, null, false);
			
			
			if (row.Released != true)
			{
				if (row.CuryDetailsBalanceDiff != Decimal.Zero)
				{
                    sender.RaiseExceptionHandling<CABankStatement.curyEndBalance>(row, row.CuryEndBalance, new PXSetPropertyException(Messages.StatementIsOutOfBalanceEndBalanceDoesNotMatchDetailsTotal, (bool)row.Hold ? PXErrorLevel.Warning: PXErrorLevel.Error));
                    sender.RaiseExceptionHandling<CABankStatement.curyDetailsBalanceDiff>(row, row.CuryDetailsBalanceDiff, new PXSetPropertyException(Messages.StatementIsOutOfBalanceEndBalanceDoesNotMatchDetailsTotal, (bool)row.Hold ? PXErrorLevel.Warning : PXErrorLevel.Error));
				}
				else
				{
					sender.RaiseExceptionHandling<CABankStatement.curyEndBalance>(row, row.CuryEndBalance, null);
					sender.RaiseExceptionHandling<CABankStatement.curyDetailsBalanceDiff>(row, row.CuryDetailsBalanceDiff, null);
				}
				PXDefaultAttribute.SetPersistingCheck<CABankStatement.startBalanceDate>(sender,row,isBalanced?PXPersistingCheck.NullOrBlank: PXPersistingCheck.Nothing);
				PXUIFieldAttribute.SetRequired<CABankStatement.startBalanceDate>(sender, isBalanced);
				PXDefaultAttribute.SetPersistingCheck<CABankStatement.endBalanceDate>(sender,row,isBalanced?PXPersistingCheck.NullOrBlank: PXPersistingCheck.Nothing);
				PXUIFieldAttribute.SetRequired<CABankStatement.endBalanceDate>(sender, isBalanced);				
			}

		}

		protected virtual void CABankStatement_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			CABankStatement row = (CABankStatement)e.Row;
			if (row!= null &&  row.Released == false)
			{
				if (row.Hold != true )
				{
					if (row.StartBalanceDate > row.EndBalanceDate) 
					{
						sender.RaiseExceptionHandling<CABankStatement.endBalanceDate>(e.Row, row.EndBalanceDate, new PXSetPropertyException(Messages.StatementEndDateMustBeGreaterThenStartDate, PXErrorLevel.RowError));
					} 					
				}
			}
		}

		#endregion

		#region CABankStatementDetail Events
		protected virtual void CABankStatementDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CABankStatementDetail row = (CABankStatementDetail)e.Row;
			CABankStatement doc = this.BankStatement.Current;
			if (doc != null && row != null && doc.Released == false)
			{
				bool isNotMatched = (row.DocumentMatched == false);
                bool hasApplications = row.LineCntr.HasValue && row.LineCntr > 0;
				PXUIFieldAttribute.SetEnabled(sender, e.Row, (isNotMatched && hasApplications== false));
                if (isNotMatched && (hasApplications == false))
				{					
					bool createDocument = row.CreateDocument.Value;
					bool isAP = (row.OrigModule == GL.BatchModule.AP);
					bool isAR = (row.OrigModule == GL.BatchModule.AR);
					bool isCA = (row.OrigModule == GL.BatchModule.CA);
                    bool isPMInstanceRequired = false;
                    if (isAR && !String.IsNullOrEmpty(row.PaymentMethodID) ) 
                    {
                        PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, row.PaymentMethodID);
                        isPMInstanceRequired = (pm.IsAccountNumberRequired == true); 
                    }

					PXUIFieldAttribute.SetEnabled<CABankStatementDetail.origModule>(sender, e.Row, createDocument);
					PXUIFieldAttribute.SetEnabled<CABankStatementDetail.paymentMethodID>(sender, e.Row, createDocument && (isAP||isAR));
                    PXUIFieldAttribute.SetEnabled<CABankStatementDetail.pMInstanceID>(sender, e.Row, createDocument && isAR && isPMInstanceRequired);
					//PXUIFieldAttribute.SetEnabled<CABankStatementDetail.paymentMethodID>(sender, e.Row, createDocument && isAP);
					PXUIFieldAttribute.SetEnabled<CABankStatementDetail.payeeBAccountID>(sender, e.Row, createDocument && (isAP || isAR));
					PXUIFieldAttribute.SetEnabled<CABankStatementDetail.payeeLocationID>(sender, e.Row, createDocument && (isAP || isAR));
					PXUIFieldAttribute.SetEnabled<CABankStatementDetail.entryTypeID>(sender, e.Row, createDocument && isCA);
                    //PXDefaultAttribute.SetPersistingCheck<CABankStatementDetail.paymentMethodID>(sender,e.Row, createDocument && is
					//PXUIFieldAttribute.SetEnabled<CABankStatementDetail.finPeriodID>(sender, e.Row, createDocument);
				}
			}            
			PXUIFieldAttribute.SetEnabled<CABankStatementDetail.documentMatched>(sender, e.Row, false);
		}

		protected virtual void CABankStatementDetail_OrigModule_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CABankStatementDetail row = (CABankStatementDetail)e.Row;
			if (row!= null && row.CreateDocument ==true)
			{
				if (row.DrCr == CADrCr.CACredit)
					e.NewValue = GL.BatchModule.AP;
				else if (row.DrCr == CADrCr.CADebit)
					e.NewValue = GL.BatchModule.AR;
			}
			else
			{ 
				e.NewValue = null;
			}
			e.Cancel = true;
		}

		protected virtual void CABankStatementDetail_OrigModule_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankStatementDetail row = (CABankStatementDetail)e.Row;
			if (row != null)
			{
				sender.SetDefaultExt<CABankStatementDetail.payeeBAccountID>(e.Row);
				sender.SetDefaultExt<CABankStatementDetail.entryTypeID>(e.Row);				
			}
		}
				
		protected virtual void CABankStatementDetail_CreateDocument_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankStatementDetail row = (CABankStatementDetail)e.Row;
			if (row != null)
			{
				sender.SetDefaultExt<CABankStatementDetail.origModule>(e.Row);
			}
		}

		protected virtual void CABankStatementDetail_PayeeBAccountID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CABankStatementDetail row = (CABankStatementDetail)e.Row;
			if (row != null)
			{
				sender.SetDefaultExt<CABankStatementDetail.payeeLocationID>(e.Row);
				sender.SetDefaultExt<CABankStatementDetail.paymentMethodID>(e.Row);
				sender.SetDefaultExt<CABankStatementDetail.pMInstanceID>(e.Row);
			}
		}

		protected virtual void CABankStatementDetail_EntryTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CABankStatementDetail row = (CABankStatementDetail)e.Row;
			if (row.OrigModule == GL.BatchModule.CA)
			{
				string entryTypeID = this.casetup.Current.UnknownPaymentEntryTypeID;
				CAEntryType entryType =  PXSelect<CAEntryType,Where<CAEntryType.entryTypeId,Equal<Required<CAEntryType.entryTypeId>>>>.Select(this, entryTypeID);
				e.NewValue = (entryType != null && entryType.DrCr == row.DrCr)? entryTypeID : null;				
			}
			else
			{
				e.NewValue = null;
			}
			e.Cancel = true;
		}

        protected virtual void CABankStatementDetail_PaymentMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CABankStatementDetail row = (CABankStatementDetail)e.Row;
            if (row != null)
            {
                sender.SetDefaultExt<CABankStatementDetail.pMInstanceID>(e.Row);
            }
        }

        protected virtual void CABankStatementDetail_TranDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CABankStatementDetail row = (CABankStatementDetail)e.Row;
            if (row != null)
            {
                if (row.CATranID.HasValue == false)
                {
                    CurrencyInfoAttribute.SetEffectiveDate<CABankStatementDetail.tranDate>(sender, e);                    
                }
            }
        }
	
		protected virtual void CABankStatementDetail_TranDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			CABankStatementDetail row = (CABankStatementDetail)e.Row;
			CABankStatement doc = this.BankStatement.Current;
			if (row != null && doc != null) 
			{
				e.NewValue = doc.TranMaxDate.HasValue ? doc.TranMaxDate : doc.StartBalanceDate;
				e.Cancel = true;
			}
			
		}

		protected virtual void CABankStatementDetail_ExtTranID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{ 
			CABankStatementDetail row = (CABankStatementDetail) e.Row;
			if (row != null)
			{
				Dictionary<long, CAMessage> listMessages = PXLongOperation.GetCustomInfo(this.UID) as Dictionary<long, CAMessage>;
				TimeSpan timespan;
				Exception ex;
				PXLongRunStatus status = PXLongOperation.GetStatus(this.UID, out timespan, out ex);
				if ((status == PXLongRunStatus.Aborted || status == PXLongRunStatus.Completed)
							&& listMessages != null)
				{
					CAMessage message = null;
					if (listMessages.ContainsKey(row.CATranID.Value))
						message = listMessages[row.CATranID.Value];
					if (message != null)
					{
						string fieldName = typeof(CABankStatementDetail.extTranID).Name;

						e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(String), false, null, null, null, null, null, fieldName,
									null, null, message.Message, message.ErrorLevel, null, null, null, PXUIVisibility.Undefined, null, null, null);
						e.IsAltered = true;
					}
				}
			}
		}
        private bool _isCacheSync = false;

		protected virtual void CABankStatementDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			CABankStatementDetail row = (CABankStatementDetail)e.Row;
			CABankStatementDetail oldRow = (CABankStatementDetail)e.OldRow;
			if (row != null && row.DrCr != oldRow.DrCr)
			{
				if(row.DocumentMatched == false && row.CreateDocument == true && row.OrigModule == GL.BatchModule.CA)
					sender.SetDefaultExt<CABankStatementDetail.entryTypeID>(e.Row);
			}
            if (this._isCacheSync == false)
            {
                CAApplicationStatementDetail detailRow = (CAApplicationStatementDetail)this.ApplicationDetails.Search<CAApplicationStatementDetail.refNbr, CAApplicationStatementDetail.lineNbr>(row.RefNbr, row.LineNbr);
                if (detailRow != null)
                {
                    if (row.CreateDocument == true)
                    {
                        sender.RestoreCopy(detailRow, row);
                        this.ApplicationDetails.Update(detailRow);
                    }
                    else
                    {
                        this.ApplicationDetails.Delete(detailRow);
                    }
                }
                else 
                {
                    if (row.CreateDocument == true) 
                    {
                        CAApplicationStatementDetail detailRow1 = new CAApplicationStatementDetail();
                        sender.RestoreCopy(detailRow1, row);
                        this.ApplicationDetails.Insert(detailRow1);                        
                    }
                }
            }
		}
        protected virtual void CABankStatementDetail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            CABankStatementDetail row = (CABankStatementDetail)e.Row;
            if (row != null)
            {
                if (row.CATranID.HasValue == false)
                {
                    PXFieldUpdatedEventArgs args = new PXFieldUpdatedEventArgs(e.Row, false, false);
                    CurrencyInfoAttribute.SetEffectiveDate<CABankStatementDetail.tranDate>(sender, args);
                }
                if (row.CreateDocument == true)
                {
                    if (this._isCacheSync == false)
                    {
                        CAApplicationStatementDetail detailRow = new CAApplicationStatementDetail();
                        if (detailRow != null)
                        {
                            sender.RestoreCopy(detailRow, row);
                            this.ApplicationDetails.Insert(detailRow);
                        }
                    }
                }
            }
        }
        protected virtual void CABankStatementDetail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            CABankStatementDetail row = (CABankStatementDetail)e.Row;
            if (row != null)
            {
                if (this._isCacheSync == false)
                {
                    CAApplicationStatementDetail detailRow = (CAApplicationStatementDetail)this.ApplicationDetails.Search<CAApplicationStatementDetail.refNbr, CAApplicationStatementDetail.lineNbr>(row.RefNbr, row.LineNbr);
                    if (detailRow != null)
                    {
                        this.ApplicationDetails.Delete(detailRow);
                    }
                    if (row.CuryInfoID != null) 
                    {
                        CurrencyInfo info = this.currencyinfo.Select(row.CuryInfoID);
                        if (info != null)
                            this.currencyinfo.Delete(info);
                    }
                }
            }
        }    
         
#if false
        protected virtual void CABankStatementDetail_PaymentMethodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            CABankStatementDetail row = (CABankStatementDetail)e.Row;
            if (row != null && row.OrigModule == GL.BatchModule.AP && row.CashAccountID.HasValue)
            {
                PaymentMethod type = null;
                type = PXSelectJoin<PaymentMethod,
                                    InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
                                    And<PaymentMethodAccount.useForAP, Equal<True>>>>,
                                    Where<PaymentMethodAccount.cashAccountID, Equal<Required<CABankStatementDetail.cashAccountID>>>,
                                        OrderBy<Desc<PaymentMethodAccount.aPIsDefault>>>.Select(this, row.CashAccountID);

                e.NewValue = (type != null ? type.PaymentMethodID : null);
            }
            else
            {
                if (row != null && row.OrigModule == GL.BatchModule.AR && row.PMInstanceID.HasValue)
                {
                    CustomerPaymentMethod cpm = PXSelect<CustomerPaymentMethod, Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CABankStatementDetail.pMInstanceID>>>>.Select(this, row.PMInstanceID);
                    e.NewValue = cpm.PaymentMethodID;
                }
                else
                {
                    e.NewValue = null;
                }
            }
            e.Cancel = true;
        } 
#endif
		#endregion

        #region CABankStatementAdjustment Events

        protected virtual void CABankStatementAdjustment_AdjdRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            CABankStatementAdjustment row = (CABankStatementAdjustment)e.Row;
            CABankStatementDetail parent = PXSelect<CABankStatementDetail, Where<CABankStatementDetail.refNbr, Equal<Required<CABankStatementDetail.refNbr>>,
                                                And<CABankStatementDetail.lineNbr, Equal<Required<CABankStatementDetail.lineNbr>>>>>.Select(this, row.RefNbr, row.LineNbr);
            if (parent.DocumentMatched == true) 
            {
                e.Cancel = true;
            }
        }
        protected virtual void CABankStatementAdjustment_AdjdRefNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CABankStatementAdjustment adj = (CABankStatementAdjustment)e.Row;
            adj.AdjgCuryInfoID = ApplicationDetails.Current.CuryInfoID;
            adj.AdjgDocDate = ApplicationDetails.Current.TranDate;
            adj.Released = false;
            adj.CuryDocBal = null;
            adj.CuryDiscBal = null;
            adj.CuryWhTaxBal = null;
            adj.CuryAdjgAmt = null;

            if (ApplicationDetails.Current.OrigModule == GL.BatchModule.AP)
            {

                foreach (PXResult<APInvoice, CurrencyInfo> res in PXSelectJoin<APInvoice, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APInvoice.curyInfoID>>>, Where<APInvoice.docType, Equal<Required<APInvoice.docType>>, And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>.Select(this, adj.AdjdDocType, adj.AdjdRefNbr))
                {
                    CurrencyInfo info = (CurrencyInfo)res;
                    CurrencyInfo info_copy = null;
                    APInvoice invoice = (APInvoice)res;                    
                    
                    if (adj.AdjdDocType == APDocType.Prepayment)
                    {
                        //Prepayment cannot have RGOL
                        info = new CurrencyInfo();
                        info.CuryInfoID = ApplicationDetails.Current.CuryInfoID;
                        info_copy = info;
                    }
                    else
                    {
                        info_copy = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)res);
                        info_copy.CuryInfoID = null;
                        info_copy = (CurrencyInfo)currencyinfo.Cache.Insert(info_copy);

                        //currencyinfo.Cache.SetValueExt<CurrencyInfo.curyEffDate>(info_copy, Document.Current.DocDate);
                        info_copy.SetCuryEffDate(currencyinfo.Cache, ApplicationDetails.Current.TranDate);
                    }

                    adj.VendorID = invoice.VendorID;
                    adj.AdjdBranchID = invoice.BranchID;                   
                    adj.AdjdDocDate = invoice.DocDate;
                    adj.AdjdFinPeriodID = invoice.FinPeriodID;
                    adj.AdjgCuryInfoID = ApplicationDetails.Current.CuryInfoID;
                    adj.AdjdCuryInfoID = info_copy.CuryInfoID;
                    adj.AdjdOrigCuryInfoID = info.CuryInfoID;
                    adj.AdjgDocDate = ApplicationDetails.Current.TranDate;
                    adj.AdjdAPAcct = invoice.APAccountID;
                    adj.AdjdAPSub = invoice.APSubID;
                    adj.AdjgBalSign = (this.ApplicationDetails.Current.DocType == APDocType.Check && this.ApplicationDetails.Current.DocType == APDocType.DebitAdj || this.ApplicationDetails.Current.DocType == APDocType.VoidCheck && this.ApplicationDetails.Current.DocType == APDocType.DebitAdj ? -1m : 1m);

                    APAdjust adjustment = new APAdjust();
                    adjustment.AdjdRefNbr = adj.AdjdRefNbr;
                    adjustment.AdjdDocType = adj.AdjdDocType;
                    adjustment.AdjdAPAcct = invoice.APAccountID;
                    adjustment.AdjdAPSub = invoice.APSubID;
                    CopyToAdjust(adjustment, adj);

                    if (ApplicationDetails.Current.DrCr == CADrCr.CACredit)
                    {
                        adjustment.AdjgDocType = APDocType.Prepayment;
                    }
                    else
                    {
                        adjustment.AdjgDocType = APDocType.Refund;
                    }                   


                    APPaymentEntry.CalcBalances<APInvoice>(CurrencyInfo_CuryInfoID, adjustment, invoice, false, true);

                    decimal? CuryApplDiscAmt = (adjustment.AdjgDocType == APDocType.DebitAdj) ? 0m : adjustment.CuryDiscBal;
                    decimal? CuryApplAmt = adjustment.CuryDocBal - adjustment.CuryWhTaxBal - CuryApplDiscAmt;
                    decimal? CuryUnappliedBal = ApplicationDetails.Current.CuryUnappliedBal;

                    if (ApplicationDetails.Current != null && adjustment.AdjgBalSign < 0m)
                    {
                        if (CuryUnappliedBal < 0m)
                        {
                            CuryApplAmt = Math.Min((decimal)CuryApplAmt, Math.Abs((decimal)CuryUnappliedBal));
                        }
                    }
                    else if (ApplicationDetails.Current != null && CuryUnappliedBal > 0m && adjustment.AdjgBalSign > 0m && CuryUnappliedBal < CuryApplDiscAmt)
                    {
                        CuryApplAmt = CuryUnappliedBal;
                        CuryApplDiscAmt = 0m;
                    }
                    else if (ApplicationDetails.Current != null && CuryUnappliedBal > 0m && adj.AdjgBalSign > 0m)
                    {
                        CuryApplAmt = Math.Min((decimal)CuryApplAmt, (decimal)CuryUnappliedBal);
                    }
                    else if (ApplicationDetails.Current != null && CuryUnappliedBal <= 0m && ApplicationDetails.Current.CuryOrigDocAmt > 0) 
                    {
                        CuryApplAmt = 0m;
                    }

                    adjustment.CuryAdjgAmt = CuryApplAmt;
                    adjustment.CuryAdjgDiscAmt = CuryApplDiscAmt;
                    adjustment.CuryAdjgWhTaxAmt = adjustment.CuryWhTaxBal;

                    APPaymentEntry.CalcBalances<APInvoice>(CurrencyInfo_CuryInfoID, adjustment, invoice, true, true);


                    CopyToBankAdjustment(adj, adjustment);
                    adj.AdjdCuryRate = adjustment.AdjdCuryRate;
                    sender.SetDefaultExt<CABankStatementAdjustment.adjdTranPeriodID>(e.Row);

                }
            }
            else if (ApplicationDetails.Current.OrigModule == GL.BatchModule.AR)
            {
                foreach (PXResult<ARInvoice, CurrencyInfo> res in PXSelectJoin<ARInvoice, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARInvoice.curyInfoID>>>, Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>.Select(this, adj.AdjdDocType, adj.AdjdRefNbr))
                {
                    CurrencyInfo info_copy = PXCache<CurrencyInfo>.CreateCopy((CurrencyInfo)res);
			        info_copy.CuryInfoID = null;
			        info_copy = (CurrencyInfo)currencyinfo.Cache.Insert(info_copy);
			        ARInvoice invoice = (ARInvoice)res;
                    info_copy.SetCuryEffDate(currencyinfo.Cache, ApplicationDetails.Current.TranDate);


                    adj.VendorID = invoice.CustomerID;
                    adj.AdjgCuryInfoID = ApplicationDetails.Current.CuryInfoID;
                    adj.AdjdCuryInfoID = info_copy.CuryInfoID;
                    adj.AdjdOrigCuryInfoID = invoice.CuryInfoID;
                    adj.AdjdBranchID = invoice.BranchID;
                    adj.AdjdDocDate = invoice.DocDate;
                    adj.AdjdFinPeriodID = invoice.FinPeriodID;                    
                    adj.AdjdARAcct = invoice.ARAccountID;
                    adj.AdjdARSub = invoice.ARSubID;
                    adj.AdjgBalSign = (this.ApplicationDetails.Current.DocType == ARDocType.Payment && this.ApplicationDetails.Current.DocType == ARDocType.CreditMemo || this.ApplicationDetails.Current.DocType == ARDocType.VoidPayment && this.ApplicationDetails.Current.DocType == ARDocType.CreditMemo ? -1m : 1m);

                    ARAdjust adjustment = new ARAdjust();                    
                    adjustment.AdjdRefNbr = adj.AdjdRefNbr;
                    adjustment.AdjdDocType = adj.AdjdDocType;
                    adjustment.AdjdARAcct = invoice.ARAccountID;
                    adjustment.AdjdARSub = invoice.ARSubID;
                    CopyToAdjust(adjustment, adj); 

                    CalculateBalancesAR(adjustment, invoice, false, true);

                    decimal? CuryApplAmt = adjustment.CuryDocBal - adjustment.CuryDiscBal;
                    decimal? CuryApplDiscAmt = adjustment.CuryDiscBal;
                    decimal? CuryUnappliedBal = ApplicationDetails.Current.CuryUnappliedBal;


                    if (ApplicationDetails.Current != null && adjustment.AdjgBalSign < 0m)
                    {
                        if (CuryUnappliedBal < 0m)
                        {
                            CuryApplAmt = Math.Min((decimal)CuryApplAmt, Math.Abs((decimal)CuryUnappliedBal));
                        }
                    }
                    else if (ApplicationDetails.Current != null && CuryUnappliedBal > 0m && adjustment.AdjgBalSign > 0m)
                    {
                        CuryApplAmt = Math.Min((decimal)CuryApplAmt, (decimal)CuryUnappliedBal);

                        if (CuryApplAmt + CuryApplDiscAmt < adjustment.CuryDocBal)
                        {
                            CuryApplDiscAmt = 0m;
                        }
                    }
                    else if (ApplicationDetails.Current != null && CuryUnappliedBal <= 0m && ((CABankStatementDetail)ApplicationDetails.Current).CuryOrigDocAmt > 0)
                    {
                        CuryApplAmt = 0m;
                        CuryApplDiscAmt = 0m;
                    }

                    adjustment.CuryAdjgAmt = CuryApplAmt;
                    adjustment.CuryAdjgDiscAmt = CuryApplDiscAmt;
                    adjustment.CuryAdjgWOAmt = 0m;

                    CalculateBalancesAR(adjustment, invoice, true, true);

                    CopyToBankAdjustment(adj, adjustment);
                    sender.SetDefaultExt<CABankStatementAdjustment.adjdTranPeriodID>(e.Row);
                }
            }
      
        }

        protected virtual void CABankStatementAdjustment_AdjdCuryRate_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if ((decimal)e.NewValue <= 0m)
            {
                throw new PXSetPropertyException(CS.Messages.Entry_GT, ((int)0).ToString());
            }
        }

        protected virtual void CABankStatementAdjustment_AdjdCuryRate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            CABankStatementAdjustment adj = (CABankStatementAdjustment)e.Row;

            CurrencyInfo pay_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CABankStatementAdjustment.adjgCuryInfoID>>>>.SelectSingleBound(this, new object[] { e.Row });
            CurrencyInfo vouch_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Current<CABankStatementAdjustment.adjdCuryInfoID>>>>.SelectSingleBound(this, new object[] { e.Row });

            if (string.Equals(pay_info.CuryID, vouch_info.CuryID) && adj.AdjdCuryRate != 1m)
            {
                adj.AdjdCuryRate = 1m;
                vouch_info.SetCuryEffDate(currencyinfo.Cache, ApplicationDetails.Current.TranDate);
            }
            else if (string.Equals(vouch_info.CuryID, vouch_info.BaseCuryID))
            {
                adj.AdjdCuryRate = pay_info.CuryMultDiv == "M" ? 1 / pay_info.CuryRate : pay_info.CuryRate;
            }
            else
            {
                vouch_info.CuryRate = Math.Round((decimal)adj.AdjdCuryRate * (pay_info.CuryMultDiv == "M" ? (decimal)pay_info.CuryRate : 1m / (decimal)pay_info.CuryRate), 8, MidpointRounding.AwayFromZero);
                vouch_info.RecipRate = Math.Round((pay_info.CuryMultDiv == "M" ? 1m / (decimal)pay_info.CuryRate : (decimal)pay_info.CuryRate) / (decimal)adj.AdjdCuryRate, 8, MidpointRounding.AwayFromZero);
                vouch_info.CuryMultDiv = "M";
            }

            if (Caches[typeof(CurrencyInfo)].GetStatus(vouch_info) == PXEntryStatus.Notchanged)
            {
                Caches[typeof(CurrencyInfo)].SetStatus(vouch_info, PXEntryStatus.Updated);
            }
            UpdateBalance((CABankStatementAdjustment)e.Row, true);
        }

        protected virtual void CABankStatementAdjustment_CuryAdjgAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            CABankStatementAdjustment adj = (CABankStatementAdjustment)e.Row;

            if (adj.CuryDocBal == null || adj.CuryDiscBal == null || adj.CuryWhTaxBal == null) 
            {
                UpdateBalance((CABankStatementAdjustment)e.Row, false);
            }

            if (adj.CuryDocBal == null)
            {
                throw new PXSetPropertyException<CABankStatementAdjustment.adjdRefNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CABankStatementAdjustment.adjdRefNbr>(sender));
            }

            if (adj.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
            {
                throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
            }

            if (adj.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
            {
                throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
            }

            if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgAmt - (decimal)e.NewValue < 0)
            {
                throw new PXSetPropertyException(AP.Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgAmt).ToString());
            }
        }

        protected virtual void CABankStatementAdjustment_CuryAdjgAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.OldValue != null && ((CABankStatementAdjustment)e.Row).CuryDocBal == 0m && ((CABankStatementAdjustment)e.Row).CuryAdjgAmt < (decimal)e.OldValue)
            {
                ((CABankStatementAdjustment)e.Row).CuryAdjgDiscAmt = 0m;
            }
            UpdateBalance((CABankStatementAdjustment)e.Row, true);
        }

        protected virtual void CABankStatementAdjustment_CuryAdjgDiscAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            CABankStatementAdjustment adj = (CABankStatementAdjustment)e.Row;

            if (adj.CuryDocBal == null || adj.CuryDiscBal == null || adj.CuryWhTaxBal == null)
            {
                UpdateBalance((CABankStatementAdjustment)e.Row, false);
            }

            if (adj.CuryDocBal == null || adj.CuryDiscBal == null)
            {
                throw new PXSetPropertyException<CABankStatementAdjustment.adjdRefNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CABankStatementAdjustment.adjdRefNbr>(sender));
            }

            if (adj.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
            {
                throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
            }

            if (adj.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
            {
                throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
            }

            if ((decimal)adj.CuryDiscBal + (decimal)adj.CuryAdjgDiscAmt - (decimal)e.NewValue < 0)
            {
                throw new PXSetPropertyException(AP.Messages.Entry_LE, ((decimal)adj.CuryDiscBal + (decimal)adj.CuryAdjgDiscAmt).ToString());
            }

            if (adj.CuryAdjgAmt != null && (sender.GetValuePending<CABankStatementAdjustment.curyAdjgAmt>(e.Row) == PXCache.NotSetValue || (Decimal?)sender.GetValuePending<CABankStatementAdjustment.curyAdjgAmt>(e.Row) == adj.CuryAdjgAmt))
            {
                if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgDiscAmt - (decimal)e.NewValue < 0)
                {
                    throw new PXSetPropertyException(AP.Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgDiscAmt).ToString());
                }
            }
        }

        protected virtual void CABankStatementAdjustment_CuryAdjgDiscAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            UpdateBalance((CABankStatementAdjustment)e.Row, true);
        }

        protected virtual void CABankStatementAdjustment_CuryAdjgWhTaxAmt_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            CABankStatementAdjustment adj = (CABankStatementAdjustment)e.Row;

            if (adj.CuryDocBal == null || adj.CuryDiscBal == null || adj.CuryWhTaxBal == null)
            {
                UpdateBalance((CABankStatementAdjustment)e.Row, false);
            }

            if (adj.CuryDocBal == null || adj.CuryWhTaxBal == null)
            {
                throw new PXSetPropertyException<CABankStatementAdjustment.adjdRefNbr>(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<CABankStatementAdjustment.adjdRefNbr>(sender));
            }

            if (adj.VoidAdjNbr == null && (decimal)e.NewValue < 0m)
            {
                throw new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString());
            }

            if (adj.VoidAdjNbr != null && (decimal)e.NewValue > 0m)
            {
                throw new PXSetPropertyException(CS.Messages.Entry_LE, ((int)0).ToString());
            }

            if ((decimal)adj.CuryWhTaxBal + (decimal)adj.CuryAdjgWhTaxAmt - (decimal)e.NewValue < 0)
            {
                throw new PXSetPropertyException(AP.Messages.Entry_LE, ((decimal)adj.CuryWhTaxBal + (decimal)adj.CuryAdjgWhTaxAmt).ToString());
            }

            if (adj.CuryAdjgAmt != null && (sender.GetValuePending<CABankStatementAdjustment.curyAdjgAmt>(e.Row) == PXCache.NotSetValue || (Decimal?)sender.GetValuePending<CABankStatementAdjustment.curyAdjgAmt>(e.Row) == adj.CuryAdjgAmt))
            {
                if ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgWhTaxAmt - (decimal)e.NewValue < 0)
                {
                    throw new PXSetPropertyException(AP.Messages.Entry_LE, ((decimal)adj.CuryDocBal + (decimal)adj.CuryAdjgWhTaxAmt).ToString());
                }
            }
        }
        private bool internalCall = false;

        protected virtual void CABankStatementAdjustment_CuryDocBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (!internalCall)
            {
                if (e.Row != null && ((CABankStatementAdjustment)e.Row).AdjdCuryInfoID != null && ((CABankStatementAdjustment)e.Row).CuryDocBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
                {
                    UpdateBalance((CABankStatementAdjustment)e.Row, false);
                }
                if (e.Row != null)
                {
                    e.NewValue = ((CABankStatementAdjustment)e.Row).CuryDocBal;
                }                
            }
            e.Cancel = true;
        }

        protected virtual void CABankStatementAdjustment_CuryDiscBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (!internalCall)
            {
                if (e.Row != null && ((CABankStatementAdjustment)e.Row).AdjdCuryInfoID != null && ((CABankStatementAdjustment)e.Row).CuryDiscBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
                {
                    UpdateBalance((CABankStatementAdjustment)e.Row, false);
                }
                if (e.Row != null)
                {
                    e.NewValue = ((CABankStatementAdjustment)e.Row).CuryDiscBal;
                }
            }
            e.Cancel = true;
        }

        protected virtual void CABankStatementAdjustment_CuryWhTaxBal_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
        {
            if (!internalCall)
            {

                if (e.Row != null && ((CABankStatementAdjustment)e.Row).AdjdCuryInfoID != null && ((CABankStatementAdjustment)e.Row).CuryWhTaxBal == null && sender.GetStatus(e.Row) != PXEntryStatus.Deleted)
                {
                    UpdateBalance((CABankStatementAdjustment)e.Row, false);
                }
                if (e.Row != null)
                {
                    e.NewValue = ((CABankStatementAdjustment)e.Row).CuryWhTaxBal;
                }
            }

            e.Cancel = true;
        }

        protected virtual void CABankStatementAdjustment_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            CABankStatementAdjustment row = (CABankStatementAdjustment)e.Row;
            if (row != null)
            { 
                foreach (CABankStatementAdjustment adjustmentRecord in this.Adjustments.Select())
                {
                    if (row.AdjdRefNbr != null && adjustmentRecord.AdjdRefNbr == row.AdjdRefNbr && adjustmentRecord.AdjdDocType == row.AdjdDocType)
                    {
                        PXEntryStatus status = this.Adjustments.Cache.GetStatus(adjustmentRecord);
                        if (!(status == PXEntryStatus.InsertedDeleted || status == PXEntryStatus.Deleted))
                        {
                            sender.RaiseExceptionHandling<CABankStatementAdjustment.adjdRefNbr>(e.Row, null, new PXException(Messages.DuplicatedKeyForRow));
                            e.Cancel = true;
                            break;
                        }
                    }
                }
            }
        }

        protected virtual void CABankStatementAdjustment_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            CABankStatementAdjustment row = (CABankStatementAdjustment)e.Row;
            //if (row != null)
            //{
            //    foreach (CABankStatementAdjustment adjustmentRecord in this.Adjustments.Select())
            //    {
            //        if (row.AdjdRefNbr != null && adjustmentRecord.AdjdRefNbr == row.AdjdRefNbr && adjustmentRecord.AdjdDocType == row.AdjdDocType)
            //        {
            //            PXEntryStatus status = this.Adjustments.Cache.GetStatus(adjustmentRecord);
            //            if (!(status == PXEntryStatus.InsertedDeleted || status == PXEntryStatus.Deleted))
            //            {
            //                sender.RaiseExceptionHandling<CABankStatementAdjustment.adjdRefNbr>(e.Row, null, new PXException(Messages.DuplicatedKeyForRow));
            //                e.Cancel = true;
            //                break;
            //            }
            //        }
            //    }
            //}
        }

        #endregion

        #region CAApplicationStatementDetail Events
        protected virtual void CAApplicationStatementDetail_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            CAApplicationStatementDetail row = (CAApplicationStatementDetail)e.Row;
			CABankStatement doc = this.BankStatement.Current;
            if (doc != null && row != null && doc.Released == false)  
            {
                SetDocTypeList(e.Row);
                if (row.OrigModule == GL.BatchModule.AR)
                {
                    PXUIFieldAttribute.SetEnabled<CABankStatementAdjustment.adjdCuryRate>(Adjustments.Cache, null, false);
                }
                else
                {
                    PXUIFieldAttribute.SetEnabled<CABankStatementAdjustment.adjdCuryRate>(Adjustments.Cache, null, true);
                }
            }
            if (row != null)
            {
                if (row.CATranID.HasValue || !String.IsNullOrEmpty(row.InvoiceInfo))
                {
                    this.Adjustments.Cache.AllowInsert = false;
                    this.Adjustments.Cache.AllowUpdate = false;
                    this.Adjustments.Cache.AllowDelete = false;
                }
                else
                {
                    this.Adjustments.Cache.AllowInsert = true;
                    this.Adjustments.Cache.AllowUpdate = true;
                    this.Adjustments.Cache.AllowDelete = true;
                }
            }
            else
            {
                this.Adjustments.Cache.AllowInsert = false;
                this.Adjustments.Cache.AllowUpdate = false;
            }
        }
        protected virtual void CAApplicationStatementDetail_RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
        {
			if (e.Row != null)
			{
				CAApplicationStatementDetail row = (CAApplicationStatementDetail)e.Row;
				if (row != null && row.CuryApplAmt == null)
				{
					RecalcApplAmounts(sender, row);
				}
			}
        }
        protected virtual void CAApplicationStatementDetail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            //CAApplicationStatementDetail row = (CAApplicationStatementDetail)e.Row;
            //CAApplicationStatementDetail oldRow = (CAApplicationStatementDetail)e.OldRow;
            //if (row != null && row.LineCntr != oldRow.LineCntr)
            //{
            //    CABankStatementDetail src = (CABankStatementDetail)this.Details.Search<CABankStatementDetail.refNbr, CABankStatementDetail.lineNbr>(row.RefNbr, row.LineNbr);
            //    if (src != null)
            //    {
            //        CABankStatementDetail copy = (CABankStatementDetail)this.Details.Cache.CreateCopy(src);
            //        copy.LineCntr = row.LineCntr;
            //        try
            //        {
            //            this._isCacheSync = true;
            //            this.Details.Update(copy);
            //        }
            //        finally
            //        {
            //            this._isCacheSync = false;
            //        }
            //    }
            //}
        }
        protected virtual void CAApplicationStatementDetail_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            e.Cancel = true;
        }
        #endregion

        #region CurrencyInfo
        protected virtual void CurrencyInfo_CuryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CMSetup.Current.MCActivated == true)
			{
				if ((cashaccount.Current != null) && !string.IsNullOrEmpty(cashaccount.Current.CuryID))
				{
					e.NewValue = cashaccount.Current.CuryID;
					e.Cancel = true;
				}
			}
		}

        protected virtual void CurrencyInfo_CuryRateTypeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (CMSetup.Current.MCActivated == true)
			{
				if (cashaccount.Current != null && !string.IsNullOrEmpty(cashaccount.Current.CuryRateTypeID))
				{
					e.NewValue = cashaccount.Current.CuryRateTypeID;
					e.Cancel = true;
				}
			}
		}

		protected virtual void CurrencyInfo_CuryEffDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (this.ApplicationDetails.Current != null)
			{
                e.NewValue = this.ApplicationDetails.Current.TranDate;
				e.Cancel = true;
			}
		}

        protected virtual void CurrencyInfo_RowInserting(PXCache sender, PXRowInsertingEventArgs e) 
        {
            CurrencyInfo row = e.Row as CurrencyInfo;
        }

        protected virtual void CurrencyInfo_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            CurrencyInfo row = e.Row as CurrencyInfo;
        }
	
        #endregion
        #endregion

        #region Release-Related Functions
        public static void ReleaseDoc(List<CABankStatement> list)
		{
			foreach (CABankStatement iDoc in list)
			{
				CABankStatementEntry graph = PXGraph.CreateInstance<CABankStatementEntry>();
				CABankStatement row = graph.BankStatement.Search<CABankStatement.refNbr>(iDoc.RefNbr, iDoc.CashAccountID);
                CASetup casetup = graph.casetup.Current; 
				graph.BankStatement.Current = row;
				graph.DetailMatches.Cache.AllowUpdate = true;
				if (row.Hold == true)
					throw new PXException(Messages.DocumentOnHoldCanNotBeReleased);
                List<CATran> needsRelease = new List<CATran>(); 
				foreach (PXResult<CABankStatementDetail, CATran> iDet in PXSelectReadonly2<CABankStatementDetail, 
						    LeftJoin<CATran,On<CATran.cashAccountID,Equal<CABankStatementDetail.cashAccountID>,
							    And<CATran.tranID, Equal<CABankStatementDetail.cATranID>>>>,
							        Where<CABankStatementDetail.refNbr,Equal<Required<CABankStatement.refNbr>>>>.Select(graph, row.RefNbr))
				{
					CABankStatementDetail detail = iDet;
					CATran catran = iDet;
					if (detail.CATranID == null)
						throw new PXException(Messages.StatementCanNotBeReleasedThereAreUnmatchedDetails);

					if (catran == null || catran.TranID == null)
						throw new PXException(Messages.StatementCanNotBeReleasedSomeDetailsMatchedDeletedDocument);

                    if (catran.Released != true)
                    {
                         catran.Selected = true;
                         needsRelease.Add(catran);                        
                    }					
				}				
                if (needsRelease.Count > 0) 
                {
                    CATrxRelease.GroupReleaseTransaction(needsRelease, casetup.ReleaseAP.Value, casetup.ReleaseAR.Value, true);
                }
				
				foreach (PXResult<CABankStatementDetail, CATran> iDet in PXSelectReadonly2<CABankStatementDetail,
							InnerJoin<CATran, On<CATran.cashAccountID, Equal<CABankStatementDetail.cashAccountID>,
								And<CATran.tranID, Equal<CABankStatementDetail.cATranID>>>>,
									Where<CABankStatementDetail.refNbr, Equal<Required<CABankStatement.refNbr>>>>.Select(graph, row.RefNbr))
				{					
					CATran catran = iDet;					
					if (catran.Released == true && catran.Cleared == false)
					{
						graph.UpdateSourceDoc(catran);
					}
				}
				CABankStatement copy = (CABankStatement)graph.BankStatement.Cache.CreateCopy(row);
				copy.Released = true;
				graph.BankStatement.Update(copy);
				graph.Save.Press();
			}
		}

		protected virtual void UpdateSourceDoc(CATran aTran)
		{
			aTran.Cleared = true;
			aTran.ClearDate = Accessinfo.BusinessDate;			
			this.Caches[typeof(CATran)].Update(aTran);
			switch (aTran.OrigModule) 
			{
				case GL.BatchModule.AP:
                    if (aTran.OrigTranType != CAAPARTranType.GLEntry)
					{
						APPayment payment = PXSelect<APPayment, Where<APPayment.docType, Equal<Required<APPayment.docType>>,
											And<APPayment.refNbr, Equal<Required<APPayment.refNbr>>>>>.Select(this, aTran.OrigTranType, aTran.OrigRefNbr);
						payment.ClearDate = aTran.ClearDate;
						payment.Cleared = aTran.Cleared;
						this.Caches[typeof(APPayment)].Update(payment);
					}
					break;
				case GL.BatchModule.AR:
                    if (aTran.OrigTranType != CAAPARTranType.GLEntry)
                    {
						ARPayment payment = PXSelect<ARPayment, Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
											And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>.Select(this, aTran.OrigTranType, aTran.OrigRefNbr);
						payment.ClearDate = aTran.ClearDate;
						payment.Cleared = aTran.Cleared;
						this.Caches[typeof(ARPayment)].Update(payment);
					}
					break;
				case GL.BatchModule.CA:
					{
						if (aTran.OrigTranType == CA.CATranType.CADeposit)
						{
							CATran notClearedTran = PXSelect<CATran, Where<CATran.origModule, Equal<CATran.origModule>,
																And<CATran.origTranType, Equal<CATran.origTranType>,
																And<CATran.origRefNbr, Equal<CATran.origRefNbr>,
																And<CATran.cleared, Equal<False>,
																And<CATran.tranID, NotEqual<CATran.tranID>>>>>>>.Select(this, aTran.OrigModule, aTran.OrigTranType, aTran.OrigRefNbr, aTran.TranID);
							if(notClearedTran == null)
							{
								CADeposit payment = PXSelect<CADeposit, Where<CADeposit.tranType, Equal<Required<CADeposit.tranType>>,
											And<CADeposit.refNbr, Equal<Required<CADeposit.refNbr>>>>>.Select(this, aTran.OrigTranType, aTran.OrigRefNbr);

								payment.ClearDate = aTran.ClearDate;
								payment.Cleared = aTran.Cleared;
								this.Caches[typeof(CADeposit)].Update(payment);
							}
						}

						if (aTran.OrigTranType == CA.CATranType.CAAdjustment)
						{
							CATran notClearedTran = PXSelect<CATran, Where<CATran.origModule, Equal<CATran.origModule>,
																And<CATran.origTranType, Equal<CATran.origTranType>,
																And<CATran.origRefNbr, Equal<CATran.origRefNbr>,
																And<CATran.cleared, Equal<False>,
																And<CATran.tranID, NotEqual<CATran.tranID>>>>>>>.Select(this, aTran.OrigModule, aTran.OrigTranType, aTran.OrigRefNbr, aTran.TranID);
							if (notClearedTran == null)
							{
								CAAdj payment = PXSelect<CAAdj, Where<CAAdj.adjTranType, Equal<Required<CAAdj.adjTranType>>,
												And<CAAdj.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>>>>.Select(this, aTran.OrigTranType, aTran.OrigRefNbr);

								payment.ClearDate = aTran.ClearDate;
								payment.Cleared = aTran.Cleared;
								this.Caches[typeof(CAAdj)].Update(payment);
							}
						}

						if (aTran.OrigTranType == CA.CATranType.CATransferIn || aTran.OrigTranType == CA.CATranType.CATransferOut)
						{
							CATransfer payment = PXSelect<CATransfer, Where<CATransfer.transferNbr, Equal<Required<CATransfer.transferNbr>>>>.Select(this, aTran.OrigRefNbr);

							if (payment.TranIDIn == aTran.TranID)
							{
								payment.ClearDateIn = aTran.ClearDate;
								payment.ClearedIn = aTran.Cleared;
							}

							if (payment.TranIDOut == aTran.TranID)
							{
								payment.ClearDateOut = aTran.ClearDate;
								payment.ClearedOut = aTran.Cleared;
							}
							this.Caches[typeof(CATransfer)].Update(payment);
						}
					}
					break;
			}
		} 
		#endregion

		#region Create Document Functions
		protected virtual void CreateDocumentProc(CABankStatementDetail aRow, bool doPersist)
		{
			CATran result = null;
			PXCache sender = this.Details.Cache;
			if (aRow.CATranID != null)
			{
				sender.RaiseExceptionHandling<CABankStatementDetail.createDocument>(aRow, aRow.PayeeBAccountID, new PXSetPropertyException(Messages.DocumentIsAlreadyCreatedForThisDetail));
				return;
			}
			bool hasError = false;
			if (aRow.BAccountID == null && aRow.OrigModule != GL.BatchModule.CA)
			{
				sender.RaiseExceptionHandling<CABankStatementDetail.payeeBAccountID>(aRow, aRow.PayeeBAccountID, new PXSetPropertyException(Messages.PayeeIsRequiredToCreateAP_ARDocument));
				hasError = true;
			}

			if (aRow.LocationID == null && aRow.OrigModule != GL.BatchModule.CA)
			{
				sender.RaiseExceptionHandling<CABankStatementDetail.payeeBAccountID>(aRow, aRow.PayeeLocationID, new PXSetPropertyException(Messages.PayeeLocationIsRequiredToCreateAP_ARDocument));
				hasError = true;
			}

			if (aRow.OrigModule == GL.BatchModule.AR)                
			{
                if(string.IsNullOrEmpty(aRow.PaymentMethodID))
                {
                    sender.RaiseExceptionHandling<CABankStatementDetail.paymentMethodID>(aRow, aRow.PMInstanceID, new PXSetPropertyException(Messages.PaymentMethodIsRequiredToCreateARDocument));
				    hasError = true;
                }
                if (!hasError && aRow.PMInstanceID == null)
                {
                    PaymentMethod pm = PXSelect<PaymentMethod, Where<PaymentMethod.paymentMethodID, Equal<Required<PaymentMethod.paymentMethodID>>>>.Select(this, aRow.PaymentMethodID);
                    if (pm.IsAccountNumberRequired == true)
                    {
                        sender.RaiseExceptionHandling<CABankStatementDetail.pMInstanceID>(aRow, aRow.PMInstanceID, new PXSetPropertyException(Messages.PaymentMethodIsRequiredToCreateARDocument));
                        hasError = true;
                    }
                }
			}

			if (aRow.OrigModule == GL.BatchModule.AP && string.IsNullOrEmpty(aRow.PaymentMethodID))
			{
				sender.RaiseExceptionHandling<CABankStatementDetail.paymentMethodID>(aRow, aRow.PaymentMethodID, new PXSetPropertyException(Messages.PaymentTypeIsRequiredToCreateAPDocument));
				hasError = true;
			}

            List<ICADocAdjust> adjustments = new List<ICADocAdjust>();
			if (IsAPInvoiceSearchNeeded(aRow)) 
			{
				PXResult<APInvoice, APAdjust, APPayment> invResult = FindAPInvoiceByInvoiceInfo(aRow);
				if (invResult != null)
				{
					APInvoice invoiceToApply = invResult;
					APAdjust unreleasedAjustment = invResult;
					APPayment invoicePrepayment = invResult;
					if (invoiceToApply.Released == false && invoiceToApply.Prebooked == false)
					{
						sender.RaiseExceptionHandling<CABankStatementDetail.invoiceInfo>(aRow, aRow.InvoiceInfo, new PXSetPropertyException(Messages.APPaymentApplicationInvoiceIsNotReleased, aRow.InvoiceInfo));
						hasError = true;
					}

					if (invoiceToApply.OpenDoc == false)
					{
						sender.RaiseExceptionHandling<CABankStatementDetail.invoiceInfo>(aRow, aRow.InvoiceInfo, new PXSetPropertyException(Messages.APPaymentApplicationInvoiceIsClosed, aRow.InvoiceInfo));
						hasError = true;
					}

					if (invoiceToApply.DocDate > aRow.TranDate)
					{
						sender.RaiseExceptionHandling<CABankStatementDetail.invoiceInfo>(aRow, aRow.InvoiceInfo, new PXSetPropertyException(Messages.APPaymentApplicationInvoiceDateIsGreaterThenPaymentDate, aRow.InvoiceInfo));
						hasError = true;
					}

					if (unreleasedAjustment != null && string.IsNullOrEmpty(unreleasedAjustment.AdjgRefNbr) == false)
					{
						sender.RaiseExceptionHandling<CABankStatementDetail.invoiceInfo>(aRow, aRow.InvoiceInfo, new PXSetPropertyException(Messages.APPaymentApplicationInvoiceUnrealeasedApplicationExist, aRow.InvoiceInfo));
						hasError = true;
					}

					if (aRow.DrCr == CADrCr.CACredit && invoicePrepayment != null && string.IsNullOrEmpty(invoicePrepayment.RefNbr) == false)
					{
						sender.RaiseExceptionHandling<CABankStatementDetail.invoiceInfo>(aRow, aRow.InvoiceInfo, new PXSetPropertyException(Messages.APPaymentApplicationInvoiceIsPartOfPrepaymentOrDebitAdjustment, aRow.InvoiceInfo));
						hasError = true;
					}

					if (!hasError)
					{						
						AdjustInfo adj = new AdjustInfo();
						adj.Copy(invoiceToApply);
						adjustments.Add(adj);
					}
				}
				else
				{
					sender.RaiseExceptionHandling<CABankStatementDetail.invoiceInfo>(aRow, aRow.InvoiceInfo, new PXSetPropertyException(Messages.APPaymentApplicationInvoiceIsNotFound, aRow.InvoiceInfo, PXErrorLevel.RowWarning));
				}
			}

			if (IsARInvoiceSearchNeeded(aRow))
			{
				PXResult<ARInvoice, ARAdjust> invResult = FindARInvoiceByInvoiceInfo(aRow);
				if (invResult != null)
				{
					ARInvoice invoiceToApply = invResult;
					ARAdjust unreleasedAjustment = invResult;
					if (invoiceToApply.Released == false)
					{
						sender.RaiseExceptionHandling<CABankStatementDetail.invoiceInfo>(aRow, aRow.InvoiceInfo, new PXSetPropertyException(Messages.ARPaymentApplicationInvoiceIsNotReleased, aRow.InvoiceInfo));
						hasError = true;
					}

					if (invoiceToApply.OpenDoc == false)
					{
						sender.RaiseExceptionHandling<CABankStatementDetail.invoiceInfo>(aRow, aRow.InvoiceInfo, new PXSetPropertyException(Messages.ARPaymentApplicationInvoiceIsClosed, aRow.InvoiceInfo));
						hasError = true;
					}

					if (invoiceToApply.DocDate > aRow.TranDate)
					{
						sender.RaiseExceptionHandling<CABankStatementDetail.invoiceInfo>(aRow, aRow.InvoiceInfo, new PXSetPropertyException(Messages.ARPaymentApplicationInvoiceDateIsGreaterThenPaymentDate, aRow.InvoiceInfo));
						hasError = true;
					}

					if (unreleasedAjustment != null && string.IsNullOrEmpty(unreleasedAjustment.AdjgRefNbr) == false)
					{
						sender.RaiseExceptionHandling<CABankStatementDetail.invoiceInfo>(aRow, aRow.InvoiceInfo, new PXSetPropertyException(Messages.ARPaymentApplicationInvoiceUnrealeasedApplicationExist, aRow.InvoiceInfo));
						hasError = true;
					}

					if (!hasError)
					{						
						AdjustInfo adj = new AdjustInfo();
						adj.Copy(invoiceToApply);
						adjustments.Add(adj);
					}
				}
				else
				{
					sender.RaiseExceptionHandling<CABankStatementDetail.invoiceInfo>(aRow, aRow.InvoiceInfo, new PXSetPropertyException(Messages.ARPaymentApplicationInvoiceIsNotFound, aRow.InvoiceInfo, PXErrorLevel.RowWarning));
				}
			}

            foreach (CABankStatementAdjustment adj in PXSelect<CABankStatementAdjustment,
                                                                    Where<CABankStatementAdjustment.refNbr, Equal<Required<CABankStatement.refNbr>>,
                                                                    And<CABankStatementAdjustment.lineNbr, Equal<Required<CABankStatementDetail.lineNbr>>>>>.Select(this, aRow.RefNbr, aRow.LineNbr))
            {
                AdjustInfo adjInfo = new AdjustInfo();
                adjInfo.AdjdRefNbr = adj.AdjdRefNbr;
                adjInfo.AdjdDocType = adj.AdjdDocType;
                adjInfo.CuryAdjgAmount = adj.CuryAdjgAmt;
                adjInfo.CuryAdjgDiscAmt = adj.CuryAdjgDiscAmt;
                adjInfo.CuryAdjgWhTaxAmt = adj.CuryAdjgWhTaxAmt;
                adjInfo.AdjdCuryRate = adj.AdjdCuryRate;
                adjustments.Add(adjInfo);
            }


			if (hasError)
				return;

			CABankStatement doc = this.BankStatement.Current;
			if (aRow.CashAccountID == null)
				aRow.CashAccountID = doc.CashAccountID;

			if (aRow.OrigModule == GL.BatchModule.AR)
			{
				bool OnHold = (this.casetup.Current.ReleaseAR == false);
				result = PaymentReclassifyProcess.AddARTransaction(aRow, null, adjustments, OnHold);
			}

			if (aRow.OrigModule == GL.BatchModule.AP)
			{
				bool OnHold = (this.casetup.Current.ReleaseAP == false);
				result = PaymentReclassifyProcess.AddAPTransaction(aRow, null, adjustments, OnHold);
			}

			if (aRow.OrigModule == GL.BatchModule.CA)
			{
				result = AddCATransaction(this, aRow, false);
			}

			if (result != null)
			{
				CABankStatementDetail copy = (CABankStatementDetail)sender.CreateCopy(aRow);
				copy.CATranID = result.TranID;
				copy.DocumentMatched = true;
				aRow = (CABankStatementDetail)sender.Update(copy);
                
			}

			if (doPersist)
				this.Save.Press();
		}
	
		protected virtual bool IsARInvoiceSearchNeeded(CABankStatementDetail aRow) 
		{
			return (aRow.OrigModule == GL.BatchModule.AR && String.IsNullOrEmpty(aRow.InvoiceInfo) == false);
		}
		
		protected virtual bool IsAPInvoiceSearchNeeded(CABankStatementDetail aRow)
		{
			return (aRow.OrigModule == GL.BatchModule.AP && String.IsNullOrEmpty(aRow.InvoiceInfo) == false); 
		}
		/// <summary>
		/// Searches in database AR invoices, based on the the information in CABankStatementDetail record.
		/// The field used for the search are  - BAccountID and InvoiceInfo. First it is searching a invoice by it RefNbr, 
		/// then (if not found) - by invoiceNbr. 
		/// </summary>
		/// <param name="aRow">parameters for the search. The field used for the search are  - BAccountID and InvoiceInfo.</param>
		///	<returns>Returns null if nothing is found and PXResult<ARInvoice,ARAdjust> in the case of success.
		///		ARAdjust record represents unreleased adjustment (payment), applied to this Invoice
		///	</returns>
		protected virtual PXResult<ARInvoice, ARAdjust> FindARInvoiceByInvoiceInfo(CABankStatementDetail aRow) 
		{
			PXResult<ARInvoice,ARAdjust> invResult = (PXResult<ARInvoice, ARAdjust>)PXSelectJoin<ARInvoice,
											   LeftJoin<ARAdjust, On<ARAdjust.adjdDocType, Equal<ARInvoice.docType>,
												  And<ARAdjust.adjdRefNbr, Equal<ARInvoice.refNbr>,
												  And<ARAdjust.released, Equal<boolFalse>>>>>,
										  Where<ARInvoice.docType, Equal<AR.ARInvoiceType.invoice>,
										  And<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>,
										  And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>>.Select(this, aRow.BAccountID, aRow.InvoiceInfo);


			if (invResult == null)
			{
				invResult = (PXResult<ARInvoice, ARAdjust>)PXSelectJoin<ARInvoice,
								   LeftJoin<ARAdjust, On<ARAdjust.adjdDocType, Equal<ARInvoice.docType>,
											   And<ARAdjust.adjdRefNbr, Equal<ARInvoice.refNbr>,
											   And<ARAdjust.released, Equal<boolFalse>>>>>,											   
								   Where<ARInvoice.docType, Equal<AR.ARInvoiceType.invoice>,
									And<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>,
									And<ARInvoice.invoiceNbr, Equal<Required<ARInvoice.invoiceNbr>>>>>>.Select(this, aRow.BAccountID, aRow.InvoiceInfo);

			}
			return invResult; 			
		}

		/// <summary>
		/// Searches in database AR invoices, based on the the information in the CABankStatementDetail record.
		/// The field used for the search are  - BAccountID and InvoiceInfo. First it is searching a invoice by it RefNbr, 
		/// then (if not found) - by invoiceNbr. 
		/// </summary>
		/// <param name="aRow">Parameters for the search. The field used for the search are  - BAccountID and InvoiceInfo.</param>
		/// <returns>Returns null if nothing is found and PXResult<APInvoice,APAdjust,APPayment> in the case of success.
		/// APAdjust record represents unreleased adjustment (payment), applied to this APInvoice</returns>
		protected virtual PXResult<APInvoice, APAdjust, APPayment> FindAPInvoiceByInvoiceInfo(CABankStatementDetail aRow)
		{

			PXResult<APInvoice, APAdjust, APPayment> invResult = (PXResult<APInvoice, APAdjust, APPayment>)PXSelectJoin<APInvoice,
													 LeftJoin<APAdjust, On<APAdjust.adjdDocType, Equal<APInvoice.docType>,
														And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>,
														And<APAdjust.released, Equal<boolFalse>>>>,
													LeftJoin<APPayment, On<APPayment.docType, Equal<APInvoice.docType>,
														And<APPayment.refNbr, Equal<APInvoice.refNbr>,
														And<Where<APPayment.docType, Equal<APDocType.prepayment>,
															Or<APPayment.docType, Equal<APDocType.debitAdj>>>>>>>>,
												Where<APInvoice.docType, Equal<AP.APInvoiceType.invoice>,
												And<APInvoice.vendorID, Equal<Required<APInvoice.vendorID>>,
												And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>>.Select(this, aRow.BAccountID, aRow.InvoiceInfo);


			if (invResult == null)
			{
				invResult = (PXResult<APInvoice, APAdjust, APPayment>)PXSelectJoin<APInvoice,
								   LeftJoin<APAdjust, On<APAdjust.adjdDocType, Equal<APInvoice.docType>,
											   And<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>,
											   And<APAdjust.released, Equal<boolFalse>>>>,
										   LeftJoin<APPayment, On<APPayment.docType, Equal<APInvoice.docType>,
											   And<APPayment.refNbr, Equal<APInvoice.refNbr>,
											   And<Where<APPayment.docType, Equal<APDocType.prepayment>,
												   Or<APPayment.docType, Equal<APDocType.debitAdj>>>>>>>>,
								   Where<APInvoice.docType, Equal<AP.APInvoiceType.invoice>,
									And<APInvoice.vendorID, Equal<Required<APInvoice.vendorID>>,
									And<APInvoice.invoiceNbr, Equal<Required<APInvoice.invoiceNbr>>>>>>.Select(this, aRow.BAccountID, aRow.InvoiceInfo);

			}
			return invResult;
		}

		public static CATran AddCATransaction(PXGraph graph, ICADocSource parameters, bool IsTransferExpense)
		{
			if (parameters.OrigModule == GL.BatchModule.CA)
			{
				if (parameters.CashAccountID == null)
				{
					//AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.cashAccountID>(parameters,
					//    null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.cashAccountID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.cashAccountID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.cashAccountID).Name);
				}

				if (string.IsNullOrEmpty(parameters.EntryTypeID))
				{
					//AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.entryTypeID>(parameters,
					//    null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.entryTypeID).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.entryTypeID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.entryTypeID).Name);
				}

				if (string.IsNullOrEmpty(parameters.ExtRefNbr))
				{
					//AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.extRefNbr>(parameters,
					//    null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.extRefNbr).Name));

					throw new PXRowPersistingException(typeof(AddTrxFilter.extRefNbr).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.extRefNbr).Name);
				}

				//if (parameters.AccountID == null)
				//{
				//    AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.accountID>(parameters,
				//        null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.accountID).Name));

				//    throw new PXRowPersistingException(typeof(AddTrxFilter.accountID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.accountID).Name);
				//}

				//if (parameters.SubID == null)
				//{
				//    AddFilter.Cache.RaiseExceptionHandling<AddTrxFilter.subID>(parameters,
				//        null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.subID).Name));

				//    throw new PXRowPersistingException(typeof(AddTrxFilter.subID).Name, null, ErrorMessages.FieldIsEmpty, typeof(AddTrxFilter.subID).Name);
				//}

				CATranEntry te = PXGraph.CreateInstance<CATranEntry>();
				CurrencyInfo info = new CurrencyInfo();
				CashAccount cashacct = (CashAccount)PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(graph, parameters.CashAccountID);
				if ((cashacct != null) && (cashacct.CuryRateTypeID != null))
				{
					info.CuryID = cashacct.CuryID;
					te.currencyinfo.Cache.RaiseFieldUpdated<CurrencyInfo.curyID>(info, null);

					info.CuryRateTypeID = cashacct.CuryRateTypeID;
					te.currencyinfo.Cache.RaiseFieldUpdated<CurrencyInfo.curyRateTypeID>(info, null);
					info = te.currencyinfo.Insert(info);
				}

				CAAdj adj = new CAAdj();
				adj.AdjTranType = (IsTransferExpense ? CATranType.CATransferExp : CATranType.CAAdjustment);
				if (IsTransferExpense)
				{
					adj.TransferNbr = (graph as CashTransferEntry).Transfer.Current.TransferNbr;
				}
				adj.CashAccountID = parameters.CashAccountID;
				adj.CuryID = parameters.CuryID;
				adj.CuryInfoID = info.CuryInfoID;
				adj.DrCr = parameters.DrCr;
				adj.ExtRefNbr = parameters.ExtRefNbr;
				adj.Released = false;
				adj.Cleared = parameters.Cleared;
				adj.TranDate = parameters.TranDate;
				adj.TranDesc = parameters.TranDesc;
				adj.EntryTypeID = parameters.EntryTypeID;
				adj.CuryControlAmt = parameters.CuryOrigDocAmt;
				adj.Hold = false;
				adj.TaxZoneID = null;
				adj = te.CAAdjRecords.Insert(adj);

				CASplit split = new CASplit(); 
				split.AdjTranType = adj.AdjTranType;
				//split.AccountID = parameters.AccountID;
				split.CuryInfoID = info.CuryInfoID;
				split.Qty = (decimal)1.0;
				split.CuryUnitPrice = parameters.CuryOrigDocAmt;
				split.CuryTranAmt = parameters.CuryOrigDocAmt;
				split.TranDesc = parameters.TranDesc;
				//split.SubID = parameters.SubID;
				te.CASplitRecords.Insert(split);

				te.Save.Press();
				adj = (CAAdj)te.Caches[typeof(CAAdj)].Current;
				return (CATran)PXSelect<CATran, Where<CATran.tranID, Equal<Required<CAAdj.tranID>>>>.Select(te, adj.TranID);
			}
			return null;
		}
		
		#endregion

		#region IPXPrepareItems Members

		public bool PrepareImportRow(string viewName, IDictionary keys, IDictionary values)
		{
			if (string.Compare(viewName, "Transactions", true) == 0)
			{				
				if (values.Contains("refNbr")) values["refNbr"] = this.BankStatement.Current.RefNbr;
				else values.Add("refNbr", this.BankStatement.Current.RefNbr);
				
			}
			return true;
		}

		public bool RowImporting(string viewName, object row)
		{
			return row == null;
		}

		public bool RowImported(string viewName, object row, object oldRow)
		{
			return oldRow == null;
		}

		public void PrepareItems(string viewName, IEnumerable items)
		{
			//throw new NotImplementedException();
		}

		#endregion
	}

	[Serializable]
	public class CAApplicationStatementDetail : CABankStatementDetail
	{
		#region CashAccountID
		public new abstract class cashAccountID : PX.Data.IBqlField { }
		#endregion
		#region RefNbr
		public new abstract class refNbr : PX.Data.IBqlField { }
		#endregion
		#region LineNbr
		public new abstract class lineNbr : PX.Data.IBqlField { }
		#endregion
		#region ExtRefNbr
		public new abstract class extRefNbr : PX.Data.IBqlField { }
		#endregion
		#region CuryInfoID
		public new abstract class curyInfoID : PX.Data.IBqlField
		{
		}

		[PXDBLong()]
		public override Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region TranDate
		public new abstract class tranDate : PX.Data.IBqlField { }
		#endregion
		#region TranCode
		public new abstract class tranCode : PX.Data.IBqlField { }
		#endregion
		#region TranDesc
		public new abstract class tranDesc : PX.Data.IBqlField { }
		#endregion
		#region OrigModule
		public new abstract class origModule : PX.Data.IBqlField { }
		#endregion
		#region PayeeBAccountID
		public new abstract class payeeBAccountID : PX.Data.IBqlField { }
		#endregion
		#region PayeeName
		public new abstract class payeeName : PX.Data.IBqlField { }
		#endregion
		#region PayeeLocationID
		public new abstract class payeeLocationID : PX.Data.IBqlField { }
		#endregion
		#region PaymentMethodID
		public new abstract class paymentMethodID : PX.Data.IBqlField { }
		#endregion
		#region PMInstanceID
		public new abstract class pMInstanceID : PX.Data.IBqlField { }
		#endregion
		#region InvoiceInfo
		public new abstract class invoiceInfo : PX.Data.IBqlField { }
		#endregion
		#region CuryTotalAmt
		public new abstract class curyTotalAmount : PX.Data.IBqlField { }
		#endregion
		#region CuryApplAmt
		public new abstract class curyApplAmt : PX.Data.IBqlField { }
		#endregion
		#region CuryUnappliedBal
		public new abstract class curyUnappliedBal : PX.Data.IBqlField { }
		#endregion
		#region DocType
		public new abstract class docType : PX.Data.IBqlField { }
		#endregion
		#region LineCntr
		public new abstract class lineCntr : PX.Data.IBqlField { }
		#endregion
		#region CATranID
		public new abstract class cATranID : PX.Data.IBqlField { }
		#endregion

		#region CuryDebitAmt
		public new abstract class curyDebitAmt : PX.Data.IBqlField
		{
		}

		[PXDecimal()]
		[PXUIField(DisplayName = "Receipt")]
		public override Decimal? CuryDebitAmt
		{
			[PXDependsOnFields(typeof(drCr), typeof(curyTranAmt))]
			get
			{
				return (this._DrCr == CADrCr.CADebit) ? this._CuryTranAmt : Decimal.Zero;
			}
			set
			{
				if (value != 0m)
				{
					this._CuryTranAmt = value;
					this._DrCr = CADrCr.CADebit;
				}
				else if (this._DrCr == CADrCr.CADebit)
				{
					this._CuryTranAmt = 0m;
				}
			}
		}
		#endregion
		#region CuryCreditAmt
		public new abstract class curyCreditAmt : PX.Data.IBqlField
		{
		}

		[PXDecimal()]
		[PXUIField(DisplayName = "Disbursement")]
		public override Decimal? CuryCreditAmt
		{
			[PXDependsOnFields(typeof(drCr), typeof(curyTranAmt))]
			get
			{
				return (this._DrCr == CADrCr.CACredit) ? -this._CuryTranAmt : Decimal.Zero;
			}
			set
			{
				if (value != 0m)
				{
					this._CuryTranAmt = -value;
					this._DrCr = CADrCr.CACredit;
				}
				else if (this._DrCr == CADrCr.CACredit)
				{
					this._CuryTranAmt = 0m;
				}
			}
		}
		#endregion

		#region CuryReconciledDebit
		public new abstract class curyReconciledDebit : PX.Data.IBqlField
		{
		}
		[PXDecimal()]
		public override Decimal? CuryReconciledDebit
		{
			[PXDependsOnFields(typeof(documentMatched), typeof(curyDebitAmt))]
			get
			{
				return (this._DocumentMatched == true ? this.CuryDebitAmt : Decimal.Zero);
			}
			set
			{
			}
		}
		#endregion
		#region CuryReconciledCredit
		public new abstract class curyReconciledCredit : PX.Data.IBqlField
		{
		}
		[PXDecimal()]
		public override Decimal? CuryReconciledCredit
		{
			[PXDependsOnFields(typeof(documentMatched), typeof(curyCreditAmt))]
			get
			{
				return (this.DocumentMatched == true) ? this.CuryCreditAmt : Decimal.Zero;
			}
			set
			{
			}
		}
		#endregion

		#region CountDebit
		public new abstract class countDebit : PX.Data.IBqlField
		{
		}
		[PXInt()]
		public override Int32? CountDebit
		{
			[PXDependsOnFields(typeof(drCr))]
			get
			{
				return (this._DrCr == CADrCr.CADebit) ? (int)1 : (int)0;
			}
			set
			{
			}
		}
		#endregion
		#region CountCredit
		public new abstract class countCredit : PX.Data.IBqlField
		{
		}
		[PXInt()]
		public override Int32? CountCredit
		{
			[PXDependsOnFields(typeof(drCr))]
			get
			{
				return (this._DrCr == CADrCr.CACredit ? (int)1 : (int)0);
			}
			set
			{
			}
		}
		#endregion

		#region ReconciledCountDebit
		public new abstract class reconciledCountDebit : PX.Data.IBqlField
		{
		}
		[PXInt()]
		public override Int32? ReconciledCountDebit
		{
			[PXDependsOnFields(typeof(documentMatched), typeof(countDebit))]
			get
			{
				return (this.DocumentMatched == true) ? this.CountDebit : 0;
			}
			set
			{
			}
		}
		#endregion
		#region ReconciledCountCredit
		public new abstract class reconciledCountCredit : PX.Data.IBqlField
		{
		}
		[PXInt()]
		public override Int32? ReconciledCountCredit
		{
			[PXDependsOnFields(typeof(documentMatched), typeof(countCredit))]
			get
			{
				return (this.DocumentMatched == true) ? this.CountCredit : 0;
			}
			set
			{
			}
		}
		#endregion

	}

	public interface IStatementReader
	{
		void Read(byte[] aInput);
		bool IsValidInput(byte[] aInput);
		bool AllowsMultipleAccounts();
		void ExportTo(PX.SM.FileInfo aFileInfo, CABankStatementEntry current, out List<CABankStatement> aExported);
	}

	public abstract class StatementReader
	{
		abstract public void Read(byte[] aInput);
		abstract public bool IsValidInput(byte[] aInput);
		abstract public bool AllowsMultipleAccounts();
		abstract public void ExportTo(CABankStatementEntry current, List<CABankStatement> aCreated);
		public virtual void ExportTo(PX.SM.FileInfo aFileInfo, CABankStatementEntry current, out List<CABankStatement> statements)
		{
			UploadFileMaintenance fileGraph = PXGraph.CreateInstance<UploadFileMaintenance>();
			statements = new List<CABankStatement>();
			CABankStatementEntry graph = null;
			if (current != null)
			{
				graph = current;
			}
			else
			{
				graph = PXGraph.CreateInstance<CABankStatementEntry>();
			}
			ExportTo(graph, statements);
			if (fileGraph.SaveFile(aFileInfo, FileExistsAction.CreateVersion))
			{
				if (aFileInfo.UID.HasValue)
				{
					foreach (CABankStatement iStatement in statements)
					{
						if (Object.ReferenceEquals(graph.BankStatement.Current, iStatement) == false)
						{
							graph.BankStatement.Current = graph.BankStatement.Search<CABankStatement.cashAccountID, CABankStatement.refNbr>(iStatement.CashAccountID, iStatement.RefNbr);
						}
						PXNoteAttribute.SetFileNotes(graph.BankStatement.Cache, graph.BankStatement.Current, aFileInfo.UID.Value);
						graph.Save.Press();
					}
				}
			}
		}
	}
}
