using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public interface IDeserializerNode
    {
        IDeserializationContext Context { get; }
        IMappedType ExpectedBaseType { get; }
        string Uri { get; set; }
        object Value { get; set; }
        IMappedType ValueType { get; }
        void SetValueType(string typeName);
    }
}