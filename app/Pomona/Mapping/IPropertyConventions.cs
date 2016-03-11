#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.TypeSystem;

namespace Pomona.Mapping
{
    public interface IPropertyConventions
    {
        IEnumerable<Attribute> GetPropertyAttributes(Type type, PropertyInfo propertyInfo);
        ExpandMode GetPropertyExpandMode(Type type, PropertyInfo propertyInfo);
        LambdaExpression GetPropertyFormula(Type type, PropertyInfo propertyInfo);
        PropertyGetter GetPropertyGetter(Type type, PropertyInfo propertyInfo);
        string GetPropertyMappedName(Type type, PropertyInfo propertyInfo);
        PropertySetter GetPropertySetter(Type type, PropertyInfo propertyInfo);
        Type GetPropertyType(Type type, PropertyInfo propertyInfo);
        bool PropertyIsIncluded(Type type, PropertyInfo propertyInfo);
    }
}