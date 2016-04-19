#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Scheduler
{
    public class Job : IJob
    {
        private DateTime? completedOn;
        private string method;
        private DateTime scheduledOn;
        // First minimal version (only supports GET, expects 2?? response)
        private string url;


        protected Job()
        {
        }


        public Job(string url, DateTime scheduledOn, string method)
        {
            this.method = method;
            this.scheduledOn = scheduledOn;
            this.url = url;
        }


        public void Complete(DateTime utcNow)
        {
            this.completedOn = utcNow;
        }


        public virtual DateTime? CompletedOn
        {
            get { return this.completedOn; }
            protected internal set { this.completedOn = value; }
        }

        public virtual string Method
        {
            get { return this.method; }
            protected set { this.method = value; }
        }

        public virtual DateTime ScheduledOn
        {
            get { return this.scheduledOn; }
            protected set { this.scheduledOn = value; }
        }

        public virtual string Url
        {
            get { return this.url; }
            protected set { this.url = value; }
        }
    }
}