#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

namespace Pomona.Scheduler
{
    public interface IJobStore : IJobScheduler
    {
        IJob Complete(IJob job);
        bool TryDequeue(out IJob job);
    }
}
