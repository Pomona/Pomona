#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Common.Serialization
{
    public class DeserializeOptions
    {
        /// <summary>
        /// Expected type of deserialized object.
        /// Required when type is not encoded in serialized data.
        /// </summary>
        public Type ExpectedBaseType { get; set; }

        /// <summary>
        /// Object to deserialize to.
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// The target node.
        /// </summary>
        public IResourceNode TargetNode { get; set; }
    }
}