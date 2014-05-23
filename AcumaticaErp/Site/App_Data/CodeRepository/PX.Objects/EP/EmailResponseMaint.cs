using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	//[PXGraphName(Messages.EmailResponseMaint, typeof(EPActivity))]
	public class EmailResponseMaint : PXGraph<EmailResponseMaint>
	{
		public PXSelect<EPActivity>
			Message;
	}
}
