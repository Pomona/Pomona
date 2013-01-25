#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using System.IO;
using Mono.Cecil;
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
            var assemblyName = typeName + "Proxy" + Guid.NewGuid().ToString();
            var assembly =
                AssemblyDefinition.CreateAssembly(
                    new AssemblyNameDefinition(assemblyName, new Version(1, 0)), assemblyName, ModuleKind.Dll);

            var module = assembly.MainModule;

            var builder = new WrappedPropertyProxyBuilder(
                module, module.Import(typeof (TProxyBase)), module.Import(typeof (PropertyWrapper<,>)).Resolve());

            var typeDef = builder.CreateProxyType(typeName, module.Import(type).Resolve().WrapAsEnumerable());

            byte[] assemblyBlob;

            using (var memoryStream = new MemoryStream())
            {
                assembly.Write(memoryStream);
                assemblyBlob = memoryStream.ToArray();
            }

            var loadedAssembly = AppDomain.CurrentDomain.Load(assemblyBlob);
            proxyType = loadedAssembly.GetType(typeDef.FullName);
        }


        public static T Create()
        {
            return (T) Activator.CreateInstance(proxyType);
        }
    }
}