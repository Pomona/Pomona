using System;
using System.Collections.Generic;
using System.Reflection;

using Pomona.Common.TypeSystem;

namespace Pomona.Mapping
{
    public interface ITypeConventions
    {
        #region TypeMappingConventions

        bool TypeIsMapped(Type type);
        bool TypeIsMappedAsCollection(Type type);
        bool TypeIsMappedAsSharedType(Type type);
        bool TypeIsMappedAsTransformedType(Type type);
        bool TypeIsMappedAsValueObject(Type type);

        IEnumerable<PropertyInfo> GetAllPropertiesOfType(Type type, BindingFlags bindingFlags);
        ConstructorSpec GetTypeConstructor(Type type);
        bool GetTypeIsAbstract(Type type);
        string GetTypeMappedName(Type type);
        bool IsIndependentTypeRoot(Type type);

        #endregion
    }
}