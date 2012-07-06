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

using Pomona.TestModel;

namespace Pomona
{
    public class PomonaModule : NancyModule
    {
        private readonly ClassMappingFactory classMappingFactory;
        private readonly CritterRepository critterRepository;
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();


        public PomonaModule()
        {
            this.classMappingFactory = new ClassMappingFactory();

            // Just eagerly load the type mappings so we can manipulate it
            GetEntityTypes().Select(x => this.classMappingFactory.GetClassMapping(x)).ToList();

            // Test manipulating type mapping
            var critterMapping = (TransformedType)this.classMappingFactory.GetClassMapping(typeof(Critter));
            critterMapping.Properties.Add(
                new PropertyMapping(
                    "FirstWeapon", critterMapping, this.classMappingFactory.GetClassMapping(typeof(Weapon)), null)
                {
                    Getter = x => ((Critter)x).Weapons.FirstOrDefault()
                });

            this.jsonSerializerSettings.ContractResolver = new LowercaseContractResolver();
            this.jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            this.critterRepository = new CritterRepository();

            var registerRouteForT = typeof(PomonaModule).GetMethod(
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
                        var clientLibGenerator = new ClientLibGenerator();
                        clientLibGenerator.CreateClientDll(
                            GetEntityTypes().Select(y => this.classMappingFactory.GetClassMapping(y)).Cast
                                <TransformedType>(),
                            stream);
                    }
                };

                response.ContentType = "binary/octet-stream";

                return response;
            };

            Get["/greet/{name}"] = x => { return string.Concat("Hello ", x.name); };
        }


        private static IEnumerable<Type> GetEntityTypes()
        {
            return
                typeof(Critter).Assembly.GetTypes().Where(
                    x => x.Namespace == "Pomona.TestModel" && typeof(EntityBase).IsAssignableFrom(x));
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


        private void RegisterRouteFor<T>()
            where T : EntityBase
        {
            var type = typeof(T);
            var path = "/" + type.Name.ToLower();
            Console.WriteLine("Registering path " + path);

            Get[path + "/{id}"] = x =>
            {
                var expandedPaths = GetExpandedPaths();
                return ToJson(this.critterRepository.GetAll<T>().Where(y => y.Id == x.id).First(), type.Name.ToLower());
            };

            Get[path] = x =>
            {
                var expandedPaths = GetExpandedPaths();
                return ToJson(this.critterRepository.GetAll<T>().ToList(), type.Name.ToLower());
                //return ToJson(this.repository.GetAll<T>().Select(y => new PathTrackingProxy(y, typeof(T).Name, expandedPaths)).ToList());
            };
        }


        private Response ToJson(object o, string path)
        {
            var res = new Response();
            var expand = GetExpandedPaths().ToLower();

            bool debug = Request.Query.debug == "true";

            res.Contents = stream =>
            {
                var context = new PomonaContext(
                    typeof(EntityBase), UriResolver, path + "," + expand, debug, this.classMappingFactory);
                var wrapper = context.CreateWrapperFor(o, path, this.classMappingFactory.GetClassMapping(o.GetType()));
                wrapper.ToJson(new StreamWriter(stream));
            };

            res.ContentType = "text/plain; charset=utf-8";

            return res;
        }


        private Response ToJson2(object o)
        {
            var res = new Response();

            res.Contents = stream =>
            {
                var writer = new StreamWriter(stream);
                writer.Write(JsonConvert.SerializeObject(o, Formatting.Indented, this.jsonSerializerSettings));
                writer.Flush();
            };

            // res.ContentType = "application/json";
            res.ContentType = "text/plain; charset=utf-8";

            return res;
        }


        private string UriResolver(object o)
        {
            var entity = o as EntityBase;

            return entity != null
                       ? string.Format("http://localhost:2211/{0}/{1}", entity.GetType().Name.ToLower(), entity.Id)
                       : null;
        }
    }
}