using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CS;
using System.Collections;

namespace PX.Objects.TX
{
	public class TaxParentAttribute : PXParentAttribute
	{
		public TaxParentAttribute(Type selectParent)
			: base(selectParent)
		{
			throw new PXArgumentException();
		}

		public static void NewChild(PXCache cache, object parentrow, Type ParentType, out object child)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXParentAttribute && ((PXParentAttribute)attr).ParentType.IsAssignableFrom(ParentType))
				{
					Type childType = cache.GetItemType();

					PXView parentView = ((PXParentAttribute)attr).GetParentSelect(cache);
					Type parentType = parentView.BqlSelect.GetTables()[0];

					PXView childView = ((PXParentAttribute)attr).GetChildrenSelect(cache);
					BqlCommand selectChild = childView.BqlSelect;

					IBqlParameter[] pars = selectChild.GetParameters();
					Type[] refs = selectChild.GetReferencedFields(false);

					child = Activator.CreateInstance(childType);
					PXCache parentcache = cache.Graph.Caches[parentType];

					for (int i = 0; i < pars.Length; i++)
					{
						Type partype = pars[i].GetReferencedType();
						object val = parentcache.GetValue(parentrow, partype.Name);
						cache.SetValue(child, refs[i].Name, val);
					}
					return;
				}
			}
			child = null;
		}

		public static object ParentSelect(PXCache cache, object row, Type ParentType)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXParentAttribute && ((PXParentAttribute)attr).ParentType.IsAssignableFrom(ParentType))
				{
					PXView parentview = ((PXParentAttribute)attr).GetParentSelect(cache);
					return parentview.SelectSingleBound(new object[] { row });
				}
			}
			return null;
		}

		public static List<object> ChildSelect(PXCache cache, object row)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXParentAttribute)
				{
					PXView view = ((PXParentAttribute)attr).GetChildrenSelect(cache);
					return view.SelectMultiBound(new object[] { row });
				}
			}
			return null;
		}

		public static List<object> ChildSelect(PXCache cache, object row, Type ParentType)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXParentAttribute && ((PXParentAttribute)attr).ParentType.IsAssignableFrom(ParentType))
				{
					PXView view = ((PXParentAttribute)attr).GetChildrenSelect(cache);
					return view.SelectMultiBound(new object[] { row });
				}
			}
			return null;
		}

	}

	public enum PXTaxCheck
	{
		Line,
		RecalcLine,
		RecalcTotals,
	}

	public enum TaxCalc
	{
		NoCalc,
		Calc,
		ManualCalc,
		ManualLineCalc
	}

    public abstract class TaxAttribute : TaxBaseAttribute 
    {
		public TaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
            :base(ParentType, TaxType, TaxSumType)
		{
		}

        protected override IEnumerable<ITaxDetail> ManualTaxes(PXCache sender, object row)
        {
            List<ITaxDetail> ret = new List<ITaxDetail>();

            foreach (PXResult res in SelectTaxes(sender, row, PXTaxCheck.RecalcTotals))
            {
                ret.Add((ITaxDetail)res[0]);
            }
            return ret;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            sender.Graph.RowInserted.AddHandler(_TaxSumType, Tax_RowInserted);
            sender.Graph.RowDeleted.AddHandler(_TaxSumType, Tax_RowDeleted);
            sender.Graph.RowUpdated.AddHandler(_TaxSumType, Tax_RowUpdated);
        }

        protected bool _NoSumTotals = false;

        protected virtual void Tax_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if ((_TaxCalc != TaxCalc.NoCalc && e.ExternalCall || _TaxCalc == TaxCalc.ManualCalc))
            {
                if (e.OldRow != null && e.Row != null && ((TaxDetail)e.Row).CuryTaxAmt != ((TaxDetail)e.OldRow).CuryTaxAmt)
                {
                    CalcTotals(sender.Graph.Caches[_ChildType], null, false);
                }
            }
        }

        protected virtual void Tax_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            bool nomatch = false;
            if ((_TaxCalc != TaxCalc.NoCalc && e.ExternalCall || _TaxCalc == TaxCalc.ManualCalc))
            {
                PXCache cache = sender.Graph.Caches[_ChildType];
                PXCache taxcache = sender.Graph.Caches[_TaxType];

				List<object> details = TaxParentAttribute.ChildSelect(cache, e.Row, _ParentType);
                foreach (object det in details)
                {
                    ITaxDetail taxzonedet = MatchesCategory(cache, det, (ITaxDetail)e.Row);
                    AddOneTax(taxcache, det, taxzonedet);
                }
                _NoSumTotals = (_TaxCalc == TaxCalc.ManualCalc && ((TaxDetail)e.Row).TaxRate != 0m && e.ExternalCall == false);
				PXRowDeleting del = delegate(PXCache _sender, PXRowDeletingEventArgs _e) { nomatch |= object.ReferenceEquals(e.Row, _e.Row); };
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

                if (nomatch)
                {
                    sender.RaiseExceptionHandling("TaxID", e.Row, ((TaxDetail)e.Row).TaxID, new PXSetPropertyException(TX.Messages.NoLinesMatchTax, PXErrorLevel.RowError));
                }
            }
        }

        protected virtual void Tax_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            if ((_TaxCalc != TaxCalc.NoCalc && e.ExternalCall || _TaxCalc == TaxCalc.ManualCalc))
            {
                PXCache cache = sender.Graph.Caches[_ChildType];
                PXCache taxcache = sender.Graph.Caches[_TaxType];

                List<object> details = TaxParentAttribute.ChildSelect(cache, e.Row, _ParentType);
                foreach (object det in details)
                {
                    DelOneTax(taxcache, det, e.Row);
                }
                CalcTaxes(cache, null);
            }
        }

        protected override void CalcTotals(PXCache sender, object row, bool CalcTaxes)
        {
            base.CalcTotals(sender, row, (_NoSumTotals == false && CalcTaxes));
        }
    }

	public abstract class TaxBaseAttribute : PXAggregateAttribute, IPXRowInsertedSubscriber, IPXRowUpdatedSubscriber, IPXRowDeletedSubscriber, IComparable
	{
		protected string _CuryTaxAmt = "CuryTaxAmt";
		protected string _CuryTaxableAmt = "CuryTaxableAmt";
		protected string _CuryRateTypeID = "CuryRateTypeID";
		protected string _CuryEffDate = "CuryEffDate";
		protected string _CuryRate = "SampleCuryRate";
		protected string _RecipRate = "SampleRecipRate";
		protected string _IsTaxSaved = "IsTaxSaved";

		protected Type _ParentType;
		protected Type _ChildType;
		protected Type _TaxType;
		protected Type _TaxSumType;
		protected Type _CuryKeyField = null;

		protected Dictionary<object, object> inserted = null;
		protected Dictionary<object, object> updated = null;

		#region TaxID
		protected string _TaxID = "TaxID";
		public Type TaxID
		{
			set
			{
				_TaxID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region TaxCategoryID
		protected string _TaxCategoryID = "TaxCategoryID";
		public Type TaxCategoryID
		{
			set
			{
				_TaxCategoryID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region TaxZoneID
		protected string _TaxZoneID = "TaxZoneID";
		public Type TaxZoneID
		{
			set
			{
				_TaxZoneID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region DocDate
		protected string _DocDate = "DocDate";
		public Type DocDate
		{
			set
			{
				_DocDate = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
        #region FinPeriodID
        protected string _FinPeriodID = "FinPeriodID";
        public Type FinPeriodID
        {
            set
            {
                _FinPeriodID = value.Name;
            }
            get
            {
                return null;
            }
        }
        #endregion
		#region CuryTranAmt
		protected abstract class curyTranAmt : IBqlField { }
		protected Type CuryTranAmt = typeof(curyTranAmt);
		protected string _CuryTranAmt
		{
			get
			{
				return CuryTranAmt.Name;
			}
		}
		#endregion
        #region GroupDiscountRate
        protected abstract class groupDiscountRate : IBqlField { }
        protected Type GroupDiscountRate = typeof(groupDiscountRate);
        protected string _GroupDiscountRate
        {
            get
            {
                return GroupDiscountRate.Name;
            }
        }
        #endregion
		#region TermsID
		protected string _TermsID = "TermsID";
		public Type TermsID
		{
			set
			{
				_TermsID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryID
		protected string _CuryID = "CuryID";
		public Type CuryID
		{
			set
			{
				_CuryID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryDocBal
		protected string _CuryDocBal = "CuryDocBal";
		public Type CuryDocBal
		{
			set
			{
				_CuryDocBal = (value != null) ? value.Name : null;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryTaxTotal
		protected string _CuryTaxTotal = "CuryTaxTotal";
		public Type CuryTaxTotal
		{
			set
			{
				_CuryTaxTotal = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryWhTaxTotal
		protected string _CuryWhTaxTotal = "CuryOrigWhTaxAmt";
		public Type CuryWhTaxTotal
		{
			set
			{
				_CuryWhTaxTotal = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryLineTotal
		protected abstract class curyLineTotal : IBqlField { }
		protected Type CuryLineTotal = typeof(curyLineTotal);
		protected string _CuryLineTotal
		{
			get
			{
				return CuryLineTotal.Name;
			}
		}
		#endregion
		#region TaxCalc
		protected TaxCalc _TaxCalc = TaxCalc.Calc;
		public TaxCalc TaxCalc
		{
			set
			{
				_TaxCalc = value;
			}
			get
			{
				return _TaxCalc;
			}
		}
		#endregion

		protected bool _NoSumTaxable = false;

		public static List<PXEventSubscriberAttribute> GetAttributes<Field, Target>(PXCache sender, object data)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			bool exactfind = false;

			List<PXEventSubscriberAttribute> res = sender.GetAttributes<Field>(data).FindAll((attr) =>
			{
                return (!exactfind || data == null) && ((exactfind = attr.GetType() == typeof(Target)) || attr is TaxAttribute && typeof(Target) == typeof(TaxAttribute));
			});

			res.Sort((a, b) =>
			{
				return ((IComparable)a).CompareTo(b);
			});

			return res;
		}

		public static void SetTaxCalc<Field>(PXCache cache, object data, TaxCalc isTaxCalc)
			where Field : IBqlField
		{
			SetTaxCalc<Field, TaxAttribute>(cache, data, isTaxCalc);
		}

		public static void SetTaxCalc<Field, Target>(PXCache cache, object data, TaxCalc isTaxCalc)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(cache, data))
			{
				((TaxAttribute)attr).TaxCalc = isTaxCalc;
			}
		}

		public static TaxCalc GetTaxCalc<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			return GetTaxCalc<Field, TaxAttribute>(cache, data);
		}

		public static TaxCalc GetTaxCalc<Field, Target>(PXCache cache, object data)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(cache, data))
			{
				if (((TaxAttribute)attr).TaxCalc != TaxCalc.NoCalc)
				{
					return TaxCalc.Calc;
				}
			}
			return TaxCalc.NoCalc;
		}

		public static void Calculate<Field>(PXCache sender, PXRowInsertedEventArgs e)
			where Field : IBqlField
		{
			Calculate<Field, TaxAttribute>(sender, e);
		}

		public static void Calculate<Field, Target>(PXCache sender, PXRowInsertedEventArgs e)
			where Field : IBqlField
			where Target : TaxAttribute
		{			
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(sender, e.Row))
			{
				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualLineCalc)
				{
					((TaxAttribute)attr).TaxCalc = TaxCalc.Calc;

					try
					{
						((IPXRowInsertedSubscriber)attr).RowInserted(sender, e);
					}
					finally
					{
						((TaxAttribute)attr).TaxCalc = TaxCalc.ManualLineCalc;
					}
				}

				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualCalc)
				{
					object copy;
					if (((TaxAttribute)attr).inserted.TryGetValue(e.Row, out copy))
					{
						((IPXRowUpdatedSubscriber)attr).RowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, copy, false));
						((TaxAttribute)attr).inserted.Remove(e.Row);

                        if (((TaxAttribute)attr).updated.TryGetValue(e.Row, out copy))
                        {
                            ((TaxAttribute)attr).updated.Remove(e.Row);
                        }
					}
				}

			}
		}

		public static void Calculate<Field>(PXCache sender, PXRowUpdatedEventArgs e)
			where Field : IBqlField
		{
			Calculate<Field, TaxAttribute>(sender, e);
		}

		public static void Calculate<Field, Target>(PXCache sender, PXRowUpdatedEventArgs e)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(sender, e.Row))
			{
				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualLineCalc)
				{
					((TaxAttribute)attr).TaxCalc = TaxCalc.Calc;

					try
					{
						((IPXRowUpdatedSubscriber)attr).RowUpdated(sender, e);
					}
					finally
					{
						((TaxAttribute)attr).TaxCalc = TaxCalc.ManualLineCalc;
					}
				}

				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualCalc)
				{
					object copy;
					if (((TaxAttribute)attr).updated.TryGetValue(e.Row, out copy))
					{
						((IPXRowUpdatedSubscriber)attr).RowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, copy, false));
						((TaxAttribute)attr).updated.Remove(e.Row);
					}
				}
			}
		}

		protected virtual string GetTaxZone(PXCache sender, object row)
		{
			return (string)ParentGetValue(sender.Graph, _TaxZoneID);
		}

		protected virtual DateTime? GetDocDate(PXCache sender, object row)
		{
			return (DateTime?)ParentGetValue(sender.Graph, _DocDate);
		}

		protected virtual string GetTaxCategory(PXCache sender, object row)
		{
			return (string)sender.GetValue(row, _TaxCategoryID);
		}

		protected virtual decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType = "I")
		{
			return (decimal?)sender.GetValue(row, _CuryTranAmt);
		}

		protected virtual string GetTaxID(PXCache sender, object row)
		{
			return (string)sender.GetValue(row, _TaxID);
		}

		protected virtual object InitializeTaxDet(object data)
		{
			return data;
		}

		protected virtual void AddOneTax(PXCache cache, object detrow, ITaxDetail taxitem)
		{
			if (taxitem != null)
			{
				object newdet;
				TaxParentAttribute.NewChild(cache, detrow, _ChildType, out newdet);
				((ITaxDetail)newdet).TaxID = taxitem.TaxID;
				newdet = InitializeTaxDet(newdet);
				object insdet = cache.Insert(newdet);

				AddTaxTotals(cache, newdet, detrow);
			}
		}

		public virtual ITaxDetail MatchesCategory(PXCache sender, object row, ITaxDetail zoneitem)
		{
			string taxcat = GetTaxCategory(sender, row);
			string taxid = GetTaxID(sender, row);
			DateTime? docdate = GetDocDate(sender, row);

			TaxRev rev = PXSelect<TaxRev, Where<TaxRev.taxID, Equal<Required<TaxRev.taxID>>, And<Required<TaxRev.startDate>, Between<TaxRev.startDate, TaxRev.endDate>, And<TaxRev.outdated, Equal<boolFalse>>>>>.Select(sender.Graph, zoneitem.TaxID, docdate);

			if (rev == null)
			{
				return null;
			}

			if (string.Equals(taxid, zoneitem.TaxID))
			{
				return zoneitem;
			}

			TaxCategory cat = (TaxCategory)PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(sender.Graph, taxcat);
			PXResultset<TaxCategoryDet> cattaxlist = PXSelect<TaxCategoryDet, Where<TaxCategoryDet.taxCategoryID, Equal<Required<TaxCategoryDet.taxCategoryID>>>>.Select(sender.Graph, taxcat);

			if (cat == null)
			{
				return null;
			}
			else
			{
				return MatchesCategory(sender, row, cat, cattaxlist.FirstTableItems, zoneitem);
			}

		}

		public IEnumerable<ITaxDetail> MatchesCategory(PXCache sender, object row, IEnumerable<ITaxDetail> zonetaxlist)
		{
			string taxcat = GetTaxCategory(sender, row);

			List<ITaxDetail> ret = new List<ITaxDetail>();

			TaxCategory cat = (TaxCategory)PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(sender.Graph, taxcat);

			if (cat == null)
			{
				return ret;
			}

			PXResultset<TaxCategoryDet> cattaxlist = PXSelect<TaxCategoryDet, Where<TaxCategoryDet.taxCategoryID, Equal<Required<TaxCategoryDet.taxCategoryID>>>>.Select(sender.Graph, taxcat);

			foreach (ITaxDetail zoneitem in zonetaxlist)
			{
				ret.Add(MatchesCategory(sender, row, cat, cattaxlist.FirstTableItems, zoneitem));
			}
			return ret;
		}

		public virtual ITaxDetail MatchesCategory(PXCache sender, object row, TaxCategory cat, IEnumerable<ITaxDetail> cattaxlist, ITaxDetail zoneitem)
		{
			bool zonematchestaxcat = false;
			foreach (ITaxDetail catitem in cattaxlist)
			{
				if (object.Equals(catitem.TaxID, zoneitem.TaxID))
				{
					zonematchestaxcat = true;
					break;
				}
			}
			if (cat.TaxCatFlag == false && zonematchestaxcat || cat.TaxCatFlag == true && !zonematchestaxcat)
			{
				return zoneitem;
			}
			return null;
		}

		protected abstract IEnumerable<ITaxDetail> ManualTaxes(PXCache sender, object row);

		protected virtual void DefaultTaxes(PXCache sender, object row, bool DefaultExisting)
		{
			PXCache cache = sender.Graph.Caches[_TaxType];
			string taxzone = GetTaxZone(sender, row);
			string taxcat = GetTaxCategory(sender, row);
			DateTime? docdate = GetDocDate(sender, row);

			foreach (PXResult<TaxZoneDet, TaxCategory, TaxRev, TaxCategoryDet> r in PXSelectJoin<TaxZoneDet,
				CrossJoin<TaxCategory,
				InnerJoin<TaxRev, On<TaxRev.taxID, Equal<TaxZoneDet.taxID>>,
				LeftJoin<TaxCategoryDet, On<TaxCategoryDet.taxID, Equal<TaxZoneDet.taxID>,
					And<TaxCategoryDet.taxCategoryID, Equal<TaxCategory.taxCategoryID>>>>>>,
				Where<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>,
					And<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>,
					And<Required<TaxRev.startDate>, Between<TaxRev.startDate, TaxRev.endDate>, And<TaxRev.outdated, Equal<boolFalse>,
					And<Where<TaxCategory.taxCatFlag, Equal<boolFalse>, And<TaxCategoryDet.taxCategoryID, IsNotNull,
						Or<TaxCategory.taxCatFlag, Equal<boolTrue>, And<TaxCategoryDet.taxCategoryID, IsNull>>>>>>>>>>.Select(sender.Graph, taxzone, taxcat, docdate))
			{
				AddOneTax(cache, row, (TaxZoneDet)r);
			}

			string taxID;
			if ((taxID = GetTaxID(sender, row)) != null)
			{
				AddOneTax(cache, row, new TaxZoneDet() { TaxID = taxID });
			}

			if (DefaultExisting)
			{
				foreach (ITaxDetail r in MatchesCategory(sender, row, ManualTaxes(sender, row)))
				{
					AddOneTax(cache, row, r);
				}
			}
		}

		protected virtual void DefaultTaxes(PXCache sender, object row)
		{
			DefaultTaxes(sender, row, true);
		}

		private Type GetFieldType(PXCache cache, string FieldName)
		{
			List<Type> fields = cache.BqlFields;
			for (int i = 0; i < fields.Count; i++)
			{
				if (String.Compare(fields[i].Name, FieldName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return fields[i];
				}
			}
			return null;
		}

		private Type GetTaxIDType(PXCache cache)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXSelectorAttribute)
				{
					if (((PXSelectorAttribute)attr).Field == typeof(Tax.taxID))
					{
						return GetFieldType(cache, ((PXSelectorAttribute)attr).FieldName);
					}
				}
			}
			return null;
		}

		private Type AddWhere(Type command, Type where)
		{
			if (command.IsGenericType)
			{
				Type[] args = command.GetGenericArguments();
				Type[] pars = new Type[args.Length + 1];
				pars[0] = command.GetGenericTypeDefinition();
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].IsGenericType && (
						args[i].GetGenericTypeDefinition() == typeof(Where<,>) ||
						args[i].GetGenericTypeDefinition() == typeof(Where2<,>) ||
						args[i].GetGenericTypeDefinition() == typeof(Where<,,>)))
					{
						pars[i + 1] = typeof(Where2<,>).MakeGenericType(args[i], typeof(And<>).MakeGenericType(where));
					}
					else
					{
						pars[i + 1] = args[i];
					}
				}
				return BqlCommand.Compose(pars);
			}
			return null;
		}

		protected List<object> SelectTaxes(PXCache sender, object row, PXTaxCheck taxchk)
		{
			return SelectTaxes<Where<boolTrue, Equal<boolTrue>>>(sender.Graph, row, taxchk);
		}

		protected abstract List<Object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
			where Where : IBqlWhere, new();

		protected virtual void ClearTaxes(PXCache sender, object row)
		{
			PXCache cache = sender.Graph.Caches[_TaxType];
			foreach (object taxrow in SelectTaxes(sender, row, PXTaxCheck.Line))
			{
				cache.Delete(((PXResult)taxrow)[0]);
			}
		}

		private decimal Sum(PXGraph graph, List<Object> list, Type field)
		{
			decimal ret = 0.0m;
			list.ForEach(new Action<Object>(delegate(object a)
			{
				decimal? val = (decimal?)graph.Caches[BqlCommand.GetItemType(field)].GetValue(((PXResult)a)[BqlCommand.GetItemType(field)], field.Name);
				ret += (val ?? 0m);
			}
			));

			return ret;
		}

		protected virtual void AddTaxTotals(PXCache sender, object taxrow, object row)
		{
			PXCache cache = sender.Graph.Caches[_TaxSumType];

			object newdet = Activator.CreateInstance(_TaxSumType);
			((TaxDetail)newdet).TaxID = ((TaxDetail)taxrow).TaxID;
			newdet = InitializeTaxDet(newdet); 
			object insdet = cache.Insert(newdet);
		}

		protected Terms SelectTerms(PXGraph graph)
		{
			string TermsID = (string)ParentGetValue(graph, _TermsID);
			Terms ret = TermsAttribute.SelectTerms(graph, TermsID);

			if (ret == null)
			{
				ret = new Terms();
			}

			return ret;
		}

        protected virtual void SetTaxableAmt(PXCache sender, object row, decimal? value)
        {
        }

        protected virtual void SetTaxAmt(PXCache sender, object row, decimal? value)
        {
        }

		private void TaxSetLineDefault(PXCache sender, object taxrow, object row)
		{
			if (taxrow == null)
			{
				throw new PXArgumentException("taxrow", ErrorMessages.ArgumentNullException);
			}

			PXCache cache = sender.Graph.Caches[_TaxType];

			TaxDetail taxdet = (TaxDetail)((PXResult)taxrow)[0];
			Tax tax = PXResult.Unwrap<Tax>(taxrow);
			TaxRev taxrev = PXResult.Unwrap<TaxRev>(taxrow);

            decimal CuryTranAmt = (decimal)GetCuryTranAmt(sender, row, tax.TaxCalcType);

			if (taxrev.TaxID == null)
			{
				taxrev.TaxableMin = 0m;
				taxrev.TaxableMax = 0m;
				taxrev.TaxRate = 0m;
			}

			Terms terms = SelectTerms(sender.Graph);

			List<object> incltaxes = SelectTaxes<Where<Tax.taxCalcLevel, Equal<string0>, And<Tax.taxType, NotEqual<CSTaxType.withholding>, And<Tax.directTax, Equal<False>>>>>(sender.Graph, row, PXTaxCheck.Line);

			decimal InclRate = Sum(sender.Graph,
					incltaxes,
					typeof(TaxRev.taxRate));

			decimal CuryInclTaxAmt = Sum(sender.Graph,
					incltaxes,
					GetFieldType(cache, _CuryTaxAmt));

			decimal CuryTaxableAmt = 0.0m;
			decimal TaxableAmt = 0.0m;
			decimal CuryTaxAmt = 0.0m;

			switch (tax.TaxCalcLevel)
			{
				case "0":
					CuryTaxableAmt = CuryTranAmt / (1 + InclRate / 100);
					CuryTaxAmt = PXDBCurrencyAttribute.RoundCury(cache, taxdet, CuryTaxableAmt * (decimal)taxrev.TaxRate / 100);
					CuryInclTaxAmt = 0m;

					incltaxes.ForEach(delegate(object a)
					{
						decimal? TaxRate = (decimal?)sender.Graph.Caches[typeof(TaxRev)].GetValue<TaxRev.taxRate>(((PXResult)a)[typeof(TaxRev)]);

						CuryInclTaxAmt += PXDBCurrencyAttribute.RoundCury(cache, taxdet, (CuryTaxableAmt * TaxRate / 100m) ?? 0m);
					});

					CuryTaxableAmt = CuryTranAmt - CuryInclTaxAmt;

                    SetTaxableAmt(sender, row, CuryTaxableAmt);
                    SetTaxAmt(sender, row, CuryInclTaxAmt);
					break;
				case "1":
					CuryTaxableAmt = CuryTranAmt - CuryInclTaxAmt;
					break;
				case "2":
					decimal CuryLevel1TaxAmt = Sum(sender.Graph,
							SelectTaxes<Where<Tax.taxCalcLevel, Equal<string1>, And<Tax.taxCalcLevel2Exclude, Equal<boolFalse>>>>(sender.Graph, row, PXTaxCheck.Line),
							GetFieldType(cache, _CuryTaxAmt));

					CuryTaxableAmt = CuryTranAmt - CuryInclTaxAmt + CuryLevel1TaxAmt;
					break;
			}

			if ((tax.TaxCalcLevel == "1" || tax.TaxCalcLevel == "2") && tax.TaxApplyTermsDisc == "X")
			{
				CuryTaxableAmt = CuryTaxableAmt * (1 - (decimal)(terms.DiscPercent ?? 0m) / 100);
			}

			if (tax.TaxCalcType == "I" && (tax.TaxCalcLevel == "1" || tax.TaxCalcLevel == "2"))
			{
				PXDBCurrencyAttribute.CuryConvBase(cache, taxdet, CuryTaxableAmt, out TaxableAmt);

				if (taxrev.TaxableMin != 0.0m)
				{
					if (TaxableAmt < (decimal)taxrev.TaxableMin)
					{
						CuryTaxableAmt = 0.0m;
						TaxableAmt = 0.0m;
					}
				}

				if (taxrev.TaxableMax != 0.0m)
				{
					if (TaxableAmt > (decimal)taxrev.TaxableMax)
					{
						PXDBCurrencyAttribute.CuryConvCury(cache, taxdet, (decimal)taxrev.TaxableMax, out CuryTaxableAmt);
						TaxableAmt = (decimal)taxrev.TaxableMax;
					}
				}

				CuryTaxAmt = CuryTaxableAmt * (decimal)taxrev.TaxRate / 100;

				if (tax.TaxApplyTermsDisc == "T")
				{
					CuryTaxAmt = CuryTaxAmt * (1 - (decimal)(terms.DiscPercent ?? 0m) / 100);
				}

			}
			else if (tax.TaxCalcType != "I")
			{
				CuryTaxAmt = 0.0m;
			}

			taxdet.TaxRate = taxrev.TaxRate;
			taxdet.CuryTaxableAmt = PXDBCurrencyAttribute.RoundCury(cache, taxdet, CuryTaxableAmt);
			taxdet.CuryTaxAmt = PXDBCurrencyAttribute.RoundCury(cache, taxdet, CuryTaxAmt);

			if (taxrev.TaxID != null && tax.DirectTax != true)
			{
				cache.Update(taxdet);
			}
			else
			{
				cache.Delete(taxdet);
			}
		}

		protected virtual void AdjustTaxableAmount(PXCache cache, object row, List<object> taxitems, ref decimal CuryTaxableAmt)
		{
		}

		//row is not needed actually, selecttaxes recalcline & recalctotals do not use it, can be passed as null
		private TaxDetail TaxSummarize(PXCache sender, object taxrow, object row)
		{
			if (taxrow == null)
			{
				throw new PXArgumentException("taxrow", ErrorMessages.ArgumentNullException);
			}

			PXCache cache = sender.Graph.Caches[_TaxType];
			PXCache sumcache = sender.Graph.Caches[_TaxSumType];

			TaxDetail taxdet = (TaxDetail)((PXResult)taxrow)[0];
			Tax tax = PXResult.Unwrap<Tax>(taxrow);
			TaxRev taxrev = PXResult.Unwrap<TaxRev>(taxrow);

			if (taxrev.TaxID == null)
			{
				taxrev.TaxableMin = 0m;
				taxrev.TaxableMax = 0m;
				taxrev.TaxRate = 0m;
			}

			Terms terms = SelectTerms(sender.Graph);

			decimal CuryTaxableAmt = 0.0m;
			decimal TaxableAmt = 0.0m;
			decimal CuryTaxAmt = 0.0m;
			decimal CuryLevel1TaxAmt = 0.0m;

			List<object> taxitems = SelectTaxes<Where<Tax.taxID, Equal<Required<Tax.taxID>>>>(sender.Graph, row, PXTaxCheck.RecalcLine, taxdet.TaxID);
			if (tax.TaxCalcType == "I")
			{
				//SWUIFieldAttribute.SetEnabled(cache, taxdet, _CuryTaxableAmt, false);
				//SWUIFieldAttribute.SetEnabled(cache, taxdet, _CuryTaxAmt, true);
				CuryTaxableAmt = Sum(sender.Graph,
						taxitems,
						GetFieldType(cache, _CuryTaxableAmt));

				CuryTaxAmt = Sum(sender.Graph,
						taxitems,
						GetFieldType(cache, _CuryTaxAmt));
			}
			else
			{
				List<object> level1taxes = SelectTaxes<Where<Tax.taxCalcLevel, Equal<string1>, And<Tax.taxCalcLevel2Exclude, Equal<string0>>>>(sender.Graph, row, PXTaxCheck.RecalcTotals);

				if (_NoSumTaxable && (tax.TaxCalcLevel == "1" || level1taxes.Count == 0))
				{
					//when changing doc date will not recalc taxable amount
					CuryTaxableAmt = (decimal)taxdet.CuryTaxableAmt;
				}
				else
				{
				CuryTaxableAmt = Sum(sender.Graph,
						taxitems,
						GetFieldType(cache, _CuryTaxableAmt));

				AdjustTaxableAmount(sender, row, taxitems, ref CuryTaxableAmt);

				if (tax.TaxCalcLevel == "2")
				{
						CuryLevel1TaxAmt = Sum(sender.Graph, level1taxes, GetFieldType(sumcache, _CuryTaxAmt));
					CuryTaxableAmt += CuryLevel1TaxAmt;
				}
				}

				PXDBCurrencyAttribute.CuryConvBase(sumcache, taxdet, CuryTaxableAmt, out TaxableAmt);

				if (taxrev.TaxableMin != 0.0m)
				{
					if (TaxableAmt < (decimal)taxrev.TaxableMin)
					{
						CuryTaxableAmt = 0.0m;
						TaxableAmt = 0.0m;
					}
				}

				if (taxrev.TaxableMax != 0.0m)
				{
					if (TaxableAmt > (decimal)taxrev.TaxableMax)
					{
						PXDBCurrencyAttribute.CuryConvCury(sumcache, taxdet, (decimal)taxrev.TaxableMax, out CuryTaxableAmt);
						TaxableAmt = (decimal)taxrev.TaxableMax;
					}
				}

				CuryTaxAmt = CuryTaxableAmt * (decimal)taxrev.TaxRate / 100;

				if ((tax.TaxCalcLevel == "1" || tax.TaxCalcLevel == "2") && tax.TaxApplyTermsDisc == "T")
				{
					CuryTaxAmt = CuryTaxAmt * (1 - (decimal)(terms.DiscPercent ?? 0m) / 100);
				}
			}

            if (taxitems.Count > 0 && taxrev.TaxID != null)
            {
                taxdet = (TaxDetail)sumcache.CreateCopy(taxdet);
                taxdet.TaxRate = taxrev.TaxRate;
                taxdet.CuryTaxableAmt = PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxableAmt);
				if (tax.DeductibleVAT == true)
				{
					taxdet.NonDeductibleTaxRate = taxrev.NonDeductibleTaxRate;
					taxdet.CuryExpenseAmt = PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxAmt * (1 - (taxrev.NonDeductibleTaxRate ?? 0m) / 100));
					taxdet.CuryTaxAmt = PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxAmt * ((taxrev.NonDeductibleTaxRate ?? 0m) / 100));	
				}
				else
				{
					taxdet.CuryTaxAmt = PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxAmt);
					taxdet.NonDeductibleTaxRate = 100;
				}

                return (TaxDetail)sumcache.Update(taxdet);
            }
			else
			{
				sumcache.Delete(taxdet);
				return null;
			}
		}

		protected virtual void CalcTaxes(PXCache sender, object row)
		{
			CalcTaxes(sender, row, PXTaxCheck.RecalcLine);
		}

		protected virtual void CalcTaxes(PXCache sender, object row, PXTaxCheck taxchk)
		{
			PXCache cache = sender.Graph.Caches[_TaxType];

			foreach (object taxrow in SelectTaxes(sender, row, taxchk))
			{
				object detrow;
				if ((detrow = TaxParentAttribute.ParentSelect(cache, ((PXResult)taxrow)[0], _ChildType)) != null)
				{
					if (sender.ObjectsEqual(row, detrow))
					{
						detrow = row;
					}

					TaxSetLineDefault(sender, taxrow, detrow);
				}
			}

			CalcTotals(sender, row, true);
		}

		protected virtual void CalcDocTotals(PXCache sender, object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
		{
			_CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);
		}

		protected virtual decimal CalcLineTotal(PXCache sender, object row)
		{
			decimal CuryLineTotal = 0m;

			object[] details = PXParentAttribute.SelectSiblings(sender, null);

			if (details != null)
			{
				foreach (object detrow in details)
				{
					CuryLineTotal += GetCuryTranAmt(sender, sender.ObjectsEqual(detrow, row) ? row : detrow) ?? 0m;
				}
			}
			return CuryLineTotal;
		}

		protected virtual void _CalcDocTotals(PXCache sender, object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
		{
			decimal CuryLineTotal = CalcLineTotal(sender, row);

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


		protected virtual void CalcTotals(PXCache sender, object row, bool CalcTaxes)
		{
			bool IsUseTax = false;

			decimal CuryTaxTotal = 0m;
			decimal CuryInclTaxTotal = 0m;
			decimal CuryWhTaxTotal = 0m;

			foreach (object taxrow in SelectTaxes(sender, row, PXTaxCheck.RecalcTotals))
			{
				TaxDetail taxdet = null;
				if (CalcTaxes)
				{
					taxdet = TaxSummarize(sender, taxrow, row);
				}
				else
				{
					taxdet = (TaxDetail)((PXResult)taxrow)[0];
				}

				if (taxdet != null && PXResult.Unwrap<Tax>(taxrow).TaxType == CSTaxType.Use)
				{
					IsUseTax = true;
				}
				else if (taxdet != null && PXResult.Unwrap<Tax>(taxrow).TaxCalcLevel == "0")
				{
					decimal CuryTaxAmt = (decimal)taxdet.CuryTaxAmt;
					if (PXResult.Unwrap<Tax>(taxrow).TaxType == CSTaxType.Withholding)
					{
						CuryWhTaxTotal += CuryTaxAmt;
					}
					CuryInclTaxTotal += CuryTaxAmt;
					CuryTaxTotal += CuryTaxAmt;
					if (PXResult.Unwrap<Tax>(taxrow).DeductibleVAT == true)
					{
						CuryTaxTotal += (decimal) taxdet.CuryExpenseAmt;
                        CuryInclTaxTotal += (decimal)taxdet.CuryExpenseAmt;
					}
				}
				else if (taxdet != null && PXResult.Unwrap<Tax>(taxrow).TaxCalcLevel != "0" && PXResult.Unwrap<Tax>(taxrow).ReverseTax == false)
				{
					decimal CuryTaxAmt = (decimal)taxdet.CuryTaxAmt;
					CuryTaxTotal += CuryTaxAmt;
					if (PXResult.Unwrap<Tax>(taxrow).DeductibleVAT == true)
					{
						CuryTaxTotal += (decimal)taxdet.CuryExpenseAmt;
					}
				}
				else if (taxdet != null && PXResult.Unwrap<Tax>(taxrow).TaxCalcLevel != "0" && PXResult.Unwrap<Tax>(taxrow).ReverseTax == true)
				{
					decimal CuryTaxAmt = (decimal)taxdet.CuryTaxAmt;
					CuryTaxTotal -= CuryTaxAmt;
					if (PXResult.Unwrap<Tax>(taxrow).DeductibleVAT == true)
					{
						CuryTaxTotal -= (decimal)taxdet.CuryExpenseAmt;
					}
				}

			}

			if (ParentGetStatus(sender.Graph) != PXEntryStatus.Deleted && ParentGetStatus(sender.Graph) != PXEntryStatus.InsertedDeleted)
			{
				CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);
			}

			if (IsUseTax)
			{
				ParentCache(sender.Graph).RaiseExceptionHandling(_CuryTaxTotal, ParentRow(sender.Graph), CuryTaxTotal, new PXSetPropertyException(Messages.UseTaxExcludedFromTotals, PXErrorLevel.Warning));
			}
		}

		protected virtual PXCache ParentCache(PXGraph graph)
		{
			return graph.Caches[_ParentType];
		}

		protected virtual object ParentRow(PXGraph graph)
		{
			if (_ParentRow == null)
			{
				return ParentCache(graph).Current;
			}
			else
			{
				return _ParentRow;
			}
		}

		protected virtual PXEntryStatus ParentGetStatus(PXGraph graph)
		{
			PXCache cache = ParentCache(graph);
			if (_ParentRow == null)
			{
				return cache.GetStatus(cache.Current);
			}
			else
			{
				return cache.GetStatus(_ParentRow);
			}
		}

		protected virtual void ParentSetValue(PXGraph graph, string fieldname, object value)
		{
			PXCache cache = ParentCache(graph); 
			if (_ParentRow == null)
			{
				object copy = cache.CreateCopy(cache.Current);
				cache.SetValueExt(cache.Current, fieldname, value);
				if (cache.GetStatus(cache.Current) == PXEntryStatus.Notchanged)
				{
					cache.SetStatus(cache.Current, PXEntryStatus.Updated);
				}
				cache.RaiseRowUpdated(cache.Current, copy);
			}
			else
			{
				cache.SetValueExt(_ParentRow, fieldname, value);
			}
		}

		protected virtual object ParentGetValue(PXGraph graph, string fieldname)
		{
			PXCache cache = ParentCache(graph);
			if (_ParentRow == null)
			{
				return cache.GetValue(cache.Current, fieldname);
			}
			else
			{
				return cache.GetValue(_ParentRow, fieldname);
			}
		}

		protected object ParentGetValue<Field>(PXGraph graph)
			where Field : IBqlField
		{
			return ParentGetValue(graph, typeof(Field).Name.ToLower());
		}

		protected void ParentSetValue<Field>(PXGraph graph, object value)
			where Field : IBqlField 
		{
			ParentSetValue(graph, typeof(Field).Name.ToLower(), value);
		}

		protected virtual bool CompareZone(PXGraph graph, string zoneA, string zoneB)
		{
			foreach (PXResult<TaxZoneDet> r in PXSelectGroupBy<TaxZoneDet, Where<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>, Or<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>>>, Aggregate<GroupBy<TaxZoneDet.taxID, Count>>>.Select(graph, zoneA, zoneB))
			{
				if (r.RowCount == 1)
				{
					return false;
				}
			}
			return true;
		}
		
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXRowInsertedSubscriber) ||
				typeof(ISubscriber) == typeof(IPXRowUpdatedSubscriber) ||
				typeof(ISubscriber) == typeof(IPXRowDeletedSubscriber))
			{
				subscribers.Add(this as ISubscriber);
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (_TaxCalc != TaxCalc.NoCalc && _TaxCalc != TaxCalc.ManualLineCalc)
			{
				for (int i = 0; i < _Attributes.Count; i++)
				{
					if (_Attributes[i] is IPXRowInsertedSubscriber)
					{
						((IPXRowInsertedSubscriber)_Attributes[i]).RowInserted(sender, e);
					}
				}

				object copy;
				if (!inserted.TryGetValue(e.Row, out copy))
				{
					inserted[e.Row] = sender.CreateCopy(e.Row);
				}
			}

			decimal? val;
			if (GetTaxCategory(sender, e.Row) == null && ((val = GetCuryTranAmt(sender, e.Row)) == null || val == 0m))
			{
				return;
			}

			if (_TaxCalc == TaxCalc.Calc)
			{
				Preload(sender);

				DefaultTaxes(sender, e.Row);
				CalcTaxes(sender, e.Row, PXTaxCheck.Line);
			}
			else if (_TaxCalc == TaxCalc.ManualCalc)
			{
				CalcTotals(sender, e.Row, true);
			}
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (_TaxCalc != TaxCalc.NoCalc && _TaxCalc != TaxCalc.ManualLineCalc)
			{
				for (int i = 0; i < _Attributes.Count; i++)
				{
					if (_Attributes[i] is IPXRowUpdatedSubscriber)
					{
						((IPXRowUpdatedSubscriber)_Attributes[i]).RowUpdated(sender, e);
					}
				}

				object copy;
				if (!updated.TryGetValue(e.Row, out copy))
				{
					updated[e.Row] = sender.CreateCopy(e.Row);
				}
			}

			if (_TaxCalc == TaxCalc.Calc)
			{
				if (!object.Equals(GetTaxCategory(sender, e.OldRow), GetTaxCategory(sender, e.Row)))
				{
					Preload(sender);

					ClearTaxes(sender, e.OldRow);
					DefaultTaxes(sender, e.Row);
				}
				else if (!object.Equals(GetTaxID(sender, e.OldRow), GetTaxID(sender, e.Row)))
				{
					PXCache cache = sender.Graph.Caches[_TaxType];
					DelOneTax(cache, e.Row, new TaxDetail() { TaxID = GetTaxID(sender, e.OldRow) });
					AddOneTax(cache, e.Row, new TaxZoneDet() { TaxID = GetTaxID(sender, e.Row) });
				}

				if (!object.Equals(GetTaxCategory(sender, e.OldRow), GetTaxCategory(sender, e.Row)) ||
					!object.Equals(GetCuryTranAmt(sender, e.OldRow), GetCuryTranAmt(sender, e.Row)) ||
					!object.Equals(GetTaxID(sender, e.OldRow), GetTaxID(sender, e.Row)))
				{
					CalcTaxes(sender, e.Row, PXTaxCheck.Line);
				}
			}
			else if (_TaxCalc == TaxCalc.ManualCalc)
			{
				CalcTotals(sender, e.Row, true);
			}
		}

		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (_TaxCalc != TaxCalc.NoCalc)
			{
				for (int i = 0; i < _Attributes.Count; i++)
				{
					if (_Attributes[i] is IPXRowDeletedSubscriber)
					{
						((IPXRowDeletedSubscriber)_Attributes[i]).RowDeleted(sender, e);
					}
				}
			}

			PXEntryStatus parentStatus = ParentGetStatus(sender.Graph);
			if (parentStatus == PXEntryStatus.Deleted || parentStatus == PXEntryStatus.InsertedDeleted) return;		

			decimal? val;
			if (GetTaxCategory(sender, e.Row) == null && ((val = GetCuryTranAmt(sender, e.Row)) == null || val == 0m))
			{
				return;
			}

			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				ClearTaxes(sender, e.Row);
				CalcTaxes(sender, null, PXTaxCheck.Line);
			}
			else if (_TaxCalc == TaxCalc.ManualCalc)
			{
				CalcTotals(sender, e.Row, true);
			}
		}

		protected object _ParentRow;

		protected virtual void CurrencyInfo_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				if (e.Row != null && ((CurrencyInfo)e.Row).CuryRate != null && (e.OldRow == null || !sender.ObjectsEqual<CurrencyInfo.curyRate, CurrencyInfo.curyMultDiv>(e.Row, e.OldRow)))
				{
					PXView siblings = CurrencyInfoAttribute.GetView(sender.Graph, _ChildType, _CuryKeyField);
					if (siblings != null && siblings.SelectSingle() != null)
					{
						CalcTaxes(siblings.Cache, null);
					}
				}
			}
 		}

		protected virtual void ParentFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				if (e.Row.GetType() == _ParentType)
				{
					_ParentRow = e.Row;
				}
				CalcTaxes(sender.Graph.Caches[_ChildType], null);
				_ParentRow = null;
			}
		}

		protected virtual void IsTaxSavedFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			decimal? curyTaxTotal = (decimal?) sender.GetValue(e.Row, _CuryTaxTotal);
			decimal? curyWhTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryWhTaxTotal);
			CalcDocTotals(sender, e.Row, curyTaxTotal.GetValueOrDefault(), 0, curyWhTaxTotal.GetValueOrDefault());
		}

        protected virtual List<object> ChildSelect(PXCache cache, object data)
        {
            return TaxParentAttribute.ChildSelect(cache, data, this._ParentType);
        }

		protected virtual void ZoneUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			TaxZone newTaxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID));
			TaxZone oldTaxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)e.OldValue);

			if ( oldTaxZone != null && oldTaxZone.IsExternal == true )
			{
				_TaxCalc = TaxCalc.Calc;
			}

			if (newTaxZone != null && newTaxZone.IsExternal == true)
			{
				_TaxCalc = TaxCalc.ManualCalc;
			}
			

			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				if (!CompareZone(sender.Graph, (string)e.OldValue, (string)sender.GetValue(e.Row, _TaxZoneID)))
				{
					Preload(sender);

					List<object> details = this.ChildSelect(cache, e.Row);
					foreach (object det in details)
					{
						ClearTaxes(cache, det);
						DefaultTaxes(cache, det, false);
					}

					_ParentRow = e.Row;
					CalcTaxes(cache, null);
					_ParentRow = null;
				}
			}
		}

		protected virtual void DateUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				Preload(sender);

				PXCache cache = sender.Graph.Caches[_ChildType];
				List<object> details = this.ChildSelect(cache, e.Row);
				foreach (object det in details)
				{
					ClearTaxes(cache, det);
					DefaultTaxes(cache, det, true);
				}
				_ParentRow = e.Row;
				_NoSumTaxable = true;
				try
				{
				CalcTaxes(cache, null);
				}
				finally
				{
				_ParentRow = null;
					_NoSumTaxable = false;
				}
			}
		}

        protected virtual void PeriodUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
            {
                PXCache cache = sender.Graph.Caches[_TaxSumType];
                List<object> details = TaxParentAttribute.ChildSelect(cache, e.Row, _ParentType);
                foreach (object det in details)
                {
                    if (cache.GetStatus(det) == PXEntryStatus.Notchanged || cache.GetStatus(det) == PXEntryStatus.Held)
                    {
                        cache.SetStatus(det, PXEntryStatus.Updated);
                    }
                }
            }
        }

		protected virtual void DelOneTax(PXCache sender, object detrow, object taxrow)
		{
			PXCache cache = sender.Graph.Caches[_ChildType];
			foreach (object taxdet in SelectTaxes(cache, detrow, PXTaxCheck.Line))
			{
				if (object.Equals(((TaxDetail)((PXResult)taxdet)[0]).TaxID, ((TaxDetail)taxrow).TaxID))
				{
					sender.Delete(((PXResult)taxdet)[0]);
				}
			}
		}

		protected virtual void Preload(PXCache sender)
		{
			SelectTaxes(sender, null, PXTaxCheck.RecalcTotals);
		}

		public override void CacheAttached(PXCache sender)
		{
			_ChildType = sender.GetItemType();

			inserted = new Dictionary<object, object>();
			updated = new Dictionary<object, object>();

			{
				PXCache cache = sender.Graph.Caches[_TaxType];
			}

			sender.Graph.FieldUpdated.AddHandler(_ParentType, _DocDate, DateUpdated);
            sender.Graph.FieldUpdated.AddHandler(_ParentType, _FinPeriodID, PeriodUpdated);
            sender.Graph.FieldUpdated.AddHandler(_ParentType, _TermsID, ParentFieldUpdated);
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _CuryID, ParentFieldUpdated);
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _IsTaxSaved, IsTaxSavedFieldUpdated);

			sender.Graph.FieldUpdated.AddHandler(_ParentType, _TaxZoneID, ZoneUpdated);

			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(null))
			{
				if (attr is CurrencyInfoAttribute)
				{
					_CuryKeyField = sender.GetBqlField(attr.FieldName);
					break;
				}
			}

			if (_CuryKeyField != null)
			{
				sender.Graph.RowUpdated.AddHandler<CurrencyInfo>(CurrencyInfo_RowUpdated);
			}

            PXUIFieldAttribute.SetVisible<Tax.exemptTax>(sender.Graph.Caches[typeof(Tax)], null, false);
            PXUIFieldAttribute.SetVisible<Tax.statisticalTax>(sender.Graph.Caches[typeof(Tax)], null, false);
            PXUIFieldAttribute.SetVisible<Tax.reverseTax>(sender.Graph.Caches[typeof(Tax)], null, false);
            PXUIFieldAttribute.SetVisible<Tax.pendingTax>(sender.Graph.Caches[typeof(Tax)], null, false);
            PXUIFieldAttribute.SetVisible<Tax.taxType>(sender.Graph.Caches[typeof(Tax)], null, false);            

		}

		public TaxBaseAttribute(Type ParentType, Type TaxType, Type TaxSumType)
		{
			_ParentType = ParentType;
			_TaxType = TaxType;
			_TaxSumType = TaxSumType;
		}


		public virtual int CompareTo(object other)
		{
			return 0;
		}
	}

	public class WhereExempt<TaxID> : BqlFormula<TaxID>, IBqlWhere
		where TaxID : IBqlOperand
	{
		IBqlCreator _whereExempt = new Where<Selector<TaxID, Tax.exemptTax>, Equal<True>, And<Selector<TaxID, Tax.statisticalTax>, Equal<False>, And<Selector<TaxID, Tax.reverseTax>, Equal<False>, And<Selector<TaxID, Tax.taxType>, Equal<CSTaxType.vat>>>>>();

		public override void Verify(PXCache cache, object item, System.Collections.Generic.List<object> pars, ref bool? result, ref object value)
		{
			_whereExempt.Verify(cache, item, pars, ref result, ref value);
		}
	}

	public class WhereTaxable<TaxID> : BqlFormula<TaxID>, IBqlWhere
		where TaxID : IBqlOperand
	{
		IBqlCreator _whereExempt = new Where<Selector<TaxID, Tax.exemptTax>, Equal<False>, And<Selector<TaxID, Tax.statisticalTax>, Equal<False>, And<Selector<TaxID, Tax.reverseTax>, Equal<False>, And<Selector<TaxID, Tax.taxType>, Equal<CSTaxType.vat>>>>>();

		public override void Verify(PXCache cache, object item, System.Collections.Generic.List<object> pars, ref bool? result, ref object value)
		{
			_whereExempt.Verify(cache, item, pars, ref result, ref value);
		}
	}

	[System.SerializableAttribute()]
	public class TaxDetail : ITaxDetail
	{		
		#region TaxID
		protected String _TaxID;
		public virtual String TaxID
		{
			get
			{
				return this._TaxID;
			}
			set
			{
				this._TaxID = value;
			}
		}
		#endregion
		#region TaxRate
		protected Decimal? _TaxRate;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? TaxRate
		{
			get
			{
				return this._TaxRate;
			}
			set
			{
				this._TaxRate = value;
			}
		}
		#endregion
		#region CuryInfoID
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[PXDefault()]
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
		#region CuryTaxableAmt
		protected Decimal? _CuryTaxableAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryTaxableAmt
		{
			get
			{
				return this._CuryTaxableAmt;
			}
			set
			{
				this._CuryTaxableAmt = value;
			}
		}
		#endregion
		#region TaxableAmt
		protected Decimal? _TaxableAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? TaxableAmt
		{
			get
			{
				return this._TaxableAmt;
			}
			set
			{
				this._TaxableAmt = value;
			}
		}
		#endregion
		#region CuryTaxAmt
		protected Decimal? _CuryTaxAmt;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryTaxAmt
		{
			get
			{
				return this._CuryTaxAmt;
			}
			set
			{
				this._CuryTaxAmt = value;
			}
		}
		#endregion
		#region TaxAmt
		protected Decimal? _TaxAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? TaxAmt
		{
			get
			{
				return this._TaxAmt;
			}
			set
			{
				this._TaxAmt = value;
			}
		}
		#endregion
		#region NonDeductibleTaxRate
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "100.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Deductible Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? NonDeductibleTaxRate { get; set; }
		#endregion
		#region ExpenseAmt
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? ExpenseAmt { get; set; }
		#endregion
		#region CuryExpenseAmt
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryExpenseAmt { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.IBqlField
		{
		}
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.IBqlField
		{
		}
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.IBqlField
		{
		}
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.IBqlField
		{
		}
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.IBqlField
		{
		}
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}

	public class VendorTaxPeriodType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { SemiMonthly, Monthly, BiMonthly, Quarterly, SemiAnnually, Yearly, FiscalPeriod },
				new string[] { Messages.SemiMonthly, Messages.Monthly, Messages.BiMonthly, Messages.Quarterly, Messages.SemiAnnually, Messages.Yearly, Messages.FiscalPeriod }) { }
		}

		public const string Monthly = "M";
		public const string SemiMonthly = "B";
		public const string Quarterly = "Q";
		public const string Yearly = "Y";
		public const string FiscalPeriod = "F";
        public const string BiMonthly = "E";
		public const string SemiAnnually = "H";

		public class monthly : Constant<string>
		{
			public monthly() : base(Monthly) { ;}
		}
		public class semiMonthly : Constant<string>
		{
			public semiMonthly() : base(SemiMonthly) { ;}
		}
		public class quarterly : Constant<string>
		{
			public quarterly() : base(Quarterly) { ;}
		}
		public class yearly : Constant<string>
		{
			public yearly() : base(Yearly) { ;}
		}
		public class fiscalPeriod : Constant<string>
		{
			public fiscalPeriod() : base(FiscalPeriod) { ;}
		}
        public class biMonthly : Constant<string>
        {
            public biMonthly() : base(BiMonthly) { ;}
        }
		public class semiAnnually : Constant<string>
		{
			public semiAnnually() : base(SemiAnnually) { ;}
		}
	}

	public static class TaxCalendar
	{
		public static TPeriod Create<TPeriod>(PXGraph graph, PXSelectBase<TaxYear> TaxYear_Select, PXSelectBase<TPeriod> TaxPeriod_Select, AP.Vendor vendor, DateTime? date)
			where TPeriod : TaxPeriod, new()
		{
			short? CalendarPeriods = null;

			switch (vendor.TaxPeriodType)
			{
				case VendorTaxPeriodType.Monthly:
					CalendarPeriods = 12;
					break;
				case VendorTaxPeriodType.SemiMonthly:
					CalendarPeriods = 24;
					break;
				case VendorTaxPeriodType.Quarterly:
					CalendarPeriods = 4;
					break;
				case VendorTaxPeriodType.Yearly:
					CalendarPeriods = 1;
					break;
                case VendorTaxPeriodType.BiMonthly:
                    CalendarPeriods = 6;
                    break;
				case VendorTaxPeriodType.SemiAnnually:
					CalendarPeriods = 2;
					break;
			}

			if (CalendarPeriods != null)
			{
				int year = ((DateTime)date).Year;
				int month = 1;

				TaxYear new_year = new TaxYear();
				new_year.VendorID = vendor.BAccountID;
				new_year.Year = year.ToString();
				TaxYear_Select.Cache.Insert(new_year);

				for (int i = 1; i < CalendarPeriods + 1; i++)
				{
					TPeriod new_per = new TPeriod();
					new_per.VendorID = new_year.VendorID;
					new_per.TaxYear = new_year.Year;
					new_per.TaxPeriodID = new_year.Year + (i < 10 ? "0" : "") + i.ToString();

					if (CalendarPeriods <= 12)
					{
						new_per.StartDate = new DateTime(year, month, 1);
						month += 12 / (int)CalendarPeriods;
						if (month > 12)
						{
							month = 1;
							year++;
						}
						new_per.EndDate = new DateTime(year, month, 1);
					}
					else if (CalendarPeriods == 24)
					{
						new_per.StartDate = (i % 2 == 1) ? new DateTime(year, month, 1) : new DateTime(year, month, 16);
						if (i % 2 == 0)
						{
							month++;
							if (month > 12)
							{
								month = 1;
								year++;
							}
						}
						new_per.EndDate = (i % 2 == 1) ? new DateTime(year, month, 16) : new DateTime(year, month, 1);
					}
					new_per = (TPeriod) TaxPeriod_Select.Cache.Insert(new_per);
				}
			}
			else
			{
				TaxYear new_year = null;
				foreach (PXResult<FinYear, FinPeriod> res in PXSelectJoin<FinYear, InnerJoin<FinPeriod, On<FinPeriod.finYear, Equal<FinYear.year>>>, Where<FinYear.startDate, LessEqual<Required<FinYear.startDate>>>, OrderBy<Desc<FinYear.year>>>.Select(graph, (object)date))
				{
					if (new_year == null)
					{
						new_year = (TaxYear)(FinYear)res;
						new_year.VendorID = vendor.BAccountID;
						TaxYear_Select.Cache.Insert(new_year);
					}
					else if (object.Equals(((TaxYear)(FinYear)res).Year, new_year.Year) == false)
					{
						break;
					}
					TaxPeriod new_per = (TaxPeriod)(FinPeriod)res;
					new_per.VendorID = vendor.BAccountID;
					
					if (typeof(TPeriod) == typeof(TaxPeriod))
					{
						TaxPeriod_Select.Cache.Insert(new_per);
					}
					else
					{
						TaxPeriod_Select.Cache.SetStatus((TPeriod)TaxPeriod_Select.Cache.Extend<TaxPeriod>(new_per), PXEntryStatus.Inserted);
					}
				}
			}

			return (TPeriod)TaxPeriod_Select.Select(vendor.BAccountID, date, date);
		}

		/// <summary>
		/// Returns FinPeriod form the given date.
		/// </summary>
		public static string GetPeriod(int vendorID, DateTime? fromdate)
		{
			using (PXDataRecord record = PXDatabase.SelectSingle<TaxPeriod>(
				new PXDataField(typeof(TaxPeriod.taxPeriodID).Name),
				new PXDataFieldValue(typeof(TaxPeriod.vendorID).Name, PXDbType.Int, vendorID),
				new PXDataFieldValue(typeof(TaxPeriod.startDate).Name, PXDbType.SmallDateTime, 4, fromdate, PXComp.LE),
				new PXDataFieldValue(typeof(TaxPeriod.endDate).Name, PXDbType.SmallDateTime, 4, fromdate, PXComp.GT)))
			{
				if (record != null)
				{
					return record.GetString(0);
				}
			}
			return null;
		}
	}

	public class TaxReportLineSelector : PXSelectorAttribute
	{		
		public TaxReportLineSelector(Type search)
			:base(search)
		{
			this.DescriptionField = typeof (TaxReportLine.descr);
			_UnconditionalSelect = BqlCommand.CreateInstance(typeof(Search<TaxReportLine.lineNbr, Where<TaxReportLine.vendorID, Equal<Current<TaxReportLine.vendorID>>, And<TaxReportLine.lineNbr, Equal<Required<TaxReportLine.lineNbr>>>>>));
			_CacheGlobal = false;
		}
	}
}
