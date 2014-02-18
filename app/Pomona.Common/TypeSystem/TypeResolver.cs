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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pomona.Common.Internals;
using Pomona.Common.Serialization;

namespace Pomona.Common.TypeSystem
{
    public class TypeResolver : ITypeResolver
    {
        private readonly IEnumerable<ITypeFactory> typeFactories;

        private readonly ConcurrentDictionary<Type, TypeSpec> typeMap = new ConcurrentDictionary<Type, TypeSpec>();

        public TypeResolver()
        {
            typeFactories =
                GetType().WalkTree(x => x.BaseType)
                .TakeUntil(x => x == typeof(object))
                .Select(x => x.Assembly).Distinct()
                .SelectMany(x => x.GetTypes())
                    .Where(x => typeof(TypeSpec).IsAssignableFrom(x))
                    .SelectMany(
                        x =>
                            x.GetMethod("GetFactory", BindingFlags.Static | BindingFlags.Public)
                                .WrapAsEnumerable()
                                .Where(y => y != null && y.DeclaringType == x))
                    .Select(m => (ITypeFactory)m.Invoke(null, null))
                    .OrderBy(x => x.Priority)
                    .ToList();
        }


        public virtual IEnumerable<PropertySpec> LoadProperties(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException("typeSpec");
            return typeSpec.OnLoadProperties();
        }


        public virtual IEnumerable<TypeSpec> LoadInterfaces(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException("typeSpec");
            return typeSpec.OnLoadInterfaces();
        }


        public virtual IEnumerable<TypeSpec> LoadGenericArguments(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException("typeSpec");
            return typeSpec.OnLoadGenericArguments();
        }


        public virtual string LoadName(MemberSpec memberSpec)
        {
            if (memberSpec == null)
                throw new ArgumentNullException("memberSpec");
            return memberSpec.OnLoadName();
        }


        public virtual string LoadNamespace(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException("typeSpec");

            return typeSpec.OnLoadNamespace();
        }


        public virtual TypeSpec LoadDeclaringType(PropertySpec propertySpec)
        {
            if (propertySpec == null)
                throw new ArgumentNullException("propertySpec");

            return propertySpec.OnLoadDeclaringType();
        }


        public virtual TypeSpec LoadReflectedType(PropertySpec propertySpec)
        {
            if (propertySpec == null) throw new ArgumentNullException("propertySpec");

            return propertySpec.OnLoadReflectedType();
        }

        public virtual TypeSpec LoadBaseType(TypeSpec typeSpec)
        {
            if (typeSpec == null) throw new ArgumentNullException("typeSpec");
            return typeSpec.OnLoadBaseType();
        }

        public virtual TypeSpec LoadPropertyType(PropertySpec propertySpec)
        {
            if (propertySpec == null) throw new ArgumentNullException("propertySpec");
            return propertySpec.OnLoadPropertyType();
        }

        public virtual PropertyFlags LoadPropertyFlags(PropertySpec propertySpec)
        {
            if (propertySpec == null) throw new ArgumentNullException("propertySpec");
            return propertySpec.OnLoadPropertyFlags();
        }

        public virtual ResourceType LoadUriBaseType(ResourceType resourceType)
        {
            if (resourceType == null) throw new ArgumentNullException("resourceType");
            return resourceType.OnLoadUriBaseType();
        }

        public virtual PropertySpec LoadBaseDefinition(PropertySpec propertySpec)
        {
            if (propertySpec == null) throw new ArgumentNullException("propertySpec");
            return propertySpec.OnLoadBaseDefinition();
        }

        public virtual PropertySpec WrapProperty(TypeSpec typeSpec, PropertyInfo propertyInfo)
        {
            if (typeSpec == null) throw new ArgumentNullException("typeSpec");
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            return typeSpec.OnWrapProperty(propertyInfo);
        }

        public virtual Func<object, IContextResolver, object> LoadGetter(PropertySpec propertySpec)
        {
            if (propertySpec == null) throw new ArgumentNullException("propertySpec");
            return propertySpec.OnLoadGetter();
        }

        public virtual Action<object, object, IContextResolver> LoadSetter(PropertySpec propertySpec)
        {
            if (propertySpec == null) throw new ArgumentNullException("propertySpec");
            return propertySpec.OnLoadSetter();
        }


        public virtual RuntimeTypeDetails LoadRuntimeTypeDetails(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException("typeSpec");
            return typeSpec.OnLoadRuntimeTypeDetails();
        }


        public virtual IEnumerable<PropertySpec> LoadRequiredProperties(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException("typeSpec");
            return typeSpec.OnLoadRequiredProperties();
        }


        public virtual ConstructorSpec LoadConstructor(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException("typeSpec");
            return typeSpec.OnLoadConstructor();
        }


        public virtual IEnumerable<Attribute> LoadDeclaredAttributes(MemberSpec memberSpec)
        {
            if (memberSpec == null)
                throw new ArgumentNullException("memberSpec");
            return memberSpec.OnLoadDeclaredAttributes();
        }


        public virtual PropertySpec FromProperty(PropertyInfo propertyInfo)
        {
            return FromType(propertyInfo.ReflectedType).GetPropertyByName(propertyInfo.Name, false);
        }

        public virtual TypeSpec FromType(Type type)
        {
            var typeSpec = typeMap.GetOrAdd(type, CreateType);
            return typeSpec;
        }

        protected virtual TypeSpec CreateType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            var typeSpec = typeFactories.Select(x => x.CreateFromType(this, type)).FirstOrDefault(x => x != null);
            if (typeSpec == null)
                throw new InvalidOperationException("Unable to find a TypeSpec factory for mapping type " + type);
            return typeSpec;
        }
    }
}