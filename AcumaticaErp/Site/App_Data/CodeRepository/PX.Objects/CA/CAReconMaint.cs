using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;

using PX.Objects.CM;

namespace PX.Objects.CA
{
	public class CAReconEntry : PXGraph<CAReconEntry>
	{
        #region Internal  Type Definitions
        [Serializable]
        public partial class CATranExt : CATran
        {
            #region CuryEffTranAmt
            public abstract class curyEffTranAmt : PX.Data.IBqlField
            {
            }
            protected Decimal? _CuryEffTranAmt;
            [PXCury(typeof(CATran.curyID))]            
            [PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.Visible)]
            public virtual Decimal? CuryEffTranAmt
            {
                get
                {
                    return this._CuryEffTranAmt;
                }
                set
                {
                    this._CuryEffTranAmt = value;
                }
            }
            #endregion
            #region EffTranAmt
            public abstract class effTranAmt : PX.Data.IBqlField
            {
            }
            protected Decimal? _EffTranAmt;
            [PXDecimal(4)]            
            [PXUIField(DisplayName = "Tran. Amount")]
            public virtual Decimal? EffTranAmt
            {
                get
                {
                    return this._EffTranAmt;
                }
                set
                {
                    this._EffTranAmt = value;
                }
            }
            #endregion
            #region CuryEffDebitAmt
            public abstract class curyEffDebitAmt : PX.Data.IBqlField
            {
            }
            
            [PXDecimal()]
            [PXUIField(DisplayName = "Receipt")]            
            public virtual Decimal? CuryEffDebitAmt
            {
                [PXDependsOnFields(typeof(drCr), typeof(curyTranAmt))]
                get
                {
                    if (this._DrCr == null)
                    {
                        return null;
                    }
                    else
                    {
                        return (this._DrCr == CADrCr.CADebit) ? this._CuryEffTranAmt : 0m;
                    }
                }
                set
                {
                    
                }
            }
            #endregion
            #region CuryEffCreditAmt
            public abstract class curyEffCreditAmt : PX.Data.IBqlField
            {
            }            
            [PXDecimal()]
            //[PXFormula(null, typeof(SumCalc<CARecon.curyReconciledCredits>))]
            [PXUIField(DisplayName = "Disbursement")]
            public virtual Decimal? CuryEffCreditAmt
            {
                [PXDependsOnFields(typeof(drCr), typeof(curyEffTranAmt))]
                get
                {
                    if (this._DrCr == null)
                    {
                        return null;
                    }
                    else
                    {
                        return (this._DrCr == CADrCr.CACredit) ? -this._CuryEffTranAmt : 0m;
                    }
                }
                set
                {
                   
                }
            }
            #endregion
            #region CuryEffClearedDebitAmt
            public abstract class curyEffClearedDebitAmt : PX.Data.IBqlField
            {
            }
            [PXDecimal()]
            [PXUIField(DisplayName = "Receipt")]
            public virtual Decimal? CuryEffClearedDebitAmt
            {
                [PXDependsOnFields(typeof(cleared), typeof(drCr), typeof(curyEffDebitAmt))]
                get
                {
                    return (this._Cleared == true ? (this.CuryEffDebitAmt ?? Decimal.Zero) : Decimal.Zero);
                }
                set
                {
                }
            }
            #endregion
            #region CuryEffClearedCreditAmt
            public abstract class curyEffClearedCreditAmt : PX.Data.IBqlField
            {
            }
            [PXDecimal()]            
            [PXUIField(DisplayName = "Disbursement")]
            public virtual Decimal? CuryEffClearedCreditAmt
            {
                [PXDependsOnFields(typeof(cleared), typeof(drCr), typeof(curyTranAmt))]
                get
                {
                    return (this._Cleared == true && this._DrCr == CADrCr.CACredit) ? -this._CuryEffTranAmt : 0m;
                }
                set
                {
                }
            }
            #endregion
            #region Reconcilation Members
            #region ReconciledDebit
            public abstract class reconciledDebit : PX.Data.IBqlField
            {
            }
            [PXDecimal()]
            [PXFormula(null, typeof(SumCalc<CARecon.reconciledDebits>))]
            public virtual Decimal? ReconciledDebit
            {
                [PXDependsOnFields(typeof(reconciled), typeof(drCr), typeof(effTranAmt))]
                get
                {
                    return (this._Reconciled == true && this._DrCr == CADrCr.CADebit) ? this._EffTranAmt : 0m;
                }
                set
                {
                }
            }
            #endregion
            #region CuryReconciledDebit
            public abstract class curyReconciledDebit : PX.Data.IBqlField
            {
            }
            [PXDecimal()]
            [PXFormula(null, typeof(SumCalc<CARecon.curyReconciledDebits>))]
            public virtual Decimal? CuryReconciledDebit
            {
                [PXDependsOnFields(typeof(reconciled), typeof(drCr), typeof(curyEffTranAmt))]
                get
                {
                    return (this._Reconciled == true && this._DrCr == CADrCr.CADebit) ? this._CuryEffTranAmt : 0m;
                }
                set
                {
                }
            }
            #endregion
            #region ReconciledCredit
            public abstract class reconciledCredit : PX.Data.IBqlField
            {
            }
            [PXDecimal()]
            [PXFormula(null, typeof(SumCalc<CARecon.reconciledCredits>))]
            public virtual Decimal? ReconciledCredit
            {
                [PXDependsOnFields(typeof(reconciled), typeof(drCr), typeof(effTranAmt))]
                get
                {
                    return (this._Reconciled == true && this._DrCr == CADrCr.CACredit) ? -this._EffTranAmt : 0m;
                }
                set
                {
                }
            }
            #endregion
            #region CuryReconciledCredit
            public abstract class curyReconciledCredit : PX.Data.IBqlField
            {
            }
            [PXDecimal()]
            [PXFormula(null, typeof(SumCalc<CARecon.curyReconciledCredits>))]
            public virtual Decimal? CuryReconciledCredit
            {
                [PXDependsOnFields(typeof(reconciled), typeof(drCr), typeof(curyEffTranAmt))]
                get
                {
                    return (this._Reconciled == true && this._DrCr == CADrCr.CACredit) ? -this._CuryEffTranAmt : 0m;
                }
                set
                {
                }
            }
            #endregion
            #region CountDebit
            public abstract class countDebit : PX.Data.IBqlField
            {
            }
            [PXInt()]
            [PXFormula(null, typeof(SumCalc<CARecon.countDebit>))]
            public virtual Int32? CountDebit
            {
                [PXDependsOnFields(typeof(reconciled), typeof(drCr))]
                get
                {
                    return (this._Reconciled == true && this._DrCr == CADrCr.CADebit) ? (int)1 : (int)0;
                }
                set
                {
                }
            }
            #endregion
            #region CountCredit
            public abstract class countCredit : PX.Data.IBqlField
            {
            }
            [PXInt()]
            [PXFormula(null, typeof(SumCalc<CARecon.countCredit>))]
            public virtual Int32? CountCredit
            {
                [PXDependsOnFields(typeof(reconciled), typeof(drCr))]
                get
                {
                    return (this._Reconciled == true && this._DrCr == CADrCr.CACredit) ? (int)1 : (int)0;
                }
                set
                {
                }
            }
            #endregion
            #endregion
            #region Voided
            public abstract class voided : PX.Data.IBqlField
            {
            }
            protected Boolean? _Voided;
            [PXBool()]            
            [PXUIField(DisplayName = "Voided")]
            public virtual Boolean? Voided
            {
                get
                {
                    return this._Voided;
                }
                set
                {
                    this._Voided = value;
                }
            }
            #endregion
            #region VoidingTranID
            public abstract class voidingTranID : PX.Data.IBqlField
            {
            }
            protected Int64? _VoidingTranID;
            [PXLong()]
            public virtual Int64? VoidingTranID
            {
                get
                {
                    return this._VoidingTranID;
                }
                set
                {
                    this._VoidingTranID = value;
                }
            }
            #endregion
            #region VoidingNotReleased
            public abstract class voidingNotReleased : PX.Data.IBqlField
            {
            }
            protected Boolean? _VoidingNotReleased;
            [PXBool()]            
            public virtual Boolean? VoidingNotReleased
            {
                get
                {
                    return this._VoidingNotReleased;
                }
                set
                {
                    this._VoidingNotReleased = value;
                }
            }
            #endregion
            #region Status
            public abstract new class status : PX.Data.IBqlField
            {
            }
            [PXString(11, IsFixed = true)]
            [PXUIField(DisplayName = "Status", Enabled = false)]
            [ExtTranStatus.List()]
            public override string Status
            {
                [PXDependsOnFields(typeof(posted), typeof(released), typeof(hold), typeof(voided),typeof(voidingNotReleased))]
                get
                {
                    if (this.VoidingNotReleased == true)
                    {
                        return ExtTranStatus.PartiallyVoided;
                    }
                    else if (this.Voided == true) 
                    {
                        return ExtTranStatus.Voided;
                    }
                    else
                    if (this._Posted == true)
                    {
                        if (this._Released == true)
                            return GL.BatchStatus.Posted;
                        else
                            return GL.BatchStatus.Unposted;
                    }
                    else if (this._Released == true && this._Posted != true)
                    {
                        return GL.BatchStatus.Released;
                    }
                    else if (this._Hold == true)
                    {
                        return GL.BatchStatus.Hold;
                    }
                    else
                    {
                        return GL.BatchStatus.Balanced;
                    }
                }
                set
                {
                }
            }
            #endregion
           	public class ExtTranStatus: BatchStatus
        	{ 
		        public new class ListAttribute: PXStringListAttribute
		        {
			        public ListAttribute(): base(
			        new string[] { Hold, Balanced, Unposted, Posted, Completed, Voided, Released, PartiallyReleased, Scheduled, PartiallyVoided },
			        new string[] { Messages.Hold, Messages.Balanced, GL.Messages.Unposted, GL.Messages.Posted, GL.Messages.Completed, Messages.Voided, Messages.Released, GL.Messages.PartiallyReleased, GL.Messages.Scheduled,CA.Messages.VoidPendingStatus }) { }
		        }
    	        public const string PartiallyVoided = "PV";
            }
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
            #region Released
            public new abstract class released : PX.Data.IBqlField
            {
            }            
            #endregion
            #region Reconciled
            public new abstract class reconciled : PX.Data.IBqlField
            {
            }
            
            #endregion
            #region ReconDate
            public new abstract class reconDate : PX.Data.IBqlField
            {
            }
            
            #endregion
            #region ReconNbr
            public new abstract class reconNbr : PX.Data.IBqlField
            {
            }            
            #endregion
            #region Cleared
            public new abstract class cleared : PX.Data.IBqlField
            {
            }            
            #endregion
            #region ClearDate
            public new abstract class clearDate : PX.Data.IBqlField
            {
            }
            
            #endregion
        }
        #endregion
        #region Buttons Definition
			public PXSave<CARecon> Save;
			#region Button Cancel
			  public PXAction<CARecon> cancel;
			  [PXCancelButton]
			  [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
			  protected virtual System.Collections.IEnumerable Cancel(PXAdapter a)
			  {
                  CARecon current = null;
                  CARecon cachesRecon = null;
                  PXSelectBase<CARecon> currentSelectStatement;
                  object cashAccountCD = a.Searches != null && a.Searches.Length > 0
                      ? a.Searches[0]
                      : null;

                  object reconNbr = a.Searches != null && a.Searches.Length > 1
                      ? a.Searches[1]
                      : null;

                  foreach (CARecon recon in (new PXCancel<CARecon>(this, "Cancel")).Press(a))
                  {
                      cachesRecon = recon;
                  }

                  CashAccount acct = PXSelect<CashAccount,
                      Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>,
                      And<Match<Current<AccessInfo.userName>>>>>.Select(this, cashAccountCD);

                  if (acct != null)
                  {
                      if (reconNbr != null)
                      {
                          currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
                                                                                  And<CARecon.reconNbr, Equal<Required<CARecon.reconNbr>>>>>(this);
                          current = (CARecon)currentSelectStatement.View.SelectSingle(acct.CashAccountID, reconNbr);
                      }

                      if (current == null)
                      {
                          currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
                                                                                  And<CARecon.reconciled, Equal<boolFalse>>>,
                                                                              OrderBy<Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>(this);
                          current = (CARecon)currentSelectStatement.View.SelectSingle(acct.CashAccountID);
                      }

                      if (current == null)
                      {
                          if (cachesRecon != null && CAReconRecords.Cache.GetStatus(cachesRecon) == PXEntryStatus.Inserted)
                          {
                              current = cachesRecon;
                          }
                          else
                          {
                              current = new CARecon();
                              current.CashAccountID = acct.CashAccountID;
                              current = CAReconRecords.Insert(current);
                          }
                      }
                      if (cachesRecon != null && current != null && (cachesRecon.CashAccountID != current.CashAccountID || cachesRecon.ReconNbr != current.ReconNbr))
                      {
                          if (CAReconRecords.Cache.GetStatus(cachesRecon) == PXEntryStatus.Inserted)
                          {
                              CAReconRecords.Delete(cachesRecon);
                              CAReconRecords.Cache.IsDirty = false;
                          }
                          CAReconRecords.Current = current;
                      }
                  }
                  yield return current; 

                 // return new PXCancel<CARecon>(this, "Cancel").Press(a);
			  }
			#endregion
			public PXInsert<CARecon>	Insert;
			#region Button Delete
				public PXAction<CARecon> delete;
				[PXDeleteButton]
				[PXUIField]
				protected virtual System.Collections.IEnumerable Delete(PXAdapter a)
				{
					CARecon current = null;
					object cashAccountCD = a.Searches != null && a.Searches.Length > 0
						? a.Searches[0]
						: null;
					foreach (CARecon headerDeleted in (new PXDelete<CARecon>(this, "Delete")).Press(a))
					{
						current = headerDeleted;
					}

                    CashAccount acct = PXSelect<CashAccount,
                        Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>,
						And<Match<Current<AccessInfo.userName>>>>>.Select(this, cashAccountCD);

					if (acct != null)
					{
						PXSelectBase<CARecon> activitySelect = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
																							   And<CARecon.reconciled, Equal<boolFalse>>>,
																							 OrderBy<Asc<CARecon.cashAccountID, Desc<CARecon.reconNbr>>>>(this);
						current = (CARecon)activitySelect.View.SelectSingle(acct.AccountID);
					}

					if (current == null)
					{
						current = new CARecon();
						current.CashAccountID = acct.CashAccountID;
						current = CAReconRecords.Insert(current);
						CAReconRecords.Cache.SetStatus(current, PXEntryStatus.Inserted);
					}

					yield return current;
				}
			#endregion
			#region Button First
			  public PXAction<CARecon> first;
			  [PXFirstButton]
			  [PXUIField]
			  protected virtual System.Collections.IEnumerable First(PXAdapter a)
			  {
				  PXLongOperation.ClearStatus(this.UID);
				  CARecon current		= null;
				  CARecon cachesRecon	= null;
				  PXSelectBase<CARecon> currentSelectStatement;
				  object cashAccountCD = a.Searches != null && a.Searches.Length > 0
					  ? a.Searches[0]
					  : null;

				  object reconNbr = a.Searches != null && a.Searches.Length > 1
					  ? a.Searches[1]
					  : null;

				  foreach (CARecon recon in (new PXFirst<CARecon>(this, "First")).Press(a))
				  {
					  cachesRecon = recon;
				  }

				  Account acct = PXSelect<Account,
					  Where<Account.accountCD, Equal<Required<Account.accountCD>>,
					  And<Match<Current<AccessInfo.userName>>>>>.Select(this, cashAccountCD);

				  if (acct != null)
				  {
					  if (reconNbr != null)
					  { 	
						  currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>>,
																				 OrderBy<Asc<CARecon.reconDate, Asc<CARecon.reconNbr>>>>(this);
						  current = (CARecon)currentSelectStatement.View.SelectSingle(acct.AccountID);
					  }
					  if (cachesRecon != null && current != null && (cachesRecon.CashAccountID != current.CashAccountID || cachesRecon.ReconNbr != current.ReconNbr))
					  {
						  if (CAReconRecords.Cache.GetStatus(cachesRecon) == PXEntryStatus.Inserted)
						  {
						  	  CAReconRecords.Delete(cachesRecon);
							  CAReconRecords.Cache.IsDirty = false;	
						  }
						  CAReconRecords.Current = current;
					  }
				  }
				  yield return current;
			  }
			#endregion
			#region Button Prev
			  public PXAction<CARecon> prev;
			  [PXPreviousButton]
			  [PXUIField]
			  protected virtual System.Collections.IEnumerable Prev(PXAdapter a)
			  {
				  CARecon current		= null;
				  CARecon currentPre	= CAReconRecords.Current;
				  CARecon cachesRecon	= null;
				  PXSelectBase<CARecon> currentSelectStatement;
				  object cashAccountCD = a.Searches != null && a.Searches.Length > 0
					  ? a.Searches[0]
					  : null;

				  object reconNbr = a.Searches != null && a.Searches.Length > 1
					  ? a.Searches[1]
					  : null;

				  foreach (CARecon recon in (new PXPrevious<CARecon>(this, "Prev")).Press(a))
				  {
					  cachesRecon = recon;
				  }

				  Account acct = PXSelect<Account,
					  Where<Account.accountCD, Equal<Required<Account.accountCD>>,
					  And<Match<Current<AccessInfo.userName>>>>>.Select(this, cashAccountCD);

				  if (acct != null)
				  {
					  if (reconNbr != null)
					  { 	
						  currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
																				  And<CARecon.reconNbr, Less<Required<CARecon.reconNbr>>>>,
																				 OrderBy<Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>(this);
						  current = (CARecon)currentSelectStatement.View.SelectSingle(acct.AccountID, reconNbr);
					  }
					  if(current == null)
					  { 
						  currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>>,
																				 OrderBy<Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>(this);
						  current = (CARecon)currentSelectStatement.View.SelectSingle(acct.AccountID);						
					  }
					  if (cachesRecon != null && current != null && (cachesRecon.CashAccountID != current.CashAccountID || cachesRecon.ReconNbr != current.ReconNbr))
					  {
						  if (CAReconRecords.Cache.GetStatus(cachesRecon) == PXEntryStatus.Inserted)
						  {
						  	  CAReconRecords.Delete(cachesRecon);
							  CAReconRecords.Cache.IsDirty = false;	
						  }
						  CAReconRecords.Current = current;
					  }
				  }
				  yield return current;
			  }
			#endregion
			#region Button Next
			  public PXAction<CARecon> next;
			  [PXNextButton]
			  [PXUIField]
			  protected virtual System.Collections.IEnumerable Next(PXAdapter a)
			  {
				  PXLongOperation.ClearStatus(this.UID);
				  CARecon current		= null;
				  CARecon cachesRecon	= null;
				  PXSelectBase<CARecon> currentSelectStatement;
				  object cashAccountCD = a.Searches != null && a.Searches.Length > 0
					  ? a.Searches[0]
					  : null;

				  object reconNbr = a.Searches != null && a.Searches.Length > 1
					  ? a.Searches[1]
					  : null;

				  foreach (CARecon recon in (new PXNext<CARecon>(this, "Next")).Press(a))
				  {
					  cachesRecon = recon;
				  }

				  Account acct = PXSelect<Account,
					  Where<Account.accountCD, Equal<Required<Account.accountCD>>,
					  And<Match<Current<AccessInfo.userName>>>>>.Select(this, cashAccountCD);

				  if (acct != null)
				  {
					  if (reconNbr != null)
					  { 	
						  currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
																				  And<CARecon.reconNbr, Greater<Required<CARecon.reconNbr>>>>,
																				 OrderBy<Asc<CARecon.reconDate, Asc<CARecon.reconNbr>>>>(this);
						  current = (CARecon)currentSelectStatement.View.SelectSingle(acct.AccountID, reconNbr);
					  }
					  if (cachesRecon != null && current != null && (cachesRecon.CashAccountID != current.CashAccountID || cachesRecon.ReconNbr != current.ReconNbr))
					  {
						  if (CAReconRecords.Cache.GetStatus(cachesRecon) == PXEntryStatus.Inserted)
						  {
						  	  CAReconRecords.Delete(cachesRecon);
							  CAReconRecords.Cache.IsDirty = false;	
						  }
						  CAReconRecords.Current = current;
					  }
				  }
				  yield return current;
			  }
			#endregion
			#region Button Last
			  public PXAction<CARecon> last;
			  [PXLastButton]
			  [PXUIField]
			  protected virtual System.Collections.IEnumerable Last(PXAdapter a)
			  {
				  PXLongOperation.ClearStatus(this.UID);
				  CARecon current		= null;
				  CARecon cachesRecon	= null;
				  PXSelectBase<CARecon> currentSelectStatement;
				  object cashAccountCD = a.Searches != null && a.Searches.Length > 0
					  ? a.Searches[0]
					  : null;

				  object reconNbr = a.Searches != null && a.Searches.Length > 1
					  ? a.Searches[1]
					  : null;

				  foreach (CARecon recon in (new PXLast<CARecon>(this, "Last")).Press(a))
				  {
					  cachesRecon = recon;
				  }

				  Account acct = PXSelect<Account,
					  Where<Account.accountCD, Equal<Required<Account.accountCD>>,
					  And<Match<Current<AccessInfo.userName>>>>>.Select(this, cashAccountCD);

				  if (acct != null)
				  {
					  if (reconNbr != null)
					  { 	
						  currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>>,
																				 OrderBy<Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>(this);
						  current = (CARecon)currentSelectStatement.View.SelectSingle(acct.AccountID);
					  }
					  if (cachesRecon != null && current != null && (cachesRecon.CashAccountID != current.CashAccountID || cachesRecon.ReconNbr != current.ReconNbr))
					  {
						  if (CAReconRecords.Cache.GetStatus(cachesRecon) == PXEntryStatus.Inserted)
						  {
						  	  CAReconRecords.Delete(cachesRecon);
							  CAReconRecords.Cache.IsDirty = false;	
						  }
						  CAReconRecords.Current = current;
					  }
				  }
				  yield return current;
			  }
			#endregion

            #region Button Toggle Reconciled
            public PXAction<CARecon> ToggleReconciled;

            [PXUIField(DisplayName = "Toggle Reconciled", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
            [PXProcessButton]
            public virtual IEnumerable toggleReconciled(PXAdapter adapter)
            {
                bool? reconciledValue = null;

                foreach (CATranExt tran in GetReconTrans())
                {
                    if (tran.Released != true)
                        continue;

                    reconciledValue = reconciledValue ?? !tran.Reconciled;
                    
                    CATranExt copy = PXCache<CATranExt>.CreateCopy(tran);
                    copy.Reconciled = reconciledValue;
                    CAReconTranRecords.Update(copy);
                }
                return adapter.Get();
            }
            #endregion

            #region Button Toggle Cleared
            public PXAction<CARecon> ToggleCleared;

            [PXUIField(DisplayName = "Toggle Cleared", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
            [PXProcessButton]
            public virtual IEnumerable toggleCleared(PXAdapter adapter)
            {
                bool? clearedValue = null;

                foreach (CATranExt tran in GetReconTrans())
                {
                    if (tran.Released != true)
                        continue;

                    clearedValue = clearedValue ?? !tran.Cleared;
                    CATranExt copy = PXCache<CATranExt>.CreateCopy(tran);
                    copy.Cleared = clearedValue;
                    CAReconTranRecords.Update(copy);
                }
                return adapter.Get();
            }
            #endregion


            #region ToggleReconciled and ToggleCleared helper
            private IEnumerable<CATranExt> GetReconTrans()
            {
                PXSelectBase<CATranExt> trans = new PXSelectJoin<CATranExt, LeftJoin<CABankStatementDetail, On<CATranExt.tranID, Equal<CABankStatementDetail.cATranID>>,
                                                                            LeftJoin<CABankStatement, On<CABankStatementDetail.refNbr, Equal<CABankStatement.refNbr>>,
                                                                            LeftJoin<CATran2, On<CATran2.cashAccountID, Equal<CATranExt.cashAccountID>,
                                                                                    And<CATran2.voidedTranID, Equal<CATranExt.tranID>>>>>>,
                                                                               Where<CATranExt.cashAccountID, Equal<Current<CARecon.cashAccountID>>,
                                                                               And<Where<CATranExt.reconNbr, Equal<Current<CARecon.reconNbr>>,
                                                                               Or<Current<CARecon.reconciled>, Equal<boolFalse>,
                                                                               And<CATranExt.reconNbr, IsNull,
                                                                               And<Where<CATranExt.tranDate, LessEqual<Optional<CARecon.loadDocumentsTill>>,
                                                                                  Or<Optional<CARecon.loadDocumentsTill>, IsNull>>>>>>>>,
                                                                               OrderBy<Asc<CATranExt.tranDate>>>(this);

                int start = 0;
                int total = 0;

                foreach (PXResult<CATranExt, CABankStatementDetail, CABankStatement, CATran2> record in trans.View.Select(
                    PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings,
                    CAReconTranRecords.View.GetExternalFilters(), ref start, PXView.MaximumRows, ref total))
                {
                    yield return (CATranExt)record;
                }
            }
            #endregion

            #region Button Release
            public PXAction<CARecon> Release;
				[PXUIField(DisplayName = "Release", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
				[PXProcessButton]
				public virtual IEnumerable release(PXAdapter adapter)
				{
					IEnumerable ret = Save.Press(adapter);
                    IList<CARecon> list = ret as IList<CARecon> ?? ret.Cast<CARecon>().ToList();
				    foreach (CARecon recon in list)
				    {
				        CARecon r = recon;
				        PXLongOperation.StartOperation(this, () => ReconRelease(r, true));
				        break;
				    }
				    return list;
				}
			#endregion

			#region Button Voided
			public PXAction<CARecon> Voided;
			[PXUIField(DisplayName = "Void", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
			[PXProcessButton]
			public virtual IEnumerable voided(PXAdapter adapter)
			{
			    CARecon recon = CAReconRecords.Current;
                PXLongOperation.ClearStatus(this.UID);
                PXLongOperation.StartOperation(this, () => ReconVoided(recon));
			    yield return recon;
			}
			#endregion

			#region Button ViewDoc
				public PXAction<CARecon> viewDoc;
				[PXUIField(DisplayName = "View Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
				[PXLookupButton]
				public virtual IEnumerable ViewDoc(PXAdapter adapter)
				{
					CATran tran = CAReconTranRecords.Current;
					CAReconTranRecords.Current = null;
					PXSelect<CATran, Where<CATran.cashAccountID, Equal<Current<CARecon.cashAccountID>>,
						And<Where<CATran.reconNbr, Equal<Current<CARecon.reconNbr>>,
						Or<Current<CARecon.reconciled>, Equal<boolFalse>,
						And<CATran.reconNbr, IsNull,
						And<Where<CATran.tranDate, LessEqual<Current<CARecon.loadDocumentsTill>>,
						Or<Current<CARecon.loadDocumentsTill>, IsNull>>>>>>>>>.Clear(this);
					CATran.Redirect(CAReconRecords.Cache, tran);
					return adapter.Get();
				}
			#endregion

			#region Button CreateAdjustment
				public PXAction<CARecon> CreateAdjustment;
				[PXUIField(DisplayName = "Create Adjustment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
				[PXProcessButton]
				public virtual IEnumerable createAdjustment(PXAdapter adapter)
				{
					CARecon header = CAReconRecords.Current;
					if ((header != null) && (this.headerUpdateEnabled(header) == true))
                    {
                        WebDialogResult result = this.Views["AddFilter"].AskExt(true);
                        if (result == WebDialogResult.OK)
                        {
                            using (new PXTimeStampScope(this.TimeStamp))
                            {
                                CATran rectran = null;

                                rectran = (rectran ?? AddTrxFilter.AddAPTransaction(this, AddFilter, currencyinfo));
                                rectran = (rectran ?? AddTrxFilter.AddARTransaction(this, AddFilter, currencyinfo));
                                rectran = (rectran ?? AddTrxFilter.AddCATransaction(this, AddFilter, currencyinfo));
                                AddTrxFilter.Clear(this, AddFilter);
                                Save.Press();
                                /*
                                if (rectran != null)
                                {
                                    try
                                    {
                                        rectran.Reconciled = true;
                                        CAReconTranRecords.Update(rectran);
                                        Save.Press();
                                        CAReconRecords.Current = header;
                                    }
                                    catch (PXOverridableException ex)
                                    {
                                        ex.SetMessage(ex.Message + " " + PXMessages.LocalizeNoPrefix(Messages.ReloadRecon));
                                        throw ex;
                                    }
                                }
                                */
                            }
                        }
					}

					yield return header;

					//return adapter.Get();
				}
			#endregion
		#endregion

		#region Variables
			public ToggleCurrency<CARecon> CurrencyView;

			public PXSelectOrderBy<CARecon, OrderBy<Asc<CARecon.cashAccountID, Desc<CARecon.reconDate>>>> CAReconRecords;
			public PXSelect<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>, And<CARecon.reconNbr, Equal<Required<CARecon.reconNbr>>>>> CAReconRecordByCashAccountAndNumbr;
			[PXFilterable]			
			public PXSelectJoin<CATranExt,LeftJoin<CABankStatementDetail, On<CATranExt.tranID, Equal<CABankStatementDetail.cATranID>>,
                                                                    LeftJoin<CABankStatement, On<CABankStatementDetail.refNbr, Equal<CABankStatement.refNbr>>,
                                                                    LeftJoin<CATran2, On<CATran2.cashAccountID, Equal<CATranExt.cashAccountID>,
                                                                    And<CATran2.voidedTranID, Equal<CATranExt.tranID>>>>>>, 
                                                                                Where<True, Equal<True>>, OrderBy<Asc<CATran.tranDate>>> CAReconTranRecords;

            public PXSelect<CATran2, Where<CATran2.cashAccountID, Equal<Required<CATran2.cashAccountID>>, And<CATran2.tranID, Equal<Required<CATran2.tranID>>>>> VoidingTrans;
            public PXSelectJoin<CATranExt, LeftJoin<CATran2, On<CATran2.cashAccountID, Equal<CATranExt.cashAccountID>,
                                                                    And<CATran2.voidedTranID, Equal<CATranExt.tranID>,
                                                                    And<CATran2.released, Equal<False>>>>>,
                                                Where<CATranExt.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
                                                And<Where<CATranExt.reconNbr, Equal<Required<CARecon.reconNbr>>>>>> PartiallyVoidedTrans; 
			


			public PXFilter<AddTrxFilter> AddFilter;
  			public PXSelect<CurrencyInfo> currencyinfo;
			
			public PXSetup<CashAccount, Where<CashAccount.reconcile, Equal<boolTrue>>> cashaccount;
			public PXSetup<CASetup> casetup;
			public CMSetupSelect CMSetup;
			public bool updateEnabled = false;
			public bool insertEnabled = false;
		#endregion
		
		#region Execute Select
			protected virtual IEnumerable carecontranrecords()
			{
				if (CAReconRecords.Current== null || CAReconRecords.Current.Voided == true)
                {
                    yield break;
                }
                CARecon doc = CAReconRecords.Current;                
                bool skipVoids = this.casetup.Current.SkipVoided??false;
                  
                foreach (PXResult<CATranExt, CABankStatementDetail, CABankStatement, CATran2> result in PXSelectJoin<CATranExt, LeftJoin<CABankStatementDetail, On<CATranExt.tranID, Equal<CABankStatementDetail.cATranID>>,
                                                                                                LeftJoin<CABankStatement, On<CABankStatementDetail.refNbr, Equal<CABankStatement.refNbr>>,
                                                                                                LeftJoin<CATran2, On<CATran2.cashAccountID, Equal<CATranExt.cashAccountID>,
                                                                                                        And<CATran2.voidedTranID, Equal<CATranExt.tranID>>>>>>, 
                                                                               Where<CATranExt.cashAccountID, Equal<Current<CARecon.cashAccountID>>,
                                                                               And<Where<CATranExt.reconNbr, Equal<Current<CARecon.reconNbr>>,
                                                                               Or<Current<CARecon.reconciled>, Equal<boolFalse>,
                                                                               And<CATranExt.reconNbr, IsNull,
                                                                               And<Where<CATranExt.tranDate, LessEqual<Optional<CARecon.loadDocumentsTill>>,
                                                                                  Or<Optional<CARecon.loadDocumentsTill>, IsNull>>>>>>>>>.Select(this))
                {
                    CATranExt tran = result;
                    CATran2 voidingTran = result;
                    tran.CuryEffTranAmt = tran.CuryTranAmt;
                    tran.EffTranAmt = tran.TranAmt;
                    tran.Voided = false;
                    tran.VoidingNotReleased = false;
                    tran.VoidingTranID = null;
                    if (skipVoids) 
                    {
                        if(tran.VoidedTranID.HasValue && tran.Released == true) continue; //Skip voiding transaction
                        if (voidingTran != null && voidingTran.TranID.HasValue) 
                        {
                            if (voidingTran.Released == true)
                            {
                                tran.CuryEffTranAmt += (voidingTran.CuryTranAmt ?? Decimal.Zero);
                                tran.EffTranAmt += (voidingTran.TranAmt ?? Decimal.Zero);
                                tran.Voided = true;
                                tran.VoidingTranID = voidingTran.TranID;                    
                            }
                            else
                            {
                                tran.VoidingNotReleased = true;
                                tran.VoidingTranID = voidingTran.TranID;                    
                            }
                        }                        
                    }
                    PXCache cache = this.Caches[typeof(CATranExt)];
                    PXEntryStatus status = cache.GetStatus(tran);
                    if(status == PXEntryStatus.Notchanged)
                        cache.SetStatus(tran, PXEntryStatus.Held);                   
                    
                    yield return result;
                }
			}
		#endregion
		
		#region Implement
			public override void Persist()
			{
                List<CATran> list = new List<CATran>((IEnumerable<CATran>)this.Caches[typeof(CATran)].Updated);

                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    base.Persist();

                    foreach (CATran tran in list)
                    {
                        switch (tran.OrigModule)
                        {
                            case GL.BatchModule.AP:
                                PXDatabase.Update<APPayment>(
                                    new PXDataFieldAssign<APPayment.cleared>(tran.Cleared),
                                    new PXDataFieldAssign<APPayment.clearDate>(tran.ClearDate),
                                    new PXDataFieldRestrict<APPayment.docType>(tran.OrigTranType),
                                    new PXDataFieldRestrict<APPayment.refNbr>(tran.OrigRefNbr));
                                break;
                            case GL.BatchModule.AR:
                                PXDatabase.Update<ARPayment>(
                                    new PXDataFieldAssign<ARPayment.cleared>(tran.Cleared),
                                    new PXDataFieldAssign<ARPayment.clearDate>(tran.ClearDate),
                                    new PXDataFieldRestrict<ARPayment.docType>(tran.OrigTranType),
                                    new PXDataFieldRestrict<ARPayment.refNbr>(tran.OrigRefNbr));
                                break;
                        }
                    }

                    ts.Complete();
                }

				foreach (CM.CurrencyInfo iInfo in Caches[typeof(CM.CurrencyInfo)].Cached) 
				{
					long? key = this.AddFilter.Current.CuryInfoID;
					if (iInfo.CuryInfoID == key) 
					{
						Caches[typeof(CM.CurrencyInfo)].SetStatus(iInfo, PXEntryStatus.Inserted);
					}
				}				
			}

			public override void Clear()
			{
				if (AddFilter.Current != null)
				{
					AddFilter.Current.TranDate = null;
					AddFilter.Current.FinPeriodID = null;
					AddFilter.Current.CuryInfoID = null;
				}
				base.Clear();
			}

			public CAReconEntry()
			{             
				CASetup setup     = casetup.Current;
                PXCache tranCache = CAReconTranRecords.Cache;

                PXParentAttribute.SetLeaveChildren<CATran.reconNbr>(tranCache, null, true);
				Views["_AddTrxFilter.curyInfoID_CurrencyInfo.CuryInfoID_"] = new PXView(this, false, new Select<AddTrxFilter>(), new PXSelectDelegate(AddFilter.Get));
                
				PXDBCurrencyAttribute.SetBaseCalc<CATranExt.curyClearedCreditAmt>(tranCache, null, false);
				PXDBCurrencyAttribute.SetBaseCalc<CATranExt.curyClearedDebitAmt> (tranCache, null, false);
				PXDBCurrencyAttribute.SetBaseCalc<CATranExt.curyCreditAmt>		  (tranCache, null, false);
				PXDBCurrencyAttribute.SetBaseCalc<CATranExt.curyDebitAmt>		  (tranCache, null, false);
				PXDBCurrencyAttribute.SetBaseCalc<CATranExt.curyReconciledCredit>(tranCache, null, false);
				PXDBCurrencyAttribute.SetBaseCalc<CATranExt.curyReconciledDebit> (tranCache, null, false);
				PXDBCurrencyAttribute.SetBaseCalc<CATranExt.curyTranAmt>		  (tranCache, null, false);

                PXCache reconCache = CAReconRecords.Cache;
				PXDBCurrencyAttribute.SetBaseCalc<CARecon.curyReconciledBalance> (reconCache, null, false);
				PXDBCurrencyAttribute.SetBaseCalc<CARecon.curyReconciledCredits> (reconCache, null, false);
				PXDBCurrencyAttribute.SetBaseCalc<CARecon.curyReconciledDebits>	 (reconCache, null, false);
				PXDBCurrencyAttribute.SetBaseCalc<CARecon.curyReconciledTurnover>(reconCache, null, false);
				PXUIFieldAttribute.SetVisible<CARecon.curyID>(reconCache, null, (bool)CMSetup.Current.MCActivated);
			}
		#endregion

		#region Functions
			
			public static void ReconCreate(CashAccount acct)		
			{
				if (acct == null) return;
				if (acct.Reconcile != true)
					throw new Exception(Messages.CashAccounNotReconcile);
				CAReconEntry graph = PXGraph.CreateInstance<CAReconEntry>();
				PXSelectBase<CARecon> selectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>>,
																						  OrderBy<Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>(graph);
				CARecon lastRecon = (CARecon)selectStatement.View.SelectSingle(acct.CashAccountID);
				if (lastRecon != null && lastRecon.Reconciled != true)
                    throw new Exception(Messages.CanNotCreateStatement);

				CARecon current = new CARecon();
				current.CashAccountID = acct.CashAccountID;
				graph.CAReconRecords.Insert(current);
				throw new PXRedirectRequiredException(graph, "Document");
			}

			public static void ReconVoided(CARecon recon)
			{

				CAReconEntry graph = PXGraph.CreateInstance<CAReconEntry>();
				
                if(!IsVoidAllowed(recon,graph))
					throw new Exception(Messages.CanNotVoidStatement);

				recon = (CARecon)graph.CAReconRecords.Update(recon);
				CATranExt newrow;
				foreach (CATranExt res in PXSelect<CATranExt, Where<CATranExt.reconNbr, Equal<Required<CARecon.reconNbr>>, And <CATranExt.cashAccountID, Equal<Required<CARecon.cashAccountID>>>>>
												  .Select(graph, recon.ReconNbr, recon.CashAccountID))
				{
					if (res.Reconciled == true)
					{
						newrow = PXCache<CATranExt>.CreateCopy(res);
						newrow.Reconciled = false;
						graph.CAReconTranRecords.Update(newrow);
					}
				}

				CARecon newheader = (CARecon) graph.CAReconRecords.Cache.CreateCopy(recon);
				newheader.Reconciled = false;
				newheader.Voided	 = true;
				graph.CAReconRecords.Update(newheader);

				graph.Save.Press();
			}

            private static bool IsVoidAllowed(CARecon recon, CAReconEntry graph)
            {
                PXSelectBase<CARecon> statements = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
                                                                                        And<CARecon.reconDate,GreaterEqual<Required<CARecon.reconDate>>>>,
                                                                                          OrderBy<Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>(graph);
                
                foreach(CARecon statement in statements.Select(recon.CashAccountID,recon.ReconDate))
                {
                    if(statement != null)
                    {
                        if (statement.ReconNbr == recon.ReconNbr)
                            return statement.Reconciled ?? false;
                        else if (statement.Voided == false)
                            return false;
                    }
                }

                return false;
            }

            public static void ReconRelease(CARecon recon, bool updateCATranInfo)
            {
				if (recon.Hold == true)
				{
					throw new PXException(AP.Messages.Document_OnHold_CannotRelease);
				}

				CAReconEntry graph = PXGraph.CreateInstance<CAReconEntry>();
				List<CATran> tranList = new List<CATran>();
                bool skipVoids = graph.casetup.Current.SkipVoided??false;
                if (skipVoids) 
                {
                    CATranExt tran = graph.PartiallyVoidedTrans.Select(recon.CashAccountID, recon.ReconNbr);
                    if (tran != null && tran.TranID.HasValue) 
                    {                        
                        throw new PXException(CA.Messages.TransactionsWithVoidPendingStatusMayNotBeAddedToReconciliation);
                    }
                }

				foreach (CATran transToRelease in PXSelect<CATran, Where<CATran.reconNbr, Equal<Required<CARecon.reconNbr>>, And <CATran.cashAccountID, Equal<Required<CARecon.cashAccountID>>>>>
												  .Select(graph, recon.ReconNbr, recon.CashAccountID))
				{
					if (transToRelease.Reconciled == true &&
						transToRelease.Cleared == true)
					{
                        transToRelease.Selected = true;
						tranList.Add(transToRelease);
					}
				}
				try
				{
					bool releaseComplete = true;
					if (tranList.Count > 0)
					{
						CATrxRelease.GroupReleaseTransaction(tranList, true, true, updateCATranInfo);
						
						for (int i = 0; i< tranList.Count; i++)
						{
							CATran tran = (CATran)PXSelectReadonly<CATran, Where<CATran.tranID, Equal<Required<CATran.tranID>>>>.Select(graph, ((CATran)tranList[i]).TranID);
							if (tran != null && tran.Reconciled == true && tran.Released != true)
							{
								releaseComplete = false;
								break;	
							}
						}
					}

					if (releaseComplete == true)
					{
						using (new PXConnectionScope())
						{
							using (PXTransactionScope ts = new PXTransactionScope())
							{
								recon.Reconciled = true;
								graph.CAReconRecords.Update(recon);
								graph.Save.Press();
								ts.Complete();
							}
						}
					}
					else
						throw new Exception(Messages.OneOrMoreItemsAreNotReleasedAndStatementCannotBeCompleted);
				}
				catch(Exception)
				{
					throw new Exception(Messages.OneOrMoreItemsAreNotReleasedAndStatementCannotBeCompleted);
				}
                    
            }

			public virtual CARecon previousHeader(CARecon _lastHeader)
			{
				CARecon previousHeader = null;
				if (_lastHeader != null && _lastHeader.CashAccountID != null)
				{
					PXSelectBase<CARecon> selectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>, And<CARecon.voided, NotEqual<True>>>,
																						  OrderBy<Asc<CARecon.cashAccountID, Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>>(this);

					previousHeader = (CARecon)selectStatement.View.SelectSingle(_lastHeader.CashAccountID);
				}
				return previousHeader;
			}

			public virtual bool headerInsertEnabled(CARecon _previousHeader)
			{
				return ((_previousHeader == null) || (_previousHeader.Reconciled == true) || _previousHeader.Voided == true);
			}
			
			public virtual bool headerUpdateEnabled(CARecon _header)
			{
                bool ret = ((_header.Reconciled != true) && (_header.Voided != true) && (this.insertEnabled));
				return	ret;
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
						e.Cancel   = true;
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
				if (CAReconRecords.Current != null)
				{
					e.NewValue = CAReconRecords.Current.ReconDate;
					e.Cancel = true;
				}
			}

			protected virtual void CurrencyInfo_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
			{
				if (AddFilter.Current != null && AddFilter.Current.CuryInfoID == ((CurrencyInfo)e.Row).CuryInfoID)
				{
					e.Cancel = true;
				} 
			}
 
		#endregion
		
		#region CATran Events
			protected virtual void CATranExt_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
			{
				CATranExt row = (CATranExt)e.Row;
                bool skipVoids = this.casetup.Current.SkipVoided ?? false;
				if (row.Reconciled == true)
				{
					if (row.Cleared != true)
					{
						sender.RaiseExceptionHandling<CATran.cleared>(row, row.Cleared, new PXSetPropertyException(Messages.ReconciledDocCanNotBeNotCleared));    	
					}

					if (row.ClearDate == null)
					{
						sender.RaiseExceptionHandling<CATran.clearDate>(row, row.ClearDate, new PXSetPropertyException(Messages.ReconciledDocCanNotBeNotCleared));
                    }

                    if (row.Released == false) 
                    {
                        sender.RaiseExceptionHandling<CATran.reconciled>(row, row.Reconciled, new PXSetPropertyException(Messages.NotReleasedDocCanNotAddToReconciliation));
                    }

                    if (skipVoids && row.VoidingTranID.HasValue && row.VoidingNotReleased== true) 
                    {
                        sender.RaiseExceptionHandling<CATran.reconciled>(row, row.ClearDate, new PXSetPropertyException(Messages.VoidedTransactionHavingNotReleasedVoidCannotBeAddedToReconciliation));
                    }
                }

				if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
					(e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
				{
					CARecon header = CAReconRecords.Current;
					if (row.Reconciled == true)
					{
						row.ReconNbr  = header.ReconNbr;
						row.ReconDate = header.ReconDate;
					}
					else
					{
						row.ReconNbr  = null;
						row.ReconDate = null;
					}
				}
			}
			protected virtual void CATranExt_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
			{
				CATran row = (CATran)e.Row;

				if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
					(e.Operation & PXDBOperation.Command) == PXDBOperation.Update) &&
					e.TranStatus == PXTranStatus.Aborted && row.Reconciled == true)
				{
					CARecon header = CAReconRecords.Current;
					row.ReconNbr = header.ReconNbr;
					row.ReconDate = header.ReconDate;
				}
			}
			protected virtual void CATranExt_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
			{
				CATranExt    row = (CATranExt)e.Row;
				CARecon recon = CAReconRecords.Current;
                bool skipVoids = this.casetup.Current.SkipVoided??false;
				if (row == null || recon == null) return;
                bool isReconcilable = (recon.Reconciled != true);
				PXUIFieldAttribute.SetEnabled(cache, row, false);
				PXUIFieldAttribute.SetEnabled<CATran.tranID>	(cache, row, false);
                PXUIFieldAttribute.SetEnabled<CATran.hold>      (cache, row, false);
				PXUIFieldAttribute.SetEnabled<CATran.reconciled>(cache, row, isReconcilable);
				bool allowClear = isReconcilable;
				if (allowClear)
				{					
					PXResult<CABankStatementDetail, CABankStatement> result = (PXResult<CABankStatementDetail, CABankStatement>) PXSelectJoin<CABankStatementDetail, 
																	  InnerJoin<CABankStatement, On<CABankStatementDetail.refNbr, Equal<CABankStatement.refNbr>>>,
																				  Where<CABankStatementDetail.cATranID, Equal<Required<CABankStatementDetail.cATranID>>>>.Select(this, row.TranID);
					if(result!= null)
					{
						CABankStatementDetail bankStatementDetail = result;
						CABankStatement bankStatement = result;
						if (bankStatement != null && bankStatement.CashAccountID.HasValue && bankStatement.Released == true)
						{
							allowClear = false;						
						}
					}					
				}
                 
				PXUIFieldAttribute.SetEnabled<CATran.cleared>(CAReconTranRecords.Cache, row, allowClear);
				PXUIFieldAttribute.SetEnabled<CATran.clearDate>(CAReconTranRecords.Cache, row, allowClear && row.Cleared == true);
                if (isReconcilable)
                {
                    if (row.Released != true || (skipVoids && row.VoidingNotReleased == true && row.VoidingTranID.HasValue))
                    {
                        PXUIFieldAttribute.SetEnabled(cache, row, false);
                        if(skipVoids && row.VoidingNotReleased == true && row.VoidingTranID.HasValue && row.Reconciled == true)
                        {
                            PXUIFieldAttribute.SetEnabled<CATran.reconciled>(cache, row, true);
                        }
                    }
                }
            }

            protected virtual void CATranExt_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
            {
                CATranExt row = (CATranExt)e.Row;
                CATranExt oldRow = (CATranExt)e.OldRow;
                CARecon recon = CAReconRecords.Current;
                if (row == null || recon == null) return;
                bool skipVoids = this.casetup.Current.SkipVoided ?? false;
                CATran2 voidingTran = null;
                if (skipVoids && row.VoidingTranID.HasValue)
                {
                    voidingTran = this.VoidingTrans.Select(row.CashAccountID, row.VoidingTranID);
                    if (voidingTran != null && voidingTran.TranID.HasValue) 
                    {
                        bool isModified = false;
                        if(row.Cleared != oldRow.Cleared || row.ClearDate!=oldRow.ClearDate)
                        {
                            voidingTran.ClearDate = row.ClearDate;
                            voidingTran.Cleared = row.Cleared;
                            isModified = true;
                        }
                        if (row.Reconciled != oldRow.Reconciled || row.ReconDate != oldRow.ReconDate || row.ReconNbr != oldRow.ReconNbr) 
                        {
                            isModified = true;
                            voidingTran.Reconciled = row.Reconciled;
                            voidingTran.ReconDate = row.ReconDate;
                            voidingTran.ReconNbr = row.ReconNbr;
                        }
                        if (isModified) 
                        {
                            voidingTran = (CATran2)this.VoidingTrans.Update(voidingTran);
                        }
                    }
                }
                
            }
			protected virtual void CATranExt_Reconciled_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
			{
				CATranExt row = (CATranExt)e.Row;
                bool skipVoids = this.casetup.Current.SkipVoided ?? false;
				if ((bool)e.NewValue == true)
				{
					if (row.Hold == true)
					{
						e.NewValue = false;
						e.Cancel   = true;
						throw new PXSetPropertyException(Messages.HoldDocCanNotAddToReconciliation);
					}
					if (row.Released != true)
					{
						e.NewValue = false;
						e.Cancel   = true;						
						throw new PXSetPropertyException(Messages.NotReleasedDocCanNotAddToReconciliation);
					}
                    if (skipVoids == true && row.VoidingTranID.HasValue && row.VoidingNotReleased == true) 
                    {
                        e.NewValue = false;
                        e.Cancel = true;
                        throw new PXSetPropertyException(Messages.TransactionsWithVoidPendingStatusMayNotBeAddedToReconciliation);
                    }

                    if (skipVoids == true && row.VoidedTranID.HasValue)
                    {
                        e.NewValue = false;
                        e.Cancel = true;
                        throw new PXSetPropertyException(Messages.TransactionsWithVoidPendingStatusMayNotBeAddedToReconciliation);
                    }
                }
			}
			protected virtual void CATranExt_Cleared_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
			{
				CATranExt row = (CATranExt)e.Row;
				if (row.Cleared == true)
				{
					row.ClearDate = CAReconRecords.Current.ReconDate;
				}
				else
				{
					row.ClearDate = null;
				}                
			}
			protected virtual void CATranExt_Reconciled_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
			{
				CATranExt row = (CATranExt)e.Row;
                
				if (row.Reconciled == true)
				{
					row.Cleared   = true;
					row.ReconNbr  = CAReconRecords.Current.ReconNbr;
					row.ReconDate = CAReconRecords.Current.ReconDate;
					if (row.ClearDate == null)
					{
						row.ClearDate = CAReconRecords.Current.ReconDate;
					}                
				}
				else
				{
					row.ReconNbr = null;
					row.ReconDate = null;                   
				}				
			}
            protected virtual void CATranExt_Status_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
            {
                CATranExt row = (CATranExt)e.Row;
                bool skipVoids = (this.casetup.Current.SkipVoided ?? false);
                if (row != null)
                {
                    Dictionary<long, CAMessage> listMessages = PXLongOperation.GetCustomInfo(this.UID) as Dictionary<long, CAMessage>;
                    TimeSpan timespan;
                    Exception ex;
                    PXLongRunStatus status = PXLongOperation.GetStatus(this.UID, out timespan, out ex);
                    string fieldName = typeof(CATranExt.status).Name;
                    if ((status == PXLongRunStatus.Aborted || status == PXLongRunStatus.Completed) && listMessages != null)
                    {
                        CAMessage message = null;
                        if (listMessages.ContainsKey(row.TranID.Value))
                            message = listMessages[row.TranID.Value];
                        if (message != null)
                        {
                            
                            e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(String), false, null, null, null, null, null, fieldName,
                                        null, null, message.Message, message.ErrorLevel, null, null, null, PXUIVisibility.Undefined, null, null, null);
                            e.IsAltered = true;
                        }
                    }
                    else
                    {
                        string message = String.Empty;
                        PXErrorLevel msgLevel = PXErrorLevel.RowWarning;
                        if (skipVoids && row.VoidingNotReleased == true && row.VoidingTranID.HasValue)
                        {
                            message = CA.Messages.VoidedTransactionHavingNotReleasedVoidCannotBeAddedToReconciliation;
                            if (row.Reconciled == true)
                            {
                                msgLevel = PXErrorLevel.RowError;
                            }
                        }
                        else if (row.Released != true)
                        {
                            message = Messages.NotReleasedDocCanNotAddToReconciliation;
                        }
                        else if (row.Hold == true)
                        {
                            message = Messages.NotReleasedDocCanNotAddToReconciliation;
                        }

                        if (string.IsNullOrEmpty(message) == false)
                        {
                            e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(String), false, null, null, null, null, null, fieldName,
                             null, null, message, msgLevel, null, null, null, PXUIVisibility.Undefined, null, null, null);
                            e.IsAltered = true;
                        }

                    }                        
                }
            }

            protected virtual void CATran2_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
            {
                CATran2 row = (CATran2)e.Row;                
                if (row.Reconciled == true)
                {
                    if (row.Cleared != true)
                    {
                        sender.RaiseExceptionHandling<CATran.cleared>(row, row.Cleared, new PXSetPropertyException(Messages.ReconciledDocCanNotBeNotCleared));
                    }
                    if (row.ClearDate == null)
                    {
                        sender.RaiseExceptionHandling<CATran.clearDate>(row, row.ClearDate, new PXSetPropertyException(Messages.ReconciledDocCanNotBeNotCleared));
                    }                  
                }

                if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
                    (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
                {
                    CARecon header = CAReconRecords.Current;
                    if (row.Reconciled == true)
                    {
                        row.ReconNbr = header.ReconNbr;
                        row.ReconDate = header.ReconDate;
                    }
                    else
                    {
                        row.ReconNbr = null;
                        row.ReconDate = null;
                    }
                }
            }
            protected virtual void CATran2_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
            {
                CATran2 row = (CATran2)e.Row;

                if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert ||
                    (e.Operation & PXDBOperation.Command) == PXDBOperation.Update) &&
                    e.TranStatus == PXTranStatus.Aborted && row.Reconciled == true)
                {
                    CARecon header = CAReconRecords.Current;
                    row.ReconNbr = header.ReconNbr;
                    row.ReconDate = header.ReconDate;
                }
            }


		#endregion

		#region CARecon Events
		protected virtual void CARecon_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            CARecon header = (CARecon)e.Row;
            foreach (CATranExt res in CAReconTranRecords.Select())
            {
                if (res.Reconciled == true)
                {
					CATranExt newTran = (CATranExt)CAReconTranRecords.Cache.CreateCopy(res);
                    newTran.Reconciled = false;
					CAReconTranRecords.Update(newTran);
                }
            }
			if (CAReconRecords.Cache.GetStatus(header) != PXEntryStatus.Inserted)
			{
				Save.Press();
			}
        }
		protected virtual void CARecon_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			CARecon header		   = (CARecon)e.Row;
			CARecon previousHeader = this.previousHeader(header);
			if (  (previousHeader != null) &&
				  (	(previousHeader.ReconNbr != header.ReconNbr) && (cache.GetStatus(header) != PXEntryStatus.Inserted) || (cache.GetStatus(header) == PXEntryStatus.Inserted)) &&
				  (previousHeader.ReconDate >= header.ReconDate))
			{
				cache.RaiseExceptionHandling<CARecon.reconDate>(header, header.ReconDate, new PXSetPropertyException(Messages.PrevStatementHasGreaterOREqualDate));    	
			}

			if (header.Hold != true && header.Voided != true)
            {
                if (header.CuryDiffBalance != 0)
                {
                    cache.RaiseExceptionHandling<CARecon.curyBalance>(header, header.CuryBalance, new PXSetPropertyException(Messages.DocumentOutOfBalance));
                }
            }
		}
		protected virtual void CARecon_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CARecon header = (CARecon)e.Row;
            if (header == null) return;

			if (header.CashAccountID != null && cashaccount.Current.CashAccountID != header.CashAccountID)
			{
				cashaccount.Current = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, header.CashAccountID);
			}
			if (cashaccount.Current != null && AddFilter.Current!= null && AddFilter.Current.CashAccountID != header.CashAccountID)
			{
                AddFilter.Cache.SetValueExt<AddTrxFilter.cashAccountID>(AddFilter.Current, cashaccount.Current.CashAccountCD);
			}

            header.CuryReconciledTurnover = header.CuryReconciledDebits - header.CuryReconciledCredits;
            header.CuryReconciledBalance  = header.CuryBegBalance       + header.CuryReconciledTurnover;
            header.CuryDiffBalance        = header.CuryBalance          - header.CuryReconciledBalance;

			//PXUIFieldAttribute.SetEnabled(cache, header, false);
			PXUIFieldAttribute.SetEnabled<CARecon.reconNbr>		(cache, header, true);
            PXUIFieldAttribute.SetEnabled<CARecon.cashAccountID>(cache, header, true);
			
            bool headerReconciled = (header.Reconciled == true);
            bool headerOnHold     = (header.Hold == true);
		    bool headerVoided = (header.Voided == true);
            bool headerEnalable   = headerOnHold && !headerReconciled && !headerVoided;




			CARecon previousHeader		 = this.previousHeader(header);
            bool voidedEnabled			 = (previousHeader != null) && (previousHeader.Reconciled == true) && (previousHeader.ReconNbr == header.ReconNbr);
            insertEnabled				 = header.CashAccountID != null;
            updateEnabled				 = this.headerUpdateEnabled(header);
            bool deleteEnabled			 = !headerReconciled && !headerVoided && (previousHeader != null) && (previousHeader.ReconNbr == header.ReconNbr);  
			
			bool releaseEnabled			 = !headerReconciled && !headerOnHold && !headerVoided;
 
            Voided.SetEnabled(voidedEnabled);

            CAReconRecords.Cache.AllowUpdate = updateEnabled;
            CAReconRecords.Cache.AllowDelete = deleteEnabled;
            CAReconRecords.Cache.AllowInsert = insertEnabled;

            CAReconTranRecords.Cache.AllowDelete = false; // updateEnabled;
            CAReconTranRecords.Cache.AllowInsert = false; // updateEnabled;
            CAReconTranRecords.Cache.AllowUpdate = updateEnabled;
            CreateAdjustment.SetEnabled(updateEnabled);

            PXUIFieldAttribute.SetEnabled<CARecon.curyBalance>(cache, header, !headerReconciled && !headerVoided);
            PXUIFieldAttribute.SetEnabled<CARecon.hold>(cache, header, !headerReconciled && !headerVoided);
            PXUIFieldAttribute.SetEnabled<CARecon.reconDate>(cache, header, !headerReconciled && !headerVoided);
            PXUIFieldAttribute.SetEnabled<CARecon.loadDocumentsTill>(cache, header, !headerReconciled && !headerVoided);
            
			AddFilter.Cache.RaiseRowSelected(AddFilter.Current);
            Release.SetEnabled(releaseEnabled);
			
			CARecon cachesReconNext	= null;
			CARecon cachesReconPrev	= null;
			PXSelectBase<CARecon> currentSelectStatement;
		
			bool nextEnabled  = false;
			bool prevEnabled  = false;
			bool lastEnabled  = false;
			bool firstEnabled = false;

			if(CAReconRecords.Cache.GetStatus(header) == PXEntryStatus.Inserted)
			{ 	
				currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>>,
																		 OrderBy<Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>(this);
				cachesReconPrev = (CARecon)currentSelectStatement.View.SelectSingle(header.CashAccountID);
				
				prevEnabled  = cachesReconPrev != null;
			}
			else
			{
				currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
																		  And<CARecon.reconNbr, Greater<Required<CARecon.reconNbr>>>>,
																		 OrderBy<Asc<CARecon.reconDate, Asc<CARecon.reconNbr>>>>(this);
				cachesReconNext = (CARecon)currentSelectStatement.View.SelectSingle(header.CashAccountID, header.ReconNbr);

				currentSelectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
																		  And<CARecon.reconNbr, Less<Required<CARecon.reconNbr>>>>,
																		 OrderBy<Desc<CARecon.reconDate, Desc<CARecon.reconNbr>>>>(this);
				cachesReconPrev = (CARecon)currentSelectStatement.View.SelectSingle(header.CashAccountID, header.ReconNbr);
				nextEnabled  = cachesReconNext != null;
				prevEnabled  = cachesReconPrev != null;
				
			}

			firstEnabled = (header.CashAccountID != null) && prevEnabled;
			lastEnabled  = (header.CashAccountID != null) && nextEnabled;
		    
			next.SetEnabled (nextEnabled);
		    prev.SetEnabled (prevEnabled);
			first.SetEnabled(firstEnabled);
			last.SetEnabled (lastEnabled);

            bool CuryViewStateNotSet = (this.Accessinfo.CuryViewState != true);
            PXUIFieldAttribute.SetVisible<CARecon.curyBegBalance>       (cache, header, CuryViewStateNotSet);
            PXUIFieldAttribute.SetVisible<CARecon.curyBalance>          (cache, header, CuryViewStateNotSet);
            PXUIFieldAttribute.SetVisible<CARecon.curyDiffBalance>      (cache, header, CuryViewStateNotSet);
            PXUIFieldAttribute.SetVisible<CARecon.curyReconciledBalance>(cache, header, CuryViewStateNotSet);
		}

        protected virtual void CARecon_Hold_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{

            CARecon header = (CARecon)e.Row;

            if ((bool)e.NewValue != true)
            {
                if (header.CuryDiffBalance != null && (Decimal)header.CuryDiffBalance != 0)
                {
                    cache.RaiseExceptionHandling<CARecon.curyBalance>(header, header.CuryBalance, new PXSetPropertyException(Messages.DocumentOutOfBalance));
                }
            }
        }
        protected virtual void CARecon_ReconDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
			CARecon header		   = (CARecon)e.Row;
			CARecon previousHeader = this.previousHeader(header);

			if (previousHeader != null && previousHeader.LastReconDate != null && e.NewValue != null
             && ((DateTime)e.NewValue).Date <= previousHeader.LastReconDate.Value.Date)
            {
                throw new PXSetPropertyException(Messages.ReconDateNotAvailable);
            }
        }
        protected virtual void CARecon_LoadDocumentsTill_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            CARecon header = (CARecon)e.Row;
            if (e.NewValue != null && (DateTime)e.NewValue < header.ReconDate)
            {
                cache.RaiseExceptionHandling<CARecon.loadDocumentsTill>(header, (DateTime)e.NewValue, new PXSetPropertyException(Messages.LastDateToLoadNotAvailable, PXErrorLevel.Warning));
            }
        }

		protected virtual void CARecon_ReconDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
			CARecon header = (CARecon)e.Row;
			if (header == null) return;
			header.LoadDocumentsTill = header.ReconDate;

			CATranExt newrow;
			foreach (CATranExt res in PXSelect<CATranExt, Where<CATranExt.reconNbr, Equal<Required<CARecon.reconNbr>>, And <CATranExt.cashAccountID, Equal<Required<CARecon.cashAccountID>>>>>
												  .Select(this, header.ReconNbr, header.CashAccountID))
			{
				if (res.Reconciled == true)
				{
					newrow = PXCache<CATranExt>.CreateCopy(res);
					newrow.ReconDate = header.ReconDate;
					CAReconTranRecords.Update(newrow);
				}
			}
		}

		protected virtual void CARecon_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			CARecon row = (CARecon)e.Row;
			CARecon oldRow = (CARecon)e.OldRow;
			if ( oldRow.ReconDate != row.ReconDate 
					|| oldRow.LoadDocumentsTill != row.LoadDocumentsTill ) 
			{
				this.CAReconTranRecords.View.RequestRefresh();
			}		
		}

		protected virtual void CARecon_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            CARecon header = (CARecon)e.Row;
            if (CMSetup.Current.MCActivated == true)
            {
                CurrencyInfo info = CurrencyInfoAttribute.SetDefaults<CARecon.curyInfoID>(cache, header);

                string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyID>(currencyinfo.Cache, info);
                if (string.IsNullOrEmpty(message) != true)
                {
                    throw new PXSetPropertyException(message, PXErrorLevel.Error);
                }
				
				message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(currencyinfo.Cache, info);
                if (string.IsNullOrEmpty(message) != true)
                {
                    throw new PXSetPropertyException(message, PXErrorLevel.Error);
                }

                if (info != null)
                {
                    header.CuryID = info.CuryID;
                }
            }
            PXSelectBase<CARecon> selectStatement = new PXSelectReadonly<CARecon, Where<CARecon.cashAccountID, Equal<Required<CARecon.cashAccountID>>,
                                                                                    And<CARecon.reconNbr, NotEqual<Required<CARecon.reconNbr>>, And<CARecon.voided, NotEqual<True>>>>,
                                                                                OrderBy<Asc<CARecon.cashAccountID, Desc<CARecon.reconDate>>>>(this);
			CARecon lastDoc = (CARecon)selectStatement.View.SelectSingle(header.CashAccountID, header.ReconNbr);

            if ((lastDoc != null) && (lastDoc.ReconDate != null))
            {
                if (lastDoc.Reconciled == true)
                {
                    header.LastReconDate         = lastDoc.ReconDate;
                    header.CuryBegBalance        = lastDoc.CuryBalance;
                    header.CuryReconciledBalance = header.CuryBegBalance + header.CuryReconciledDebits - header.CuryReconciledCredits;
                    header.CuryDiffBalance       = header.CuryBalance - header.CuryReconciledBalance;
                }
                else
                {
                    cache.RaiseExceptionHandling<CARecon.reconNbr>(header, header.ReconNbr, new PXSetPropertyException(Messages.PrevStatementNotReconciled));    
                }
            }
		}
		#endregion
	}

	[TableAndChartDashboardType]
	public class CAReconEnq : PXGraph<CAReconEnq>
	{
        #region Buttons Definition
			#region Button Cancel
				public PXAction<CAEnqFilter> cancel;
		        
				[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
				[PXCancelButton]
				protected virtual IEnumerable Cancel(PXAdapter adapter)
				{
					CAReconRecords.Cache.Clear();
					Filter.Cache.Clear();
					TimeStamp = null;
					PXLongOperation.ClearStatus(this.UID);
					return adapter.Get();
				}
			#endregion             
			#region Button Voided
				public PXAction<CAEnqFilter> voided;
				[PXUIField(DisplayName = "Void", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
				[PXProcessButton]
				public virtual IEnumerable Voided(PXAdapter adapter)
				{
					CARecon recon = CAReconRecords.Current;
					if (recon != null)
					{
						PXLongOperation.StartOperation(this, delegate() { CAReconEntry.ReconVoided(recon); });
						CAReconRecords.View.RequestRefresh();
					}
					return adapter.Get();
				}
			#endregion
			#region Button CreateRecon
				public PXAction<CAEnqFilter> createRecon;
				[PXUIField(DisplayName = "Create Reconciliation", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
				[PXProcessButton]
				public virtual IEnumerable CreateRecon(PXAdapter adapter)
				{
					if (this.Views.ContainsKey("cashAccountFilter"))
					{
						CashAccountFilter createReconFilter = cashAccountFilter.Current;
						WebDialogResult result = this.Views["cashAccountFilter"].AskExt();
						if (result == WebDialogResult.OK)
						{
							CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<AddTrxFilter.cashAccountID>>>>.Select(this, createReconFilter.CashAccountID);
							CAReconEntry.ReconCreate(acct);
							CAReconRecords.View.RequestRefresh();
						}
					}
					
					return adapter.Get();
				}
			#endregion
			#region Button ViewDoc
				public PXAction<CAEnqFilter> viewDoc;
				[PXUIField(DisplayName = "View Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
				[PXLookupButton]
				public virtual IEnumerable ViewDoc(PXAdapter adapter)
				{
					CARecon curRecon = CAReconRecords.Current;
					CAReconEntry graph = PXGraph.CreateInstance<CAReconEntry>();
					graph.CAReconRecords.Current = curRecon;
					throw new PXRedirectRequiredException(graph, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
				}
			#endregion
		#endregion

		#region Variables
			public PXFilter<CAEnqFilter> Filter;
			[PXFilterable]
			public PXSelect<CARecon> CAReconRecords;
			public PXFilter<CashAccountFilter> cashAccountFilter;
			public PXSetup<CASetup> casetup;
			public PXSelect<CashAccount> cashAccount;
		#endregion
		
		#region Execute Select
			protected virtual IEnumerable careconrecords()
			{
				List<CAReconMessage> listMessages = PXLongOperation.GetCustomInfo(this.UID) as List<CAReconMessage>;
				CAEnqFilter filter = Filter.Current;
				foreach (CARecon recon in PXSelect<CARecon, Where2<Where<CARecon.cashAccountID, Equal<Required<CAEnqFilter.accountID>>,
																	  Or<Required<CAEnqFilter.accountID>, IsNull>>,
																And2<Where<CARecon.reconDate, GreaterEqual<Required<CAEnqFilter.startDate>>,
																		Or<Required<CAEnqFilter.startDate>, IsNull>>,
																And<Where<CARecon.reconDate, LessEqual<Required<CAEnqFilter.endDate>>,
																		Or<Required<CAEnqFilter.endDate>, IsNull>>>>>,
														   OrderBy<Asc<CARecon.reconDate, Asc<CARecon.reconNbr>>>>
						.Select(this, filter.AccountID, filter.AccountID, filter.StartDate, filter.StartDate, filter.EndDate, filter.EndDate))
				{
					TimeSpan timespan;
					Exception ex;
					if ((PXLongOperation.GetStatus(UID, out timespan, out ex) == PXLongRunStatus.Aborted || PXLongOperation.GetStatus(UID, out timespan, out ex) == PXLongRunStatus.Completed) && 
						listMessages != null && listMessages.Count > 0)
						for(int i=0; i<listMessages.Count; i++)
						{
							CAReconMessage message = (CAReconMessage)listMessages[i];
							if (message.KeyCashAccount == recon.CashAccountID && message.KeyReconNbr == recon.ReconNbr)
							{
								CAReconRecords.Cache.RaiseExceptionHandling<CARecon.reconNbr>(recon, recon.ReconNbr, new PXSetPropertyException(message.Message, message.ErrorLevel));	
							}
						}
					yield return recon;
				}
			}
		#endregion
		
		#region CAEnqFilter Events 
		protected virtual void CAEnqFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
            CAEnqFilter filter = (CAEnqFilter) e.Row;
            if (filter == null) return;
			PXCache reconCache = CAReconRecords.Cache;
			reconCache.AllowInsert = false;
			reconCache.AllowUpdate = false;
			reconCache.AllowDelete = false;			
			CashAccountFilter reconCreateFilter = cashAccountFilter.Current;
			cashAccountFilter.Cache.RaiseRowSelected(reconCreateFilter);
		}
		#endregion
		
		#region CashAccountFilter Events
		protected virtual void CashAccountFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			CashAccountFilter reconCreateFilter = (CashAccountFilter)e.Row;
			cache.AllowUpdate = true;
			PXUIFieldAttribute.SetEnabled(cache, reconCreateFilter, false);
			PXUIFieldAttribute.SetEnabled<CashAccountFilter.cashAccountID>(cache, reconCreateFilter, true);
		}
		protected virtual void CashAccountFilter_CashAccountID_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
		{
			CashAccountFilter createReconFilter = (CashAccountFilter)e.Row;
			if (createReconFilter == null) return;
            CashAccount acct = PXSelect<CashAccount, Where<CashAccount.cashAccountCD, Equal<Required<CashAccount.cashAccountCD>>>>.Select(this, (string)e.NewValue);
			if (acct != null && acct.Reconcile != true)
			{
				e.Cancel = true;
				throw new Exception(Messages.CashAccounNotReconcile);
			}
		}
		#endregion		
			
	}
}
