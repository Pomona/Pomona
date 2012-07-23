using Nancy;

namespace Pomona.Example
{
    public class CritterBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoC.TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register<CritterDataSource>().AsSingleton();
        }

        protected override void ApplicationStartup(TinyIoC.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

        }
    }
}