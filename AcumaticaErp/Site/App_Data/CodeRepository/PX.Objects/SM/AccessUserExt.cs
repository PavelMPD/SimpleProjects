using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.SM.Descriptor;
using PX.SM;

namespace PX.Objects.SM
{
    public class AccessUserExt : PXGraphExtension<AccessUsers>
    {
         public PXSelectPureUsers<Users> UserList;

         public override void Initialize()
         {
             Base.UserList.RemovePersistHandler();
         }
    }
}
