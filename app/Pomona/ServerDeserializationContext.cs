#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class ServerDeserializationContext : IDeserializationContext
    {
        private readonly IPomonaSession pomonaSession;

        public ServerDeserializationContext(IPomonaSession pomonaSession)
        {
            this.pomonaSession = pomonaSession;
        }

        public IMappedType GetClassMapping(Type type)
        {
            return pomonaSession.TypeMapper.GetClassMapping(type);
        }

        public object CreateReference(IMappedType type, string uri)
        {
            return pomonaSession.GetResultByUri(uri);
        }

        public void Deserialize<TReader>(IDeserializerNode node, IDeserializer<TReader> deserializer, TReader reader)
            where TReader : ISerializerReader
        {
            deserializer.DeserializeNode(node, reader);
        }

        public IMappedType GetTypeByName(string typeName)
        {
            return pomonaSession.TypeMapper.GetClassMapping(typeName);
        }

        public void SetProperty(IDeserializerNode targetNode, IPropertyInfo property, object propertyValue)
        {
            if (!property.IsWriteable)
            {
                var propPath = string.IsNullOrEmpty(targetNode.ExpandPath)
                                   ? property.Name
                                   : targetNode.ExpandPath + "." + property.Name;
                throw new ResourceValidationException(
                    string.Format("Property {0} of resource {1} is not writable.", property.Name,
                                  targetNode.ValueType.Name), propPath,
                    targetNode.ValueType.Name, null);
            }

            property.Setter(targetNode.Value, propertyValue);
        }
    }
}