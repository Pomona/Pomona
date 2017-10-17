#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Threading.Tasks;

using Pomona.Common.TypeSystem;

namespace Pomona.RequestProcessing
{
    internal class HandlerMethodTakingExistingResource : HandlerMethodInvoker<object>
    {
        private readonly HandlerParameter resourceParameter;


        public HandlerMethodTakingExistingResource(HandlerMethod method, ResourceType resourceType)
            : base(method)
        {
            if (resourceType == null)
                throw new ArgumentNullException(nameof(resourceType));
            this.resourceParameter =
                method.Parameters.LastOrDefault(x => x.Type == resourceType) ??
                method.Parameters.LastOrDefault(x => x.IsResource && x.Type.IsAssignableFrom(resourceType));

            if (this.resourceParameter == null)
                throw new ArgumentException("Method has no argument accepting resource type.", nameof(method));
        }


        protected override async Task<object> OnGetArgument(HandlerParameter parameter, PomonaContext context, object state)
        {
            if (parameter == this.resourceParameter)
                return await context.Node.GetValueAsync();
            return await base.OnGetArgument(parameter, context, state);
        }
    }
}