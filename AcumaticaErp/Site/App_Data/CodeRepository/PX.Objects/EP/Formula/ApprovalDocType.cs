using System.Collections.Generic;
using System.Web.Compilation;
using PX.Data;

namespace PX.Objects.EP
{
	public class ApprovalDocType<EntityType> : BqlFormula<EntityType>
		where EntityType : IBqlOperand
	{
		public override void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			string entityType = (string)Calculate<EntityType>(cache, item);
			value = EntityHelper.GetFriendlyEntityName(BuildManager.GetType(entityType, false, true));
		}
	}
}
