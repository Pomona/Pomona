#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Threading.Tasks;

using Pomona.Common.Serialization;
using Pomona.Common.Web;

namespace Pomona.Common
{
    public interface IRequestDispatcher
    {
        IWebClient WebClient { get; }
        event EventHandler<ClientRequestLogEventArgs> RequestCompleted;
        object SendRequest(string uri, string httpMethod, object body, ISerializationContextProvider provider, RequestOptions options = null);


        Task<object> SendRequestAsync(string uri,
                                      string httpMethod,
                                      object body,
                                      ISerializationContextProvider provider,
                                      RequestOptions options = null);
    }
}