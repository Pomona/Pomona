#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

using Critters.Client;
using Critters.Client.Pomona.Common;
using Critters.Client.Pomona.Common.Web;

using Nancy;

using Pomona.Example;
using Pomona.TestHelpers;
using Pomona.UnitTests.Client;

namespace Pomona.IndependentClientTests
{
    public abstract class IndependentClientTestsBase : CritterServiceTestsBase<CritterClient>
    {
        public override CritterClient CreateHttpTestingClient(string baseUri)
        {
            throw new NotImplementedException();
        }


        public override CritterClient CreateInMemoryTestingClient(string baseUri,
                                                                  CritterBootstrapper critterBootstrapper)
        {
            return new CritterClient(baseUri,
                                     new HttpWebClient(new IndependentNancyTestingHttpMessageHandler(critterBootstrapper.GetEngine())));
        }


        public override void SetupRequestCompletedHandler()
        {
            Client.RequestCompleted += ClientOnRequestCompleted;
        }


        public override void TeardownRequestCompletedHandler()
        {
            Client.RequestCompleted -= ClientOnRequestCompleted;
        }


        private void ClientOnRequestCompleted(object sender, ClientRequestLogEventArgs e)
        {
            Console.WriteLine("Sent:\r\n{0}\r\nReceived:\r\n{1}\r\n",
                              e.Request,
                              (object)e.Response ?? "(nothing received)");
        }


        private class IndependentNancyTestingHttpMessageHandler : NancyTestingHttpMessageHandler
        {
            public IndependentNancyTestingHttpMessageHandler(INancyEngine engine)
                : base(engine)
            {
            }
        }
    }
}

