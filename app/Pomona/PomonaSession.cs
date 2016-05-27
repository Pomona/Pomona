#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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


        public PomonaSession(IPomonaSessionFactory factory, IContainer container, IUriResolver uriResolver)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (uriResolver == null)
                throw new ArgumentNullException(nameof(uriResolver));
            Factory = factory;
            this.container = container;
            UriResolver = uriResolver;
        }


        public IPomonaSessionFactory Factory { get; }

        private PomonaContext CurrentContext { get; set; }


        private async Task<PomonaResponse> DispatchInternal(PomonaContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (context.Session != this)
                throw new ArgumentException("Request session is not same as this.");
            var savedOuterContext = CurrentContext;
            try
            {
                CurrentContext = context;
                var result = await Factory.Pipeline.Process(context);

                var resultEntity = result.Entity;
                var resultAsQueryable = resultEntity as IQueryable;
                if (resultAsQueryable != null && context.ExecuteQueryable)
                    result = await ExecuteQueryable(context, resultAsQueryable);

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
                CurrentContext = savedOuterContext;
            }
        }


        private async Task<PomonaResponse> ExecuteQueryable(PomonaContext context, IQueryable resultAsQueryable)
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
                    return new PomonaResponse(entity, expandedPaths : context.ExpandedPaths);
                }
                resultAsQueryable = queryableActionResult.WrappedQueryable;
            }

            var queryExecutor = GetInstance<IQueryExecutor>();
            var pomonaQuery = ParseQuery(context, resultAsQueryable.ElementType, defaultPageSize);
            return await queryExecutor.ApplyAndExecute(resultAsQueryable, pomonaQuery);
        }


        private PomonaQuery ParseQuery(PomonaContext context, Type rootType, int? defaultPageSize = null)
        {
            return new PomonaHttpQueryTransformer(TypeResolver,
                                                  new QueryExpressionParser(
                                                      new QueryTypeResolver(TypeResolver)))
                .TransformRequest(context, (ResourceType)TypeResolver.FromType(rootType), defaultPageSize);
        }


        public virtual async Task<PomonaResponse> Dispatch(PomonaRequest request)
        {
            var finalSegmentMatch = await new PomonaRouteResolver(Factory.Routes).Resolve(this, request);
            return await Dispatch(new PomonaContext(finalSegmentMatch, request, executeQueryable : true));
        }


        public virtual async Task<PomonaResponse> Dispatch(PomonaContext context)
        {
            try
            {
                return await DispatchInternal(context);
            }
            catch (Exception ex)
            {
                if (!context.HandleException)
                    throw;

                var error = this.container.GetInstance<IPomonaErrorHandler>().HandleException(ex);
                if (error == null)
                    throw;

                return new PomonaResponse(error.Entity ?? PomonaResponse.NoBodyEntity,
                                          error.StatusCode,
                                          responseHeaders : error.ResponseHeaders);
            }
        }


        public T GetInstance<T>()
        {
            if (typeof(T).IsAssignableFrom(GetType()))
                return (T)((object)this);
            return this.container.GetInstance<T>();
        }


        public IEnumerable<RouteAction> GetRouteActions(PomonaContext context)
        {
            var route = context.Node.Route;
            return Factory.ActionResolver.Resolve(route, context.Method);
        }


        object IResourceResolver.ResolveUri(string uri)
        {
            var pomonaResponse = Task.Run(() => this.Get(uri)).Result;

            if ((int)pomonaResponse.StatusCode >= 400)
                throw new ReferencedResourceNotFoundException(uri, pomonaResponse);

            return pomonaResponse.Entity;
        }


        public ISerializationContextProvider SerializationContextProvider
            => new ServerSerializationContextProvider(TypeResolver, UriResolver, this,
                                                      this.container);

        public TypeMapper TypeResolver
        {
            get { return Factory.TypeMapper; }
        }

        public IUriResolver UriResolver { get; }
        ITextDeserializer IPomonaSession.Deserializer => Factory.SerializerFactory.GetDeserializer(SerializationContextProvider);
    }
}