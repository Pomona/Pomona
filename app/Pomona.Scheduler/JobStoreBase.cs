#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Scheduler
{
    public abstract class JobStoreBase<TJob> : IJobStore
        where TJob : class, IJob
    {
        public abstract TJob Complete(TJob job);
        public abstract TJob Queue(JobDetails jobDetails);
        public abstract bool TryDequeue(out TJob job);


        IJob IJobStore.Complete(IJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));
            var castJob = job as TJob;
            if (castJob == null)
                throw new ArgumentException("Job must be of type " + typeof(TJob).FullName, nameof(job));
            return Complete(castJob);
        }


        IJob IJobScheduler.Queue(JobDetails jobDetails)
        {
            if (jobDetails == null)
                throw new ArgumentNullException(nameof(jobDetails));
            return Queue(jobDetails);
        }


        bool IJobStore.TryDequeue(out IJob job)
        {
            TJob castJob = null;
            try
            {
                return TryDequeue(out castJob);
            }
            finally
            {
                job = castJob;
            }
        }
    }
}