#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using Nancy;
using Pomona.CodeGen;
using Pomona.Queries;
using Pomona.Schemas;

namespace Pomona
{
    public abstract class PomonaModule : NancyModule
    {
        private readonly IHttpQueryTransformer queryTransformer;
        private readonly PomonaSession session;
        private readonly TypeMapper typeMapper;
        private IPomonaDataSource dataSource;

        private string htmlLinks = string.Empty;


        protected PomonaModule(IPomonaDataSource dataSource, TypeMapper typeMapper)
            : this(dataSource, typeMapper, null)
        {
        }


        protected PomonaModule(
            IPomonaDataSource dataSource,
            TypeMapper typeMapper,
            IHttpQueryTransformer queryTransformer)
        {
            this.dataSource = dataSource;

            // TODO: This is performance hotspot: cache typemapper between each request.
            this.typeMapper = typeMapper;

            if (queryTransformer == null)
            {
                queryTransformer = new PomonaHttpQueryTransformer(
                    this.typeMapper, new QueryExpressionParser(new QueryTypeResolver(this.typeMapper)));
            }
            this.queryTransformer = queryTransformer;

            session = new PomonaSession(dataSource, this.typeMapper, GetBaseUri);

            Console.WriteLine("Registering routes..");
            foreach (var transformedType in this.typeMapper.TransformedTypes.Select(x => x.UriBaseType).Distinct())
                RegisterRoutesFor(transformedType);

            Get["/schemas"] = x => GetSchemas();

            Get[string.Format("/{0}.dll", this.typeMapper.Filter.GetClientAssemblyName())] = x => GetClientLibrary();

            RegisterClientNugetPackageRoute();
        }


        public IPomonaDataSource DataSource
        {
            get { return dataSource; }
        }


        private void FillJsonResponse(Response res, string json)
        {
            // Very simple content negotiation. Ainnt need noo fancy thing here.

            if (Request.Headers.Accept.Any(x => x.Item1 == "text/html"))
            {
                HtmlJsonPrettifier.CreatePrettifiedHtmlJsonResponse(
                    res, htmlLinks, json, Context.Request.Url.BasePath);
            }
            else
            {
                res.ContentsFromString(json);
                res.ContentType = "text/plain; charset=utf-8";
            }
        }


        private Response GetAsJson(TransformedType transformedType, object id)
        {
            var res = new Response();
            var expand = GetExpandedPaths().ToLower();

            var json = session.GetAsJson(transformedType, id, expand);

            FillJsonResponse(res, json);

            return res;
        }


        private Uri GetBaseUri()
        {
            return new Uri(
                string.Format(
                    "http://{0}:{1}/",
                    Request.Url.HostName,
                    Request.Url.Port));
        }


        private Response GetByForeignKeyPropertyAsJson(TransformedType type, PropertyMapping key, object id)
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

            var query = (PomonaQuery) queryTransformer.TransformRequest(Request, Context, type);

            string jsonStr;
            using (var strWriter = new StringWriter())
            {
                session.Query(query, strWriter);
                jsonStr = strWriter.ToString();
            }

            var response = new Response();
            FillJsonResponse(response, jsonStr);

            return response;
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


        private Response GetPropertyFromEntityAsJson(TransformedType transformedType, object id, string propname)
        {
            var res = new Response();
            var expand = GetExpandedPaths().ToLower();

            FillJsonResponse(res, session.GetPropertyAsJson(transformedType, id, propname, expand));

            return res;
        }


        private Response GetSchemas()
        {
            var res = new Response();

            var schemas = new SchemaGenerator(typeMapper).Generate().ToJson();
            res.ContentsFromString(schemas);
            res.ContentType = "text/plain; charset=utf-8";

            return res;
        }


        private Response PostFromJson(TransformedType transformedType)
        {
            var req = Request;

            var res = new Response();

            var responseBodyText = session.PostJson(transformedType, new StreamReader(req.Body));
            res.ContentsFromString(responseBodyText);

            // TODO: Set correct encoding [KNS]
            res.ContentType = "text/plain; charset=utf-8";

            return res;
        }


        private Response QueryAsJson(TransformedType transformedType)
        {
            var query = queryTransformer.TransformRequest(Request, Context, transformedType);

            string jsonStr;
            using (var strWriter = new StringWriter())
            {
                session.Query(query, strWriter);
                jsonStr = strWriter.ToString();
            }

            var response = new Response();
            FillJsonResponse(response, jsonStr);

            return response;
        }


        private void RegisterClientNugetPackageRoute()
        {
            var packageBuilder = new ClientNugetPackageBuilder(typeMapper);
            Get["/" + packageBuilder.PackageFileName] = x => GetClientNugetPackage();
        }


        private void RegisterRoutesFor(TransformedType type)
        {
            var path = "/" + type.UriRelativePath;
            //Console.WriteLine("Registering path " + path);
            htmlLinks = htmlLinks
                        + string.Format("<li><a href=\"{0}\">{1}</a></li>", path, type.Name);

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

            Put[path + "/{id}"] = x => UpdateFromJson(type, x.id);
            Post[path] = x => PostFromJson(type);

            Get[path] = x => QueryAsJson(type);
        }


        private Response UpdateFromJson(TransformedType transformedType, object id)
        {
            var req = Request;

            var res = new Response();
            res.Contents =
                stream =>
                session.UpdateFromJson(transformedType, id, new StreamReader(req.Body), new StreamWriter(stream));
            res.ContentType = "text/plain; charset=utf-8";

            return res;
        }
    }
}