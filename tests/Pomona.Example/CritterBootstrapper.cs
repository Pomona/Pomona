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
        public CritterBootstrapper()
            : this(null)
        {
        }


        public CritterBootstrapper(CritterRepository repository = null)
        {
            TypeMapper = new TypeMapper(new CritterPomonaConfiguration());
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