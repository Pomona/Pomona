using System;

using NHibernate;

using Nancy.Hosting.Self;

namespace PomonaNHibernateTest
{
    public class TestPomonaHost
    {
        private readonly Uri baseUri;
        private NancyHost host;
        private ISessionFactory sessionFactory;

        public TestPomonaHost(Uri baseUri, ISessionFactory sessionFactory)
        {
            this.baseUri = baseUri;
            this.sessionFactory = sessionFactory;
        }


        public Uri BaseUri
        {
            get { return this.baseUri; }
        }

        public NancyHost Host
        {
            get { return this.host; }
        }


        public void Start()
        {
            this.host = new NancyHost(this.baseUri, new TestPomonaBootstrapper(this.sessionFactory));
            this.host.Start();
        }


        public void Stop()
        {
            this.host.Stop();
            this.host = null;
        }
    }
}