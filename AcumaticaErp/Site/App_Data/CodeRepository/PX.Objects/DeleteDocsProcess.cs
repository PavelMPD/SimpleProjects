using System.Collections;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.FA
{
    public class DeleteDocsProcess : PXGraph<DeleteDocsProcess>
    {
        public PXCancel<FARegister> Cancel;
        [PXFilterable]
        public PXProcessing<FARegister, Where<FARegister.released, NotEqual<True>>> Docs;
        public PXSelect<FATran> Transactions;
		public PXSelect<FAAccrualTran> Additions;

        protected virtual void FARegister_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            Docs.SetProcessCaption(Messages.DeleteProc);
            Docs.SetProcessAllCaption(Messages.DeleteAllProc);
            Docs.SetProcessDelegate(delegate(List<FARegister> list)
                                        {
                                            foreach (FARegister register in list)
                                            {
                                                FARegister copy = (FARegister) Docs.Cache.CreateCopy(register);
                                                Docs.Delete(copy);
                                            }
                                            Actions.PressSave();
                                        });
        }

        public PXAction<FARegister> viewDocument;
        [PXUIField(DisplayName = Messages.ViewDocument, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = true)]
        [PXButton]
        public virtual IEnumerable ViewDocument(PXAdapter adapter)
        {
            TransactionEntry graph = CreateInstance<TransactionEntry>();
            graph.Document.Current = graph.Document.Search<FARegister.refNbr>(Docs.Current.RefNbr);
            throw new PXRedirectRequiredException(graph, "FATransactions") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

    }
}
