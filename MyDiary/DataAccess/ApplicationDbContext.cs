using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using MyDiary.Model;

namespace MyDiary.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        #region Db sets

        public DbSet<User> Users { get; set; }

        #endregion Db sets

        /// <summary>
        ///   Initializes a new instance of the ApplicationDbContext class.
        /// </summary>
        public ApplicationDbContext()
            : base((string) ConnectionNameProvider.ConnectionName)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.AutoDetectChangesEnabled = false;
            try
            {
                ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 120;

            }
            catch
            {
                // this [try] is scotch for instantiating dbContext in UnitTests
            }
        }

        public void DetectChanges()
        {
            ChangeTracker.DetectChanges();
        }

        public void AttachEntity<T>(T entity) where T : class, IEntity
        {
            DbSet<T> set = Set<T>();

            if (set.Local.Any(e => e.Id == entity.Id)) return;

            set.Attach(entity);
            Entry(entity).State = EntityState.Modified;
        }

        private void AttachEntityUnchanged<T>(T entity) where T : class, IEntity
        {
            DbSet<T> set = Set<T>();

            if (set.Local.Any(e => e.Id == entity.Id)) return;

            set.Attach(entity);
            Entry(entity).State = EntityState.Unchanged;
        }

        private T GetAttachedEntity<T>(T entity) where T : class, IEntity
        {
            DbSet<T> set = Set<T>();

            return set.Local.FirstOrDefault(e => e.Id == entity.Id);
        }

        public void DetachEntity<T>(T entity) where T : class
        {
            Entry(entity).State = EntityState.Detached;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //задаем отношения между таблицами
            base.OnModelCreating(modelBuilder);
        }
    }
}
