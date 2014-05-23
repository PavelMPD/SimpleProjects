using PX.Data;
using System.Linq;

namespace PX.Objects.CR
{
	public sealed class LastActivity<TargetField, ReturnField> : IBqlUnboundAggregateCalculator
		where TargetField : IBqlField
		where ReturnField : IBqlField
	{
		private static object CalcFormula(IBqlCreator formula, PXCache cache, object item)
		{
			object value = null;
			bool? result = null;

			BqlFormula.Verify(cache, item, formula, ref result, ref value);

			return value;
		}

		public object Calculate(PXCache cache, object row, IBqlCreator formula, object[] records, int digit)
		{
			if (records.Length < 1 || !(records[0] is EPActivity)) return null;

			return
				cache.GetValue<ReturnField>(
					records.Where(a => ((bool?) CalcFormula(formula, cache, a) == true))
					       .OrderBy(a => ((EPActivity) a).CreatedDateTime)
					       .LastOrDefault());
		}

		public object Calculate(PXCache cache, object row, object oldrow, IBqlCreator formula, int digit)
		{
			return null;
		}
	}
}
