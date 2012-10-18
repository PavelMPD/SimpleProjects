using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Finance
{
    public class Franc : Money
    {
        public Franc(Int32 amount, String currency) : base(amount, currency)
        {
            
        }

        public override Money Times(Int32 multiplier)
        {
            return Money.NewFranc(amount * multiplier);
        }  
    }
}