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
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

using Pomona.Common.Proxies;

namespace Pomona.Common.ExtendedResources
{
    public static class ExtendedResourceExtensions
    {
        private static readonly ConcurrentDictionary<Assembly, ClientTypeMapper> assemblyTypeMapperDict =
            new ConcurrentDictionary<Assembly, ClientTypeMapper>();


        public static TOriginal Unwrap<TOriginal>(this IClientResource wrapped)
            where TOriginal : class, IClientResource
        {
            var er = wrapped as IExtendedResourceProxy;
            if (er == null)
                throw new ArgumentException("Can only unwrap a wrapped resource.", "wrapped");
            var unwrapped = er.WrappedResource as TOriginal;
            if (unwrapped == null)
                throw new ArgumentException("Unable to unwrap to type " + typeof(TOriginal), "wrapped");
            return unwrapped;
        }


        public static TExtended Wrap<TOriginal, TExtended>(this TOriginal resource)
            where TOriginal : IClientResource
            where TExtended : TOriginal, IClientResource
        {
            var typeMapper = GetTypeMapper(typeof(TOriginal));
            return (TExtended)typeMapper.WrapResource(resource, typeof(TOriginal), typeof(TExtended));
        }


        private static ClientTypeMapper GetTypeMapper(Type type)
        {
            var assembly = type.Assembly;
            return assemblyTypeMapperDict.GetOrAdd(assembly, GetTypeMapperFromAssembly);
        }


        private static ClientTypeMapper GetTypeMapperFromAssembly(Assembly assembly)
        {
            var generatedClientInterface =
                assembly.GetTypes().Single(
                    x => x.IsInterface && typeof(IPomonaClient).IsAssignableFrom(x) && x != typeof(IPomonaClient));
            var clientBaseType = typeof(ClientBase<>).MakeGenericType(generatedClientInterface);
            var clientTypeMapper =
                clientBaseType.GetProperty("ClientTypeMapper", BindingFlags.NonPublic | BindingFlags.Static).GetValue(
                    null,
                    null);
            return (ClientTypeMapper)clientTypeMapper;
        }
    }
}