#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Nancy;

using Pomona.Common.TypeSystem;

namespace Pomona.Nancy
{
    internal class ServerContainer : IContainer
    {
        public ServerContainer(NancyContext nancyContext)
        {
            if (nancyContext == null)
                throw new ArgumentNullException(nameof(nancyContext));
            NancyContext = nancyContext;
        }


        public NancyContext NancyContext { get; }


        public virtual T GetInstance<T>()
        {
            return (T)NancyContext.Resolve(typeof(T));
        }
    }
}