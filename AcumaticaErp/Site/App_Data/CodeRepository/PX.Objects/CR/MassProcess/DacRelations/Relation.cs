using System;
using System.Collections.Generic;

namespace PX.Objects.CR.MassProcess.DacRelations
{
	public class Relation
	{
		public DacReference Left;
		public DacReference Right;

		public bool Contains(Type fieldType)
		{
			return (Left != null && (Left.ReferenceField == fieldType || Left.TargetField == fieldType))
			       ||
				   (Right != null && (Right.ReferenceField == fieldType || Right.TargetField == fieldType));

		}
	}
}
