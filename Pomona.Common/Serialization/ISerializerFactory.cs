namespace Pomona.Common.Serialization
{
    public interface ISerializerFactory
    {
        ISerializer GetSerialier();
    }
}