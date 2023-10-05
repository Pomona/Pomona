#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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

