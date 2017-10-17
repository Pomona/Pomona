#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Threading.Tasks;

namespace Pomona
{
    public interface IResourceResolver
    {
        // TODO: Rename to Resolve. @asbjornu
        object ResolveUri(string uri);
    }
}