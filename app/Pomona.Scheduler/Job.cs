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