#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

namespace Pomona.Example.Models
{
    public class DictionaryContainer : EntityBase
    {
        public DictionaryContainer()
        {
            Map = new Dictionary<string, string>();
        }


        public IDictionary<string, string> Map { get; set; }
    }
}

