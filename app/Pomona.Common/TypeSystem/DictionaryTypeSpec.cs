#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using System.Collections.Generic;

namespace Pomona.Common.TypeSystem
{
    public class DictionaryTypeSpec : RuntimeTypeSpec
    {
        private readonly Lazy<TypeSpec> keyType;
        private readonly Lazy<TypeSpec> valueType;


        public DictionaryTypeSpec(ITypeResolver typeResolver,
                                  Type type,
                                  Func<TypeSpec> keyType,
                                  Func<TypeSpec> valueType)
            : base(typeResolver, type)
        {
            if (keyType == null)
                throw new ArgumentNullException("keyType");
            if (valueType == null)
                throw new ArgumentNullException("valueType");
            this.keyType = CreateLazy(keyType);
            this.valueType = CreateLazy(valueType);
        }


        public override bool IsDictionary
        {
            get { return true; }
        }

        public virtual TypeSpec KeyType
        {
            get { return this.keyType.Value; }
        }

        public override TypeSerializationMode SerializationMode
        {
            get { return TypeSerializationMode.Dictionary; }
        }

        public virtual TypeSpec ValueType
        {
            get { return this.valueType.Value; }
        }


        public new static ITypeFactory GetFactory()
        {
            return new DictionaryTypeSpecFactory();
        }

        #region Nested type: DictionaryTypeSpecFactory

        internal class DictionaryTypeSpecFactory : ITypeFactory
        {
            public TypeSpec CreateFromType(ITypeResolver typeResolver, Type type)
            {
                Type[] typeArgs;
                if (!type.TryExtractTypeArguments(typeof(IDictionary<,>), out typeArgs))
                    return null;
                var keyType = typeArgs[0];
                var valueType = typeArgs[1];
                return new DictionaryTypeSpec(typeResolver,
                                              type,
                                              () => typeResolver.FromType(keyType),
                                              () => typeResolver.FromType(valueType));
            }


            public int Priority
            {
                get { return 0; }
            }
        }

        #endregion
    }
}