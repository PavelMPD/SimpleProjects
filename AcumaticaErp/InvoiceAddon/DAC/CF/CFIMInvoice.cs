using System;
using PX.Data;

namespace InvoiceAddon.DAC.CF
{
    [SerializableAttribute()]
    public class CFIMInvoice : IBqlTable
    {
        public abstract class invoiceID : IBqlField {}
        public abstract class code : IBqlField { }
        public abstract class name : IBqlField { }
        public abstract class status : IBqlField { }
        
        [PXDBIdentity(IsKey = true)]
        public virtual Int32? InvoceID { get; set; }

        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Invoice Code")]
        public virtual String Code { get; set; }

        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Invoice Name")]
        public virtual String Name { get; set; }

        [PXDBInt()]
        [PXUIField(DisplayName = "Invoice Status")]
        public virtual Int32? Status { get; set; }
    }
}
