#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    public interface ITypeResolver
    {
        TypeSpec FromType(Type type);
        PropertySpec FromProperty(PropertyInfo propertyInfo);
        IEnumerable<Attribute> LoadDeclaredAttributes(MemberSpec memberSpec);
        TypeSpec LoadDeclaringType(PropertySpec propertySpec);
        IEnumerable<TypeSpec> LoadGenericArguments(TypeSpec typeSpec);
        IEnumerable<TypeSpec> LoadInterfaces(TypeSpec typeSpec);
        string LoadName(MemberSpec memberSpec);
        string LoadNamespace(TypeSpec typeSpec);
        IEnumerable<PropertySpec> LoadProperties(TypeSpec typeSpec);
        TypeSpec LoadReflectedType(PropertySpec propertySpec);
        TypeSpec LoadBaseType(TypeSpec typeSpec);
        TypeSpec LoadPropertyType(PropertySpec propertySpec);
        PropertySpec.PropertyFlags LoadPropertyFlags(PropertySpec propertySpec);
        ResourceType LoadUriBaseType(ResourceType resourceType);
        PropertySpec LoadBaseDefinition(PropertySpec propertySpec);
        PropertySpec WrapProperty(TypeSpec typeSpec, PropertyInfo propertyInfo);
        Func<object, object> LoadGetter(PropertySpec propertySpec);
        Action<object, object> LoadSetter(PropertySpec propertySpec);
        RuntimeTypeDetails LoadRuntimeTypeDetails(TypeSpec typeSpec);
        IEnumerable<PropertySpec> LoadRequiredProperties(TypeSpec typeSpec);
        ConstructorSpec LoadConstructor(TypeSpec typeSpec);
    }
}