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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

using Pomona.Common;

namespace Pomona.Ioc
{
    public abstract class RuntimeContainerWrapper : IServiceProvider
    {
        public static readonly ReadOnlyCollection<string> PreferredContainersTypes = new List<string>()
        {
            "Castle.Windsor.IWindsorContainer",
            "StructureMap.IContainer",
            "Autofac.ILifetimeScope",
            "Ninject.IKernel"
        }.AsReadOnly();

        private static readonly ConcurrentDictionary<Type, Func<object, RuntimeContainerWrapper>> ctorCache =
            new ConcurrentDictionary<Type, Func<object, RuntimeContainerWrapper>>();


        public static RuntimeContainerWrapper Create(object container)
        {
            var containerType = container.GetType();

            return ctorCache.GetOrAdd(containerType, CreateWrapperCtor)(container);
        }


        public abstract object GetInstance(Type serviceType);


        private static Func<object, RuntimeContainerWrapper> CreateWrapperCtor(Type containerType)
        {
            var interfaceType =
                containerType.GetInterfaces().FirstOrDefault(x => PreferredContainersTypes.Contains(x.FullName))
                ?? containerType;
            var wrapperTypeInstance = typeof(RuntimeContainerWrapper<>).MakeGenericType(interfaceType);
            var ctor = wrapperTypeInstance.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
                                                          null,
                                                          new Type[] { interfaceType },
                                                          null);
            var ctorFuncParam = Expression.Parameter(typeof(object));
            var ctorFunc = Expression.Lambda<Func<object, RuntimeContainerWrapper>>(
                Expression.Convert(Expression.New(ctor, Expression.Convert(ctorFuncParam, interfaceType)),
                                   typeof(RuntimeContainerWrapper)),
                ctorFuncParam).Compile();
            return ctorFunc;
        }


        object IServiceProvider.GetService(Type serviceType)
        {
            return GetInstance(serviceType);
        }
    }

    public class RuntimeContainerWrapper<T> : RuntimeContainerWrapper
        where T : class
    {
        private static readonly Func<T, Type, object> resolveByTypeMethod;
        private readonly T container;


        static RuntimeContainerWrapper()
        {
            if (typeof(IServiceProvider).IsAssignableFrom(typeof(T)))
                resolveByTypeMethod = (c, t) => ((IServiceProvider)c).GetService(t);
            else
            {
                resolveByTypeMethod = GetMethodWithSignature<Func<T, Type, object>>("GetInstance",
                                                                                    "Resolve",
                                                                                    "Get",
                                                                                    "GetService");
            }
        }


        internal RuntimeContainerWrapper(T container)
        {
            this.container = container;
        }


        public override object GetInstance(Type serviceType)
        {
            if (resolveByTypeMethod == null)
            {
                throw new InvalidOperationException("I don't know how to call IoC container of type "
                                                    + typeof(T).FullName + " to get instance by type.");
            }

            return resolveByTypeMethod(this.container, serviceType);
        }


        private static TDel GetExtensionMethodWithSignature<TDel>(string[] validNames)
        {
            var containerAssembly = typeof(T).Assembly;
            var delSignature = typeof(TDel).GetDelegateInvokeMethod();
            var delParams = delSignature.GetParameters();
            var ns = typeof(T).Namespace;

            // Mega linq that will find a matching extension method. Breath calmly.
            var extensionMethod =
                typeof(T).Assembly.GetTypes()
                         .Where(t => t.Namespace == ns && t.IsClass && t.IsSealed && t.IsAbstract && t.IsPublic)
                         .SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public))
                         .Where(x => x.GetCustomAttributes(typeof(ExtensionAttribute), false).Any())
                         .Select(x => new { p = x.GetParameters(), m = x })
                         .Where(x => !x.m.IsGenericMethodDefinition
                                     && x.p.Length == delParams.Length
                                     && x.p[0].ParameterType.Assembly == containerAssembly
                                     && x.p[0].ParameterType.IsAssignableFrom(delParams[0].ParameterType)
                                     && x.p.Select(y => y.ParameterType)
                                         .Skip(1)
                                         .SequenceEqual(delParams.Select(y => y.ParameterType).Skip(1))
                                     && x.m.ReturnType == delSignature.ReturnType)
                         .Select(x => x.m)
                         .FirstOrDefault();

            var exprParams = delSignature.GetParameters().Select(x => Expression.Parameter(x.ParameterType)).ToList();
            return Expression.Lambda<TDel>(Expression.Call(extensionMethod, exprParams), exprParams).Compile();
        }


        private static TDel GetMethodWithSignature<TDel>(params string[] validNames)
        {
            var delSignature = typeof(TDel).GetDelegateInvokeMethod();

            var method =
                typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(
                    x => x.GetParameters().Select(y => y.ParameterType).SequenceEqual(
                        delSignature.GetParameters().Select(y => y.ParameterType).Skip(1))
                         && x.ReturnType == delSignature.ReturnType && validNames.Contains(x.Name));

            if (method == null)
            {
                // Okay then.. lets try to find an extension method..
                return GetExtensionMethodWithSignature<TDel>(validNames);
            }

            var lambdaParams = delSignature.GetParameters().Select(x => Expression.Parameter(x.ParameterType)).ToList();
            return
                Expression.Lambda<TDel>(Expression.Call(lambdaParams[0], method, lambdaParams.Skip(1)), lambdaParams)
                          .Compile();
        }
    }
}