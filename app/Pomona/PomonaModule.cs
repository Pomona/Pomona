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
using System.IO;
using System.Linq;
using System.Reflection;

using Nancy;

using Pomona.CodeGen;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Serialization.Json;
using Pomona.Common.TypeSystem;
using Pomona.RequestProcessing;
using Pomona.Schemas;

namespace Pomona
{
    public abstract class PomonaModule : NancyModule
    {
        private readonly IPomonaDataSource dataSource;
        private readonly TypeMapper typeMapper;


        protected PomonaModule(
            IPomonaDataSource dataSource,
            TypeMapper typeMapper)
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
            get { return this.dataSource; }
        }

        public ITypeMapper TypeMapper
        {
            get { return this.typeMapper; }
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
            if (pomonaException != null)
                return new PomonaError(pomonaException.StatusCode, pomonaException.Entity ?? pomonaException.Message);
            return new PomonaError(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString());
        }


        protected virtual Exception UnwrapException(Exception exception)
        {
            if (exception is TargetInvocationException || exception is RequestExecutionException)
                return exception.InnerException != null ? UnwrapException(exception.InnerException) : exception;
            return exception;
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

            var packageBuilder = new ClientNugetPackageBuilder(this.typeMapper);
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


        private object GetJsonBrowserHtmlResponse()
        {
            var resourceName = "Pomona.Content.jsonbrowser.html";
            return
                Response.FromStream(
                    () => Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName),
                    "text/html");
        }


        private LinkedListNode<string> GetPathNodes()
        {
            return new LinkedList<string>(Request.Url.Path.Split('/').Select(HttpUtility.UrlDecode)).First;
        }


        private PomonaResponse ProcessRequest()
        {
            var pathNodes = GetPathNodes();
            var rootNode = new DataSourceRootNode(TypeMapper, this.dataSource);
            PathNode node = rootNode;
            foreach (var pathPart in pathNodes.WalkTree(x => x.Next).Skip(1).Select(x => x.Value))
                node = node.GetChildNode(pathPart);

            var pomonaRequest = new PomonaRequest(node,
                Context,
                new PomonaJsonSerializerFactory(Context.GetSerializationContextProvider()));

            if (!node.AllowedMethods.HasFlag(pomonaRequest.Method))
                ThrowMethodNotAllowedForType(node.AllowedMethods);

            var response = new DefaultRequestProcessorPipeline().Process(pomonaRequest);
            if (response == null)
                throw new PomonaException("Unable to find RequestProcessor able to handle request.");
            return response;
        }


        private Response GetSchemas()
        {
            var res = new Response();

            var schemas = new SchemaGenerator(this.typeMapper).Generate().ToJson();
            res.ContentsFromString(schemas);
            res.ContentType = "text/plain; charset=utf-8";

            return res;
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
            Get["/client.nupkg"] = x => Response.AsRedirect(packageBuilder.PackageFileName);
            Get["/" + packageBuilder.PackageFileName] = x => GetClientNugetPackage();
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


        private void SetErrorHandled()
        {
            Context.Items["ERROR_HANDLED"] = true;
        }


        private void ThrowMethodNotAllowedForType(HttpMethod allowedMethods)
        {
            var allowedMethodsString = string.Join(", ",
                Enum.GetValues(typeof(HttpMethod)).Cast<HttpMethod>().Where(x => allowedMethods.HasFlag(x)).Select(
                    x => x.ToString().ToUpper()));

            var allowHeader = new KeyValuePair<string, string>("Allow", allowedMethodsString);

            throw new PomonaException("Method " + Context.Request.Method + " not allowed!",
                null,
                HttpStatusCode.MethodNotAllowed,
                allowHeader.WrapAsEnumerable());
        }
    }
}