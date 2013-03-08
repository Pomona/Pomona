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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Pomona.Internals;

namespace Pomona.Common.Linq
{
    public class TransformAdditionalPropertiesToAttributesVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo dictionarySafeGetMethod;
        private readonly ParameterExpression newParam;
        private readonly ParameterExpression origParam;
        private readonly PropertyInfo targetDictProperty;
        private readonly Type targetType;
        private readonly Type userType;


        static TransformAdditionalPropertiesToAttributesVisitor()
        {
            dictionarySafeGetMethod =
                ReflectionHelper.GetGenericMethodDefinition<IDictionary<string, string>>(x => x.SafeGet(null));
        }


        public TransformAdditionalPropertiesToAttributesVisitor(
            Type userType, Type targetType, PropertyInfo targetDictProperty)
        {
            if (userType == null)
                throw new ArgumentNullException("userType");
            if (targetType == null)
                throw new ArgumentNullException("targetType");
            if (targetDictProperty == null)
                throw new ArgumentNullException("targetDictProperty");
            this.userType = userType;
            this.targetType = targetType;
            this.targetDictProperty = targetDictProperty;
        }


        private TransformAdditionalPropertiesToAttributesVisitor(
            Type userType,
            Type targetType,
            PropertyInfo targetDictProperty,
            ParameterExpression origParam,
            ParameterExpression newParam) : this(userType, targetType, targetDictProperty)
        {
            if (origParam == null)
                throw new ArgumentNullException("origParam");
            if (newParam == null)
                throw new ArgumentNullException("newParam");
            this.origParam = origParam;
            this.newParam = newParam;
        }


        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type.UniqueToken() == typeof (RestQuery<>).UniqueToken() &&
                node.Type.GetGenericArguments()[0] == userType)
            {
                var queryable = node.Value as IQueryable;
                if (queryable != null)
                {
                    var provider = (RestQueryProvider) queryable.Provider;
                    var restQueryOfTargetType = typeof (RestQuery<>).MakeGenericType(targetType);
                    var modifiedSourceQueryable = Activator.CreateInstance(
                        restQueryOfTargetType, new RestQueryProvider(provider.Client, targetType));
                    return Expression.Constant(modifiedSourceQueryable, restQueryOfTargetType);
                }
            }
            return base.VisitConstant(node);
        }


        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var body = node.Body;
            var replacementParams = new List<ParameterExpression>(node.Parameters.Count);
            foreach (var param in node.Parameters)
            {
                if (param.Type == userType)
                {
                    var replacementParam = Expression.Parameter(targetType, param.Name);
                    var visitor = new TransformAdditionalPropertiesToAttributesVisitor(
                        userType,
                        targetType,
                        targetDictProperty,
                        param,
                        replacementParam);
                    replacementParams.Add(replacementParam);
                    body = visitor.Visit(body);
                }
                else
                    replacementParams.Add(param);
            }

            return Expression.Lambda(body, replacementParams);
        }


        protected override Expression VisitMember(MemberExpression node)
        {
            if (origParam != null)
            {
                var member = node.Member;
                if (member.DeclaringType == userType)
                {
                    var propInfo = member as PropertyInfo;
                    if (propInfo == null)
                        throw new InvalidOperationException(
                            "Only properties can be defined on custom user types, not methods or fields.");

                    Type targetDictInterface;
                    var idictionaryMetadataToken = typeof (IDictionary<,>).UniqueToken();
                    if (targetDictProperty.PropertyType.UniqueToken() == idictionaryMetadataToken)
                        targetDictInterface = targetDictProperty.PropertyType;
                    else
                    {
                        targetDictInterface = targetDictProperty
                            .PropertyType
                            .GetInterfaces()
                            .FirstOrDefault(x => x.UniqueToken() == idictionaryMetadataToken);

                        if (targetDictInterface == null)
                        {
                            throw new InvalidOperationException(
                                "Unable to find IDictionary interface in type "
                                + targetDictProperty.PropertyType.FullName);
                        }
                    }

                    Expression attrAccessExpression =
                        Expression.Call(
                            dictionarySafeGetMethod.MakeGenericMethod(targetDictInterface.GetGenericArguments()),
                            Expression.Property(Visit(node.Expression), targetDictProperty),
                            Expression.Constant(propInfo.Name));

                    if (attrAccessExpression.Type != propInfo.PropertyType)
                        attrAccessExpression = Expression.TypeAs(attrAccessExpression, propInfo.PropertyType);

                    return attrAccessExpression;

                    //return Expression.Call(Expression.Property(Visit(node.Expression), targetDictProperty), OdataFunctionMapping.DictGetMethod,
                    //                       Expression.Constant(propInfo.Name));
                }
            }
            return base.VisitMember(node);
        }


        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return Expression.Call(
                node.Object != null ? Visit(node.Object) : null,
                ReplaceInGenericMethod(node.Method, userType, targetType),
                node.Arguments.Select(Visit));
        }


        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == origParam)
                return newParam;
            return base.VisitParameter(node);
        }


        private static Type ReplaceInGenericArguments(Type typeToSearch, Type typeToReplace, Type replacementType)
        {
            if (typeToSearch == typeToReplace)
                return replacementType;

            if (typeToSearch.IsGenericType)
            {
                var genArgs = typeToSearch.GetGenericArguments();
                var newGenArgs =
                    genArgs.Select(x => ReplaceInGenericArguments(x, typeToReplace, replacementType)).ToArray();

                if (newGenArgs.SequenceEqual(genArgs))
                    return typeToSearch;

                return typeToSearch.GetGenericTypeDefinition().MakeGenericType(newGenArgs);
            }

            return typeToSearch;
        }


        private static MethodInfo ReplaceInGenericMethod(
            MethodInfo methodToSearch, Type typeToReplace, Type replacementType)
        {
            if (!methodToSearch.IsGenericMethod)
                return methodToSearch;

            var genArgs = methodToSearch.GetGenericArguments();
            var newGenArgs = genArgs.Select(x => ReplaceInGenericArguments(x, typeToReplace, replacementType)).ToArray();
            if (genArgs.SequenceEqual(newGenArgs))
                return methodToSearch;

            return methodToSearch.GetGenericMethodDefinition().MakeGenericMethod(newGenArgs);
        }
    }
}