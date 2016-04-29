#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Threading.Tasks;

namespace Pomona.Routing
{
    internal class DelegateRouteAction : RouteAction
    {
        private readonly Func<PomonaContext, bool> condition;
        private readonly Func<PomonaContext, PomonaResponse> func;


        public DelegateRouteAction(Func<PomonaContext, PomonaResponse> func, Func<PomonaContext, bool> condition = null)
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
            return Task.FromResult(this.func(context));
        }
    }
}