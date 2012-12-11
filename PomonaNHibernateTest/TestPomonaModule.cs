using Pomona;

namespace PomonaNHibernateTest
{
    public class TestPomonaModule : PomonaModule
    {
        public TestPomonaModule(IPomonaDataSource dataSource, TypeMapper typeMapper) : base(dataSource, typeMapper)
        {
        }
    }
}