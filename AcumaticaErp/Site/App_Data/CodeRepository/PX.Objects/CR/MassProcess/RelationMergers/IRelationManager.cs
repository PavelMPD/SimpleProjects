using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CR.MassProcess.DacRelations;

namespace PX.Objects.CR.MassProcess.RelationMergers
{
	public interface IRelationManager
	{
		void SetRelation(PXGraph graph,
						   Relation relation,
						   object oldRecord,
						   Type changingField,
						   object newValue);

		void RemapRelations(PXGraph graph,
							Relation relation,
							object oldRecord,
							object remapToRecord);

		bool CanProcess(Relation relation);
	}
}
