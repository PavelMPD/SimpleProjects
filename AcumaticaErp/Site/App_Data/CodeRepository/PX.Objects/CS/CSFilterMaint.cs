using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Compilation;
using PX.Data;
using PX.SM;

namespace PX.Objects.CS
{
	public class CSFilterMaint : FilterMaint
	{
		[PXDefault(0)]
		[PXDBInt]
		[PXUIField(DisplayName = "Operator")]
		[PXIntList(new[] { 0, 1 }, new[] { Messages.OperatorAnd, Messages.OperatorOr })]
		protected virtual void FilterRow_Operator_CacheAttached(PXCache sender)
		{

		}
	}
}
