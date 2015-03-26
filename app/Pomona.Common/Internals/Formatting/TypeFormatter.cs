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
using System.Linq;

namespace Pomona.Common.Internals.Formatting
{
    internal class TypeFormatter : FormatterBase
    {
        private readonly Type type;


        public TypeFormatter(Type type)
            : base(GetName(type))
        {
            this.type = type;
        }


        protected override Type[] GetGenericArguments()
        {
            if (!this.type.IsGenericType)
                return null;

            var genericTypeDefinitionArguments = this.type
                                                     .GetGenericTypeDefinition()
                                                     .GetGenericArguments();

            return this.type
                       .GetGenericArguments()
                       .Zip(genericTypeDefinitionArguments, (a, p) => a ?? p)
                       .ToArray();
        }


        private static string GetName(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var name = genericTypeDefinition.FullName ?? genericTypeDefinition.Name;
                var tickIndex = name.IndexOf('`');
                return tickIndex > -1 ? name.Substring(0, tickIndex) : name;
            }

            return type.FullName ?? type.Name;
        }
    }
}