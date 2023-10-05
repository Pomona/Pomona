#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

namespace Pomona.Example.Models
{
    public abstract class EntityBase : IEntityWithId
    {
        /// <summary>
        /// Id of the entity.
        /// </summary>
        public int Id { get; set; }

        int IEntityWithId.Id
        {
            get { return Id; }
            set { Id = value; }
        }
    }
}

