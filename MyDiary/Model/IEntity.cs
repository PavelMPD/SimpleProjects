namespace MyDiary.Model
{
    /// <summary>
    ///   Base interface for entities in the system.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        ///   Gets or sets entity id.
        /// </summary>
        /// <permission cref = "System.Security.PermissionSet">public</permission>
        long Id
        {
            get;
            set;
        }

        /// <summary>
        ///   Gets a value indicating whether entity is newly created.
        /// </summary>
        bool IsNew
        {
            get;
        }
    }
}
