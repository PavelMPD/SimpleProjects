using System;
using PX.Data;
using PX.Objects.CM;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.GL
{
	public class SubAccountMaint : PXGraph<SubAccountMaint>
	{
        public PXSavePerRow<Sub, Sub.subID> Save;
		public PXCancel<Sub> Cancel;
		[PXImport(typeof(Sub))]
		[PXFilterable]
		public PXSelectOrderBy<Sub, OrderBy<Asc<Sub.subCD>>> SubRecords;
		
		public SubAccountMaint()
		{
			if (Company.Current.BAccountID.HasValue == false)
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Branch), CS.Messages.BranchMaint);
			}
		}
		public PXSetup<Branch> Company;

		public PXAction<Sub> viewRestrictionGroups;
		[PXUIField(DisplayName = Messages.ViewRestrictionGroups, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewRestrictionGroups(PXAdapter adapter)
		{
			if (SubRecords.Current != null)
			{
				GLAccessBySub graph = CreateInstance<GLAccessBySub>();
				graph.Sub.Current = graph.Sub.Search<Sub.subCD>(SubRecords.Current.SubCD);
				throw new PXRedirectRequiredException(graph, false, "Restricted Groups");
			}
			return adapter.Get();
		}

	}
}
