#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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

namespace Pomona.Common.Serialization
{
    public abstract class TextSerializerFactoryBase<TSerializer>
        : TextSerializerFactoryBase<TSerializer, ITextDeserializer>
        where TSerializer : ITextSerializer
    {
        public sealed override ITextDeserializer GetDeserializer(ISerializationContextProvider contextProvider)
        {
            throw new NotSupportedException("Deserialization not supported for format.");
        }
    }

    public abstract class TextSerializerFactoryBase<TSerializer, TDeserializer> : ITextSerializerFactory
        where TSerializer : ITextSerializer
        where TDeserializer : ITextDeserializer
    {
        public abstract TDeserializer GetDeserializer(ISerializationContextProvider contextProvider);

        public abstract TSerializer GetSerializer(ISerializationContextProvider contextProvider);


        ITextDeserializer ITextSerializerFactory.GetDeserializer(ISerializationContextProvider contextProvider)
        {
            return GetDeserializer(contextProvider);
        }


        ITextSerializer ITextSerializerFactory.GetSerializer(ISerializationContextProvider contextProvider)
        {
            return GetSerializer(contextProvider);
        }
    }
}