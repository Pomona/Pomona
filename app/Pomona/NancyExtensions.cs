#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Nancy;

using Pomona.Common;
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
                using (var writer = new NonClosingStreamWriter(stream))
                {
                    writer.Write(text);
                    writer.Flush();
                }
                stream.Flush();

                Thread.MemoryBarrier();
            };
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


        internal static IPomonaSession GetPomonaSession(this NancyContext nancyContext)
        {
            if (nancyContext == null)
                throw new ArgumentNullException(nameof(nancyContext));
            IPomonaSession instance;
            var key = typeof(IPomonaSession).FullName;
            if (!nancyContext.Items.TryGetValueAsType(key, out instance))
                throw new KeyNotFoundException($"Unable to locate item {key} in {nameof(NancyContext)} items");
            return instance;
        }


        internal static object Resolve(this NancyContext context, Type type)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (type == typeof(NancyContext))
                return context;
            return context.GetIocContainerWrapper().GetInstance(type);
        }


        internal static void SetPomonaSession(this NancyContext nancyContext, IPomonaSession pomonaSession)
        {
            if (nancyContext == null)
                throw new ArgumentNullException(nameof(nancyContext));
            nancyContext.Items[typeof(IPomonaSession).FullName] = pomonaSession;
        }
    }
}