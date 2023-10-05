#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

using Pomona.Common.TypeSystem;
using Pomona.Routing;

namespace Pomona.Queries
{
    public interface IQueryableResolver
    {
        /// <summary>
        /// Get the ResourceCollectionNode as IQueryable
        /// </summary>
        /// <param name="node">The node to get corresponding IQueryable for.</param>
        /// <param name="ofType">Optional: The subclass to get.</param>
        /// <returns>The resulting IQueryable if success, null if not.</returns>
        IQueryable Resolve(UrlSegment node, TypeSpec ofType = null);
    }
}
