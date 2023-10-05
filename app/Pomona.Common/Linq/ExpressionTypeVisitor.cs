#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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


        public virtual Type VisitType(Type typeToSearch)
        {
            if (typeToSearch.IsGenericType)
            {
                var genArgs = typeToSearch.GetGenericArguments();
                var newGenArgs =
                    genArgs.Select(VisitType).ToArray();

                if (newGenArgs.SequenceEqual(genArgs))
                    return typeToSearch;

                return typeToSearch.GetGenericTypeDefinition().MakeGenericType(newGenArgs);
            }

            return typeToSearch;
        }


        protected virtual ConstructorInfo VisitConstructor(ConstructorInfo methodToSearch)
        {
            var newReflectedType = VisitType(methodToSearch.ReflectedType);
            if (newReflectedType != methodToSearch.ReflectedType)
            {
                methodToSearch = newReflectedType.GetConstructor(
                    (methodToSearch.IsStatic ? BindingFlags.Static : BindingFlags.Instance)
                    | (methodToSearch.IsPublic
                        ? BindingFlags.Public
                        : BindingFlags.NonPublic),
                    null,
                    methodToSearch.GetParameters().Select(x => VisitType(x.ParameterType)).ToArray(),
                    null);
            }

            return methodToSearch;
        }


        protected virtual FieldInfo VisitField(FieldInfo field)
        {
            var origType = field.DeclaringType;
            var replacedType = VisitType(origType);
            if (replacedType != origType)
            {
                return field.IsStatic
                    ? replacedType.GetField(field.Name,
                                            BindingFlags.Static | BindingFlags.NonPublic
                                            | BindingFlags.Public)
                    : replacedType.GetField(field.Name,
                                            BindingFlags.Instance | BindingFlags.NonPublic
                                            | BindingFlags.Public);
            }
            return field;
        }


        protected virtual MemberInfo VisitMemberInfo(MemberInfo member)
        {
            var methodBase = member as MethodBase;
            if (methodBase != null)
                return VisitMethodBase(methodBase);

            var prop = member as PropertyInfo;
            if (prop != null)
                return VisitProperty(prop);
            var field = member as FieldInfo;
            if (field != null)
                return VisitField(field);
            var type = member as Type;
            if (type != null)
                return VisitType(type);
            return member;
        }


        protected virtual MethodInfo VisitMethod(MethodInfo methodToSearch)
        {
            var newReflectedType = VisitType(methodToSearch.ReflectedType);
            if (newReflectedType != methodToSearch.ReflectedType)
            {
                methodToSearch = newReflectedType.GetMethod(methodToSearch.Name,
                                                            (methodToSearch.IsStatic
                                                                ? BindingFlags.Static
                                                                : BindingFlags.Instance)
                                                            | (methodToSearch.IsPublic
                                                                ? BindingFlags.Public
                                                                : BindingFlags.NonPublic),
                                                            null,
                                                            methodToSearch.GetParameters().Select(
                                                                x => VisitType(x.ParameterType)).ToArray(),
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


        protected virtual MethodBase VisitMethodBase(MethodBase methodBase)
        {
            var methodInfo = methodBase as MethodInfo;
            if (methodInfo != null)
                return VisitMethod(methodInfo);
            var ctorInfo = methodBase as ConstructorInfo;
            if (ctorInfo != null)
                return VisitConstructor(ctorInfo);
            return methodBase;
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


        protected override Expression VisitNew(NewExpression node)
        {
            var replacementCtor = VisitConstructor(node.Constructor);
            if (replacementCtor != node.Constructor)
            {
                var visitedArguments = Visit(node.Arguments);
                return Expression.New(replacementCtor, visitedArguments, node.Members != null ? node.Members.Select(VisitMemberInfo) : null);
            }
            return base.VisitNew(node);
        }


        protected override Expression VisitParameter(ParameterExpression node)
        {
            var serverType = VisitType(node.Type);
            if (serverType != node.Type)
                return this.replacementParameters.GetOrCreate(node, () => Expression.Parameter(serverType, node.Name));
            return base.VisitParameter(node);
        }


        protected virtual PropertyInfo VisitProperty(PropertyInfo prop)
        {
            var origType = prop.DeclaringType;
            var replacedType = VisitType(origType);
            if (replacedType != origType)
            {
                return prop.IsStatic()
                    ? replacedType.GetProperty(prop.Name,
                                               BindingFlags.Static | BindingFlags.NonPublic
                                               | BindingFlags.Public)
                    : replacedType.GetProperty(prop.Name,
                                               BindingFlags.Instance | BindingFlags.NonPublic
                                               | BindingFlags.Public);
            }
            return prop;
        }
    }
}

