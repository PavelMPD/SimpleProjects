using InvoiceAddon.DAC.CF;
using PX.Data;

namespace InvoiceAddon.Controllers
{
    public class InvoiceController : PXGraph<InvoiceController, CFIMInvoice>
    {
        public PXSelect<CFIMInvoice, Where<CFIMInvoice.code, Equal<Optional<CFIMInvoice.code>>>> Invoices;
        public PXAction<CFIMInvoice> insert;
        public PXAction<CFIMInvoice> save;
    }
}
