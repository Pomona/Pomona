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

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;
using Nancy;
using Nancy.Routing;
using Pomona.CodeGen;
using Pomona.Queries;
using Pomona.Schemas;

namespace Pomona
{
    public abstract class PomonaModule : NancyModule, IPomonaUriResolver
    {
        private readonly IServiceLocator container;
        private readonly IPomonaDataSource dataSource;
        private readonly IHttpQueryTransformer queryTransformer;
        private readonly PomonaSession session;
        private readonly TypeMapper typeMapper;

        private string htmlLinks = string.Empty;


        protected PomonaModule(IPomonaDataSource dataSource, TypeMapper typeMapper, IServiceLocator container)
            : this(dataSource, typeMapper, container, null)
        {
        }


        protected PomonaModule(
            IPomonaDataSource dataSource,
            TypeMapper typeMapper,
            IServiceLocator container,
            IHttpQueryTransformer queryTransformer)
        {
            // HACK TO SUPPORT NANCY TESTING (set a valid host name)
            Before += ctx =>
                {
                    if (string.IsNullOrEmpty(ctx.Request.Url.HostName))
                    {
                        ctx.Request.Url.HostName = "test";
                    }
                    return null;
                };


            this.dataSource = dataSource;

            this.typeMapper = typeMapper;
            this.container = container;

            if (queryTransformer == null)
            {
                queryTransformer = new PomonaHttpQueryTransformer(
                    this.typeMapper, new QueryExpressionParser(new QueryTypeResolver(this.typeMapper)));
            }
            this.queryTransformer = queryTransformer;

            session = new PomonaSession(dataSource, this.typeMapper, this);

            foreach (var transformedType in this.typeMapper
                                                .TransformedTypes
                                                .Select(x => x.UriBaseType)
                                                .Where(x => x != null)
                                                .Distinct())
            {
                RegisterRoutesFor(transformedType);
            }

            Get["/schemas"] = x => GetSchemas();

            Get[string.Format("/{0}.dll", this.typeMapper.Filter.GetClientAssemblyName())] = x => GetClientLibrary();

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

        object IPomonaUriResolver.GetResultByUri(string uriString)
        {
            var routeResolver = container.GetInstance<IRouteResolver>();
            var uri = new Uri(uriString, UriKind.Absolute);

            var modulePath = uri.AbsolutePath;
            var basePath = Request.Url.BasePath ?? string.Empty;
            if (modulePath.StartsWith(basePath))
                modulePath = modulePath.Substring(basePath.Length);

            var url = Request.Url.Clone();
            url.Path = modulePath;
            url.Query = uri.Query;

            var innerRequest = new Request("GET", url);
            var innerContext = new NancyContext
                {
                    Culture = Context.Culture,
                    CurrentUser = Context.CurrentUser,
                    Request = innerRequest
                };
            
            var routeMatch = routeResolver.Resolve(innerContext);
            var route = routeMatch.Route;
            var dynamicDict = routeMatch.Parameters;

            var pomonaResponse = (PomonaResponse) route.Action((dynamic) dynamicDict);

            return pomonaResponse.Entity;
        }

        public virtual string RelativeToAbsoluteUri(string path)
        {
            if (string.IsNullOrEmpty(Request.Url.HostName))
            {
                return path;
            }

            return string.Format("{0}{1}", GetBaseUri(), path);
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

        private PomonaResponse GetAsJson(TransformedType transformedType, object id)
        {
            var expand = GetExpandedPaths().ToLower();

            return session.GetAsJson(transformedType, id, expand);
        }


        protected virtual Uri GetBaseUri()
        {
            var appUrl = GetAppVirtualPath();
            var uriString = string.Format("{0}://{1}:{2}{3}{4}", Request.Url.Scheme, Request.Url.HostName,
                                          Request.Url.Port, appUrl, appUrl.EndsWith("/") ? string.Empty : "/");

            return new Uri(uriString);
        }


        private PomonaResponse GetByForeignKeyPropertyAsJson(TransformedType type, PropertyMapping key, object id)
        {
            // HACK: This is quite hacky, I'll gladly admit that [KNS]
            // TODO: Fix that this only works if primary key is named Id [KNS]

            // Fetch entity first to see if entity with id actually exists.
            session.GetAsJson((TransformedType) key.PropertyType, id, null);

            if (Request.Query["$filter"].HasValue)
            {
                Request.Query["$filter"] = string.Format(
                    "{0}.{1} eq {2} and ({3})", key.JsonName, key.PropertyType.PrimaryId.JsonName, id,
                    Request.Query["$filter"]);
            }
            else
                Request.Query["$filter"] = string.Format("{0}.id eq {1}", key.JsonName, id);

            return Query(type);
        }


        private Response GetClientLibrary()
        {
            var response = new Response();

            response.Contents = stream => session.WriteClientLibrary(stream);
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


        private string GetExpandedPaths()
        {
            string expand = null;

            try
            {
                expand = Request.Query["$expand"];
            }
            catch (Exception)
            {
            }

            return expand ?? string.Empty;
        }


        private PomonaResponse GetPropertyFromEntityAsJson(TransformedType transformedType, object id, string propname)
        {
            var expand = GetExpandedPaths().ToLower();

            return session.GetPropertyAsJson(transformedType, id, propname, expand);
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
            return session.PostJson(transformedType, Request.Body);
        }

        private PomonaResponse Query(TransformedType transformedType)
        {
            var query = queryTransformer.TransformRequest(Request, Context, transformedType);

            return session.Query(query);
        }


        private void RegisterClientNugetPackageRoute()
        {
            var packageBuilder = new ClientNugetPackageBuilder(typeMapper);
            Get["/client.nupkg"] = x => Response.AsRedirect(packageBuilder.PackageFileName);
            Get["/" + packageBuilder.PackageFileName] = x => GetClientNugetPackage();
        }


        protected virtual string GetAppVirtualPath()
        {
            return "/";
        }

        private void RegisterRoutesFor(TransformedType type)
        {
            var appVirtualPath = GetAppVirtualPath();
            var absLinkPath = string.Format("{0}{1}{2}", appVirtualPath,
                                            appVirtualPath.EndsWith("/") ? string.Empty : "/", type.UriRelativePath);

            var path = "/" + type.UriRelativePath;
            //Console.WriteLine("Registering path " + path);
            htmlLinks = htmlLinks
                        + string.Format("<li><a href=\"{0}\">{1}</a></li>", absLinkPath, type.Name);

            Get[path + "/{id}"] = x => GetAsJson(type, x.id);

            foreach (var prop in type.Properties)
            {
                var transformedProp = prop as PropertyMapping;
                if (transformedProp != null && transformedProp.IsOneToManyCollection
                    && transformedProp.ElementForeignKey != null)
                {
                    var collectionElementType = (TransformedType) prop.PropertyType.ElementType;
                    var elementForeignKey = transformedProp.ElementForeignKey;

                    Get[path + "/{id}/" + prop.JsonName] =
                        x => GetByForeignKeyPropertyAsJson(collectionElementType, elementForeignKey, x.id);

                    var propname = prop.Name;
                    Get[path + "/{id}/_old_" + prop.JsonName] = x => GetPropertyFromEntityAsJson(type, x.id, propname);
                }
                else
                {
                    var propname = prop.Name;
                    Get[path + "/{id}/" + prop.JsonName] = x => GetPropertyFromEntityAsJson(type, x.id, propname);
                }
            }

            Get[path + "/{id}/{propname}"] = x => GetPropertyFromEntityAsJson(type, x.id, x.propname);

            Patch[path + "/{id}"] = x => UpdateFromJson(type, x.id);
            Post[path] = x => PostFromJson(type);

            Get[path] = x => Query(type);
        }


        private PomonaResponse UpdateFromJson(TransformedType transformedType, object id)
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

            return session.Patch(transformedType, id, Request.Body, ifMatch);
        }
    }
}