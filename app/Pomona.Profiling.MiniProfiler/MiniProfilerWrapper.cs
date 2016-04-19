#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using StackExchange.Profiling;

namespace Pomona.Profiling
{
    public class MiniProfilerWrapper : Profiler
    {
        private readonly MiniProfiler wrapped;


        public MiniProfilerWrapper(MiniProfiler wrapped)
        {
            this.wrapped = wrapped;
        }


        protected override IDisposable OnStep(string name)
        {
            return MiniProfilerExtensions.Step(this.wrapped, name);
        }
    }
}