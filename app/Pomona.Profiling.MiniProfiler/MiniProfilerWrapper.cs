#region License

// --------------------------------------------------
// Copyright © OKB. All Rights Reserved.
// 
// This software is proprietary information of OKB.
// USE IS SUBJECT TO LICENSE TERMS.
// --------------------------------------------------

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
            return this.wrapped.Step(name);
        }
    }
}