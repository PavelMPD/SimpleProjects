using Microsoft.Practices.Unity;

namespace DebtCollection.Common
{
    /// <summary>
    /// Object container holder.
    /// </summary>
    public static class ObjectContainer
    {
        private static IUnityContainer container = new UnityContainer();

        public static IUnityContainer Current
        {
            get
            {
                return container;
            }
        }

        public static TI Resolve<TI>()
        {
            return Current.Resolve<TI>();
        }

        public static void Reset()
        {
            container.Dispose();
            container = new UnityContainer();
        }
    }
}