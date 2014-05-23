using System;
using System.Collections;
using System.Linq;
using PX.Data;

namespace PX.Objects.IN
{
	public class INReplenishmentPolicyMaint : PXGraph<INReplenishmentPolicyMaint, INReplenishmentPolicy>
	{
		public PXSelect<INReplenishmentPolicy> Policies;

		[PXImport(typeof (INReplenishmentPolicy))] 
		public PXSelect<INReplenishmentSeason, Where<INReplenishmentSeason.replenishmentPolicyID, Equal<Optional<INReplenishmentPolicy.replenishmentPolicyID>>>> Seasons;

		protected virtual void INReplenishmentPolicy_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			INReplenishmentPolicy row = (INReplenishmentPolicy) e.Row;
			if (row == null) return;
		}

		protected virtual void INReplenishmentPolicy_ReplenishmentSource_FieldUpdated(PXCache sender,
		                                                                              PXFieldUpdatedEventArgs e)
		{
			INReplenishmentPolicy row = (INReplenishmentPolicy) e.Row;
			if (row == null) return;
		}

		protected virtual void INReplenishmentSeason_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			INReplenishmentSeason row = (INReplenishmentSeason)e.NewRow;
			INReplenishmentSeason old_row = (INReplenishmentSeason)e.Row;

			if (row.StartDate != null && row.EndDate != null &&
				(!sender.ObjectsEqual<INReplenishmentSeason.startDate>(row, old_row) ||
				 !sender.ObjectsEqual<INReplenishmentSeason.endDate>(row, old_row)) &&
				!ValidateOverlap(sender, row))
			{
				if (!sender.ObjectsEqual<INReplenishmentSeason.startDate>(row, old_row))
				{
					sender.RaiseExceptionHandling<INReplenishmentSeason.startDate>(row, row.StartDate, new PXSetPropertyException(Messages.PeriodsOverlap));
				}

				if (!sender.ObjectsEqual<INReplenishmentSeason.endDate>(row, old_row))
				{
					sender.RaiseExceptionHandling<INReplenishmentSeason.endDate>(row, row.EndDate, new PXSetPropertyException(Messages.PeriodsOverlap));
				}
				e.Cancel = true;
			}
		}

		protected virtual void INReplenishmentSeason_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			INReplenishmentSeason row = (INReplenishmentSeason)e.Row;			
			if (row.StartDate != null && row.EndDate != null &&!ValidateOverlap(sender, row))
			{				
				sender.RaiseExceptionHandling<INReplenishmentSeason.endDate>(row, row.EndDate, new PXSetPropertyException(Messages.PeriodsOverlap));
				e.Cancel = true;				
			}
		}

		protected virtual bool ValidateOverlap(PXCache sender, INReplenishmentSeason row)
		{
			foreach (INReplenishmentSeason season in this.Seasons.Select())
			{
				if (season.SeasonID == row.SeasonID || row.StartDate > season.EndDate || row.EndDate < season.StartDate) continue;
				return false;				
			}
			return true;
		}

		protected virtual void INReplenishmentSeason_Factor_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((decimal) e.NewValue == 0)
			{
				throw new PXSetPropertyException(EP.Messages.ValueMustBeGreaterThanZero);
			}
		}
	}
}
