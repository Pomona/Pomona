#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.Linq;

namespace Pomona.Common.ExtendedResources
{
    public class ExtendedQueryProvider : QueryProviderBase
    {
        private static readonly MethodInfo queryableExecuteGenericMethod =
            ReflectionHelper.GetMethodDefinition<IQueryProvider>(x => x.Execute<object>(null));

        private readonly ExtendedResourceMapper extendedResourceMapper;


        public ExtendedQueryProvider(ExtendedResourceMapper extendedResourceMapper)
        {
            if (extendedResourceMapper == null)
                throw new ArgumentNullException(nameof(extendedResourceMapper));
            this.extendedResourceMapper = extendedResourceMapper;
        }


        public override IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new ExtendedQueryable<TElement>(this, expression);
        }


        public override object Execute(Expression expression, Type returnType)
        {
            var visitor = new TransformAdditionalPropertiesToAttributesVisitor(this.extendedResourceMapper);
            var transformedExpression = visitor.Visit(expression);
            if (visitor.Root == null)
                throw new Exception("Unable to find queryable source in expression.");

            var transformedReturnType = visitor.VisitType(returnType);
            var result =
                queryableExecuteGenericMethod.MakeGenericMethod(transformedReturnType).Invoke(
                    visitor.Root.WrappedSource.Provider,
                    new object[] { transformedExpression });

            var wrapResource = this.extendedResourceMapper.WrapResource(result,
                                                                        transformedReturnType,
                                                                        returnType);
            return wrapResource;
        }
    }
}
