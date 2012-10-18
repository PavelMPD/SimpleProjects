using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ED.Domen.Entities;

namespace ED.Repository.Interfaces
{
    public interface IRepository<T> where T : Entity
    {
        T GetEntity(Int32 id);
        void Save(T entity);
        IEnumerable<T> GetList();
    }
}
