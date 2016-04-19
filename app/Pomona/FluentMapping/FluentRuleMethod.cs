#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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


        public FluentRuleMethod(MethodInfo method, object instance)
        {
            AppliesToType = method.GetParameters()[0].ParameterType.GetGenericArguments()[0];
            Method = method;
            Instance = instance;
        }


        public Type AppliesToType { get; }

        public object Instance { get; }

        public MethodInfo Method { get; }


        public IEnumerable<FluentRuleMethod> GetChildRules()
        {
            return
                (IEnumerable<FluentRuleMethod>)getChildRulesMethod.MakeGenericMethod(AppliesToType).Invoke(this, null);
        }


        public override string ToString()
        {
            var declaringType = Method.DeclaringType;
            return String.Format("{1}.{2} for {0}", AppliesToType.Name,
                                 declaringType != null ? declaringType.Name : "?", Method.Name);
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