using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.FA
{
	public class DepreciationMethodMaint : PXGraph<DepreciationMethodMaint, FADepreciationMethod>
	{
		#region Cache Attached

		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXSelector(typeof(Search<FADepreciationMethod.methodCD, Where<FADepreciationMethod.isTableMethod, Equal<boolFalse>>>))]
		[PXUIField(DisplayName = "Depreciation Method ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PX.Data.EP.PXFieldDescription]
		public void FADepreciationMethod_MethodCD_CacheAttached(PXCache cache) { }
		
		[PXDBString(1, IsFixed = true)]
		[PXDefault(FARecordType.BothType, PersistingCheck = PXPersistingCheck.Nothing)]
		[FARecordType.MethodList]
		public void FADepreciationMethod_RecordType_CacheAttached(PXCache cache) { }

		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Table Method", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public void FADepreciationMethod_IsTableMethod_CacheAttached(PXCache cache) { }
		#endregion

		#region Selects Declaration
		public PXSelect<FADepreciationMethod, Where<FADepreciationMethod.isTableMethod, Equal<False>>> Method;
		public PXSetup<FASetup> FASetup;
		#endregion

		#region Ctor
		public DepreciationMethodMaint()
		{
		}
		#endregion

		#region Events


		protected virtual void FADepreciationMethod_UsefulLife_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FADepreciationMethod meth = (FADepreciationMethod)e.Row;
            if(meth == null) return;
			meth.RecoveryPeriod = (int)((meth.UsefulLife ?? 0) * 12);
        }
		#endregion

	}

	public class DepreciationTableMethodMaint : PXGraph<DepreciationTableMethodMaint, FADepreciationMethod>
	{
		#region Cache Attached
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXSelector(typeof(Search<FADepreciationMethod.methodCD, Where<FADepreciationMethod.isTableMethod, Equal<boolTrue>>>))]
		[PXUIField(DisplayName = "Depreciation Method ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PX.Data.EP.PXFieldDescription]
		public void FADepreciationMethod_MethodCD_CacheAttached(PXCache cache) { }
		#endregion

		#region Selects Declaration
		public PXSelect<FADepreciationMethod, Where<FADepreciationMethod.isTableMethod, Equal<True>>> Method;
		public PXSelect<FADepreciationMethodLines, Where<FADepreciationMethodLines.methodID, Equal<Current<FADepreciationMethod.methodID>>>> details;
		public PXSetup<FASetup> FASetup;
		#endregion

		#region Ctor
		public DepreciationTableMethodMaint()
		{
			FASetup setup = FASetup.Current;
			details.Cache.AllowInsert = false;
		}
		#endregion

		#region Events
		protected virtual void FADepreciationMethod_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			FADepreciationMethod method = (FADepreciationMethod)e.Row;
			if (method == null) return;

			PXUIFieldAttribute.SetEnabled<FADepreciationMethod.parentMethodID>(sender, method, method.RecordType == FARecordType.AssetType);
			PXUIFieldAttribute.SetEnabled<FADepreciationMethod.averagingConvPeriod>(sender, method, method.AveragingConvention != FAAveragingConvention.FullYear);
		}

		protected virtual void AdjustMethodLines(FADepreciationMethod meth)
		{
			if (meth.UsefulLife == null || meth.AveragingConvention == null || meth.AveragingConvPeriod == null) return;
			int periodFactor;
			if (meth.AveragingConvention == FAAveragingConvention.FullYear ||
				 meth.AveragingConvention == FAAveragingConvention.HalfYear)
			{
				periodFactor = 12;

			}
			else if (meth.AveragingConvention == FAAveragingConvention.FullQuarter ||
				 meth.AveragingConvention == FAAveragingConvention.HalfQuarter)
			{
				periodFactor = 3;
			}
			else
			{
				periodFactor = 1;
			}
			int offsetPeriods = ((int)meth.AveragingConvPeriod - 1) * periodFactor;
			int periods = (int)Math.Ceiling((decimal)meth.UsefulLife * 12 + offsetPeriods);
			if (meth.AveragingConvention == FAAveragingConvention.HalfYear ||
				 meth.AveragingConvention == FAAveragingConvention.HalfQuarter ||
				 meth.AveragingConvention == FAAveragingConvention.HalfPeriod ||
				 meth.AveragingConvention == FAAveragingConvention.ModifiedPeriod ||
				 meth.AveragingConvention == FAAveragingConvention.ModifiedPeriod2 ||
				 meth.AveragingConvention == FAAveragingConvention.NextPeriod
				)
			{
				periods++;
			}

			int years = (int)Math.Ceiling(periods / 12d);
			List<FADepreciationMethodLines> lines = new List<FADepreciationMethodLines>();
			foreach (FADepreciationMethodLines line in PXSelect<FADepreciationMethodLines, Where<FADepreciationMethodLines.methodID, Equal<Current<FADepreciationMethod.methodID>>>>.Select(this))
			{
				lines.Add(line);
			}
			if (years != lines.Count)
			{
				decimal newTotalRate = 0m;
				int count = 0;
				FADepreciationMethodLines lastLine = null;
				lines.Sort(delegate(FADepreciationMethodLines line1, FADepreciationMethodLines line2)
				{
					return (line1.Year ?? 0m).CompareTo(line2.Year ?? 0m);
				});
				foreach (FADepreciationMethodLines line in lines)
				{
					count++;
					if (count > years)
					{
						details.Delete((FADepreciationMethodLines)details.Cache.CreateCopy(line));
					}
					else
					{
						lastLine = (FADepreciationMethodLines)details.Cache.CreateCopy(line);
						newTotalRate += line.RatioPerYear ?? 0m;
					}
				}
				if (count > years && lastLine != null)
				{
					lastLine.RatioPerYear += 1m - newTotalRate;
					details.Update(lastLine);
				}
				else
				{
					for (int year_num = count + 1; year_num <= years; year_num++)
					{
						details.Insert(new FADepreciationMethodLines { Year = year_num, RatioPerYear = 0m });
					}
				}
			}

		}

		protected virtual void FADepreciationMethod_AveragingConvention_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FADepreciationMethod meth = (FADepreciationMethod)e.Row;
			if (meth == null) return;
			if (Method.Current.AveragingConvention == FAAveragingConvention.FullYear)
			{
				sender.SetDefaultExt<FADepreciationMethod.recoveryPeriod>(Method.Current);
			}
			AdjustMethodLines(meth);
		}

		protected virtual void FADepreciationMethod_UsefulLife_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FADepreciationMethod meth = (FADepreciationMethod)e.Row;
			if (meth == null) return;
			meth.RecoveryPeriod = (int)((meth.UsefulLife ?? 0) * 12);
			AdjustMethodLines(meth);
		}

		protected virtual void FADepreciationMethod_AveragingConvPeriod_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FADepreciationMethod meth = (FADepreciationMethod)e.Row;
			if (meth == null) return;
			AdjustMethodLines(meth);
		}

		protected virtual void FADepreciationMethod_RecordType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			FADepreciationMethod meth = (FADepreciationMethod)e.Row;
			if (meth == null) return;
			if (meth.RecordType == FARecordType.AssetType)
			{
				PXDefaultAttribute.SetPersistingCheck<FADepreciationMethod.parentMethodID>(sender, meth, PXPersistingCheck.NullOrBlank);
				PXUIFieldAttribute.SetRequired<FADepreciationMethod.parentMethodID>(sender, true);
			}
			else
			{
				PXDefaultAttribute.SetPersistingCheck<FADepreciationMethod.parentMethodID>(sender, meth, PXPersistingCheck.Nothing);
				PXUIFieldAttribute.SetRequired<FADepreciationMethod.parentMethodID>(sender, false);
			}
		}

		protected virtual void FADepreciationMethod_AveragingConvPeriod_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.NewValue == null)
				e.Cancel = true;
			else
			{
				short newValue = (short)e.NewValue;
				if (newValue < 1)
				{
					throw new PXSetPropertyException(CS.Messages.Entry_GE, PXErrorLevel.Error, 1);
				}
				switch (Method.Current.AveragingConvention)
				{
					case FAAveragingConvention.HalfPeriod:
						if (newValue > 12)
						{
							throw new PXSetPropertyException(CS.Messages.Entry_LE, PXErrorLevel.Error, 12);
						}
						break;
					case FAAveragingConvention.HalfQuarter:
						if (newValue > 4)
						{
							throw new PXSetPropertyException(CS.Messages.Entry_LE, PXErrorLevel.Error, 4);
						}
						break;
					case FAAveragingConvention.HalfYear:
						if (newValue > 2)
						{
							throw new PXSetPropertyException(CS.Messages.Entry_LE, PXErrorLevel.Error, 2);
						}
						break;
				}
			}
		}
		#endregion

	}

}
