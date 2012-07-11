namespace Pomona
{
    public class PomonaSession
    {
        private readonly IPomonaDataSource dataSource;

        public PomonaSession(IPomonaDataSource dataSource)
        {
            this.dataSource = dataSource;
        }
    }
}