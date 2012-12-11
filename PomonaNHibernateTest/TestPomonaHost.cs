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
            get { return baseUri; }
        }

        public NancyHost Host
        {
            get { return host; }
        }


        public void Start()
        {
            host = new NancyHost(baseUri, new TestPomonaBootstrapper(sessionFactory));
            host.Start();
        }


        public void Stop()
        {
            host.Stop();
            host = null;
        }
    }
}