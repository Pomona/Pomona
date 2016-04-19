#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Threading;

using Nancy;

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
            return (IPomonaSession)nancyContext.Items[typeof(IPomonaSession).FullName];
        }


        internal static IUriResolver GetUriResolver(this NancyContext nancyContext)
        {
            return (IUriResolver)nancyContext.Items[typeof(IUriResolver).FullName];
        }


        internal static object Resolve(this NancyContext context, Type type)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (type == typeof(NancyContext))
                return context;
            if (type == typeof(IUriResolver))
                return context.GetUriResolver();

            return context.GetIocContainerWrapper().GetInstance(type);
        }
    }
}