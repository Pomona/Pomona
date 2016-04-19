#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Reflection;

using Pomona.Common.Proxies;

namespace Pomona.Common.Serialization.Patch
{
    public class RepositoryDeltaProxyBase<TElement, TRepository> : CollectionDelta<TElement>, IDelta<TRepository>
    {
        protected RepositoryDeltaProxyBase()
            : base()
        {
        }


        protected virtual TPropType OnGet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property)
        {
            return property.Get((TOwner)base.Original);
        }


        protected virtual object OnInvokeMethod(MethodInfo methodInfo, object[] args)
        {
            if (methodInfo.Name == "Post" && args.Length == 1)
            {
                Add((TElement)args[0]);
                return null;
            }
            if (methodInfo.Name == "Delete" && args.Length == 1)
            {
                Remove((TElement)args[0]);
                return null;
            }
            throw new NotImplementedException();
        }


        protected virtual void OnSet<TOwner, TPropType>(PropertyWrapper<TOwner, TPropType> property, TPropType value)
        {
            throw new NotSupportedException("Setting property " + property.Name
                                            + " is not supported through delta proxy.");
        }


        public new TRepository Original => (TRepository)base.Original;
    }
}