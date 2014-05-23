using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace DebtCollection.Model
{
    /// <summary>
    ///   Base class for entitities.
    /// </summary>
    [DebuggerDisplay("Id = {Id}")]
    public class Entity : IEntity
    {
        /// <summary>
        ///   Gets or sets object id.
        /// </summary>
        /// <permission cref = "System.Security.PermissionSet">public</permission>
        [Key]
        [Display(Name = "Id")]
        public long Id
        {
            get;
            set;
        }

        /// <summary>
        ///   Gets a value indicating whether entity is newly created.
        /// </summary>
        public bool IsNew
        {
            get
            {
                return Id <= 0;
            }
        }

        /// <summary>
        ///   Overrides base equals.
        /// </summary>
        /// <permission cref = "System.Security.PermissionSet">public</permission>
        /// <param name = "obj">Object to compare with.</param>
        /// <returns>Returns the result indicating whether objects are equal.</returns>
        public override bool Equals(object obj)
        {
            Entity _entity = obj as Entity;
            if (obj == null || GetType() != _entity.GetType())
            {
                return false;
            }

            return _entity.Id.Equals(this.Id);
        }



        private int? hashCode;
        /// <summary>
        ///   Calculates hash code of the object.
        /// </summary>
        /// <permission cref = "System.Security.PermissionSet">public</permission>
        /// <returns>Returns hash code of the object.</returns>
        public override int GetHashCode()
        {
            if (hashCode == null)
            {
                hashCode = (int)(37 * Id) + 17;
            }

            return hashCode.Value;
        }
    }
}