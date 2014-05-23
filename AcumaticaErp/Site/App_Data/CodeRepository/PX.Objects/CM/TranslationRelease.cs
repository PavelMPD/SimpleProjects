using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.BQLConstants;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.CM
{
	[TableAndChartDashboardType]
	public class TranslationRelease : PXGraph<TranslationRelease>
	{
		#region Implementation		
		public TranslationRelease()
		{
			CMSetup setup = CMSetup.Current;
			TranslationReleaseList.SetProcessDelegate(
				delegate(TranslationHistory transl)
				{
					TranslationHistoryMaint.CreateBatch(transl, false);
				}
			);
			TranslationReleaseList.SetProcessCaption(Messages.Release);
			TranslationReleaseList.SetProcessAllVisible(false);
		}
		#endregion

		#region Buttons
		public PXAction<TranslationHistory> cancel;
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected virtual IEnumerable Cancel(PXAdapter adapter)
		{
			TranslationReleaseList.Cache.Clear();
			TimeStamp = null;
			PXLongOperation.ClearStatus(this.UID);
			return adapter.Get();
		}
		public PXAction<TranslationHistory> viewTranslation;
		[PXUIField(DisplayName = Messages.ViewTranslation, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXLookupButton]
		public virtual IEnumerable ViewTranslation(PXAdapter adapter)
		{
			if (TranslationReleaseList.Current != null)
			{
				if (TranslationReleaseList.Current.ReferenceNbr != null)
				{
					TranslationHistoryMaint graph = PXGraph.CreateInstance<TranslationHistoryMaint>();
					graph.Clear();
					TranslationHistory newTranslation = new TranslationHistory();
					graph.TranslHistRecords.Current = PXSelect<TranslationHistory,
							Where<TranslationHistory.referenceNbr, Equal<Required<TranslationHistory.referenceNbr>>>>
							.Select(this, TranslationReleaseList.Current.ReferenceNbr);
					throw new PXRedirectRequiredException(graph, true, Messages.ViewTranslation){Mode = PXBaseRedirectException.WindowMode.NewWindow};
				}
			}
			return TranslationReleaseList.Select();
		}
		#endregion

		[PXFilterable]
		public PXProcessing<TranslationHistory, Where<TranslationHistory.released, Equal<False>>> TranslationReleaseList;
		public PXSetup<CMSetup> CMSetup;
	}
}
