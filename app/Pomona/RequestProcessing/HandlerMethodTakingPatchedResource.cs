#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.Common.TypeSystem;

namespace Pomona.RequestProcessing
{
    internal class HandlerMethodTakingPatchedResource : HandlerMethodTakingExistingResource
    {
        public HandlerMethodTakingPatchedResource(HandlerMethod method, ResourceType resourceType)
            : base(method, resourceType)
        {
        }


        protected override object OnInvoke(object target, PomonaContext context, object state)
        {
            context.Bind(context.Node.ActualResultType, context.Node.Value);
            return base.OnInvoke(target, context, state);
        }
    }
}