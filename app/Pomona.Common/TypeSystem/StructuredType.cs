#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    public abstract class StructuredType : RuntimeTypeSpec
    {
        private readonly Lazy<StructuredTypeDetails> structuredTypeDetails;
        private readonly Lazy<ReadOnlyCollection<StructuredType>> subTypes;
        private Delegate createUsingPropertySourceFunc;


        protected StructuredType(IStructuredTypeResolver typeResolver,
                                 Type type,
                                 Func<IEnumerable<TypeSpec>> genericArguments = null)
            : base(typeResolver, type, genericArguments)
        {
            this.subTypes = CreateLazy(() => typeResolver.LoadSubTypes(this).ToList().AsReadOnly());
            this.structuredTypeDetails = CreateLazy(() => typeResolver.LoadStructuredTypeDetails(this));
        }


        public virtual HttpMethod AllowedMethods => StructuredTypeDetails.AllowedMethods;

        public bool DeleteAllowed => StructuredTypeDetails.AllowedMethods.HasFlag(HttpMethod.Delete);

        public override bool IsAbstract => StructuredTypeDetails.IsAbstract;

        public override bool IsAlwaysExpanded => StructuredTypeDetails.AlwaysExpand;

        public bool MappedAsValueObject => StructuredTypeDetails.MappedAsValueObject;

        public Action<object> OnDeserialized => StructuredTypeDetails.OnDeserialized;

        public bool PatchAllowed => StructuredTypeDetails.AllowedMethods.HasFlag(HttpMethod.Patch);

        public bool PostAllowed => StructuredTypeDetails.AllowedMethods.HasFlag(HttpMethod.Post);

        public virtual StructuredProperty PrimaryId => StructuredTypeDetails.PrimaryId;

        public new virtual IEnumerable<StructuredProperty> Properties => base.Properties.Cast<StructuredProperty>();

        public virtual ResourceInfoAttribute ResourceInfo => DeclaredAttributes.OfType<ResourceInfoAttribute>().FirstOrDefault();

        public IEnumerable<StructuredType> SubTypes => this.subTypes.Value;

        public new IResourceTypeResolver TypeResolver => (IResourceTypeResolver)(((TypeSpec)this).TypeResolver);

        protected StructuredTypeDetails StructuredTypeDetails => this.structuredTypeDetails.Value;


        public override object Create(IConstructorPropertySource propertySource)
        {
            if (IsAbstract)
            {
                throw new PomonaSerializationException("Pomona was unable to instantiate type " + Name
                                                       + ", as it's an abstract type.");
            }

            if (Constructor == null)
            {
                throw new PomonaSerializationException("Pomona was unable to instantiate type " + Name
                                                       + ", Constructor property was null.");
            }

            if (this.createUsingPropertySourceFunc == null)
            {
                var param = Expression.Parameter(typeof(IConstructorPropertySource));
                var expr =
                    Expression.Lambda(
                        Expression.Invoke(Constructor.InjectingConstructorExpression, param),
                        param);
                this.createUsingPropertySourceFunc = expr.Compile();
            }

            return ((Func<IConstructorPropertySource, object>)this.createUsingPropertySourceFunc)(propertySource);
        }


        protected internal override IEnumerable<PropertySpec> OnLoadProperties()
        {
            return
                Type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Select(x => TypeResolver.WrapProperty(this, x));
        }


        protected internal override PropertySpec OnWrapProperty(PropertyInfo property)
        {
            return new StructuredProperty(TypeResolver, property, this);
        }


        protected override TypeSerializationMode OnLoadSerializationMode()
        {
            return TypeSerializationMode.Structured;
        }

        #region Nested type: ConstructorPropertySource

        private class ConstructorPropertySource : IConstructorPropertySource
        {
            private readonly IDictionary<PropertySpec, object> args;


            public ConstructorPropertySource(IDictionary<PropertySpec, object> args)
            {
                this.args = args;
            }


            public TContext Context<TContext>()
            {
                throw new NotImplementedException();
            }


            public TProperty GetValue<TProperty>(PropertyInfo propertyInfo, Func<TProperty> defaultFactory)
            {
                defaultFactory = defaultFactory ?? (() =>
                {
                    throw new InvalidOperationException("Unable to get required property.");
                });

                // TODO: Optimize a lot!!!
                return this.args
                           .Where(x => x.Key.PropertyInfo.Name == propertyInfo.Name)
                           .Select(x => (TProperty)x.Value)
                           .MaybeFirst()
                           .OrDefault(defaultFactory);
            }


            public TParentType Parent<TParentType>()
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}