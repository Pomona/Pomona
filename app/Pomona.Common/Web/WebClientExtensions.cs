#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Net.Http;
using System.Threading;

namespace Pomona.Common.Web
{
    public static class WebClientExtensions
    {
        public static HttpResponseMessage SendSync(this IWebClient client, HttpRequestMessage request)
        {
            return client.SendAsync(request, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}