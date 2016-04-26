#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pomona.Common.Web
{
    public static class WebClientExtensions
    {
        public static HttpResponseMessage SendSync(this IWebClient client, HttpRequestMessage request)
        {
            return Task.Run(() => client.SendAsync(request, CancellationToken.None)).Result;
        }
    }
}