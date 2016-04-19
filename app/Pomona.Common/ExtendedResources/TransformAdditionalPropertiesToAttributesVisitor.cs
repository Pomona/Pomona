#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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
    internal class TransformAdditionalPropertiesToAttributesVisitor : ExpressionTypeVisitor
    {
        private static readonly MethodInfo dictionarySafeGetMethod;
        private readonly ExtendedResourceMapper mapper;

        private readonly IDictionary<ParameterExpression, ParameterExpression> replacementParameters =
            new Dictionary<ParameterExpression, ParameterExpression>();


        static TransformAdditionalPropertiesToAttributesVisitor()
        {
            dictionarySafeGetMethod =
                ReflectionHelper.GetMethodDefinition<IDictionary<string, string>>(x => x.SafeGet(null));
        }


        public TransformAdditionalPropertiesToAttributesVisitor(ExtendedResourceMapper mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            this.mapper = mapper;
        }


        internal IExtendedQueryableRoot Root { get; private set; }


        public Type ReplaceInGenericArguments(Type typeToSearch)
        {
            return TypeUtils.ReplaceInGenericArguments(typeToSearch, ReplaceType);
        }


        public override Type VisitType(Type typeToSearch)
        {
            return base.VisitType(ReplaceType(typeToSearch));
        }


        protected override Expression VisitConstant(ConstantExpression node)
        {
            var extendedQueryableRoot = node.Value as IExtendedQueryableRoot;
            if (extendedQueryableRoot != null)
            {
                if (Root != null)
                {
                    throw new InvalidOperationException(
                        "Does not support queryable expression with multiple combined queryable sources!");
                }
                Root = extendedQueryableRoot;
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
                var target = nodeExpression != null ? ((ConstantExpression)nodeExpression).Value : null;

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
                    throw new ExtendedResourceMappingException(
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
                        throw new ExtendedResourceMappingException("Unable to find underlying server side property " +
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


        private bool IsUserType(Type userType, out ExtendedResourceInfo userTypeInfo)
        {
            return this.mapper.TryGetExtendedResourceInfo(userType, out userTypeInfo);
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