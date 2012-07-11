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

        public PomonaModule(IPomonaDataSource dataSource)
        {
            this.dataSource = dataSource;
            typeMapper = new TypeMapper(GetEntityTypes());
            session = new PomonaSession(dataSource, typeMapper, UriResolver);

            // Just eagerly load the type mappings so we can manipulate it
            // TODO: This should be done in TypeMapper constructor?
            GetEntityTypes().Select(x => typeMapper.GetClassMapping(x)).ToList();

            var registerRouteForT = typeof (PomonaModule).GetMethod(
                "RegisterRouteFor", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var type in GetEntityTypes().Where(x => !x.IsAbstract))
            {
                var genericMethod = registerRouteForT.MakeGenericMethod(type);
                genericMethod.Invoke(this, null);
            }

            Get["Pomona.Client.dll"] = x =>
                                           {
                                               var response = new Response()
                                                                  {
                                                                      Contents = stream =>
                                                                                     {
                                                                                         var clientLibGenerator =
                                                                                             new ClientLibGenerator(
                                                                                                 typeMapper);
                                                                                         clientLibGenerator.
                                                                                             CreateClientDll(stream);
                                                                                     }
                                                                  };

                                               response.ContentType = "binary/octet-stream";

                                               return response;
                                           };
        }


        public IPomonaDataSource DataSource
        {
            get { return dataSource; }
        }

        protected abstract T GetById<T>(int id);
        protected abstract Type GetEntityBaseType();

        protected abstract IEnumerable<Type> GetEntityTypes();

        protected abstract int GetIdFor(object entity);

        protected abstract IList<T> ListAll<T>();


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


        private void RegisterRouteFor<T>()
        {
            var type = typeof (T);
            var lowerTypeName = type.Name.ToLower();
            var path = "/" + lowerTypeName;
            Console.WriteLine("Registering path " + path);

            Get[path + "/{id}"] = x => GetAsJson<T>(x.id);

            Put[path + "/{id}"] = x => { return UpdateFromJson(GetById<T>(x.id), lowerTypeName); };

            Get[path] = x => ListAsJson<T>();
        }

        private Response UpdateFromJson(object o, string path)
        {
            var req = Request;

            var res = new Response();

            res.Contents = stream =>
                               {
                                   var context = new FetchContext(UriResolver, path, false, session);
                                   var wrapper = context.CreateWrapperFor(o, path,
                                                                          typeMapper.GetClassMapping(
                                                                              o.GetType()));
                                   wrapper.UpdateFromJson(new StreamReader(req.Body));

                                   wrapper.ToJson(new StreamWriter(stream));
                               };

            res.ContentType = "text/plain; charset=utf-8";

            return res;
        }

        private Response ListAsJson<T>()
        {
            var res = new Response();
            var expand = GetExpandedPaths().ToLower();

            res.Contents = stream => session.ListAsJson<T>(expand, new StreamWriter(stream));

            res.ContentType = "text/plain; charset=utf-8";

            return res;
        }

        private Response GetAsJson<T>(object id)
        {
            var res = new Response();
            var expand = GetExpandedPaths().ToLower();

            res.Contents = stream => session.GetAsJson<T>(id, expand, new StreamWriter(stream));

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