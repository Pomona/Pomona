#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;

namespace Pomona.Example.Models
{
    public class StringToObjectDictionaryContainer : EntityBase, ISetEtaggedEntity
    {
        public StringToObjectDictionaryContainer()
        {
            Map = new Dictionary<string, object>();
        }


        public string ETag { get; private set; } = Guid.NewGuid().ToString();

        public IDictionary<string, object> Map { get; set; }


        public void SetEtag(string newEtagValue)
        {
            ETag = newEtagValue;
        }
    }
}
