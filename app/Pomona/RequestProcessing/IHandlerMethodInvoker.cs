#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Threading.Tasks;

namespace Pomona.RequestProcessing
{
    public interface IHandlerMethodInvoker
    {
        Type ReturnType { get; }
        Task<object> Invoke(object target, PomonaContext context);
    }
}