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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using Nancy;
using Pomona.CodeGen;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.Internals;
using Pomona.Queries;
using Pomona.RequestProcessing;
using Pomona.Schemas;

namespace Pomona
{
    public abstract class PomonaModule : NancyModule
    {
        private static readonly MethodInfo getByIdMethod =
            ReflectionHelper.GetMethodDefinition<IPomonaDataSource>(dst => dst.GetById<object>(null));

        private static readonly MethodInfo postGenericMethod =
            ReflectionHelper.GetMethodDefinition<PomonaModule>(dst => dst.InvokeDataSourcePost((object)null));

        private static readonly MethodInfo patchGenericMethod =
            ReflectionHelper.GetMethodDefinition<PomonaModule>(dst => dst.InvokeDataSourcePatch((object)null));

        private readonly IPomonaDataSource dataSource;
        private readonly IDeserializer deserializer;
        private readonly IHttpQueryTransformer queryTransformer;
        private readonly IServiceLocator serviceLocator;
        private readonly TypeMapper typeMapper;


        protected PomonaModule(
            IPomonaDataSource dataSource,
            TypeMapper typeMapper,
            IServiceLocator serviceLocator)
        {
            // HACK TO SUPPORT NANCY TESTING (set a valid host name)
            Before += ctx =>
                {
                    if (String.IsNullOrEmpty(ctx.Request.Url.HostName))
                    {
                        ctx.Request.Url.HostName = "test";
                    }
                    return null;
                };

            this.dataSource = dataSource;

            this.typeMapper = typeMapper;
            this.serviceLocator = serviceLocator;

            queryTransformer = new PomonaHttpQueryTransformer(
                this.typeMapper, new QueryExpressionParser(new QueryTypeResolver(this.typeMapper)));

            deserializer = typeMapper.SerializerFactory.GetDeserializer();

            foreach (var transformedType in this.typeMapper
                                                .TransformedTypes
                                                .Select(x => x.UriBaseType)
                                                .Where(x => x != null && !x.IsValueType && !x.IsAnonymous())
                                                .Distinct())
            {
                RegisterRoutesFor(transformedType);
            }

            Get["/schemas"] = x => GetSchemas();

            Get[String.Format("/{0}.dll", this.typeMapper.Filter.GetClientAssemblyName())] = x => GetClientLibrary();

            RegisterClientNugetPackageRoute();

            Get["/"] = x => GetJsonBrowserHtmlResponse();
            RegisterResourceContent("antlr3-all-min.js");
            RegisterResourceContent("antlr3-all.js");
            RegisterResourceContent("PomonaQueryJsLexer.js");
            RegisterResourceContent("PomonaQueryJsParser.js");
            RegisterResourceContent("QueryEditor.css");
            RegisterResourceContent("QueryEditor.js");
        }

        public IPomonaDataSource DataSource
        {
            get { return dataSource; }
        }


        public ITypeMapper TypeMapper
        {
            get { return typeMapper; }
        }

        public virtual IResourceResolver ResourceResolver
        {
            get { return new ResourceResolver(typeMapper, Context, serviceLocator); }
        }


        public void WriteClientLibrary(Stream stream, bool embedPomonaClient = true)
        {
            ClientLibGenerator.WriteClientLibrary(typeMapper, stream, embedPomonaClient);
        }

        public PomonaResponse Query(PomonaQuery query)
        {
            throw new NotImplementedException();
            //return dataSource.Query(query);
        }

        private object InvokeDataSourcePatch<T>(T entity) where T:class
        {
            if (!((TransformedType)typeMapper.GetClassMapping<T>()).PatchAllowed)
                throw new PomonaException("Method PATCH not allowed", null, HttpStatusCode.MethodNotAllowed);
            return dataSource.Patch(entity);
        }

        private object InvokeDataSourcePost<T>(T entity) where T : class
        {
            if (!((TransformedType)typeMapper.GetClassMapping<T>()).PostAllowed)
                throw new PomonaException("Method POST not allowed", null, HttpStatusCode.MethodNotAllowed);
            return dataSource.Post(entity);
        }


        public object Deserialize(TransformedType expectedBaseType, Stream body, object patchedObject = null)
        {
            using (var textReader = new StreamReader(body))
            {
                var deserializationContext = new ServerDeserializationContext(TypeMapper, this.ResourceResolver);
                return deserializer.Deserialize(textReader, expectedBaseType, deserializationContext,
                                                patchedObject);
            }
        }

        private PomonaResponse PostOrPatch(TransformedType transformedType, Stream body,
                                           object patchedObject = null)
        {
            var postResource = Deserialize(transformedType, body, patchedObject);
            var method = patchedObject != null ? patchGenericMethod : postGenericMethod;
            var postResponse = method.MakeGenericMethod(postResource.GetType())
                                     .Invoke(this, new[] { postResource });

            var successStatusCode = patchedObject != null ? HttpStatusCode.OK : HttpStatusCode.Created;
            var expandedPaths = string.Join(",", Request.Headers["X-Pomona-Expand"]);

            return new PomonaResponse(postResponse, successStatusCode, expandedPaths);
        }


        public object GetById(TransformedType transformedType, object id)
        {
            return getByIdMethod.MakeGenericMethod(transformedType.MappedTypeInstance)
                                .Invoke(dataSource, new[] { id });
        }


        public PomonaResponse GetPropertyAsJson(
            TransformedType transformedType, object id, string propertyName, string expand)
        {
            // Note this is NOT optimized, as we should make the API in a way where it's possible to select by parent id.
            propertyName = propertyName.ToLower();

            var o = GetById(transformedType, id);
            var mappedType = (TransformedType)typeMapper.GetClassMapping(o.GetType());

            var property =
                mappedType.Properties.OfType<PropertyMapping>()
                          .FirstOrDefault(
                              x => String.Equals(propertyName, x.UriName, StringComparison.InvariantCultureIgnoreCase));

            if (property == null)
                throw new ResourceNotFoundException("Resource not found.");

            var propertyValue = property.Getter(o);
            var propertyType = property.PropertyType;

            return
                new PomonaResponse(propertyValue, expandedPaths: expand, resultType: propertyType);
        }


        private void RegisterResourceContent(string name)
        {
            var mediaType = "text/html";
            if (name.EndsWith(".js"))
                mediaType = "text/javascript";
            if (name.EndsWith(".css"))
                mediaType = "text/css";

            var resourceName = "Pomona.Content." + name;
            Get["/" + name] = x =>
                              Response.FromStream(
                                  () => Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName),
                                  mediaType);
        }

        private object GetJsonBrowserHtmlResponse()
        {
            var resourceName = "Pomona.Content.jsonbrowser.html";
            return
                Response.FromStream(
                    () => Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName), "text/html");
        }


        public PomonaResponse GetAsJson(TransformedType transformedType, object id, string expand)
        {
            var o = GetById(transformedType, id);
            return
                new PomonaResponse(o, expandedPaths: expand, resultType: transformedType);
        }


        private Response GetClientLibrary()
        {
            var response = new Response();

            response.Contents = stream => WriteClientLibrary(stream);
            response.ContentType = "binary/octet-stream";

            return response;
        }


        private Response GetClientNugetPackage()
        {
            var response = new Response();

            var packageBuilder = new ClientNugetPackageBuilder(typeMapper);
            response.Contents = stream =>
                {
                    using (var memstream = new MemoryStream())
                    {
                        packageBuilder.BuildPackage(memstream);
                        var bytes = memstream.ToArray();
                        stream.Write(bytes, 0, bytes.Length);
                    }
                };
            response.ContentType = "binary/octet-stream";

            return response;
        }


        private Response GetSchemas()
        {
            var res = new Response();

            var schemas = new SchemaGenerator(typeMapper).Generate().ToJson();
            res.ContentsFromString(schemas);
            res.ContentType = "text/plain; charset=utf-8";

            return res;
        }


        private PomonaResponse PostFromJson(TransformedType transformedType)
        {
            if (!transformedType.PostAllowed)
                ThrowMethodNotAllowedForType(transformedType.AllowedMethods);

            return PostOrPatch(transformedType, Request.Body);
        }

        private PomonaResponse Query(TransformedType transformedType)
        {
            // REMOVE THIS

            var query = queryTransformer.TransformRequest(Context, transformedType);

            return Query(query);
        }


        private void RegisterClientNugetPackageRoute()
        {
            var packageBuilder = new ClientNugetPackageBuilder(typeMapper);
            Get["/client.nupkg"] = x => Response.AsRedirect(packageBuilder.PackageFileName);
            Get["/" + packageBuilder.PackageFileName] = x => GetClientNugetPackage();
        }


        protected virtual Exception UnwrapException(Exception exception)
        {
            if (exception is TargetInvocationException || exception is RequestExecutionException)
            {
                return exception.InnerException != null ? UnwrapException(exception.InnerException) : exception;
            }
            return exception;
        }

        protected virtual PomonaError OnException(Exception exception)
        {
            if (exception is PomonaSerializationException)
            {
                return new PomonaError(HttpStatusCode.BadRequest, exception.Message);
            }
            if (exception is PomonaException)
            {
                return new PomonaError(((PomonaException)exception).StatusCode);
            }
            return null;
        }

        private void Register(RouteBuilder routeBuilder, string path, Func<dynamic, PomonaResponse> handler)
        {
            routeBuilder[path] = x =>
                {
                    try
                    {
                        var pomonaResponse = (PomonaResponse)handler(x);

                        if ((int)pomonaResponse.StatusCode >= 400)
                            SetErrorHandled();

                        return pomonaResponse;
                    }
                    catch (Exception ex)
                    {
                        var error = OnException(UnwrapException(ex));
                        if (error == null)
                            throw;

                        SetErrorHandled();
                        return new PomonaResponse(error.Entity ?? PomonaResponse.NoBodyEntity,
                                                  error.StatusCode, responseHeaders: error.ResponseHeaders);
                    }
                };
        }

        private void SetErrorHandled()
        {
            Context.Items["ERROR_HANDLED"] = true;
        }

        private void RegisterRoutesFor(TransformedType type)
        {
            var path = "/" + type.UriRelativePath;
            //Console.WriteLine("Registering path " + path);

            //Register(Get, path + "/{id}", x => GetAsJson(type, x.id));

            //foreach (var prop in type.Properties)
            //{
            //    var transformedProp = prop as PropertyMapping;
            //    if (transformedProp != null && transformedProp.IsOneToManyCollection
            //        && transformedProp.ElementForeignKey != null)
            //    {
            //        var collectionElementType = (TransformedType)prop.PropertyType.ElementType;
            //        var elementForeignKey = transformedProp.ElementForeignKey;

            //        Register(Get, path + "/{id}/" + transformedProp.UriName,
            //                 x => GetByForeignKeyPropertyAsJson(collectionElementType, elementForeignKey, x.id));
            //    }
            //}

            //Register(Get, path + "/{id}/{propname}", x => GetPropertyFromEntityAsJson(type, x.id, x.propname));

            //Register(Get, path, x => Query(type));
            Register(Get, path + "/{remaining*}", x => GetResource());
            Register(Get, path, x => GetResource());

            Register(Post, path + "/{id}", x => PostToResource(type, x.id));

            Register(Patch, path + "/{id}", x => PatchFromJson(type, x.id));

            Register(Post, path, x => GetResource());

        }

        private PomonaResponse GetResource()
        {
            var pathNodes = new LinkedList<string>(Request.Url.Path.Split('/')).First;
            var rootNode = new DataSourceRootNode(TypeMapper, dataSource);
            PathNode node = rootNode;
            foreach (var pathPart in pathNodes.WalkTree(x => x.Next).Skip(1).Select(x => x.Value))
            {
                node = node.GetChildNode(pathPart);
            }

            var pomonaRequest = new PomonaRequest(node, Context, ResourceResolver);

            if (!node.AllowedMethods.HasFlag(pomonaRequest.Method))
                ThrowMethodNotAllowedForType(node.AllowedMethods);

            var processors =
                node.GetRequestProcessors(pomonaRequest).Concat(
                    new DefaultGetRequestProcessor(node.GetQueryExecutor()));
            return processors.Select(x => x.Process(pomonaRequest)).FirstOrDefault(x => x != null);
        }


        private PomonaResponse PostToResource(TransformedType type, object id, string actionName = "")
        {
            var o = GetById(type, id);
            var mappedType = (TransformedType)typeMapper.GetClassMapping(o.GetType());
            var form = Deserialize(null, Request.Body);

            var handlers =
                mappedType.PostHandlers.Where(
                    x => String.Equals(actionName, x.UriName ?? "", StringComparison.InvariantCultureIgnoreCase))
                          .Where(x => x.FormType.MappedTypeInstance.IsInstanceOfType(form))
                          .ToList();

            if (handlers.Count < 1)
                throw new ResourceNotFoundException("TODO: Should throw method not allowed..");

            if (handlers.Count > 1)
                throw new NotImplementedException(
                    "TODO: Overload resolution not fully implemented when posting to a resource.");

            var handler = handlers[0];
            var result = handler.Method.Invoke(DataSource, new[] { o, form });

            return new PomonaResponse(result);
        }


        private PomonaResponse PatchFromJson(TransformedType transformedType, object id)
        {
            if (!transformedType.PatchAllowed)
                ThrowMethodNotAllowedForType(transformedType.AllowedMethods);

            var ifMatch = GetIfMatchFromRequest();

            var o = GetById(transformedType, id);

            if (o != null && ifMatch != null)
            {
                var etagProp = transformedType.ETagProperty;
                if (etagProp == null)
                    throw new InvalidOperationException("Unable to perform If-Match on entity with no etag.");

                if ((string)etagProp.Getter(o) != ifMatch)
                {
                    throw new ResourcePreconditionFailedException("Etag of entity did not match If-Match header.");
                }
            }

            var objType = (TransformedType)typeMapper.GetClassMapping(o.GetType());
            return PostOrPatch(objType, Request.Body, o);
        }

        private string GetIfMatchFromRequest()
        {
            var ifMatch = Request.Headers.IfMatch.FirstOrDefault();
            if (ifMatch != null)
            {
                ifMatch = ifMatch.Trim();
                if (ifMatch.Length < 2 || ifMatch[0] != '"' || ifMatch[ifMatch.Length - 1] != '"')
                    throw new NotImplementedException(
                        "Only recognized If-Match with quotes around, * not yet supported (TODO).");

                ifMatch = ifMatch.Substring(1, ifMatch.Length - 2);
            }
            return ifMatch;
        }

        private void ThrowMethodNotAllowedForType(HttpMethod allowedMethods)
        {
            var allowedMethodsString = string.Join(", ",
                Enum.GetValues(typeof(HttpMethod)).Cast<HttpMethod>().Where(x => allowedMethods.HasFlag(x)).Select(
                    x => x.ToString().ToUpper()));

            var allowHeader = new KeyValuePair<string, string>("Allow", allowedMethodsString);

            throw new PomonaException("Method " + Context.Request.Method + " not allowed!", null,
                                      HttpStatusCode.MethodNotAllowed, allowHeader.WrapAsEnumerable());
        }
    }
}