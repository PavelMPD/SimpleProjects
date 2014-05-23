using System;
using PX.Data;
using System.Text;
using System.Collections.Generic;

namespace PX.Objects.FA
{
	public class AssetClassMaint : PXGraph<AssetClassMaint, FixedAsset>
	{
        #region DAC Overrides
        [PXDBString(1, IsFixed = true)]
        [PXDefault(FARecordType.ClassType)]
        [PXUIField(DisplayName = "Record Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [FARecordType.List]
        protected virtual void FixedAsset_RecordType_CacheAttached(PXCache sender)
        {
        }

		[PXDBInt()]
		[PXUIField(DisplayName = "Branch", Visibility = PXUIVisibility.Invisible, Enabled = false)]
		public virtual void FixedAsset_BranchID_CacheAttached(PXCache sender)
		{ 
		}

        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Asset Class ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<FixedAsset.assetCD, Where<FixedAsset.recordType, Equal<FARecordType.classType>>>),
            typeof(FixedAsset.assetCD),
            typeof(FixedAsset.description),
            typeof(FixedAsset.assetType),
            typeof(FixedAsset.usefulLife),
            Filterable = true)]
        protected virtual void FixedAsset_AssetCD_CacheAttached(PXCache sender)
        {
        }

        [PXDBInt()]
        [PXParent(typeof(Select<FixedAsset, Where<FixedAsset.assetID, Equal<Current<FixedAsset.parentAssetID>>>>), UseCurrent = true, LeaveChildren = true)]
        [PXSelector(typeof(Search<FixedAsset.assetID, Where<FixedAsset.assetID, NotEqual<Current<FixedAsset.assetID>>,
                                                     And<Where<FixedAsset.recordType, Equal<Current<FixedAsset.recordType>>,
                                                           And<Current<FixedAsset.recordType>, NotEqual<FARecordType.elementType>,
                                                           Or<Current<FixedAsset.recordType>, Equal<FARecordType.elementType>,
                                                           And<FixedAsset.recordType, Equal<FARecordType.assetType>>>>>>>>),
                    typeof(FixedAsset.assetCD), 
                    typeof(FixedAsset.description),
                    typeof(FixedAsset.assetType),
                    typeof(FixedAsset.usefulLife),
                    SubstituteKey = typeof(FixedAsset.assetCD),
                    DescriptionField = typeof(FixedAsset.description))]
        [PXUIField(DisplayName = "Parent Class", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void FixedAsset_ParentAssetID_CacheAttached(PXCache sender)
        {
        }

        [PXString(1, IsFixed = true)]
        protected virtual void FixedAsset_Status_CacheAttached(PXCache sender)
        {
        }

        [PXDefault]
        [SubAccountMask(DisplayName = "Combine Fixed Asset Sub. from")]
        protected virtual void FixedAsset_FASubMask_CacheAttached(PXCache sender)
        {
        }

        [PXDefault]
        [SubAccountMask(DisplayName = "Combine Accumulated Depreciation Sub. from")]
        protected virtual void FixedAsset_AccumDeprSubMask_CacheAttached(PXCache sender)
        {
        }

        [PXDefault]
        [SubAccountMask(DisplayName = "Combine Depreciation Expense Sub. from")]
        protected virtual void FixedAsset_DeprExpenceSubMask_CacheAttached(PXCache sender)
        {
        }

        [PXDefault]
        [SubAccountMask(DisplayName = "Combine Proceeds Sub. from")]
        protected virtual void FixedAsset_ProceedsSubMask_CacheAttached(PXCache sender)
        {
        }

        [PXDefault]
        [SubAccountMask(DisplayName = "Combine Gain/Loss Sub. from")]
        protected virtual void FixedAsset_GainLossSubMask_CacheAttached(PXCache sender)
        {
        }
        #endregion

		#region Selects Declaration

		public PXSelect<FixedAsset, Where<FixedAsset.recordType, Equal<FARecordType.classType>>> AssetClass;
		public PXSelect<FixedAsset, Where<FixedAsset.assetCD, Equal<Current<FixedAsset.assetCD>>>> CurrentAssetClass;
		public PXSelectJoin<FABookSettings,
			LeftJoin<FABook, On<FABook.bookID, Equal<FABookSettings.bookID>>>,
			Where<FABookSettings.assetID, Equal<Current<FixedAsset.assetID>>>> DepreciationSettings;
		public PXSetup<FASetup> FASetup;
		#endregion

		#region Ctor
		public AssetClassMaint()
		{
			PXCache cache = AssetClass.Cache;
			FASetup setup = FASetup.Current;

			PXUIFieldAttribute.SetRequired<FixedAsset.usefulLife>(cache, true);
		}
		#endregion

		#region FixedAsset Events
		protected virtual void FixedAsset_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			FixedAsset assetClass = (FixedAsset)e.Row;
			if (assetClass == null) return;
			
			if (assetClass.AssetCD != null)
			{
				
				foreach (FABook book in PXSelect<FABook>.Select(this))
				{
					FABookSettings settings = new FABookSettings{BookID = book.BookID};
					DepreciationSettings.Insert(settings);
				}

			DepreciationSettings.Cache.IsDirty = false;
			}
		}

		protected virtual void FixedAsset_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FixedAsset cls = (FixedAsset)e.Row;
			if (cls == null) return;

			PXUIFieldAttribute.SetEnabled<FixedAsset.accumDeprSubMask>(sender, cls, cls.UseFASubMask != true);

			bool? UpdateGL = null;
			foreach (FABookSettings books in DepreciationSettings.Select())
			{
				if ((UpdateGL = books.UpdateGL) == true)
				{
					break;
				}
			}

			PXUIFieldAttribute.SetEnabled<FixedAsset.holdEntry>(sender, e.Row, (UpdateGL != true));

			if (((FixedAsset)e.Row).HoldEntry == false && UpdateGL == true)
			{
				((FixedAsset)e.Row).HoldEntry = true;

				if (sender.GetStatus(e.Row) == PXEntryStatus.Notchanged)
				{
					sender.SetStatus(e.Row, PXEntryStatus.Updated);
					sender.IsDirty = true;
				}
			}

			if (cls.IsTangible == true)
			{
				PXStringListAttribute.SetList<FixedAsset.assetType>(sender, null,
															   new FixedAsset.assetType.TangibleListAttribute().AllowedValues,
															   new FixedAsset.assetType.TangibleListAttribute().AllowedLabels);
			}
			else
			{
				PXStringListAttribute.SetList<FixedAsset.assetType>(sender, null,
															   new FixedAsset.assetType.NonTangibleListAttribute().AllowedValues,
															   new FixedAsset.assetType.NonTangibleListAttribute().AllowedLabels);
			}

		}

		protected virtual void FixedAsset_RecordType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = FARecordType.ClassType;
			e.Cancel = true;
		}

		protected virtual void FixedAsset_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			FixedAsset header = (FixedAsset)e.Row;
			if (header == null) return;

			if ((header.UsefulLife ?? 0m) == 0m)
			{
				sender.RaiseExceptionHandling<FixedAsset.usefulLife>(header, header.UsefulLife, new PXSetPropertyException(Messages.ValueCanNotBeEmpty));
			}
			if (header.Active != true)
			{
				PXSelectBase<FixedAsset> selectStatement = new PXSelectJoin<FixedAsset,
										InnerJoin<FADetails, On<FixedAsset.assetID, Equal<FADetails.assetID>>>,
										Where<FixedAsset.classID, Equal<Required<FixedAsset.classID>>,
															And<Where<FADetails.status, Equal<FixedAssetStatus.active>,
																	Or<FADetails.status, Equal<FixedAssetStatus.underConstruction>>>>>>(this);

				PXResult<FixedAsset, FADetails> res = (PXResult<FixedAsset, FADetails>)selectStatement.View.SelectSingle(header.AssetID);
				if ((FixedAsset)res != null)
					sender.RaiseExceptionHandling<FixedAsset.active>(header, header.Active, new PXSetPropertyException(Messages.DeactivateImpossible));
			}
		}

		protected virtual void FixedAsset_UsefulLife_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FixedAsset header = (FixedAsset)e.Row;
			if (header == null) return;

		    foreach(FABookSettings set in PXSelect<FABookSettings, Where<FABookSettings.assetID, Equal<Required<FABookSettings.assetID>>>>.Select(this, header.AssetID))
			{
				FABookSettings newSettings = (FABookSettings) DepreciationSettings.Cache.CreateCopy(set);		
				newSettings.UsefulLife = header.UsefulLife;						
				DepreciationSettings.Update(newSettings);
			}
		}

		protected virtual void FixedAsset_FASubMask_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FixedAsset cls = (FixedAsset)e.Row;
			if (cls == null) return;

			if(cls.UseFASubMask == true)
			{
				cls.AccumDeprSubMask = cls.FASubMask;
			}
		}

		protected virtual void FixedAsset_UseFASubMask_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FixedAsset_FASubMask_FieldUpdated(sender, e);
		}

		protected virtual void FixedAsset_ParentAssetID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			FixedAsset asset = (FixedAsset)e.Row;
			if (asset == null) return;

			PXSelectBase<FixedAsset> cmd = new PXSelect<FixedAsset, Where<FixedAsset.assetID, Equal<Required<FixedAsset.parentAssetID>>>>(this);
			int? parentID = (int?)e.NewValue;
			string parentCD = null;
			while (parentID != null)
			{
				FixedAsset parent = cmd.Select(parentID);
				parentCD = parentCD ?? parent.AssetCD;
				if (parent.ParentAssetID == asset.AssetID)
				{
					e.NewValue = asset.ParentAssetID != null ? ((FixedAsset)cmd.Select(asset.ParentAssetID)).AssetCD : null;
					throw new PXSetPropertyException(Messages.CyclicParentRef, parentCD);
				}
				parentID = parent.ParentAssetID;
			}
		}

		#endregion
		#region FABookSettings Events

		protected virtual void FABookSettings_DepreciationMethodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<FABookSettings.averagingConvention>(e.Row);
		}

		protected virtual void FABookSettings_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FABookSettings set  = (FABookSettings)e.Row;
			if (set == null) return;
			
			bool depreciate = (set.Depreciate == true);
			PXUIFieldAttribute.SetEnabled<FABookSettings.bookID>(sender, set, set.BookID == null);
			PXUIFieldAttribute.SetEnabled<FABookSettings.averagingConvention>(sender, set, depreciate);
			PXUIFieldAttribute.SetEnabled<FABookSettings.depreciationMethodID>(sender, set, depreciate);
			PXUIFieldAttribute.SetEnabled<FABookSettings.recoveryPeriod>(sender, set, depreciate);			
			PXUIFieldAttribute.SetEnabled<FABookSettings.bonus>(sender, set, depreciate);
			PXUIFieldAttribute.SetEnabled<FABookSettings.sect179>(sender, set, depreciate);

			bool midMonthEnable = (set.AveragingConvention == FAAveragingConvention.HalfPeriod ||
			                       set.AveragingConvention == FAAveragingConvention.ModifiedPeriod ||
								   set.AveragingConvention == FAAveragingConvention.ModifiedPeriod2);

			PXUIFieldAttribute.SetEnabled<FABookSettings.midMonthDay>(sender, set, midMonthEnable);
			PXUIFieldAttribute.SetEnabled<FABookSettings.midMonthType>(sender, set, midMonthEnable);
		}
		
		protected virtual void FABookSettings_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			FixedAsset header = AssetClass.Current;
			FABookSettings set = (FABookSettings)e.Row;
			if (set == null || header == null) return;

			if (set.Depreciate == true)
			{
				if (set.AveragingConvention == null || string.IsNullOrEmpty(set.AveragingConvention.Trim()))
					sender.RaiseExceptionHandling<FABookSettings.averagingConvention>(set, set.AveragingConvention, new PXSetPropertyException(Messages.ValueCanNotBeEmpty));
				if (set.DepreciationMethodID == null)
					sender.RaiseExceptionHandling<FABookSettings.depreciationMethodID>(set, set.DepreciationMethodID, new PXSetPropertyException(Messages.ValueCanNotBeEmpty));
			}
        }

		#endregion
		#region Funcs

        public override void Persist()
        {
            foreach (FABookSettings set in this.DepreciationSettings.Cache.Inserted)
            {
                FABookPeriodSetup period = PXSelect<FABookPeriodSetup, Where<FABookPeriodSetup.bookID, Equal<Required<FABookPeriodSetup.bookID>>>>.SelectWindowed(this, 0, 1, set.BookID);
                if (period == null && set.UpdateGL == false)
                {
                    FABook book = PXSelect<FABook, Where<FABook.bookID, Equal<Required<FABook.bookID>>>>.SelectWindowed(this, 0, 1, set.BookID);
                    if (book != null)
                        this.DepreciationSettings.Cache.RaiseExceptionHandling<FABookSettings.bookID>(set, book.BookCode, new PXSetPropertyException<FABookSettings.bookID>(Messages.NoCalendarDefined));
                }            
            }
            foreach (FABookSettings set in this.DepreciationSettings.Cache.Updated)
            {
                FABookPeriodSetup period = PXSelect<FABookPeriodSetup, Where<FABookPeriodSetup.bookID, Equal<Required<FABookPeriodSetup.bookID>>>>.SelectWindowed(this, 0, 1, set.BookID);
                if (period == null && set.UpdateGL==false)
                {
                    FABook book = PXSelect<FABook, Where<FABook.bookID, Equal<Required<FABook.bookID>>>>.SelectWindowed(this, 0, 1, set.BookID);
                    if (book != null)
                        this.DepreciationSettings.Cache.RaiseExceptionHandling<FABookSettings.bookID>(set, book.BookCode, new PXSetPropertyException<FABookSettings.bookID>(Messages.NoCalendarDefined));
                }
            }
            base.Persist();
        }

		#endregion
	}

	public class CalcRecoveryPeriods<BookID, UsefulLife> : BqlFormula<BookID, UsefulLife>
		where BookID : IBqlOperand
		where UsefulLife : IBqlOperand

	{
		public override void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			int? bookID = (int?)Calculate<BookID>(cache, item);
			decimal? usefulLife = (decimal?)Calculate<UsefulLife>(cache, item);

            if (bookID == null || usefulLife == null)
			{
				value = null;
				return;
			}

			int depreciationPeriodsInYear = FABookPeriodIDAttribute.GetBookPeriodsInYear(cache.Graph, bookID);
			value = (int)Decimal.Ceiling((decimal)usefulLife * depreciationPeriodsInYear);
		}
	}
}
