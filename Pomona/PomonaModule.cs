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


        public PomonaModule(IPomonaDataSource dataSource, ITypeMapperFilter typeMapperFilter = null)
        {
            this.dataSource = dataSource;
            this.typeMapper = new TypeMapper(GetEntityTypes(), typeMapperFilter);
            this.session = new PomonaSession(dataSource, this.typeMapper, UriResolver);

            // Just eagerly load the type mappings so we can manipulate it

            var registerRouteForT = typeof(PomonaModule).GetMethod(
                "RegisterRouteFor", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var type in GetEntityTypes().Where(x => !x.IsAbstract))
            {
                var genericMethod = registerRouteForT.MakeGenericMethod(type);
                genericMethod.Invoke(this, null);
            }

            Get["Pomona.Client.dll"] = x => GetClientLibrary();
        }


        public IPomonaDataSource DataSource
        {
            get { return this.dataSource; }
        }

        protected abstract Type GetEntityBaseType();

        protected abstract IEnumerable<Type> GetEntityTypes();

        // TODO: Move this into TypeMapper or PomonaSession?
        protected abstract int GetIdFor(object entity);


        private Response GetAsJson<T>(object id)
        {
            var res = new Response();
            var expand = GetExpandedPaths().ToLower();

            res.Contents = stream => this.session.GetAsJson<T>(id, expand, new StreamWriter(stream));

            res.ContentType = "text/plain; charset=utf-8";

            return res;
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

            res.Contents = stream => this.session.GetPropertyAsJson<T>(id, propname, expand, new StreamWriter(stream));

            res.ContentType = "text/plain; charset=utf-8";

            return res;
        }


        private Response ListAsJson<T>()
        {
            var res = new Response();
            var expand = GetExpandedPaths().ToLower();

            res.Contents = stream => this.session.ListAsJson<T>(expand, new StreamWriter(stream));
            res.ContentType = "text/plain; charset=utf-8";

            return res;
        }


        private Response PostFromJson<T>()
        {
            var req = Request;

            var res = new Response();
            res.Contents = stream => this.session.PostJson<T>(new StreamReader(req.Body), new StreamWriter(stream));
            res.ContentType = "text/plain; charset=utf-8";

            return res;
        }


        private void RegisterRouteFor<T>()
        {
            var type = typeof(T);
            var lowerTypeName = type.Name.ToLower();
            var path = "/" + lowerTypeName;
            Console.WriteLine("Registering path " + path);

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


        private string UriResolver(object o)
        {
            if (!GetEntityBaseType().IsAssignableFrom(o.GetType()))
                return null;

            return string.Format(
                "http://{0}:{1}/{2}/{3}",
                Request.Url.HostName,
                Request.Url.Port,
                o.GetType().Name.ToLower(),
                GetIdFor(o));
        }
    }
}