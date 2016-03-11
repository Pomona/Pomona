#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq.Expressions;
using System.Net.Http;

namespace Pomona.Common
{
    public interface IRequestOptions
    {
        IRequestOptions ModifyRequest(Action<HttpRequestMessage> action);
    }

    public interface IRequestOptions<T> : IRequestOptions
    {
        IRequestOptions<T> Expand<TRetValue>(Expression<Func<T, TRetValue>> expression);
        new IRequestOptions<T> ModifyRequest(Action<HttpRequestMessage> action);
    }
}