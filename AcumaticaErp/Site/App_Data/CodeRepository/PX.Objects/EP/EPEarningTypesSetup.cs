using System;
using PX.Data;


namespace PX.Objects.EP
{
	public class EPEarningTypesSetup : PXGraph<EPEarningTypesSetup>
	{
		#region Selects

		public PXSelect<EPEarningType> EarningTypes;

		#endregion

		#region Actions

		public PXSave<EPEarningType> Save;
		public PXCancel<EPEarningType> Cancel;

		#endregion

		protected void EPEarningType_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			EPEarningType row = (EPEarningType)e.Row;
			if (row == null) return;

			EPSetup setup = PXSelect<
				EPSetup
				, Where<EPSetup.regularHoursType, Equal<Required<EPEarningType.typeCD>>
					, Or<EPSetup.holidaysType, Equal<Required<EPEarningType.typeCD>>
						, Or<EPSetup.vacationsType, Equal<Required<EPEarningType.typeCD>>>
						>
					>
				>.Select(this, row.TypeCD, row.TypeCD, row.TypeCD);
			if(setup != null)
				throw new PXException(Messages.CannotDeleteInUse);

			CR.CRCaseClassLaborMatrix caseClassLabor = PXSelect<CR.CRCaseClassLaborMatrix, Where<CR.CRCaseClassLaborMatrix.earningType, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.TypeCD);
			if (caseClassLabor != null)
				throw new PXException(Messages.CannotDeleteInUse);

			EPContractRate contractRate = PXSelect<EPContractRate, Where<EPContractRate.earningType, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.TypeCD);
			if (contractRate != null)
				throw new PXException(Messages.CannotDeleteInUse);

			EPEmployeeClassLaborMatrix employeeLabor = PXSelect<EPEmployeeClassLaborMatrix, Where<EPEmployeeClassLaborMatrix.earningType, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.TypeCD);
			if (employeeLabor != null)
				throw new PXException(Messages.CannotDeleteInUse);

			CR.EPActivity activity = PXSelect<CR.EPActivity, Where<CR.EPActivity.earningTypeID, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.TypeCD);
			if (activity != null)
				throw new PXException(Messages.CannotDeleteInUse);

			PM.PMTran pmTran = PXSelect<PM.PMTran, Where<PM.PMTran.earningType, Equal<Required<EPEarningType.typeCD>>>>.Select(this, row.TypeCD);
			if (pmTran != null)
				throw new PXException(Messages.CannotDeleteInUse);

		}
	}
}
