using PX.Data;

namespace PX.Objects.EP
{
	public class EPPayGroupMaint : PXGraph<EPPayGroupMaint>
	{
		public PXSelect<EPPayGroup> PayGroups;
        public PXSavePerRow<EPPayGroup> Save;
        public PXCancel<EPPayGroup> Cancel;

        //TODO: Remove when it is fixed verification for min-max values in the numeric attributes
        protected virtual void EPPayGroup_HoursPerPayPeriod_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (e.NewValue == null || (decimal)(e.NewValue) <= 0m)
            {
                throw new PXSetPropertyException(Messages.ValueMustBeGreaterThanZero);
            }
        }
	}
}
