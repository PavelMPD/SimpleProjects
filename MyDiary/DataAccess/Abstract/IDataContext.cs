using System;
using MyDiary.Model;

namespace MyDiary.DataAccess.Abstract
{
    public interface IDataContext : IDisposable
    {
        ApplicationDbContext DataContext { get; }

        int SaveChanges();

        void DetectChanges();

        void AttachEntity<T>(T entity) where T : class, IEntity;
    }
}