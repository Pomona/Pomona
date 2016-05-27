#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Threading.Tasks;

namespace Pomona.Routing
{
    public abstract class RouteAction : IPomonaRequestProcessor
    {
        public abstract bool CanProcess(PomonaContext context);


        public static RouteAction Create(Func<PomonaContext, PomonaResponse> func,
                                         Func<PomonaContext, bool> condition = null)
        {
            return new DelegateRouteAction(func, condition);
        }


        public static RouteAction Create(Func<PomonaContext, Task<PomonaResponse>> func,
                                         Func<PomonaContext, bool> condition = null)
        {
            return new AsyncDelegateRouteAction(func, condition);
        }


        public abstract Task<PomonaResponse> Process(PomonaContext context);


        Task<PomonaResponse> IPomonaRequestProcessor.Process(PomonaContext context)
        {
            return Process(context);
        }

        #region Operators

        public static implicit operator RouteAction(Func<PomonaContext, PomonaResponse> func)
        {
            if (func == null)
                return null;
            return Create(func);
        }


        public static implicit operator RouteAction(Func<PomonaContext, Task<PomonaResponse>> func)
        {
            if (func == null)
                return null;
            return Create(func);
        }

        #endregion
    }
}