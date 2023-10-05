#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;

namespace Pomona.Common.TypeSystem
{
    internal class QueryResultType<T> : QueryResultType
    {
        public QueryResultType(IStructuredTypeResolver typeResolver)
            : base(typeResolver, typeof(QueryResult<T>), GetGenericArguments(typeResolver))
        {
        }


        protected internal override ConstructorSpec OnLoadConstructor()
        {
            Expression<Func<IConstructorControl<QueryResult<T>>, QueryResult<T>>> expr =
                x =>
                    new QueryResult<T>(x.Requires().Items, x.Optional().Skip, x.Optional().TotalCount,
                                       x.Optional().Previous, x.Optional().Next);
            return new ConstructorSpec(expr);
        }


        private static Func<IEnumerable<TypeSpec>> GetGenericArguments(ITypeResolver typeResolver)
        {
            if (typeResolver == null)
                throw new ArgumentNullException(nameof(typeResolver));
            return () => new[] { typeResolver.FromType(typeof(T)) };
        }
    }

    public class QueryResultType : StructuredType
    {
        protected QueryResultType(IStructuredTypeResolver typeResolver,
                                  Type type,
                                  Func<IEnumerable<TypeSpec>> genericArguments = null)
            : base(typeResolver, type, genericArguments)
        {
        }


        public override TypeSerializationMode SerializationMode => TypeSerializationMode.Structured;


        public new static ITypeFactory GetFactory()
        {
            return new TypeFactory();
        }


        protected internal override IEnumerable<PropertySpec> OnLoadProperties()
        {
            var includedProps = new List<string>() { "TotalCount", "Items", "Skip", "Previous", "Next", "DebugInfo" };
            return
                Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(x => includedProps.Contains(x.Name))
                    .Select(x => TypeResolver.WrapProperty(this, x))
                    .OrderBy(x => includedProps.IndexOf(x.Name));
        }


        protected internal override PropertySpec OnWrapProperty(PropertyInfo property)
        {
            if (property.Name == "Items")
                return new ItemsPropertySpec(TypeResolver, property, this);
            return base.OnWrapProperty(property);
        }


        internal class ItemsPropertySpec : StructuredProperty
        {
            public ItemsPropertySpec(IStructuredTypeResolver typeResolver, PropertyInfo propertyInfo, QueryResultType reflectedType)
                : base(typeResolver, propertyInfo, reflectedType)
            {
            }


            public override ExpandMode ExpandMode => ExpandMode.Full;
        }

        #region Nested type: QueryResultTypeFactory

        private class TypeFactory : ITypeFactory
        {
            private static readonly Func<Type, TypeFactory, IStructuredTypeResolver, bool, TypeSpec> createFromTypeInvoker = GenericInvoker
                .Instance<TypeFactory>()
                .CreateFunc1<IStructuredTypeResolver, bool, TypeSpec>(x => x.CreateFromType<object>(null, false));


            private TypeSpec CreateFromType<T>(IStructuredTypeResolver typeResolver, bool createSet)
            {
                if (createSet)
                    return new QuerySetResultType<T>(typeResolver);
                return new QueryResultType<T>(typeResolver);
            }


            public TypeSpec CreateFromType(ITypeResolver typeResolver, Type type)
            {
                if (typeResolver == null)
                    throw new ArgumentNullException(nameof(typeResolver));
                var structuredTypeResolver = typeResolver as IStructuredTypeResolver;
                if (structuredTypeResolver == null)
                {
                    throw new ArgumentException("typerResolver must be of type " + typeof(IStructuredTypeResolver).FullName,
                                                nameof(typeResolver));
                }
                if (type == null)
                    throw new ArgumentNullException(nameof(type));
                if (!typeof(QueryResult).IsAssignableFrom(type))
                    return null;

                Type[] genArgs;
                if (type.TryExtractTypeArguments(typeof(QueryResult<>), out genArgs))
                    return createFromTypeInvoker(genArgs[0], this, structuredTypeResolver, false);
                if (type.TryExtractTypeArguments(typeof(QuerySetResult<>), out genArgs))
                    return createFromTypeInvoker(genArgs[0], this, structuredTypeResolver, true);
                return new QueryResultType(structuredTypeResolver, type);
            }


            public int Priority => 0;
        }

        #endregion
    }
}

