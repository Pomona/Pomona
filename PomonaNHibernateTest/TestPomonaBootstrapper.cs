using NHibernate;

using Nancy.TinyIoc;

using Pomona;

namespace PomonaNHibernateTest
{
    public class TestPomonaBootstrapper : PomonaBootstrapper
    {
        private readonly ISessionFactory sessionFactory;
        private readonly TypeMapper typeMapper;

        public TestPomonaBootstrapper(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
            this.typeMapper = new TypeMapper(new TestPomonaConfiguration());
        }


        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(this.sessionFactory);
            container.Register(this.typeMapper);
            container.Register<IPomonaDataSource, TestPomonaDataSource>();
        }
    }
}