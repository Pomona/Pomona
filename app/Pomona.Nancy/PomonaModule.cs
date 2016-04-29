#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Nancy;

using Pomona.CodeGen;
using Pomona.Common;
using Pomona.Common.TypeSystem;
using Pomona.Schemas;

namespace Pomona.Nancy
{
    public abstract class PomonaModule : NancyModule
    {
        private IPomonaSessionFactory sessionFactory;


        protected PomonaModule(string modulePath = "/")
            : this(null, modulePath)
        {
        }


        protected PomonaModule(IPomonaSessionFactory sessionFactory,
                               string modulePath = "/")
            : base(modulePath)
        {
            Initialize(sessionFactory);
        }


        private TypeMapper TypeMapper => this.sessionFactory.TypeMapper;


        protected virtual Task<PomonaResponse> ProcessRequest(CancellationToken cancellationToken)
        {
            var pomonaSession = this.sessionFactory.CreateSession(new ServerContainer(Context), 
                                                                  new UriResolver(TypeMapper, new BaseUriProvider(Context, ModulePath)));
            Context.SetPomonaSession(pomonaSession);
            var pomonaEngine =
                new PomonaEngine(pomonaSession);
            return pomonaEngine.Handle(Context, ModulePath, cancellationToken);
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


        private void Initialize(IPomonaSessionFactory sessionFactory)
        {
            if (sessionFactory == null)
                sessionFactory = PomonaModuleConfigurationBinder.Current.GetFactory(this);

            this.sessionFactory = sessionFactory;

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
            Register(Get, "/");

            Get[PomonaRouteMetadataProvider.JsonSchema, "/schemas"] = x => GetSchemas();

            var clientAssemblyFileName = $"/{TypeMapper.Filter.ClientMetadata.AssemblyName}.dll";
            Get[PomonaRouteMetadataProvider.ClientAssembly, clientAssemblyFileName] = x => GetClientLibrary();

            RegisterClientNugetPackageRoute();
        }


        private void Register(RouteBuilder routeBuilder, string path)
        {
            routeBuilder[path, true] = async (x, ct) =>
            {
                try
                {
                    var pomonaResponse = await ProcessRequest(ct);

                    if ((int)pomonaResponse.StatusCode >= 400)
                        SetErrorHandled();

                    return pomonaResponse;
                }
                catch (Exception ex)
                {
                    var error = ((IPomonaErrorHandler)this.Context.Resolve(typeof(IPomonaErrorHandler))).HandleException(ex);
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
            var subPaths = path + "/{remaining*}";
            Register(Get, subPaths);
            Register(Get, path);
            Register(Post, path);
            Register(Patch, subPaths);
            Register(Post, subPaths);
            Register(Delete, subPaths);
        }


        private void SetErrorHandled()
        {
            Context.Items["ERROR_HANDLED"] = true;
        }
    }
}