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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Nancy;

using Newtonsoft.Json;

namespace Pomona
{
    public abstract class PomonaModule : NancyModule
    {
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
        private readonly PomonaSession session;
        private readonly TypeMapper typeMapper;
        private IPomonaDataSource dataSource;

        private string htmlLinks = string.Empty;


        public PomonaModule(IPomonaDataSource dataSource, ITypeMappingFilter typeMappingFilter = null)
        {
            this.dataSource = dataSource;
            this.typeMapper = new TypeMapper(typeMappingFilter);
            this.session = new PomonaSession(dataSource, this.typeMapper, GetBaseUri);

            // Just eagerly load the type mappings so we can manipulate it

            var registerRouteForT = typeof(PomonaModule).GetMethod(
                "RegisterRouteFor", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (
                var type in
                    this.typeMapper.TransformedTypes.Where(x => x.SourceType != null && !x.SourceType.IsAbstract).Select(x => x.UriBaseType ?? x).Distinct().Where(
                        x => !x.MappedAsValueObject))
            {
                var genericMethod = registerRouteForT.MakeGenericMethod(type.SourceType);
                genericMethod.Invoke(this, null);
            }

            Get["/schemas"] = x => GetSchemas();

            Get["/Pomona.Client.dll"] = x => GetClientLibrary();
        }


        public IPomonaDataSource DataSource
        {
            get { return this.dataSource; }
        }

        private void FillJsonResponse(Response res, string json)
        {
            // Very simple content negotiation. Ainnt need noo fancy thing here.

            if (Request.Headers.Accept.Any(x => x.Item1 == "text/html"))
                HtmlJsonPrettifier.CreatePrettifiedHtmlJsonResponse(res, this.htmlLinks, json);
            else
            {
                res.ContentsFromString(json);
                res.ContentType = "text/plain; charset=utf-8";
            }
        }


        private Response GetAsJson<T>(object id)
        {
            var res = new Response();
            var expand = GetExpandedPaths().ToLower();

            var json = this.session.GetAsJson<T>(id, expand);

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


        private Response GetClientLibrary()
        {
            var response = new Response();

            response.Contents = stream => this.session.WriteClientLibrary(stream);
            response.ContentType = "binary/octet-stream";

            return response;
        }


        private string GetExpandedPaths()
        {
            var expand = string.Empty;

            try
            {
                expand = Request.Query.expand;
            }
            catch (Exception)
            {
            }

            return expand;
        }


        private Response GetPropertyFromEntityAsJson<T>(object id, string propname)
        {
            var res = new Response();
            var expand = GetExpandedPaths().ToLower();

            FillJsonResponse(res, this.session.GetPropertyAsJson<T>(id, propname, expand));

            return res;
        }


        private Response GetSchemas()
        {
            var res = new Response();

            var schemas = new JsonSchemaGenerator(this.session).GenerateAllSchemas().ToString();
            res.ContentsFromString(schemas);
            res.ContentType = "text/plain; charset=utf-8";

            return res;
        }


        private Response ListAsJson<T>()
        {
            var res = new Response();
            var expand = GetExpandedPaths().ToLower();

            FillJsonResponse(res, this.session.ListAsJson<T>(expand));

            return res;
        }


        private Response PostFromJson<T>()
        {
            var req = Request;

            var res = new Response();

            var responseBodyText = this.session.PostJson<T>(new StreamReader(req.Body));
            res.ContentsFromString(responseBodyText);
            res.ContentType = "text/plain; charset=utf-8";

            return res;
        }


        private void RegisterRouteFor<T>()
        {
            var type = typeof(T);
            var lowerTypeName = type.Name.ToLower();
            var path = "/" + lowerTypeName;
            Console.WriteLine("Registering path " + path);

            this.htmlLinks = this.htmlLinks
                             + string.Format("<li><a href=\"/{0}\">{1}</a></li>", lowerTypeName, type.Name);

            Get[path + "/{id}"] = x => GetAsJson<T>(x.id);

            Get[path + "/{id}/{propname}"] = x => GetPropertyFromEntityAsJson<T>(x.id, x.propname);

            Put[path + "/{id}"] = x => UpdateFromJson<T>(x.id);
            Post[path] = x => PostFromJson<T>();

            Get[path] = x => ListAsJson<T>();
        }


        private Response UpdateFromJson<T>(object id)
        {
            var req = Request;

            var res = new Response();
            res.Contents =
                stream => this.session.UpdateFromJson<T>(id, new StreamReader(req.Body), new StreamWriter(stream));
            res.ContentType = "text/plain; charset=utf-8";

            return res;
        }
    }
}