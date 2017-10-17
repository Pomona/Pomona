#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Threading.Tasks;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.RequestProcessing
{
    internal class HandlerMethodTakingResourceId : HandlerMethodInvoker<object>
    {
        public HandlerMethodTakingResourceId(HandlerMethod method)
            : base(method)
        {
        }


        protected override Task<object> OnGetArgument(HandlerParameter parameter, PomonaContext context, object state)
        {
            var node = context.Node;
            var resourceResultType = node.Route.ResultItemType as ResourceType;
            if (resourceResultType != null)
            {
                var primaryIdType = resourceResultType.PrimaryId.PropertyType;
                if (parameter.Type == primaryIdType)
                {
                    object parsedId;
                    if (!node.PathSegment.TryParse(primaryIdType, out parsedId))
                        throw new HandlerMethodInvocationException(context, this, "Unable to parse id from url segment");
                    return Task.FromResult(parsedId);
                }
            }
            return base.OnGetArgument(parameter, context, state);
        }
    }
}