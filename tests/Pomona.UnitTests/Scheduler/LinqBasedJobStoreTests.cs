#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Pomona.Scheduler;

namespace Pomona.UnitTests.Scheduler
{
    [TestFixture]
    public class LinqBasedJobStoreTests
    {
        [Test]
        public void TryDequeue_NoExpiredJobsInQueue_ReturnsFalse()
        {
            var jobStore = new TestJobStore() { UtcNow = new DateTime(2011, 1, 1, 1, 1, 1, DateTimeKind.Utc) };
            jobStore.Jobs.Add(new Job("http://jalla", jobStore.UtcNow.AddDays(3), "GET"));
            Job job;
            Assert.That(jobStore.TryDequeue(out job), Is.False);
            Assert.That(job, Is.Null);
        }


        [Test]
        public void TryDequeue_TwoExpiredJobsInQueue_DequeuesOldestJob()
        {
            var jobStore = new TestJobStore() { UtcNow = new DateTime(2011, 1, 1, 1, 1, 1, DateTimeKind.Utc) };
            var oldJob = new Job("http://stale", jobStore.UtcNow.AddDays(-100), "GET");
            jobStore.Jobs.Add(oldJob);
            var newJob = new Job("http://fresh", jobStore.UtcNow.AddDays(-1), "GET");
            jobStore.Jobs.Add(newJob);
            Job job;
            Assert.That(jobStore.TryDequeue(out job), Is.True);
            Assert.That(job, Is.EqualTo(oldJob));
        }


        [Test]
        public void TryDequeue_ZeroJobsQueued_ReturnsFalse()
        {
            var jobStore = new TestJobStore();
            Job job;
            Assert.That(jobStore.TryDequeue(out job), Is.False);
            Assert.That(job, Is.Null);
        }


        public class TestJobStore : LinqBasedJobStoreBase<Job>
        {
            public List<Job> Jobs { get; } = new List<Job>();

            public DateTime UtcNow { get; set; }


            public override Job Complete(Job job)
            {
                if (job == null)
                    throw new ArgumentNullException(nameof(job));
                if (job.CompletedOn.HasValue)
                    throw new ArgumentException("Job has already been completed.", nameof(job));
                job.Complete(OnGetUtcNow());
                return job;
            }


            public override Job Queue(JobDetails jobDetails)
            {
                if (jobDetails == null)
                    throw new ArgumentNullException(nameof(jobDetails));
                var job = new Job(jobDetails.Url, jobDetails.ScheduledOn, jobDetails.Method);
                Jobs.Add(job);
                return job;
            }


            protected override IQueryable<Job> OnGetQueryable()
            {
                return Jobs.AsQueryable();
            }


            protected override DateTime OnGetUtcNow()
            {
                return UtcNow;
            }
        }
    }
}
