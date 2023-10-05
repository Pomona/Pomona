#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Net.Http;

using Nancy;
using Nancy.Bootstrapper;

namespace Pomona.TestHelpers
{
    public class NancyTestingWebClient : HttpClient
    {
        public NancyTestingWebClient(NancyTestingHttpMessageHandler httpMessageHandler)
            : this(httpMessageHandler, false)
        {
        }


        public NancyTestingWebClient(NancyTestingHttpMessageHandler httpMessageHandler, bool disposeHandler)
            : base(httpMessageHandler, disposeHandler)
        {
        }


        public NancyTestingWebClient(INancyEngine nancyEngine)
            : this(new NancyTestingHttpMessageHandler(nancyEngine), true)
        {
        }


        public NancyTestingWebClient(INancyBootstrapper bootstrapper)
            : this(bootstrapper.GetEngine())
        {
        }
    }
}

