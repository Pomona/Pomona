#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Pomona.Common.Internals;

namespace Pomona.Common.TypeSystem
{
    public abstract class TypeResolver : ITypeResolver
    {
        private readonly Lazy<Dictionary<string, TypeSpec>> primitiveNameTypeMap;
        private readonly IEnumerable<ITypeFactory> typeFactories;
        private readonly ConcurrentDictionary<Type, TypeSpec> typeMap = new ConcurrentDictionary<Type, TypeSpec>();


        public TypeResolver()
        {
            var typeSpecTypes = new[]
            {
                typeof(DictionaryTypeSpec),
                typeof(EnumerableTypeSpec),
                typeof(EnumTypeSpec),
                typeof(RuntimeTypeSpec),
                typeof(QueryResultType)
            };
            this.typeFactories =
                typeSpecTypes
                    .SelectMany(
                        x =>
                            x.GetMethod("GetFactory", BindingFlags.Static | BindingFlags.Public)
                             .WrapAsEnumerable()
                             .Where(y => y != null && y.DeclaringType == x))
                    .Select(m => (ITypeFactory)m.Invoke(null, null))
                    .OrderBy(x => x.Priority)
                    .ToList();

            this.primitiveNameTypeMap = new Lazy<Dictionary<string, TypeSpec>>(() =>
                                                                                   TypeUtils.GetNativeTypes()
                                                                                            .Select(FromType)
                                                                                            .ToDictionary(x => x.Name, x => x,
                                                                                                          StringComparer.OrdinalIgnoreCase));
        }


        protected ConcurrentDictionary<Type, TypeSpec> TypeMap
        {
            get { return this.typeMap; }
        }


        protected virtual TypeSpec CreateType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            var typeSpec = this.typeFactories.Select(x => x.CreateFromType(this, type)).FirstOrDefault(x => x != null);
            if (typeSpec == null)
                throw new InvalidOperationException("Unable to find a TypeSpec factory for mapping type " + type);
            return typeSpec;
        }


        /// <summary>
        /// This method is responsible for mapping from a proxy or hidden clr type to an exposed type.
        /// </summary>
        /// <param name="type">The potentially hidden type.</param>
        /// <returns>An exposed type, which will often be the same type as given in argument.</returns>
        protected virtual Type MapExposedClrType(Type type)
        {
            return type;
        }


        public virtual PropertySpec FromProperty(Type reflectedType, PropertyInfo propertyInfo)
        {
            return FromType(reflectedType).GetPropertyByName(propertyInfo.Name, false);
        }


        public virtual TypeSpec FromType(string typeName)
        {
            TypeSpec typeSpec;
            if (!TryGetTypeByName(typeName, out typeSpec))
                throw new PomonaException("Type with name " + typeName + " not recognized.");
            return typeSpec;
        }


        public virtual TypeSpec FromType(Type type)
        {
            type = MapExposedClrType(type);
            var typeSpec = this.typeMap.GetOrAdd(type, CreateType);
            return typeSpec;
        }


        public virtual PropertySpec LoadBaseDefinition(PropertySpec propertySpec)
        {
            if (propertySpec == null)
                throw new ArgumentNullException(nameof(propertySpec));
            return propertySpec.OnLoadBaseDefinition();
        }


        public virtual TypeSpec LoadBaseType(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException(nameof(typeSpec));
            return typeSpec.OnLoadBaseType();
        }


        public virtual ConstructorSpec LoadConstructor(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException(nameof(typeSpec));
            return typeSpec.OnLoadConstructor();
        }


        public virtual IEnumerable<Attribute> LoadDeclaredAttributes(MemberSpec memberSpec)
        {
            if (memberSpec == null)
                throw new ArgumentNullException(nameof(memberSpec));
            return memberSpec.OnLoadDeclaredAttributes();
        }


        public virtual TypeSpec LoadDeclaringType(PropertySpec propertySpec)
        {
            if (propertySpec == null)
                throw new ArgumentNullException(nameof(propertySpec));

            return propertySpec.OnLoadDeclaringType();
        }


        public virtual IEnumerable<TypeSpec> LoadGenericArguments(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException(nameof(typeSpec));
            return typeSpec.OnLoadGenericArguments();
        }


        public virtual PropertyGetter LoadGetter(PropertySpec propertySpec)
        {
            if (propertySpec == null)
                throw new ArgumentNullException(nameof(propertySpec));
            return propertySpec.OnLoadGetter();
        }


        public virtual IEnumerable<TypeSpec> LoadInterfaces(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException(nameof(typeSpec));
            return typeSpec.OnLoadInterfaces();
        }


        public virtual string LoadName(MemberSpec memberSpec)
        {
            if (memberSpec == null)
                throw new ArgumentNullException(nameof(memberSpec));
            return memberSpec.OnLoadName();
        }


        public virtual string LoadNamespace(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException(nameof(typeSpec));

            return typeSpec.OnLoadNamespace();
        }


        public virtual IEnumerable<PropertySpec> LoadProperties(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException(nameof(typeSpec));
            return typeSpec.OnLoadProperties();
        }


        public virtual PropertyFlags LoadPropertyFlags(PropertySpec propertySpec)
        {
            if (propertySpec == null)
                throw new ArgumentNullException(nameof(propertySpec));
            return propertySpec.OnLoadPropertyFlags();
        }


        public virtual TypeSpec LoadPropertyType(PropertySpec propertySpec)
        {
            if (propertySpec == null)
                throw new ArgumentNullException(nameof(propertySpec));
            return propertySpec.OnLoadPropertyType();
        }


        public virtual IEnumerable<PropertySpec> LoadRequiredProperties(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException(nameof(typeSpec));
            return typeSpec.OnLoadRequiredProperties();
        }


        public virtual RuntimeTypeDetails LoadRuntimeTypeDetails(TypeSpec typeSpec)
        {
            if (typeSpec == null)
                throw new ArgumentNullException(nameof(typeSpec));
            return typeSpec.OnLoadRuntimeTypeDetails();
        }


        public virtual PropertySetter LoadSetter(PropertySpec propertySpec)
        {
            if (propertySpec == null)
                throw new ArgumentNullException(nameof(propertySpec));
            return propertySpec.OnLoadSetter();
        }


        public virtual ResourceType LoadUriBaseType(ResourceType resourceType)
        {
            if (resourceType == null)
                throw new ArgumentNullException(nameof(resourceType));
            return resourceType.OnLoadUriBaseType();
        }


        public virtual bool TryGetTypeByName(string typeName, out TypeSpec typeSpec)
        {
            return this.primitiveNameTypeMap.Value.TryGetValue(typeName, out typeSpec);
        }


        public virtual PropertySpec WrapProperty(TypeSpec typeSpec, PropertyInfo propertyInfo)
        {
            if (typeSpec == null)
                throw new ArgumentNullException(nameof(typeSpec));
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));
            return typeSpec.OnWrapProperty(propertyInfo);
        }
    }
}