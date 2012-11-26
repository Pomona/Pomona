using System;

namespace Pomona.Common.TypeSystem
{
    /// <summary>
    /// This is the pomona way of representing a property.
    /// 
    /// Can't use PropertyInfo directly, since the transformed types might not exist
    /// as Type in server context.
    /// </summary>
    public interface IPropertyInfo
    {
        string Name { get; }
        string JsonName { get; }
        IMappedType PropertyType { get; }
        IMappedType DeclaringType { get; }
        PropertyCreateMode CreateMode { get; }
        Func<object, object> Getter { get; }
        Action<object, object> Setter { get; }
        string LowerCaseName { get; }
        bool AlwaysExpand { get; }
        bool IsWriteable { get; }
    }
}