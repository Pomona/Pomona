#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Pomona.Common.Internals
{
    public static class ReflectionHelper
    {
        public static MemberInfo GetInstanceMemberInfo<TInstance>(Expression<Func<TInstance, object>> expr)
        {
            var body = expr.Body;
            if (body.NodeType == ExpressionType.Convert)
                body = ((UnaryExpression)body).Operand;

            if (body.NodeType == ExpressionType.Call)
                return ((MethodCallExpression)body).Method;

            if (body.NodeType == ExpressionType.MemberAccess)
                return ((MemberExpression)body).Member;

            throw new ArgumentException("Needs node of type Call or MemberAccess");
        }


        public static MethodInfo GetInstanceMethodInfo<TInstance>(Expression<Func<TInstance, object>> expr)
        {
            var body = expr.Body;
            if (body.NodeType == ExpressionType.Convert)
                body = ((UnaryExpression)body).Operand;

            if (body.NodeType != ExpressionType.Call)
                throw new ArgumentException("Needs node of type Call");

            var call = (MethodCallExpression)body;
            return call.Method;
        }


        public static MethodInfo GetMethodDefinition<TInstance>(Expression<Action<TInstance>> expr)
        {
            return GetMethodDefinition((LambdaExpression)expr);
        }


        public static MethodInfo GetMethodDefinition(Expression<Action> expr)
        {
            return GetMethodDefinition((LambdaExpression)expr);
        }


        public static MethodInfo GetMethodDefinition(LambdaExpression expr)
        {
            var body = expr.Body;
            while (body.NodeType == ExpressionType.Convert)
                body = ((UnaryExpression)body).Operand;

            var callExpressionBody = body as MethodCallExpression;
            if (callExpressionBody == null)
                throw new ArgumentException("Needs node of type Call, was " + expr.Body.NodeType);

            var method = callExpressionBody.Method;
            if (!method.IsGenericMethod)
                return method;

            return method.GetGenericMethodDefinition();
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

