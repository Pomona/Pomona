#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Threading.Tasks;

namespace Pomona.Routing
{
    internal class AsyncDelegateRouteAction : RouteAction
    {
        private readonly Func<PomonaContext, bool> condition;
        private readonly Func<PomonaContext, Task<PomonaResponse>> func;


        public AsyncDelegateRouteAction(Func<PomonaContext, Task<PomonaResponse>> func, Func<PomonaContext, bool> condition = null)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            this.func = func;
            this.condition = condition;
        }


        public override bool CanProcess(PomonaContext context)
        {
            return this.condition == null || this.condition(context);
        }


        public override Task<PomonaResponse> Process(PomonaContext context)
        {
            return this.func(context);
        }
    }
}