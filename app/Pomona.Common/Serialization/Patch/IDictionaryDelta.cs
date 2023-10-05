#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

namespace Pomona.Common.Serialization.Patch
{
    public interface IDictionaryDelta<TKey, TValue> : IDelta
    {
        IEnumerable<KeyValuePair<TKey, TValue>> ModifiedItems { get; }
        new IDictionary<TKey, TValue> Original { get; }
        IEnumerable<TKey> RemovedKeys { get; }
    }
}

