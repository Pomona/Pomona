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
using System.Linq.Expressions;
using System.Reflection;
using Pomona.Example.Models;
using System.Linq;

namespace Pomona.Example
{
    /// <summary>
    /// This is used to simulate the fact that generated properties can't be queried from DB.
    /// </summary>
    public class ThrowOnCalculatedPropertyVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            if (typeof (EntityBase).IsAssignableFrom(node.Expression.Type))
            {
                MemberInfo memberInfo = node.Member;
                if (memberInfo.GetCustomAttributes(true).OfType<NotKnownToDataSourceAttribute>().Any())
                {
                    throw new InvalidOperationException("Property " + memberInfo.Name + " of entity "+ memberInfo.DeclaringType.Name + " is read-only, and shouldn't be used in search.");
                }
            }
            return base.VisitMember(node);
        }
    }
}