namespace Pomona.Common.Serialization
{
    public interface IPomonaSerializable
    {
        bool PropertyIsSerialized(string propertyName);
    }
}