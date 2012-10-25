#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using System.Reflection;

namespace Pomona.Internals
{
    public static class ReflectionHelper
    {
        public static MethodInfo GetGenericMethodDefinition<TInstance>(Expression<Func<TInstance, object>> expr)
        {
            var callExpressionBody = expr.Body as MethodCallExpression;
            if (callExpressionBody == null)
                throw new ArgumentException("Needs node of type Call, was " + expr.Body.NodeType);

            var method = callExpressionBody.Method;
            if (!method.IsGenericMethod)
                throw new ArgumentException("Can't get generic method definition of non-generic method " + method.Name);

            return method.GetGenericMethodDefinition();
        }


        public static MethodInfo GetInstanceMethodInfo<TInstance>(Expression<Func<TInstance, object>> expr)
        {
            var body = expr.Body;
            if (body.NodeType != ExpressionType.Call)
                throw new ArgumentException("Needs node of type Call");

            var call = (MethodCallExpression)body;
            return call.Method;
        }


        public static MethodInfo GetMethodInfo<TO1, TOResult>(Expression<Func<TO1, TOResult>> expr)
        {
            var body = expr.Body;
            if (body.NodeType != ExpressionType.Call)
                throw new ArgumentException("Needs node of type Call");

            var call = (MethodCallExpression)body;
            return call.Method;
        }
    }
}