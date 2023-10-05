#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;
using System.Linq;

namespace Pomona.RequestProcessing
{
    public class DefaultRequestProcessorPipeline : IRequestProcessorPipeline
    {
        public virtual IEnumerable<IPomonaRequestProcessor> After
        {
            get { yield break; }
        }

        public virtual IEnumerable<IPomonaRequestProcessor> Before
        {
            get { yield return new ValidateEtagProcessor(); }
        }


        public virtual PomonaResponse Process(PomonaContext context)
        {
            var routeActions = context.Session.GetRouteActions(context).ToList();
            return Before
                .Concat(routeActions.Where(x => x.CanProcess(context)))
                .Concat(After)
                .Where(x => x != null)
                .Select(x => x.Process(context))
                .FirstOrDefault(response => response != null);
        }
    }
}
