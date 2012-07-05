using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Nancy;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Pomona.TestModel;

namespace Pomona
{
    public class AutoRestModule : NancyModule
    {
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();

        private Repository repository;

        private string GetExpandedPaths()
        {
            string expand = string.Empty;

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

            this.Get[path + "/{id}"] = x =>
            {
                var expandedPaths = GetExpandedPaths();
                return ToJson(this.repository.GetAll<T>().Where(y => y.Id == x.id).First(), type.Name.ToLower());
            };

            this.Get[path] = x =>
            {
                var expandedPaths = GetExpandedPaths();
                return ToJson(this.repository.GetAll<T>().ToList(), type.Name.ToLower());
                //return ToJson(this.repository.GetAll<T>().Select(y => new PathTrackingProxy(y, typeof(T).Name, expandedPaths)).ToList());
            };
        }

        public AutoRestModule()
        {
            this.jsonSerializerSettings.ContractResolver = new LowercaseContractResolver();
            jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            this.repository = new Repository();

            var registerRouteForT = typeof(AutoRestModule).GetMethod("RegisterRouteFor", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var type in GetEntityTypes())
            {
                var genericMethod = registerRouteForT.MakeGenericMethod(type);
                genericMethod.Invoke(this, null);
            }

            this.Get["Pomona.Client.dll"] = x =>
            {
                var response = new Response()
                {
                    Contents = stream =>
                    {
                        var clientLibGenerator = new ClientLibGenerator();
                        clientLibGenerator.CreateClientDll(GetEntityTypes(), stream);
                    }
                };

                response.ContentType = "binary/octet-stream";

                return response;
            };

            this.Get["/greet/{name}"] = x =>
            {
                return string.Concat("Hello ", x.name);
            };
        }


        private static IEnumerable<Type> GetEntityTypes()
        {
            return typeof(Critter).Assembly.GetTypes().Where(x => x.Namespace == "Pomona.TestModel" && x.BaseType == typeof(EntityBase));
        }


        private string UriResolver(object o)
        {
            var entity = o as EntityBase;

            return entity != null ? string.Format("http://localhost:2211/{0}/{1}", entity.GetType().Name.ToLower(), entity.Id) : null;
        }

        private Response ToJson(object o, string path)
        {
            var res = new Response();
            var expand = GetExpandedPaths().ToLower();

            bool debug = Request.Query.debug == "true";

            res.Contents = stream =>
            {
                var context = new PomonaContext(typeof(TestModel.EntityBase), UriResolver, path + "," + expand, debug);
                var wrapper = context.CreateWrapperFor(o, path);
                wrapper.ToJson(new StreamWriter(stream));
            };

            res.ContentType = "text/plain; charset=utf-8";

            return res;
        }


        private Response ToJson2(object o)
        {
            Response res = new Response();

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
    }
}