#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Scheduler
{
    public class JobDetails
    {
        public JobDetails(string url, DateTime scheduledOn, string method = null)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            Url = url;
            Method = method ?? "GET";
            ScheduledOn = scheduledOn;
        }


        public string Method { get; }

        public DateTime ScheduledOn { get; }

        public string Url { get; }
    }
}