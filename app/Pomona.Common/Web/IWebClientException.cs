#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Common.Web
{
    public interface IWebClientException<out TBody>
    {
        TBody Body { get; }
        string Message { get; }
    }
}