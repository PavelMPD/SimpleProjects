using System.Globalization;
using PX.SM;
using PX.TM;
using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.GL;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FABookHist = PX.Objects.FA.Overrides.AssetProcess.FABookHist;

namespace PX.Objects.FA
{
	public class DepreciationCalculation : PXGraph<DepreciationCalculation, FADepreciationMethod>, IDepreciationCalculation
	{
		protected int _Precision	 = 2;
		protected int monthsInYear	 = 12;
		protected int quarterToMonth = 3;
		protected int quartersCount	 = 4;

		protected FADestination _Destination = FADestination.Calculated;

		public enum FADestination
		{
			Calculated,
			Tax179,
			Bonus
		}
		

		#region Selects Declaration

		public PXSelect<FADepreciationMethod> DepreciationMethod;
		public PXSelect<FADepreciationMethodLines, Where<FADepreciationMethodLines.methodID, Equal<Current<FADepreciationMethod.methodID>>>> 	DepreciationMethodLines;
		public PXSetup<FASetup> FASetup;
		public PXSelect<FABookHistory> RawHistory;
		public PXSelect<FABookHist> BookHistory;
		public PXSelect<FABookBalance> BookBalance;
		
		
		#endregion

		#region Constructor
		public DepreciationCalculation() 
		{
			FASetup setup = FASetup.Current;
			PXCache cache = DepreciationMethodLines.Cache;
			cache.AllowDelete = false;
			cache.AllowInsert = false;			
			PXCache methodCache = DepreciationMethod.Cache;
			PXDBCurrencyAttribute.SetBaseCalc<FADepreciationMethod.totalPercents> (methodCache, null, true);

			Currency cury = PXSelectJoin<Currency, InnerJoin<GL.Company, On<GL.Company.baseCuryID, Equal<Currency.curyID>>>>.Select(this);
			_Precision = cury.DecimalPlaces ?? 4;
		}

		public override void Clear()
		{
			PXDBBaseCuryAttribute.EnsurePrecision(this.Caches[typeof(FABookHist)]);
			base.Clear();
		}

		public decimal? Round(decimal? value)
		{
			return Math.Round((decimal)value, _Precision, MidpointRounding.AwayFromZero);
		}
		#endregion
		
		#region Buttons
			#region Button Create Lines
			public PXAction<FADepreciationMethod> CalculatePercents;
				[PXUIField(DisplayName = "Calculate Percents", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
				[PXProcessButton]
				public virtual IEnumerable calculatePercents(PXAdapter adapter)
				{
					FADepreciationMethod method = DepreciationMethod.Current;
					CalculateLinesPercents(method); 
					return adapter.Get();
				}
			#endregion
		#endregion

		#region Events Lines
			protected virtual void FADepreciationMethodLines_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod) DepreciationMethod.Current;
				FADepreciationMethodLines line = (FADepreciationMethodLines)e.Row;
				if (header == null || line == null) return;

				if (line.MethodID != header.MethodID && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update))
				{
					line.MethodID = header.MethodID;
				}
			}
			protected virtual void FADepreciationMethodLines_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod) DepreciationMethod.Current;
				FADepreciationMethodLines line = (FADepreciationMethodLines)e.Row;
				if (header == null || line == null) return;

				if (line.MethodID != header.MethodID && (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update))
				{
					line.MethodID = header.MethodID;
				}
			}
			protected virtual void FADepreciationMethodLines_RatioPerYear_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod) DepreciationMethod.Current;
				FADepreciationMethodLines line = (FADepreciationMethodLines)e.Row;
				if (header == null || line == null) return;
				SetMethodTotalPercents(header, sender.Locate(e.Row) == null ? line : null);
			}
			protected virtual void FADepreciationMethodLines_RatioPerMonth1_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod) DepreciationMethod.Current;
				FADepreciationMethodLines line = (FADepreciationMethodLines)e.Row;
				if (header == null || line == null) return;
				SetMethodTotalPercents(header, sender.Locate(e.Row) == null ? line : null);
			}
			protected virtual void FADepreciationMethodLines_RatioPerMonth2_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod) DepreciationMethod.Current;
				FADepreciationMethodLines line = (FADepreciationMethodLines)e.Row;
				if (header == null || line == null) return;
				SetMethodTotalPercents(header, sender.Locate(e.Row) == null ? line : null);
			}
			protected virtual void FADepreciationMethodLines_RatioPerMonth3_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod) DepreciationMethod.Current;
				FADepreciationMethodLines line = (FADepreciationMethodLines)e.Row;
				if (header == null || line == null) return;
				SetMethodTotalPercents(header, sender.Locate(e.Row) == null ? line : null);
			}
			protected virtual void FADepreciationMethodLines_RatioPerMonth4_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod) DepreciationMethod.Current;
				FADepreciationMethodLines line = (FADepreciationMethodLines)e.Row;
				if (header == null || line == null) return;
				SetMethodTotalPercents(header, sender.Locate(e.Row) == null ? line : null);
			}
			protected virtual void FADepreciationMethodLines_RatioPerMonth5_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod) DepreciationMethod.Current;
				FADepreciationMethodLines line = (FADepreciationMethodLines)e.Row;
				if (header == null || line == null) return;
				SetMethodTotalPercents(header, sender.Locate(e.Row) == null ? line : null);
			}
			protected virtual void FADepreciationMethodLines_RatioPerMonth6_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod) DepreciationMethod.Current;
				FADepreciationMethodLines line = (FADepreciationMethodLines)e.Row;
				if (header == null || line == null) return;
				SetMethodTotalPercents(header, sender.Locate(e.Row) == null ? line : null);
			}
			protected virtual void FADepreciationMethodLines_RatioPerMonth7_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod) DepreciationMethod.Current;
				FADepreciationMethodLines line = (FADepreciationMethodLines)e.Row;
				if (header == null || line == null) return;
				SetMethodTotalPercents(header, sender.Locate(e.Row) == null ? line : null);
			}
			protected virtual void FADepreciationMethodLines_RatioPerMonth8_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod) DepreciationMethod.Current;
				FADepreciationMethodLines line = (FADepreciationMethodLines)e.Row;
				if (header == null || line == null) return;
				SetMethodTotalPercents(header, sender.Locate(e.Row) == null ? line : null);
			}
			protected virtual void FADepreciationMethodLines_RatioPerMonth9_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod) DepreciationMethod.Current;
				FADepreciationMethodLines line = (FADepreciationMethodLines)e.Row;
				if (header == null || line == null) return;
				SetMethodTotalPercents(header, sender.Locate(e.Row) == null ? line : null);
			}
			protected virtual void FADepreciationMethodLines_RatioPerMonth10_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod) DepreciationMethod.Current;
				FADepreciationMethodLines line = (FADepreciationMethodLines)e.Row;
				if (header == null || line == null) return;
				SetMethodTotalPercents(header, sender.Locate(e.Row) == null ? line : null);
			}
			protected virtual void FADepreciationMethodLines_RatioPerMonth11_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod) DepreciationMethod.Current;
				FADepreciationMethodLines line = (FADepreciationMethodLines)e.Row;
				if (header == null || line == null) return;
				SetMethodTotalPercents(header, sender.Locate(e.Row) == null ? line : null);
			}
			protected virtual void FADepreciationMethodLines_RatioPerMonth12_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod) DepreciationMethod.Current;
				FADepreciationMethodLines line = (FADepreciationMethodLines)e.Row;
				if (header == null || line == null) return;
				SetMethodTotalPercents(header, sender.Locate(e.Row) == null ? line : null);
			}
		#endregion
		
		#region Events Headers
			protected virtual void FADepreciationMethod_RecoveryPeriod_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod)e.Row;
				if (header == null) return;
				ClearMethodLines(header);
				CreateMethodLines(header);
			}
			protected virtual void FADepreciationMethod_DepreciationStartDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod)e.Row;
				if (header == null) return;
				ClearMethodLines(header);
				CreateMethodLines(header);
			}
			protected virtual void FADepreciationMethod_AveragingConvention_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod)e.Row;
				if (header == null) return;
				ClearMethodLines(header);
				CreateMethodLines(header);
			}
			protected virtual void FADepreciationMethod_DepreciationStopDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod)e.Row;
				if (header == null) return;
				ClearMethodLines(header);
				CreateMethodLines(header);
			}
			protected virtual void FADepreciationMethod_DepreciationPeriodsInYear_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod)e.Row;
				if (header == null) return;
				ClearMethodLines(header);
			}
			protected virtual void FADepreciationMethod_YearlyAccountancy_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod)e.Row;
				if (header == null) return;
				ClearMethodLines(header);
				PXCache cache = DepreciationMethodLines.Cache;
				foreach(FADepreciationMethodLines line in PXSelect<FADepreciationMethodLines, Where<FADepreciationMethodLines.methodID, Equal<Required<FADepreciationMethodLines.methodID>>>>.Select(this, header.MethodID))
				{
					FADepreciationMethodLines newLine = (FADepreciationMethodLines) cache.CreateCopy(line);	
					newLine.SetYearRatioByUser = (header.IsTableMethod == true) && (header.YearlyAccountancy == true);
					newLine = (FADepreciationMethodLines) cache.Update(newLine);
				}
			}
			protected virtual void FADepreciationMethod_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				FADepreciationMethod header = (FADepreciationMethod)e.Row;
				if (header == null) return;
				CalculatePercents.SetEnabled(header.RecoveryPeriod != 0m);
				bool halfYear	= header.DepreciationPeriodsInYear == 2;
				bool quarter	= header.DepreciationPeriodsInYear == 4;
				bool month		= header.DepreciationPeriodsInYear == 12;
				
				PXCache cache = DepreciationMethodLines.Cache;
				PXUIFieldAttribute.SetEnabled<FADepreciationMethodLines.ratioPerYear>	 (cache, null, header.YearlyAccountancy == true && header.IsTableMethod == true);
				PXUIFieldAttribute.SetVisible<FADepreciationMethodLines.ratioPerPeriod1> (cache, null, halfYear || quarter || month);
				PXUIFieldAttribute.SetVisible<FADepreciationMethodLines.ratioPerPeriod2> (cache, null, halfYear || quarter || month);
				PXUIFieldAttribute.SetVisible<FADepreciationMethodLines.ratioPerPeriod3> (cache, null, quarter || month);
				PXUIFieldAttribute.SetVisible<FADepreciationMethodLines.ratioPerPeriod4> (cache, null, quarter || month);
				PXUIFieldAttribute.SetVisible<FADepreciationMethodLines.ratioPerPeriod5> (cache, null, month);
				PXUIFieldAttribute.SetVisible<FADepreciationMethodLines.ratioPerPeriod6> (cache, null, month);
				PXUIFieldAttribute.SetVisible<FADepreciationMethodLines.ratioPerPeriod7> (cache, null, month);
				PXUIFieldAttribute.SetVisible<FADepreciationMethodLines.ratioPerPeriod8> (cache, null, month);
				PXUIFieldAttribute.SetVisible<FADepreciationMethodLines.ratioPerPeriod9> (cache, null, month);
				PXUIFieldAttribute.SetVisible<FADepreciationMethodLines.ratioPerPeriod10>(cache, null, month);
				PXUIFieldAttribute.SetVisible<FADepreciationMethodLines.ratioPerPeriod11>(cache, null, month);
				PXUIFieldAttribute.SetVisible<FADepreciationMethodLines.ratioPerPeriod12>(cache, null, month);
			}	
		#endregion

		#region Functions
			private void CreateMethodLines(FADepreciationMethod method)
			{
				int wholeRecoveryPeriods  = method.RecoveryPeriod ?? 0;
				int count = 0;
				DateTime depreciationStartDate = method.DepreciationStartDate ?? DateTime.Now;

				FinPeriod depreciationStartBookPeriod	= (FinPeriod)PXSelect<FinPeriod, Where<FinPeriod.startDate, LessEqual<Required<FinPeriod.startDate>>, And<FinPeriod.endDate, Greater<Required<FinPeriod.endDate>>>>>.Select(this, depreciationStartDate, depreciationStartDate);

				if (depreciationStartBookPeriod == null)
					throw new PXException(Messages.FinPeriodsNotDefined, depreciationStartDate.Year);
				
				int depreciationStartPeriod;
				int.TryParse(depreciationStartBookPeriod.PeriodNbr, out depreciationStartPeriod);

				int financialYears = DepreciationCalc.GetFinancialYears(wholeRecoveryPeriods, depreciationStartPeriod, method.DepreciationPeriodsInYear ?? 12, ((depreciationStartDate - depreciationStartBookPeriod.StartDate.Value).Days == 0 ? true : false));
	
				foreach (FADepreciationMethodLines existLine in PXSelect<FADepreciationMethodLines, Where<FADepreciationMethodLines.methodID, Equal<Required<FADepreciationMethodLines.methodID>>>>.Select(this, method.MethodID))
				{
					count++;
					if (count > financialYears )
					{
						DepreciationMethodLines.Delete(existLine);	
					}
				}
				if(count < financialYears)
				{
					for(int i = count + 1; i <= financialYears; i++)
					{	
						FADepreciationMethodLines line = new FADepreciationMethodLines();
						line.MethodID = method.MethodID;
						line.Year = i;
						line.RatioPerYear = 0m;
						line = (FADepreciationMethodLines) DepreciationMethodLines.Insert(line);
					}
				}
				SetMethodTotalPercents(method, null);
			}
			private void ClearMethodLines(FADepreciationMethod method)
			{
				foreach (FADepreciationMethodLines existLine in PXSelect<FADepreciationMethodLines, Where<FADepreciationMethodLines.methodID, Equal<Required<FADepreciationMethodLines.methodID>>>>.Select(this, method.MethodID))
				{
					FADepreciationMethodLines line = (FADepreciationMethodLines) DepreciationMethodLines.Cache.CreateCopy(existLine);
					line.RatioPerPeriod1 = null;
					line.RatioPerPeriod2 = null;
					line.RatioPerPeriod3 = null;	
					line.RatioPerPeriod4 = null;
					line.RatioPerPeriod5 = null;
					line.RatioPerPeriod6 = null;	
					line.RatioPerPeriod7 = null;
					line.RatioPerPeriod8 = null;
					line.RatioPerPeriod9 = null;	
					line.RatioPerPeriod10 = null;
					line.RatioPerPeriod11 = null;
					line.RatioPerPeriod12 = null;	
					DepreciationMethodLines.Update(line);
				}
			}
			private void CalculateLinesPercents(FADepreciationMethod method)
			{
				string depreciationMethod		= method.DepreciationMethod ?? "";
				bool yearlyAccountancy			= method.YearlyAccountancy ?? false;
				bool isTableMethod				= method.IsTableMethod ?? false;
				decimal multiPlier				= method.DBMultiPlier ?? 0m;
				bool switchToSL					= method.SwitchToSL ?? false;
				int bookID						= method.BookID ?? 0;

				if (depreciationStartDate		== null			||
					method.RecoveryPeriod		== 0			|| 
					bookID						== 0			||
					string.IsNullOrEmpty(depreciationMethod)	|| 
					depreciationMethod == FADepreciationMethod.depreciationMethod.DecliningBalance && isTableMethod != true && multiPlier == 0m
					) return;
					
				FABook book = PXSelect<FABook, Where<FABook.bookID, Equal<Required<FABook.bookID>>>>.Select(this, bookID);
				DepreciationCalc.SetParameters(this, bookID, method.DepreciationStartDate.Value, method.DepreciationStopDate, method, method.AveragingConvention ?? "", method.RecoveryPeriod ?? 0, method.DepreciationPeriodsInYear ?? 12, book.MidMonthType, book.MidMonthDay);

				depreciationBasis				= 1m;				
				FADepreciationMethodLines line	= null;
				int count						= 0;
				decimal percents				= 0;
				decimal otherDepreciation		= 0m;
				decimal lastDepreciation		= 0m;
				decimal	rounding				= 0;
				decimal	slDepreciation			= depreciationBasis / (decimal) wholeRecoveryPeriods;
				DateTime previousEndDate		= DateTime.MinValue;
				
				foreach (FADepreciationMethodLines existLine in PXSelect<FADepreciationMethodLines, Where<FADepreciationMethodLines.methodID, Equal<Required<FADepreciationMethodLines.methodID>>>>.Select(this, method.MethodID))
				{
					count++;
					if (count <= depreciationYears )
					{
						line = (FADepreciationMethodLines) DepreciationMethodLines.Cache.CreateCopy(existLine);
						SetLinePercents(isTableMethod, false, yearlyAccountancy, line, count, null, bookID, depreciationMethod, multiPlier, switchToSL, slDepreciation, ref otherDepreciation, ref lastDepreciation, ref rounding, ref previousEndDate);
						line = DepreciationMethodLines.Update(line);
						percents += line.RatioPerYear ?? 0m;
					}
					else
					{
						DepreciationMethodLines.Delete(existLine);	
					}
				}
				if(count < depreciationYears)
				{
					for(int i = count + 1; i <= depreciationYears; i++)
					{	
						line = new FADepreciationMethodLines();
						line.MethodID = method.MethodID;
						line.Year = i;
						SetLinePercents(isTableMethod, false, yearlyAccountancy, line, i, null, bookID, depreciationMethod, multiPlier, switchToSL, slDepreciation, ref otherDepreciation, ref lastDepreciation, ref rounding, ref previousEndDate);	
						percents += line.RatioPerYear ?? 0m;
						line = (FADepreciationMethodLines) DepreciationMethodLines.Insert(line);
					}
				}
				SetMethodTotalPercents(method, null);
			}

			Dictionary<string, FABookHist> histdict = null;

			public virtual bool UseAcceleratedDepreciation(FADepreciationMethod method)
			{
				return FASetup.Current.AcceleratedDepreciation == true && method.IsPureStraightLine;
			}
			
			public virtual void CalculateDepreciationAddition(FABookBalance bookbal, FADepreciationMethod method, FABookHistory next)
			{
				string bookDeprFromPeriod = bookbal.DeprFromPeriod;
				int? additionRecoveryPeriod = bookbal.RecoveryPeriod;
				string additionDeprFromPeriod = FABookPeriodIDAttribute.PeriodPlusPeriod(this, next.FinPeriodID, -(next.YtdReversed ?? 0), next.BookID);

				if (UseAcceleratedDepreciation(method) && string.Compare(additionDeprFromPeriod, bookDeprFromPeriod) > 0)
				{
					additionRecoveryPeriod -= FABookPeriodIDAttribute.PeriodMinusPeriod(this, additionDeprFromPeriod, bookDeprFromPeriod, bookbal.BookID);
					bookbal.RecoveryPeriod = additionRecoveryPeriod;
				}

				bookbal.DeprFromPeriod = additionDeprFromPeriod;
				bookbal.DeprFromDate = (additionDeprFromPeriod == bookDeprFromPeriod) ? bookbal.DeprFromDate : FABookPeriodIDAttribute.PeriodStartDate(this, additionDeprFromPeriod, bookbal.BookID);
				bookbal.DeprToPeriod = FABookPeriodIDAttribute.PeriodPlusPeriod(this, bookbal.DeprToPeriod, -(bookbal.YtdSuspended ?? 0), bookbal.BookID);

				if (bookbal.DeprToDate != null)
				{
					FABookPeriod per1 = FABookPeriodIDAttribute.FABookPeriodFromDate(this, bookbal.DeprToDate, bookbal.BookID);
					FABookPeriod per2 = PXSelect<FABookPeriod, Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>, And<FABookPeriod.finPeriodID, Equal<Required<FABookPeriod.finPeriodID>>>>>.Select(this, bookbal.BookID, bookbal.DeprToPeriod);

					if (DateTime.Equals(bookbal.DeprToDate, ((DateTime)per1.EndDate).AddDays(-1)))
					{
						bookbal.DeprToDate = ((DateTime)per2.EndDate).AddDays(-1);
					}
					else
					{
						int days = ((TimeSpan)(bookbal.DeprToDate - per1.StartDate)).Days;

						if (days < ((TimeSpan)(per2.EndDate - per2.StartDate)).Days)
						{
							bookbal.DeprToDate = ((DateTime)per2.StartDate).AddDays(days);
						}
						else
						{
							bookbal.DeprToDate = per2.EndDate;
						}
					}

					FABookBalance bookbal2 = PXCache<FABookBalance>.CreateCopy(bookbal);
					bookbal2.DeprToDate = null;

					DepreciationCalc.SetParameters(this, method, bookbal2);

					if (DateTime.Compare((DateTime)this.recoveryEndDate, (DateTime)bookbal.DeprToDate) < 0)
					{
						bookbal.DeprToDate = this.recoveryEndDate;
						bookbal.DeprToPeriod = FABookPeriodIDAttribute.PeriodFromDate(this, this.recoveryEndDate, bookbal.BookID);
					}
				}

				bookbal.AcquisitionCost = this.Round(next.PtdDeprBase * bookbal.BusinessUse * 0.01m);
				bookbal.AcquisitionCost -= (additionDeprFromPeriod == bookDeprFromPeriod) ? bookbal.Tax179Amount : 0m;
				bookbal.AcquisitionCost -= (additionDeprFromPeriod == bookDeprFromPeriod) ? bookbal.BonusAmount : 0m;
				bookbal.SalvageAmount = (additionDeprFromPeriod == bookDeprFromPeriod) ? bookbal.SalvageAmount : 0m;

				this._Destination = FADestination.Calculated;

				CalculateDepreciation(method, bookbal);
			}

			public virtual void CalculateDepreciation(FABookBalance assetBalance)
			{
				CalculateDepreciation(assetBalance, null);
			} 
		   
			public virtual void CalculateDepreciation(FABookBalance assetBalance, string maxPeriodID)
			{
				histdict = new Dictionary<string, FABookHist>();
				foreach (FABookHist item in  PXSelectReadonly<FABookHist, Where<FABookHist.assetID, Equal<Required<FABookHist.assetID>>, And<FABookHist.bookID, Equal<Required<FABookHist.bookID>>>>>.Select(this, assetBalance.AssetID, assetBalance.BookID))
				{
					histdict[item.FinPeriodID] = item;
				}

				FADepreciationMethod method = PXSelect<FADepreciationMethod, Where<FADepreciationMethod.methodID, Equal<Required<FADepreciationMethod.methodID>>>>.Select(this, assetBalance.DepreciationMethodID);
				if (method == null)
				{
					throw new PXException(Messages.DepreciationMethodDoesNotExist);
				}

				if (string.IsNullOrEmpty(assetBalance.DeprFromPeriod) || string.IsNullOrEmpty(assetBalance.DeprToPeriod) || string.Compare(assetBalance.DeprFromPeriod, assetBalance.DeprToPeriod) > 0)
				{
					FABook book = PXSelect<FABook, Where<FABook.bookID, Equal<Current<FABookBalance.bookID>>>>.SelectSingleBound(this, new object[] { assetBalance });
					throw new PXException(Messages.IncorrectDepreciationPeriods, book.BookCode);
				}
				string minPeriod = assetBalance.DeprFromPeriod;

				if (maxPeriodID == null || string.Compare(maxPeriodID, assetBalance.DeprToPeriod) > 0)
				{
					maxPeriodID = assetBalance.DeprToPeriod;
				}

				PXRowInserting FABookHistRowInserting = delegate(PXCache sender, PXRowInsertingEventArgs e)
															 {
																 FABookHist item = e.Row as FABookHist;
																 if (item == null) return;
																
																 if (string.Compare(item.FinPeriodID, maxPeriodID) > 0)
																 {
																	 e.Cancel = true;
																 }
															 };

				RowInserting.AddHandler<FABookHist>(FABookHistRowInserting);

				foreach (PXResult<FABookHistoryNextPeriod, FABookHistory> res in PXSelectReadonly2<FABookHistoryNextPeriod, InnerJoin<FABookHistory, 
																						On<FABookHistory.assetID, Equal<FABookHistoryNextPeriod.assetID>, 
																							And<FABookHistory.bookID, Equal<FABookHistoryNextPeriod.bookID>, 
																							And<FABookHistory.finPeriodID, Equal<FABookHistoryNextPeriod.nextPeriodID>>>>>, 
																						Where<FABookHistoryNextPeriod.assetID, Equal<Current<FABookBalance.assetID>>, 
																							And<FABookHistoryNextPeriod.bookID, Equal<Current<FABookBalance.bookID>>, 
																							And<FABookHistoryNextPeriod.ptdDeprBase, NotEqual<decimal0>, 
																							And<FABookHistoryNextPeriod.finPeriodID, LessEqual<Current<FABookBalance.deprToPeriod>>>>>>, 
																						OrderBy<Asc<FABookHistoryNextPeriod.finPeriodID>>>.SelectMultiBound(this, new object[] { assetBalance }))
				{
					FABookHistory next = res;
					next.PtdDeprBase = ((FABookHistoryNextPeriod)res).PtdDeprBase;

					FABookBalance bookbal = PXCache<FABookBalance>.CreateCopy(assetBalance);
					string bookDeprFromPeriod = bookbal.DeprFromPeriod;
					string additionDeprFromPeriod = FABookPeriodIDAttribute.PeriodPlusPeriod(this, next.FinPeriodID, -(next.YtdReversed ?? 0), next.BookID);

					if (string.Compare(additionDeprFromPeriod, minPeriod) < 0)
					{
						minPeriod = additionDeprFromPeriod;
					}
					PXRowInserting AdditionInserting = delegate(PXCache sender, PXRowInsertingEventArgs e)
					{
						FABookHist item = e.Row as FABookHist;
						if (item == null) return;

						if (string.Compare(item.FinPeriodID, additionDeprFromPeriod) < 0)
						{
							e.Cancel = true;
						}
					};

					RowInserting.AddHandler<FABookHist>(AdditionInserting);

					CalculateDepreciationAddition(bookbal, method, next);

					if (additionDeprFromPeriod == bookDeprFromPeriod && bookbal.Tax179Amount > 0m)
					{
						FABookHist accuhist = new FABookHist();
						accuhist.AssetID = bookbal.AssetID;
						accuhist.BookID = bookbal.BookID;
						accuhist.FinPeriodID = bookDeprFromPeriod;

						accuhist = BookHistory.Insert(accuhist);

						accuhist.PtdCalculated += bookbal.Tax179Amount;
						accuhist.YtdCalculated += bookbal.Tax179Amount;
						accuhist.PtdTax179Calculated += bookbal.Tax179Amount;
						accuhist.YtdTax179Calculated += bookbal.Tax179Amount;

						this._Destination = FADestination.Tax179;

						bookbal.AcquisitionCost = bookbal.Tax179Amount;
						bookbal.SalvageAmount = 0m;

						CalculateDepreciation(method, bookbal);
					}

					if (additionDeprFromPeriod == bookDeprFromPeriod && bookbal.BonusAmount > 0m)
					{
						FABookHist accuhist = new FABookHist();
						accuhist.AssetID = bookbal.AssetID;
						accuhist.BookID = bookbal.BookID;
						accuhist.FinPeriodID = bookDeprFromPeriod;

						accuhist = BookHistory.Insert(accuhist);

						accuhist.PtdCalculated += bookbal.BonusAmount;
						accuhist.YtdCalculated += bookbal.BonusAmount;
						accuhist.PtdBonusCalculated += bookbal.BonusAmount;
						accuhist.YtdBonusCalculated += bookbal.BonusAmount;

						this._Destination = FADestination.Bonus;

						bookbal.AcquisitionCost = bookbal.BonusAmount;
						bookbal.SalvageAmount = 0m;

						CalculateDepreciation(method, bookbal);
					}

					RowInserting.RemoveHandler<FABookHist>(AdditionInserting);
				}

				PXCache cache = this.Caches[typeof(FABookHist)];

				List<FABookHist> inserted = new List<FABookHist>((IEnumerable<FABookHist>)cache.Inserted);
				inserted.Sort((a, b) =>
				{
					return a.FinPeriodID.CompareTo(b.FinPeriodID);
				});

				decimal? running = 0m;
				string lastGoodPeriodID = minPeriod;
				decimal? lastGoodVal = 0m;
				bool lastGoodFlag = true;

				foreach (FABookHist item in inserted)
				{
					item.YtdCalculated += running;
					running += item.PtdCalculated;

					FABookHist existing;
					if (histdict.TryGetValue(item.FinPeriodID, out existing))
					{
						item.PtdDepreciated = existing.PtdDepreciated;
						item.YtdDepreciated = existing.YtdDepreciated;

						if (UseAcceleratedDepreciation(method) && string.Equals(existing.FinPeriodID, assetBalance.CurrDeprPeriod) && Math.Abs((decimal)item.YtdCalculated - (decimal)item.PtdCalculated - (decimal)existing.YtdDepreciated) > 0.00005m)
						{
							//previous period YtdCalculated - YtdDepreciated
							decimal? catchup = item.YtdCalculated - item.PtdCalculated - existing.YtdDepreciated;

							FABookBalance bookbal = PXCache<FABookBalance>.CreateCopy(assetBalance);

							FABookHistory next = new FABookHistory()
							{
								FinPeriodID = assetBalance.CurrDeprPeriod,
								BookID = assetBalance.BookID,
								PtdDeprBase = catchup,
								YtdSuspended = existing.YtdSuspended,
								YtdReversed = existing.YtdReversed
							};

							CalculateDepreciationAddition(bookbal, method, next);

							item.YtdCalculated = existing.YtdDepreciated + item.PtdCalculated;
							running = item.YtdCalculated;
						}
						//

						if (lastGoodFlag)
						{
							lastGoodPeriodID = item.FinPeriodID;
							if (UseAcceleratedDepreciation(method) && string.Compare(item.FinPeriodID, assetBalance.CurrDeprPeriod) < 0)
							{
								if (Math.Abs((decimal)existing.YtdDepreciated - (decimal)existing.YtdCalculated) >= 0.00005m)
								{
									lastGoodFlag = false;
									continue;
								}
								lastGoodVal = existing.YtdDepreciated;
							}
							else
							{
								if (Math.Abs((decimal)existing.YtdCalculated - (decimal)item.YtdCalculated) >= 0.00005m)
								{
									lastGoodFlag = false;
									continue;
								}
								lastGoodVal = existing.YtdCalculated;
							}
							cache.SetStatus(item, PXEntryStatus.Notchanged);
						}
					}
					else
					{
						//in case of hole found in existing depreciation mark as last good
						if (lastGoodFlag)
						{
							lastGoodPeriodID = item.FinPeriodID;
							lastGoodFlag = false;
						}
					}
				}
				RowInserting.RemoveHandler<FABookHist>(FABookHistRowInserting);

				foreach (FABookHist item in inserted)
				{
					decimal adjusted = 0m;
					FABookHist existing;

					if (UseAcceleratedDepreciation(method) && string.Compare(item.FinPeriodID, assetBalance.CurrDeprPeriod) < 0)
					{
						item.PtdCalculated = item.PtdDepreciated;

					}

					if (UseAcceleratedDepreciation(method) && string.Compare(item.FinPeriodID, lastGoodPeriodID) >= 0)
					{
						if (histdict.TryGetValue(item.FinPeriodID, out existing))
						{
							adjusted = existing.PtdAdjusted ?? 0m;
						} 
					}

					item.YtdCalculated = item.PtdCalculated + adjusted;
					item.PtdDepreciated = 0m;
					item.YtdDepreciated = 0m;
				}

				using (PXTransactionScope ts = new PXTransactionScope())
				{
					PXDatabase.Delete<FABookHistory>(
							new PXDataFieldRestrict("AssetID", PXDbType.Int, 4, assetBalance.AssetID, PXComp.EQ),
							new PXDataFieldRestrict("BookID", PXDbType.Int, 4, assetBalance.BookID, PXComp.EQ),
							new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, minPeriod, PXComp.LT));

					if(string.Compare(assetBalance.LastDeprPeriod, assetBalance.DeprToPeriod) < 0)
					{
						PXDatabase.Delete<FABookHistory>(
								new PXDataFieldRestrict("AssetID", PXDbType.Int, 4, assetBalance.AssetID, PXComp.EQ),
								new PXDataFieldRestrict("BookID", PXDbType.Int, 4, assetBalance.BookID, PXComp.EQ),
								new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, assetBalance.DeprToPeriod, PXComp.GT));
					}

					//otherwise PtdDepreciated will be reset to zero on the last period
					if (!lastGoodFlag)
					{
						PXDatabase.Update<FABookHistory>(
								new PXDataFieldRestrict("AssetID", PXDbType.Int, 4, assetBalance.AssetID, PXComp.EQ),
								new PXDataFieldRestrict("BookID", PXDbType.Int, 4, assetBalance.BookID, PXComp.EQ),
								new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, lastGoodPeriodID, PXComp.GE),
								new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, maxPeriodID, PXComp.LE),
								new PXDataFieldAssign("PtdCalculated", PXDbType.Decimal, 0m),
								new PXDataFieldAssign("YtdCalculated", PXDbType.Decimal, lastGoodVal),
								new PXDataFieldAssign("PtdBonusCalculated", PXDbType.Decimal, 0m),
								new PXDataFieldAssign("YtdBonusCalculated", PXDbType.Decimal, 0m),
								new PXDataFieldAssign("PtdTax179Calculated", PXDbType.Decimal, 0m),
								new PXDataFieldAssign("YtdTax179Calculated", PXDbType.Decimal, 0m),
								new PXDataFieldAssign("PtdBonusTaken", PXDbType.Decimal, 0m),
								new PXDataFieldAssign("YtdBonusTaken", PXDbType.Decimal, 0m),
								new PXDataFieldAssign("PtdTax179Taken", PXDbType.Decimal, 0m),
								new PXDataFieldAssign("YtdTax179Taken", PXDbType.Decimal, 0m)
								);
					}
					Save.Press();

					if (assetBalance.UpdateGL == true)
					{
						string maxClosedPeriod = null;
						foreach (FABookHistory hist in PXSelectJoin<FABookHistory, InnerJoin<FinPeriod, On<FABookHistory.finPeriodID, Equal<FinPeriod.finPeriodID>>>, Where<FABookHistory.closed, NotEqual<True>, And<FinPeriod.fAClosed, Equal<True>, And<FABookHistory.assetID, Equal<Current<FABookBalance.assetID>>, And<FABookHistory.bookID, Equal<Current<FABookBalance.bookID>>>>>>>.SelectMultiBound(this, new object[] { assetBalance }))
						{
							FABookHist accuhist = new FABookHist();
							accuhist.AssetID = assetBalance.AssetID;
							accuhist.BookID = assetBalance.BookID;
							accuhist.FinPeriodID = hist.FinPeriodID;

							accuhist = BookHistory.Insert(accuhist);

							accuhist.Closed = true;

							if (maxClosedPeriod == null || string.Compare(hist.FinPeriodID, maxClosedPeriod) > 0)
							{
								maxClosedPeriod = hist.FinPeriodID;
							}
						}

						AssetProcess.SetLastDeprPeriod(BookBalance, assetBalance, maxClosedPeriod);
						Save.Press();
					}

					ts.Complete();
				}
			}

			public virtual void CalculateDepreciation(FADepreciationMethod method, FABookBalance assetBalance)
			{
				FABook book = PXSelect<FABook, Where<FABook.bookID, Equal<Required<FABook.bookID>>>>.Select(this, assetBalance.BookID);

				string calculationMethod = method.DepreciationMethod ?? "";
				decimal multiPlier = method.DBMultiPlier ?? 0m;
				bool switchToSL = method.SwitchToSL ?? false;
				bool isTableMethod = method.IsTableMethod ?? false;
				bool yearlyAccountancy = method.YearlyAccountancy ?? false;

				DepreciationCalc.SetParameters(this, method, assetBalance);

				depreciationBasis = (decimal)(method.DepreciationMethod == FADepreciationMethod.depreciationMethod.DecliningBalance || method.DepreciationMethod == FADepreciationMethod.depreciationMethod.Dutch1 ? assetBalance.AcquisitionCost : assetBalance.AcquisitionCost - assetBalance.SalvageAmount);

				if (depreciationPeriodsInYear == 0 ||
					depreciationBasis == 0m ||
					string.IsNullOrEmpty(calculationMethod) ||
					calculationMethod == FADepreciationMethod.depreciationMethod.DecliningBalance && isTableMethod == false && multiPlier == 0m
					) return;
				
				decimal otherDepreciation		= 0m;
				decimal lastDepreciation		= 0m;
				decimal	slDepreciation			= (decimal) this.Round((assetBalance.AcquisitionCost - assetBalance.SalvageAmount) / (decimal)wholeRecoveryPeriods);
				decimal	rounding				= 0m;
				int yearCount					= 0;
				int currYear					= 0;
				DateTime previousEndDate		= DateTime.MinValue;

				if (method.IsTableMethod != true)
				{
					foreach(FABookPeriod per in PXSelect<FABookPeriod, Where<FABookPeriod.finPeriodID, Between<Required<FABookPeriod.finPeriodID>, Required<FABookPeriod.finPeriodID>>,
																		 And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>, OrderBy<Asc<FABookPeriod.finYear>>>.Select(this, depreciationStartBookPeriod.FinPeriodID, depreciationStopBookPeriod.FinPeriodID, assetBalance.BookID))
					{
						int.TryParse(per.FinYear, out currYear);
						if(yearCount != currYear)	
						{
							yearCount = currYear;
							SetLinePercents(isTableMethod, true, yearlyAccountancy, null, yearCount - depreciationStartYear + 1, assetBalance, (book.BookID ?? 0), calculationMethod, multiPlier, switchToSL, slDepreciation, ref otherDepreciation, ref lastDepreciation, ref rounding, ref previousEndDate);	
						}
					}
				}
				else
				{
					foreach(FABookPeriod per in PXSelect<FABookPeriod, Where<FABookPeriod.finPeriodID, Between<Required<FABookPeriod.finPeriodID>, Required<FABookPeriod.finPeriodID>>,
																		 And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>, OrderBy<Asc<FABookPeriod.finYear>>>.Select(this, depreciationStartBookPeriod.FinPeriodID, depreciationStopBookPeriod.FinPeriodID, assetBalance.BookID))
					{
						int.TryParse(per.FinYear, out currYear);
						if(yearCount != currYear)	
						{
							yearCount = currYear;
							FADepreciationMethodLines methodLine = (FADepreciationMethodLines)PXSelect<FADepreciationMethodLines, Where<FADepreciationMethodLines.methodID, Equal<Required<FADepreciationMethodLines.methodID>>,
																																	And<FADepreciationMethodLines.year,		Equal<Required<FADepreciationMethodLines.year>>>>>.Select(this, method.MethodID, yearCount - depreciationStartYear + 1);
							if (methodLine == null)
							{
								throw new PXException(Messages.TableMethodHasNoLineForYear, method.MethodCD, per.FinYear);
							}
							SetLinePercents(isTableMethod, true, yearlyAccountancy, methodLine, yearCount - depreciationStartYear + 1, assetBalance, (book.BookID ?? 0), calculationMethod, multiPlier, switchToSL, slDepreciation, ref otherDepreciation, ref lastDepreciation, ref rounding, ref previousEndDate);	
						}
					}	
				}
			}
			private void SetLinePercents(bool isTableMethod, bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, int bookID, string depreciationMethod, decimal multiPlier, bool switchToSL, decimal slDepreciation, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding, ref DateTime previousEndDate)
			{
				if (isTableMethod == true && yearlyAccountancy == true)
				{
					averagingConvention = FAAveragingConvention.FullPeriod;
					SetSLDeprOther(writeToAsset, yearlyAccountancy, line, year, assetBalance, bookID, ref otherDepreciation, ref lastDepreciation, ref rounding, ref previousEndDate);					
				}
				else
				{
					if (averagingConvention == FAAveragingConvention.FullDay)
					{
						SetSLDeprOther(writeToAsset, yearlyAccountancy, line, year, assetBalance, bookID, ref otherDepreciation, ref lastDepreciation, ref rounding, ref previousEndDate);
					}
					switch(depreciationMethod)
					{
						case FADepreciationMethod.depreciationMethod.StraightLine:
							switch(depreciationPeriodsInYear)
							{
								case 12:
									SetSLDepr12(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
									break;
								case 4:
									SetSLDepr4(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
									break;
								case 2:
									SetSLDepr2(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
									break;
								case 1:
									SetSLDepr1(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
									break;
								default:
									SetSLDeprOther(writeToAsset, yearlyAccountancy, line, year, assetBalance, bookID, ref otherDepreciation, ref lastDepreciation, ref rounding, ref previousEndDate);
									break;
							}
							break;
						case FADepreciationMethod.depreciationMethod.DecliningBalance:
							SetDBDepr(writeToAsset, yearlyAccountancy, line, year, assetBalance, bookID, multiPlier, switchToSL, slDepreciation, ref rounding, ref previousEndDate);
							break;
						case FADepreciationMethod.depreciationMethod.SumOfTheYearsDigits:
							SetYDDepr(writeToAsset, yearlyAccountancy, line, year, assetBalance, bookID, ref rounding, ref previousEndDate);
							break;
						case FADepreciationMethod.depreciationMethod.Dutch1:
							SetNL1Depr(writeToAsset, line, year, assetBalance, ref rounding);
							break;
						case FADepreciationMethod.depreciationMethod.Dutch2:
							SetNL2Depr(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref rounding);
							break;
					}
				}
			}
			private decimal GetRoundingDelta(decimal rounding)
			{
				decimal decimals = (decimal)Math.Pow((double)0.1, (double)_Precision);
				return 	rounding > 0m ? 
							rounding >=	   decimals  ?   decimals : 0m : 
						rounding < 0m ? 
							rounding <= (- decimals) ? - decimals : 0m : 
						0m;			
			}
			private void SetFinalRounding(ref decimal rounding)
			{
				decimal decimals = (decimal)Math.Pow((double)0.1, (double)_Precision);
				decimal delta	 = depreciationBasis - accumulatedDepreciation;
				if (delta != decimals && delta != -decimals) return;
				
				decimal centIsAppear = rounding > 0m ? 
											(decimal) this.Round(rounding) ==    decimals  ?  decimals : 0m : 
									   rounding < 0m ? 
											(decimal) this.Round(rounding) == (- decimals) ? -decimals : 0m : 
									   0m;	
				if (centIsAppear != delta)
				{
					centIsAppear =	(delta ==  decimals) ?  decimals : 
									(delta == -decimals) ? -decimals : 0m;	
				}
				rounding = centIsAppear;
			}
			private void SetDepreciationPerPeriod(FADepreciationMethodLines line, int period, decimal value, bool useRounding, ref decimal rounding)
			{
				if (line == null) return;

				if (useRounding && rounding != 0m)
				{
					SetFinalRounding(ref rounding);
					accumulatedDepreciation += GetRoundingDelta(rounding);
					value					+= GetRoundingDelta(rounding);
					rounding				-= GetRoundingDelta(rounding);
				}
				value = (decimal) this.Round(value);
				switch(period)
				{
					case 1:
						line.RatioPerPeriod1 = value;
						break;
					case 2:
						line.RatioPerPeriod2 = value;
						break;
					case 3:
						line.RatioPerPeriod3 = value;
						break;
					case 4:
						line.RatioPerPeriod4 = value;
						break;
					case 5:
						line.RatioPerPeriod5 = value;
						break;
					case 6:
						line.RatioPerPeriod6 = value;
						break;
					case 7:
						line.RatioPerPeriod7 = value;
						break;
					case 8:
						line.RatioPerPeriod8 = value;
						break;
					case 9:
						line.RatioPerPeriod9 = value;
						break;
					case 10:
						line.RatioPerPeriod10 = value;
						break;
					case 11:
						line.RatioPerPeriod11 = value;
						break;
					case 12:
						line.RatioPerPeriod12 = value;
						break;
				}
			}

			private void SetBookDepreciationPerPeriod(FABookBalance assetBalance, int year, int period, decimal value, bool useRounding, ref decimal rounding)
			{
				if (assetBalance == null || assetBalance.DeprFromDate == null || string.IsNullOrEmpty(assetBalance.DeprFromPeriod)) return;

				//int finYear = int.Parse(assetBalance.DeprFromPeriod.Substring(0, 4)) + year - 1;
				int finYear = depreciationStartYear + year - 1;
				
				if (useRounding && rounding != 0m)
				{
					SetFinalRounding(ref rounding);
					accumulatedDepreciation += GetRoundingDelta(rounding);
					value					+= GetRoundingDelta(rounding);
					rounding				-= GetRoundingDelta(rounding);
				}
				value = (decimal)this.Round(value);

				string PeriodID = string.Format("{0:0000}{1:00}", finYear, period);

				FABookHist hist;
				FABookHist newhist = null;
				if (histdict.TryGetValue(PeriodID, out hist) && hist.YtdSuspended > 0)
				{
					for (int i = 0; i <= hist.YtdSuspended; i++)
					{
						//insert suspended periods + next open
						newhist = new FABookHist();
						newhist.AssetID = assetBalance.AssetID;
						newhist.BookID = assetBalance.BookID;
						newhist.FinPeriodID = FABookPeriodIDAttribute.PeriodPlusPeriod(this, PeriodID, i, hist.BookID);
						newhist = BookHistory.Insert(newhist);

						if (newhist != null && !histdict.ContainsKey(newhist.FinPeriodID))
						{ 
							FABookHist copy = PXCache<FABookHist>.CreateCopy(newhist);
							copy.YtdSuspended = hist.YtdSuspended;
							copy.PtdCalculated = 0m;
							copy.YtdCalculated = 0m;
							copy.PtdDepreciated = 0m;
							copy.YtdDepreciated = 0m;

							histdict.Add(newhist.FinPeriodID, copy);
						}
					}
				}
				else
				{
					newhist = new FABookHist();
					newhist.AssetID = assetBalance.AssetID;
					newhist.BookID = assetBalance.BookID;
					newhist.FinPeriodID = PeriodID;
					newhist = BookHistory.Insert(newhist);
				}

				if(newhist != null)
				{
					switch (_Destination)
					{
						case FADestination.Bonus:
							newhist.PtdBonusTaken += value;
							newhist.YtdBonusTaken += value;
							break;
						case FADestination.Tax179:
							newhist.PtdTax179Taken += value;
							newhist.YtdTax179Taken += value;
							break;
						default:
							newhist.PtdCalculated += value;
							newhist.YtdCalculated += value;
							break;
					}
				}
			}
			private void WhereToWriteDepreciation(bool writeToAsset, FADepreciationMethodLines methodLine, FABookBalance assetBalance, int year, int period, decimal value, bool useRounding, ref decimal rounding)
			{
				if (writeToAsset != true)
					SetDepreciationPerPeriod(methodLine, period, value, useRounding, ref rounding);
				else
					SetBookDepreciationPerPeriod(assetBalance, year, period, value, useRounding, ref rounding);	
			}
			private void SetSLDepr12(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
			{
				decimal recoveryYearsExactly	= (decimal) wholeRecoveryPeriods / (decimal) depreciationPeriodsInYear;
				decimal yearDepreciation		= (decimal) depreciationBasis / (decimal) recoveryYearsExactly;

				if ( wholeRecoveryPeriods <= 2										  && averagingConvention == FAAveragingConvention.HalfPeriod  ||
					(wholeRecoveryPeriods / (decimal) depreciationPeriodsInYear) <= 2 && averagingConvention == FAAveragingConvention.HalfYear    ||
					(wholeRecoveryPeriods / (decimal) quartersCount) <= 2			  && averagingConvention == FAAveragingConvention.HalfQuarter ||
					!(averagingConvention == FAAveragingConvention.HalfYear		||
					  averagingConvention == FAAveragingConvention.FullYear		||
					  averagingConvention == FAAveragingConvention.HalfQuarter	||
					  averagingConvention == FAAveragingConvention.FullQuarter  ||
					  averagingConvention == FAAveragingConvention.HalfPeriod	||
					  averagingConvention == FAAveragingConvention.ModifiedPeriod	||
					  averagingConvention == FAAveragingConvention.ModifiedPeriod2 ||
					  averagingConvention == FAAveragingConvention.FullPeriod ||
					  averagingConvention == FAAveragingConvention.NextPeriod)  ||
					recoveryYears == 1 && !(	averagingConvention == FAAveragingConvention.FullPeriod  ||
												averagingConvention == FAAveragingConvention.NextPeriod  ||
												averagingConvention == FAAveragingConvention.HalfPeriod  ||
												averagingConvention == FAAveragingConvention.ModifiedPeriod   ||
												averagingConvention == FAAveragingConvention.ModifiedPeriod2 ||
												averagingConvention == FAAveragingConvention.HalfQuarter ||
												averagingConvention == FAAveragingConvention.FullQuarter ||
											   (averagingConvention == FAAveragingConvention.FullYear && wholeRecoveryPeriods == depreciationPeriodsInYear)
											)) return;
						
				if (year == 1 && year < recoveryYears)
				{
					otherDepreciation = yearDepreciation / depreciationPeriodsInYear;

					switch(averagingConvention)
					{
						case FAAveragingConvention.HalfPeriod:
							SetSLDeprHalfPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.ModifiedPeriod:
						case FAAveragingConvention.ModifiedPeriod2:
							SetSLDeprModifiedPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.FullPeriod:
						case FAAveragingConvention.NextPeriod:
							SetSLDeprFullPeriodFirstYearNotEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);							
							break;
						case FAAveragingConvention.HalfQuarter: // do not use with other metrics
							SetSLDeprHalfQuarterFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, yearDepreciation, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.FullQuarter: // do not use with other metrics
							SetSLDeprFullQuarterFirstYearNotEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, yearDepreciation, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.HalfYear:
							SetSLDeprHalfYearFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.FullYear:
							SetSLDeprFullYearFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation,ref rounding);
							break;
					}
					
				}
				else
					if (year == 1 && year == recoveryYears)
					{
						otherDepreciation = (decimal) depreciationBasis / (decimal) wholeRecoveryPeriods;
						switch(averagingConvention)
						{
							case FAAveragingConvention.HalfPeriod:
								SetSLDeprHalfPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
							case FAAveragingConvention.ModifiedPeriod:
							case FAAveragingConvention.ModifiedPeriod2:
								SetSLDeprModifiedPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
								break;
							case FAAveragingConvention.FullPeriod:
							case FAAveragingConvention.FullYear:
							case FAAveragingConvention.NextPeriod:
								SetSLDeprFullPeriodFirstYearEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
						}
					}
					else
					if(year == recoveryYears)
					{
						switch(averagingConvention)
						{
							case FAAveragingConvention.HalfPeriod:
							case FAAveragingConvention.ModifiedPeriod:
							case FAAveragingConvention.ModifiedPeriod2:
								SetSLDeprHalfPeriodLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
								break;
							case FAAveragingConvention.FullPeriod:
							case FAAveragingConvention.NextPeriod:
								SetSLDeprFullPeriodLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
							case FAAveragingConvention.HalfQuarter:
								SetSLDeprHalfQuarterLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
								break;
							case FAAveragingConvention.FullQuarter:
								SetSLDeprFullQuarterLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
							case FAAveragingConvention.HalfYear:
								SetSLDeprHalfYearLastYear(writeToAsset, line, year, assetBalance, ref lastDepreciation, ref rounding);
								break;
							case FAAveragingConvention.FullYear:
								SetSLDeprFullYearLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
						}
					}
					else
					{
						SetSLDeprOtherYears(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
					}
			}
			private void SetSLDepr4(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
			{
				if ( wholeRecoveryPeriods <= 2											&& (averagingConvention == FAAveragingConvention.HalfPeriod || averagingConvention == FAAveragingConvention.HalfQuarter) ||
					(wholeRecoveryPeriods / (decimal) depreciationPeriodsInYear) <= 2	&& averagingConvention == FAAveragingConvention.HalfYear																 ||
					!(averagingConvention == FAAveragingConvention.HalfYear		||
					  averagingConvention == FAAveragingConvention.FullYear		||
					  averagingConvention == FAAveragingConvention.HalfPeriod	||
					  averagingConvention == FAAveragingConvention.ModifiedPeriod	||
					  averagingConvention == FAAveragingConvention.ModifiedPeriod2 ||
					  averagingConvention == FAAveragingConvention.FullPeriod ||
					  averagingConvention == FAAveragingConvention.NextPeriod	||
					  averagingConvention == FAAveragingConvention.HalfQuarter	||
					  averagingConvention == FAAveragingConvention.FullQuarter) ||
					recoveryYears == 1 && !(	averagingConvention == FAAveragingConvention.FullPeriod  ||
												averagingConvention == FAAveragingConvention.NextPeriod	 ||
												averagingConvention == FAAveragingConvention.HalfPeriod  ||
												averagingConvention == FAAveragingConvention.ModifiedPeriod   ||
												averagingConvention == FAAveragingConvention.ModifiedPeriod2 ||
												averagingConvention == FAAveragingConvention.HalfQuarter ||
												averagingConvention == FAAveragingConvention.FullQuarter ||
												(averagingConvention == FAAveragingConvention.FullYear && wholeRecoveryPeriods == depreciationPeriodsInYear)
											)) return;
						
				if (year == 1 && year < recoveryYears)
				{
					otherDepreciation = (decimal) depreciationBasis / (decimal) wholeRecoveryPeriods;
					switch(averagingConvention)
					{
						case FAAveragingConvention.HalfPeriod:
						case FAAveragingConvention.HalfQuarter:
							SetSLDeprHalfPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.ModifiedPeriod:
						case FAAveragingConvention.ModifiedPeriod2:
							SetSLDeprModifiedPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.FullPeriod:
						case FAAveragingConvention.FullQuarter:
						case FAAveragingConvention.NextPeriod:
							SetSLDeprFullPeriodFirstYearNotEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);							
							break;
						case FAAveragingConvention.HalfYear:
							SetSLDeprHalfYearFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.FullYear:
							SetSLDeprFullYearFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation,ref rounding);
							break;
					}
				}
				else
					if (year == 1 && year == recoveryYears)
					{
						otherDepreciation = (decimal) depreciationBasis / (decimal) wholeRecoveryPeriods;
						switch(averagingConvention)
						{
							case FAAveragingConvention.HalfPeriod:
							case FAAveragingConvention.HalfQuarter:
								SetSLDeprHalfPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
							case FAAveragingConvention.ModifiedPeriod:
							case FAAveragingConvention.ModifiedPeriod2:
								SetSLDeprModifiedPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
								break;
							case FAAveragingConvention.FullPeriod:
							case FAAveragingConvention.NextPeriod:
							case FAAveragingConvention.FullQuarter:
							case FAAveragingConvention.FullYear:
								SetSLDeprFullPeriodFirstYearEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
						}
					}
					else
					if(year == recoveryYears)
					{
						switch(averagingConvention)
						{
							case FAAveragingConvention.HalfPeriod:
							case FAAveragingConvention.HalfQuarter:
							case FAAveragingConvention.ModifiedPeriod:
							case FAAveragingConvention.ModifiedPeriod2:
								SetSLDeprHalfPeriodLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
								break;
							case FAAveragingConvention.FullPeriod:
							case FAAveragingConvention.NextPeriod:
							case FAAveragingConvention.FullQuarter:
								SetSLDeprFullPeriodLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
							case FAAveragingConvention.HalfYear:
								SetSLDeprHalfYearLastYear(writeToAsset, line, year, assetBalance, ref lastDepreciation, ref rounding);
								break;
							case FAAveragingConvention.FullYear:
								SetSLDeprFullYearLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
						}
					}
					else
					{
						SetSLDeprOtherYears(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
					}
			}
			private void SetSLDepr2(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
			{
				int totalHalfYears	= wholeRecoveryPeriods;
				
				if ( totalHalfYears <= 2 && (averagingConvention == FAAveragingConvention.HalfPeriod || averagingConvention == FAAveragingConvention.FullQuarter)	||
					(wholeRecoveryPeriods / (decimal) depreciationPeriodsInYear) <= 2 && averagingConvention == FAAveragingConvention.HalfYear						||
					!(averagingConvention == FAAveragingConvention.HalfYear    ||
					  averagingConvention == FAAveragingConvention.FullYear    ||
					  averagingConvention == FAAveragingConvention.HalfPeriod  ||
					  averagingConvention == FAAveragingConvention.ModifiedPeriod   ||
					  averagingConvention == FAAveragingConvention.ModifiedPeriod2 ||
					  averagingConvention == FAAveragingConvention.FullQuarter ||
					  averagingConvention == FAAveragingConvention.FullPeriod  ||
					  averagingConvention == FAAveragingConvention.NextPeriod) ||
					recoveryYears == 1 && !(	averagingConvention == FAAveragingConvention.HalfPeriod	 ||
												averagingConvention == FAAveragingConvention.ModifiedPeriod	 ||
											  averagingConvention == FAAveragingConvention.ModifiedPeriod2 ||
												averagingConvention == FAAveragingConvention.FullQuarter ||
												averagingConvention == FAAveragingConvention.FullPeriod  ||
												averagingConvention == FAAveragingConvention.NextPeriod  ||
												averagingConvention == FAAveragingConvention.HalfYear	 ||
												(averagingConvention == FAAveragingConvention.FullYear && totalHalfYears == depreciationPeriodsInYear)
											)) return;
		
				if (year == 1 && year < recoveryYears)
				{
					otherDepreciation = (decimal) depreciationBasis / (decimal) totalHalfYears;
					switch(averagingConvention)
					{
						case FAAveragingConvention.HalfPeriod:
						case FAAveragingConvention.FullQuarter:
							SetSLDeprHalfPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.ModifiedPeriod:
						case FAAveragingConvention.ModifiedPeriod2:
							SetSLDeprModifiedPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.FullPeriod:
						case FAAveragingConvention.NextPeriod:
						case FAAveragingConvention.HalfYear:
							SetSLDeprFullPeriodFirstYearNotEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);							
							break;
						case FAAveragingConvention.FullYear:
							SetSLDeprFullYearFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation,ref rounding);
							break;
					}
				}
				else
					if (year == 1 && year == recoveryYears)
					{
						otherDepreciation = (decimal) depreciationBasis / (decimal) totalHalfYears;
						switch(averagingConvention)
						{
							case FAAveragingConvention.HalfPeriod:
							case FAAveragingConvention.FullQuarter:
								SetSLDeprHalfPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
							case FAAveragingConvention.ModifiedPeriod:
							case FAAveragingConvention.ModifiedPeriod2:
								SetSLDeprModifiedPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
								break;
							case FAAveragingConvention.FullPeriod:
							case FAAveragingConvention.NextPeriod:
							case FAAveragingConvention.HalfYear:
							case FAAveragingConvention.FullYear:
								SetSLDeprFullPeriodFirstYearEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
						}
					}
					else
					if(year == recoveryYears)
					{
						switch(averagingConvention)
						{
							case FAAveragingConvention.HalfPeriod:
							case FAAveragingConvention.FullQuarter:
							case FAAveragingConvention.ModifiedPeriod:
							case FAAveragingConvention.ModifiedPeriod2:
								SetSLDeprHalfPeriodLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
								break;
							case FAAveragingConvention.FullPeriod:
							case FAAveragingConvention.NextPeriod:
							case FAAveragingConvention.HalfYear:
								SetSLDeprFullPeriodLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
							case FAAveragingConvention.FullYear:
								SetSLDeprFullYearLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
						}
					}
					else
					{
						SetSLDeprOtherYears(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
					}
			}
			private void SetSLDepr1(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
			{
				depreciationStartPeriod   = 1;
				depreciationStopPeriod    = 1;
				recoveryEndPeriod		  = 1;
				
				if (depreciationYears <= 2 && (averagingConvention == FAAveragingConvention.HalfYear || averagingConvention == FAAveragingConvention.HalfPeriod) ||
					!(averagingConvention == FAAveragingConvention.HalfYear    ||
					  averagingConvention == FAAveragingConvention.FullYear    ||
					  averagingConvention == FAAveragingConvention.HalfPeriod  ||
					  averagingConvention == FAAveragingConvention.ModifiedPeriod   ||
					  averagingConvention == FAAveragingConvention.ModifiedPeriod2 ||
					  averagingConvention == FAAveragingConvention.FullPeriod ||
					  averagingConvention == FAAveragingConvention.NextPeriod) ||
					recoveryYears == 1 && !(	averagingConvention == FAAveragingConvention.FullPeriod ||
												averagingConvention == FAAveragingConvention.NextPeriod ||
												averagingConvention == FAAveragingConvention.FullYear
											)) return;
		
				if (year == 1 && year < recoveryYears)
				{
					otherDepreciation = (decimal) depreciationBasis / (decimal) depreciationYears;
					switch(averagingConvention)
					{
						case FAAveragingConvention.HalfYear:
						case FAAveragingConvention.HalfPeriod:
							SetSLDeprHalfPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.ModifiedPeriod:
						case FAAveragingConvention.ModifiedPeriod2:
							SetSLDeprModifiedPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.FullYear:
						case FAAveragingConvention.FullPeriod:
						case FAAveragingConvention.NextPeriod:
							SetSLDeprFullPeriodFirstYearNotEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);							
							break;
					}
				}
				else
					if (year == 1 && year == recoveryYears)
					{
						otherDepreciation = (decimal) depreciationBasis / (decimal) depreciationYears;
						switch(averagingConvention)
						{
							case FAAveragingConvention.HalfPeriod:
							case FAAveragingConvention.HalfYear:
								SetSLDeprHalfPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
							case FAAveragingConvention.ModifiedPeriod:
							case FAAveragingConvention.ModifiedPeriod2:
								SetSLDeprModifiedPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
								break;
							case FAAveragingConvention.FullYear:
							case FAAveragingConvention.FullPeriod:
							case FAAveragingConvention.NextPeriod:
								SetSLDeprFullPeriodFirstYearEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
						}
					}
					else
					if(year == recoveryYears)
					{
						switch(averagingConvention)
						{
							case FAAveragingConvention.HalfYear:
							case FAAveragingConvention.HalfPeriod:
							case FAAveragingConvention.ModifiedPeriod:
							case FAAveragingConvention.ModifiedPeriod2:
								SetSLDeprHalfPeriodLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
								break;
							case FAAveragingConvention.FullYear:
							case FAAveragingConvention.FullPeriod:
							case FAAveragingConvention.NextPeriod:
								SetSLDeprFullPeriodLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
						}
					}
					else
					{
						SetSLDeprOtherYears(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
					}
			}
			private void SetSLDeprOther(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, int bookID, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding, ref DateTime previousEndDate)
			{
				if ( wholeRecoveryPeriods <= 2 && averagingConvention == FAAveragingConvention.HalfPeriod										||
					(wholeRecoveryPeriods / (decimal) depreciationPeriodsInYear) <= 2 && averagingConvention == FAAveragingConvention.HalfYear  ||
					!(averagingConvention == FAAveragingConvention.HalfYear		||
					  averagingConvention == FAAveragingConvention.FullYear		||
					  averagingConvention == FAAveragingConvention.HalfPeriod	||
					  averagingConvention == FAAveragingConvention.ModifiedPeriod	||
					  averagingConvention == FAAveragingConvention.ModifiedPeriod2 ||
					  averagingConvention == FAAveragingConvention.FullPeriod ||
					  averagingConvention == FAAveragingConvention.NextPeriod   ||
					  averagingConvention == FAAveragingConvention.FullDay && (depreciationStartDate != null && depreciationStopDate != null && wholeRecoveryDays != 0 && depreciationStartYear != 0))  ||
					recoveryYears == 1 && !(	averagingConvention == FAAveragingConvention.HalfPeriod	||
												averagingConvention == FAAveragingConvention.ModifiedPeriod	||
												  averagingConvention == FAAveragingConvention.ModifiedPeriod2 ||
												averagingConvention == FAAveragingConvention.FullPeriod ||
												averagingConvention == FAAveragingConvention.NextPeriod ||
												averagingConvention == FAAveragingConvention.FullDay && (depreciationStartDate != null && depreciationStopDate != null && wholeRecoveryDays != 0 && depreciationStartYear != 0)  ||
											   (averagingConvention == FAAveragingConvention.FullYear && wholeRecoveryPeriods == depreciationPeriodsInYear)
											)
					) return;

				if (year == 1 && year < recoveryYears)
				{
					otherDepreciation = (decimal) depreciationBasis / (decimal) wholeRecoveryPeriods;
					switch(averagingConvention)
					{
						case FAAveragingConvention.FullDay:
							
							for(int i = depreciationStartPeriod; i <= depreciationPeriodsInYear; i++)
							{
								int periodDays = DepreciationCalc.GetDaysOnPeriod(this, depreciationStartDate, depreciationStopDate, depreciationStartYear + year - 1, i, bookID, ref previousEndDate);
								otherDepreciation  = depreciationBasis * (decimal) periodDays / (decimal) wholeRecoveryDays;	
								rounding		  += (otherDepreciation - (decimal) this.Round(otherDepreciation));
								WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, true, ref rounding);
							}
							break;
						case FAAveragingConvention.HalfPeriod:
							SetSLDeprHalfPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.ModifiedPeriod:
						case FAAveragingConvention.ModifiedPeriod2:
							SetSLDeprModifiedPeriodFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;	
						case FAAveragingConvention.FullPeriod:
						case FAAveragingConvention.NextPeriod:
							SetSLDeprFullPeriodFirstYearNotEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);							
							break;
						case FAAveragingConvention.HalfYear:
							SetSLDeprHalfYearFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
							break;
						case FAAveragingConvention.FullYear:
							SetSLDeprFullYearFirstYearNotEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation,ref rounding);
							break;
					}
				}
				else
					if (year == 1 && year == recoveryYears)
					{
						otherDepreciation = (decimal) depreciationBasis / (decimal) wholeRecoveryPeriods;
						switch(averagingConvention)
						{
							case FAAveragingConvention.FullDay:
								for(int i = depreciationStartPeriod; i <= depreciationStopPeriod; i++)
								{
									int periodDays = DepreciationCalc.GetDaysOnPeriod(this, depreciationStartDate, depreciationStopDate, depreciationStartYear + year - 1, i, bookID, ref previousEndDate);
									otherDepreciation  = depreciationBasis * (decimal) periodDays / (decimal) wholeRecoveryDays;	
									rounding		  += (otherDepreciation - (decimal) this.Round(otherDepreciation));
									WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, true, ref rounding);
								}
								break;
							case FAAveragingConvention.HalfPeriod:
								SetSLDeprHalfPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
							case FAAveragingConvention.ModifiedPeriod:
							case FAAveragingConvention.ModifiedPeriod2:
								SetSLDeprModifiedPeriodFirstYearEqualLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
								break;	
							case FAAveragingConvention.FullPeriod:
							case FAAveragingConvention.NextPeriod:
							case FAAveragingConvention.FullYear:
								SetSLDeprFullPeriodFirstYearEqualLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
						}
					}
					else
					if(year == recoveryYears)
					{
						switch(averagingConvention)
						{
							case FAAveragingConvention.FullDay:
								for(int i = 1; i <= depreciationStopPeriod; i++)
								{
									int periodDays = DepreciationCalc.GetDaysOnPeriod(this, depreciationStartDate, depreciationStopDate, depreciationStartYear + year - 1, i, bookID, ref previousEndDate);
									otherDepreciation = depreciationBasis * (decimal) periodDays / (decimal) wholeRecoveryDays;	
									rounding		 += (otherDepreciation - (decimal) this.Round(otherDepreciation));
									WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, true, ref rounding);
								}
								break;
							case FAAveragingConvention.HalfPeriod:
							case FAAveragingConvention.ModifiedPeriod:
							case FAAveragingConvention.ModifiedPeriod2:
								SetSLDeprHalfPeriodLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref lastDepreciation, ref rounding);
								break;
							case FAAveragingConvention.FullPeriod:
							case FAAveragingConvention.NextPeriod:
								SetSLDeprFullPeriodLastYear(writeToAsset, yearlyAccountancy, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
							case FAAveragingConvention.HalfYear:
								SetSLDeprHalfYearLastYear(writeToAsset, line, year, assetBalance, ref lastDepreciation, ref rounding);
								break;
							case FAAveragingConvention.FullYear:
								SetSLDeprFullYearLastYear(writeToAsset, line, year, assetBalance, ref otherDepreciation, ref rounding);
								break;
						}
					}
					else
					{
						if (yearlyAccountancy == true)
						{
							if (averagingConvention == FAAveragingConvention.FullPeriod)
							{
								otherDepreciation		 = (line.RatioPerYear ?? 0m) * depreciationBasis / (decimal) depreciationPeriodsInYear;	
							}
						}
						for(int i = 1; i <= depreciationPeriodsInYear; i++)
						{
							if (averagingConvention == FAAveragingConvention.FullDay)
							{
								int periodDays = DepreciationCalc.GetDaysOnPeriod(this, depreciationStartDate, depreciationStopDate, depreciationStartYear + year - 1, i, bookID, ref previousEndDate);
								otherDepreciation = depreciationBasis * (decimal) periodDays / (decimal) wholeRecoveryDays;	
								rounding		 += (otherDepreciation - (decimal) this.Round(otherDepreciation));
							}
							if (yearlyAccountancy == true && (averagingConvention == FAAveragingConvention.FullPeriod))
							{
								accumulatedDepreciation += (decimal) this.Round(otherDepreciation);
								rounding				+= otherDepreciation - (decimal) this.Round(otherDepreciation);
							}
							WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, true, ref rounding);
						}	
					}
			}
			private void SetDBDepr(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, int bookID, decimal multiPlier, bool switchToSL, decimal slDepreciation, ref decimal rounding, ref DateTime previousEndDate)
			{
				decimal depreciation	 = 0m;
				decimal yearDepreciation = 0m;
				int yearDays			 = 0;
	
				int fromPeriod;
				int toPeriod; 

				if (yearlyAccountancy)
				{
					yearDepreciation = (depreciationBasis - accumulatedDepreciation) * multiPlier / ((decimal) wholeRecoveryPeriods / (decimal) depreciationPeriodsInYear);
				}
						
				if (year == 1 && year < recoveryYears)
				{
					fromPeriod  = depreciationStartPeriod;
					toPeriod	= depreciationPeriodsInYear;	
				}
				else
				if (year == 1 && year == recoveryYears)
				{
					fromPeriod  = depreciationStartPeriod;
					toPeriod	= recoveryEndPeriod;
				}
				else
				if(year == recoveryYears)
				{
					fromPeriod = 1;
					toPeriod   = recoveryEndPeriod;
				}
				else
				{
					fromPeriod  = 1;
					toPeriod	= depreciationPeriodsInYear;
				}
				int depreciateToPeriod = toPeriod; 

				if (year == depreciationYears)
					depreciateToPeriod = depreciationStopPeriod;

				if (yearlyAccountancy)
				{
					if (averagingConvention == FAAveragingConvention.FullDay)
					{
						yearDays = 0;
						DateTime periodPreviousEndDate = previousEndDate;
						for(int i = fromPeriod; i <= depreciateToPeriod; i++)
						{
							yearDays += DepreciationCalc.GetDaysOnPeriod(this, depreciationStartDate, recoveryEndDate, depreciationStartYear + year - 1, i, bookID, ref previousEndDate);
						}
						previousEndDate = periodPreviousEndDate;
					}
						
					for(int i = fromPeriod; i <= depreciateToPeriod; i++)
					{
						if (depreciationBasis == accumulatedDepreciation) return;
						if (averagingConvention == FAAveragingConvention.FullDay)
						{
							int periodDays = DepreciationCalc.GetDaysOnPeriod(this, depreciationStartDate, depreciationStopDate, depreciationStartYear + year - 1, i, bookID, ref previousEndDate);
							depreciation   = yearDepreciation * (decimal) periodDays / (decimal) yearDays;
						}
						else
						{
							depreciation = yearDepreciation / (decimal) depreciationPeriodsInYear;
						}
						depreciation = switchToSL ? slDepreciation > depreciation ? slDepreciation > (depreciationBasis - accumulatedDepreciation) ? (depreciationBasis - accumulatedDepreciation) : slDepreciation : depreciation : depreciation;
						accumulatedDepreciation += (decimal) this.Round(depreciation);
						if ((decimal) this.Round(depreciation) != (depreciationBasis - accumulatedDepreciation))
							rounding += depreciation - (decimal) this.Round(depreciation);
						WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciation, true, ref rounding);
					}
				}
				else
				{
					for(int i = fromPeriod; i <= depreciateToPeriod; i++)
					{	
						if (depreciationBasis == accumulatedDepreciation) return;
						depreciation = (depreciationBasis - accumulatedDepreciation) * multiPlier / (decimal) wholeRecoveryPeriods;
						depreciation = switchToSL ? slDepreciation > depreciation ? slDepreciation > (depreciationBasis - accumulatedDepreciation) ? (depreciationBasis - accumulatedDepreciation) : slDepreciation : depreciation : depreciation;
						accumulatedDepreciation += (decimal) this.Round(depreciation);
						if ((decimal) this.Round(depreciation) != (depreciationBasis - accumulatedDepreciation))
							rounding += depreciation - (decimal) this.Round(depreciation);
						WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciation, true, ref rounding);
					}
				}
			}

			private void SetNL1Depr(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal rounding)
			{
				int fromPeriod;
				int toPeriod;

				if (year == 1 && year < recoveryYears)
				{
					fromPeriod = depreciationStartPeriod;
					toPeriod = depreciationPeriodsInYear;
				}
				else if (year == 1 && year == recoveryYears)
				{
					fromPeriod = depreciationStartPeriod;
					toPeriod = recoveryEndPeriod;
				}
				else if (year == recoveryYears)
				{
					fromPeriod = 1;
					toPeriod = recoveryEndPeriod;
				}
				else
				{
					fromPeriod = 1;
					toPeriod = depreciationPeriodsInYear;
				}
				int depreciateToPeriod = toPeriod;

				if (year == depreciationYears)
					depreciateToPeriod = depreciationStopPeriod;

				{
					for (int i = fromPeriod; i <= depreciateToPeriod; i++)
					{
						if (depreciationBasis == accumulatedDepreciation) return;
						decimal depreciation = (decimal)((double)(depreciationBasis - accumulatedDepreciation) * (1 - Math.Pow((double)(assetBalance.SalvageAmount ?? 0)/(double)depreciationBasis, 1.0/wholeRecoveryPeriods)));
						accumulatedDepreciation += (Round(depreciation) ?? 0m);
						if ((Round(depreciation) ?? 0m) != (depreciationBasis - accumulatedDepreciation))
							rounding += depreciation - (Round(depreciation) ?? 0m);
						WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciation, true, ref rounding);
					}
				}
			}

			private void SetNL2Depr(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal rounding)
			{
				int fromPeriod;
				int toPeriod;

				if (year == 1 && year < recoveryYears)
				{
					fromPeriod = depreciationStartPeriod;
					toPeriod = depreciationPeriodsInYear;
				}
				else if (year == 1 && year == recoveryYears)
				{
					fromPeriod = depreciationStartPeriod;
					toPeriod = recoveryEndPeriod;
				}
				else if (year == recoveryYears)
				{
					fromPeriod = 1;
					toPeriod = recoveryEndPeriod;
				}
				else
				{
					fromPeriod = 1;
					toPeriod = depreciationPeriodsInYear;
				}
				int depreciateToPeriod = toPeriod;

				if (year == depreciationYears)
					depreciateToPeriod = depreciationStopPeriod;

				decimal yearDepreciation = (depreciationBasis - accumulatedDepreciation) * (depreciationMethod.PercentPerYear ?? 0);
				for (int i = fromPeriod; i <= depreciateToPeriod; i++)
				{
					if (depreciationBasis == accumulatedDepreciation) return;
					decimal depreciation = yearlyAccountancy
											   ? yearDepreciation/depreciationPeriodsInYear
											   : (decimal)((double) (depreciationBasis - accumulatedDepreciation)*(1 - Math.Pow(1 - (double) (depreciationMethod.PercentPerYear ?? 0m), 1.0/depreciationPeriodsInYear)));
					accumulatedDepreciation += (Round(depreciation) ?? 0m);
					if ((Round(depreciation) ?? 0m) != (depreciationBasis - accumulatedDepreciation))
						rounding += depreciation - (Round(depreciation) ?? 0m);
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciation, true, ref rounding);
				}
			}

			private void SetYDDepr(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, int bookID, ref decimal rounding, ref DateTime previousEndDate)
			{
				if (yearlyAccountancy != true) return;

				decimal wholeYears		  = wholeRecoveryPeriods / (decimal) depreciationPeriodsInYear;
				decimal	sumOfYears		  = wholeYears * (wholeYears + 1) / 2m;
				decimal yearDepreciation1 = 0m;
				decimal yearDepreciation2 = 0m;
				decimal remainingYears	  = wholeYears - yearOfUsefulLife + 1m;
				decimal depreciation	  = 0m;
	
				int fromPeriod;
				int toPeriod; 

				yearDepreciation1 = (depreciationBasis) * (decimal) (remainingYears)  / ((decimal) sumOfYears);
				yearDepreciation2 = (depreciationBasis) * (decimal) (remainingYears - 1) / ((decimal) sumOfYears);
						
				if (year == 1 && year < recoveryYears)
				{
					fromPeriod  = depreciationStartPeriod;
					toPeriod	= depreciationPeriodsInYear;	
				}
				else
				if (year == 1 && year == recoveryYears)
				{
					fromPeriod  = depreciationStartPeriod;
					toPeriod	= recoveryEndPeriod;
				}
				else
				if(year == recoveryYears)
				{
					fromPeriod = 1;
					toPeriod   = recoveryEndPeriod;
				}
				else
				{
					fromPeriod  = 1;
					toPeriod	= depreciationPeriodsInYear;
				}
				if (year == depreciationYears)
				{
					toPeriod = depreciationStopPeriod;
				}
				bool edgeOfUsefulYear = (year == 1);
				switch (averagingConvention)
				{
					case  FAAveragingConvention.FullDay:
						int yearDays = 0;
						DateTime periodPreviousEndDate = previousEndDate;
						for(int i = fromPeriod; i <= toPeriod; i++)
						{
							yearDays += DepreciationCalc.GetDaysOnPeriod(this, depreciationStartDate, recoveryEndDate, depreciationStartYear + year - 1, i, bookID, ref previousEndDate);
						}
						previousEndDate = periodPreviousEndDate;
						
						for(int i = fromPeriod; i <= toPeriod; i++)
						{
							if (depreciationBasis == accumulatedDepreciation) return;
							if (averagingConvention == FAAveragingConvention.FullDay)
							{		
								if(edgeOfUsefulYear == false) 
								{
									edgeOfUsefulYear	 = (i == depreciationStartPeriod);
									yearOfUsefulLife	+= edgeOfUsefulYear ? 1 : 0;
								}
								int periodDays = DepreciationCalc.GetDaysOnPeriod(this, depreciationStartDate, depreciationStopDate, depreciationStartYear + year - 1, i, bookID, ref previousEndDate);
								depreciation			 = (wholeYears - remainingYears + 1m == yearOfUsefulLife ? yearDepreciation1 : yearDepreciation2) * (decimal) periodDays / (decimal) yearDays;
								accumulatedDepreciation += (decimal) this.Round(depreciation);
								rounding				+= depreciation - (decimal) this.Round(depreciation);
								WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciation, true, ref rounding);
							}
						}
						break;
					case  FAAveragingConvention.FullPeriod:
						for(int i = fromPeriod; i <= toPeriod; i++)
						{	
							if(edgeOfUsefulYear == false) 
							{
								edgeOfUsefulYear	 = (i == depreciationStartPeriod);
								yearOfUsefulLife	+= edgeOfUsefulYear ? 1 : 0;
							}
							depreciation			 = (wholeYears - remainingYears + 1m == yearOfUsefulLife ? yearDepreciation1 : yearDepreciation2) / (decimal) depreciationPeriodsInYear;
							accumulatedDepreciation += (decimal) this.Round(depreciation);
							rounding				+= depreciation - (decimal) this.Round(depreciation);
							WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciation, true, ref rounding);
						}
						break;
				}
			}
			private void SetMethodTotalPercents(FADepreciationMethod header, FADepreciationMethodLines line)
			{
				decimal sum = 0m;
				foreach(FADepreciationMethodLines l in PXSelect<FADepreciationMethodLines, Where<FADepreciationMethodLines.methodID, Equal<Required<FADepreciationMethodLines.methodID>>>>.Select(this, header.MethodID))
				{	
					sum += l.RatioPerYear ?? 0m;
				}
				if (line != null)
				{
					sum += line.RatioPerYear ?? 0m;
				}
				header = (FADepreciationMethod) DepreciationMethod.Cache.CreateCopy(header);
				if (sum != 0m)
				{
					header.TotalPercents = sum;
					DepreciationMethod.Update(header);
				}
			}
			private void SetSLDeprHalfPeriodFirstYearNotEqualLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
			{
				decimal firstDepreciation		= otherDepreciation / 2m ;
				lastDepreciation				= firstDepreciation;
				int checkZero					= (wholeRecoveryPeriods + (startPeriodIsWhole ? 0 : 1) - 2);

				rounding = depreciationBasis - (decimal) this.Round(firstDepreciation) * 2m;
				if (checkZero > 0)
				{
					otherDepreciation = (depreciationBasis - firstDepreciation * 2m) / checkZero;
					rounding		 -= (decimal) this.Round(otherDepreciation) * checkZero;
				}
				int depreciateToPeriod = depreciationPeriodsInYear;
				if (year == depreciationYears)
					depreciateToPeriod = depreciationStopPeriod;

				for(int i = depreciationStartPeriod; i <= depreciateToPeriod; i++)
				{
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, (depreciationStartPeriod == i) ? firstDepreciation: otherDepreciation, i > depreciationStartPeriod, ref rounding);
				}
			}
			private void SetSLDeprHalfQuarterFirstYearNotEqualLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, decimal yearDepreciation, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
			{
				// use only with metric 12
				decimal firstDepreciation		= yearDepreciation / quartersCount / 2m / (lastDepreciationStartQuarterPeriod - depreciationStartPeriod + 1);
				lastDepreciation				= yearDepreciation / quartersCount / 2m / (recoveryEndPeriod - firstRecoveryEndQuarterPeriod + 1);

				int checkZero = (wholeRecoveryPeriods + (startPeriodIsWhole ? 0 : 1) - (lastDepreciationStartQuarterPeriod - depreciationStartPeriod + 1) - (recoveryEndPeriod - firstRecoveryEndQuarterPeriod + 1));
				if (checkZero > 0 )
					otherDepreciation = (depreciationBasis - firstDepreciation * (lastDepreciationStartQuarterPeriod - depreciationStartPeriod + 1) - lastDepreciation * (recoveryEndPeriod - firstRecoveryEndQuarterPeriod + 1)) / checkZero;

				rounding  = depreciationBasis - (decimal) this.Round(firstDepreciation) * (lastDepreciationStartQuarterPeriod - depreciationStartPeriod + 1);
				rounding -= (decimal) this.Round(lastDepreciation) * (recoveryEndPeriod - firstRecoveryEndQuarterPeriod + 1);
				if (checkZero > 0 )
					rounding -= (decimal) this.Round(otherDepreciation) * checkZero;

				int depreciateToPeriod = depreciationPeriodsInYear;
				if (year == depreciationYears)
					depreciateToPeriod = depreciationStopPeriod;

				for(int i = depreciationStartPeriod; i <= depreciateToPeriod; i++)
				{
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciationStartPeriod <= i && lastDepreciationStartQuarterPeriod >= i ? firstDepreciation: 
																						otherDepreciation, 
											i > depreciationStartPeriod, ref rounding);
				}
			}
			private void SetSLDeprModifiedPeriodFirstYearNotEqualLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
			{
				decimal firstDepreciation			= otherDepreciation * startDepreciationMidPeriodRatio;
				lastDepreciation = averagingConvention == FAAveragingConvention.ModifiedPeriod2 ? otherDepreciation * stopDepreciationMidPeriodRatio : firstDepreciation;
				int checkZero						= (wholeRecoveryPeriods + (startPeriodIsWhole ? 0 : 1) - 2 );

				rounding  = depreciationBasis - (decimal) this.Round(firstDepreciation);
				rounding -= (decimal) this.Round(lastDepreciation);
				if (checkZero > 0)
				{
					otherDepreciation = (depreciationBasis - firstDepreciation - lastDepreciation) / checkZero;
					rounding		 -= (decimal) this.Round(otherDepreciation) * checkZero;
				}
				int depreciateToPeriod = depreciationPeriodsInYear;
				if (year == depreciationYears)
					depreciateToPeriod = depreciationStopPeriod;

				for(int i = depreciationStartPeriod; i <= depreciateToPeriod; i++)
				{
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciationStartPeriod == i ? firstDepreciation : (year == depreciationYears && i == recoveryEndPeriod) ? lastDepreciation : otherDepreciation, i > depreciationStartPeriod, ref rounding);
				}
			}
			private void SetSLDeprFullPeriodFirstYearNotEqualLastYear(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal rounding)
			{
				if (yearlyAccountancy == true)
				{
					otherDepreciation		 = (line.RatioPerYear ?? 0m) * depreciationBasis / (depreciationPeriodsInYear - depreciationStartPeriod + 1);	
				}
				else
				{
					rounding = depreciationBasis - (decimal) this.Round(otherDepreciation) * wholeRecoveryPeriods;
				}

				int depreciateToPeriod = depreciationPeriodsInYear;
				if (year == depreciationYears)
					depreciateToPeriod = depreciationStopPeriod;

				for(int i = depreciationStartPeriod; i <= depreciateToPeriod; i++)
				{
					if (yearlyAccountancy == true)
					{
						accumulatedDepreciation += (decimal) this.Round(otherDepreciation);
						rounding				+= otherDepreciation - (decimal) this.Round(otherDepreciation);
					}
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, depreciationStartPeriod < depreciationPeriodsInYear ? i > depreciationStartPeriod || yearlyAccountancy == true : true, ref rounding);
				}
			}
			private void SetSLDeprFullQuarterFirstYearNotEqualLastYear(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, decimal yearDepreciation, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
			{
				// use only with metric 12
				decimal firstDepreciation		= yearDepreciation / quartersCount / (lastDepreciationStartQuarterPeriod - depreciationStartPeriod + 1);
				int checkZero = wholeRecoveryPeriods + (startPeriodIsWhole ? 0 : 1) - quarterToMonth; 
				if (checkZero > 0 )
					otherDepreciation = (depreciationBasis - firstDepreciation * (lastDepreciationStartQuarterPeriod - depreciationStartPeriod + 1)) / checkZero; 

				rounding  = depreciationBasis - (decimal) this.Round(firstDepreciation) * (lastDepreciationStartQuarterPeriod - depreciationStartPeriod + 1);
				if (checkZero > 0 )
					rounding -= (decimal) this.Round(otherDepreciation) * checkZero;

				int depreciateToPeriod = depreciationPeriodsInYear;

				if (year == depreciationYears)
					depreciateToPeriod = depreciationStopPeriod;

				for(int i = depreciationStartPeriod; i <= depreciateToPeriod; i++)
				{
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciationStartPeriod <= i && lastDepreciationStartQuarterPeriod >= i ? firstDepreciation: 
																						otherDepreciation, 
											 i > depreciationStartPeriod, ref rounding);
				}
			}
			private void SetSLDeprHalfYearFirstYearNotEqualLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
			{
				decimal firstDepreciation			= otherDepreciation * depreciationPeriodsInYear / (depreciationPeriodsInYear - depreciationStartPeriod + 1) / 2m;
				lastDepreciation					= otherDepreciation * depreciationPeriodsInYear / recoveryEndPeriod / 2m;
				int checkZero = (wholeRecoveryPeriods + (startPeriodIsWhole ? 0 : 1) - (depreciationPeriodsInYear - depreciationStartPeriod + 1) - recoveryEndPeriod);
				if(checkZero > 0)
					otherDepreciation = (depreciationBasis - firstDepreciation * (depreciationPeriodsInYear - depreciationStartPeriod + 1) - lastDepreciation * recoveryEndPeriod) / checkZero;

				rounding  = depreciationBasis - (decimal) this.Round(firstDepreciation) * (depreciationPeriodsInYear - depreciationStartPeriod + 1);
				rounding -= (decimal) this.Round(lastDepreciation) * recoveryEndPeriod;

				if(checkZero > 0)
					rounding -= (decimal) this.Round(otherDepreciation) * checkZero;

				int depreciateToPeriod = depreciationPeriodsInYear;
				if (year == depreciationYears)
					depreciateToPeriod = depreciationStopPeriod;

				for(int i = depreciationStartPeriod; i <= depreciateToPeriod; i++)
				{
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, firstDepreciation, i > depreciationStartPeriod, ref rounding);
				}
			}
			private void SetSLDeprFullYearFirstYearNotEqualLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
			{
				decimal firstDepreciation		= otherDepreciation * depreciationPeriodsInYear / (depreciationPeriodsInYear - depreciationStartPeriod + 1);

				int checkZero = wholeRecoveryPeriods + (startPeriodIsWhole ? 0 : 1) - depreciationPeriodsInYear; 
				if(checkZero > 0)
					otherDepreciation = (depreciationBasis - firstDepreciation * (depreciationPeriodsInYear - depreciationStartPeriod + 1)) / checkZero; 

				rounding  = depreciationBasis - (decimal) this.Round(firstDepreciation) * (depreciationPeriodsInYear - depreciationStartPeriod + 1);

				if(checkZero > 0)
					rounding -= (decimal) this.Round(otherDepreciation) * checkZero;

				int depreciateToPeriod = depreciationPeriodsInYear;
				if (year == depreciationYears)
					depreciateToPeriod = depreciationStopPeriod;

				for(int i = depreciationStartPeriod; i <= depreciateToPeriod; i++)
				{
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, firstDepreciation, i > depreciationStartPeriod, ref rounding);
				}
			}
			private void SetSLDeprHalfPeriodFirstYearEqualLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal rounding)
			{
				decimal firstDepreciation		= otherDepreciation / 2;
				int checkZero = (wholeRecoveryPeriods + (startPeriodIsWhole ? 0 : 1) - 2);
				if (checkZero > 0) 
					otherDepreciation = (depreciationBasis - firstDepreciation * 2) / checkZero;

				rounding = depreciationBasis - (decimal) this.Round(firstDepreciation) * 2;

				if (checkZero > 0) 
					rounding -= (decimal) this.Round(otherDepreciation) * checkZero;

				for(int i = depreciationStartPeriod; i <= depreciationStopPeriod; i++)
				{
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciationStartPeriod == i || recoveryEndPeriod == i ? firstDepreciation: otherDepreciation, i > depreciationStartPeriod, ref rounding);
				}
			}
			private void SetSLDeprModifiedPeriodFirstYearEqualLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
			{
				decimal firstDepreciation			= otherDepreciation * startDepreciationMidPeriodRatio;
				//decimal lastDepreciationRecoveryEnd	= otherDepreciation * startDepreciationMidPeriodRatio;
				lastDepreciation = averagingConvention == FAAveragingConvention.ModifiedPeriod2 ? otherDepreciation * stopDepreciationMidPeriodRatio : firstDepreciation;

				int checkZero = (wholeRecoveryPeriods + (startPeriodIsWhole ? 0 : 1) - 2);
				if (checkZero > 0) 
					otherDepreciation = (depreciationBasis - firstDepreciation - lastDepreciation) / checkZero;

				rounding  = depreciationBasis - (decimal) this.Round(firstDepreciation);
				rounding -= (decimal) this.Round(lastDepreciation);

				if (checkZero > 0) 
					rounding -= (decimal) this.Round(otherDepreciation) * checkZero;

				for(int i = depreciationStartPeriod; i <= depreciationStopPeriod; i++)
				{
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, depreciationStartPeriod == i ? firstDepreciation : recoveryEndPeriod == i ? lastDepreciation: otherDepreciation, i > depreciationStartPeriod, ref rounding);
				}
			}
			private void SetSLDeprFullPeriodFirstYearEqualLastYear(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal rounding)	
			{
				if (yearlyAccountancy == true)
				{
					otherDepreciation		 = (line.RatioPerYear ?? 0m) * depreciationBasis / (decimal) wholeRecoveryPeriods;	
				}
				else
				{
					rounding = depreciationBasis - (decimal) this.Round(otherDepreciation) * wholeRecoveryPeriods;
				}
				for(int i = depreciationStartPeriod; i <= depreciationStopPeriod; i++)
				{
					if (yearlyAccountancy == true)
					{
						accumulatedDepreciation += (decimal) this.Round(otherDepreciation);
						rounding				+= otherDepreciation - (decimal) this.Round(otherDepreciation);
					}
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, i > depreciationStartPeriod || (yearlyAccountancy == true), ref rounding);
				}
			}
			private void SetSLDeprHalfPeriodLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
			{
				for(int i = 1; i <= depreciationStopPeriod; i++)
				{
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, recoveryEndPeriod == i ? lastDepreciation: otherDepreciation, true, ref rounding);
				}
			}
			private void SetSLDeprHalfQuarterLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal lastDepreciation, ref decimal rounding)
			{
				for(int i = 1; i <= depreciationStopPeriod; i++)
				{
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, (recoveryEndPeriod >= i && firstRecoveryEndQuarterPeriod <= i) ? lastDepreciation : otherDepreciation, true, ref rounding);
				}
			}
			private void SetSLDeprFullPeriodLastYear(bool writeToAsset, bool yearlyAccountancy, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal rounding)	
			{
				if (yearlyAccountancy == true)
				{
					otherDepreciation		 = (line.RatioPerYear ?? 0m) * depreciationBasis / (decimal) recoveryEndPeriod;	
				}
				for(int i = 1; i <= depreciationStopPeriod; i++)
				{
					if (yearlyAccountancy == true)
					{
						accumulatedDepreciation += (decimal) this.Round(otherDepreciation);
						rounding				+= otherDepreciation - (decimal) this.Round(otherDepreciation);
					}
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, true, ref rounding);
				}
			}
			private void SetSLDeprFullQuarterLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal rounding)	
			{
				for(int i = 1; i <= depreciationStopPeriod; i++)
				{
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, true, ref rounding);
				}
			}
			private void SetSLDeprHalfYearLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal lastDepreciation, ref decimal rounding)
			{
				for(int i = 1; i <= depreciationStopPeriod; i++)
				{
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, lastDepreciation, true, ref rounding);
				}
			}
			private void SetSLDeprFullYearLastYear(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal rounding)
			{
				for(int i = 1; i <= depreciationStopPeriod; i++)
				{
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, true, ref rounding);
				}
			}
			private void SetSLDeprOtherYears(bool writeToAsset, FADepreciationMethodLines line, int year, FABookBalance assetBalance, ref decimal otherDepreciation, ref decimal rounding)	
			{
				int depreciateToPeriod = depreciationPeriodsInYear;
				if (year == depreciationYears)
					depreciateToPeriod = depreciationStopPeriod;

				for(int i = 1; i <= depreciateToPeriod; i++)
				{
					WhereToWriteDepreciation(writeToAsset, line, assetBalance, year, i, otherDepreciation, true, ref rounding);
				}	
			}
		#endregion

			#region IDepreciationCalculation Members
			public decimal depreciationBasis
			{
				get;
				set;
			}
			public decimal accumulatedDepreciation
			{
				get;
				set;
			}
			public int depreciationStartYear
			{
				get;
				set;
			}
			public FABookPeriod depreciationStopBookPeriod
			{
				get;
				set;
			}

			public FABookPeriod depreciationStartBookPeriod
			{
				get;
				set;
			}

			public int depreciationStartPeriod
			{
				get;
				set;
			}

			public int depreciationStopPeriod
			{
				get;
				set;
			}

			public int recoveryEndPeriod
			{
				get;
				set;
			}

			public int firstRecoveryEndQuarterPeriod
			{
				get;
				set;
			}

			public int firstDepreciationStopQuarterPeriod
			{
				get;
				set;
			}

			public int lastDepreciationStartQuarterPeriod
			{
				get;
				set;
			}

			public DateTime recoveryEndDate
			{
				get;
				set;
			}

			public int recoveryYears
			{
				get;
				set;
			}

			public int depreciationYears
			{
				get;
				set;
			}

			public decimal startDepreciationMidPeriodRatio
			{
				get;
				set;
			}

			public decimal stopDepreciationMidPeriodRatio
			{
				get;
				set;
			}

			public DateTime depreciationStartDate
			{
				get;
				set;
			}

			public DateTime? depreciationStopDate
			{
				get;
				set;
			}

			public int wholeRecoveryDays
			{
				get;
				set;
			}

			public bool startPeriodIsWhole
			{
				get;
				set;
			}

			public string averagingConvention
			{
				get;
				set;
			}

			public int wholeRecoveryPeriods
			{
				get;
				set;
			}

			public int depreciationPeriodsInYear
			{
				get;
				set;
			}

			public FADepreciationMethod depreciationMethod
			{
				get;
				set;
			}
			public int yearOfUsefulLife
			{
				get;
				set;
			}
			#endregion
		}

		public class DepreciationCalc : IDepreciationCalculation
		{
			protected static int monthsInYear = 12;
			protected static int quarterToMonth = 3;
			protected static int quartersCount = 4;

			public DepreciationCalc()
			{
			}

			public static DateTime GetRecoveryEndDate(PXGraph graph, FABookBalance assetBalance)
			{
				DepreciationCalc calc = new DepreciationCalc();
				DepreciationCalc.SetParameters(graph, calc, assetBalance);
				return calc.recoveryEndDate;
			}


			public static string GetRecoveryStartPeriod(PXGraph graph, FABookBalance assetBalance)
			{
				DepreciationCalc calc = new DepreciationCalc();
				DepreciationCalc.SetParameters(graph, calc, assetBalance);
				string startPeriodID = string.Format("{0:0000}{1:00}", calc.depreciationStartYear, calc.depreciationStartPeriod) ;
				if (calc.startDepreciationMidPeriodRatio == 0m && (assetBalance.AveragingConvention == FAAveragingConvention.ModifiedPeriod || assetBalance.AveragingConvention == FAAveragingConvention.ModifiedPeriod2))
				{
					return FABookPeriodIDAttribute.PeriodPlusPeriod(graph, startPeriodID, 1, assetBalance.BookID);
				}
				return startPeriodID;
			}

			public static void SetParameters(PXGraph graph, FABookBalance assetBalance)
			{
				SetParameters(graph, graph as IDepreciationCalculation ?? new DepreciationCalc(), assetBalance);
			}

			public static void SetParameters(PXGraph graph, IDepreciationCalculation calc, FABookBalance assetBalance)
			{
				FADepreciationMethod depreciationMethod = PXSelect<FADepreciationMethod, Where<FADepreciationMethod.methodID, Equal<Required<FADepreciationMethod.methodID>>>>.Select(graph, assetBalance.DepreciationMethodID);
				SetParameters(graph, calc, depreciationMethod, assetBalance);
			}

			public static void SetParameters(PXGraph graph, FADepreciationMethod depreciationMethod, FABookBalance assetBalance)
			{
				SetParameters(graph, graph as IDepreciationCalculation ?? new DepreciationCalc(), depreciationMethod, assetBalance);
			}

			public static void SetParameters(PXGraph graph, IDepreciationCalculation calc, FADepreciationMethod depreciationMethod, FABookBalance assetBalance)
			{
				int depreciationPeriodsInYear = FABookPeriodIDAttribute.GetBookPeriodsInYear(graph, assetBalance.BookID);
				SetParameters(graph, calc, assetBalance.BookID, 
							  (DateTime)assetBalance.DeprFromDate, 
							  assetBalance.DeprToDate, 
							  depreciationMethod, 
							  assetBalance.AveragingConvention, 
							  (int)assetBalance.RecoveryPeriod, 
							  depreciationPeriodsInYear, 
							  assetBalance.MidMonthType, 
							  assetBalance.MidMonthDay);
			}

			private static void InitParameters(IDepreciationCalculation calc)
			{ 
				calc.depreciationBasis = 0m;
				calc.accumulatedDepreciation = 0m;
				calc.depreciationStartYear = 0;
				calc.depreciationStopBookPeriod = null;
				calc.depreciationStartBookPeriod = null;
				calc.depreciationStartPeriod = 0;
				calc.depreciationStopPeriod = 0;
				calc.recoveryEndPeriod = 0;
				calc.firstRecoveryEndQuarterPeriod = 0;
				calc.firstDepreciationStopQuarterPeriod = 0;
				calc.lastDepreciationStartQuarterPeriod = 0;
				calc.recoveryEndDate = DateTime.MinValue;
				calc.recoveryYears = 0;
				calc.depreciationYears = 0;
				calc.startDepreciationMidPeriodRatio = 0m;
				calc.stopDepreciationMidPeriodRatio = 0m;
				calc.wholeRecoveryDays = 0;
				calc.startPeriodIsWhole = false;
				calc.averagingConvention = null;
				calc.wholeRecoveryPeriods = 0;
				calc.depreciationPeriodsInYear = 12;
				calc.depreciationMethod = null;
				calc.yearOfUsefulLife = 1;
			}

			public static void SetParameters(PXGraph graph,
				int? bookID,
				DateTime depreciationStartDate,
				DateTime? depreciationStopDate,
				FADepreciationMethod depreciationMethod,
				string averagingConvention,
				int wholeRecoveryPeriods,
				int depreciationPeriodsInYear,
				string midMonthType,
				int? midMonthDay)
			{
				SetParameters(graph, graph as IDepreciationCalculation ?? new DepreciationCalc(), bookID, depreciationStartDate, depreciationStopDate, depreciationMethod, averagingConvention, wholeRecoveryPeriods, depreciationPeriodsInYear, midMonthType, midMonthDay);
			}

			public static void SetParameters(PXGraph graph, 
				IDepreciationCalculation calc,
				int? bookID,
				DateTime depreciationStartDate,
				DateTime? depreciationStopDate,
				FADepreciationMethod depreciationMethod,
				string averagingConvention,
				int wholeRecoveryPeriods,
				int depreciationPeriodsInYear,
				string midMonthType,
				int? midMonthDay)
			{
				InitParameters(calc);

				calc.depreciationStartDate		= depreciationStartDate;
				calc.depreciationStopDate = depreciationStopDate;
				calc.depreciationMethod = depreciationMethod;
				if(depreciationMethod == null )
				{
					calc.averagingConvention = averagingConvention;
				}
				else
				{
					calc.averagingConvention = (depreciationMethod.IsTableMethod == true && depreciationMethod.YearlyAccountancy == true) ? FAAveragingConvention.FullPeriod : depreciationMethod.AveragingConvention ?? averagingConvention;
				}
				calc.wholeRecoveryPeriods = wholeRecoveryPeriods;
				calc.depreciationPeriodsInYear = depreciationPeriodsInYear;

				FABook book = PXSelect<FABook, Where<FABook.bookID, Equal<Required<FABook.bookID>>>>.Select(graph, bookID);

				calc.depreciationStartBookPeriod = (FABookPeriod)PXSelect<FABookPeriod, Where<FABookPeriod.startDate, LessEqual<Required<FABookPeriod.startDate>>, And<FABookPeriod.endDate, Greater<Required<FABookPeriod.endDate>>,
																							 And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>>>.Select(graph, calc.depreciationStartDate, calc.depreciationStartDate, bookID);

				if (calc.depreciationStartBookPeriod == null)
					throw new PXException(Messages.FABookPeriodsNotDefinedFrom, calc.depreciationStartDate.ToShortDateString(), calc.depreciationStartDate.Year, book.BookCode);


				calc.depreciationStartYear = int.Parse(calc.depreciationStartBookPeriod.FinYear);
				calc.depreciationStartPeriod = int.Parse(calc.depreciationStartBookPeriod.PeriodNbr);

				int recoveryStartPeriod = calc.depreciationStartPeriod;
				decimal depreciationStartPeriodDivide3;
				int depreciationStartQuarter;
				FABookPeriod recoveryStartBookPeriod = calc.depreciationStartBookPeriod;
				bool edgeOfPeriod					 = false;
				calc.startPeriodIsWhole = false;

				switch (calc.averagingConvention)
				{
					case FAAveragingConvention.FullDay:
						calc.startPeriodIsWhole = (calc.depreciationStartDate == calc.depreciationStartBookPeriod.StartDate.Value);
						break;
					case FAAveragingConvention.ModifiedPeriod:
					case FAAveragingConvention.ModifiedPeriod2:
						switch (midMonthType)
						{
							case FABook.midMonthType.PeriodDaysHalve:
								if (((calc.depreciationStartDate - calc.depreciationStartBookPeriod.StartDate.Value).Days + 1) > (calc.depreciationStartBookPeriod.EndDate.Value - calc.depreciationStartBookPeriod.StartDate.Value).Days / 2m)
								{
									calc.startDepreciationMidPeriodRatio = calc.averagingConvention == FAAveragingConvention.ModifiedPeriod2 ? 0m : 0.5m;
								}
								else
								{
									calc.startDepreciationMidPeriodRatio = 1m;
									calc.startPeriodIsWhole = true;
								}
								break;
							case FABook.midMonthType.FixedDay:
								if (((calc.depreciationStartDate - calc.depreciationStartBookPeriod.StartDate.Value).Days + 1) > midMonthDay)
								{
									calc.startDepreciationMidPeriodRatio = calc.averagingConvention == FAAveragingConvention.ModifiedPeriod2 ? 0m : 0.5m;
								}
								else
								{
									calc.startDepreciationMidPeriodRatio = 1m;
									calc.startPeriodIsWhole = true;
								}
								break;
							case FABook.midMonthType.NumberOfDays:
								int previousPeriod = calc.depreciationStartPeriod - 1;
								int previousYear = calc.depreciationStartYear;
								if (previousPeriod == 0)
								{
									previousPeriod = calc.depreciationPeriodsInYear;
									previousYear--;
								}
								
								FABookPeriod previousBookPeriod = (FABookPeriod)PXSelect<FABookPeriod, Where<FABookPeriod.periodNbr, Equal<Required<FABookPeriod.periodNbr>>,
																										And<FABookPeriod.finYear, Equal<Required<FABookPeriod.finYear>>,
																										And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>>>.Select(graph, previousPeriod.ToString("00"), previousYear.ToString(), bookID);

								if (((calc.depreciationStartDate - previousBookPeriod.EndDate.Value).Days + 1) > midMonthDay)
								{
									calc.startDepreciationMidPeriodRatio = calc.averagingConvention == FAAveragingConvention.ModifiedPeriod2 ? 0m : 0.5m;
								}
								else
								{
									calc.startDepreciationMidPeriodRatio = 1m;
									calc.startPeriodIsWhole = true;
								}
								break;
						}
						break;
					case FAAveragingConvention.FullPeriod:
						calc.startPeriodIsWhole = true;
						break;
					case FAAveragingConvention.NextPeriod:
						IYearSetup year = book.UpdateGL == true
											  ? (IYearSetup) ((FinYearSetup) PXSelect<FinYearSetup>.Select(graph))
											  : (FABookYearSetup)PXSelect<FABookYearSetup, Where<FABookYearSetup.bookID, Equal<Current<FABook.bookID>>>>.SelectSingleBound(graph, new object[] {book});
						int periodsInYear = calc.depreciationPeriodsInYear;
						if (year.HasAdjustmentPeriod == true && calc.depreciationPeriodsInYear == year.FinPeriods)
						{
							periodsInYear--;
						}
						recoveryStartPeriod = calc.depreciationStartPeriod + 1;
						int recoveryStartYear = calc.depreciationStartYear;
						if (recoveryStartPeriod > periodsInYear)
						{
							recoveryStartPeriod = recoveryStartPeriod % calc.depreciationPeriodsInYear;
							recoveryStartYear++;
						}
						recoveryStartBookPeriod = PXSelect<FABookPeriod, Where<FABookPeriod.periodNbr, Equal<Required<FABookPeriod.periodNbr>>, And<FABookPeriod.finYear, Equal<Required<FABookPeriod.finYear>>,
								And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>>>.Select(graph, recoveryStartPeriod.ToString("00"), recoveryStartYear.ToString(CultureInfo.InvariantCulture), bookID);
						if (recoveryStartBookPeriod == null)
							throw new PXException(Messages.FABookPeriodsNotDefinedFrom, string.Format("{0}-{1}", recoveryStartPeriod.ToString("00"), recoveryStartYear), recoveryStartYear, book.BookCode);
						calc.startPeriodIsWhole = true;
						calc.depreciationStartPeriod = recoveryStartPeriod;
						calc.depreciationStartYear = recoveryStartYear;
						break;
					case FAAveragingConvention.FullQuarter:
						if (calc.depreciationPeriodsInYear == monthsInYear)
						{
							depreciationStartPeriodDivide3 = (decimal)calc.depreciationStartPeriod / (decimal)quarterToMonth;
							depreciationStartQuarter		= (int)Decimal.Ceiling(depreciationStartPeriodDivide3);
							recoveryStartPeriod				= (depreciationStartQuarter - 1) * quarterToMonth + 1;
							recoveryStartBookPeriod			= (FABookPeriod)PXSelect<FABookPeriod, Where<FABookPeriod.periodNbr, Equal<Required<FABookPeriod.periodNbr>>,
																									And<FABookPeriod.finYear, Equal<Required<FABookPeriod.finYear>>,
																									And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>>>.Select(graph, recoveryStartPeriod.ToString("00"), calc.depreciationStartYear.ToString(), bookID);
							if (recoveryStartBookPeriod == null)
								throw new PXException(Messages.FABookPeriodsNotDefinedFrom, string.Format("{0}-{1}", recoveryStartPeriod.ToString("00"), calc.depreciationStartYear), calc.depreciationStartYear, book.BookCode);
							calc.startPeriodIsWhole = true;
						}
						break;
					case FAAveragingConvention.FullYear:
						recoveryStartPeriod = 1;
						recoveryStartBookPeriod = (FABookPeriod)PXSelect<FABookPeriod, Where<FABookPeriod.periodNbr, Equal<Required<FABookPeriod.periodNbr>>,
																										And<FABookPeriod.finYear, Equal<Required<FABookPeriod.finYear>>,
																										And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>>>.Select(graph, recoveryStartPeriod.ToString("00"), calc.depreciationStartYear.ToString(), bookID);
						if (recoveryStartBookPeriod == null)
							throw new PXException(Messages.FABookPeriodsNotDefinedFrom, string.Format("{0}-{1}", recoveryStartPeriod.ToString("00"), calc.depreciationStartYear), calc.depreciationStartYear, book.BookCode);
						calc.startPeriodIsWhole = true;
						break;
					case FAAveragingConvention.HalfPeriod:
						if (calc.depreciationStartDate == calc.depreciationStartBookPeriod.StartDate.Value)
							edgeOfPeriod = true;
						break;
					case FAAveragingConvention.HalfQuarter:
						if (calc.depreciationPeriodsInYear == monthsInYear)
						{
							if (calc.wholeRecoveryPeriods % quarterToMonth != 0)
								throw new PXException(Messages.CanNotUseAveragingConventionWhithRecoveryPeriods, calc.averagingConvention, calc.wholeRecoveryPeriods);

							depreciationStartPeriodDivide3 = (decimal)calc.depreciationStartPeriod / (decimal)quarterToMonth;
							depreciationStartQuarter		= (int)Decimal.Ceiling(depreciationStartPeriodDivide3);
							recoveryStartPeriod				= (depreciationStartQuarter - 1) * quarterToMonth + 1;
							recoveryStartBookPeriod			= (FABookPeriod)PXSelect<FABookPeriod, Where<FABookPeriod.periodNbr, Equal<Required<FABookPeriod.periodNbr>>,
																									And<FABookPeriod.finYear, Equal<Required<FABookPeriod.finYear>>,
																									And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>>>.Select(graph, recoveryStartPeriod.ToString("00"), calc.depreciationStartYear.ToString(), bookID);
							if (recoveryStartBookPeriod == null)
								throw new PXException(Messages.FABookPeriodsNotDefinedFrom, string.Format("{0}-{1}", recoveryStartPeriod.ToString("00"), calc.depreciationStartYear), calc.depreciationStartYear, book.BookCode);
							edgeOfPeriod = (calc.depreciationStartDate == recoveryStartBookPeriod.StartDate.Value);
							recoveryStartPeriod = calc.depreciationStartPeriod;
							if (edgeOfPeriod != true)
								calc.startPeriodIsWhole = (calc.depreciationStartDate == calc.depreciationStartBookPeriod.StartDate.Value);
						}
						break;
					case FAAveragingConvention.HalfYear:
						recoveryStartPeriod = 1;
						recoveryStartBookPeriod = (FABookPeriod)PXSelect<FABookPeriod, Where<FABookPeriod.periodNbr, Equal<Required<FABookPeriod.periodNbr>>,
																										And<FABookPeriod.finYear, Equal<Required<FABookPeriod.finYear>>,
																										And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>>>.Select(graph, recoveryStartPeriod.ToString("00"), calc.depreciationStartYear.ToString(), bookID);
						if (recoveryStartBookPeriod == null)
							throw new PXException(Messages.FABookPeriodsNotDefinedFrom, string.Format("{0}-{1}", recoveryStartPeriod.ToString("00"), calc.depreciationStartYear), calc.depreciationStartYear, book.BookCode);
						edgeOfPeriod = (calc.depreciationStartDate == recoveryStartBookPeriod.StartDate.Value);
						recoveryStartPeriod = calc.depreciationStartPeriod;
						if (edgeOfPeriod != true)
							calc.startPeriodIsWhole = (calc.depreciationStartDate == calc.depreciationStartBookPeriod.StartDate.Value);
						break;
				}

				calc.recoveryYears = GetFinancialYears(calc.wholeRecoveryPeriods, recoveryStartPeriod, calc.depreciationPeriodsInYear, calc.startPeriodIsWhole);
				calc.recoveryEndPeriod = (calc.recoveryEndPeriod = (recoveryStartPeriod + calc.wholeRecoveryPeriods - 1 + (calc.startPeriodIsWhole == false ? 1 : 0)) % calc.depreciationPeriodsInYear) == 0 ? calc.depreciationPeriodsInYear : calc.recoveryEndPeriod;
				int recoveryEndYear = calc.depreciationStartYear + calc.recoveryYears - 1;
				FABookPeriod recoveryEndBookPeriod = (FABookPeriod)PXSelect<FABookPeriod, Where<FABookPeriod.periodNbr, Equal<Required<FABookPeriod.periodNbr>>,
																						And<FABookPeriod.finYear, Equal<Required<FABookPeriod.finYear>>,
																						 And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>>>.Select(graph, calc.recoveryEndPeriod.ToString("00"), recoveryEndYear.ToString(), bookID);

				if (recoveryEndBookPeriod == null)
					throw new PXException(Messages.FABookPeriodsNotDefinedFromTo, FinPeriodIDFormattingAttribute.FormatForError(recoveryStartBookPeriod.FinPeriodID), string.Format("{0}-{1}", calc.recoveryEndPeriod.ToString("00"), recoveryEndYear), recoveryStartBookPeriod.FinYear, recoveryEndYear, book.BookCode);


				DateTime previousEndDate = DateTime.MinValue;
				if (calc.startPeriodIsWhole != true)
				{
					decimal rateFromRecoveryStartDate = (calc.depreciationStartDate - calc.depreciationStartBookPeriod.StartDate.Value).Days / (decimal)(calc.depreciationStartBookPeriod.EndDate.Value - calc.depreciationStartBookPeriod.StartDate.Value).Days;
					int daysFromRecoveryStartDate		= (int)Decimal.Round(rateFromRecoveryStartDate * (decimal)(recoveryEndBookPeriod.EndDate.Value - recoveryEndBookPeriod.StartDate.Value).Days, 0);
					calc.recoveryEndDate = recoveryEndBookPeriod.StartDate.Value.AddDays(daysFromRecoveryStartDate);
					if (edgeOfPeriod != true)
						calc.recoveryEndDate = calc.recoveryEndDate.AddDays(-1);
				}
				else
				{
					calc.recoveryEndDate = recoveryEndBookPeriod.EndDate.Value.AddDays(-1);
				}

				if (calc.averagingConvention == FAAveragingConvention.FullDay)
				{
					calc.wholeRecoveryDays = 0;
					for (int i = calc.depreciationStartYear; i <= recoveryEndYear; i++)
					{
						for (int j = 1; j <= calc.depreciationPeriodsInYear; j++)
						{
							calc.wholeRecoveryDays += GetDaysOnPeriod(graph, calc, calc.depreciationStartDate, calc.recoveryEndDate, i, j, bookID, ref previousEndDate);
						}
					}
				}


				int depreciationStopYear;
				if (calc.depreciationStopDate != null)
				{
					calc.depreciationStopBookPeriod = (FABookPeriod)PXSelect<FABookPeriod, Where<FABookPeriod.startDate, LessEqual<Required<FABookPeriod.startDate>>, And<FABookPeriod.endDate, Greater<Required<FABookPeriod.endDate>>,
																								 And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>>>.Select(graph, calc.depreciationStopDate, calc.depreciationStopDate, bookID);
					if (calc.depreciationStopBookPeriod == null)
						throw new PXException(Messages.FABookPeriodsNotDefinedFromTo, calc.depreciationStartDate.ToShortDateString(), ((DateTime)calc.depreciationStopDate).ToShortDateString(), calc.depreciationStartDate.Year, ((DateTime)calc.depreciationStopDate).Year, book.BookCode);


					depreciationStopYear = int.Parse(calc.depreciationStopBookPeriod.FinYear);
					calc.depreciationYears = depreciationStopYear - calc.depreciationStartYear + 1;
					calc.depreciationStopPeriod = int.Parse(calc.depreciationStopBookPeriod.PeriodNbr);
				}
				else
				{
					calc.depreciationStopDate = calc.recoveryEndDate;
					calc.depreciationStopPeriod = calc.recoveryEndPeriod;
					calc.depreciationYears = calc.recoveryYears;
					depreciationStopYear		= recoveryEndYear;
					calc.depreciationStopBookPeriod = (FABookPeriod)PXSelect<FABookPeriod, Where<FABookPeriod.startDate, LessEqual<Required<FABookPeriod.startDate>>, And<FABookPeriod.endDate, Greater<Required<FABookPeriod.endDate>>,
																								 And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>>>.Select(graph, calc.depreciationStopDate, calc.depreciationStopDate, bookID);
					if (calc.depreciationStopBookPeriod == null)
						throw new PXException(Messages.FABookPeriodsNotDefinedFromTo, calc.depreciationStartDate.ToShortDateString(), ((DateTime)calc.depreciationStopDate).ToShortDateString(), calc.depreciationStartDate.Year, ((DateTime)calc.depreciationStopDate).Year, book.BookCode);
				}

				if (calc.depreciationStopDate > calc.recoveryEndDate)
					throw new PXException("DepreciationToDate can not be greather then RecoveryEndDate!");

				switch (calc.averagingConvention)
				{
					case FAAveragingConvention.ModifiedPeriod:
						switch(midMonthType)
						{
							case FABook.midMonthType.PeriodDaysHalve:
								if (((calc.depreciationStopDate.Value - calc.depreciationStopBookPeriod.StartDate.Value).Days + 1) >= (calc.depreciationStopBookPeriod.EndDate.Value - calc.depreciationStopBookPeriod.StartDate.Value).Days / 2m)
									calc.stopDepreciationMidPeriodRatio = 1m;
								else
									calc.stopDepreciationMidPeriodRatio = 0.5m;
								break;
							case FABook.midMonthType.FixedDay:
								if (((calc.depreciationStopDate.Value - calc.depreciationStopBookPeriod.StartDate.Value).Days + 1) >= midMonthDay)
									calc.stopDepreciationMidPeriodRatio = 1m;
								else
									calc.stopDepreciationMidPeriodRatio = 0.5m;
								break;
							case FABook.midMonthType.NumberOfDays:
								int previousPeriod = calc.depreciationStopPeriod - 1;
								int previousYear	= depreciationStopYear;
								if (previousPeriod == 0)
								{
									previousPeriod = calc.depreciationPeriodsInYear;
									previousYear--;
								}
								
								FABookPeriod previousBookPeriod = (FABookPeriod)PXSelect<FABookPeriod, Where<FABookPeriod.periodNbr, Equal<Required<FABookPeriod.periodNbr>>,
																										And<FABookPeriod.finYear, Equal<Required<FABookPeriod.finYear>>,
																										And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>>>.Select(graph, previousPeriod.ToString("00"), previousYear.ToString(), bookID);

								if (((calc.depreciationStartDate - previousBookPeriod.EndDate.Value).Days + 1) >= midMonthDay)
									calc.stopDepreciationMidPeriodRatio = 1m;
								else
									calc.stopDepreciationMidPeriodRatio = 0.5m;
								break;
						}
						break;
					case FAAveragingConvention.ModifiedPeriod2:
						calc.stopDepreciationMidPeriodRatio = 1m;
						break;
					case FAAveragingConvention.FullQuarter:
					case FAAveragingConvention.HalfQuarter:
						if (calc.depreciationPeriodsInYear == monthsInYear)
						{
							decimal recoveryEndPeriodDivide3 = (decimal)calc.recoveryEndPeriod / (decimal)quarterToMonth;
							int recoveryEndQuarter				= (int)Decimal.Ceiling(recoveryEndPeriodDivide3);
							calc.firstRecoveryEndQuarterPeriod = (recoveryEndQuarter - 1) * quarterToMonth + 1;

							decimal depreciationStopPeriodDivide3 = (decimal)calc.depreciationStopPeriod / (decimal)quarterToMonth;
							int depreciationStopQuarter				= (int)Decimal.Ceiling(depreciationStopPeriodDivide3);
							calc.firstDepreciationStopQuarterPeriod = (depreciationStopQuarter - 1) * quarterToMonth + 1;

							depreciationStartPeriodDivide3 = (decimal)calc.depreciationStartPeriod / (decimal)quarterToMonth;
							int depreciationStartPeriodQuarter	= (int)Decimal.Ceiling(depreciationStartPeriodDivide3);
							calc.lastDepreciationStartQuarterPeriod = depreciationStartPeriodQuarter * quarterToMonth;
						}
						break;
				}
			}

			public static int GetFinancialYears(int wholeRecoveryPeriods, int startPeriod, int depreciationPeriodsInYear, bool startPeriodIsWhole)
			{
				if (wholeRecoveryPeriods == 0 || startPeriod == 0) return 0;
				decimal financialYearsToCalendar = (decimal)(wholeRecoveryPeriods + startPeriod - 1 + (startPeriodIsWhole == false ? 1 : 0)) / (decimal)depreciationPeriodsInYear;
				int financialYears = (int)Decimal.Ceiling(financialYearsToCalendar);
				return financialYears;
			}

			public static int GetDaysOnPeriod(PXGraph graph, DateTime recoveryStartDate, DateTime? recoveryEndDate, int currYear, int currPeriod, int? bookID, ref DateTime previousEndDate)
			{
				return GetDaysOnPeriod(graph, graph as IDepreciationCalculation, recoveryStartDate, recoveryEndDate, currYear, currPeriod, bookID, ref previousEndDate);
			}

			public static int GetDaysOnPeriod(PXGraph graph, IDepreciationCalculation calc, DateTime recoveryStartDate, DateTime? recoveryEndDate, int currYear, int currPeriod, int? bookID, ref DateTime previousEndDate)
			{
				DateTime? existPeriodStartDate = null;
				DateTime? existPeriodEndDate = null;

				FABook book = PXSelect<FABook, Where<FABook.bookID, Equal<Required<FABook.bookID>>>>.Select(graph, bookID);

				FABookPeriod existBookPeriod = (FABookPeriod)PXSelect<FABookPeriod, Where<FABookPeriod.periodNbr, Equal<Required<FABookPeriod.periodNbr>>,
																								And<FABookPeriod.finYear, Equal<Required<FABookPeriod.finYear>>,
																								And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>>>>>.Select(graph, currPeriod.ToString("00"), currYear.ToString(), bookID);
				if (existBookPeriod == null)
					throw new PXException(Messages.FABookPeriodsNotDefined, book.BookCode, currYear);
				existPeriodStartDate = existBookPeriod.StartDate.Value;
				existPeriodEndDate	 = existBookPeriod.EndDate.Value;

				int recoveryDays = 0;
				if (recoveryStartDate	 <= existPeriodEndDate &&
					existPeriodStartDate <= recoveryEndDate)
				{
					DateTime? periodStartDate	= existPeriodStartDate	> recoveryStartDate ? existPeriodStartDate	: recoveryStartDate;
					DateTime? periodEndDate		= existPeriodEndDate	< recoveryEndDate	? existPeriodEndDate	: recoveryEndDate;
					recoveryDays  = (periodEndDate.Value - periodStartDate.Value).Days;
					recoveryDays += (periodStartDate == previousEndDate) ?
										(periodEndDate == recoveryEndDate) ? 1 : 0 :
										(previousEndDate == DateTime.MinValue) ? 0 : 1;
					previousEndDate = periodEndDate.Value;
				}

				return recoveryDays;
			}

			#region IDepreciationCalculation Members
			public decimal depreciationBasis
			{
				get;
				set;
			}
			public decimal accumulatedDepreciation
			{
				get;
				set;
			}
			public int depreciationStartYear
			{
				get;
				set;
			}
			public FABookPeriod depreciationStopBookPeriod
			{
				get;
				set;
			}

			public FABookPeriod depreciationStartBookPeriod
			{
				get;
				set;
			}

			public int depreciationStartPeriod
			{
				get;
				set;
			}

			public int depreciationStopPeriod
			{
				get;
				set;
			}

			public int recoveryEndPeriod
			{
				get;
				set;
			}

			public int firstRecoveryEndQuarterPeriod
			{
				get;
				set;
			}

			public int firstDepreciationStopQuarterPeriod
			{
				get;
				set;
			}

			public int lastDepreciationStartQuarterPeriod
			{
				get;
				set;
			}

			public DateTime recoveryEndDate
			{
				get;
				set;
			}

			public int recoveryYears
			{
				get;
				set;
			}

			public int depreciationYears
			{
				get;
				set;
			}

			public decimal startDepreciationMidPeriodRatio
			{
				get;
				set;
			}

			public decimal stopDepreciationMidPeriodRatio
			{
				get;
				set;
			}

			public DateTime depreciationStartDate
			{
				get;
				set;
			}

			public DateTime? depreciationStopDate
			{
				get;
				set;
			}

			public int wholeRecoveryDays
			{
				get;
				set;
			}

			public bool startPeriodIsWhole
			{
				get;
				set;
			}

			public string averagingConvention
			{
				get;
				set;
			}

			public int wholeRecoveryPeriods
			{
				get;
				set;
			}

			public int depreciationPeriodsInYear
			{
				get;
				set;
			}

			public FADepreciationMethod depreciationMethod
			{
				get;
				set;
			}
			public int yearOfUsefulLife
			{
				get;
				set;
			}
			#endregion
		}

		public interface IDepreciationCalculation
		{
			decimal depreciationBasis { get; set; }
			decimal accumulatedDepreciation { get; set; }
			int depreciationStartYear { get; set; }
			FABookPeriod depreciationStopBookPeriod { get; set; }
			FABookPeriod depreciationStartBookPeriod { get; set; }
			int depreciationStartPeriod { get; set; }
			int depreciationStopPeriod { get; set; }
			int recoveryEndPeriod { get; set; }
			int firstRecoveryEndQuarterPeriod { get; set; }
			int firstDepreciationStopQuarterPeriod { get; set; }
			int lastDepreciationStartQuarterPeriod { get; set; }
			DateTime recoveryEndDate { get; set; }
			int recoveryYears { get; set; }
			int depreciationYears { get; set; }
			decimal startDepreciationMidPeriodRatio { get; set; }
			decimal stopDepreciationMidPeriodRatio { get; set; }
			DateTime depreciationStartDate { get; set; }
			DateTime? depreciationStopDate { get; set; }
			int wholeRecoveryDays { get; set; }
			bool startPeriodIsWhole { get; set; }
			FADepreciationMethod depreciationMethod { get; set; }
			string averagingConvention { get; set; }
			int wholeRecoveryPeriods { get; set; }
			int depreciationPeriodsInYear { get; set; }
			int yearOfUsefulLife { get; set; }
		}
}
