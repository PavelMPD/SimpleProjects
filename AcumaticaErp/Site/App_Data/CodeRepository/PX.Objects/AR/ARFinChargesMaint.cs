using System;
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.AR
{
	public class ARFinChargesMaint : PXGraph<ARFinChargesMaint>
	{
        public PXSavePerRow<ARFinCharge> Save;
		public PXCancel<ARFinCharge> Cancel;
		public PXSelect<ARFinCharge> ARFinChargesList;

		protected virtual void ARFinCharge_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			ARFinCharge c = PXSelect<ARFinCharge, Where<ARFinCharge.finChargeID, Equal<Required<ARFinCharge.finChargeID>>>>.SelectWindowed(this, 0, 1, ((ARFinCharge)e.Row).FinChargeID);
			if (c != null)
			{
                cache.RaiseExceptionHandling<ARFinCharge.finChargeID>(e.Row, ((ARFinCharge)e.Row).FinChargeID, new PXException(Messages.RecordAlreadyExists));
				e.Cancel = true;
			}
		}

		protected virtual void ARFinCharge_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			ARFinCharge fin = e.Row as ARFinCharge;

			if (fin == null)
			{
				return;
			}

			//SWUIFieldAttribute.SetEnabled<ARFinCharge.finChargeAmount>(cache, fin, (bool)!(fin.PercentFlag));
			PXUIFieldAttribute.SetEnabled<ARFinCharge.finChargePercent>(cache, fin, (bool)(fin.PercentFlag));
			PXUIFieldAttribute.SetEnabled<ARFinCharge.minFinChargeAmount>(cache, fin, (bool)(fin.MinFinChargeFlag));
		}

		public ARFinChargesMaint() 
		{
			GLSetup setup = GLSetup.Current;
		}

		public PXSetup<GLSetup> GLSetup;
	}
}
