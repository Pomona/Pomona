#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Pomona.Common.Internals
{
    public static class GenericInvoker
    {
        public static MethodInfo ExtractMethodInfo(this LambdaExpression expr)
        {
            return ReflectionHelper.GetMethodDefinition(expr);
        }


        public static InstanceMethodFactory<TInstance> Instance<TInstance>()
        {
            return new InstanceMethodFactory<TInstance>();
        }


        internal static GenericInvokerBase Create(Type delegateType, MethodInfo method)
        {
            return
                (GenericInvokerBase)
                    Activator.CreateInstance(typeof(GenericInvoker<>).MakeGenericType(delegateType), method);
        }

        #region Nested type: InstanceMethodFactory

        public class InstanceMethodFactory<TInstance>
        {
            public Action<Type, TInstance> CreateAction1(Expression<Action<TInstance>> expr)
            {
                return WrapInstanceGenericMethod<Action<Type, TInstance>>(1, expr);
            }


            public Action<Type, TInstance, T1> CreateAction1<T1>(Expression<Action<TInstance>> expr)
            {
                return WrapInstanceGenericMethod<Action<Type, TInstance, T1>>(1, expr);
            }


            public Action<Type, TInstance, T1, T2> CreateAction1<T1, T2>(Expression<Action<TInstance>> expr)
            {
                return WrapInstanceGenericMethod<Action<Type, TInstance, T1, T2>>(1, expr);
            }


            public Action<Type, TInstance, T1, T2, T3> CreateAction1<T1, T2, T3>(Expression<Action<TInstance>> expr)
            {
                return WrapInstanceGenericMethod<Action<Type, TInstance, T1, T2, T3>>(1, expr);
            }


            public Action<Type, Type, TInstance> CreateAction2(Expression<Action<TInstance>> expr)
            {
                return WrapInstanceGenericMethod<Action<Type, Type, TInstance>>(2, expr);
            }


            public Action<Type, Type, TInstance, T1> CreateAction2<T1>(Expression<Action<TInstance>> expr)
            {
                return WrapInstanceGenericMethod<Action<Type, Type, TInstance, T1>>(2, expr);
            }


            public Action<Type, Type, TInstance, T1, T2> CreateAction2<T1, T2>(Expression<Action<TInstance>> expr)
            {
                return WrapInstanceGenericMethod<Action<Type, Type, TInstance, T1, T2>>(2, expr);
            }


            public Action<Type, Type, TInstance, T1, T2, T3> CreateAction2<T1, T2, T3>(
                Expression<Action<TInstance>> expr)
            {
                return WrapInstanceGenericMethod<Action<Type, Type, TInstance, T1, T2, T3>>(2, expr);
            }


            public Func<Type, TInstance, TReturn> CreateFunc1<TReturn>(Expression<Func<TInstance, object>> expr)
            {
                return WrapInstanceGenericMethod<Func<Type, TInstance, TReturn>>(1, expr);
            }


            public Func<Type, TInstance, T1, TReturn> CreateFunc1<T1, TReturn>(Expression<Func<TInstance, object>> expr)
            {
                return WrapInstanceGenericMethod<Func<Type, TInstance, T1, TReturn>>(1, expr);
            }


            public Func<Type, TInstance, T1, T2, TReturn> CreateFunc1<T1, T2, TReturn>(
                Expression<Func<TInstance, object>> expr)
            {
                return WrapInstanceGenericMethod<Func<Type, TInstance, T1, T2, TReturn>>(1, expr);
            }


            public Func<Type, TInstance, T1, T2, T3, TReturn> CreateFunc1<T1, T2, T3, TReturn>(
                Expression<Func<TInstance, object>> expr)
            {
                return WrapInstanceGenericMethod<Func<Type, TInstance, T1, T2, T3, TReturn>>(1, expr);
            }


            public Func<Type, Type, TInstance, TReturn> CreateFunc2<TReturn>(Expression<Func<TInstance, object>> expr)
            {
                return WrapInstanceGenericMethod<Func<Type, Type, TInstance, TReturn>>(2, expr);
            }


            public Func<Type, Type, TInstance, T1, TReturn> CreateFunc2<T1, TReturn>(
                Expression<Func<TInstance, object>> expr)
            {
                return WrapInstanceGenericMethod<Func<Type, Type, TInstance, T1, TReturn>>(2, expr);
            }


            public Func<Type, Type, TInstance, T1, T2, TReturn> CreateFunc2<T1, T2, TReturn>(
                Expression<Func<TInstance, object>> expr)
            {
                return WrapInstanceGenericMethod<Func<Type, Type, TInstance, T1, T2, TReturn>>(2, expr);
            }


            public Func<Type, Type, TInstance, T1, T2, T3, TReturn> CreateFunc2<T1, T2, T3, TReturn>(
                Expression<Func<TInstance, object>> expr)
            {
                return WrapInstanceGenericMethod<Func<Type, Type, TInstance, T1, T2, T3, TReturn>>(2, expr);
            }


            public Func<Type, Type, TInstance, T1, T2, T3, T4, TReturn> CreateFunc2<T1, T2, T3, T4, TReturn>(
                Expression<Func<TInstance, object>> expr)
            {
                return WrapInstanceGenericMethod<Func<Type, Type, TInstance, T1, T2, T3, T4, TReturn>>(2, expr);
            }


            public Func<Type, Type, TInstance, T1, T2, T3, T4, T5, TReturn> CreateFunc2<T1, T2, T3, T4, T5, TReturn>(
                Expression<Func<TInstance, object>> expr)
            {
                return WrapInstanceGenericMethod<Func<Type, Type, TInstance, T1, T2, T3, T4, T5, TReturn>>(2, expr);
            }


            public Func<Type, Type, Type, TInstance, T1, T2, T3, T4, T5, TReturn> CreateFunc3<T1, T2, T3, T4, T5, TReturn>(
                Expression<Func<TInstance, object>> expr)
            {
                return WrapInstanceGenericMethod<Func<Type, Type, Type, TInstance, T1, T2, T3, T4, T5, TReturn>>(3, expr);
            }


            public Func<Type[], TInstance, T1, T2, TReturn> CreateFuncy<T1, T2, TReturn>(
                Expression<Func<TInstance, object>> expr)
            {
                var gi =
                    new GenericInvoker<Func<TInstance, T1, T2, TReturn>>(
                        expr.ExtractMethodInfo().GetGenericMethodDefinition());
                Func<Type[], TInstance, T1, T2, TReturn> wrappedFunc =
                    (ta, i, arg1, arg2) => gi.GetDelegate(ta)(i, arg1, arg2);
                return wrappedFunc;
            }


            private TDel WrapInstanceGenericMethod<TDel>(int typeArgCount, LambdaExpression expr)
                where TDel : class
            {
                var tDelInvoke = typeof(TDel).GetDelegateInvokeMethod();
                var innerDelegateSignature =
                    tDelInvoke.GetParameters().Skip(typeArgCount).Select(x => x.ParameterType).Append(
                        tDelInvoke.ReturnType).ToArray();
                var innerDelegate = Expression.GetDelegateType(innerDelegateSignature);
                var gi = Create(innerDelegate, expr.ExtractMethodInfo().GetGenericMethodDefinition());
                var typeParams =
                    Enumerable.Range(0, typeArgCount).Select(x => Expression.Parameter(typeof(Type), "targ" + x)).ToList
                        ();
                var forwardParams = gi.InArgs.Select(Expression.Parameter).ToList();

                return
                    Expression.Lambda<TDel>(Expression.Invoke(Expression.Call(Expression.Constant(gi),
                                                                              "GetDelegate",
                                                                              null,
                                                                              Expression.NewArrayInit(typeof(Type), typeParams)),
                                                              forwardParams),
                                            typeParams.Concat(forwardParams)).Compile();
            }
        }

        #endregion
    }

    /// <summary>
    /// For calling a method with ONE type argument and ONE argument
    /// </summary>
    public class GenericInvoker<T> : GenericInvokerBase
        where T : class
    {
        private static readonly Type delReturnType;
        private static readonly Type[] inArgs;
        private readonly MethodInfo methodDefinition;

        private readonly ConcurrentDictionary<Type[], T> methodInstanceCache =
            new ConcurrentDictionary<Type[], T>(new SequenceEqualityComparer<Type, Type[]>());


        static GenericInvoker()
        {
            var invokeMethod = typeof(T).GetMethod("Invoke");
            inArgs = invokeMethod.GetParameters().Select(x => x.ParameterType).ToArray();
            delReturnType = invokeMethod.ReturnType;
        }


        public GenericInvoker(MethodInfo methodDefinition)
        {
            this.methodDefinition = methodDefinition;
        }


        internal override Type[] InArgs => inArgs.ToArray();


        public static Expression<T> CreateCallWrapper(MethodInfo method, IEnumerable<Type> inArgs, Type returnType)
        {
            ParameterExpression instanceParam = null;
            var lambdaParams = new List<ParameterExpression>();
            var args = new List<Expression>();

            if (!method.IsStatic)
            {
                instanceParam = Expression.Parameter(method.ReflectedType, "_this");
                lambdaParams.Add(instanceParam);
                inArgs = inArgs.Skip(1);
            }

            foreach (var item in method.GetParameters().Zip(inArgs, (p, b) => new { p, inParamType = b }))
            {
                var par = Expression.Parameter(item.inParamType);
                Expression arg = par;
                if (arg.Type != item.p.ParameterType)
                    arg = Expression.Convert(arg, item.p.ParameterType);
                lambdaParams.Add(par);
                args.Add(arg);
            }

            Expression body;
            if (method.IsStatic)
                body = Expression.Call(method, args);
            else
                body = Expression.Call(instanceParam, method, args);
            if (returnType != typeof(void))
                body = Expression.Convert(body, returnType);
            return Expression.Lambda<T>(body, lambdaParams);
        }


        public T GetDelegate(Type[] typeArgs)
        {
            return this.methodInstanceCache
                       .GetOrAdd(typeArgs,
                                 k => CreateCallWrapper(this.methodDefinition.MakeGenericMethod(k), inArgs, delReturnType).Compile());
        }


        protected override Delegate OnGetDelegate(Type[] typeArgs)
        {
            return (Delegate)((object)GetDelegate(typeArgs));
        }

        #region Nested type: SequenceEqualityComparer

        private class SequenceEqualityComparer<TItem, TEnumerable> : IEqualityComparer<TEnumerable>
            where TEnumerable : class, IEnumerable<TItem>
        {
            private static readonly int initialHash = typeof(TItem).AssemblyQualifiedName.GetHashCode();


            public bool Equals(TEnumerable x, TEnumerable y)
            {
                if (ReferenceEquals(x, y))
                    return true;
                if (x == null)
                    return y == null;
                return x.SequenceEqual(y);
            }


            public int GetHashCode(TEnumerable obj)
            {
                var hash = initialHash;
                foreach (var item in obj)
                    hash = hash * 31 + item.GetHashCode();
                return hash;
            }
        }

        #endregion
    }
}

