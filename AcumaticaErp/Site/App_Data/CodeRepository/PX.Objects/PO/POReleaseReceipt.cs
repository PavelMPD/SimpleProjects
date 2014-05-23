using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.AP;
using PX.Objects.PO;
using PX.Objects.GL;
using PX.Objects.CM;

namespace PX.Objects.PO
{
	[TableAndChartDashboardType]
	public class POReleaseReceipt : PXGraph<POReleaseReceipt>
	{
		public PXCancel<POReceipt> Cancel;
		[PXFilterable]
		public PXProcessing<POReceipt> Orders;

		public PXAction<POReceipt> ViewDocument;
		[PXUIField(DisplayName = Messages.ViewPODocument, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton()]
		public virtual IEnumerable viewDocument(PXAdapter adapter)
		{
			if (this.Orders.Current != null)
			{
				if (this.Orders.Current.Released == false)
				{
					POReceiptEntry graph = PXGraph.CreateInstance<POReceiptEntry>();
					POReceipt poDoc = graph.Document.Search<POReceipt.receiptNbr>(this.Orders.Current.ReceiptNbr, this.Orders.Current.ReceiptType);
					if( poDoc != null)
					{
						graph.Document.Current = poDoc;
						throw new PXRedirectRequiredException(graph, true, "Document"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
					}
				}				
			}
			return adapter.Get();
		}


		public POReleaseReceipt()
		{
			Orders.SetSelected<POReceipt.selected>();
			Orders.SetProcessCaption(Messages.Process);
			Orders.SetProcessAllCaption(Messages.ProcessAll);
			Orders.SetProcessDelegate(delegate(List<POReceipt> list)
			{
				ReleaseDoc(list,true);
			});
		}


		public virtual IEnumerable orders()
		{
			foreach (POReceipt order in Orders.Cache.Updated)
			{
				yield return order;
			}

			foreach (PXResult res in PXSelectJoinGroupBy<POReceipt,
			InnerJoin<Vendor, On<Vendor.bAccountID, Equal<POReceipt.vendorID>>,
			InnerJoin<POReceiptLine, On<POReceiptLine.receiptNbr, Equal<POReceipt.receiptNbr>>,			
			LeftJoin<APTran, On<APTran.receiptNbr, Equal<POReceiptLine.receiptNbr>, 
				And<APTran.receiptLineNbr, Equal<POReceiptLine.lineNbr>>>>>>,
			Where2<Match<Vendor, Current<AccessInfo.userName>>, 
			And<POReceipt.hold, Equal<boolFalse>,
			And<POReceipt.released, Equal<boolFalse>,			
			And<APTran.refNbr, IsNull>>>>,
			Aggregate<GroupBy<POReceipt.receiptNbr,
			GroupBy<POReceipt.receiptType,
			GroupBy<POReceipt.released,
			GroupBy<POReceipt.hold,
			GroupBy<POReceipt.autoCreateInvoice>>>>>>>
			.Select(this))
			{
				POReceipt order;
				if ((order = (POReceipt)Orders.Cache.Locate(res[typeof(POReceipt)])) == null 
						|| Orders.Cache.GetStatus(order) == PXEntryStatus.Notchanged)
				{
					yield return res[typeof(POReceipt)];
				}
			}
			Orders.Cache.IsDirty = false;
		}

		public static void ReleaseDoc( List<POReceipt> list, bool aIsMassProcess)
		{
			POReceiptEntry docgraph = PXGraph.CreateInstance<POReceiptEntry>();
			DocumentList<INRegister> created = new DocumentList<INRegister>(docgraph);
			DocumentList<APInvoice> invoicesCreated = new DocumentList<APInvoice>(docgraph);
			INReceiptEntry iRe = null;
			INIssueEntry iIe = null;
			AP.APInvoiceEntry apInvoiceGraph = PXGraph.CreateInstance<APInvoiceEntry>();
			int iRow = 0;
			bool failed = false;			
			foreach (POReceipt order in list)
			{
				try
				{
					switch (order.ReceiptType)
					{
						case POReceiptType.POReceipt:
							if (iRe == null) iRe = PXGraph.CreateInstance<INReceiptEntry>();
							docgraph.ReleaseReceipt(iRe, apInvoiceGraph, order, created, invoicesCreated, aIsMassProcess);
							break;
						case POReceiptType.POReturn:
							if (iIe == null) iIe = PXGraph.CreateInstance<INIssueEntry>();
							docgraph.ReleaseReturn(iIe, apInvoiceGraph, order, created, invoicesCreated, aIsMassProcess);
							break;
					}
					PXProcessing<POReceipt>.SetInfo(iRow, ActionsMessages.RecordProcessed);
				}
				catch (Exception e) 
				{
					if (aIsMassProcess)
					{
						PXProcessing<POReceipt>.SetError(iRow, e);
						failed = true;
					}
					else
						throw;
				}
				iRow++;
			}
			if (failed)
			{
				throw new PXException(Messages.ReleaseOfOneOrMoreReceiptsHasFailed);
			}
		}
	}
	
	[PXHidden()]
	public class LandedCostProcess : PXGraph<LandedCostProcess>
	{

		#region Type Definition

		public class NoApplicableSourceException : PXException
		{
			public NoApplicableSourceException() : base()
			{
			}

			public NoApplicableSourceException(string message, params object[] aParams)
				: base(message, aParams)
			{

			}

			public NoApplicableSourceException(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
				PXReflectionSerializer.RestoreObjectProps(this, info);
			}

			public override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				PXReflectionSerializer.GetObjectData(this, info);
				base.GetObjectData(info, context);
			}

		}

		#endregion
		

		public PXSelect<LandedCostTran, Where<LandedCostTran.lCTranID, Equal<Required<LandedCostTran.lCTranID>>>> landedCostTrans;
		public PXSelect<POReceiptLine, Where<POReceiptLine.receiptNbr, Equal<Required<POReceiptLine.receiptNbr>>>> poLines;
		public PXSetup<POSetup> poSetup;
		public LandedCostProcess()
		{
			POSetup record = poSetup.Current;
		}

		public virtual void ReleaseLCTrans(IEnumerable<LandedCostTran> aTranSet, DocumentList<INRegister> aINCreated, DocumentList<APInvoice> aAPCreated)
		{
			Dictionary<int, APInvoiceEntry> apGraphs = new Dictionary<int, APInvoiceEntry>();
			Dictionary<int, INAdjustmentEntry> inGraphs = new Dictionary<int, INAdjustmentEntry>();			
			Dictionary<int, int> combinations = new Dictionary<int, int>();
			List<APRegister> forReleaseAP = new List<APRegister>();
			List<INRegister> forReleaseIN = new List<INRegister>();
			DocumentList<APInvoice> apDocuments = new DocumentList<APInvoice>(this); 
			POSetup poSetupR = this.poSetup.Select();
			bool autoReleaseIN = poSetupR.AutoReleaseLCIN.Value;
			bool autoReleaseAP = poSetupR.AutoReleaseAP.Value;
			bool noApplicableItems = false;
			foreach (LandedCostTran iTran in aTranSet)
			{
				LandedCostCode lcCode = PXSelect<LandedCostCode, Where<LandedCostCode.landedCostCodeID, Equal<Required<LandedCostCode.landedCostCodeID>>>>.Select(this, iTran.LandedCostCodeID);
				if ((string.IsNullOrEmpty(iTran.APDocType) || string.IsNullOrEmpty(iTran.APRefNbr))&& iTran.PostponeAP == false)
				{
					APInvoiceEntry apGraph = null;					
					foreach (KeyValuePair<int, APInvoiceEntry> iGraph in apGraphs) 
					{
						APInvoice apDoc = iGraph.Value.Document.Current;
						string terms = String.IsNullOrEmpty(iTran.TermsID) ? lcCode.TermsID : iTran.TermsID;

						if (apDoc.VendorID == iTran.VendorID 
							&& apDoc.VendorLocationID == iTran.VendorLocationID							
							&& apDoc.InvoiceNbr == iTran.InvoiceNbr
							&& apDoc.CuryID == iTran.CuryID
							&& apDoc.DocDate == iTran.InvoiceDate
							&& apDoc.TermsID == terms 
                            && (apDoc.DocType == AP.APDocType.Invoice && iTran.CuryLCAmount > Decimal.Zero)) 
						{
							combinations.Add(iTran.LCTranID.Value, iGraph.Key);
							apGraph = iGraph.Value;
						}
					}
					if (apGraph == null)
					{
						apGraph = PXGraph.CreateInstance<APInvoiceEntry>();
						if (autoReleaseAP)
						{
							apGraph.APSetup.Current.RequireControlTotal = false;
							apGraph.APSetup.Current.HoldEntry = false;
						}
						apGraphs[iTran.LCTranID.Value] = apGraph;
					}
					apGraph.InvoiceLandedCost(iTran, null, false);
					apDocuments.Add(apGraph.Document.Current);
				}
				
				if (lcCode.AllocationMethod != LandedCostAllocationMethod.None)
				{
					List<POReceiptLine> receiptLines = new List<POReceiptLine>();
					List<LandedCostTranSplit> lcTranSplits = new List<LandedCostTranSplit>();
					GetReceiptLinesToAllocate(receiptLines, lcTranSplits, iTran);					
					List<LandedCostUtils.INCostAdjustmentInfo> result = LandedCostUtils.AllocateOverRCTLines(receiptLines, this.poLines.Cache, lcCode, iTran, lcTranSplits);
					if (result.Count > 0)
					{
						if (result.Count == 1 && !result[0].InventoryID.HasValue)
						{
							noApplicableItems = true;  //Skip Cost adjustment creation;
						}
						else
						{
							INAdjustmentEntry inGraph = PXGraph.CreateInstance<INAdjustmentEntry>();
							if (autoReleaseIN)
							{
								inGraph.insetup.Current.RequireControlTotal = false;
								inGraph.insetup.Current.HoldEntry = false;
							}
							CreateCostAjustment(inGraph, lcCode, iTran, result);
							inGraphs[iTran.LCTranID.Value] = inGraph;
						}
					}
				}
			}

			using (PXConnectionScope cs = new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					foreach (LandedCostTran iTran in aTranSet)
					{
						bool needUpdate = false;
						LandedCostTran tran = this.landedCostTrans.Select(iTran.LCTranID);
						
						if (apGraphs.ContainsKey(tran.LCTranID.Value))
						{							
							APInvoiceEntry apGraph = apGraphs[iTran.LCTranID.Value];
							apGraph.Save.Press();
							tran.APDocType = apGraph.Document.Current.DocType;
							tran.APRefNbr = apGraph.Document.Current.RefNbr;
							if(!tran.APCuryInfoID.HasValue)
								tran.APCuryInfoID = apGraph.Document.Current.CuryInfoID;
							tran.Processed = true;
							forReleaseAP.Add(apGraph.Document.Current);
							if (aAPCreated != null)
								aAPCreated.Add(apGraph.Document.Current);
							needUpdate = true;
						}
						else if (combinations.ContainsKey(tran.LCTranID.Value)) 
						{
							//Its already saved at this point
							APInvoiceEntry apGraph = apGraphs[combinations[tran.LCTranID.Value]];
							tran.APDocType = apGraph.Document.Current.DocType;
							tran.APRefNbr = apGraph.Document.Current.RefNbr;
							if (!tran.APCuryInfoID.HasValue)
								tran.APCuryInfoID = apGraph.Document.Current.CuryInfoID;							
							tran.Processed = true;
							needUpdate = true;
						}

						if (inGraphs.ContainsKey(tran.LCTranID.Value))
						{
							INAdjustmentEntry inGraph = inGraphs[iTran.LCTranID.Value];
							inGraph.Save.Press();
							tran.INDocType = inGraph.adjustment.Current.DocType;
							tran.INRefNbr = inGraph.adjustment.Current.RefNbr;
							tran.Processed = true;
							forReleaseIN.Add(inGraph.adjustment.Current); 
							if (aINCreated != null)
								aINCreated.Add(inGraph.adjustment.Current);
							needUpdate = true;
						}
						if (needUpdate)
						{
							LandedCostTran copy = (LandedCostTran)this.landedCostTrans.Cache.CreateCopy(tran);
							tran = this.landedCostTrans.Update(copy);
						}
					}
					this.Actions.PressSave();
					ts.Complete();
				}
			}
			if(noApplicableItems == true) 
			{
				throw new NoApplicableSourceException(Messages.LandedCostAmountRemainderCannotBeDistributedMultyLines);
			}

			if (autoReleaseAP)
			{
				if (forReleaseAP.Count > 0)
						APDocumentRelease.ReleaseDoc(forReleaseAP, true);
			}

			if (autoReleaseIN)
			{
				if (forReleaseIN.Count > 0)
					INDocumentRelease.ReleaseDoc(forReleaseIN, false);
			}			
		}

		protected static void CreateCostAjustment(INAdjustmentEntry inGraph, LandedCostCode aLCCode, LandedCostTran aTran, List<LandedCostUtils.INCostAdjustmentInfo> aAllocatedLines)
		{
			inGraph.Clear();
			inGraph.insetup.Current.RequireControlTotal = false;
			inGraph.insetup.Current.HoldEntry = false;

			INRegister newdoc = new INRegister();
			newdoc.DocType = INDocType.Adjustment;
			newdoc.OrigModule = BatchModule.PO;
			newdoc.SiteID = null;
			newdoc.TranDate = aTran.InvoiceDate;
			inGraph.adjustment.Insert(newdoc);
			//Find IN Receipt (released??) for the LCTran's  POReceipt 
			foreach (LandedCostUtils.INCostAdjustmentInfo it in aAllocatedLines)
			{
				PXResult<INRegister, INTran> res = (PXResult<INRegister, INTran>)PXSelectJoin<INRegister, InnerJoin<INTran, On<INTran.docType, Equal<INRegister.docType>,
											And<INTran.refNbr, Equal<INRegister.refNbr>>>>,
										Where<INRegister.docType, NotEqual<INDocType.adjustment>, And<INTran.pOReceiptNbr, Equal<Required<INTran.pOReceiptNbr>>>>>.SelectWindowed(inGraph, 0, 1, it.POReceiptNbr);

				INRegister inReceipt = new INRegister();
				if (res == null || (inReceipt = res).Released != true)
				{
					throw new PXException(Messages.INReceiptMustBeReleasedBeforeLCProcessing, inReceipt.RefNbr, aTran.POReceiptNbr);
				}

				INTran origtran = res;

				INTran tran = new INTran();
				if (it.IsStockItem)
				{
					tran.TranType = INTranType.ReceiptCostAdjustment;
				}
				else
				{
					//Landed cost for non-stock items are handled in special way in the inventory.
					//They should create a GL Batch, but for convinience and unforminty this functionality is placed into IN module
					//Review this part when the functionality is implemented in IN module
					tran.TranType = INTranType.Adjustment;
					tran.InvtMult = 0;
				}
				tran.InventoryID = it.InventoryID;
				tran.SubItemID = it.SubItemID;
				tran.SiteID = it.SiteID;
				tran.BAccountID = aTran.VendorID;
				tran.LocationID = it.LocationID ?? origtran.LocationID;
				tran.LotSerialNbr = it.LotSerialNbr;
				tran.POReceiptNbr = it.POReceiptNbr;
				tran.POReceiptLineNbr = it.POReceiptLineNbr;

				tran.ARDocType = (origtran.DocType == INDocType.Issue) ? origtran.ARDocType : null;
				tran.ARRefNbr = (origtran.DocType == INDocType.Issue) ? origtran.ARRefNbr : null;
				tran.ARLineNbr = (origtran.DocType == INDocType.Issue) ? origtran.ARLineNbr : null;

				//tran.Qty = it.ReceiptQty;
				tran.TranDesc = aTran.Descr;
				//tran.UnitCost = PXDBPriceCostAttribute.Round(inGraph.transactions.Cache, (decimal)(it.ExtCost / it.ReceiptQty));
				tran.TranCost = it.AdjustmentAmount;
				tran.ReasonCode = aLCCode.ReasonCode;
				tran.OrigTranType = inReceipt.DocType;
				tran.OrigRefNbr = inReceipt.RefNbr;
				tran.AcctID = aLCCode.LCAccrualAcct;
				tran.SubID = aLCCode.LCAccrualSub;
				int? acctID = null;
				int? subID = null;
				GetLCVarianceAccountSub(ref acctID, ref subID, inGraph, it);
				tran.COGSAcctID = acctID;
				tran.COGSSubID = subID;
				tran = inGraph.transactions.Insert(tran);
			}

		}

		//Graph passed in function must be the one using newly created sub in details - otherwise save will fail (inserted subs will be created in other graph)
		protected static void GetLCVarianceAccountSub(ref int? aAccountID, ref int? aSubID, PXGraph aGraph, LandedCostUtils.INCostAdjustmentInfo aRow)
		{
			if (aRow.InventoryID.HasValue)
			{
				PXResult<InventoryItem, INPostClass> res = (PXResult<InventoryItem, INPostClass>)PXSelectJoin<InventoryItem,
									LeftJoin<INPostClass, On<INPostClass.postClassID, Equal<InventoryItem.postClassID>>>,
									Where<InventoryItem.inventoryID, Equal<Required<POLine.inventoryID>>>>.Select(aGraph, aRow.InventoryID);
				if (res != null)
				{
					InventoryItem invItem = (InventoryItem)res;
					INPostClass postClass = (INPostClass)res;
					if ((bool)invItem.StkItem)
					{
						if (postClass == null)
							throw new PXException(Messages.PostingClassIsNotDefinedForTheItemInReceiptRow, invItem.InventoryCD, aRow.POReceiptNbr, aRow.POReceiptLineNbr);
						INSite invSite = null;
						if (aRow.SiteID != null)
							invSite = PXSelect<INSite, Where<INSite.siteID, Equal<Required<POReceiptLine.siteID>>>>.Select(aGraph, aRow.SiteID);
						aAccountID = INReleaseProcess.GetAcctID<INPostClass.lCVarianceAcctID>(aGraph, postClass.LCVarianceAcctDefault, invItem, invSite, postClass);
						if (aAccountID == null)
						{
							throw new PXException(Messages.LCVarianceAccountCanNotBeFoundForItemInReceiptRow, invItem.InventoryCD, aRow.POReceiptNbr, aRow.POReceiptLineNbr, postClass.PostClassID, invSite != null ? invSite.SiteCD : String.Empty);
						}

						try
						{
							aSubID = INReleaseProcess.GetSubID<INPostClass.lCVarianceSubID>(aGraph, postClass.LCVarianceAcctDefault, postClass.LCVarianceSubMask, invItem, invSite, postClass);
						}
						catch(PXException ex)
						{
							if (postClass.LCVarianceSubID == null
								|| string.IsNullOrEmpty(postClass.LCVarianceSubMask)
									|| invItem.LCVarianceSubID == null || invSite == null || invSite.LCVarianceSubID == null)
							{
								throw new PXException(Messages.LCVarianceSubAccountCanNotBeFoundForItemInReceiptRow, invItem.InventoryCD, aRow.POReceiptNbr, aRow.POReceiptLineNbr, postClass.PostClassID, invSite != null ? invSite.SiteCD : String.Empty);
							}
							else 
							{
								throw ex;
							}							
						}
						if(aSubID == null)
							throw new PXException(Messages.LCVarianceSubAccountCanNotBeFoundForItemInReceiptRow, invItem.InventoryCD, aRow.POReceiptNbr, aRow.POReceiptLineNbr, postClass.PostClassID, invSite != null ? invSite.SiteCD : String.Empty);
					}
					else
					{
						aAccountID = aRow.AccountID;
						aSubID = aRow.SubAccountID;
					}
				}
				else
				{
					throw
						new PXException(Messages.LCInventoryItemInReceiptRowIsNotFound, aRow.InventoryID, aRow.POReceiptNbr, aRow.POReceiptLineNbr);
				}
			}
			else
			{
				aAccountID = aRow.AccountID;
				aSubID = aRow.SubAccountID;				
			}
		}

		protected virtual void GetReceiptLinesToAllocate(List<POReceiptLine> aReceiptLines, List<LandedCostTranSplit> aSplits, LandedCostTran aTran)
		{
			Dictionary<string, int> used = new Dictionary<string,int>();
			foreach (POReceiptLine it in this.poLines.Select(aTran.POReceiptNbr)) 
			{
					aReceiptLines.Add(it);
					used[aTran.POReceiptNbr] = 1;
			}
			foreach (LandedCostTranSplit iSplit in PXSelect<LandedCostTranSplit, Where<LandedCostTranSplit.lCTranID, Equal<Required<LandedCostTranSplit.lCTranID>>>>.Select(this, aTran.LCTranID))
			{
				aSplits.Add(iSplit); 
				if (used.ContainsKey(iSplit.POReceiptNbr))
				{
					used[iSplit.POReceiptNbr] += 1;
					continue;
				}						
				foreach (POReceiptLine it in this.poLines.Select(iSplit.POReceiptNbr))
				{
						aReceiptLines.Add(it);
						used[iSplit.POReceiptNbr] = 1;
				}
			}			
		}
	}


	#region Landed Cost Utility Class
	public static class LandedCostUtils
	{

		public static bool IsApplicable(LandedCostTran aTran, LandedCostCode aCode, POReceiptLine aLine) 
		{
			return IsApplicable(aTran, null, aCode, aLine);
		}
		public static bool IsApplicable(LandedCostTran aTran, IEnumerable<LandedCostTranSplit> aSplits, LandedCostCode aCode, POReceiptLine aLine)
		{
			if (!IsApplicable(aCode, aLine)) return false;
			bool passes = false;
			if (aSplits != null)
			{
				foreach (LandedCostTranSplit it in aSplits)
				{					
					if (it.POReceiptNbr == aLine.ReceiptNbr)
					{
						passes = true;
						if (passes && it.POReceiptLineNbr.HasValue)
							if (it.POReceiptLineNbr != aLine.LineNbr) passes = false;
						if (passes && it.InventoryID.HasValue)
							if (it.InventoryID != aLine.InventoryID) passes = false;						
					}
					if (passes)
						break;
				}
			}

			if (!passes)
			{
				if(String.IsNullOrEmpty(aTran.POReceiptNbr)
					|| (aTran.POReceiptNbr != aLine.ReceiptNbr))
					return false;
				if(aTran.POReceiptLineNbr.HasValue)
					if (aTran.POReceiptLineNbr != aLine.LineNbr) return false;
				if (aTran.InventoryID.HasValue)
					if (aTran.InventoryID != aLine.InventoryID) return false;
				passes = true;
			}
			if (passes)
			{
				if (aTran.SiteID.HasValue)
					if (aTran.SiteID != aLine.SiteID) return false;
				if (aTran.LocationID.HasValue)
					if (aTran.LocationID != aLine.LocationID) return false;
			}
			return passes;
		}

		public static bool NeedsApplicabilityChecking(LandedCostCode aCode) 
		{
			return (aCode.AllocationMethod != LandedCostAllocationMethod.None);
		}

		public static bool IsApplicable(LandedCostCode aCode, POReceiptLine aLine)
		{
			//Memo - in this release, non-stock Items are not applicable for the landed cost. Review later.
			return ((aCode.AllocationMethod!=LandedCostAllocationMethod.None) && aLine.LineType == POLineType.GoodsForInventory || 
				aLine.LineType == POLineType.GoodsForReplenishment ||
				aLine.LineType == POLineType.GoodsForSalesOrder ||
				aLine.LineType == POLineType.GoodsForDropShip);
		}
		public static Decimal GetBaseValue(LandedCostCode aCode, POReceiptLine aLine)
		{
			Decimal value = Decimal.Zero;
			switch (aCode.AllocationMethod)
			{
				case LandedCostAllocationMethod.ByCost: value = aLine.CuryExtCost ?? Decimal.Zero; break;
				case LandedCostAllocationMethod.ByQuantity: value = aLine.BaseQty ?? Decimal.Zero; break;
				case LandedCostAllocationMethod.ByWeight: value = aLine.ExtWeight ?? Decimal.Zero; break;
				case LandedCostAllocationMethod.ByVolume: value = aLine.ExtVolume ?? Decimal.Zero; break;
				case LandedCostAllocationMethod.None: value = Decimal.One; break; //Line Count
				default:
					throw new PXException(Messages.UnknownLCAllocationMethod, aCode.LandedCostCodeID);

			}
			return value;
		}
		public static Decimal CalcAllocationValue(LandedCostCode aCode, POReceiptLine aLine, Decimal aBaseTotal, Decimal aAllocationTotal)
		{
			Decimal value = Decimal.Zero;
			if (IsApplicable(aCode, aLine))
			{
				Decimal baseShare = GetBaseValue(aCode, aLine);
				value = (baseShare * aAllocationTotal) / aBaseTotal; 
			}
			return value;
		}
		public static List<INCostAdjustmentInfo> AllocateOverRCTLines(IEnumerable<POReceiptLine> aLines, PXCache aCache, LandedCostCode aLCCode, LandedCostTran aTran, IEnumerable<LandedCostTranSplit> aSplits)
		{
			List<INCostAdjustmentInfo> result = new List<INCostAdjustmentInfo>();
			Decimal toDistribute = aTran.LCAmount.Value;
			Decimal baseTotal = decimal.Zero;
			int distributeCount = 0;

			foreach (POReceiptLine iDet in aLines)
			{
				if (LandedCostUtils.IsApplicable(aTran,aSplits,aLCCode, iDet))
				{
					baseTotal += LandedCostUtils.GetBaseValue(aLCCode, iDet);
					distributeCount++;
				}
			}
			Decimal leftToDistribute = toDistribute;
			INCostAdjustmentInfo last = null;
			foreach (POReceiptLine iDet in aLines)
			{
				if (LandedCostUtils.IsApplicable(aTran,aSplits, aLCCode, iDet))
				{
					decimal shareAmt =Decimal.Zero;
					if (distributeCount > 1)
					{
						shareAmt = LandedCostUtils.CalcAllocationValue(aLCCode, iDet, baseTotal, toDistribute);
						shareAmt = PXDBCurrencyAttribute.BaseRound(aCache.Graph, shareAmt);
					}
					else 
					{
						shareAmt = toDistribute;
					}
					if (shareAmt != Decimal.Zero)
					{
						leftToDistribute -= shareAmt;
						INCostAdjustmentInfo newLine = new INCostAdjustmentInfo(iDet);
						//POReceiptLine newLine = (POReceiptLine)aCache.CreateCopy(iDet);
						//newLine.CuryExtCost = shareAmt;
						//newLine.ExtCost = shareAmt;
						newLine.AdjustmentAmount = shareAmt;
						if (last == null || Math.Abs(last.AdjustmentAmount) < Math.Abs(newLine.AdjustmentAmount)) last = newLine;
						result.Add(newLine);
					}
				}
			}
			if (leftToDistribute != Decimal.Zero)
			{
				if (last == null)
				{
					last = new INCostAdjustmentInfo();
					last.AdjustmentAmount = leftToDistribute;
					result.Add(last);
				}
				else
				{
					last.AdjustmentAmount += leftToDistribute;
				}
			}
			return result;
		}

		public class INCostAdjustmentInfo
		{
			public INCostAdjustmentInfo() { }
			public INCostAdjustmentInfo(POReceiptLine aLine)
			{
				this.InventoryID = aLine.InventoryID;
				this.SubItemID = aLine.SubItemID;
				this.SiteID = aLine.SiteID;
				this.LocationID = aLine.LocationID;
				this.LotSerialNbr = aLine.LotSerialNbr;
				this.Quantity = aLine.BaseReceiptQty;
				this.IsStockItem = aLine.IsStockItem();
				this.POReceiptLineNbr = aLine.LineNbr;
				this.POReceiptNbr = aLine.ReceiptNbr;
				//Account values may be used for the non stock items
				this.AccountID = aLine.ExpenseAcctID; 
				this.SubAccountID = aLine.ExpenseSubID;
			}

			public string POReceiptNbr;
			public int? POReceiptLineNbr;
			public int? InventoryID;
			public int? SubItemID;
			public int? SiteID;
			public int? LocationID;
			public bool IsStockItem;
			public String LotSerialNbr;
			public Decimal AdjustmentAmount;
			public Decimal? Quantity;
			public string INReceiptDocType;
			public string INReceiptRefNbr;
			public int? AccountID;
			public int? SubAccountID;
  
		}
	} 
	#endregion

}
