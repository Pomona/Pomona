#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Net.Http;

using Pomona.Common.Web;

namespace Pomona.Scheduler
{
    public class JobDispatcher : IJobDispatcher
    {
        private readonly IJobStore jobStore;
        private readonly IWebClient webClient;


        public JobDispatcher(IJobStore jobStore)
        {
            if (jobStore == null)
                throw new ArgumentNullException(nameof(jobStore));
            this.jobStore = jobStore;
            this.webClient = new HttpWebClient();
        }


        private void RunJob(IJob job)
        {
            var response = this.webClient.SendSync(new HttpRequestMessage(new HttpMethod(job.Method), job.Url));
            var statusCode = (int)response.StatusCode;
            if (statusCode - (statusCode % 100) != 200)
                throw new NotImplementedException("TODO: Implement error handling and retrying.");

            this.jobStore.Complete(job);
        }


        public bool Tick()
        {
            IJob job;
            if (this.jobStore.TryDequeue(out job))
            {
                RunJob(job);
                return true;
            }
            return false;
        }
    }
}