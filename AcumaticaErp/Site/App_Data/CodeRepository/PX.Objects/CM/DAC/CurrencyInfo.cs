using PX.SM;

namespace PX.Objects.CM
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	
	[System.SerializableAttribute()]
	[ForceSaveInDash(false)]
    [PXCacheName(Messages.CurrencyInfo)]
	public partial class CurrencyInfo : PX.Data.IBqlTable
	{
		private CMSetup getCMSetup(PXCache cache)
		{
			CMSetup CMSetup = (CMSetup)cache.Graph.Caches[typeof(CMSetup)].Current;
			if (CMSetup == null)
			{
				CMSetup = PXSelectReadonly<CMSetup>.Select(cache.Graph);
			}
			return CMSetup;
		}
		private CurrencyRate getCuryRate(PXCache cache)
		{
			return PXSelectReadonly<CurrencyRate,
							Where<CurrencyRate.toCuryID, Equal<Required<CurrencyInfo.baseCuryID>>,
							And<CurrencyRate.fromCuryID, Equal<Required<CurrencyInfo.curyID>>,
							And<CurrencyRate.curyRateType, Equal<Required<CurrencyInfo.curyRateTypeID>>,
							And<CurrencyRate.curyEffDate, LessEqual<Required<CurrencyInfo.curyEffDate>>>>>>,
							OrderBy<Desc<CurrencyRate.curyEffDate>>>.SelectWindowed(cache.Graph, 0, 1, BaseCuryID, CuryID, CuryRateTypeID, CuryEffDate);
		}
		public virtual void SetCuryEffDate(PXCache sender, object value)
		{
			sender.SetValue<CurrencyInfo.curyEffDate>(this, value);
			defaultCuryRate(sender, false);
		}

		public virtual bool AllowUpdate(PXCache sender)
		{
			if (sender != null && !sender.AllowUpdate && !sender.AllowDelete) return false;
			return !(this.IsReadOnly == true || (this.CuryID == this.BaseCuryID));
		}

		private void SetDefaultEffDate(PXCache cache)
		{
			object newValue;
			if (cache.RaiseFieldDefaulting<CurrencyInfo.curyEffDate>(this, out newValue))
			{
				cache.RaiseFieldUpdating<CurrencyInfo.curyEffDate>(this, ref newValue);
			}
			this.CuryEffDate = (DateTime?)newValue;
		}

		private void defaultCuryRate(PXCache cache)
		{
			defaultCuryRate(cache, true);
		}
		private void defaultCuryRate(PXCache cache, bool ForceDefault)
		{
			CurrencyRate rate = getCuryRate(cache);
			if (rate != null)
			{
				DateTime? UserCuryEffDate = CuryEffDate;

				CuryEffDate = rate.CuryEffDate;
				CuryRate = Math.Round((decimal)rate.CuryRate, 8);
				CuryMultDiv = rate.CuryMultDiv;
				RecipRate = Math.Round((decimal)rate.RateReciprocal, 8);

				if (rate.CuryEffDate < UserCuryEffDate)
				{
					CurrencyRateType ratetype = (CurrencyRateType)PXSelectorAttribute.Select<CurrencyInfo.curyRateTypeID>(cache, this);
					if (ratetype != null && ratetype.RateEffDays > 0 && ((TimeSpan)(UserCuryEffDate - rate.CuryEffDate)).Days > ratetype.RateEffDays)
					{
						throw new PXRateIsNotDefinedForThisDateException(rate.CuryRateType, rate.FromCuryID, rate.ToCuryID, (DateTime)UserCuryEffDate);
					}
				}
			}
			else if (ForceDefault)
			{
				if (object.Equals(this._CuryID, this._BaseCuryID))
				{
					bool dirty = cache.IsDirty;
					CurrencyInfo dflt = new CurrencyInfo();
					cache.SetDefaultExt<CurrencyInfo.curyRate>(dflt);
					cache.SetDefaultExt<CurrencyInfo.curyMultDiv>(dflt);
					cache.SetDefaultExt<CurrencyInfo.recipRate>(dflt);
					CuryRate = Math.Round((decimal)dflt.CuryRate, 8);
					CuryMultDiv = dflt.CuryMultDiv;
					RecipRate = Math.Round((decimal)dflt.RecipRate, 8);
					cache.IsDirty = dirty;
				}
				else if (this._CuryRateTypeID == null || this._CuryEffDate == null)
				{
					this.CuryRate = null;
					this.RecipRate = null;
					this.CuryMultDiv = "M"; 
				}
				else
				{
					this.CuryRate = null;
					this.RecipRate = null;
					this.CuryMultDiv = "M";
					throw new PXSetPropertyException(Messages.RateNotFound, PXErrorLevel.Warning);
				}
			}
		}
		public virtual bool CheckRateVariance(PXCache cache)
		{
			CMSetup CMSetup = getCMSetup(cache);
			if (CMSetup != null && CMSetup.RateVarianceWarn == true && CMSetup.RateVariance != 0)
			{
				CurrencyRate rate = getCuryRate(cache);
				if (rate != null && rate.CuryRate != null && rate.CuryRate != 0)
				{
					decimal Variance;
					if (rate.CuryMultDiv == CuryMultDiv || CuryRate == 0)
					{
						Variance = 100 * ((decimal)CuryRate - (decimal)rate.CuryRate) / (decimal)rate.CuryRate;
					}
					else
					{
						Variance = 100 * (1 / (decimal)CuryRate - (decimal)rate.CuryRate) / (decimal)rate.CuryRate;
					}
					if (Math.Abs(Variance) > CMSetup.RateVariance)
					{
						return true;
					}
				}
			}
			return false;
		}
		private sealed class CuryRateTypeIDAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber, IPXFieldVerifyingSubscriber, IPXFieldUpdatedSubscriber
		{
			public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null && !String.IsNullOrEmpty(info.ModuleCode))
				{
					CMSetup CMSetup = info.getCMSetup(sender);
					if (CMSetup != null && CMSetup.MCActivated == true)
					{
						string rateType;
						switch (info.ModuleCode)
						{
							case "CA":
								rateType = CMSetup.CARateTypeDflt;
								break;
							case "AP":
								rateType = CMSetup.APRateTypeDflt;
								break;
							case "AR":
								rateType = CMSetup.ARRateTypeDflt;
								break;
							case "GL":
								rateType = CMSetup.GLRateTypeDflt;
								break;
							default:
								rateType = null;
								break;
						}
						if (!string.IsNullOrEmpty(rateType))
						{
							e.NewValue = rateType;
						}
					}
				}
			}
			public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null && !String.IsNullOrEmpty(info.ModuleCode))
				{
					CMSetup CMSetup = info.getCMSetup(sender);
					//if (CMSetup != null && CMSetup.MCActivated == true)
                    //{
                    //    if (info.ModuleCode == "AP" && CMSetup.APRateTypeOverride != true ||
                    //        info.ModuleCode == "AR" && CMSetup.ARRateTypeOverride != true)
                    //    {
                    //        object newValue;
                    //        sender.RaiseFieldDefaulting<CurrencyInfo.curyRateTypeID>(e.Row, out newValue);
                    //        if (!object.Equals(newValue, e.NewValue))
                    //        {
                    //            throw new PXSetPropertyException(Messages.RateTypeCannotBeChanged, "CuryRateTypeID");
                    //        }
                    //    }
                    //}
				}
			}
			public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null)
				{
					//reset effective date to document date first
					info.SetDefaultEffDate(sender);
					try
					{
						info.defaultCuryRate(sender);
					}
					catch (PXSetPropertyException ex)
					{
						if (e.ExternalCall)
						{
							sender.RaiseExceptionHandling(_FieldName, e.Row, sender.GetValue(e.Row, _FieldOrdinal), ex);
						}
					}
				}
			}
		}
		private sealed class CuryEffDateAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
		{
			public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null)
				{
					try
					{
						info.defaultCuryRate(sender);
					}
					catch (PXSetPropertyException ex)
					{
						sender.RaiseExceptionHandling(_FieldName, e.Row, sender.GetValue(e.Row, _FieldOrdinal), ex);
					}
				}
			}
		}
		private sealed class CuryIDAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber, IPXRowSelectedSubscriber, IPXRowUpdatingSubscriber, IPXRowUpdatedSubscriber, IPXFieldVerifyingSubscriber
		{
			public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null)
				{
					//reset effective date to document date first
					info.SetDefaultEffDate(sender);
					try
					{
						info.defaultCuryRate(sender);
					}
					catch (PXSetPropertyException ex)
					{
						sender.RaiseExceptionHandling(_FieldName, e.Row, sender.GetValue(e.Row, _FieldOrdinal), ex);
					}
					info.CuryPrecision = null;
				}
			}
			public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null)
				{
					bool disabled = info.IsReadOnly == true || (info.CuryID == info.BaseCuryID);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyMultDiv>(sender, info, !disabled);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleCuryRate>(sender, info, !disabled);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.sampleRecipRate>(sender, info, !disabled);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyRateTypeID>(sender, info, !disabled);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyEffDate>(sender, info, !disabled);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.baseCuryID>(sender, info, false);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.displayCuryID>(sender, info, false);
					PXUIFieldAttribute.SetEnabled<CurrencyInfo.curyID>(sender, info, true);
				}
			}
			private bool? currencyInfoDirty = null;
			public void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null && info._IsReadOnly == true)
				{
					e.Cancel = true;
				}
				else
				{
					currencyInfoDirty = sender.IsDirty;
				}
			}
			public void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null)
				{
					CurrencyInfo old = e.OldRow as CurrencyInfo;
					if (old != null && (String.IsNullOrEmpty(info.CuryID) || String.IsNullOrEmpty(info.BaseCuryID)))
					{
						info.BaseCuryID = old.BaseCuryID;
						info.CuryID = old.CuryID;
					}
					if (currencyInfoDirty == false
						&& info.CuryID == old.CuryID
						&& info.CuryRateTypeID == old.CuryRateTypeID
						&& info.CuryEffDate == old.CuryEffDate
						&& info.CuryMultDiv == old.CuryMultDiv
						&& info.CuryRate == old.CuryRate)
					{
						sender.IsDirty = false;
						currencyInfoDirty = null;
					}
				}
			}
			public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null)
				{
					CMSetup CMSetup = info.getCMSetup(sender);
					//if (CMSetup != null && (bool)CMSetup.MCActivated)
                    //{

                    //    if (info.ModuleCode == "AP" && CMSetup.APCuryOverride != true ||
                    //        info.ModuleCode == "AR" && CMSetup.ARCuryOverride != true)
                    //    {
                    //        object newValue;
                    //        sender.RaiseFieldDefaulting<CurrencyInfo.curyID>(e.Row, out newValue);
                    //        if (!object.Equals(newValue, e.NewValue))
                    //        {
                    //            throw new PXSetPropertyException(Messages.CuryIDCannotBeChanged, "CuryID");
                    //        }
                    //    }
                    //}
				}
			}
		}
		private sealed class CuryRateAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
		{
			public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null && e.ExternalCall)
				{
					decimal rate = Math.Round((decimal)info.SampleCuryRate, 8);
					if (rate == 0)
						rate = 1;
					info.CuryRate = rate;
					info.RecipRate = Math.Round((decimal)(1 / rate), 8);
					info.CuryMultDiv = "M";
					if (info.CheckRateVariance(sender))
					{
						PXUIFieldAttribute.SetWarning(sender, e.Row, "SampleCuryRate", Messages.RateVarianceExceeded);
					}
				}
			}
		}
		private sealed class CuryRecipRateAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
		{
			public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				CurrencyInfo info = e.Row as CurrencyInfo;
				if (info != null && e.ExternalCall)
				{
					decimal rate = Math.Round((decimal)info.SampleRecipRate, 8);
					if (rate == 0)
						rate = 1;
					info.CuryRate = rate;
					info.RecipRate = Math.Round((decimal)(1 / rate), 8);
					info.CuryMultDiv = "D";
					if (info.CheckRateVariance(sender))
					{
						PXUIFieldAttribute.SetWarning(sender, e.Row, "SampleRecipRate", Messages.RateVarianceExceeded);
					}
				}
			}
		}
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.IBqlField
		{
		}
		protected Int64? _CuryInfoID;
		[PXDBLongIdentity(IsKey = true)]
		[PXUIField(Visible = false)]
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
		#region ModuleCode
		protected string _ModuleCode;
		public virtual string ModuleCode
		{
			get
			{
				return this._ModuleCode;
			}
			set
			{
				this._ModuleCode = value;
			}
		}
		#endregion
		#region IsReadOnly
		protected bool? _IsReadOnly;
		public virtual bool? IsReadOnly
		{
			get
			{
				return this._IsReadOnly;
			}
			set
			{
				this._IsReadOnly = value;
			}
		}
		#endregion
		#region BaseCalc
		public abstract class baseCalc : IBqlField { }
		protected bool? _BaseCalc;
		[PXDBBool()]
		[PXDefault(true)]
		public virtual bool? BaseCalc
		{
			get
			{
				return this._BaseCalc;
			}
			set
			{
				this._BaseCalc = value;
			}
		}
		#endregion
		#region BaseCuryID
		public abstract class baseCuryID : PX.Data.IBqlField
		{
		}
		protected String _BaseCuryID;
		[PXDBString(5, IsUnicode = true)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXUIField(DisplayName = "Base Currency ID")]
		public virtual String BaseCuryID
		{
			get
			{
				return this._BaseCuryID;
			}
			set
			{
				this._BaseCuryID = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.IBqlField
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXUIField(DisplayName = "Currency", ErrorHandling = PXErrorHandling.Never)]
		[PXSelector(typeof(Currency.curyID))]
		[CuryID]
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
		#region DisplayCuryID
		public abstract class displayCuryID : PX.Data.IBqlField
		{
		}
		[PXString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency ID")]
		public virtual String DisplayCuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				;
			}
		}
		#endregion
		#region CuryRateTypeID
		public abstract class curyRateTypeID : PX.Data.IBqlField
		{
		}
		protected String _CuryRateTypeID;
		[PXDBString(6, IsUnicode = true)]
		[CuryRateTypeID()]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "Curr. Rate Type ID")]
		public virtual String CuryRateTypeID
		{
			get
			{
				return this._CuryRateTypeID;
			}
			set
			{
				this._CuryRateTypeID = value;
			}
		}
		#endregion
		#region CuryEffDate
		public abstract class curyEffDate : PX.Data.IBqlField
		{
		}
		protected DateTime? _CuryEffDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName="Effective Date")]
		[CuryEffDate()]
		public virtual DateTime? CuryEffDate
		{
			get
			{
				return this._CuryEffDate;
			}
			set
			{
				this._CuryEffDate = value;
			}
		}
		#endregion
		#region CuryMultDiv
		public abstract class curyMultDiv : PX.Data.IBqlField
		{
		}
		protected String _CuryMultDiv;
		[PXDBString(1, IsFixed = true)]
        [PXDefault("M", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName="Mult Div")]
		public virtual String CuryMultDiv
		{
			get
			{
				return this._CuryMultDiv;
			}
			set
			{
				this._CuryMultDiv = value;
			}
		}
		#endregion
		#region CuryRate
		public abstract class curyRate : PX.Data.IBqlField
		{
		}
		protected Decimal? _CuryRate;
		[PXDBDecimal(8)]
		[PXDefault(TypeCode.Decimal,"1.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? CuryRate
		{
			get
			{
				return this._CuryRate;
			}
			set
			{
				this._CuryRate = value;
			}
		}
		#endregion
		#region RecipRate
		public abstract class recipRate : PX.Data.IBqlField
		{
		}
		protected Decimal? _RecipRate;
		[PXDBDecimal(8)]
        [PXDefault(TypeCode.Decimal, "1.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? RecipRate
		{
			get
			{
				return this._RecipRate;
			}
			set
			{
				this._RecipRate = value;
			}
		}
		#endregion
		#region SampleCuryRate
		public abstract class sampleCuryRate : PX.Data.IBqlField
		{
		}
		[PXDecimal(8)]
		[PXUIField(DisplayName = "Curr. Rate")]
        [PXDefault()]
		[CuryRate()]
		public virtual Decimal? SampleCuryRate
		{
			[PXDependsOnFields(typeof(curyMultDiv),typeof(curyRate),typeof(recipRate))]
			get
			{
				return (this._CuryMultDiv == "M") ? this._CuryRate : this._RecipRate;
			}
			set
			{
				if (this.CuryMultDiv == "M")
				{
					this._CuryRate = value;
				}
				else
				{
					this._RecipRate = value;
				}
			}
		}
		#endregion
		#region SampleRecipRate
		public abstract class sampleRecipRate : PX.Data.IBqlField
		{
		}
		[PXDecimal(8)]
		[PXUIField(DisplayName = "Reciprocal Rate")]
		[CuryRecipRate()]
		public virtual Decimal? SampleRecipRate
		{
			[PXDependsOnFields(typeof(curyMultDiv),typeof(recipRate),typeof(curyRate))]
			get
			{
				return (this._CuryMultDiv == "M") ? this._RecipRate : this._CuryRate;
			}
			set
			{
				if (this.CuryMultDiv == "M")
				{
					this._RecipRate = value;
				}
				else
				{
					this._CuryRate = value;
				}
			}
		}
		#endregion
		#region CuryPrecision
		public abstract class curyPrecision : PX.Data.IBqlField
		{
		}
		protected short? _CuryPrecision;
		[PXDefault((short)2, typeof(Search<Currency.decimalPlaces, Where<Currency.curyID, Equal<Current<CurrencyInfo.curyID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual short? CuryPrecision
		{
			get
			{
				return _CuryPrecision;
			}
			set
			{
				_CuryPrecision = value;
			}
		}
		#endregion
		#region BasePrecision
		public abstract class basePrecision : PX.Data.IBqlField
		{
		}
		protected short? _BasePrecision;
		[PXDefault((short)2, typeof(Search<Currency.decimalPlaces, Where<Currency.curyID, Equal<Current<CurrencyInfo.baseCuryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual short? BasePrecision
		{
			get
			{
				return _BasePrecision;
			}
			set
			{
				_BasePrecision = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.IBqlField
		{
		}
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}
}
