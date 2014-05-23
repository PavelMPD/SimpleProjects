using System;
using PX.Data;

namespace PX.Objects.CM
{
	public class CurrencyRateTypeMaint : PXGraph<CurrencyRateTypeMaint>
    {
        public PXSavePerRow<CurrencyRateType> Save;
		public PXCancel<CurrencyRateType> Cancel;
		[PXImport(typeof(CurrencyRateType))]
		public PXSelect<CurrencyRateType> CuryRateTypeRecords;
    }
}
