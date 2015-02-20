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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

using Pomona.Common.Internals;

namespace Pomona.Common.TypeSystem
{
    public class RuntimeTypeSpec : TypeSpec
    {
        private readonly Lazy<IEnumerable<TypeSpec>> genericArguments;
        private readonly Lazy<ReadOnlyCollection<TypeSpec>> interfaces;
        private readonly Lazy<ReadOnlyCollection<PropertySpec>> properties;
        private readonly Lazy<RuntimeTypeDetails> runtimeTypeDetails;
        private readonly Lazy<ConstructorSpec> constructor;

        public RuntimeTypeSpec(ITypeResolver typeResolver,
            Type type,
            Func<IEnumerable<TypeSpec>> genericArguments = null)
            : base(typeResolver, type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            this.properties = CreateLazy(() => typeResolver.LoadProperties(this).ToList().AsReadOnly());
            this.interfaces = CreateLazy(() => typeResolver.LoadInterfaces(this).ToList().AsReadOnly());
            this.genericArguments =
                CreateLazy(genericArguments ?? (() => typeResolver.LoadGenericArguments(this).ToList().AsReadOnly()));
            runtimeTypeDetails = CreateLazy(() => typeResolver.LoadRuntimeTypeDetails(this));
            this.constructor = CreateLazy(() => typeResolver.LoadConstructor(this));
        }


        protected RuntimeTypeDetails RuntimeTypeDetails
        {
            get { return runtimeTypeDetails.Value; }
        }

        public virtual IEnumerable<TypeSpec> GenericArguments
        {
            get { return this.genericArguments.Value; }
        }


        public override IEnumerable<Attribute> InheritedAttributes
        {
            get
            {
                if (BaseType != null)
                    return BaseType.Attributes;
                return Enumerable.Empty<Attribute>();
            }
        }

        public override IEnumerable<TypeSpec> Interfaces
        {
            get { return this.interfaces.Value; }
        }

        public override IEnumerable<PropertySpec> Properties
        {
            get { return this.properties.Value; }
        }

        public override IEnumerable<PropertySpec> RequiredProperties
        {
            get { return this.TypeResolver.LoadRequiredProperties(this); }
        }

        public override ConstructorSpec Constructor
        {
            get { return constructor.Value; }
        }


        public static ITypeFactory GetFactory()
        {
            return new RuntimeTypeSpecFactory();
        }


        public override string ToString()
        {
            return IsGenericType ? string.Format("{0}<{1}>", Name.Split('`')[0], string.Join(", ", GenericArguments)) : Name;
        }


        protected virtual TypeSerializationMode OnLoadSerializationMode()
        {
            if (Type.IsAnonymous())
                return TypeSerializationMode.Structured;
            return TypeSerializationMode.Value;
            //return TypeSerializationMode.Complex;
        }


        protected internal override IEnumerable<PropertySpec> OnLoadRequiredProperties()
        {
            return Enumerable.Empty<PropertySpec>();
        }

        public override bool IsAbstract
        {
            get { return Type.IsAbstract; }
        }

        protected internal override RuntimeTypeDetails OnLoadRuntimeTypeDetails()
        {
            return new RuntimeTypeDetails(OnLoadSerializationMode());
        }


        protected internal override IEnumerable<TypeSpec> OnLoadGenericArguments()
        {
            if (Type == null)
                return Enumerable.Empty<TypeSpec>();
            return Type.GetGenericArguments().Select(x => TypeResolver.FromType(x));
        }


        protected internal override IEnumerable<TypeSpec> OnLoadInterfaces()
        {
            return Type.GetInterfaces().Select(TypeResolver.FromType);
        }


        protected internal override IEnumerable<PropertySpec> OnLoadProperties()
        {
            return
                Type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
                                   | BindingFlags.NonPublic).Select(
                                       x => TypeResolver.WrapProperty(this, x));
        }


        protected internal override PropertySpec OnWrapProperty(PropertyInfo property)
        {
            return new RuntimePropertySpec(TypeResolver, property, this);
        }


        protected internal override ConstructorSpec OnLoadConstructor()
        {
            return null;
        }


        public override TypeSerializationMode SerializationMode
        {
            get { return RuntimeTypeDetails.SerializationMode; }
        }

        public override bool IsNullable
        {
            get { return Type.IsNullable(); }
        }

        #region Nested type: RuntimeTypeSpecFactory

        public class RuntimeTypeSpecFactory : ITypeFactory
        {
            public int Priority
            {
                get { return 200; }
            }


            public TypeSpec CreateFromType(ITypeResolver typeResolver, Type type)
            {
                if (typeResolver == null)
                    throw new ArgumentNullException("typeResolver");
                if (type == null)
                    throw new ArgumentNullException("type");

                return new RuntimeTypeSpec(typeResolver, type);
            }
        }

        #endregion
    }
}