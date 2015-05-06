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
using System.Linq;

namespace Pomona.Common.TypeSystem
{
    public class EnumTypeSpec : RuntimeTypeSpec
    {
        private readonly Lazy<IDictionary<string, long>> enumValues;


        public EnumTypeSpec(ITypeResolver typeResolver, Type type, Func<IEnumerable<TypeSpec>> genericArguments = null)
            : base(typeResolver, type, genericArguments)
        {
            this.enumValues =
                CreateLazy(
                    () =>
                        (IDictionary<string, long>)new ReadOnlyDictionary<string, long>(
                        Enum.GetValues(type).Cast<object>().ToDictionary(x => Enum.GetName(type, x), Convert.ToInt64)));
        }


        public IDictionary<string, long> EnumValues
        {
            get { return this.enumValues.Value; }
        }

        public override TypeSerializationMode SerializationMode
        {
            get { return TypeSerializationMode.Value; }
        }


        public new static ITypeFactory GetFactory()
        {
            return new EnumTypeSpecFactory();
        }


        public class EnumTypeSpecFactory : ITypeFactory
        {
            public TypeSpec CreateFromType(ITypeResolver typeResolver, Type type)
            {
                if (typeResolver == null)
                    throw new ArgumentNullException("typeResolver");
                if (type == null)
                    throw new ArgumentNullException("type");

                if (!type.IsEnum)
                    return null;

                return new EnumTypeSpec(typeResolver, type);
            }


            public int Priority
            {
                get { return -400; }
            }
        }
    }
}