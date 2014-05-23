using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DebtCollection.DataAccess.Abstract;

namespace DataAccess.Abstract
{
    public interface IRepository<T> : IRequireDataContext
    {
        T GetById(long id);

        IEnumerable<T> Get(Expression<Func<T, bool>> where, params String[] includes);

        IEnumerable<T> GetAll(params String[] includes);

        void Save(T entity);

        void Delete(long id);

        void Delete(T entity);

        int Count();
    }
}
