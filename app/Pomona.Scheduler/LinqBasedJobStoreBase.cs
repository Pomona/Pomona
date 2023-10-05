#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq;

namespace Pomona.Scheduler
{
    public abstract class LinqBasedJobStoreBase<TJob> : JobStoreBase<TJob>
        where TJob : class, IJob
    {
        public override bool TryDequeue(out TJob job)
        {
            var utcNow = OnGetUtcNow();
            job = OnGetQueryable()
                .Where(x => x.ScheduledOn < utcNow && x.CompletedOn == null)
                .OrderBy(x => x.ScheduledOn)
                .FirstOrDefault();
            return job != null;
        }


        protected abstract IQueryable<TJob> OnGetQueryable();


        protected virtual DateTime OnGetUtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}

