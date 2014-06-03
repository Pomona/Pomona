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

using Pomona.Common.Internals;
using Pomona.Common.Linq;

namespace Pomona.Common.ExtendedResources
{
    public class TransformAdditionalPropertiesToAttributesVisitor : ExpressionTypeVisitor
    {
        private static readonly MethodInfo dictionarySafeGetMethod;
        private readonly IClientTypeResolver client;

        private readonly IDictionary<ParameterExpression, ParameterExpression> replacementParameters =
            new Dictionary<ParameterExpression, ParameterExpression>();

        private IExtendedQueryableRoot root;


        static TransformAdditionalPropertiesToAttributesVisitor()
        {
            dictionarySafeGetMethod =
                ReflectionHelper.GetMethodDefinition<IDictionary<string, string>>(x => x.SafeGet(null));
        }


        public TransformAdditionalPropertiesToAttributesVisitor(IClientTypeResolver client)
        {
            this.client = client;
        }


        internal IExtendedQueryableRoot Root
        {
            get { return this.root; }
        }


        protected override Expression VisitConstant(ConstantExpression node)
        {
            var extendedQueryableRoot = node.Value as IExtendedQueryableRoot;
            if (extendedQueryableRoot != null)
            {
                if (this.root != null)
                {
                    throw new InvalidOperationException(
                        "Does not support queryable expression with multiple combined queryable sources!");
                }
                this.root = extendedQueryableRoot;
                return Expression.Constant(extendedQueryableRoot.WrappedSource);
            }
            return node;
        }


        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var visitedBody = Visit(node.Body);
            var visitedParams = node.Parameters.Select(Visit).Cast<ParameterExpression>();
            var visitedNode = Expression.Lambda(visitedBody, node.Name, node.TailCall, visitedParams);

            return visitedNode;
        }


        protected override Expression VisitMember(MemberExpression node)
        {
            var member = node.Member;
            ExtendedResourceInfo declaringUserTypeInfo;
            var propInfo = member as PropertyInfo;
            var visitedExpression = Visit(node.Expression);

            // Evaluate closures at this point
            var nodeExpression = Visit(node.Expression);
            if (nodeExpression == null || nodeExpression.NodeType == ExpressionType.Constant)
            {
                var target =  nodeExpression != null ? ((ConstantExpression)nodeExpression).Value : null;

                if (propInfo != null)
                    return Expression.Constant(propInfo.GetValue(target, null), propInfo.PropertyType);
                var fieldInfo = node.Member as FieldInfo;
                if (fieldInfo != null)
                    return Expression.Constant(fieldInfo.GetValue(target), fieldInfo.FieldType);
            }

            if (IsUserType(member.DeclaringType, out declaringUserTypeInfo))
            {
                if (propInfo == null)
                {
                    throw new InvalidOperationException(
                        "Only properties can be defined on custom user types, not methods or fields.");
                }

                Type memberServerType;
                if (TryReplaceWithServerType(propInfo.PropertyType, out memberServerType) &&
                    propInfo.GetIndexParameters().Length == 0)
                {
                    var serverProp =
                        declaringUserTypeInfo.ServerType.GetAllInheritedPropertiesFromInterface()
                            .FirstOrDefault(x => x.Name == propInfo.Name);
                    if (serverProp == null)
                    {
                        throw new InvalidOperationException("Unable to find underlying server side property " +
                                                            propInfo.Name);
                    }

                    //if (!serverProp.PropertyType.IsAssignableFrom(propInfo.PropertyType))
                    //    throw new InvalidOperationException("Unable to convert from type " + propInfo.PropertyType +
                    //                                        " to " + serverProp.PropertyType);

                    return Expression.Property(visitedExpression, serverProp);
                }
                else
                {
                    Type targetDictInterface;
                    var targetDictProperty = declaringUserTypeInfo.DictProperty;
                    var idictionaryMetadataToken = typeof(IDictionary<,>).UniqueToken();
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
                            Expression.Property(visitedExpression, targetDictProperty),
                            Expression.Constant(propInfo.Name));

                    if (attrAccessExpression.Type != propInfo.PropertyType)
                        attrAccessExpression = Expression.TypeAs(attrAccessExpression, propInfo.PropertyType);

                    return attrAccessExpression;
                }

                //return Expression.Call(Expression.Property(Visit(node.Expression), targetDictProperty), OdataFunctionMapping.DictGetMethod,
                //                       Expression.Constant(propInfo.Name));
            }

            var originalDeclaringType = node.Member.DeclaringType;
            var modifiedDeclaringType = ReplaceInGenericArguments(originalDeclaringType);
            if (modifiedDeclaringType != originalDeclaringType)
            {
                var modifiedMember =
                    modifiedDeclaringType.GetMember(node.Member.Name,
                        node.Member.MemberType,
                        BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic |
                        BindingFlags.Public)
                        .First(x => x.UniqueToken() == node.Member.UniqueToken());
                return Expression.MakeMemberAccess(node.Expression != null ? visitedExpression : null,
                    modifiedMember);
            }

            return base.VisitMember(node);
        }


        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var modifiedMethod = TypeUtils.ReplaceInGenericMethod(node.Method, ReplaceType);
            var modifiedArguments = node.Arguments.Select(Visit).ToList();

            return Expression.Call(
                node.Object != null ? Visit(node.Object) : null,
                modifiedMethod,
                modifiedArguments);
        }


        protected override Expression VisitParameter(ParameterExpression node)
        {
            var serverType = ReplaceInGenericArguments(node.Type);
            if (serverType != node.Type)
            {
                return this.replacementParameters.GetOrCreate(node,
                    () => Expression.Parameter(serverType, node.Name));
            }
            return base.VisitParameter(node);
        }


        private bool IsUserType(Type userType)
        {
            ExtendedResourceInfo tmpvar;
            return IsUserType(userType, out tmpvar);
        }


        private bool IsUserType(Type userType, out ExtendedResourceInfo userTypeInfo)
        {
            return ExtendedResourceInfo.TryGetExtendedResourceInfo(userType, this.client, out userTypeInfo);
        }


        public Type ReplaceInGenericArguments(Type typeToSearch)
        {
            return TypeUtils.ReplaceInGenericArguments(typeToSearch, ReplaceType);
        }


        private Type ReplaceType(Type t)
        {
            ExtendedResourceInfo userTypeInfo;
            if (IsUserType(t, out userTypeInfo))
                return userTypeInfo.ServerType;

            return t;
        }


        private bool TryReplaceWithServerType(Type userType, out Type serverType)
        {
            serverType = ReplaceInGenericArguments(userType);
            return userType != serverType;
        }
    }
}