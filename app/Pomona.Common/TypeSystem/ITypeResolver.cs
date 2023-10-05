#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    public interface ITypeResolver
    {
        PropertySpec FromProperty(Type reflectedType, PropertyInfo propertyInfo);
        TypeSpec FromType(Type type);
        TypeSpec FromType(string typeName);
        PropertySpec LoadBaseDefinition(PropertySpec propertySpec);
        TypeSpec LoadBaseType(TypeSpec typeSpec);
        ConstructorSpec LoadConstructor(TypeSpec typeSpec);
        IEnumerable<Attribute> LoadDeclaredAttributes(MemberSpec memberSpec);
        TypeSpec LoadDeclaringType(PropertySpec propertySpec);
        IEnumerable<TypeSpec> LoadGenericArguments(TypeSpec typeSpec);
        PropertyGetter LoadGetter(PropertySpec propertySpec);
        IEnumerable<TypeSpec> LoadInterfaces(TypeSpec typeSpec);
        string LoadName(MemberSpec memberSpec);
        string LoadNamespace(TypeSpec typeSpec);
        IEnumerable<PropertySpec> LoadProperties(TypeSpec typeSpec);
        PropertyFlags LoadPropertyFlags(PropertySpec propertySpec);
        TypeSpec LoadPropertyType(PropertySpec propertySpec);
        IEnumerable<PropertySpec> LoadRequiredProperties(TypeSpec typeSpec);
        RuntimeTypeDetails LoadRuntimeTypeDetails(TypeSpec typeSpec);
        PropertySetter LoadSetter(PropertySpec propertySpec);
        ResourceType LoadUriBaseType(ResourceType resourceType);
        bool TryGetTypeByName(string typeName, out TypeSpec typeSpec);
        PropertySpec WrapProperty(TypeSpec typeSpec, PropertyInfo propertyInfo);
    }
}

