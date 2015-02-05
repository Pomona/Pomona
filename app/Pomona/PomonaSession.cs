#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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

using Nancy;

using Pomona.Common.Internals;
using Pomona.Common.Linq.NonGeneric;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.Queries;
using Pomona.RequestProcessing;
using Pomona.Routing;

namespace Pomona
{
    internal class PomonaSession : IPomonaSession, IResourceResolver
    {
        private readonly IContainer container;
        private readonly IPomonaSessionFactory factory;


        public PomonaSession(IPomonaSessionFactory factory,
                             IContainer container = null)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");
            this.factory = factory;
            this.container = container;
        }


        public Route Routes
        {
            get { return Factory.Routes; }
        }

        public IPomonaSessionFactory Factory
        {
            get { return factory; }
        }

        public PomonaContext CurrentContext { get; private set; }

        public TypeMapper TypeMapper
        {
            get { return this.factory.TypeMapper; }
        }


        public virtual PomonaResponse Dispatch(PomonaRequest request)
        {
            var finalSegmentMatch = MatchUrlSegment(request);
            return this.Dispatch(new PomonaContext(finalSegmentMatch, request, executeQueryable : true));
        }


        public virtual PomonaResponse Dispatch(PomonaContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (context.Session != this)
                throw new ArgumentException("Request session is not same as this.");
            var savedOuterContext = this.CurrentContext;
            try
            {
                this.CurrentContext = context;
                var result = this.Factory.Pipeline.Process(context);

                var resultEntity = result.Entity;
                var resultAsQueryable = resultEntity as IQueryable;
                if (resultAsQueryable != null && context.ExecuteQueryable)
                    result = ExecuteQueryable(context, resultAsQueryable);

                if (context.AcceptType != null && !context.AcceptType.IsInstanceOfType(resultEntity))
                {
                    var route = context.Route;
                    var resultType = route.ResultType;
                    if (typeof(IQueryable).IsAssignableFrom(context.AcceptType) && route.IsSingle
                        && resultType.Type.IsInstanceOfType(resultEntity))
                    {
                        var array = Array.CreateInstance(resultType, 1);
                        array.SetValue(resultEntity, 0);
                        return new PomonaResponse(array.AsQueryable());
                    }

                    throw new InvalidOperationException("Result is not of accepted type.");
                }
                return result;
            }
            finally
            {
                this.CurrentContext = savedOuterContext;
            }
        }


        public T GetInstance<T>()
        {
            // TODO: This should instead be injected to IOC container:
            if (typeof(T) == typeof(ISerializationContextProvider))
            {
                return
                    (T)(object)new ServerSerializationContextProvider(GetInstance<IUriResolver>(),
                                                                      GetInstance<IResourceResolver>(),
                                                                      GetInstance<NancyContext>());
            }

            if (typeof(T) == typeof(ITextDeserializer))
            {
                return
                    (T)
                        this.Factory.SerializerFactory.GetDeserializer(
                            this.GetInstance<ISerializationContextProvider>());
            }
            if (typeof(T) == typeof(ITextSerializer))
            {
                return
                    (T)
                        this.Factory.SerializerFactory.GetSerializer(
                            this.GetInstance<ISerializationContextProvider>());
            }

            if (typeof(T).IsAssignableFrom(GetType()))
                return (T)((object)this);
            return this.container.GetInstance<T>();
        }


        public IEnumerable<RouteAction> GetRouteActions(PomonaContext context)
        {
            var route = context.Node.Route;
            return this.Factory.ActionResolver.Resolve(route, context.Method);
        }


        public object ResolveUri(string uri)
        {
            var pomonaResponse = this.Get(uri);

            if ((int)pomonaResponse.StatusCode >= 400)
                throw new ReferencedResourceNotFoundException(uri, pomonaResponse);

            return pomonaResponse.Entity;
        }


        private UrlSegment MatchUrlSegment(PomonaRequest request)
        {
            var match = new PomonaRouteResolver(this.Routes).Resolve(this, request.RelativePath);

            if (match == null)
                throw new ResourceNotFoundException("Resource not found.");

            var finalSegmentMatch = match.Root.SelectedFinalMatch;
            if (finalSegmentMatch == null)
            {
                // Route conflict resolution:
                var node = match.Root.NextConflict;
                while (node != null)
                {
                    var actualResultType = node.ActualResultType;
                    // Reduce using input type difference
                    var validSelection =
                        node.Children.Where(x => x.Route.InputType.IsAssignableFrom(actualResultType))
                            .SingleOrDefaultIfMultiple();
                    if (validSelection == null)
                        throw new ResourceNotFoundException("No route alternative found due to conflict.");
                    node.SelectedChild = validSelection;
                    node = node.NextConflict;
                }
                finalSegmentMatch = match.Root.SelectedFinalMatch;
            }
            return finalSegmentMatch;
        }


        private PomonaResponse ExecuteQueryable(PomonaContext context, IQueryable resultAsQueryable)
        {
            var queryableActionResult = resultAsQueryable as IQueryableActionResult;
            int? defaultPageSize = null;
            if (queryableActionResult != null && queryableActionResult.Projection != null)
            {
                defaultPageSize = queryableActionResult.DefaultPageSize;
                if (queryableActionResult.Projection != QueryProjection.AsEnumerable)
                {
                    var entity = queryableActionResult.Execute(queryableActionResult.Projection);
                    if (entity == null)
                        throw new ResourceNotFoundException("Resource not found.");
                    return new PomonaResponse(entity);
                }
                resultAsQueryable = queryableActionResult.WrappedQueryable;
            }

            var queryExecutor = (GetInstance<IPomonaDataSource>() as IQueryExecutor) ?? new DefaultQueryExecutor();
            var pomonaQuery = ParseQuery(context, resultAsQueryable.ElementType, defaultPageSize);
            return queryExecutor.ApplyAndExecute(resultAsQueryable, pomonaQuery);
        }


        private PomonaQuery ParseQuery(PomonaContext context, Type rootType, int? defaultPageSize = null)
        {
            return new PomonaHttpQueryTransformer(this.TypeMapper,
                                                  new QueryExpressionParser(
                                                      new QueryTypeResolver(this.TypeMapper)))
                .TransformRequest(context, (ResourceType)this.TypeMapper.GetClassMapping(rootType), defaultPageSize);
        }
    }
}