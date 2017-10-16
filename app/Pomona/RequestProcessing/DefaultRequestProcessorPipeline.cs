#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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


        public virtual async Task<PomonaResponse> Process(PomonaContext context)
        {
            var routeActions = context.Session.GetRouteActions(context).ToList();
            var chain = Before
                .Concat(routeActions.Where(x => x.CanProcess(context)))
                .Concat(After)
                .Where(x => x != null);

            foreach (var processor in chain)
            {
                var response = await processor.Process(context);
                if (response != null)
                    return response;
            }
            return null;
        }
    }
}