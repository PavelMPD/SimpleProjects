using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.SO;

namespace PX.Objects.CS
{
	public class CSBoxMaint : PXGraph<CSBoxMaint>
	{

        public PXSelectJoin<CSBox, CrossJoin<PX.Objects.IN.INSetup>> Records;
        public PXSavePerRow<CSBox> Save;
        public PXCancel<CSBox> Cancel;
    }
}
