#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;
using System.Net.Http;

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
            return new CritterClient(baseUri, new HttpClient(new IndependentNancyTestingHttpMessageHandler(critterBootstrapper.GetEngine())));
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