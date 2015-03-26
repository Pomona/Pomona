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
using System.Text;

namespace Pomona.Common.Internals.Formatting
{
    internal abstract class FormatterBase
    {
        private readonly string name;


        protected FormatterBase(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            this.name = name;
        }


        public override string ToString()
        {
            var genericParameterString = GetGenericParameterString();
            return String.Concat(this.name, genericParameterString);
        }


        protected abstract Type[] GetGenericArguments();


        private string GetGenericParameterString()
        {
            var genericArguments = GetGenericArguments();

            if (genericArguments == null || !genericArguments.Any())
                return null;

            var genericParameterBuilder = new StringBuilder("<");

            for (int i = 0; i < genericArguments.Length; i++)
            {
                var genericArgument = new TypeFormatter(genericArguments[i]);
                genericParameterBuilder.Append(genericArgument);

                if (i < genericArguments.Length - 1)
                    genericParameterBuilder.Append(", ");
            }

            genericParameterBuilder.Append(">");

            return genericParameterBuilder.ToString();
        }
    }
}