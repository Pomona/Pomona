#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Nancy.Hosting.Self;

namespace Pomona.Example
{
    public class CritterHost
    {
        public CritterHost(Uri baseUri)
        {
            BaseUri = baseUri;
        }


        public Uri BaseUri { get; }

        public NancyHost Host { get; private set; }
        public CritterRepository Repository { get; private set; }
        public TypeMapper TypeMapper { get; private set; }


        public void Start()
        {
            var bootstrapper = new CritterBootstrapper();
            Repository = bootstrapper.Repository;
            TypeMapper = bootstrapper.TypeMapper;
            Host = new NancyHost(BaseUri, bootstrapper);
            Host.Start();
        }


        public void Stop()
        {
            Host.Stop();
            Host = null;
        }
    }
}