#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    public class RuntimeTypeSpec : TypeSpec
    {
        private readonly Lazy<ConstructorSpec> constructor;
        private readonly Lazy<IEnumerable<TypeSpec>> genericArguments;
        private readonly Lazy<ReadOnlyCollection<TypeSpec>> interfaces;
        private readonly Lazy<ReadOnlyCollection<PropertySpec>> properties;
        private readonly Lazy<RuntimeTypeDetails> runtimeTypeDetails;


        public RuntimeTypeSpec(ITypeResolver typeResolver,
                               Type type,
                               Func<IEnumerable<TypeSpec>> genericArguments = null)
            : base(typeResolver, type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            this.properties = CreateLazy(() => typeResolver.LoadProperties(this).ToList().AsReadOnly());
            this.interfaces = CreateLazy(() => typeResolver.LoadInterfaces(this).ToList().AsReadOnly());
            this.genericArguments =
                CreateLazy(genericArguments ?? (() => typeResolver.LoadGenericArguments(this).ToList().AsReadOnly()));
            this.runtimeTypeDetails = CreateLazy(() => typeResolver.LoadRuntimeTypeDetails(this));
            this.constructor = CreateLazy(() => typeResolver.LoadConstructor(this));
        }


        public override ConstructorSpec Constructor
        {
            get { return this.constructor.Value; }
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

        public override bool IsAbstract
        {
            get { return Type.IsAbstract; }
        }

        public override bool IsNullable
        {
            get { return Type.IsNullable(); }
        }

        public override string NameWithGenericArguments
        {
            get
            {
                return IsGenericType
                    ? string.Format("{0}<{1}>", Name.Split('`')[0],
                                    string.Join(", ", GenericArguments.Select(x => x.NameWithGenericArguments)))
                    : Name;
            }
        }

        public override IEnumerable<PropertySpec> Properties
        {
            get { return this.properties.Value; }
        }

        public override IEnumerable<PropertySpec> RequiredProperties
        {
            get { return TypeResolver.LoadRequiredProperties(this); }
        }

        public override TypeSerializationMode SerializationMode
        {
            get { return RuntimeTypeDetails.SerializationMode; }
        }

        protected RuntimeTypeDetails RuntimeTypeDetails
        {
            get { return this.runtimeTypeDetails.Value; }
        }


        public static ITypeFactory GetFactory()
        {
            return new RuntimeTypeSpecFactory();
        }


        public override string ToString()
        {
            return IsGenericType ? string.Format("{0}<{1}>", Name.Split('`')[0], string.Join(", ", GenericArguments)) : Name;
        }


        protected internal override ConstructorSpec OnLoadConstructor()
        {
            return null;
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
            // If you want properties map stuff to StructuredType instead.
            // Note that properties that are not externally exposed should _not_ be mapped.
            return Enumerable.Empty<PropertySpec>();
        }


        protected internal override IEnumerable<PropertySpec> OnLoadRequiredProperties()
        {
            return Enumerable.Empty<PropertySpec>();
        }


        protected internal override RuntimeTypeDetails OnLoadRuntimeTypeDetails()
        {
            return new RuntimeTypeDetails(OnLoadSerializationMode());
        }


        protected internal override PropertySpec OnWrapProperty(PropertyInfo property)
        {
            return new RuntimePropertySpec(TypeResolver, property, this);
        }


        protected virtual TypeSerializationMode OnLoadSerializationMode()
        {
            if (Type.IsAnonymous())
                return TypeSerializationMode.Structured;
            return TypeSerializationMode.Value;
            //return TypeSerializationMode.Complex;
        }

        #region Nested type: RuntimeTypeSpecFactory

        public class RuntimeTypeSpecFactory : ITypeFactory
        {
            public TypeSpec CreateFromType(ITypeResolver typeResolver, Type type)
            {
                if (typeResolver == null)
                    throw new ArgumentNullException(nameof(typeResolver));
                if (type == null)
                    throw new ArgumentNullException(nameof(type));

                return new RuntimeTypeSpec(typeResolver, type);
            }


            public int Priority
            {
                get { return 200; }
            }
        }

        #endregion
    }
}