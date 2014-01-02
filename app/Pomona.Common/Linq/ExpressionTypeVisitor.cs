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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Pomona.Common.Linq
{
    public class ExpressionTypeVisitor : ExpressionVisitor
    {
        private readonly IDictionary<ParameterExpression, ParameterExpression> replacementParameters =
            new Dictionary<ParameterExpression, ParameterExpression>();


        protected virtual MethodInfo VisitMethod(MethodInfo methodToSearch)
        {
            var newReflectedType = VisitType(methodToSearch.ReflectedType);
            if (newReflectedType != methodToSearch.ReflectedType)
            {
                methodToSearch = newReflectedType.GetMethod(methodToSearch.Name,
                    (methodToSearch.IsStatic ? BindingFlags.Static : BindingFlags.Instance)
                    | (methodToSearch.IsPublic
                        ? BindingFlags.Public
                        : BindingFlags.NonPublic),
                    null,
                    methodToSearch.GetParameters().Select(x => x.ParameterType).ToArray(),
                    null);
            }

            if (!methodToSearch.IsGenericMethod)
                return methodToSearch;

            var genArgs = methodToSearch.GetGenericArguments();
            var newGenArgs = genArgs.Select(VisitType).ToArray();
            if (genArgs.SequenceEqual(newGenArgs))
                return methodToSearch;

            return methodToSearch.GetGenericMethodDefinition().MakeGenericMethod(newGenArgs);
        }


        protected virtual Type VisitType(Type typeToSearch)
        {
            if (typeToSearch.IsGenericType)
            {
                var genArgs = typeToSearch.GetGenericArguments();
                var newGenArgs =
                    genArgs.Select(x => VisitType(typeToSearch)).ToArray();

                if (newGenArgs.SequenceEqual(genArgs))
                    return typeToSearch;

                return typeToSearch.GetGenericTypeDefinition().MakeGenericType(newGenArgs);
            }

            return typeToSearch;
        }


        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var replacementMethod = VisitMethod(node.Method);
            if (replacementMethod != node.Method)
            {
                var visitedArguments = Visit(node.Arguments);
                if (node.Object != null)
                    return Expression.Call(node.Object, replacementMethod, visitedArguments);
                return Expression.Call(replacementMethod, visitedArguments);
            }
            return base.VisitMethodCall(node);
        }


        protected override Expression VisitParameter(ParameterExpression node)
        {
            var serverType = VisitType(node.Type);
            if (serverType != node.Type)
                return this.replacementParameters.GetOrCreate(node, () => Expression.Parameter(serverType, node.Name));
            return base.VisitParameter(node);
        }
    }
}