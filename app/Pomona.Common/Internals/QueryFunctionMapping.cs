#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Pomona.Common.Internals
{
    public class QueryFunctionMapping
    {
        #region MethodCallStyle enum

        public enum MethodCallStyle
        {
            Chained,
            Static
        }

        #endregion

        public static readonly MethodInfo DictStringStringGetMethod;
        public static readonly MethodInfo EnumerableContainsMethod;
        public static readonly MethodInfo ListContainsMethod;
        public static readonly MethodInfo SafeGetMethod;
        public static readonly MethodInfo StringEqualsTakingComparisonTypeMethod;

        private static readonly Dictionary<UniqueMemberToken, MemberMapping> metadataTokenToMemberMappingDict =
            new Dictionary<UniqueMemberToken, MemberMapping>();

        private static readonly Dictionary<string, List<MemberMapping>> nameToMemberMappingDict =
            new Dictionary<string, List<MemberMapping>>();


        static QueryFunctionMapping()
        {
            DictStringStringGetMethod = ReflectionHelper.GetInstanceMethodInfo<IDictionary<string, string>>(x => x[null]);
            EnumerableContainsMethod =
                ReflectionHelper.GetMethodDefinition<IEnumerable<object>>(x => x.Contains(null));
            ListContainsMethod =
                ReflectionHelper.GetMethodDefinition<List<object>>(x => x.Contains(null));
            SafeGetMethod =
                ReflectionHelper.GetMethodDefinition<IDictionary<object, object>>(x => x.SafeGet(null));
            StringEqualsTakingComparisonTypeMethod =
                ReflectionHelper.GetMethodDefinition<String>(
                    x => string.Equals(x, null, StringComparison.InvariantCulture));

            Add<string>(x => x.Length, "length({0})");
            Add<string>(x => x.StartsWith(null), "startswith({0},{1})");
            Add<string>(x => x.EndsWith(null), "endswith({0},{1})");
            Add<string>(x => x.Contains(null), "substringof({1},{0})");
            Add<string>(x => x.Substring(0), "substring({0},{1})");
            Add<string>(x => x.Substring(0, 0), "substring({0},{1},{2})");
            Add<string>(x => x.Replace("", ""), "replace({0},{1},{2})");
            Add<string>(x => x.Replace('a', 'a'), "replace({0},{1},{2})");
            Add<string>(x => x.ToLower(), "tolower({0})", localExec : true);
            Add<string>(x => x.ToUpper(), "toupper({0})", localExec : true);
            Add<string>(x => x.Trim(), "trim({0})");
            Add<string>(x => x.IndexOf("a"), "indexof({0},{1})");
            Add<string>(x => x.IndexOf('a'), "indexof({0},{1})");
            Add<string>(x => string.Concat("", ""), "concat({0},{1})", localExec : true);
            Add<string>(x => x.Trim(), "trim({0})");

            // TODO: Concat function, this one's static

            Add<DateTime>(x => x.Day, "day({0})");
            Add<DateTime>(x => x.Hour, "hour({0})");
            Add<DateTime>(x => x.Minute, "minute({0})");
            Add<DateTime>(x => x.Month, "month({0})");
            Add<DateTime>(x => x.Second, "second({0})");
            Add<DateTime>(x => x.Year, "year({0})");
            Add<DateTime>(x => x.Date, "date({0})");

            // TODO Math functions, these are static
            Add<double>(x => Math.Sqrt(x), "sqrt({0})");

            // TODO: Multiple overloads working on different types are not yet working as it should.
            Add<double>(x => Math.Round(x), "round({0})");
            Add<decimal>(x => decimal.Round(x), "round({0})");
            Add<double>(x => Math.Floor(x), "floor({0})");
            Add<decimal>(x => decimal.Floor(x), "floor({0})");
            Add<double>(x => Math.Ceiling(x), "ceiling({0})");
            Add<decimal>(x => decimal.Ceiling(x), "ceiling({0})");

            Add<WildcardStructType?>(x => x.HasValue, "hasValue({0})", MethodCallStyle.Chained);
            Add<WildcardStructType?>(x => x.Value, "value({0})", MethodCallStyle.Chained);

            // Custom functions, not odata standard
            Add<IEnumerable<WildcardType>>(x => x.Any(), "any({0})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Any(null), "any({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.All(null), "all({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(
                x => x.Select(y => (WildcardType)null),
                "select({0},{1})",
                MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Where(y => false), "where({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Count(), "count({0})");
            Add<IEnumerable<WildcardType>>(x => x.Count(y => true), "count({0},{1})");
            Add<IEnumerable<WildcardType>>(x => x.SelectMany(y => (IEnumerable<string>)null),
                                           "many({0},{1})",
                                           MethodCallStyle.Chained);
            Add<ICollection<WildcardType>>(x => x.Count, "count({0})");

            Add<IEnumerable<WildcardType>>(x => x.First(), "first({0})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.FirstOrDefault(), "firstdefault({0})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.First(y => true), "first({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.FirstOrDefault(y => true),
                                           "firstdefault({0},{1})",
                                           MethodCallStyle.Chained);

            Add<IEnumerable<WildcardType>>(x => x.Single(), "single({0})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.SingleOrDefault(), "singledefault({0})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Single(y => true), "single({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.SingleOrDefault(y => true),
                                           "singledefault({0},{1})",
                                           MethodCallStyle.Chained);

            Add<IEnumerable<int>>(x => x.Average(), "average({0})");
            Add<IEnumerable<double>>(x => x.Average(), "average({0})");
            Add<IEnumerable<float>>(x => x.Average(), "average({0})");
            Add<IEnumerable<decimal>>(x => x.Average(), "average({0})");
            Add<IEnumerable<int?>>(x => x.Average(), "average({0})");
            Add<IEnumerable<double?>>(x => x.Average(), "average({0})");
            Add<IEnumerable<float?>>(x => x.Average(), "average({0})");
            Add<IEnumerable<decimal?>>(x => x.Average(), "average({0})");

            Add<IEnumerable<WildcardType>>(x => x.Average(y => 10m), "average({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Average(y => 10), "average({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Average(y => 10.0), "average({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Average(y => 10f), "average({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Average(y => (decimal?)10m),
                                           "average({0},{1})",
                                           MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Average(y => (int?)10), "average({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Average(y => (double?)10.0),
                                           "average({0},{1})",
                                           MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Average(y => (float?)10f),
                                           "average({0},{1})",
                                           MethodCallStyle.Chained);

            Add<IEnumerable<string>>(x => string.Join("?", x), "join({1},{0})", MethodCallStyle.Chained);

            Add<IEnumerable<int>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<double>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<float>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<decimal>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<int?>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<double?>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<float?>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<decimal?>>(x => x.Sum(), "sum({0})");

            Add<IEnumerable<WildcardType>>(x => x.Sum(y => 10m), "sum({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Sum(y => 10), "sum({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Sum(y => 10.0), "sum({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Sum(y => 10f), "sum({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Sum(y => (decimal?)10m), "sum({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Sum(y => (int?)10), "sum({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Sum(y => (double?)10.0), "sum({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Sum(y => (float?)10f), "sum({0},{1})", MethodCallStyle.Chained);

            Add<IEnumerable<int>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<decimal>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<double>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<float>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<int?>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<decimal?>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<double?>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<float?>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<DateTime>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<DateTime?>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);

            Add<IEnumerable<WildcardType>>(x => x.Max(y => 10m), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => 10), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => 10.0), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => 10f), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => (decimal?)10m), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => (int?)10), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => (double?)10.0), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => (float?)10f), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => (DateTime?)null), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => default(DateTime)), "max({0},{1})", MethodCallStyle.Chained);

            Add<IEnumerable<int>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<decimal>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<double>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<float>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<int?>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<decimal?>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<double?>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<float?>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<DateTime>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<DateTime?>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);

            Add<IEnumerable<WildcardType>>(x => x.Min(y => 10m), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => 10), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => 10.0), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => 10f), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => (decimal?)10m), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => (int?)10), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => (double?)10.0), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => (float?)10f), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => (DateTime?)null), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => default(DateTime)), "min({0},{1})", MethodCallStyle.Chained);

            Add<IDictionary<WildcardType, WildcardType>>(
                x => x.Contains(null, null),
                "contains({0},{1},{2})",
                MethodCallStyle.Chained);
            Add<IDictionary<WildcardType, WildcardType>>(
                x => x.SafeGet(null),
                "safeget({0},{1})",
                MethodCallStyle.Chained);

            Add<WildcardType>(x => Convert.ChangeType(x, null),
                              "convert({0},{1})",
                              postResolveHook : FixChangeTypeReturnValue);
        }


        public static IEnumerable<MemberMapping> GetMemberCandidates(
            string odataFunctionName,
            int argCount)
        {
            return nameToMemberMappingDict.SafeGet(odataFunctionName + argCount)
                   ?? Enumerable.Empty<MemberMapping>();
        }


        public static bool TryGetMemberMapping(MemberInfo member, out MemberMapping memberMapping)
        {
            return metadataTokenToMemberMappingDict.TryGetValue(member.UniqueToken(), out memberMapping);
        }


        private static void Add<T>(
            Expression<Func<T, object>> expr,
            string functionFormat,
            MethodCallStyle preferredCallStyle = MethodCallStyle.Static,
            Func<Expression, Expression> postResolveHook = null,
            bool localExec = false)
        {
            var memberInfo = ReflectionHelper.GetInstanceMemberInfo(expr);

            var memberMapping = MemberMapping.Parse(memberInfo, functionFormat, preferredCallStyle, postResolveHook, localExec);
            nameToMemberMappingDict.GetOrCreate(memberMapping.Name + memberMapping.ArgumentCount).Add(memberMapping);
            metadataTokenToMemberMappingDict[memberMapping.Member.UniqueToken()] = memberMapping;
        }


        /// <summary>
        /// This function converts result value of Convert.ChangeType to if type argument is a constant.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        private static Expression FixChangeTypeReturnValue(Expression expr)
        {
            var callExpr = (MethodCallExpression)expr;

            var typeArgExpr = callExpr.Arguments[1] as ConstantExpression;
            if (typeArgExpr == null || typeArgExpr.Value == null)
                return expr;

            var convertToType = (Type)typeArgExpr.Value;

            return Expression.Convert(expr, convertToType);
        }


        private static IEnumerable<int> GetArgumentOrder(string formatString)
        {
            var startParenIndex = formatString.IndexOf('(');
            if (startParenIndex == -1)
                yield break;

            var stopParenIndex = formatString.IndexOf(')', startParenIndex + 1);
            if (stopParenIndex == -1)
                yield break;

            var insideParens = formatString.Substring(startParenIndex + 1, stopParenIndex - startParenIndex - 1);

            foreach (var arg in insideParens.Split(','))
            {
                var argTrimmed = arg.Trim('{', '}', ' ');
                yield return int.Parse(argTrimmed);
            }
        }

        #region Nested type: MemberMapping

        public class MemberMapping
        {
            private readonly Func<Expression, Expression> postResolveHookFunction;


            private MemberMapping(
                MemberInfo member,
                string name,
                IList<int> argumentOrder,
                string staticCallFormat,
                string chainedCallFormat,
                MethodCallStyle preferredCallStyle,
                Func<Expression, Expression> postResolveHookFunction,
                bool localExecutionPreferred)
            {
                Member = member;
                Name = name;
                ArgumentOrder = argumentOrder;
                StaticCallFormat = staticCallFormat;
                ChainedCallFormat = chainedCallFormat;
                PreferredCallStyle = preferredCallStyle;
                this.postResolveHookFunction = postResolveHookFunction;
                LocalExecutionPreferred = localExecutionPreferred;
            }


            public int ArgumentCount => ArgumentOrder.Count;

            public IList<int> ArgumentOrder { get; }

            public string ChainedCallFormat { get; }

            public bool LocalExecutionPreferred { get; }

            public MemberInfo Member { get; }

            public string Name { get; }

            public MethodCallStyle PreferredCallStyle { get; }

            public string StaticCallFormat { get; }


            public static MemberMapping Parse(
                MemberInfo member,
                string odataMethodFormat,
                MethodCallStyle preferredCallStyle,
                Func<Expression, Expression> postResolveHookFunction,
                bool localExecutionPreferred)
            {
                var name = odataMethodFormat.Split('(').First();
                var argOrder = GetArgumentOrder(odataMethodFormat);

                var memberAsMethod = member as MethodInfo;
                if (memberAsMethod != null)
                {
                    if (memberAsMethod.IsGenericMethod
                        && memberAsMethod.GetGenericArguments().Any(x => x == typeof(WildcardType)))
                        member = memberAsMethod.GetGenericMethodDefinition();
                }

                if (HasWildcardArgument(member.DeclaringType))
                {
                    var memberLocal = member;
                    member =
                        member.DeclaringType.GetGenericTypeDefinition().GetMembers()
                              .First(x => x.UniqueToken() == memberLocal.UniqueToken());
                }

                var argOrderArray = argOrder.ToArray();
                var extensionMethodFormatString = CreateChainedCallFormatString(name, argOrderArray);
                return new MemberMapping(
                    member,
                    name,
                    argOrderArray,
                    odataMethodFormat,
                    extensionMethodFormatString,
                    preferredCallStyle,
                    postResolveHookFunction,
                    localExecutionPreferred);
            }


            public Expression PostResolveHook(Expression expression)
            {
                return this.postResolveHookFunction != null ? this.postResolveHookFunction(expression) : expression;
            }


            public IList<T> ReorderArguments<T>(IList<T> arguments)
            {
                return new ReorderedList<T>(arguments, ArgumentOrder);
            }


            private static string CreateChainedCallFormatString(string name, IList<int> argumentOrder)
            {
                // Functions can be called in two ways. Either as a stand-alone function like this:
                //    any(items,x:x gt 5)
                // or like this:
                //    items.any(x:x gt 5)
                // The second way has a format string called "extensionMethodFormatString" which we generate in this function.

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("{{{0}}}.", argumentOrder[0]);
                stringBuilder.Append(name);
                stringBuilder.Append('(');
                var firstArgWritten = false;
                for (var i = 1; i < argumentOrder.Count; i++)
                {
                    if (firstArgWritten)
                        stringBuilder.Append(',');
                    else
                        firstArgWritten = true;

                    stringBuilder.Append('{');
                    stringBuilder.Append(argumentOrder[i]);
                    stringBuilder.Append('}');
                }
                stringBuilder.Append(')');
                return stringBuilder.ToString();
            }


            private static bool HasWildcardArgument(Type type)
            {
                if (!type.IsGenericType)
                    return false;

                var genericArguments = type.GetGenericArguments();
                var wildcardType = typeof(WildcardType);
                var wildcardStructType = typeof(WildcardStructType);
                return genericArguments.Any(x => x == wildcardType || x == wildcardStructType)
                       || genericArguments.Any(HasWildcardArgument);
            }
        }

        #endregion

        #region Nested type: ReorderedList

        private class ReorderedList<T> : IList<T>
        {
            private readonly IList<int> order;
            private readonly IList<T> targetList;


            public ReorderedList(IList<T> targetList, IList<int> order)
            {
                if (targetList == null)
                    throw new ArgumentNullException(nameof(targetList));
                if (order == null)
                    throw new ArgumentNullException(nameof(order));
                if (targetList.Count != order.Count)
                    throw new ArgumentException();
                this.targetList = targetList;
                this.order = order;
            }


            public void Add(T item)
            {
                throw new NotSupportedException();
            }


            public void Clear()
            {
                throw new NotSupportedException();
            }


            public bool Contains(T item)
            {
                return this.targetList.Contains(item);
            }


            public void CopyTo(T[] array, int arrayIndex)
            {
                this.ToList().CopyTo(array, arrayIndex);
            }


            public int Count => this.targetList.Count;


            public IEnumerator<T> GetEnumerator()
            {
                for (var i = 0; i < Count; i++)
                    yield return this[i];
            }


            public int IndexOf(T item)
            {
                var index = this.targetList.IndexOf(item);
                if (index != -1)
                    index = this.order.IndexOf(index);
                return index;
            }


            public void Insert(int index, T item)
            {
                throw new NotSupportedException();
            }


            public bool IsReadOnly => true;

            public T this[int index]
            {
                get { return this.targetList[this.order[index]]; }
                set { throw new NotSupportedException(); }
            }


            public bool Remove(T item)
            {
                throw new NotSupportedException();
            }


            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }


            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        #endregion

        #region Nested type: WildcardStructType

        private struct WildcardStructType
        {
        }

        #endregion

        #region Nested type: WildcardType

        private class WildcardType
        {
        }

        #endregion
    }
}

