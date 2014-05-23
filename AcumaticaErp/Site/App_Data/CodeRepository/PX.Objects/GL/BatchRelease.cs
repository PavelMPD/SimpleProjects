using System;
using System.Text;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.CM;
using System.Linq;

namespace PX.Objects.GL
{
	[PX.Objects.GL.TableAndChartDashboardType]
	public class BatchRelease : PXGraph<BatchRelease>
	{
        #region Type Override events

        #region BatchNbr
        
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXDefault()]
        [PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<Optional<Batch.module>>, And<Batch.status, Equal<statusB>>>>))]
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

        public PXCancel<Batch> Cancel;
		[PXFilterable]
		public PXProcessing<Batch, 
			Where<Batch.released, Equal<boolFalse>, 
			And<Batch.scheduled, Equal<boolFalse>, 
			And<Batch.voided,Equal<boolFalse>,
            And<Batch.hold, Equal<boolFalse>>>>>> BatchList;
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

        public BatchRelease()
		{
			GLSetup setup = GLSetup.Current;
            BatchList.SetProcessDelegate<PostGraph>(ReleaseBatch);
            BatchList.SetProcessCaption(Messages.ProcRelease);
            BatchList.SetProcessAllCaption(Messages.ProcReleaseAll);
			PXNoteAttribute.ForcePassThrow<Batch.noteID>(BatchList.Cache);
			gLEditReport.SetEnabled(false);

		}

		public PXSetup<GLSetup> GLSetup;
		
		public static void ReleaseBatch(PostGraph pg, Batch batch)
        {
            pg.Clear();
            pg.ReleaseBatchProc(batch);
            if ((bool)batch.AutoReverse)
            {
                Batch copy = pg.ReverseBatchProc(batch);
                if (pg.AutoPost)
                {
                    pg.PostBatchProc(batch);
                }
                pg.Clear();
				pg.TimeStamp = copy.tstamp;
                pg.ReleaseBatchProc(copy);
                if (pg.AutoPost)
                {
                    pg.PostBatchProc(copy);
                }
            }
            else if (pg.AutoPost)
            {
                pg.PostBatchProc(batch);
            }
        }

		public PXAction<Batch> gLEditReport;
		[PXUIField(DisplayName = Messages.ViewGLEdit, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable GLEditReport(PXAdapter adapter)
		{
			if (this.BatchList.Current != null)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				int i = 0;
				foreach (Batch batch in adapter.Get())
				{
					if (batch.Selected == true)
					{
						StringBuilder BatchNbr = new StringBuilder("Batch.BatchNbr");
						BatchNbr.Append(Convert.ToString(i));

						parameters[BatchNbr.ToString()] = batch.BatchNbr;
						i++;
					}
				}

				if (parameters.Count > 0)
				{
					parameters["LedgerID"] = null;
					parameters["PeriodFrom"] = null;
					parameters["PeriodTo"] = null;
					parameters["BranchID"] = null;
					throw new PXReportRequiredException(parameters, "GL610500", PXBaseRedirectException.WindowMode.New, "GL Edit");
				}
			}
			return BatchList.Select();
		}

		protected virtual void Batch_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null)
				return;

			gLEditReport.SetEnabled(sender.Updated.Cast<Batch>().Any(item => item.Selected == true));
		}

	}
}
