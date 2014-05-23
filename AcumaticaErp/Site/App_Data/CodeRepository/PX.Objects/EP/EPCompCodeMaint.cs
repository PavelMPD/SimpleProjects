using PX.Data;

namespace PX.Objects.EP
{
	public class EPCompCodeMaint : PXGraph<EPCompCodeMaint>
	{
		public PXSelect<EPCompensationCode> CompensationCodes;
        public PXSavePerRow<EPCompensationCode> Save;
        public PXCancel<EPCompensationCode> Cancel;
    }
}
