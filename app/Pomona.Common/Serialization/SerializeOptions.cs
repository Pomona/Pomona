#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Common.Serialization
{
    public class SerializeOptions
    {
        public string ExpandedPaths { get; set; }
        public Type ExpectedBaseType { get; set; }
    }
}