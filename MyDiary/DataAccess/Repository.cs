using DataAccess.Abstract;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using MyDiary.Common;
using MyDiary.Model;

namespace MyDiary.DataAccess
{
    public class Repository<T> : ContextHolder, IRepository<T> where T : class, IEntity
    {
        public static IRepository<T> Instance()
        {
            return ObjectContainer.Resolve<IRepository<T>>();
        }

        public virtual T GetById(long id)
        {
            return DataContext.Set<T>().Find(id);
        }

        public virtual IEnumerable<T> Get(Expression<Func<T, bool>> where, params String[] includes)
        {
            var query = AddIncludes(DataContext.Set<T>(), includes);
            return query.Where(where).ToArray();
        }

        public virtual IEnumerable<T> GetAll(params String[] includes)
        {
            var query = AddIncludes(DataContext.Set<T>(), includes);

            return query.ToArray();
        }


        public virtual void Save(T entity)
        {
            if (entity.IsNew)
            {
                DataContext.Set<T>().Add(entity);
                DataContext.SaveChanges();
            }
            else
            {
                DataContext.Set<T>().Attach(entity);
                DataContext.Entry(entity).State = EntityState.Modified;
                DataContext.DetectChanges();
            }
        }

        public virtual void Delete(long id)
        {
            var entity = GetById(id);
            if (entity != null)
            {
                DataContext.Entry(entity).State = EntityState.Unchanged;
                DataContext.Set<T>().Remove(entity);
            }
        }

        public virtual void Delete(T entity)
        {
            DataContext.Set<T>().Remove(entity);
        }

        public virtual int Count()
        {
            return DataContext.Set<T>().Count();
        }

        protected IQueryable<T> AddIncludes(IQueryable<T> query, string[] includes)
        {
            for (int i = 0; i < includes.Length; i++)
            {
                query = query.Include(includes[i]);
            }

            return query;
        }
    }
}