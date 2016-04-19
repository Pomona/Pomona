#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Common.Internals
{
    public abstract class GenericInvokerBase
    {
        internal abstract Type[] InArgs { get; }
        protected abstract Delegate OnGetDelegate(Type[] typeArgs);
    }
}