#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

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

        #region Operators

        public static implicit operator RouteAction(Func<PomonaContext, PomonaResponse> func)
        {
            if (func == null)
                return null;
            return Create(func);
        }

        #endregion

        public abstract PomonaResponse Process(PomonaContext context);
    }
}
