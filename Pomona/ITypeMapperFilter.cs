using System.Reflection;

namespace Pomona
{
    public interface ITypeMapperFilter
    {
        bool PropertyIsIncluded(PropertyInfo propertyInfo);
    }
}