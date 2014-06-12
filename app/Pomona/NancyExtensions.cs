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
using System.Linq;
using System.Threading;

using Nancy;

using Pomona.Common.Serialization;
using Pomona.Ioc;

namespace Pomona
{
    internal static class NancyExtensions
    {
        private static string cachedIocContainerKey = null;


        internal static void ContentsFromString(this Response resp, string text)
        {
            resp.Contents = stream =>
            {
                using (var writer = new NoCloseStreamWriter(stream))
                {
                    writer.Write(text);
                    writer.Flush();
                }
                stream.Flush();

                Thread.MemoryBarrier();
            };
        }


        internal static ISerializationContextProvider GetSerializationContextProvider(this NancyContext nancyContext)
        {
            return (ISerializationContextProvider)nancyContext.Items[typeof(ISerializationContextProvider).FullName];
        }


        internal static IUriResolver GetUriResolver(this NancyContext nancyContext)
        {
            return (IUriResolver)nancyContext.Items[typeof(IUriResolver).FullName];
        }


        internal static RuntimeContainerWrapper GetIocContainerWrapper(this NancyContext context)
        {
            object childContainer;
            var childContainerSuffix = "BootstrapperChildContainer";
            if (cachedIocContainerKey == null || !context.Items.TryGetValue(cachedIocContainerKey, out childContainer))
            {
                var item =
                    context.Items.Where(x => x.Key.EndsWith(childContainerSuffix)).OrderByDescending(
                        x =>
                            RuntimeContainerWrapper.PreferredContainersTypes.Contains(
                                x.Key.Substring(x.Key.Length - childContainerSuffix.Length))).First();
                cachedIocContainerKey = item.Key;
                childContainer = item.Value;
            }
            return RuntimeContainerWrapper.Create(childContainer);

        }

        internal static object Resolve(this NancyContext context, Type type)
        {
            if (type == typeof(NancyContext))
                return context;

            return context.GetIocContainerWrapper().GetInstance(type);
        }
    }
}