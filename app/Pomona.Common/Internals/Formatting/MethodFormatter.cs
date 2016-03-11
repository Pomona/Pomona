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
using System.Linq;
using System.Reflection;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Internals.Formatting
{
    internal class MethodFormatter : FormatterBase
    {
        private readonly MethodInfo method;


        public MethodFormatter(MethodInfo method)
            : base(method.Maybe().Select(m => m.Name).OrDefault())
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            this.method = method;
        }


        public override string ToString()
        {
            var parameters = this.method.GetParameters().Select(x => new TypeFormatter(x.ParameterType));
            var parameterString = String.Join(", ", parameters);

            return String.Format("{0} {1}.{2}({3})",
                                 new TypeFormatter(this.method.ReturnType),
                                 new TypeFormatter(this.method.DeclaringType),
                                 base.ToString(),
                                 parameterString);
        }


        protected override Type[] GetGenericArguments()
        {
            if (this.method.DeclaringType == null || !this.method.IsGenericMethod)
                return null;

            var genericMethodDefinitionArguments = this.method
                                                       .GetGenericMethodDefinition()
                                                       .GetGenericArguments();

            return this.method
                       .GetGenericArguments()
                       .Zip(genericMethodDefinitionArguments, (a, p) => a ?? p)
                       .ToArray();
        }
    }
}