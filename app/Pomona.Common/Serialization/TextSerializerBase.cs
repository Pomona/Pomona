#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.IO;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public abstract class TextSerializerBase<TWriter> : ITextSerializer
    {
        protected abstract TWriter CreateWriter(TextWriter textWriter);
        protected abstract void SerializeNode(ISerializerNode node, TWriter writer);


        protected void SerializeThroughContext(ISerializerNode node, TWriter writer)
        {
            node.Context.Serialize(node, n => SerializeNode(n, writer));
        }


        internal void Serialize(ISerializationContext serializationContext,
                                object obj,
                                TextWriter textWriter,
                                TypeSpec expectedBaseType)
        {
            if (textWriter == null)
                throw new ArgumentNullException(nameof(textWriter));

            var writer = CreateWriter(textWriter);
            var itemValueNode = new ItemValueSerializerNode(obj,
                                                            expectedBaseType,
                                                            string.Empty,
                                                            serializationContext,
                                                            null);
            SerializeNode(itemValueNode, writer);
        }


        public abstract void Serialize(TextWriter textWriter, object o, SerializeOptions options = null);
    }
}