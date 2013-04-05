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

using System;
using System.IO;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Xml
{
    public class PomonaXmlDeserializer : IDeserializer<PomonaXmlDeserializer.Reader>
    {
        public Reader CreateReader(TextReader textReader)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(TextReader textReader, IMappedType expectedBaseType, IDeserializationContext context,
                                  object patchedObject = null)
        {
            throw new NotImplementedException();
        }


        public void DeserializeNode(IDeserializerNode node, Reader reader)
        {
            throw new NotImplementedException();
        }


        public void DeserializeNode(IDeserializerNode node, ISerializerReader reader)
        {
            throw new NotImplementedException();
        }


        public void DeserializeQueryResult(QueryResult queryResult, ISerializationContext fetchContext, Reader reader)
        {
            throw new NotImplementedException();
        }


        public void DeserializeQueryResult(QueryResult queryResult,
                                           ISerializationContext fetchContext,
                                           ISerializerReader reader)
        {
            throw new NotImplementedException();
        }


        ISerializerReader IDeserializer.CreateReader(TextReader textReader)
        {
            return CreateReader(textReader);
        }

        #region Nested type: Reader

        public class Reader : ISerializerReader
        {
        }

        #endregion
    }
}