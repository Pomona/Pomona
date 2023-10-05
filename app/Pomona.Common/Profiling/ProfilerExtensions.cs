#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

namespace Pomona.Profiling
{
    public static class ProfilerExtensions
    {
#if false
        public static IDisposable Profile(this object profiled,
            string subSectionName = null,
            [CallerMemberName] string methodName = "")
        {
            if (profiled == null)
                throw new ArgumentNullException("profiled");
            var profiler = Profiler.Current;
            if (profiler == null)
                return null;

            var profiledType = profiled.GetType();
            var name = subSectionName != null
                           ? string.Format("{0}:{1}.{2}", profiledType.Name, methodName, subSectionName)
                           : string.Format("{0}:{1}", profiledType.Name, methodName);
            return profiler.Step(name);
        }
#endif


        public static IDisposable Step(this IProfiler profiler, string methodName = "")
        {
            var profImpl = profiler as IProfilerImplementation;
            if (profImpl == null)
                return null;
            return profImpl.Step(methodName);
        }
    }
}

