#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

namespace Pomona.Common.Serialization.Patch
{
    public interface ICollectionDelta : IDelta
    {
        IEnumerable<object> AddedItems { get; }
        bool Cleared { get; }
        IEnumerable<Delta> ModifiedItems { get; }
        IEnumerable<object> RemovedItems { get; }
    }
}
