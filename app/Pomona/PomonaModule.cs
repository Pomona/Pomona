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
using System.IO;
using System.Linq;
using System.Reflection;

using Nancy;

using Pomona.CodeGen;
using Pomona.Common;
using Pomona.Common.Serialization;
using Pomona.Common.Serialization.Json;
using Pomona.Common.TypeSystem;
using Pomona.RequestProcessing;
using Pomona.Routing;
using Pomona.Schemas;

namespace Pomona
{
    public abstract class PomonaModule : NancyModule, IPomonaModule
    {
        private readonly IPomonaDataSource dataSource;
        private readonly TypeMapper typeMapper;


        protected PomonaModule(IPomonaDataSource dataSource, TypeMapper typeMapper)
            : this(dataSource, typeMapper, "/")
        {
        }


        protected PomonaModule(IPomonaDataSource dataSource, TypeMapper typeMapper, string baseUrl)
            : base(baseUrl)
        {
            // HACK TO SUPPORT NANCY TESTING (set a valid host name)
            Before += ctx =>
            {
                if (String.IsNullOrEmpty(ctx.Request.Url.HostName))
                    ctx.Request.Url.HostName = "test";
                return null;
            };

            this.dataSource = dataSource;
            this.typeMapper = typeMapper;

            foreach (var transformedType in this.typeMapper
                .TransformedTypes.OfType<ResourceType>()
                .Select(x => x.UriBaseType)
                .Where(x => x != null && !x.IsAnonymous() && x.IsRootResource)
                .Distinct())
                RegisterRoutesFor(transformedType);

            // For root resource links!
            Register(Get, "/", x => ProcessRequest());

            Get[PomonaRouteMetadataProvider.JsonSchema, "/schemas"] = x => GetSchemas();

            var clientAssemblyFileName = String.Format("/{0}.dll", this.typeMapper.Filter.ClientMetadata.AssemblyName);
            Get[PomonaRouteMetadataProvider.ClientAssembly, clientAssemblyFileName] = x => GetClientLibrary();

            RegisterClientNugetPackageRoute();

            RegisterSerializationProvider(typeMapper);
        }


        public IPomonaDataSource DataSource
        {
            get { return this.dataSource; }
        }

        public IRequestProcessorPipeline Pipeline
        {
            get { return new DefaultRequestProcessorPipeline(); }
        }

        public PathNode RootNode
        {
            get { return new DataSourceRootNode(this.typeMapper, this.dataSource, ModulePath); }
        }


        public void WriteClientLibrary(Stream stream, bool embedPomonaClient = true)
        {
            ClientLibGenerator.WriteClientLibrary(this.typeMapper, stream, embedPomonaClient);
        }


        protected virtual PomonaError OnException(Exception exception)
        {
            if (exception is PomonaSerializationException)
                return new PomonaError(HttpStatusCode.BadRequest, exception.Message);
            var pomonaException = exception as PomonaException;

            return pomonaException != null
                ? new PomonaError(pomonaException.StatusCode, pomonaException.Entity ?? pomonaException.Message)
                : new PomonaError(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString());
        }


        protected virtual PomonaResponse ProcessRequest()
        {
            var module = this;

            var pomonaContext = new PomonaContext(Context, module, new PomonaJsonSerializerFactory());

            return new DefaultRequestDispatcher().Dispatch(pomonaContext);
        }


        protected virtual Exception UnwrapException(Exception exception)
        {
            if (exception is TargetInvocationException || exception is RequestExecutionException)
                return exception.InnerException != null ? UnwrapException(exception.InnerException) : exception;

            return exception;
        }


        private Response GetClientLibrary()
        {
            var response = new Response
            {
                Contents = stream => WriteClientLibrary(stream),
                ContentType = "binary/octet-stream"
            };

            return response;
        }


        private Response GetClientNugetPackage()
        {
            var packageBuilder = new ClientNugetPackageBuilder(this.typeMapper);
            var response = new Response
            {
                Contents = stream =>
                {
                    using (var memstream = new MemoryStream())
                    {
                        packageBuilder.BuildPackage(memstream);
                        var bytes = memstream.ToArray();
                        stream.Write(bytes, 0, bytes.Length);
                    }
                },
                ContentType = "application/zip",
            };

            return response;
        }


        private Response GetSchemas()
        {
            var response = new Response();

            var schemas = new SchemaGenerator(this.typeMapper).Generate().ToJson();
            response.ContentsFromString(schemas);
            response.ContentType = "application/json; charset=utf-8";

            return response;
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
                                              error.StatusCode,
                                              responseHeaders : error.ResponseHeaders);
                }
            };
        }


        private void RegisterClientNugetPackageRoute()
        {
            var packageBuilder = new ClientNugetPackageBuilder(this.typeMapper);
            var packageFileName = packageBuilder.PackageFileName;

            Get[PomonaRouteMetadataProvider.ClientNugetPackage, "/Client.nupkg"] =
                x => Response.AsRedirect(packageFileName);
            Get[PomonaRouteMetadataProvider.ClientNugetPackageVersioned, "/" + packageFileName] =
                x => GetClientNugetPackage();
        }


        private void RegisterRoutesFor(ResourceType type)
        {
            var path = "/" + type.UriRelativePath;

            Register(Get, path + "/{remaining*}", x => ProcessRequest());
            Register(Get, path, x => ProcessRequest());
            Register(Post, path, x => ProcessRequest());
            Register(Patch, path + "/{remaining*}", x => ProcessRequest());
            Register(Post, path + "/{remaining*}", x => ProcessRequest());
            Register(Delete, path + "/{remaining*}", x => ProcessRequest());
        }


        private void RegisterSerializationProvider(ITypeMapper typeMapper)
        {
            Before += context =>
            {
                var uriResolver = new UriResolver(typeMapper, new BaseUriResolver(context, ModulePath));
                var resourceResolver = new ResourceResolver(typeMapper, context, context.GetRouteResolver());
                var contextProvider = new ServerSerializationContextProvider(uriResolver, resourceResolver, context);

                context.Items[typeof(IUriResolver).FullName] = uriResolver;
                context.Items[typeof(ISerializationContextProvider).FullName] = contextProvider;
                return null;
            };
        }


        private void SetErrorHandled()
        {
            Context.Items["ERROR_HANDLED"] = true;
        }
    }
}