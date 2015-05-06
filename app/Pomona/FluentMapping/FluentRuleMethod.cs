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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Pomona.Common.Internals;

namespace Pomona.FluentMapping
{
    internal class FluentRuleMethod
    {
        public static readonly MethodInfo getChildRulesMethod =
            ReflectionHelper.GetMethodDefinition<FluentRuleMethod>(x => x.GetChildRules<object>());

        private readonly Type appliesToType;
        private readonly object instance;
        private readonly MethodInfo method;


        public FluentRuleMethod(MethodInfo method, object instance)
        {
            this.appliesToType = method.GetParameters()[0].ParameterType.GetGenericArguments()[0];
            this.method = method;
            this.instance = instance;
        }


        public Type AppliesToType
        {
            get { return this.appliesToType; }
        }

        public object Instance
        {
            get { return this.instance; }
        }

        public MethodInfo Method
        {
            get { return this.method; }
        }


        public IEnumerable<FluentRuleMethod> GetChildRules()
        {
            return
                (IEnumerable<FluentRuleMethod>)getChildRulesMethod.MakeGenericMethod(AppliesToType).Invoke(this, null);
        }


        public override string ToString()
        {
            var declaringType = this.method.DeclaringType;
            return String.Format("{1}.{2} for {0}", this.appliesToType.Name,
                                 declaringType != null ? declaringType.Name : "?", this.method.Name);
        }


        private IEnumerable<FluentRuleMethod> GetChildRules<T>()
        {
            var typeConfigDelegates = new List<Delegate>();
            var nestedScanner = new NestedTypeMappingConfigurator<T>(typeConfigDelegates);
            Method.Invoke(Instance, new object[] { nestedScanner });
            return
                typeConfigDelegates.Select<Delegate, FluentRuleMethod>(x => new FluentRuleMethod(x.Method, x.Target))
                                   .ToList<FluentRuleMethod>();
        }
    }
}