#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Scheduler
{
    public static class JobStoreExtensions
    {
        public static IJob Create(this IJobStore jobStore, string url, DateTime scheduledOn, string method = null)
        {
            if (jobStore == null)
                throw new ArgumentNullException(nameof(jobStore));
            return jobStore.Queue(new JobDetails(url, scheduledOn, method));
        }
    }
}