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

using System.Linq.Expressions;
using System.Reflection;

namespace Pomona.Common.Internals
{
    public class EvaluateClosureMemberVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == null)
                return Expression.Constant(node.Member.GetPropertyOrFieldValue(null));

            var nodeExpression = Visit(node.Expression);
            if (nodeExpression.NodeType == ExpressionType.Constant)
            {
                var target = ((ConstantExpression)nodeExpression).Value;

                var propInfo = node.Member as PropertyInfo;
                if (propInfo != null)
                    return Expression.Constant(propInfo.GetValue(target, null), propInfo.PropertyType);
                var fieldInfo = node.Member as FieldInfo;
                if (fieldInfo != null)
                    return Expression.Constant(fieldInfo.GetValue(target), fieldInfo.FieldType);
            }
            return base.VisitMember(node);
        }
    }
}