using System;
using MyDiary.DataAccess.Abstract;
using MyDiary.Model;

namespace MyDiary.DataAccess
{
    public class DataContext : ContextHolder, IDataContext
    {
        private bool disposed;

        private readonly bool initializedContext;

        public DataContext()
        {
            if (!ContextInitialized)
            {
                DataContext = new ApplicationDbContext();
                initializedContext = true;
            }
        }

        public int SaveChanges()
        {
            return DataContext.SaveChanges();
        }

        public void DetectChanges()
        {
            DataContext.DetectChanges();
        }

        public void AttachEntity<T>(T entity) where T : class, IEntity
        {
            DataContext.AttachEntity(entity);
        }

        ~DataContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (DataContext != null && initializedContext)
                    {
                        try
                        {
                            DataContext.SaveChanges();
                        }
                        finally
                        {
                            DataContext.Dispose();
                            DataContext = null;
                        }
                    }
                }
            }
            disposed = true;
        }
    }
}