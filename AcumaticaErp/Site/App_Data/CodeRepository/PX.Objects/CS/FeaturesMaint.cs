using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;

namespace PX.Objects.CS
{

	public class FeaturesMaint : PXGraph<FeaturesMaint>
	{
		#region DACs


		#endregion
		public PXSelect<FeaturesSet> Features;

		protected IEnumerable features()
		{
			FeaturesSet current = (FeaturesSet)PXSelect<FeaturesSet,
				                  Where<True, Equal<True>>,
				                  OrderBy<Desc<FeaturesSet.status>>>
				                  .SelectWindowed(this, 0, 1) ?? Features.Insert();
			current.LicenseID = PXVersionInfo.InstallationID;
			yield return current;				
		}
		

		public PXSave<FeaturesSet> Save;		
		public PXCancel<FeaturesSet> Cancel;
		public PXAction<FeaturesSet> Insert;

		public PXAction<FeaturesSet> RequestValidation;
		public PXAction<FeaturesSet> CancelRequest;

		public override IEnumerable ExecuteSelect(string viewName, object[] parameters, object[] searches, string[] sortcolumns, bool[] descendings, PXFilterRow[] filters, ref int startRow, int maximumRows, ref int totalRows)
		{
			if (viewName == "Features")
				searches = null;

			return base.ExecuteSelect(viewName, parameters, searches, sortcolumns, descendings, filters, ref startRow, maximumRows, ref totalRows);
		}

		[PXButton]
		[PXUIField(DisplayName = "Modify", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Select)]
		public IEnumerable insert(PXAdapter adapter)
		{
			foreach (var item in new PXInsert<FeaturesSet>(this, "Insert").Press(adapter))
				yield return item;
		}

		[PXButton]
		//[PXUIField(DisplayName = "Request Validation")]
		[PXUIField(DisplayName = "Enable")]
		public IEnumerable requestValidation(PXAdapter adapter)
		{
			foreach (FeaturesSet feature in adapter.Get())
			{
				if (feature.Status == 3)
				{
					FeaturesSet update = PXCache<FeaturesSet>.CreateCopy(feature);
					update.Status = 0;					
					update = this.Features.Update(update);
					this.Features.Delete(feature);
					
					if (update.Status != 1)					
						this.Features.Delete(new FeaturesSet() {Status = 1});
					

					this.Persist();
					yield return update;
				}
				else
					yield return feature;
			}
			
			PXDatabase.ResetSlots();
			PXPageCacheUtils.InvalidateCachedPages();
			this.Clear();

			throw new PXRedirectToUrlException(@"~\Main.aspx$target=_top", PXBaseRedirectException.WindowMode.Same, Messages.RefreshSettings);
		}

		[PXButton]
		[PXUIField(DisplayName = "Cancel Validation Request", Visible = false)]
		public IEnumerable cancelRequest(PXAdapter adapter)
		{
			foreach (FeaturesSet feature in adapter.Get())
			{
				if (feature.Status == 2)
				{
					FeaturesSet update = PXCache<FeaturesSet>.CreateCopy(feature);
					update.Status = 3;					
					this.Features.Delete(feature);
					update = this.Features.Update(update);
					this.Persist();
					yield return update;
				}
				else
					yield return feature;
			}
		}

		protected virtual void FeaturesSet_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			this.Features.Cache.AllowInsert = true;
			FeaturesSet row = (FeaturesSet)e.Row;
			if (row == null) return;

			this.RequestValidation.SetEnabled(row.Status == 3);
			this.CancelRequest.SetEnabled(row.Status == 2);
			this.Features.Cache.AllowInsert = row.Status < 2;
			this.Features.Cache.AllowUpdate = row.Status == 3;
			this.Features.Cache.AllowDelete = false;			
		}

		protected virtual void FeaturesSet_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			int? status = (int?)sender.GetValue<FeaturesSet.status>(e.Row);
			if (status != 3) return;

			FeaturesSet current = PXSelect<FeaturesSet,
				Where<True, Equal<True>>,
				OrderBy<Desc<FeaturesSet.status>>>
				.SelectWindowed(this,0,1);
			if (current != null)
			{
				sender.RestoreCopy(e.Row, current);
				sender.SetValue<FeaturesSet.status>(e.Row, 3);
			}
		}
	}
}
