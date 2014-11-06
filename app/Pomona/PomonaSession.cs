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

using Pomona.Common.Linq.NonGeneric;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.Queries;
using Pomona.RequestProcessing;
using Pomona.Routing;

namespace Pomona
{
    internal class PomonaSession : IPomonaSession
    {
        private readonly IContainer container;
        private readonly IRequestProcessorPipeline pipeline;
        private readonly IRouteActionResolver routeActionResolver;
        private readonly ITextSerializerFactory textSerializerFactory;
        private readonly TypeMapper typeMapper;
        private PomonaRequest currentRequest;


        public PomonaSession(TypeMapper typeMapper,
                             IRequestProcessorPipeline pipeline,
                             ITextSerializerFactory textSerializerFactory,
                             IRouteActionResolver routeActionResolver,
                             IContainer container = null)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            if (pipeline == null)
                throw new ArgumentNullException("pipeline");
            if (textSerializerFactory == null)
                throw new ArgumentNullException("textSerializerFactory");
            if (routeActionResolver == null)
                throw new ArgumentNullException("routeActionResolver");
            this.typeMapper = typeMapper;
            this.pipeline = pipeline;
            this.textSerializerFactory = textSerializerFactory;
            this.routeActionResolver = routeActionResolver;
            this.container = container;
        }


        public PomonaRequest CurrentRequest
        {
            get { return this.currentRequest; }
        }

        public TypeMapper TypeMapper
        {
            get { return this.typeMapper; }
        }


        public virtual PomonaResponse Dispatch(PomonaRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            if (request.Session != this)
                throw new ArgumentException("Request session is not same as this.");
            var savedOuterRequest = this.currentRequest;
            try
            {
                this.currentRequest = request;
                var result = this.pipeline.Process(request);

                var resultEntity = result.Entity;
                var resultAsQueryable = resultEntity as IQueryable;
                if (resultAsQueryable != null && request.ExecuteQueryable)
                    result = ExecuteQueryable(request, resultAsQueryable);

                if (request.AcceptType != null && !request.AcceptType.IsInstanceOfType(resultEntity))
                {
                    var route = request.Route;
                    var resultType = route.ResultType;
                    if (typeof(IQueryable).IsAssignableFrom(request.AcceptType) && route.IsSingle
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
                this.currentRequest = savedOuterRequest;
            }
        }


        public T GetInstance<T>()
        {
            // TODO: This should instead be injected to IOC container:
            if (typeof(T) == typeof(ITextDeserializer))
            {
                return
                    (T)
                        this.textSerializerFactory.GetDeserializer(
                            this.container.GetInstance<ISerializationContextProvider>());
            }
            if (typeof(T) == typeof(ITextSerializer))
            {
                return
                    (T)
                        this.textSerializerFactory.GetSerializer(
                            this.container.GetInstance<ISerializationContextProvider>());
            }

            if (typeof(T).IsAssignableFrom(GetType()))
                return (T)((object)this);
            return this.container.GetInstance<T>();
        }


        public IEnumerable<RouteAction> GetRouteActions(PomonaRequest request)
        {
            var route = request.Node.Route;
            return this.routeActionResolver.Resolve(route, request.Method);
        }


        private PomonaResponse ExecuteQueryable(PomonaRequest request, IQueryable resultAsQueryable)
        {
            var queryableActionResult = resultAsQueryable as IQueryableActionResult;
            if (queryableActionResult != null && queryableActionResult.Projection != null)
            {
                if (queryableActionResult.Projection != QueryProjection.AsEnumerable)
                {
                    var entity = queryableActionResult.Execute(queryableActionResult.Projection);
                    if (entity == null)
                        throw new ResourceNotFoundException("Resource not found.");
                    return new PomonaResponse(entity);
                }
                else
                {
                    resultAsQueryable = queryableActionResult.WrappedQueryable;
                }
            }

            var queryExecutor = (GetInstance<IPomonaDataSource>() as IQueryExecutor) ?? new DefaultQueryExecutor();
            var pomonaQuery = ParseQuery(request, resultAsQueryable.ElementType);
            return queryExecutor.ApplyAndExecute(resultAsQueryable, pomonaQuery);
        }


        private PomonaQuery ParseQuery(PomonaRequest request, Type rootType)
        {
            return new PomonaHttpQueryTransformer(this.typeMapper,
                                                  new QueryExpressionParser(
                                                      new QueryTypeResolver(this.typeMapper)))
                .TransformRequest(request, (ResourceType)this.typeMapper.GetClassMapping(rootType));
        }
    }
}