using System;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.CM
{
	public class CurrencyMaint : PXGraph<CurrencyMaint, Currency>
	{
		public PXSelect<Currency, Where<Currency.curyID, NotEqual<Current<Company.baseCuryID>>>> CuryRecords;
		public PXSelect<Currency, Where<Currency.curyID, Equal<Current<Currency.curyID>>>> CurrentCury;

		public PXSetup<Company> company;
				
		public CurrencyMaint()
		{
			Company setup = company.Current;
			if (string.IsNullOrEmpty(setup.BaseCuryID))
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Company), CS.Messages.BranchMaint);
			}
		}
		
		protected virtual void Currency_DecimalPlaces_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			cache.SetValue(e.Row, typeof(Currency.decimalPlaces).Name, (short)2);
		}
	}
}
