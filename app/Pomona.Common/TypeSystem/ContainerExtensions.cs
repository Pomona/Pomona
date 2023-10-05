#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

using Pomona.Common.Internals;

namespace Pomona.Common.TypeSystem
{
    public static class ContainerExtensions
    {
        private static readonly Func<Type, IContainer, object> getInstanceMethod =
            GenericInvoker.Instance<IContainer>().CreateFunc1<object>(x => x.GetInstance<object>());


        public static object GetInstance(this IContainer container, Type type)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            return getInstanceMethod(type, container);
        }
    }
}

