using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Finance
{
    public abstract class Money
    {
        protected Int32 amount;
        protected String currency;

        public Money(Int32 amount, String currency)
        {
            this.amount = amount;
            this.currency = currency;
        }
        
        public abstract Money Times(Int32 multiplier);
        
        public String Currency()
        {
            return currency;
        }

        public override Boolean Equals(Object obj)
        {
            if (obj == null)
                return false;
            Money entity = obj as Money;
            if (entity == null)
                return false;
            return GetType().Equals(entity.GetType()) && (amount == entity.amount);
        }

        /// <summary>
        /// Фабричный метод
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static Money NewDollar(int amount)
        {
            return new Dollar(amount, "USD");
        }

        public static Money NewFranc(int amount)
        {
            return new Franc(amount, "CHF");
        }


    }
}
