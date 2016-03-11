#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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