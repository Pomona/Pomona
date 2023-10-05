#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

namespace Pomona.Scheduler
{
    public interface IJob
    {
        DateTime? CompletedOn { get; }
        string Method { get; }
        DateTime ScheduledOn { get; }
        string Url { get; }
    }
}
