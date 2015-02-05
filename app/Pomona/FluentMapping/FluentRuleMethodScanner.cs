#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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

using Pomona.Common;
using Pomona.Common.Internals;

namespace Pomona.FluentMapping
{
    internal class FluentRuleMethodScanner
    {
        internal static IEnumerable<FluentRuleMethod> Scan(IEnumerable<object> fluentRuleObjects,
                                                           IEnumerable<Delegate> fluentRuleDelegates)
        {
            return
                GetMappingRulesFromObjects(fluentRuleObjects).Concat(GetMappingRulesFromDelegates(fluentRuleDelegates))
                    .Flatten
                    (x => x.GetChildRules()).ToList();
        }


        private static IEnumerable<FluentRuleMethod> GetMappingRulesFromDelegates(IEnumerable<Delegate> mapDelegates)
        {
            return mapDelegates == null
                ? Enumerable.Empty<FluentRuleMethod>()
                : mapDelegates.Where(x => IsRuleMethod(x.Method)).Select(x => new FluentRuleMethod(x.Method, x.Target));
        }


        private static IEnumerable<FluentRuleMethod> GetMappingRulesFromObjects(IEnumerable<object> ruleContainers)
        {
            if (ruleContainers == null)
                return Enumerable.Empty<FluentRuleMethod>();
            return ruleContainers
                .SelectMany(x => x.GetType()
                                .GetMethods()
                                .Where(IsRuleMethod)
                                .Select(m => new FluentRuleMethod(m, x)));
        }


        private static bool IsRuleMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != 1)
                return false;
            var paramType = parameters[0].ParameterType;

            // Metadata token is the same across all generic type instances and generic type definition
            return paramType.UniqueToken() == typeof(ITypeMappingConfigurator<>).UniqueToken();
        }
    }
}