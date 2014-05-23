using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CR.MassProcess.DacRelations;

namespace PX.Objects.CR.MassProcess.DacRelations
{
	public class OneToManyRelation	:Relation
	{
		public OneToManyRelation(Type childFkField, Type parentIdField)
		{
			Right = new DacReference{TargetField = parentIdField,ReferenceField = childFkField};
		}
	}

	public class OneToManyRelation<TChildFk, TParentID> : OneToManyRelation
		where TParentID : IBqlField
		where TChildFk : IBqlField
	{
		public OneToManyRelation(): base(typeof(TChildFk), typeof(TParentID))
	    {
	      
	    }
	}
}
