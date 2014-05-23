using System;

namespace MyDiary.DataAccess
{
    /// <summary>
    /// Holder of DbContext for usage in repositories.
    /// </summary>
    public class ContextHolder
    {
        /// <summary>
        /// Thread-local context storage.
        /// </summary>
        [ThreadStatic]
        private static ApplicationDbContext context;

        /// <summary>
        /// Gets a value indicating whether context initialized.
        /// </summary>
        public bool ContextInitialized
        {
            get
            {
                return context != null;
            }
        }

        /// <summary>
        /// Gets or sets DbContext in current thread slot.
        /// </summary>
        public virtual ApplicationDbContext DataContext
        {
            get
            {
                if (context == null)
                {
                    throw new InvalidOperationException("DbContext not initialized. Make sure that repository is correctly registered via object container.");
                }

                return context;
            }
            set
            {
                context = value;
            }
        }
    }
}