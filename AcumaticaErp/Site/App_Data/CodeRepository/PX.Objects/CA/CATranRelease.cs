using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.TX;
using PX.Objects.AP;
using PX.Objects.AR;

namespace PX.Objects.CA
{
	[System.SerializableAttribute()]
	public partial class CARegister : PX.Data.IBqlTable
	{
		#region TranID
		public abstract class tranID : PX.Data.IBqlField
		{
		}
		protected Int64? _TranID;
		[PXLong(IsKey = true)]
		[PXUIField(DisplayName = "Transaction Num.")]
		public virtual Int64? TranID
		{
			get
			{
				return this._TranID;
			}
			set
			{
				this._TranID = value;
			}
		}
		#endregion
		#region Selected
		public abstract class selected : IBqlField
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
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
		#region Hold
		public abstract class hold : IBqlField
		{
		}
		protected bool? _Hold = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Hold")]
		public bool? Hold
		{
			get
			{
				return _Hold;
			}
			set
			{
				_Hold = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : IBqlField
		{
		}
		protected bool? _Released = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released")]
		public bool? Released
		{
			get
			{
				return _Released;
			}
			set
			{
				_Released = value;
			}
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.IBqlField
		{
		}
		protected String _TranType;
		[PXString(3)]
		[CAAPARTranType.List()]
		[PXUIField(DisplayName = "Transaction Type")]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region Module
		public abstract class module : PX.Data.IBqlField
		{
		}
		protected String _Module;
		[PXString(3)]
		[GL.BatchModule.List()]
		[PXUIField(DisplayName = "Module")]
		public virtual String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region ReferenceNbr
		public abstract class referenceNbr : PX.Data.IBqlField
		{
		}
		protected String _ReferenceNbr;
		[PXString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Transaction Number", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String ReferenceNbr
		{
			get
			{
				return this._ReferenceNbr;
			}
			set
			{
				this._ReferenceNbr = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.IBqlField
		{
		}
		protected String _Description;
		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
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
		#region DocDate
		public abstract class docDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _DocDate;
		[PXDate()]
		[PXUIField(DisplayName = "Doc. Date")]
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
		[FinPeriodID()]
		[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region NoteID
		public abstract class noteID : PX.Data.IBqlField
		{
		}
		protected Int64? _NoteID;
		[PXNote()]
		public virtual Int64? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.IBqlField
		{
		}
		protected Int32? _CashAccountID;
		[PXDefault()]
		[GL.CashAccount(DisplayName = "Cash Account", Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(CashAccount.descr))]
		public virtual Int32? CashAccountID
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
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Enabled = false)]
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
		#region TranAmt
		public abstract class tranAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tran. Amount", Enabled = false)]
		public virtual Decimal? TranAmt
		{
			get
			{
				return this._TranAmt;
			}
			set
			{
				this._TranAmt = value;
			}
		}
		#endregion
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranAmt;
		[PXDBCury(typeof(CARecon.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount", Enabled = false)]
		public virtual Decimal? CuryTranAmt
		{
			get
			{
				return this._CuryTranAmt;
			}
			set
			{
				this._CuryTranAmt = value;
			}
		}
		#endregion
	}

	public abstract class InfoMessage 
	{
        public InfoMessage(PXErrorLevel aLevel, string aMessage) 
        {
            this._ErrorLevel = aLevel;
            this._Message = aMessage;
        } 

		#region PXErrorLevel
		protected PXErrorLevel _ErrorLevel;
	    public virtual PXErrorLevel ErrorLevel
		{
			get
			{
				return this._ErrorLevel;
			}
			set
			{
				this._ErrorLevel = value;
			}
		}
		#endregion
		#region Message
		
		protected String _Message;
		public virtual String Message
		{
			get
			{
				return this._Message;
			}
			set
			{
				this._Message = value;
			}
		}
		#endregion
	}

	public class CAMessage : InfoMessage
	{
        public CAMessage(long aKey, PXErrorLevel aLevel, string aMessage):base(aLevel, aMessage) 
        {
            this._Key = aKey;            
        } 
		#region Key
		protected Int64 _Key;
		public virtual Int64 Key
		{
			get
			{
				return this._Key;
			}
			set
			{
				this._Key = value;
			}
		}
		#endregion      
	}

	public class CAReconMessage : InfoMessage
	{
        public CAReconMessage(int aCashAccountID, string aReconNbr, PXErrorLevel aLevel, string aMessage): 
                base(aLevel,aMessage) 
        {
            this._KeyCashAccount = aCashAccountID;
            this._KeyReconNbr = aReconNbr;            
        }

		#region KeyCashAccount
		public abstract class keyCashAccount : PX.Data.IBqlField
		{
		}
		protected Int32 _KeyCashAccount;
		public virtual Int32 KeyCashAccount
		{
			get
			{
				return this._KeyCashAccount;
			}
			set
			{
				this._KeyCashAccount = value;
			}
		}
		#endregion
		#region KeyReconNbr
		protected String _KeyReconNbr;
		public virtual String KeyReconNbr
		{
			get
			{
				return this._KeyReconNbr;
			}
			set
			{
				this._KeyReconNbr = value;
			}
		}
		#endregion        
	}

	[System.SerializableAttribute()]
	[PXPrimaryGraph(new Type[] {
					typeof(CATranEntry),
					typeof(CashTransferEntry)},
						new Type[] {
					typeof(Select<CAAdj, Where<CAAdj.tranID, Equal<Current<CATran.tranID>>>>),
					typeof(Select<CATransfer, Where<CATransfer.tranIDIn, Equal<Current<CATran.tranID>>, 
							Or<CATransfer.tranIDOut, Equal<Current<CATran.tranID>>>>>) 
					})]
	[TableAndChartDashboardType]    
	public class CATrxRelease : PXGraph<CATrxRelease>
	{
		/// <summary>
		/// CashAccount override - SQL Alias
		/// </summary>
        [Serializable]
		public class CashAccount1: CashAccount 
		{
			public new abstract class cashAccountID : PX.Data.IBqlField
			{
			}
		}
		public CATrxRelease()
		{
			CASetup setup = cASetup.Current;
			CARegisterList.SetProcessDelegate(delegate(List<CARegister> list) 
            {
                GroupRelease(list, true); 
            });
            CARegisterList.SetProcessCaption("Release");
            CARegisterList.SetProcessAllCaption("Release All");
			
		}
		#region Buttons
		#region Cancel
		public PXAction<CARegister> cancel;
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected virtual IEnumerable Cancel(PXAdapter adapter)
		{
			CARegisterList.Cache.Clear();
			TimeStamp = null;
			PXLongOperation.ClearStatus(this.UID);
			return adapter.Get();
		}
		#endregion

		#region viewCATrax
		public PXAction<CARegister> viewCATrx;
		[PXUIField(DisplayName = "View Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewCATrx(PXAdapter adapter)
		{
			CARegister register = CARegisterList.Current;
			if (register != null)
			{
				switch (register.TranType)
				{
					case (CAAPARTranType.CAAdjustment):
						CATranEntry graphTranEntry = PXGraph.CreateInstance<CATranEntry>();
						graphTranEntry.Clear();
						CARegisterList.Cache.IsDirty = false;
						graphTranEntry.CAAdjRecords.Current = PXSelect<CAAdj, Where<CAAdj.adjRefNbr, Equal<Required<CATran.origRefNbr>>>>
							.Select(this, register.ReferenceNbr); // !!!
						throw new PXRedirectRequiredException(graphTranEntry, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					//break;
					case (CAAPARTranType.CATransfer):
						CashTransferEntry graphTransferEntry = PXGraph.CreateInstance<CashTransferEntry>();
						graphTransferEntry.Clear();
						CARegisterList.Cache.IsDirty = false;
						graphTransferEntry.Transfer.Current = PXSelect<CATransfer, Where<CATransfer.transferNbr, Equal<Required<CATransfer.transferNbr>>>>
							.Select(this, register.ReferenceNbr);
						throw new PXRedirectRequiredException(graphTransferEntry, true, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
					//break;
				}
			}
			return CARegisterList.Select();
		}
		#endregion

		#endregion

		#region selectStatements
		[PXFilterable]
		public PXProcessing<CARegister> CARegisterList;
		public PXSetup<CASetup> cASetup;
		#endregion

		#region Function
		protected virtual IEnumerable caregisterlist()
		{
			bool anyFound = false;       
			foreach (CARegister tlist in CARegisterList.Cache.Inserted)
			{
				anyFound = true;				
				yield return tlist;
			}

			if (anyFound)
			{
				yield break;
			}

			foreach (CAAdj adj in PXSelectJoin<CAAdj, InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CAAdj.cashAccountID>, And<Match<CashAccount,Current<AccessInfo.userName>>>>>, Where<CAAdj.released, Equal<boolFalse>, And<CAAdj.status, Equal<CATransferStatus.balanced>, And<CAAdj.adjTranType, Equal<CATranType.cAAdjustment>>>>>.Select(this))
			{
				if (adj.TranID != null)
					yield return CARegisterList.Cache.Insert(CARegister(adj));
			}

			foreach (CATransfer trf in PXSelectJoin<CATransfer,
											InnerJoin<CashAccount,On<CashAccount.cashAccountID,Equal<CATransfer.inAccountID>,And<Match<Account,Current<AccessInfo.userName>>>>,
											InnerJoin<CashAccount1, On<CashAccount1.cashAccountID, Equal<CATransfer.outAccountID>, And<Match<CashAccount1, Current<AccessInfo.userName>>>>>>,
											Where<CATransfer.released, Equal<boolFalse>, And<CATransfer.hold, Equal<boolFalse>>>>.Select(this))
			{
				foreach (CATran tran in PXSelect<CATran, Where<CATran.released, Equal<boolFalse>,
																													 And<CATran.hold, Equal<boolFalse>,
																													 And<Where<CATran.tranID, Equal<Required<CATransfer.tranIDIn>>,
																																	Or<CATran.tranID, Equal<Required<CATransfer.tranIDOut>>>>>>>>.Select(this, trf.TranIDIn, trf.TranIDOut))
				{
					yield return CARegisterList.Cache.Insert(CARegister(trf, tran));
				}
			}

			CARegisterList.Cache.IsDirty = false;
		}

        protected virtual void CARegister_TranID_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            CARegister row = (CARegister)e.Row;
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
                    
                    if (listMessages.ContainsKey(row.TranID.Value))
                        message = listMessages[row.TranID.Value];
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

		public static void GroupReleaseTransaction(List<CATran> tranList, bool allowAP, bool allowAR, bool updateInfo)
		{
			Dictionary<long,CAMessage> listMessages = new Dictionary<long,CAMessage>();
			if (updateInfo == true)
				PXLongOperation.SetCustomInfo(listMessages);
			List<CARegister> caRegisterList = new List<CARegister>();
            bool allPassed = true;
            PXGraph searchGraph = null;
			for (int i = 0; i < tranList.Count; i++)
            {
                CATran tran = tranList[i];
                try
                {
                    if (tran.Released == true)
                    {
                        continue;
                    }
                    switch (tran.OrigModule)
                    {
                        case GL.BatchModule.GL:
                            throw new PXException(Messages.ThisDocTypeNotAvailableForRelease);
                        case GL.BatchModule.AP:
                            if (allowAP != true)
                            {
                                throw new PXException(Messages.APDocumentsCanNotBeReleasedFromCAModule);
                            }
                            else
                            {
                                CATrxRelease.ReleaseCATran(tran, ref searchGraph);
                            }
                            break;
                        case GL.BatchModule.AR:
                            if (allowAR != true)
                            {
                                throw new PXException(Messages.ARDocumentsCanNotBeReleasedFromCAModule);
                            }
                            else
                            {
                                CATrxRelease.ReleaseCATran(tran, ref searchGraph);
                            }
                            break;
                        case GL.BatchModule.CA:
                            CATrxRelease.ReleaseCATran(tran, ref searchGraph);
                            break;
                        default:
                            throw new Exception(Messages.ThisDocTypeNotAvailableForRelease);
                    }
                    if (updateInfo == true)
                        listMessages.Add(tran.TranID.Value, new CAMessage(tran.TranID.Value, PXErrorLevel.RowInfo, ActionsMessages.RecordProcessed));
                }
                catch (Exception ex)
                {
                    allPassed = false;
                    if (updateInfo == true)
                        listMessages.Add(tran.TranID.Value, new CAMessage(tran.TranID.Value, PXErrorLevel.RowError, ex.Message));
                }
            }
			if (!allPassed)
			{
				throw new PXException(Messages.OneOrMoreItemsAreNotReleased);
			}			
		}

        public static void ReleaseCATran(CATran aTran, ref PXGraph aGraph)
        {
            int i = 0;
            if (aTran != null)
            {                
                if(aGraph == null)
                    aGraph = PXGraph.CreateInstance<CATranEntry>();
                PXGraph caGraph = aGraph;
                switch (aTran.OrigModule)
                {
                    case GL.BatchModule.AP:
                        List<APRegister> apList = new List<APRegister>();
                        APRegister apReg = PXSelect<APRegister,
                                                            Where<APRegister.docType, Equal<Required<APRegister.docType>>,
                                                                And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>.
                                                                Select(caGraph, aTran.OrigTranType, aTran.OrigRefNbr);
                        if (apReg != null)
                        {
                            if (apReg.Released == false)
                            {
                                apList.Add(apReg);
                                APDocumentRelease.ReleaseDoc(apList, false);
                            }
                        }
                        else
                            throw new Exception(Messages.DocNotFound);
                        break;

                    case GL.BatchModule.AR:
                        List<ARRegister> arList = new List<ARRegister>();
                        ARRegister arReg = PXSelect<ARRegister,
                                                        Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
                                                            And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>.
                                                            Select(caGraph, aTran.OrigTranType, aTran.OrigRefNbr);
                        if (arReg != null)
                        {
                            if (arReg.Released == false)
                            {
                                arList.Add(arReg);
                                ARDocumentRelease.ReleaseDoc(arList, false);
                            }
                        }
                        else
                            throw new Exception(Messages.DocNotFound);
                        break;

                    case GL.BatchModule.CA:
                        switch (aTran.OrigTranType)
                        {
                            case CAAPARTranType.CAAdjustment:
                                CAAdj docAdj = PXSelect<CAAdj, Where<CAAdj.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>, And<CAAdj.adjTranType, Equal<Required<CAAdj.adjTranType>>>>>.Select(caGraph, aTran.OrigRefNbr, aTran.OrigTranType);
                                if (docAdj != null)
                                {
                                    if (docAdj.Released == false)
                                    {   
                                        ReleaseDoc<CAAdj>(docAdj, i,null);                                      
                                    }
                                }
                                else
                                    throw new Exception(Messages.DocNotFound);
                                break;
                            case CAAPARTranType.CATransferIn:
                            case CAAPARTranType.CATransferOut:
                            case CAAPARTranType.CATransferExp:

                                CATransfer docTransfer = PXSelect<CATransfer, Where<CATransfer.transferNbr, Equal<Required<CATransfer.transferNbr>>>>.Select(caGraph, aTran.OrigRefNbr);
                                if (docTransfer != null)
                                {
                                    if (docTransfer.Released == false)
                                    {
                                        ReleaseDoc<CATransfer>(docTransfer, i, null);
                                    }
                                }
                                else
                                    throw new Exception(Messages.DocNotFound);
                                break;
                            default:
                                throw new Exception(Messages.DocNotFound);
                        }
                        break;
                    default:
                        throw new Exception(Messages.ThisDocTypeNotAvailableForRelease);
                }
            }
        }

		public static void GroupRelease(List<CARegister> list, bool updateInfo)
		{			
            Dictionary<long, CAMessage> listMessages = new Dictionary<long, CAMessage>();
			if (updateInfo == true)
                PXLongOperation.SetCustomInfo(listMessages);

			Exception exception = null;
			for (int i = 0; i < list.Count; i++)
			{
				CARegister caRegisterItem = list[i];
				if (caRegisterItem != null)
				{
					try
					{
						if (caRegisterItem.Released == false)
						{
							if ((bool)caRegisterItem.Hold)
							{
								throw new Exception(Messages.HoldDocCanNotBeRelease);
							}
							else
							{
								JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
								je.FieldVerifying.AddHandler<GLTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
								je.FieldVerifying.AddHandler<GLTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			

								switch (caRegisterItem.Module)
								{
									case GL.BatchModule.AP:
										List<APRegister> apList = new List<APRegister>();
										APRegister apReg = PXSelect<APRegister,
																		Where<APRegister.docType, Equal<Required<APRegister.docType>>,
																			And<APRegister.refNbr, Equal<Required<APRegister.refNbr>>>>>.
																			Select(je, caRegisterItem.TranType, caRegisterItem.ReferenceNbr);
										if (apReg != null)
										{
											apList.Add(apReg);
											APDocumentRelease.ReleaseDoc(apList, false);			
										}
										else
											throw new Exception(Messages.TransactionNotComplete);
										break;
									case GL.BatchModule.AR:
										List<ARRegister> arList = new List<ARRegister>();
										ARRegister arReg = PXSelect<ARRegister,
																		Where<ARRegister.docType, Equal<Required<ARRegister.docType>>,
																			And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>.
																			Select(je, caRegisterItem.TranType, caRegisterItem.ReferenceNbr);
										if (arReg != null)
										{
											arList.Add(arReg);
											ARDocumentRelease.ReleaseDoc(arList, false);											
										}
										else
											throw new Exception(Messages.TransactionNotComplete);
										break;
									case GL.BatchModule.CA:
										switch (caRegisterItem.TranType)
										{
											case CAAPARTranType.CAAdjustment:
												CAAdj docAdj = PXSelect<CAAdj, Where<CAAdj.adjRefNbr, Equal<Required<CAAdj.adjRefNbr>>, And<CAAdj.adjTranType, Equal<Required<CAAdj.adjTranType>>>>>.Select(je, caRegisterItem.ReferenceNbr, caRegisterItem.TranType);
												if (docAdj != null)
												{
													ReleaseDoc<CAAdj>(docAdj, i,null);											
												}
												else
													throw new Exception(Messages.DocNotFound);
												break;
											case CAAPARTranType.CATransfer:
												CATransfer docTransfer = PXSelect<CATransfer, Where<CATransfer.transferNbr, Equal<Required<CATransfer.transferNbr>>>>.Select(je, caRegisterItem.ReferenceNbr);
												if (docTransfer != null)
												{
													ReleaseDoc<CATransfer>(docTransfer, i, null);													//PXProcessing<CATran>.SetInfo(i, ActionsMessages.RecordProcessed);
												}
												else
													throw new Exception(Messages.DocNotFound);
												break;
											default:
												throw new Exception(Messages.DocNotFound);
										}
										break;
									default:
										throw new Exception(Messages.DocNotFound);
								}                                
								if (updateInfo == true)
                                    listMessages.Add(caRegisterItem.TranID.Value,new CAMessage(caRegisterItem.TranID.Value, PXErrorLevel.RowInfo, ActionsMessages.RecordProcessed));								
							}
						}
						else
						{
							throw new Exception(Messages.OriginalDocAlreadyReleased);
						}
					}
					catch (Exception e)
					{                        
                        if (updateInfo == true)
                        {
                            string message = e is PXOuterException ? (e.Message + " " + String.Join(" ", ((PXOuterException)e).InnerMessages)) : e.Message;
                            listMessages.Add(caRegisterItem.TranID.Value, new CAMessage(caRegisterItem.TranID.Value, PXErrorLevel.RowError, message));
                        }
                        exception = e;
					}
				}
			}
			if (exception != null)
				if (list.Count == 1)
					throw exception;
				else
					throw new Exception(Messages.OneOrMoreItemsAreNotReleased);
		}

		public static CARegister CARegister(CATran item)
		{
			CATranEntry caGraph = PXGraph.CreateInstance<CATranEntry>();

			switch (item.OrigModule)
			{
				case GL.BatchModule.AP:
					APPayment apPay = (APPayment)PXSelect<APPayment, Where<APPayment.cATranID, Equal<Required<APPayment.cATranID>>>>.
																							Select(caGraph, item.TranID);
					if (apPay != null)
					{
						return CARegister(apPay);
					}
					else
						throw new Exception(Messages.OrigDocCanNotBeFound);

				case GL.BatchModule.AR:
					ARPayment arPay = (ARPayment)PXSelect<ARPayment, Where<ARPayment.cATranID, Equal<Required<ARPayment.cATranID>>>>.
																							Select(caGraph, item.TranID);
					if (arPay != null)
					{
						return CARegister(arPay);
					}
					else
						throw new Exception(Messages.OrigDocCanNotBeFound);

				case GL.BatchModule.GL:
					GLTran gLTran = PXSelect<GLTran,
														 Where<GLTran.module, Equal<Required<GLTran.module>>,
															 And<GLTran.cATranID, Equal<Required<GLTran.cATranID>>>>>.
													Select(caGraph, item.OrigModule, item.TranID);
					if (gLTran != null)
					{
						CARegister reg = CARegister(gLTran);
                        int? cashAccountID;
                        if (GL.GLCashTranIDAttribute.CheckGLTranCashAcc(caGraph, gLTran,out cashAccountID) == true)
                        {
                            reg.CashAccountID = cashAccountID;
                            return reg;
                        }
                        else
                        {
                            throw new Exception(GL.Messages.CashAccountDoesNotExist);
                        }
					}
					else
						throw new Exception(Messages.OrigDocCanNotBeFound);

				case GL.BatchModule.CA:
					switch (item.OrigTranType)
					{
						case CAAPARTranType.CAAdjustment:
							CAAdj docAdj = PXSelect<CAAdj, Where<CAAdj.tranID, Equal<Required<CAAdj.tranID>>>>.Select(caGraph, item.TranID);
							if (docAdj != null)
							{
								return CARegister(docAdj);
							}
							else
								throw new Exception(Messages.OrigDocCanNotBeFound);
						case CAAPARTranType.CATransferIn:
							CATransfer docTransferIn = PXSelect<CATransfer, Where<CATransfer.tranIDIn, Equal<Required<CATransfer.tranIDIn>>>>
																			.Select(caGraph, item.TranID);
							if (docTransferIn != null)
							{
								return CARegister(docTransferIn, item);
							}
							else
								throw new Exception(Messages.OrigDocCanNotBeFound);
						case CAAPARTranType.CATransferOut:
							CATransfer docTransferOut = PXSelect<CATransfer, Where<CATransfer.tranIDOut, Equal<Required<CATransfer.tranIDOut>>>>
																			.Select(caGraph, item.TranID);
							if (docTransferOut != null)
							{
								return CARegister(docTransferOut, item);
							}
							else
								throw new Exception(Messages.OrigDocCanNotBeFound);
						default:
							throw new Exception(Messages.ThisCATranOrigDocTypeNotDefined);
					}
				default:
					throw new Exception(Messages.ThisCATranOrigDocTypeNotDefined);
			}
			throw new Exception(Messages.ThisCATranOrigDocTypeNotDefined);
		}

		public static CARegister CARegister(CAAdj item)
		{
			CARegister ret = new CARegister();
			ret.TranID = item.TranID;
			ret.Hold = item.Hold;
			ret.Released = item.Released;
			ret.Module = GL.BatchModule.CA;
			ret.TranType = item.AdjTranType;
			ret.Description = item.TranDesc;
			ret.FinPeriodID = item.FinPeriodID;
			ret.DocDate = item.TranDate;
			ret.ReferenceNbr = item.AdjRefNbr;
			ret.NoteID = item.NoteID;
			ret.CashAccountID = item.CashAccountID;
			ret.CuryID = item.CuryID;
			ret.TranAmt = item.TranAmt;
			ret.CuryTranAmt = item.CuryTranAmt;

			return ret;
		}

		public static CARegister CARegister(GLTran item)
		{
			CARegister ret = new CARegister();
			ret.TranID = item.CATranID;
			ret.Hold = (item.Released != true);
			ret.Released = item.Released;
			ret.Module = GL.BatchModule.GL;
			ret.TranType = item.TranType;
			ret.Description = item.TranDesc;
			ret.FinPeriodID = item.FinPeriodID;
			ret.DocDate = item.TranDate;
			ret.ReferenceNbr = item.RefNbr;
			ret.NoteID = item.NoteID;
			ret.CashAccountID = item.AccountID;
			//ret.CuryID        = item.;
			ret.TranAmt = item.DebitAmt - item.CreditAmt;
			ret.CuryTranAmt = item.CuryDebitAmt - item.CuryCreditAmt;

			return ret;
		}

		public static CARegister CARegister(ARPayment item)
		{
			CARegister ret = new CARegister();
			ret.TranID = item.CATranID;
			ret.Hold = item.Hold;
			ret.Released = item.Released;
			ret.Module = GL.BatchModule.AR;
			ret.TranType = item.DocType;
			ret.Description = item.DocDesc;
			ret.FinPeriodID = item.FinPeriodID;
			ret.DocDate = item.DocDate;
			ret.ReferenceNbr = item.RefNbr;
			ret.NoteID = item.NoteID;
			ret.CashAccountID = item.CashAccountID;
			ret.CuryID = item.CuryID;
			ret.TranAmt = item.DocBal;
			ret.CuryTranAmt = item.DocBal;

			return ret;
		}

		public static CARegister CARegister(APPayment item)
		{
			CARegister ret = new CARegister();
			ret.TranID = item.CATranID;
			ret.Hold = item.Hold;
			ret.Released = item.Released;
			ret.Module = GL.BatchModule.AP;
			ret.TranType = item.DocType;
			ret.Description = item.DocDesc;
			ret.FinPeriodID = item.FinPeriodID;
			ret.DocDate = item.DocDate;
			ret.ReferenceNbr = item.RefNbr;
			ret.NoteID = item.NoteID;
			ret.CashAccountID = item.CashAccountID;
			ret.CuryID = item.CuryID;
			ret.TranAmt = item.DocBal;
			ret.CuryTranAmt = item.DocBal;

			return ret;
		}

		public static CARegister CARegister(CATransfer item, CATran tran)
		{
			CARegister ret = new CARegister();
			ret.TranID = tran.TranID;
			ret.Hold = item.Hold;
			ret.Released = item.Released;
			ret.Module = GL.BatchModule.CA;
			ret.TranType = CAAPARTranType.CATransfer;
			ret.Description = item.Descr;
			ret.FinPeriodID = tran.FinPeriodID;
			ret.DocDate = item.OutDate;
			ret.ReferenceNbr = item.TransferNbr;
			ret.TranType = CAAPARTranType.CATransfer;
			ret.NoteID = item.NoteID;

			ret.CashAccountID = item.OutAccountID;
			ret.CuryID = item.OutCuryID;
			ret.CuryTranAmt = item.CuryTranOut;
			ret.TranAmt = item.TranOut;
			return ret;
		}

		public static void ReleaseDoc<TCADocument>(TCADocument _doc, int _item, List<Batch> externalPostList)
			where TCADocument : class, ICADocument, new()
		{
			CAReleaseProcess rg = PXGraph.CreateInstance<CAReleaseProcess>();
			JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
			je.FieldVerifying.AddHandler<GLTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			je.FieldVerifying.AddHandler<GLTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
			

			bool skipPost = (externalPostList != null);
			List<Batch> batchlist = new List<Batch>();
			List<int> batchbind = new List<int>();

			bool failed = false;
			rg.Clear();
			rg.ReleaseDocProc(je, ref batchlist, _doc);

			for (int i = batchbind.Count; i < batchlist.Count; i++)
			{
				batchbind.Add(i);
			}
			if (skipPost)
			{
				if (rg.AutoPost)
					externalPostList.AddRange(batchlist);
			}
			else
			{
				PostGraph pg = PXGraph.CreateInstance<PostGraph>();
				for (int i = 0; i < batchlist.Count; i++)
				{
					Batch batch = batchlist[i];
					try
					{
						if (rg.AutoPost)
						{
							pg.Clear();
							pg.TimeStamp = batch.tstamp;
							pg.PostBatchProc(batch);
						}
					}
					catch (Exception e)
					{
						throw new AP.PXMassProcessException(batchbind[i], e);
					}
				}
			}
			if (failed)
			{
				throw new PXException(GL.Messages.DocumentsNotReleased);
			}
		}
		#endregion
	}

	public interface ICADocument
	{
		string DocType
		{
			get;
		}
		string RefNbr
		{
			get;
		}
		Boolean? Released
		{
			get;
			set;
		}
	}

	[PXHidden()]
	public class CAReleaseProcess : PXGraph<CAReleaseProcess>
	{
		public PXSetup<CASetup> casetup;
		public PXSelectJoin<CATran, InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CATran.cashAccountID>>, 
            InnerJoin<Currency, On<Currency.curyID, Equal<CashAccount.curyID>>, 
            InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<CATran.curyInfoID>>, 
            LeftJoin<CAAdj, On<CAAdj.tranID, Equal<CATran.tranID>>, 
            LeftJoin<CASplit, On<CASplit.adjTranType, Equal<CAAdj.adjTranType>, 
                And<CASplit.adjRefNbr, Equal<CAAdj.adjRefNbr>>>>>>>>, 
            Where<CATran.origModule, Equal<BatchModule.moduleCA>, 
                And<CATran.origTranType, Like<Required<CATran.origTranType>>, 
                And<CATran.origRefNbr, Equal<Required<CATran.origRefNbr>>, 
                And<CATran.released, Equal<boolFalse>>>>>, OrderBy<Asc<CATran.tranID>>> CATran_CASplit_Ordered;

		public PXSelectJoin<CATaxTran, InnerJoin<Tax, On<Tax.taxID, Equal<CATaxTran.taxID>>>, Where<CATaxTran.module, Equal<BatchModule.moduleCA>, And<CATaxTran.tranType, Equal<Required<CATaxTran.tranType>>, And<CATaxTran.refNbr, Equal<Required<CATaxTran.refNbr>>>>>, OrderBy<Asc<Tax.taxCalcLevel>>> CATaxTran_TranType_RefNbr;
		public PXSelect<CADepositEntry.ARPaymentUpdate> arDocs;
		public PXSelect<CADepositEntry.APPaymentUpdate> apDocs;
		public PXSelect<CADepositDetail> depositDetails;
		public PXSelect<CADeposit> deposit;

		public bool AutoPost
		{
			get
			{
				return (bool)casetup.Current.AutoPostOption;
			}
		}

		public CAReleaseProcess()
		{
		}


        public virtual void SegregateBatch(JournalEntry je, List<Batch> batchlist, Int32? branchID, string curyID, DateTime? docDate, string finPeriodID, string description, CurrencyInfo curyInfo)
        {
            var batch = je.BatchModule.Current;
            
            if (batch != null)
            {
                if (batch.CuryCreditTotal != batch.CuryDebitTotal)
                {
                    batch.Hold = true;
                    batch.Released = false;
                    je.BatchModule.Update(batch);
                }

                je.Save.Press();
                if (batchlist.Contains(batch) == false)
                {
                    batchlist.Add(batch);
                }
            }

            JournalEntry.SegregateBatch(je, true, BatchModule.CA, branchID, curyID, docDate, finPeriodID, description, curyInfo, null);
        }

		public void ReleaseDocProc<TCADocument>(JournalEntry je, ref List<Batch> batchlist, TCADocument doc)
			where TCADocument : class, ICADocument, new()
		{
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				GLTran rgol_tran = new GLTran();
				rgol_tran.DebitAmt = 0m;
				rgol_tran.CreditAmt = 0m;
				Currency rgol_cury = null;
				CurrencyInfo rgol_info = null;
				CurrencyInfo transit_info = null;
				GLTran tran;

				CATran prev_tran = null;

				if (casetup.Current == null || casetup.Current.TransitAcctId == null || casetup.Current.TransitSubID == null)
				{
					throw new PXException();
				}
				Batch batch = null;

			    int? SourceBranchID = null;
				foreach (PXResult<CATran, CashAccount, Currency, CurrencyInfo, CAAdj, CASplit> res in CATran_CASplit_Ordered.Select(doc.DocType, doc.RefNbr))
				{
					CATran catran = (CATran)res;
					CashAccount cashacct = (CashAccount)res;
					Currency cury = (Currency)res;
					CurrencyInfo info = (CurrencyInfo)res;
					CAAdj caadj = (CAAdj)res;
					CASplit casplit = (CASplit)res;

                    if (SourceBranchID == null && catran.OrigTranType != CAAPARTranType.CATransferIn) // Type of first transaction CATransferOut or CAAdjust expected
                    {
                        SourceBranchID = cashacct.BranchID;
                    }

                    SegregateBatch(je, batchlist, SourceBranchID ?? cashacct.BranchID, catran.CuryID, catran.TranDate, catran.FinPeriodID, catran.TranDesc, info);
					batch = (Batch)je.BatchModule.Current;

					if (casplit.AdjTranType == null)
					{
						casplit = new CASplit();

						casplit.AdjTranType = catran.OrigTranType;
						casplit.CuryInfoID = catran.CuryInfoID;
						casplit.CuryTranAmt = (catran.DrCr == "D" ? catran.CuryTranAmt : -1m * catran.CuryTranAmt);
						casplit.TranAmt = (catran.DrCr == "D" ? catran.TranAmt : -1m * catran.TranAmt);
						casplit.TranDesc = "Offset";
						casplit.AccountID = casetup.Current.TransitAcctId;
						casplit.SubID = casetup.Current.TransitSubID;
						casplit.ReferenceID = catran.ReferenceID;
                        casplit.BranchID = SourceBranchID ?? cashacct.BranchID;

						switch (casplit.AdjTranType)
						{
							case CAAPARTranType.CATransferOut:
								transit_info = PXCache<CurrencyInfo>.CreateCopy(info);
								transit_info.CuryInfoID = null;
								transit_info = je.currencyinfo.Insert(transit_info);
								transit_info.BaseCalc = false;

								casplit.CuryInfoID = transit_info.CuryInfoID;
								break;
							case CAAPARTranType.CATransferIn:
								rgol_cury = cury;
								rgol_info = info;
								rgol_tran.FinPeriodID = catran.FinPeriodID;
								rgol_tran.TranPeriodID = catran.TranPeriodID;
								rgol_tran.TranDate = catran.TranDate;
                                rgol_tran.BranchID = cashacct.BranchID;

								if (string.Equals(info.CuryID, transit_info.CuryID))
								{
									casplit.CuryInfoID = transit_info.CuryInfoID;
								}
								break;
							default:
								throw new PXException();
						}

						rgol_tran.DebitAmt += (catran.DrCr == "D" ? 0m : casplit.TranAmt);
						rgol_tran.CreditAmt += (catran.DrCr == "D" ? casplit.TranAmt : 0m);
					}

					if (object.Equals(prev_tran, catran) == false)
					{
						tran = new GLTran();
						tran.SummPost = false;
						tran.CuryInfoID = catran.CuryInfoID;
						tran.TranType = catran.OrigTranType;
						tran.RefNbr = catran.OrigRefNbr;
						tran.ReferenceID = catran.ReferenceID;
                        tran.AccountID = cashacct.AccountID;
						tran.SubID = cashacct.SubID;
						tran.CATranID = catran.TranID;
						tran.TranDate = catran.TranDate;
						tran.FinPeriodID = catran.FinPeriodID;
						tran.TranPeriodID = catran.TranPeriodID;
						tran.CuryDebitAmt = (catran.DrCr == "D" ? catran.CuryTranAmt : 0m);
						tran.DebitAmt = (catran.DrCr == "D" ? catran.TranAmt : 0m);
						tran.CuryCreditAmt = (catran.DrCr == "D" ? 0m : -1m * catran.CuryTranAmt);
						tran.CreditAmt = (catran.DrCr == "D" ? 0m : -1m * catran.TranAmt);
						tran.TranDesc = catran.TranDesc;
						tran.Released = true;
                        tran.BranchID = cashacct.BranchID;
					    tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(this); 
						je.GLTranModuleBatNbr.Insert(tran);

						foreach (PXResult<CATaxTran, Tax> r in CATaxTran_TranType_RefNbr.Select(caadj.AdjTranType, caadj.AdjRefNbr))
						{
							CATaxTran x = (CATaxTran)r;
							Tax salestax = (Tax)r;

							if (salestax.TaxType == CSTaxType.Withholding)
							{
								continue;
							}

							if (salestax.ReverseTax != true)
							{
								tran = new GLTran();
								tran.SummPost = false;
								tran.CuryInfoID = catran.CuryInfoID;
								tran.TranType = caadj.AdjTranType;
								tran.TranClass = "T";
								tran.RefNbr = caadj.AdjRefNbr;
								tran.TranDate = caadj.TranDate;
								tran.AccountID = (salestax.TaxType == CSTaxType.Use) ? salestax.ExpenseAccountID : x.AccountID;
								tran.SubID = (salestax.TaxType == CSTaxType.Use) ? salestax.ExpenseSubID : x.SubID;
								tran.TranDesc = salestax.TaxID;
								tran.CuryDebitAmt = (caadj.DrCr == "C") ? x.CuryTaxAmt : 0m;
								tran.DebitAmt = (caadj.DrCr == "C") ? x.TaxAmt : 0m;
								tran.CuryCreditAmt = (caadj.DrCr == "C") ? 0m : x.CuryTaxAmt;
								tran.CreditAmt = (caadj.DrCr == "C") ? 0m : x.TaxAmt;
								tran.Released = true;
								tran.ReferenceID = null;
                                tran.BranchID = caadj.BranchID;
                                tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(this); 
								je.GLTranModuleBatNbr.Insert(tran);
							}

							if (salestax.TaxType == CSTaxType.Use || (bool)salestax.ReverseTax)
							{
								tran = new GLTran();
								tran.SummPost = false;
								tran.CuryInfoID = catran.CuryInfoID;
								tran.TranType = caadj.AdjTranType;
								tran.TranClass = "T";
								tran.RefNbr = caadj.AdjRefNbr;
								tran.TranDate = caadj.TranDate;
								tran.AccountID = x.AccountID;
								tran.SubID = x.SubID;
								tran.TranDesc = salestax.TaxID;
								tran.CuryDebitAmt = (caadj.DrCr == "C") ? 0m : x.CuryTaxAmt;
								tran.DebitAmt = (caadj.DrCr == "C") ? 0m : x.TaxAmt;
								tran.CuryCreditAmt = (caadj.DrCr == "C") ? x.CuryTaxAmt : 0m;
								tran.CreditAmt = (caadj.DrCr == "C") ? x.TaxAmt : 0m;
								tran.Released = true;
								tran.ReferenceID = null;
                                tran.BranchID = caadj.BranchID;
                                tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(this); 
								je.GLTranModuleBatNbr.Insert(tran);
							}

							x.Released = true;
							CATaxTran_TranType_RefNbr.Update(x);
						}
					}

					tran = new GLTran();
					tran.SummPost = (catran.OrigTranType == CATranType.CATransferIn || catran.OrigTranType == CATranType.CATransferOut); ;
					tran.CuryInfoID = casplit.CuryInfoID;
					tran.TranType = catran.OrigTranType;
					tran.RefNbr = catran.OrigRefNbr;

					tran.ReferenceID = casplit.ReferenceID;
					tran.AccountID = casplit.AccountID;
					tran.SubID = casplit.SubID;
					tran.CATranID = null;
					tran.TranDate = catran.TranDate;
					tran.FinPeriodID = catran.FinPeriodID;
					tran.TranPeriodID = catran.TranPeriodID;
                    tran.BranchID = casplit.BranchID;   //??
                    tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(this); 
					if (casplit.CuryTaxableAmt != null)
					{
						tran.CuryDebitAmt = (catran.DrCr == "D" ? 0m : casplit.CuryTaxableAmt);
						tran.DebitAmt = (catran.DrCr == "D" ? 0m : casplit.TaxableAmt);
						tran.CuryCreditAmt = (catran.DrCr == "D" ? casplit.CuryTaxableAmt : 0m);
						tran.CreditAmt = (catran.DrCr == "D" ? casplit.TaxableAmt : 0m);
					}
					else
					{
						tran.CuryDebitAmt = (catran.DrCr == "D" ? 0m : casplit.CuryTranAmt);
						tran.DebitAmt = (catran.DrCr == "D" ? 0m : casplit.TranAmt);
						tran.CuryCreditAmt = (catran.DrCr == "D" ? casplit.CuryTranAmt : 0m);
						tran.CreditAmt = (catran.DrCr == "D" ? casplit.TranAmt : 0m);
					}
					tran.TranDesc = casplit.TranDesc;
					tran.ProjectID = casplit.ProjectID;
					tran.TaskID = casplit.TaskID;
				    tran.NonBillable = casplit.NonBillable;
					tran.Released = true;
					je.GLTranModuleBatNbr.Insert(tran);

					prev_tran = catran;

					if (rgol_cury != null && rgol_info != null && Math.Abs(Math.Round((decimal)(rgol_tran.DebitAmt - rgol_tran.CreditAmt), 4)) >= 0.00005m)
					{
                        SegregateBatch(je, batchlist, SourceBranchID ?? rgol_tran.BranchID, rgol_cury.CuryID, null, rgol_tran.FinPeriodID, "", rgol_info);
                        batch = (Batch)je.BatchModule.Current;

						CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(rgol_info);
						new_info.CuryInfoID = null;
						new_info = je.currencyinfo.Insert(new_info);

						tran = new GLTran();
						tran.SummPost = false;
						if (Math.Sign((decimal)(rgol_tran.DebitAmt - rgol_tran.CreditAmt)) == 1)
						{
							tran.AccountID = rgol_cury.RealLossAcctID;
							tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.realLossSubID>(je, rgol_tran.BranchID,rgol_cury);
							tran.DebitAmt = Math.Round((decimal)(rgol_tran.DebitAmt - rgol_tran.CreditAmt), 4);
							tran.CuryDebitAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) ? tran.DebitAmt : 0m; //non-zero for base cury
							tran.CreditAmt = 0m;
							tran.CuryCreditAmt = 0m;
						}
						else
						{
							tran.AccountID = rgol_cury.RealGainAcctID;
                            tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.realGainSubID>(je, rgol_tran.BranchID, rgol_cury);
                            tran.DebitAmt = 0m;
							tran.CuryDebitAmt = 0m;
							tran.CreditAmt = Math.Round((decimal)(rgol_tran.CreditAmt - rgol_tran.DebitAmt), 4);
							tran.CuryCreditAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) ? tran.CreditAmt : 0m; //non-zero for base cury
						}
						tran.TranType = CATranType.CATransferRGOL;
						tran.RefNbr = doc.RefNbr;
						tran.TranDesc = "RGOL";
						tran.TranDate = rgol_tran.TranDate;
						tran.FinPeriodID = rgol_tran.FinPeriodID;
						tran.TranPeriodID = rgol_tran.TranPeriodID;
						tran.Released = true;
						tran.CuryInfoID = new_info.CuryInfoID;
                        tran.BranchID = rgol_tran.BranchID;
                        tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(this); 
						je.GLTranModuleBatNbr.Insert(tran);

						tran.AccountID = casetup.Current.TransitAcctId;
						tran.SubID = casetup.Current.TransitSubID;

						decimal? CuryAmount = tran.CuryDebitAmt;
						decimal? BaseAmount = tran.DebitAmt;
						tran.CuryDebitAmt = tran.CuryCreditAmt;
						tran.DebitAmt = tran.CreditAmt;
						tran.CuryCreditAmt = CuryAmount;
						tran.CreditAmt = BaseAmount;

						je.GLTranModuleBatNbr.Insert(tran);

						rgol_tran = new GLTran();
						rgol_tran.DebitAmt = 0m;
						rgol_tran.CreditAmt = 0m;
						rgol_cury = null;
						rgol_info = null;
					}

					if (batch != null && batch.CuryCreditTotal == batch.CuryDebitTotal)
					{
						//in normal case this happens on moving to next CATran
						if (Math.Abs(Math.Round((decimal)(batch.DebitTotal - batch.CreditTotal), 4)) >= 0.00005m)
						{
							tran = new GLTran();
							tran.SummPost = true;

							if (Math.Sign((decimal)(batch.DebitTotal - batch.CreditTotal)) == 1)
							{
								tran.AccountID = cury.RoundingGainAcctID;
                                tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(je, batch.BranchID, cury);
								tran.CreditAmt = Math.Round((decimal)(batch.DebitTotal - batch.CreditTotal), 4);
								tran.DebitAmt = 0m;
							}
							else
							{
								tran.AccountID = cury.RoundingLossAcctID;
                                tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(je, batch.BranchID, cury);
								tran.CreditAmt = 0m;
								tran.DebitAmt = Math.Round((decimal)(batch.CreditTotal - batch.DebitTotal), 4);
							}
							tran.CuryCreditAmt = 0m;
							tran.CuryDebitAmt = 0m;
							tran.TranType = CATranType.CAAdjustmentRGOL;
							tran.RefNbr = doc.RefNbr;
							tran.TranClass = "N";
							tran.TranDesc = GL.Messages.RoundingDiff;
							tran.LedgerID = batch.LedgerID;
							tran.FinPeriodID = batch.FinPeriodID;
							tran.TranDate = batch.DateEntered;
							tran.Released = true;
                            tran.BranchID = batch.BranchID;
                            tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(this); 
							CurrencyInfo infocopy = new CurrencyInfo();
							infocopy = je.currencyinfo.Insert(infocopy) ?? infocopy;

							tran.CuryInfoID = infocopy.CuryInfoID;
							je.GLTranModuleBatNbr.Insert(tran);
						}

                        batch.Released = true;
                        batch.Hold = false;
                        je.BatchModule.Update(batch);

						je.Save.Press();

						if (batchlist.Contains(batch) == false)
						{
							batchlist.Add(batch);
						}

						doc.Released = true;
						Caches[typeof(TCADocument)].Update(doc);

						if (Caches[typeof(TCADocument)].ObjectsEqual(doc, caadj) == false)
						{
							caadj.Released = true;
							Caches[typeof(CAAdj)].Update(caadj);
						}
					}
				}

                doc.Released = true;
                Caches[typeof(TCADocument)].Update(doc);
				Caches[typeof(TCADocument)].Persist(PXDBOperation.Update);
				Caches[typeof(CAAdj)].Persist(PXDBOperation.Update);
				Caches[typeof(CATaxTran)].Persist(PXDBOperation.Update);
				Caches[typeof(CADailySummary)].Persist(PXDBOperation.Insert);

				ts.Complete(this);
			}
			Caches[typeof(TCADocument)].Persisted(false);
			Caches[typeof(CAAdj)].Persisted(false);
			Caches[typeof(CATaxTran)].Persisted(false);
			Caches[typeof(CADailySummary)].Persisted(false);
		}


		public virtual void ReleaseDeposit(JournalEntry je, ref List<Batch> batchlist, CADeposit doc)
		{
			je.Clear();

			Currency rgol_cury = null;
			CurrencyInfo rgol_info = null;
			Dictionary<int, GLTran> rgols = new Dictionary<int, GLTran>();
			GLTran tran;
			CATran prev_tran = null;
			Batch batch = CreateGLBatch(je, doc);

			PXSelectBase<CATran> select = new PXSelectJoin<CATran,
									InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CATran.cashAccountID>>,
									InnerJoin<Currency, On<Currency.curyID, Equal<CashAccount.curyID>>,
									InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<CATran.curyInfoID>>,
									LeftJoin<CADepositDetail, On<CADepositDetail.tranType, Equal<CATran.origTranType>,
												And<CADepositDetail.refNbr, Equal<CATran.origRefNbr>,
												And<CADepositDetail.tranID, Equal<CATran.tranID>>>>,
									LeftJoin<CADepositEntry.ARPaymentUpdate, On<CADepositEntry.ARPaymentUpdate.docType, Equal<CADepositDetail.origDocType>,
													And<CADepositEntry.ARPaymentUpdate.refNbr, Equal<CADepositDetail.origRefNbr>,
													And<CADepositDetail.origModule, Equal<GL.BatchModule.moduleAR>>>>,
									LeftJoin<CADepositEntry.APPaymentUpdate, On<CADepositEntry.APPaymentUpdate.docType, Equal<CADepositDetail.origDocType>,
													And<CADepositEntry.APPaymentUpdate.refNbr, Equal<CADepositDetail.origRefNbr>,
													And<CADepositDetail.origModule, Equal<GL.BatchModule.moduleAP>>>>>>>>>>,
									Where<CATran.origModule, Equal<BatchModule.moduleCA>,
									And<CATran.origTranType, Equal<Required<CATran.origTranType>>,
									And<CATran.origRefNbr, Equal<Required<CATran.origRefNbr>>>>>,
									OrderBy<Asc<CATran.tranID>>>(this);

			foreach (PXResult<CATran, CashAccount, Currency, CurrencyInfo, CADepositDetail, CADepositEntry.ARPaymentUpdate, CADepositEntry.APPaymentUpdate> res in select.Select(doc.DocType, doc.RefNbr))
			{
				CATran catran = (CATran)res;
				CashAccount cashacct = (CashAccount)res;
				Currency cury = (Currency)res;
				CurrencyInfo info = (CurrencyInfo)res;
				CADepositDetail detail = (CADepositDetail)res;
				CADepositEntry.ARPaymentUpdate arDoc = (CADepositEntry.ARPaymentUpdate)res;
				CADepositEntry.APPaymentUpdate apDoc = (CADepositEntry.APPaymentUpdate)res;
				if (catran.CuryID != doc.CuryID)
					throw new PXException("Deposit of multiple currencies is not supported yet");

				batch = (Batch)je.BatchModule.Current;
				if (object.Equals(prev_tran, catran) == false)
				{
					tran = new GLTran();
					tran.SummPost = false;
					tran.CuryInfoID = batch.CuryInfoID;
					tran.TranType = catran.OrigTranType;
					tran.RefNbr = catran.OrigRefNbr;
					tran.ReferenceID = catran.ReferenceID;
                    tran.AccountID = cashacct.AccountID;
					tran.SubID = cashacct.SubID;
					tran.CATranID = catran.TranID;
					tran.TranDate = catran.TranDate;
					tran.FinPeriodID = catran.FinPeriodID;
					tran.TranPeriodID = catran.TranPeriodID;
					tran.CuryDebitAmt = (catran.DrCr == CADrCr.CADebit ? catran.CuryTranAmt : 0m);
					tran.DebitAmt = (catran.DrCr == CADrCr.CADebit ? catran.TranAmt : 0m);
					tran.CuryCreditAmt = (catran.DrCr == CADrCr.CADebit ? 0m : -1m * catran.CuryTranAmt);
					tran.CreditAmt = (catran.DrCr == CADrCr.CADebit ? 0m : -1m * catran.TranAmt);
					tran.TranDesc = catran.TranDesc;
					tran.Released = true;
					je.GLTranModuleBatNbr.Insert(tran);

					if (!String.IsNullOrEmpty(arDoc.RefNbr))
					{
						if (doc.TranType == CATranType.CADeposit)
						{
							arDoc.Deposited = true;
						}
						else
						{
							arDoc.Deposited = false;
							arDoc.DepositType = null;
							arDoc.DepositNbr = null;
							arDoc.DepositDate = null;
						}
						this.Caches[typeof(CADepositEntry.ARPaymentUpdate)].Update(arDoc);
					}

					if (!String.IsNullOrEmpty(apDoc.RefNbr))
					{
						if (doc.TranType == CATranType.CADeposit)
						{
							apDoc.Deposited = true;
						}
						else
						{
							apDoc.Deposited = false;
							apDoc.DepositType = null;
							apDoc.DepositNbr = null;
							apDoc.DepositDate = null;
						}
						this.Caches[typeof(CADepositEntry.APPaymentUpdate)].Update(apDoc);
					}

					if (!String.IsNullOrEmpty(detail.OrigRefNbr))
					{

						decimal rgol = Math.Round((detail.OrigAmtSigned.Value - detail.TranAmt.Value), 3);
						if (rgol != Decimal.Zero)
						{
							GLTran rgol_tran = null;
							if (!rgols.ContainsKey(detail.AccountID.Value))
							{
								rgol_tran = new GLTran();
								rgol_tran.DebitAmt = Decimal.Zero;
								rgol_tran.CreditAmt = Decimal.Zero;
                                rgol_tran.AccountID = cashacct.AccountID;
                                rgol_tran.SubID = cashacct.SubID;
								rgol_tran.TranDate = catran.TranDate;
								rgol_tran.FinPeriodID = catran.FinPeriodID;
								rgol_tran.TranPeriodID = catran.TranPeriodID;
								rgol_tran.TranType = CATranType.CATransferRGOL;
								rgol_tran.RefNbr = doc.RefNbr;
								rgol_tran.TranDesc = "RGOL";
								rgol_tran.Released = true;
								rgol_tran.CuryInfoID = batch.CuryInfoID;

								rgols[detail.AccountID.Value] = rgol_tran;
							}
							else
							{
								rgol_tran = rgols[detail.AccountID.Value];
							}
							rgol_tran.DebitAmt += (catran.DrCr == CADrCr.CACredit && rgol > 0 ? Decimal.Zero : Math.Abs(rgol));
							rgol_tran.CreditAmt += (catran.DrCr == CADrCr.CACredit && rgol > 0 ? rgol : Decimal.Zero);
							rgol_cury = cury;
							rgol_info = info;
						}
					}
				}
				prev_tran = catran;

			}
			if (batch != null)
			{
				foreach (CADepositCharge iCharge in PXSelect<CADepositCharge, Where<CADepositCharge.tranType, Equal<Required<CADepositCharge.tranType>>,
																	And<CADepositCharge.refNbr, Equal<Required<CADepositCharge.refNbr>>>>>.Select(this, doc.TranType, doc.RefNbr))
				{
					if (iCharge != null && iCharge.CuryChargeAmt != Decimal.Zero)
					{
						tran = new GLTran();
						tran.SummPost = false;
						tran.CuryInfoID = batch.CuryInfoID;
						tran.TranType = iCharge.TranType;
						tran.RefNbr = iCharge.RefNbr;

						tran.AccountID = iCharge.AccountID;
						tran.SubID = iCharge.SubID;
						tran.TranDate = doc.TranDate;
						tran.FinPeriodID = doc.FinPeriodID;
						tran.TranPeriodID = doc.TranPeriodID;
						tran.CuryDebitAmt = (iCharge.DrCr == CADrCr.CADebit ? Decimal.Zero : iCharge.CuryChargeAmt);
						tran.DebitAmt = (iCharge.DrCr == CADrCr.CADebit ? Decimal.Zero : iCharge.ChargeAmt);
						tran.CuryCreditAmt = (iCharge.DrCr == CADrCr.CADebit ? iCharge.CuryChargeAmt : Decimal.Zero);
						tran.CreditAmt = (iCharge.DrCr == CADrCr.CADebit ? iCharge.ChargeAmt : Decimal.Zero);
						tran.Released = true;
						je.GLTranModuleBatNbr.Insert(tran);
					}
				}

				foreach (KeyValuePair<int, GLTran> it in rgols)
				{
					GLTran rgol_tran = it.Value;
					decimal rgolAmt = (decimal)(rgol_tran.DebitAmt - rgol_tran.CreditAmt);
					int sign = Math.Sign(rgolAmt);
					rgolAmt = Math.Abs(rgolAmt);

					if ((rgolAmt) != Decimal.Zero)
					{
						tran = (GLTran)je.Caches[typeof(GLTran)].CreateCopy(rgol_tran);
						tran.CuryDebitAmt = Decimal.Zero;
						tran.CuryCreditAmt = Decimal.Zero;
						if (doc.DocType == CATranType.CADeposit)
						{
							tran.AccountID = (sign < 0) ? rgol_cury.RealLossAcctID : rgol_cury.RealGainAcctID;
							tran.SubID = (sign < 0)
                                ? GainLossSubAccountMaskAttribute.GetSubID<Currency.realLossSubID>(je, rgol_tran.BranchID, rgol_cury)
                                : GainLossSubAccountMaskAttribute.GetSubID<Currency.realGainSubID>(je, rgol_tran.BranchID, rgol_cury);
						}
						else
						{
							tran.AccountID = (sign < 0) ? rgol_cury.RealGainAcctID : rgol_cury.RealLossAcctID;
							tran.SubID = (sign < 0)
                                ? GainLossSubAccountMaskAttribute.GetSubID<Currency.realGainSubID>(je, rgol_tran.BranchID, rgol_cury)
                                : GainLossSubAccountMaskAttribute.GetSubID<Currency.realLossSubID>(je, rgol_tran.BranchID, rgol_cury);
						}

						tran.DebitAmt = sign < 0 ? rgolAmt : Decimal.Zero;
						tran.CreditAmt = sign < 0 ? Decimal.Zero : rgolAmt;
						tran.TranType = CATranType.CATransferRGOL;
						tran.RefNbr = doc.RefNbr;
						tran.TranDesc = "RGOL";
						tran.TranDate = rgol_tran.TranDate;
						tran.FinPeriodID = rgol_tran.FinPeriodID;
						tran.TranPeriodID = rgol_tran.TranPeriodID;
						tran.Released = true;
						tran.CuryInfoID = batch.CuryInfoID;
						tran = je.GLTranModuleBatNbr.Insert(tran);

						rgol_tran.CuryDebitAmt = Decimal.Zero;
						rgol_tran.DebitAmt = (sign > 0) ? rgolAmt : Decimal.Zero;
						rgol_tran.CreditAmt = (sign > 0) ? Decimal.Zero : rgolAmt;
						je.GLTranModuleBatNbr.Insert(rgol_tran);
					}
				}
			}
			if (batch != null && batch.CuryCreditTotal != batch.CuryDebitTotal)
				throw new PXException(GL.Messages.BatchOutOfBalance);
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (batch != null)
				{
					je.Save.Press();
					if (batchlist.Contains(batch) == false)
					{
						batchlist.Add(batch);
					}
					doc.Released = true;
					Caches[typeof(CADeposit)].Update(doc);
					if (doc.TranType == CATranType.CAVoidDeposit)
					{
						CADeposit orig = PXSelect<CADeposit, Where<CADeposit.tranType, Equal<CATranType.cADeposit>, And<CADeposit.refNbr, Equal<Required<CADeposit.refNbr>>>>>.Select(this, doc.RefNbr);
						orig.Voided = true;
						Caches[typeof(CADeposit)].Update(orig);
					}
				}
				Caches[typeof(CADeposit)].Persist(PXDBOperation.Update);
				Caches[typeof(CADepositDetail)].Persist(PXDBOperation.Update);
				Caches[typeof(CADepositEntry.ARPaymentUpdate)].Persist(PXDBOperation.Update);
				Caches[typeof(CADepositEntry.APPaymentUpdate)].Persist(PXDBOperation.Update);
				Caches[typeof(CADailySummary)].Persist(PXDBOperation.Insert);

				ts.Complete();
			}

			Caches[typeof(CADeposit)].Persisted(false);
			Caches[typeof(CADepositDetail)].Persisted(false);
			Caches[typeof(CADepositEntry.ARPaymentUpdate)].Persisted(false);
			Caches[typeof(CADepositEntry.APPaymentUpdate)].Persisted(false);
			Caches[typeof(CADailySummary)].Persisted(false);
		}

		private static Batch CreateGLBatch(JournalEntry je, CADeposit doc)
		{
			CurrencyInfo orig = PXSelectReadonly<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(je, doc.CuryInfoID);
			CurrencyInfo newinfo = (CurrencyInfo)je.currencyinfo.Cache.CreateCopy(orig);
			newinfo.CuryInfoID = null;
			newinfo = je.currencyinfo.Insert(newinfo);
			Batch newbatch = new Batch();
			newbatch.Module = BatchModule.CA;
			newbatch.Status = "U";
			newbatch.Released = true;
			newbatch.Hold = false;
			newbatch.DateEntered = doc.TranDate;
			newbatch.FinPeriodID = doc.FinPeriodID;
			newbatch.TranPeriodID = doc.TranPeriodID;
			newbatch.CuryID = doc.CuryID;
			newbatch.CuryInfoID = newinfo.CuryInfoID;
			newbatch.DebitTotal = 0m;
			newbatch.CreditTotal = 0m;
			newbatch = je.BatchModule.Insert(newbatch);
			je.BatchModule.Current = newbatch;
			return newbatch;
		}
	}
}
