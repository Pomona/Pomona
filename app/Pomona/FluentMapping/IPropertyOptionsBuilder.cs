namespace Pomona.FluentMapping
{
    public interface IPropertyOptionsBuilder<TDeclaringType, TPropertyType>
    {
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsPrimaryKey();
        IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Named(string name);
    }
}