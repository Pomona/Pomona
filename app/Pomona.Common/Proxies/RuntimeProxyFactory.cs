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
using System.Reflection;

using Pomona.Common.Internals;
#if !DISABLE_PROXY_GENERATION
using System.Reflection.Emit;
#endif

namespace Pomona.Common.Proxies
{
    public static class RuntimeProxyFactory
    {
        private static readonly MethodInfo createMethod =
            ReflectionHelper.GetMethodDefinition<object>(o => Create<object, object>());


        public static T Create<TProxyBase, T>()
        {
            return RuntimeProxyFactory<TProxyBase, T>.Create();
        }


        public static object Create(Type proxyBase, Type proxyTarget)
        {
            return createMethod.MakeGenericMethod(proxyBase, proxyTarget).Invoke(null, null);
        }
    }

    public static class RuntimeProxyFactory<TProxyBase, T>
    {
#if !DISABLE_PROXY_GENERATION
        private static readonly Type proxyType;
#endif


        static RuntimeProxyFactory()
        {
#if DISABLE_PROXY_GENERATION
            throw new NotSupportedException("Proxy generation has been disabled compile-time using DISABLE_PROXY_GENERATION, which makes this method not supported.");
#else

            var type = typeof(T);
            var typeName = type.Name;
            var assemblyNameString = typeName + "Proxy" + Guid.NewGuid().ToString();
            AssemblyBuilder asmBuilder;
            var modBuilder =
                EmitHelpers.CreateRuntimeModule(new AssemblyName(assemblyNameString) { Version = type.Assembly.GetName().Version },
                                                out asmBuilder);

            var proxyBaseType = typeof(TProxyBase);
            var proxyBuilder = new WrappedPropertyProxyBuilder(modBuilder, proxyBaseType,
                                                               typeof(PropertyWrapper<,>),
                                                               typeNameFormat : "{0}_" + proxyBaseType.Name,
                                                               proxyNamespace : proxyBaseType.Namespace);

            var typeDef = proxyBuilder.CreateProxyType(typeName, type.WrapAsEnumerable());

            proxyType = typeDef.CreateType();
#endif
        }


        public static T Create()
        {
#if DISABLE_PROXY_GENERATION
            throw new NotSupportedException("Proxy generation has been disabled compile-time using DISABLE_PROXY_GENERATION, which makes this method not supported.");
#else
            return (T)Activator.CreateInstance(proxyType);
#endif
        }
    }
}