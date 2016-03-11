#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pomona.Common.Web
{
    public class HttpWebClient : IWebClient, IDisposable
    {
        public HttpWebClient(HttpMessageHandler messageHandler)
            : this(new HttpClient(messageHandler))
        {
        }


        public HttpWebClient()
            : this(new HttpClient())
        {
        }


        public HttpWebClient(HttpClient httpClient)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));
            Client = httpClient;
        }


        public HttpClient Client { get; }


        public void Dispose()
        {
            Client.Dispose();
        }


        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Client.SendAsync(request, cancellationToken);
        }
    }
}