#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

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