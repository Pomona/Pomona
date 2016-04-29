#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Nancy;
using Nancy.TinyIoc;

using Pomona.Queries;

namespace Pomona.Example
{
    public class CritterBootstrapper : DefaultNancyBootstrapper
    {
        private readonly IPomonaSessionFactory factory;


        public CritterBootstrapper()
            : this(null, null)
        {
        }


        public CritterBootstrapper(CritterRepository repository = null, CritterPomonaConfiguration configuration = null)
        {
            configuration = configuration ?? new CritterPomonaConfiguration();
            this.factory = configuration.CreateSessionFactory();
            TypeMapper = this.factory.TypeMapper;
            Repository = repository ?? new CritterRepository(TypeMapper);
        }


        public CritterRepository Repository { get; }

        public TypeMapper TypeMapper { get; }

        protected override IRootPathProvider RootPathProvider => new DefaultRootPathProvider();


        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            //container.Register(new CritterPomonaConfiguration().CreateSessionFactory());
            container.Register(Repository);
            container.Register(this.factory);
            //container.Register<CritterDataSource>();
            //container.Register(this.typeMapper);
        }


        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            container.Register<IQueryExecutor, CritterDataSource>();
            base.ConfigureRequestContainer(container, context);
        }
    }
}