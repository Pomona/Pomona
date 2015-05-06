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
                throw new ArgumentNullException("job");
            var castJob = job as TJob;
            if (castJob == null)
                throw new ArgumentException("Job must be of type " + typeof(TJob).FullName, "job");
            return Complete(castJob);
        }


        IJob IJobScheduler.Queue(JobDetails jobDetails)
        {
            if (jobDetails == null)
                throw new ArgumentNullException("jobDetails");
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