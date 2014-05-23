using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.CR.MassProcess
{
	struct ObjectEntry
	{
		public object Object;
		public PXEntryStatus Status;

		public ObjectEntry(PXEntryStatus status, object o)
		{
			Status = status;
			Object = o;
		}
	}
}
