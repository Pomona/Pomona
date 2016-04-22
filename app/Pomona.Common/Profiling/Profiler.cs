#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Profiling
{
    public abstract class Profiler : IProfilerImplementation
    {
        static Profiler()
        {
            ProfilerFactory = null;
        }


        public static IProfiler Current => ProfilerFactory != null ? ProfilerFactory() : null;

        public static Func<IProfiler> ProfilerFactory { get; set; }


        public static IDisposable Step(string name = null)
        {
            return Current.Step(name);
        }


        protected abstract IDisposable OnStep(string name);


        IDisposable IProfilerImplementation.Step(string name)
        {
            return OnStep(name);
        }
    }
}