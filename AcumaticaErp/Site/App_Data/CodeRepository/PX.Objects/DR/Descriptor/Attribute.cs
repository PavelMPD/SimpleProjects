using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.IN;
using System.Collections;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.AR;
using PX.Objects.AP;

namespace PX.Objects.DR
{
	public class DRRevenueAccumAttribute : PXAccumulatorAttribute
	{
		public DRRevenueAccumAttribute()
		{
			base._SingleRecord = true;
		}
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			DRRevenueProjectionAccum item = (DRRevenueProjectionAccum)row;
			columns.Update<DRRevenueProjectionAccum.pTDProjected>(item.PTDProjected, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<DRRevenueProjectionAccum.pTDRecognized>(item.PTDRecognized, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<DRRevenueProjectionAccum.pTDRecognizedSamePeriod>(item.PTDRecognizedSamePeriod, PXDataFieldAssign.AssignBehavior.Summarize);
			
			return true;
		}
	}

	public class DRExpenseAccumAttribute : PXAccumulatorAttribute
	{
		public DRExpenseAccumAttribute()
		{
			base._SingleRecord = true;
		}
		protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
		{
			if (!base.PrepareInsert(sender, row, columns))
			{
				return false;
			}

			DRExpenseProjectionAccum item = (DRExpenseProjectionAccum)row;
			columns.Update<DRExpenseProjectionAccum.pTDProjected>(item.PTDProjected, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<DRExpenseProjectionAccum.pTDRecognized>(item.PTDRecognized, PXDataFieldAssign.AssignBehavior.Summarize);
			columns.Update<DRExpenseProjectionAccum.pTDRecognizedSamePeriod>(item.PTDRecognizedSamePeriod, PXDataFieldAssign.AssignBehavior.Summarize);

			return true;
		}
	}

	public class DRComponentSelectorAttribute : PXCustomSelectorAttribute
	{
		private const string EmptyComponentCD = "<NONE>";

		public DRComponentSelectorAttribute()
			: base(typeof(InventoryItem.inventoryID), new Type[] { typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr) })
		{
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (object.Equals(DRScheduleDetail.EmptyComponentID, e.NewValue))
				return;

			base.FieldVerifying(sender, e);
		}

		public override void SubstituteKeyFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (object.Equals(EmptyComponentCD, e.NewValue))
			{
				e.NewValue = DRScheduleDetail.EmptyComponentID;
			}
			else
				base.SubstituteKeyFieldUpdating(sender, e);
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (object.Equals(DRScheduleDetail.EmptyComponentID, e.ReturnValue))
			{
				e.ReturnValue = EmptyComponentCD;
			}
			else
				base.FieldSelecting(sender, e);
		}

		protected virtual IEnumerable GetRecords()
		{
			bool useOnlyExisting = false;
			PXCache cache = this._Graph.Caches[typeof(DRScheduleDetail)];
			if (cache.Current != null && cache.GetStatus(cache.Current) != PXEntryStatus.Inserted)
			{
				useOnlyExisting = true;
			}


			if (useOnlyExisting)
			{
				PXSelectBase<DRScheduleDetail> select = new PXSelectJoin<DRScheduleDetail,
					LeftJoin<InventoryItem, On<DRScheduleDetail.componentID, Equal<InventoryItem.inventoryID>>>, Where<DRScheduleDetail.scheduleID, Equal<Current<DRScheduleDetail.scheduleID>>>>(this._Graph);

				foreach (PXResult<DRScheduleDetail, InventoryItem> res in select.Select())
				{
					InventoryItem item = (InventoryItem)res;

					if (item.InventoryID != null)
						yield return item;
				}

			}
			else
			{
				foreach (InventoryItem item in PXSelect<InventoryItem>.Select(this._Graph))
				{
					yield return item;
				}
			}

		}
	}


	public class DRDocumentSelectorAttribute : PXCustomSelectorAttribute
	{
		private Type moduleField;
		private Type docTypeField;

		public DRDocumentSelectorAttribute(Type moduleField, Type docTypeField):base(typeof(DRDocumentRecord.refNbr))
		{
			if (moduleField == null)
				throw new ArgumentNullException("moduleField");

			if (docTypeField == null)
				throw new ArgumentNullException("docTypeField");

			if (BqlCommand.GetItemType(moduleField).Name != BqlCommand.GetItemType(docTypeField).Name)
			{
				throw new ArgumentException(string.Format("moduleField and docTypeField must be of the same declaring type. {0} vs {1}", BqlCommand.GetItemType(moduleField).Name, BqlCommand.GetItemType(docTypeField).Name) );
			}

			this.moduleField = moduleField;
			this.docTypeField = docTypeField;
						
		}

        public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {           
        }

		protected virtual IEnumerable GetRecords()
		{
			bool ar = true;
			PXCache cache = this._Graph.Caches[BqlCommand.GetItemType(moduleField)];
			string docType = ARDocType.Invoice;

			if (cache.Current != null )
			{
				docType = (string)cache.GetValue(cache.Current, docTypeField.Name);
				string module = (string)cache.GetValue(cache.Current, moduleField.Name);
				if (module == BatchModule.AP)
					ar = false;
			}

			if (ar)
			{
				PXSelectBase<ARInvoice> select = new 
					PXSelectJoin<ARInvoice, 
					InnerJoin<BAccount, On<BAccount.bAccountID, Equal<ARInvoice.customerID>>>,
					Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>>>(this._Graph);
				foreach (PXResult<ARInvoice, BAccount> res in select.Select(docType))
				{
					ARInvoice doc = (ARInvoice)res;
					BAccount customer = (BAccount)res;


					string status = null;
					ARDocStatus.ListAttribute x = new ARDocStatus.ListAttribute();
					if (x.ValueLabelDic.ContainsKey(doc.Status))
					{
						status = x.ValueLabelDic[doc.Status];
					}
					

					DRDocumentRecord record = new DRDocumentRecord();
					record.BAccountCD = customer.AcctCD;
					record.RefNbr = doc.RefNbr;
					record.Status = status;
					record.FinPeriodID = doc.FinPeriodID;
					record.DocType = doc.DocType;
					record.DocDate = doc.DocDate;
					record.LocationID = doc.CustomerLocationID;
					record.CuryOrigDocAmt = doc.CuryOrigDocAmt;
					record.CuryID = doc.CuryID;

					yield return record;
				}
			}
			else
			{
				PXSelectBase<APInvoice> select = new
					PXSelectJoin<APInvoice,
					InnerJoin<BAccount, On<BAccount.bAccountID, Equal<APInvoice.vendorID>>>,
					Where<APInvoice.docType, Equal<Required<APInvoice.docType>>>>(this._Graph);
				foreach (PXResult<APInvoice, BAccount> res in select.Select(docType))
				{
					APInvoice doc = (APInvoice)res;
					BAccount customer = (BAccount)res;

					string status = null;
					APDocStatus.ListAttribute x = new APDocStatus.ListAttribute();
					if (x.ValueLabelDic.ContainsKey(doc.Status))
					{
						status = x.ValueLabelDic[doc.Status];
					}

					DRDocumentRecord record = new DRDocumentRecord();
					record.BAccountCD = customer.AcctCD;
					record.RefNbr = doc.RefNbr;
					record.Status = status;
					record.FinPeriodID = doc.FinPeriodID;
					record.DocType = doc.DocType;
					record.DocDate = doc.DocDate;
					record.LocationID = doc.VendorLocationID;
					record.CuryOrigDocAmt = doc.CuryOrigDocAmt;
					record.CuryID = doc.CuryID;

					yield return record;
				}
			}
		}
	}

	public class DRLineSelectorAttribute : PXCustomSelectorAttribute
	{
		private Type moduleField;
		private Type docTypeField;
		private Type refNbrField;

		public DRLineSelectorAttribute(Type moduleField, Type docTypeField, Type refNbrField)
			: base(typeof(DRLineRecord.lineNbr))
		{
			if (moduleField == null)
				throw new ArgumentNullException("moduleField");

			if (docTypeField == null)
				throw new ArgumentNullException("docTypeField");

			if (refNbrField == null)
				throw new ArgumentNullException("refNbrField");

			if (BqlCommand.GetItemType(moduleField).Name != BqlCommand.GetItemType(docTypeField).Name)
			{
				throw new ArgumentException(string.Format("moduleField and docTypeField must be of the same declaring type. {0} vs {1}", BqlCommand.GetItemType(moduleField).Name, BqlCommand.GetItemType(docTypeField).Name));
			}

			this.moduleField = moduleField;
			this.docTypeField = docTypeField;
			this.refNbrField = refNbrField;

		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
		}

		protected virtual IEnumerable GetRecords()
		{
			bool ar = true;
			PXCache cache = this._Graph.Caches[BqlCommand.GetItemType(moduleField)];
			string docType = ARDocType.Invoice;
			string refNbr = null;

			if (cache.Current != null)
			{
				docType = (string)cache.GetValue(cache.Current, docTypeField.Name);
				refNbr = (string)cache.GetValue(cache.Current, refNbrField.Name);
				string module = (string)cache.GetValue(cache.Current, moduleField.Name);
				if (module == BatchModule.AP)
					ar = false;
			}

			if (ar)
			{
				PXSelectBase<ARTran> select = new PXSelect<ARTran,
					Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
					And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>(this._Graph);
				foreach (ARTran tran in select.Select(docType, refNbr))
				{
					DRLineRecord record = new DRLineRecord();
					record.CuryInfoID = tran.CuryInfoID;
					record.CuryTranAmt = tran.CuryTranAmt;
					record.InventoryID = tran.InventoryID;
					record.LineNbr = tran.LineNbr;
					record.TranAmt = tran.TranAmt;
					record.TranDesc = tran.TranDesc;

					yield return record;
				}
			}
			else
			{
				PXSelectBase<APTran> select = new PXSelect<APTran,
					Where<APTran.tranType, Equal<Required<APTran.tranType>>,
					And<APTran.refNbr, Equal<Required<APTran.refNbr>>>>>(this._Graph);
				foreach (APTran tran in select.Select(docType, refNbr))
				{
					DRLineRecord record = new DRLineRecord();
					record.CuryInfoID = tran.CuryInfoID;
					record.CuryTranAmt = tran.CuryTranAmt;
					record.InventoryID = tran.InventoryID;
					record.LineNbr = tran.LineNbr;
					record.TranAmt = tran.TranAmt;
					record.TranDesc = tran.TranDesc;

					yield return record;
				}
			}
		}
	}

	[Serializable]
	public partial class DRDocumentRecord : IBqlTable
	{
		#region DocType
		public abstract class docType : PX.Data.IBqlField
		{
		}
		protected String _DocType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true)]
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
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region DocDate
		public abstract class docDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _DocDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
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
		[FinPeriodID]
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
		#region BAccountCD
		public abstract class bAccountCD : PX.Data.IBqlField
		{
		}
		protected string _BAccountCD;
		[PXDBString]
		[PXUIField(DisplayName="Customer/Vendor", Visibility=PXUIVisibility.SelectorVisible)]
		public virtual string BAccountCD
		{
			get
			{
				return this._BAccountCD;
			}
			set
			{
				this._BAccountCD = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.IBqlField
		{
		}
		protected Int32? _LocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<DRDocumentRecord.bAccountCD>>>), Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Location.descr))]
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
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region Status
		public abstract class status : PX.Data.IBqlField
		{
		}
		protected String _Status;
		[PXDBString()]
		[PXUIField(DisplayName = "Processing Status", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryOrigDocAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal]
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

	}

	[Serializable]
	public partial class DRLineRecord : IBqlTable
	{
		#region LineNbr
		public abstract class lineNbr : PX.Data.IBqlField
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.IBqlField
		{
		}
		protected Int32? _InventoryID;
		[NonStockItem(Filterable = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.IBqlField
		{
		}
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Transaction Descr.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(ARRegister.curyInfoID))]
		public virtual Int64? CuryInfoID
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
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryTranAmt;
		[PXDBCurrency(typeof(DRLineRecord.curyInfoID), typeof(DRLineRecord.tranAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region TranAmt
		public abstract class tranAmt : PX.Data.IBqlField
		{
		}
		protected Decimal? _TranAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
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

	}


    #region Used in Reporting to define default Period 'From' and 'To'

    [Serializable]
    public partial class DRFinPeriodSelection : PX.Data.IBqlTable
    {
        #region StartPeriodID
        public abstract class startPeriodID : PX.Data.IBqlField
        {
        }
        protected String _StartPeriodID;
        [PXString]
        [DRFirstPeriodOfYearDefault]
        public virtual String StartPeriodID
        {
            get
            {
                return this._StartPeriodID;
            }
            set
            {
                this._StartPeriodID = value;
            }
        }
        #endregion

        #region EndPeriodID
        public abstract class endPeriodID : PX.Data.IBqlField
        {
        }
        protected String _EndPeriodID;
        [PXString]
        [DRLastPeriodOfYearDefault]
        public virtual String EndPeriodID
        {
            get
            {
                return this._EndPeriodID;
            }
            set
            {
                this._EndPeriodID = value;
            }
        }
        #endregion

    }

    public class DRFirstPeriodOfYearDefaultAttribute : PXDefaultAttribute
    {
        public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            base.FieldDefaulting(sender, e);

            PXSelectBase<FinPeriod> select = new PXSelect<FinPeriod, Where<FinPeriod.finYear, Equal<Required<FinPeriod.finYear>>>, OrderBy<Asc<FinPeriod.periodNbr>>>(sender.Graph);

            FinPeriod fp = select.SelectWindowed(0, 1, DateTime.Now.Year);
            if (fp != null)
            {
                e.NewValue = FinPeriodIDFormattingAttribute.FormatForDisplay(fp.FinPeriodID);
            }
        }
    }

    public class DRLastPeriodOfYearDefaultAttribute : PXDefaultAttribute
    {
        public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            base.FieldDefaulting(sender, e);

			PXSelectBase<FinPeriod> select = new PXSelect<FinPeriod, 
				Where<FinPeriod.finYear, Equal<Required<FinPeriod.finYear>>,
				And<FinPeriod.startDate, NotEqual<FinPeriod.endDate>>>, OrderBy<Desc<FinPeriod.periodNbr>>>(sender.Graph);

            FinPeriod fp = select.SelectWindowed(0, 1, DateTime.Now.Year);
            if (fp != null)
            {
                e.NewValue = FinPeriodIDFormattingAttribute.FormatForDisplay(fp.FinPeriodID);
            }
        }
    }

    #endregion

}
