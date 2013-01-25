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
            typeMapper = new TypeMapper(new TestPomonaConfiguration());
        }


        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(sessionFactory);
            container.Register(typeMapper);
            container.Register<IPomonaDataSource, TestPomonaDataSource>();
        }
    }
}