using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.CM;

namespace PX.Objects.GL
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class BatchPost : PXGraph<BatchPost>
	{
        #region Cache Attached Events
        #region Batch
        #region BatchNbr

        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        [PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<Optional<Batch.module>>, And<Batch.status, Equal<statusU>>>>))]
        [PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 3)]
        protected virtual void Batch_BatchNbr_CacheAttached(PXCache sender)
        {
        }

        #endregion
        #region ControlTotal
        
        [PXDBBaseCury(typeof(Batch.ledgerID))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Control Total")]
        protected virtual void Batch_ControlTotal_CacheAttached(PXCache sender)
        {
        }
        #endregion        
        #endregion
        #endregion

        public PXCancel<Batch> Cancel;
		[PXFilterable]
		public PXProcessing<Batch, Where<Batch.status, Equal<statusU>>> BatchList;
        public PXAction<Batch> viewBatch;

        [PXUIField(DisplayName = Messages.ViewBatch, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]        
        public virtual IEnumerable ViewBatch(PXAdapter adapter)
        {
            if (this.BatchList.Current != null)
            {
                JournalEntry graph = PXGraph.CreateInstance<JournalEntry>();
                graph.BatchModule.Current = graph.BatchModule.Search<Batch.batchNbr>(this.BatchList.Current.BatchNbr, this.BatchList.Current.Module);
                if (graph.BatchModule.Current != null)
                {
                    throw new PXRedirectRequiredException(graph, true, "ViewBatch"){Mode = PXBaseRedirectException.WindowMode.NewWindow};
                }
            }
            return adapter.Get();
        }

        public BatchPost()
        {
			GLSetup setup = GLSetup.Current;
			BatchList.SetProcessDelegate<PostGraph>(
                delegate(PostGraph pg, Batch batch)
                {
                    pg.Clear();
                    pg.PostBatchProc(batch);
                }
            );
			BatchList.SetProcessCaption(Messages.ProcPost);
			BatchList.SetProcessAllCaption(Messages.ProcPostAll);
			PXNoteAttribute.ForcePassThrow<Batch.noteID>(BatchList.Cache);
        }
	
		public PXSetup<GLSetup> GLSetup;
	}
}
