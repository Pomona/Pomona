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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    public abstract class TypeSpec : MemberSpec
    {
        private readonly Lazy<TypeSpec> baseType;
        private readonly Lazy<string> @namespace;


        protected TypeSpec(ITypeResolver typeResolver, Type type)
            : base(typeResolver, type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            this.@namespace = CreateLazy(() => typeResolver.LoadNamespace(this));
            this.baseType = CreateLazy(() => typeResolver.LoadBaseType(this));
        }


        public virtual IEnumerable<PropertySpec> AllProperties
        {
            get
            {
                if (IsInterface)
                    return Properties.Concat(Interfaces.SelectMany(y => y.Properties));
                return Properties;
            }
        }

        public virtual TypeSpec BaseType
        {
            get { return this.baseType.Value; }
        }

        public abstract ConstructorSpec Constructor { get; }

        public virtual TypeSpec ElementType
        {
            get
            {
                if (Type.IsNullable())
                    return TypeResolver.FromType(Nullable.GetUnderlyingType(Type));
                if (Type.IsArray)
                    return TypeResolver.FromType(Type.GetElementType());
                throw new NotSupportedException();
            }
        }

        public string FullName
        {
            get { return string.Format("{0}.{1}", Namespace, Name); }
        }

        public abstract IEnumerable<TypeSpec> Interfaces { get; }
        public abstract bool IsAbstract { get; }

        public virtual bool IsAlwaysExpanded
        {
            get { return true; }
        }

        public virtual bool IsArray
        {
            get { return Type.IsArray; }
        }

        public virtual bool IsCollection
        {
            get { return false; }
        }

        public virtual bool IsDictionary
        {
            get { return false; }
        }

        public virtual bool IsGenericType
        {
            get { return Type.IsGenericType; }
        }

        public virtual bool IsGenericTypeDefinition
        {
            get { return Type.IsGenericTypeDefinition; }
        }

        public virtual bool IsInterface
        {
            get { return Type.IsInterface; }
        }

        public abstract bool IsNullable { get; }

        public virtual string Namespace
        {
            get { return this.@namespace.Value; }
        }

        public abstract string NameWithGenericArguments { get; }
        public abstract IEnumerable<PropertySpec> Properties { get; }
        public abstract IEnumerable<PropertySpec> RequiredProperties { get; }
        public abstract TypeSerializationMode SerializationMode { get; }

        public virtual Type Type
        {
            get { return (Type)Member; }
        }


        public virtual object Create(IConstructorPropertySource propertySource)
        {
            throw new NotSupportedException("Unable to instantiate type " + FullName);
        }


        public PropertySpec GetPropertyByName(string propertyName, bool ignoreCase)
        {
            PropertySpec propertySpec;
            if (!TryGetPropertyByName(propertyName, ignoreCase, out propertySpec))
                throw new KeyNotFoundException("Property with name not found");
            return propertySpec;
        }


        public bool IsAssignableFrom(TypeSpec t)
        {
            return Type.IsAssignableFrom(t);
        }


        public virtual TypeSpec OnLoadBaseType()
        {
            if (Type == null)
                throw new InvalidOperationException("Don't know where to get base from when Type is null.");

            return Type.BaseType != null ? TypeResolver.FromType(Type.BaseType) : null;
        }


        public virtual string OnLoadNamespace()
        {
            if (Type == null)
                throw new InvalidOperationException("Don't know where to get namespace from when Type is null.");

            return Type.Namespace;
        }

        #region Overloaded operators

        public static implicit operator Type(TypeSpec typeSpec)
        {
            return typeSpec == null ? null : typeSpec.Type;
        }

        #endregion

        public override string ToString()
        {
            return Name;
        }


        public bool TryGetPropertyByName(string propertyName, bool ignoreCase, out PropertySpec propertySpec)
        {
            var stringComparison = ignoreCase
                ? StringComparison.InvariantCultureIgnoreCase
                : StringComparison.InvariantCulture;

            // TODO: Possible to optimize here by putting property names in a dictionary
            propertySpec = Properties.FirstOrDefault(x => string.Equals(x.Name, propertyName, stringComparison));
            return propertySpec != null;
        }


        protected internal abstract ConstructorSpec OnLoadConstructor();
        protected internal abstract IEnumerable<TypeSpec> OnLoadGenericArguments();
        protected internal abstract IEnumerable<TypeSpec> OnLoadInterfaces();
        protected internal abstract IEnumerable<PropertySpec> OnLoadProperties();
        protected internal abstract IEnumerable<PropertySpec> OnLoadRequiredProperties();
        protected internal abstract RuntimeTypeDetails OnLoadRuntimeTypeDetails();
        protected internal abstract PropertySpec OnWrapProperty(PropertyInfo property);
    }
}