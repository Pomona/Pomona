#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Nancy;

using Pomona.CodeGen;
using Pomona.Common;
using Pomona.Common.TypeSystem;
using Pomona.FluentMapping;
using Pomona.Routing;
using Pomona.Schemas;

namespace Pomona
{
    public abstract class PomonaModule : NancyModule, IPomonaErrorHandler
    {
        private IContainer container;
        private IPomonaDataSource dataSource;
        private IPomonaSessionFactory sessionFactory;


        protected PomonaModule(string modulePath = "/")
            : this(null, modulePath)
        {
        }


        [Obsolete(
            "This PomonaModule ctor will be removed in the future, replace with the one taking no arguments or just IPomonaSessionFactory."
            )]
        protected PomonaModule(IPomonaDataSource dataSource, TypeMapper typeMapper)
            : this(dataSource, typeMapper, "/")
        {
        }


        [Obsolete(
            "This PomonaModule ctor will be removed in the future, replace with the one taking no arguments or just IPomonaSessionFactory."
            )]
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


        protected IContainer Container
        {
            get
            {
                if (this.container == null)
                    this.container = new ModuleContainer(Context, this.dataSource, this);
                return this.container;
            }
        }

        private TypeMapper TypeMapper
        {
            get { return this.sessionFactory.TypeMapper; }
        }


        protected virtual void OnConfiguration(IConfigurator config)
        {
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
            var pomonaConfigAttr = GetType().GetFirstOrDefaultAttribute<PomonaConfigurationAttribute>(true);
            if (pomonaConfigAttr == null)
            {
                throw new InvalidOperationException(
                    "Unable to find config for pomona module (has no [ModuleBinding] attribute attached).");
            }
            return (PomonaConfigurationBase)Activator.CreateInstance(pomonaConfigAttr.ConfigurationType);
        }


        private Response GetClientLibrary()
        {
            var response = new Response
            {
                Contents = stream => ClientLibGenerator.WriteClientLibrary(TypeMapper, stream),
                ContentType = "binary/octet-stream"
            };

            return response;
        }


        private Response GetClientNugetPackage()
        {
            var packageBuilder = new ClientNugetPackageBuilder(TypeMapper);
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

            var schemas = new SchemaGenerator(TypeMapper).Generate().ToJson();
            response.ContentsFromString(schemas);
            response.ContentType = "application/json; charset=utf-8";

            return response;
        }


        private void Initialize(IPomonaSessionFactory sessionFactory, IPomonaDataSource dataSource)
        {
            if (sessionFactory == null)
                sessionFactory = PomonaModuleConfigurationBinder.Current.GetFactory(this);

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
                RegisterRoutesFor((ResourceType)route.ResultItemType);

            // For root resource links!
            Register(Get, "/", x => ProcessRequest());

            Get[PomonaRouteMetadataProvider.JsonSchema, "/schemas"] = x => GetSchemas();

            var clientAssemblyFileName = $"/{TypeMapper.Filter.ClientMetadata.AssemblyName}.dll";
            Get[PomonaRouteMetadataProvider.ClientAssembly, clientAssemblyFileName] = x => GetClientLibrary();

            RegisterClientNugetPackageRoute();

            RegisterSerializationProvider(TypeMapper);
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
            var packageBuilder = new ClientNugetPackageBuilder(TypeMapper);
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


        private void RegisterSerializationProvider(ITypeResolver typeMapper)
        {
            Before += context =>
            {
                var uriResolver = new UriResolver(typeMapper, new BaseUriProvider(context, ModulePath));

                context.Items[typeof(IUriResolver).FullName] = uriResolver;
                return null;
            };
        }


        private void SetErrorHandled()
        {
            Context.Items["ERROR_HANDLED"] = true;
        }


        public PomonaError HandleException(Exception exception)
        {
            return OnException(UnwrapException(exception));
        }


        internal class Configurator : IConfigurator
        {
            public List<Delegate> Delegates { get; } = new List<Delegate>();


            public IConfigurator Map<T>(Action<ITypeMappingConfigurator<T>> map)
            {
                Delegates.Add(map);
                return this;
            }
        }

        public interface IConfigurator
        {
            IConfigurator Map<T>(Action<ITypeMappingConfigurator<T>> map);
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
                if (typeof(T) == GetType())
                    return (T)((object)this.module);
                if (typeof(T) == typeof(IPomonaDataSource) && this.dataSource != null)
                    return (T)this.dataSource;
                if (typeof(T) == typeof(IPomonaErrorHandler))
                    return (T)((object)this.module);

                return base.GetInstance<T>();
            }
        }
    }
}