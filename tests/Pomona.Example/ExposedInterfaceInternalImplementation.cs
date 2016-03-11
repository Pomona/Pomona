#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.Example.Models;

namespace Pomona.Example
{
    internal class ExposedInterfaceInternalImplementation : IExposedInterface, IEntityWithId
    {
        public string FooBar { get; set; }
        public int Id { get; set; }
        public int PropertyFromInheritedInterface { get; set; }
    }
}