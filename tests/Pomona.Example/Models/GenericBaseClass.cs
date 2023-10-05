#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

namespace Pomona.Example.Models
{
    public abstract class GenericBaseClass<T> : EntityBase
    {
        public T FooBar { get; set; }
    }
}
