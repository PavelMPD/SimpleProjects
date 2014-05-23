using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.CM;


namespace PX.Objects.GL
{
    [PX.Objects.GL.TableAndChartDashboardType]
    public class VoucherRelease : PXGraph<VoucherRelease>
    {
        public PXCancel<GLDocBatch> Cancel;

        [PXFilterable]
        public PXProcessing<GLDocBatch, Where<GLDocBatch.hold, Equal<boolFalse>,
                                        And<GLDocBatch.released, Equal<boolFalse>>>> Documents;


        public static void ReleaseVoucher(GLBatchDocRelease graph, GLDocBatch batch)
        {
            graph.ReleaseBatchProc(batch, false);
        }

        public VoucherRelease()
        {
            Documents.SetProcessDelegate<GLBatchDocRelease>(ReleaseVoucher);
            Documents.SetProcessCaption(Messages.ProcRelease);
            Documents.SetProcessAllCaption(Messages.ProcReleaseAll);
        }
    }
}
