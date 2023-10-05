#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

namespace Pomona.Example.Models
{
    public abstract class AbstractAnimal : EntityBase
    {
        public virtual bool PublicAndReadOnlyThroughApi { get; set; }
        public abstract string TheAbstractProperty { get; set; }
        public virtual string TheVirtualProperty { get; set; }
    }
}

