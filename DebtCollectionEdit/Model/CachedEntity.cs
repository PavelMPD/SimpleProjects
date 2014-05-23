using System;

namespace DebtCollection.Model
{
    public abstract class CachedEntity : Entity
    {
        public virtual String CacheKey
        {
            get
            {
                return this.GetType().FullName;
            }
        }
    }
}