using System.Collections.Generic;

namespace Pomona
{
    public interface IMappedType
    {
        string Name { get; }
        bool IsGenericType { get; }
        bool IsGenericTypeDefinition { get; }
        IList<IMappedType> GenericArguments { get; }
        IMappedType BaseType { get; }
    }
}