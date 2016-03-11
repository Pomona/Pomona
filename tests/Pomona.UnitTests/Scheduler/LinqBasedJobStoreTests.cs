#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
            private readonly List<Job> jobs = new List<Job>();

            public List<Job> Jobs
            {
                get { return this.jobs; }
            }

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
                this.jobs.Add(job);
                return job;
            }


            protected override IQueryable<Job> OnGetQueryable()
            {
                return this.jobs.AsQueryable();
            }


            protected override DateTime OnGetUtcNow()
            {
                return UtcNow;
            }
        }
    }
}