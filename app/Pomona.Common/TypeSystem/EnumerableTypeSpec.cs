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

namespace Pomona.Common.TypeSystem
{
    public class EnumerableTypeSpec : RuntimeTypeSpec
    {
        private readonly Lazy<TypeSpec> itemType;


        private EnumerableTypeSpec(ITypeResolver typeResolver, Type type, Lazy<TypeSpec> itemType)
            : base(typeResolver, type)
        {
            if (itemType == null)
                throw new ArgumentNullException("itemType");
            this.itemType = itemType;
        }


        public override TypeSpec ElementType
        {
            get { return ItemType; }
        }

        public override bool IsAlwaysExpanded
        {
            get { return false; }
        }

        public override bool IsCollection
        {
            get { return true; }
        }

        public virtual TypeSpec ItemType
        {
            get { return this.itemType.Value; }
        }


        public new static ITypeFactory GetFactory()
        {
            return new EnumerableTypeSpecFactory();
        }


        protected override TypeSerializationMode OnLoadSerializationMode()
        {
            if (Type == typeof(byte[]))
                return TypeSerializationMode.Value;
            return TypeSerializationMode.Array;
        }


        public class EnumerableTypeSpecFactory : ITypeFactory
        {
            public TypeSpec CreateFromType(ITypeResolver typeResolver, Type type)
            {
                if (typeResolver == null)
                    throw new ArgumentNullException("typeResolver");
                if (type == null)
                    throw new ArgumentNullException("type");

                Type itemTypeLocal;
                if (type == typeof(string) || !type.TryGetEnumerableElementType(out itemTypeLocal))
                    return null;

                return new EnumerableTypeSpec(typeResolver, type,
                                              CreateLazy(() => typeResolver.FromType(itemTypeLocal)));
            }


            public int Priority
            {
                get { return 50; }
            }
        }
    }
}