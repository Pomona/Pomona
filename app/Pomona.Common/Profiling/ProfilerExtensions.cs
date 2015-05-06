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