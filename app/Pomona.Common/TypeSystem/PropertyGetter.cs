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
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Pomona.Common.TypeSystem
{
    public class PropertyGetter
    {
        private readonly Func<object, IContainer, object> del;


        public PropertyGetter(Func<object, IContainer, object> del)
        {
            if (del == null)
                throw new ArgumentNullException(nameof(del));
            this.del = del;
        }


        public object Invoke(object target, IContainer container)
        {
            container = container ?? new NoContainer();
            return this.del(target, container);
        }


        public static implicit operator PropertyGetter(Func<object, IContainer, object> del)
        {
            if (del == null)
                return null;
            return new PropertyGetter(del);
        }


        public static implicit operator PropertyGetter(Expression<Func<object, IContainer, object>> expression)
        {
            if (expression == null)
                return null;
            return new PropertyExpressionGetter(expression);
        }


        public static explicit operator Func<object, IContainer, object>(PropertyGetter propertyGetter)
        {
            if (propertyGetter == null)
                throw new ArgumentNullException(nameof(propertyGetter));
            return propertyGetter.del;
        }
    }
}