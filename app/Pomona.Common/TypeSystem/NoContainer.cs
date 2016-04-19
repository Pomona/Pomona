#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Common.TypeSystem
{
    internal class NoContainer : IContainer
    {
        public T GetInstance<T>()
        {
            throw new InvalidOperationException("No context available.");
        }
    }
}