using System;
using System.Collections.Generic;
using System.Collections;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CR;

namespace PX.Objects.AP
{
#if false
	[Obsolete("This graph has been replaced by APVendorBalanceEnq and should not be used",true)]
	public class APHistoryEnq : PXGraph<APHistoryEnq>
	{
		[Serializable]
		public partial class APHistoryFilter : IBqlTable
		{
			#region APAcctID
			public abstract class aPAcctID : PX.Data.IBqlField
			{
			}
			protected Int32? _APAcctID;
			[GL.Account(DisplayName = "AP Account", DescriptionField = typeof(GL.Account.description))]
			public virtual Int32? APAcctID
			{
				get
				{
					return this._APAcctID;
				}
				set
				{
					this._APAcctID = value;
				}
			}
			#endregion
			#region APSubID
			public abstract class aPSubID : PX.Data.IBqlField
			{
			}
			protected Int32? _APSubID;
			[GL.SubAccount(DisplayName = "AP Sub.", DescriptionField = typeof(GL.Sub.description))]
			public virtual Int32? APSubID
			{
				get
				{
					return this._APSubID;
				}
				set
				{
					this._APSubID = value;
				}
			}
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.IBqlField
			{
			}
			protected String _CuryID;
			[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
			[PXSelector(typeof(CM.Currency.curyID), CacheGlobal = true)]
			[PXUIField(DisplayName = "Currency ID")]
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
			#region CashAcctID
			public abstract class cashAcctID : PX.Data.IBqlField
			{
			}
			protected Int32? _CashAcctID;
			[GL.CashAccount(null, DisplayName = "Cash Account", DescriptionField = typeof(GL.Account.description))]
			public virtual Int32? CashAcctID
			{
				get
				{
					return this._CashAcctID;
				}
				set
				{
					this._CashAcctID = value;
				}
			}
			#endregion
			#region PaymentTypeID
			public abstract class paymentTypeID : PX.Data.IBqlField
			{
			}
			protected String _PaymentTypeID;
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Payment Type")]
			[PXSelector(typeof(CA.PaymentType.paymentTypeID), DescriptionField = typeof(CA.PaymentType.descr))]
			public virtual String PaymentTypeID
			{
				get
				{
					return this._PaymentTypeID;
				}
				set
				{
					this._PaymentTypeID = value;
				}
			}
			#endregion
			#region VendorClassID
			public abstract class vendorClassID : PX.Data.IBqlField
			{
			}
			protected String _VendorClassID;
			[PXDBString(10, IsUnicode = true)]
			[PXSelector(typeof(VendorClass.vendorClassID), DescriptionField = typeof(VendorClass.descr), CacheGlobal = true)]
			[PXUIField(DisplayName = "Vendor Class")]
			public virtual String VendorClassID
			{
				get
				{
					return this._VendorClassID;
				}
				set
				{
					this._VendorClassID = value;
				}
			}
			#endregion
			#region Status
			public abstract class status : PX.Data.IBqlField
			{
			}
			protected String _Status;
			[PXDBString(1, IsFixed = true)]
			[PXUIField(DisplayName = "Status")]
			[BAccount.status.List()]
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
			#region ShowWithBalanceOnly
			public abstract class showWithBalanceOnly : PX.Data.IBqlField
			{
			}
			protected bool? _ShowWithBalanceOnly;
			[PXDBBool()]
			[PXUIField(DisplayName = "Show with Balance Only")]
			public virtual bool? ShowWithBalanceOnly
			{
				get
				{
					return this._ShowWithBalanceOnly;
				}
				set
				{
					this._ShowWithBalanceOnly = value;
				}
			}
			#endregion
			#region Period
			public abstract class period : PX.Data.IBqlField
			{
			}
			protected String _Period;
			[GL.FinPeriodSelector()]
			[PXUIField(DisplayName = "Period", Visibility = PXUIVisibility.Visible)]
			public virtual String Period
			{
				get
				{
					return this._Period;
				}
				set
				{
					this._Period = value;
				}
			}
			#endregion
			#region ByFinancialPeriod
			public abstract class byFinancialPeriod : PX.Data.IBqlField
			{
			}
			protected bool? _ByFinancialPeriod;
			[PXDBBool()]
			[PXUIField(DisplayName = "By Financial Period")]
			public virtual bool? ByFinancialPeriod
			{
				get
				{
					return this._ByFinancialPeriod;
				}
				set
				{
					this._ByFinancialPeriod = value;
				}
			}
			#endregion
		}

		[Serializable]
		public partial class APHistoryResult : IBqlTable
		{
			#region AcctCD
			public abstract class acctCD : PX.Data.IBqlField
			{
			}
			protected string _AcctCD;
			[PXDimension("BIZACCT")]
			[PXDBString(30, IsUnicode = true,IsKey = true)]
			[PXUIField(DisplayName = "Vendor ID")]
			public virtual String AcctCD
			{
				get
				{
					return this._AcctCD;
				}
				set
				{
					this._AcctCD = value;
				}
			}
			#endregion
			#region AcctName
			public abstract class acctName : PX.Data.IBqlField
			{
			}
			protected String _AcctName;
			[PXDBString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Vendor Name")]
			public virtual String AcctName
			{
				get
				{
					return this._AcctName;
				}
				set
				{
					this._AcctName = value;
				}
			}
			#endregion
			#region BegBalance
			public abstract class begBalance : PX.Data.IBqlField
			{
			}
			protected Decimal? _BegBalance;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Beginning Balance", Visible = false)]
			public virtual Decimal? BegBalance
			{
				get
				{
					return this._BegBalance;
				}
				set
				{
					this._BegBalance = value;
				}
			}
			#endregion
			#region EndBalance
			public abstract class endBalance : PX.Data.IBqlField
			{
			}
			protected Decimal? _EndBalance;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Ending Balance", Visible = false)]
			public virtual Decimal? EndBalance
			{
				get
				{
					return this._EndBalance;
				}
				set
				{
					this._EndBalance = value;
				}
			}
			#endregion
			#region Balance
			public abstract class balance : PX.Data.IBqlField
			{
			}
			protected Decimal? _Balance;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Balance")]
			public virtual Decimal? Balance
			{
				get
				{
					return this._Balance;
				}
				set
				{
					this._Balance = value;
				}
			}
			#endregion
			#region Purchases
			public abstract class purchases : PX.Data.IBqlField
			{
			}
			protected Decimal? _Purchases;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Purchases")]
			public virtual Decimal? Purchases
			{
				get
				{
					return this._Purchases;
				}
				set
				{
					this._Purchases = value;
				}
			}
			#endregion
			#region Payments
			public abstract class payments : PX.Data.IBqlField
			{
			}
			protected Decimal? _Payments;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Payments")]
			public virtual Decimal? Payments
			{
				get
				{
					return this._Payments;
				}
				set
				{
					this._Payments = value;
				}
			}
			#endregion
			#region Discount
			public abstract class discount : PX.Data.IBqlField
			{
			}
			protected Decimal? _Discount;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Cash Discount Taken")]
			public virtual Decimal? Discount
			{
				get
				{
					return this._Discount;
				}
				set
				{
					this._Discount = value;
				}
			}
			#endregion
			#region RGOL
			public abstract class rGOL : PX.Data.IBqlField
			{
			}
			protected Decimal? _RGOL;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Realized Gain/Loss")]
			public virtual Decimal? RGOL
			{
				get
				{
					return this._RGOL;
				}
				set
				{
					this._RGOL = value;
				}
			}
			#endregion
			#region CrAdjustments
			public abstract class crAdjustments : PX.Data.IBqlField
			{
			}
			protected Decimal? _CrAdjustments;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Credit Adjustments")]
			public virtual Decimal? CrAdjustments
			{
				get
				{
					return this._CrAdjustments;
				}
				set
				{
					this._CrAdjustments = value;
				}
			}
			#endregion
			#region DrAdjustments
			public abstract class drAdjustments : PX.Data.IBqlField
			{
			}
			protected Decimal? _DrAdjustments;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Debit Adjustments")]
			public virtual Decimal? DrAdjustments
			{
				get
				{
					return this._DrAdjustments;
				}
				set
				{
					this._DrAdjustments = value;
				}
			}
			#endregion
		}

		private sealed class decimalZero : Constant<decimal>
		{
			public decimalZero()
				: base(0m)
			{
			}
		}

		public PXFilter<APHistoryFilter> Filter;
		public PXCancel<APHistoryFilter> Cancel;
		public PXSelect<APHistoryResult> History;

		public APHistoryEnq()
		{
			History.Cache.AllowDelete = false;
			History.Cache.AllowInsert = false;
			History.Cache.AllowUpdate = false;
		}

		protected virtual IEnumerable history()
		{
			APHistoryFilter header = Filter.Current;
			if (header == null)
			{
				yield break;
			}

			if (header.Period == null)
			{
				PXSelectBase<Vendor> sel = new PXSelectJoinGroupBy<Vendor,
					LeftJoin<APHistory,
						On<APHistory.vendorID, Equal<Vendor.bAccountID>>>,
					Aggregate<
						Sum<APHistory.finPtdCrAdjustments,
						Sum<APHistory.finPtdDiscTaken,
						Sum<APHistory.finPtdDrAdjustments,
						Sum<APHistory.finPtdPayments,
						Sum<APHistory.finPtdPurchases,
						Sum<APHistory.finPtdRGOL,
						Sum<APHistory.tranPtdCrAdjustments,
						Sum<APHistory.tranPtdDiscTaken,
						Sum<APHistory.tranPtdDrAdjustments,
						Sum<APHistory.tranPtdPayments,
						Sum<APHistory.tranPtdPurchases,
						Sum<APHistory.tranPtdRGOL,
						GroupBy<Vendor.bAccountID>>>>>>>>>>>>>>>(this);

				if (header.APAcctID != null)
				{
					sel.WhereAnd<Where<Vendor.aPAcctID, Equal<Current<APHistoryFilter.aPAcctID>>>>();
				}
				if (header.APSubID != null)
				{
					sel.WhereAnd<Where<Vendor.aPSubID, Equal<Current<APHistoryFilter.aPSubID>>>>();
				}
				bool foreign = false;
				if (header.CuryID != null)
				{
					sel.WhereAnd<Where<Vendor.curyID, Equal<Current<APHistoryFilter.curyID>>>>();
					GL.Company company = PXSelect<GL.Company>.Select(this);
					foreign = (company == null || company.BaseCuryID != header.CuryID);
					if (foreign)
					{
						PXUIFieldAttribute.SetVisible<APHistoryResult.rGOL>(History.Cache, null, false);
					}
				}
				if (header.CashAcctID != null)
				{
					sel.WhereAnd<Where<Vendor.cashAcctID, Equal<Current<APHistoryFilter.cashAcctID>>>>();
				}
				if (header.PaymentTypeID != null)
				{
					sel.WhereAnd<Where<Vendor.paymentTypeID, Equal<Current<APHistoryFilter.paymentTypeID>>>>();
				}
				if (header.VendorClassID != null)
				{
					sel.WhereAnd<Where<Vendor.vendorClassID, Equal<Current<APHistoryFilter.vendorClassID>>>>();
				}
				if (header.Status != null)
				{
					sel.WhereAnd<Where<Vendor.status, Equal<Current<APHistoryFilter.status>>>>();
				}

				foreach (PXResult<Vendor, APHistory> record in sel.Select())
				{
					Vendor vendor = record;
					APHistory history = record;

					APHistoryResult res = new APHistoryResult();

					res.AcctCD = vendor.AcctCD;
					res.AcctName = vendor.AcctName;

					if (header.ByFinancialPeriod != true)
					{
						if (!foreign)
						{
							res.Purchases = history.TranPtdPurchases ?? 0m;
							res.Payments = history.TranPtdPayments ?? 0m;
							res.Discount = history.TranPtdDiscTaken ?? 0m;
							res.RGOL = history.TranPtdRGOL ?? 0m;
							res.CrAdjustments = history.TranPtdCrAdjustments ?? 0m;
							res.DrAdjustments = history.TranPtdDrAdjustments ?? 0m;
							res.Balance = res.Purchases
								- res.Payments
								- res.Discount
								+ res.RGOL
								- res.DrAdjustments
								+ res.CrAdjustments;
						}
						else
						{
							res.Purchases = history.TranPtdPurchases ?? 0m;
							res.Payments = history.TranPtdPayments ?? 0m;
							res.Discount = history.TranPtdDiscTaken ?? 0m;
							res.RGOL = 0m;
							res.CrAdjustments = history.TranPtdCrAdjustments ?? 0m;
							res.DrAdjustments = history.TranPtdDrAdjustments ?? 0m;
							res.Balance = res.Purchases
								- res.Payments
								- res.Discount
								- res.DrAdjustments
								+ res.CrAdjustments;
						}
					}
					else
					{
						if (!foreign)
						{
							res.Purchases = history.FinPtdPurchases ?? 0m;
							res.Payments = history.FinPtdPayments ?? 0m;
							res.Discount = history.FinPtdDiscTaken ?? 0m;
							res.RGOL = history.FinPtdRGOL ?? 0m;
							res.CrAdjustments = history.FinPtdCrAdjustments ?? 0m;
							res.DrAdjustments = history.FinPtdDrAdjustments ?? 0m;
							res.Balance = res.Purchases
								- res.Payments
								- res.Discount
								+ res.RGOL
								- res.DrAdjustments
								+ res.CrAdjustments;
						}
						else
						{
							res.Purchases = history.FinPtdPurchases ?? 0m;
							res.Payments = history.FinPtdPayments ?? 0m;
							res.Discount = history.FinPtdDiscTaken ?? 0m;
							res.RGOL = 0m;
							res.CrAdjustments = history.FinPtdCrAdjustments ?? 0m;
							res.DrAdjustments = history.FinPtdDrAdjustments ?? 0m;
							res.Balance = res.Purchases
								- res.Payments
								- res.Discount
								- res.DrAdjustments
								+ res.CrAdjustments;
						}
					}

					if (res.Balance != 0m || header.ShowWithBalanceOnly != true)
					{
						yield return res;
					}
				}
			}
			else
			{
				PXSelectBase<Vendor> sel = new PXSelectJoinGroupBy<Vendor,
					LeftJoin<APHistory,
						On<APHistory.vendorID, Equal<Vendor.bAccountID>,
						And<APHistory.finPeriodID, Equal<Current<APHistoryFilter.period>>>>>,
					Aggregate<
						Sum<APHistory.finBegBalance,
						Sum<APHistory.finYtdBalance,
						Sum<APHistory.finPtdCrAdjustments,
						Sum<APHistory.finPtdDiscTaken,
						Sum<APHistory.finPtdDrAdjustments,
						Sum<APHistory.finPtdPayments,
						Sum<APHistory.finPtdPurchases,
						Sum<APHistory.finPtdRGOL,
						Sum<APHistory.tranBegBalance,
						Sum<APHistory.tranYtdBalance,
						Sum<APHistory.tranPtdCrAdjustments,
						Sum<APHistory.tranPtdDiscTaken,
						Sum<APHistory.tranPtdDrAdjustments,
						Sum<APHistory.tranPtdPayments,
						Sum<APHistory.tranPtdPurchases,
						Sum<APHistory.tranPtdRGOL,
						GroupBy<Vendor.bAccountID>>>>>>>>>>>>>>>>>>>(this);

				PXUIFieldAttribute.SetVisible<APHistoryResult.balance>(History.Cache, null, false);
				PXUIFieldAttribute.SetVisible<APHistoryResult.begBalance>(History.Cache, null);
				PXUIFieldAttribute.SetVisible<APHistoryResult.endBalance>(History.Cache, null);

				if (header.APAcctID != null)
				{
					sel.WhereAnd<Where<Vendor.aPAcctID, Equal<Current<APHistoryFilter.aPAcctID>>>>();
				}
				if (header.APSubID != null)
				{
					sel.WhereAnd<Where<Vendor.aPSubID, Equal<Current<APHistoryFilter.aPSubID>>>>();
				}
				bool foreign = false;
				if (header.CuryID != null)
				{
					sel.WhereAnd<Where<Vendor.curyID, Equal<Current<APHistoryFilter.curyID>>>>();
					GL.Company company = PXSelect<GL.Company>.Select(this);
					foreign = (company == null || company.BaseCuryID != header.CuryID);
					if (foreign)
					{
						PXUIFieldAttribute.SetVisible<APHistoryResult.rGOL>(History.Cache, null, false);
					}
				}
				if (header.CashAcctID != null)
				{
					sel.WhereAnd<Where<Vendor.cashAcctID, Equal<Current<APHistoryFilter.cashAcctID>>>>();
				}
				if (header.PaymentTypeID != null)
				{
					sel.WhereAnd<Where<Vendor.paymentTypeID, Equal<Current<APHistoryFilter.paymentTypeID>>>>();
				}
				if (header.VendorClassID != null)
				{
					sel.WhereAnd<Where<Vendor.vendorClassID, Equal<Current<APHistoryFilter.vendorClassID>>>>();
				}
				if (header.Status != null)
				{
					sel.WhereAnd<Where<Vendor.status, Equal<Current<APHistoryFilter.status>>>>();
				}

				foreach (PXResult<Vendor, APHistory> record in sel.Select())
				{
					Vendor vendor = record;
					APHistory history = record;

					APHistoryResult res = new APHistoryResult();

					res.AcctCD = vendor.AcctCD;
					res.AcctName = vendor.AcctName;

					if (header.ByFinancialPeriod != true)
					{
						if (!foreign)
						{
							res.Purchases = history.TranPtdPurchases ?? 0m;
							res.Payments = history.TranPtdPayments ?? 0m;
							res.Discount = history.TranPtdDiscTaken ?? 0m;
							res.RGOL = history.TranPtdRGOL ?? 0m;
							res.CrAdjustments = history.TranPtdCrAdjustments ?? 0m;
							res.DrAdjustments = history.TranPtdDrAdjustments ?? 0m;
							res.BegBalance = history.TranBegBalance ?? 0m;
							res.EndBalance = res.BegBalance
								+ res.Purchases
								- res.Payments
								- res.Discount
								+ res.RGOL
								- res.DrAdjustments
								+ res.CrAdjustments;
						}
						else
						{
							res.Purchases = history.TranPtdPurchases ?? 0m;
							res.Payments = history.TranPtdPayments ?? 0m;
							res.Discount = history.TranPtdDiscTaken ?? 0m;
							res.RGOL = 0m;
							res.CrAdjustments = history.TranPtdCrAdjustments ?? 0m;
							res.DrAdjustments = history.TranPtdDrAdjustments ?? 0m;
							res.BegBalance = history.TranBegBalance ?? 0m;
							res.EndBalance = res.BegBalance
								+ res.Purchases
								- res.Payments
								- res.Discount
								- res.DrAdjustments
								+ res.CrAdjustments;
						}
					}
					else
					{
						if (!foreign)
						{
							res.Purchases = history.FinPtdPurchases ?? 0m;
							res.Payments = history.FinPtdPayments ?? 0m;
							res.Discount = history.FinPtdDiscTaken ?? 0m;
							res.RGOL = history.FinPtdRGOL ?? 0m;
							res.CrAdjustments = history.FinPtdCrAdjustments ?? 0m;
							res.DrAdjustments = history.FinPtdDrAdjustments ?? 0m;
							res.BegBalance = history.FinBegBalance ?? 0m;
							res.EndBalance = res.BegBalance
								+ res.Purchases
								- res.Payments
								- res.Discount
								+ res.RGOL
								- res.DrAdjustments
								+ res.CrAdjustments;
						}
						else
						{
							res.Purchases = history.FinPtdPurchases ?? 0m;
							res.Payments = history.FinPtdPayments ?? 0m;
							res.Discount = history.FinPtdDiscTaken ?? 0m;
							res.RGOL = 0m;
							res.CrAdjustments = history.FinPtdCrAdjustments ?? 0m;
							res.DrAdjustments = history.FinPtdDrAdjustments ?? 0m;
							res.BegBalance = history.FinBegBalance ?? 0m;
							res.EndBalance = res.BegBalance
								+ res.Purchases
								- res.Payments
								- res.Discount
								- res.DrAdjustments
								+ res.CrAdjustments;
						}
					}

					if (res.Balance != 0m || header.ShowWithBalanceOnly != true)
					{
						yield return res;
					}
				}
			}
		}

		public virtual void APHistoryFilter_APAcctID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			APHistoryFilter header = e.Row as APHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.APAcctID = null;
			}
		}

		public virtual void APHistoryFilter_APSubID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			APHistoryFilter header = e.Row as APHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.APSubID = null;
			}
		}

		public virtual void APHistoryFilter_CashAcctID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			APHistoryFilter header = e.Row as APHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.CashAcctID = null;
			}
		}

		public virtual void APHistoryFilter_CuryID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			APHistoryFilter header = e.Row as APHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.CuryID = null;
			}
		}

		public virtual void APHistoryFilter_PaymentTypeID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			APHistoryFilter header = e.Row as APHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.PaymentTypeID = null;
			}
		}

		public virtual void APHistoryFilter_Period_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			APHistoryFilter header = e.Row as APHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.Period = null;
			}
		}

		public virtual void APHistoryFilter_VendorClassID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			APHistoryFilter header = e.Row as APHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.VendorClassID = null;
			}
		}
	}
#endif 
}

