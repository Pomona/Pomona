#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pomona.Common.Web
{
    /// <summary>
    /// A simple wrapper for HttpClient
    /// </summary>
    public interface IWebClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}