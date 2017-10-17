#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Threading.Tasks;

using Pomona.Common.TypeSystem;

namespace Pomona.RequestProcessing
{
    internal class HandlerMethodTakingPatchedResource : HandlerMethodTakingExistingResource
    {
        public HandlerMethodTakingPatchedResource(HandlerMethod method, ResourceType resourceType)
            : base(method, resourceType)
        {
        }


        protected override async Task<object> OnInvoke(object target, PomonaContext context, object state)
        {
            await context.Bind(await context.Node.GetActualResultTypeAsync(), await context.Node.GetValueAsync());
            return await base.OnInvoke(target, context, state);
        }
    }
}