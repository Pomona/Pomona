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
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.FluentMapping;
using Pomona.Plumbing;
using Pomona.Routing;
using Pomona.Schemas;

namespace Pomona
{
    public abstract class PomonaModule : NancyModule
    {
        private IPomonaDataSource dataSource;
        private IPomonaSessionFactory sessionFactory;

        protected PomonaModule(string modulePath = "/") : this(null, modulePath) { }


        [Obsolete("This PomonaModule ctor will be removed in the future, replace with the one taking no arguments or just IPomonaSessionFactory.")]
        protected PomonaModule(IPomonaDataSource dataSource, TypeMapper typeMapper)
            : this(dataSource, typeMapper, "/")
        {
        }


        [Obsolete("This PomonaModule ctor will be removed in the future, replace with the one taking no arguments or just IPomonaSessionFactory.")]
        protected PomonaModule(IPomonaDataSource dataSource, TypeMapper typeMapper, string modulePath)
            : this(typeMapper.SessionFactory, modulePath, dataSource)
        {
        }

        protected PomonaModule(IPomonaSessionFactory sessionFactory,
                               string modulePath = "/",
                               IPomonaDataSource dataSource = null)
            : base(modulePath)
        {
            Initialize(sessionFactory, dataSource);
        }


        private void Initialize(IPomonaSessionFactory sessionFactory, IPomonaDataSource dataSource)
        {
            if (sessionFactory == null)
            {
                sessionFactory = PomonaModuleConfigurationBinder.Current.GetFactory(this);
            }

            this.sessionFactory = sessionFactory;
            this.dataSource = dataSource;

            // HACK TO SUPPORT NANCY TESTING (set a valid host name)
            Before += ctx =>
            {
                if (String.IsNullOrEmpty(ctx.Request.Url.HostName))
                    ctx.Request.Url.HostName = "test";
                return null;
            };

            foreach (var route in this.sessionFactory.Routes.Children)
            {
                RegisterRoutesFor((ResourceType)route.ResultItemType);
            }

            // For root resource links!
            Register(Get, "/", x => ProcessRequest());

            Get[PomonaRouteMetadataProvider.JsonSchema, "/schemas"] = x => GetSchemas();

            var clientAssemblyFileName = String.Format("/{0}.dll", this.TypeMapper.Filter.ClientMetadata.AssemblyName);
            Get[PomonaRouteMetadataProvider.ClientAssembly, clientAssemblyFileName] = x => GetClientLibrary();

            RegisterClientNugetPackageRoute();

            RegisterSerializationProvider(TypeMapper);
        }


        private TypeMapper TypeMapper
        {
            get { return sessionFactory.TypeMapper; }
        }


        public void WriteClientLibrary(Stream stream, bool embedPomonaClient = true)
        {
            ClientLibGenerator.WriteClientLibrary(this.TypeMapper, stream, embedPomonaClient);
        }


        protected virtual PomonaError OnException(Exception exception)
        {
            if (exception is PomonaSerializationException)
                return new PomonaError(HttpStatusCode.BadRequest, exception.Message);
            var pomonaException = exception as PomonaServerException;

            return pomonaException != null
                ? new PomonaError(pomonaException.StatusCode, pomonaException.Entity ?? pomonaException.Message)
                : new PomonaError(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString());
        }


        public interface IConfigurator
        {
            IConfigurator Map<T>(Action<ITypeMappingConfigurator<T>> map);
        }

        internal class Configurator : IConfigurator
        {
            private List<Delegate> delegates = new List<Delegate>();

            public List<Delegate> Delegates
            {
                get { return this.delegates; }
            }

            public IConfigurator Map<T>(Action<ITypeMappingConfigurator<T>> map)
            {
                delegates.Add(map);
                return this;
            }
        }

        protected virtual void OnConfiguration(IConfigurator config)
        {
        }


        protected virtual PomonaResponse ProcessRequest()
        {
            var pomonaSession = this.sessionFactory.CreateSession(Container);
            Context.Items[typeof(IPomonaSession).FullName] = pomonaSession;
            var pomonaEngine =
                new PomonaEngine(pomonaSession);
            return pomonaEngine.Handle(Context, ModulePath);
        }


        protected virtual Exception UnwrapException(Exception exception)
        {
            if (exception is TargetInvocationException || exception is RequestExecutionException)
                return exception.InnerException != null ? UnwrapException(exception.InnerException) : exception;

            return exception;
        }


        internal PomonaConfigurationBase GetConfiguration()
        {
            var pomonaConfigAttr = this.GetType().GetFirstOrDefaultAttribute<PomonaConfigurationAttribute>(true);
            if (pomonaConfigAttr == null)
                throw new InvalidOperationException("Unable to find config for pomona module (has no [ModuleBinding] attribute attached).");
            return (PomonaConfigurationBase)Activator.CreateInstance(pomonaConfigAttr.ConfigurationType);
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
            var packageBuilder = new ClientNugetPackageBuilder(this.TypeMapper);
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

            var schemas = new SchemaGenerator(this.TypeMapper).Generate().ToJson();
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
            var packageBuilder = new ClientNugetPackageBuilder(this.TypeMapper);
            var packageFileName = packageBuilder.PackageFileName;

            Get[PomonaRouteMetadataProvider.ClientNugetPackage, "/Client.nupkg"] =
                x => Response.AsRedirect(packageFileName);
            Get[PomonaRouteMetadataProvider.ClientNugetPackageVersioned, "/" + packageFileName] =
                x => GetClientNugetPackage();
        }


        private void RegisterRoutesFor(ResourceType type)
        {
            var path = "/" + type.UrlRelativePath;

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

                context.Items[typeof(IUriResolver).FullName] = uriResolver;
                return null;
            };
        }


        private void SetErrorHandled()
        {
            Context.Items["ERROR_HANDLED"] = true;
        }


        private IContainer container;

        protected IContainer Container
        {
            get
            {
                if (container == null)
                    container = new ModuleContainer(Context, dataSource, this);
                return container;
            }
        }

        private class ModuleContainer : ServerContainer
        {
            private readonly IPomonaDataSource dataSource;
            private readonly PomonaModule module;


            public ModuleContainer(NancyContext nancyContext, IPomonaDataSource dataSource, PomonaModule module)
                : base(nancyContext)
            {
                this.dataSource = dataSource;
                this.module = module;
            }


            public override T GetInstance<T>()
            {
                if (typeof(T) == this.GetType())
                    return (T)((object)module);
                if (typeof(T) == typeof(IPomonaDataSource) && dataSource != null)
                    return (T)dataSource;

                return base.GetInstance<T>();
            }
        }
    }
}