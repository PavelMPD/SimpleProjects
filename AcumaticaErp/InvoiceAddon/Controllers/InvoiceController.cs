using InvoiceAddon.DAC.CF;
using PX.Data;

namespace InvoiceAddon.Controllers
{
    public class InvoiceController : PXGraph<InvoiceController>
    {
        public PXSelectReadonly<CFIMInvoice> Invoices;

        //protected virtual void InvoiveSave(PXCache sender, PXFieldVerifyingEventArgs e)
        //{
            
        //}

        //protected virtual void InvoiveDelete(PXCache sender, PXFieldVerifyingEventArgs e)
        //{

        //}
    }
}
