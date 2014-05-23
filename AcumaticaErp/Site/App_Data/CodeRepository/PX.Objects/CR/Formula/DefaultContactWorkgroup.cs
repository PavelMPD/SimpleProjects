using System.Collections.Generic;
using PX.Common;
using PX.Data;
using PX.TM;

namespace PX.Objects.CR
{
	class DefaultContactWorkgroup<ClassID> : BqlFormula<ClassID>, IBqlOperand
		where ClassID: IBqlField
	{
		public override void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			string classID = (string)Calculate<ClassID>(cache, item);
			CRContactClass cls = PXSelect<CRContactClass, Where<CRContactClass.classID, Equal<Required<CRContactClass.classID>>>>.Select(cache.Graph, classID);
			if (cls == null) return;

			PXSelectBase<EPCompanyTreeMember> cmd = new PXSelectJoin<EPCompanyTreeMember, 
				InnerJoin<EPCompanyTreeH, On<EPCompanyTreeMember.workGroupID, Equal<EPCompanyTreeH.workGroupID>>>, 
				Where<EPCompanyTreeMember.userID, Equal<Required<EPCompanyTreeMember.userID>>>>(cache.Graph);
			if (cls.DefaultWorkgroupID != null && cls.OwnerIsCreatedUser != true)
			{
				value = cls.DefaultWorkgroupID;
			}
			else if (cls.DefaultWorkgroupID != null && cls.OwnerIsCreatedUser == true)
			{
				cmd.WhereAnd<Where<EPCompanyTreeH.parentWGID, Equal<Required<EPCompanyTreeH.parentWGID>>>>();
				EPCompanyTreeMember m = cmd.SelectSingle(cache.Graph.Accessinfo.UserID, cls.DefaultWorkgroupID);
				value = m.With(_=>_.WorkGroupID);
			}
			else if (cls.DefaultWorkgroupID == null && cls.OwnerIsCreatedUser == true && cls.DefaultOwnerWorkgroup == true)
			{
				EPCompanyTreeMember m = cmd.SelectSingle(cache.Graph.Accessinfo.UserID);
				value = m.With(_ => _.WorkGroupID);
			}
			else
			{
				value = null;
			}
		}
	}
}
