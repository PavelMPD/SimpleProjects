using System;
using PX.Data;

namespace PX.Objects.CR.MassProcess.RelationMergers
{
	public interface IMerger
	{
		void SetField(PXGraph graph,
			       object resultOldValuesRecord,
				   Type changingField,
				   object newValue);
	}
}
