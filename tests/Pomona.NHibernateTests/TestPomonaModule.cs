using Nancy.TinyIoc;
using Pomona;

namespace PomonaNHibernateTest
{
    public class TestPomonaModule : PomonaModule
    {
        public TestPomonaModule(IPomonaDataSource dataSource, TypeMapper typeMapper, TinyIoCContainer container) : base(dataSource, typeMapper, container)
        {
        }
    }
}