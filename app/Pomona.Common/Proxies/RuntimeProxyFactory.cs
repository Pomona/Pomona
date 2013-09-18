#region License

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

#endregion

using System;
using System.Reflection;
using System.Reflection.Emit;
using Pomona.Common.Internals;

namespace Pomona.Common.Proxies
{
    public static class RuntimeProxyFactory<TProxyBase, T>
    {
        private static readonly Type proxyType;


        static RuntimeProxyFactory()
        {
            var type = typeof (T);
            var typeName = type.Name;
            var assemblyNameString = typeName + "Proxy" + Guid.NewGuid().ToString();
            var assemblyName = new AssemblyName(assemblyNameString);

            var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var modBuilder = asmBuilder.DefineDynamicModule(assemblyNameString, false);

            var proxyBaseType = typeof (TProxyBase);
            var proxyBuilder = new WrappedPropertyProxyBuilder(modBuilder, proxyBaseType,
                                                               typeof (PropertyWrapper<,>),
                                                               typeNameFormat: "{0}_" + proxyBaseType.Name,
                                                               proxyNamespace: proxyBaseType.Namespace);

            var typeDef = proxyBuilder.CreateProxyType(typeName, type.WrapAsEnumerable());

            proxyType = typeDef.CreateType();
        }


        public static T Create()
        {
            return (T)Activator.CreateInstance(proxyType);
        }
    }
}