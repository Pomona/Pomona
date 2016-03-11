#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;

namespace Pomona.RequestProcessing
{
    public interface IRequestProcessorPipeline : IPomonaRequestProcessor
    {
        IEnumerable<IPomonaRequestProcessor> After { get; }
        IEnumerable<IPomonaRequestProcessor> Before { get; }
    }
}