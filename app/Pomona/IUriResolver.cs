#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using Pomona.Common.TypeSystem;

namespace Pomona
{
    public interface IUriResolver
    {
        string GetUriFor(object entity);
        string GetUriFor(PropertySpec property, object entity);
        string RelativeToAbsoluteUri(string uri);
        string ToRelativePath(string url);
    }
}

