// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Pomona.Internals;

namespace Pomona.Common.Internals
{
    public class OdataFunctionMapping
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
        public static readonly MethodInfo SafeGetMethod;

        private static readonly Dictionary<UniqueMemberToken, MemberMapping> metadataTokenToMemberMappingDict =
            new Dictionary<UniqueMemberToken, MemberMapping>();

        private static readonly Dictionary<string, List<MemberMapping>> nameToMemberMappingDict =
            new Dictionary<string, List<MemberMapping>>();


        static OdataFunctionMapping()
        {
            DictStringStringGetMethod = ReflectionHelper.GetInstanceMethodInfo<IDictionary<string, string>>(x => x[null]);
            EnumerableContainsMethod =
                ReflectionHelper.GetMethodDefinition<IEnumerable<object>>(x => x.Contains(null));
            SafeGetMethod =
                ReflectionHelper.GetMethodDefinition<IDictionary<object, object>>(x => x.SafeGet(null));

            Add<string>(x => x.Length, "length({0})");
            Add<string>(x => x.StartsWith(null), "startswith({0},{1})");
            Add<string>(x => x.EndsWith(null), "endswith({0},{1})");
            Add<string>(x => x.Contains(null), "substringof({1},{0})");
            Add<string>(x => x.Substring(0), "substring({0},{1})");
            Add<string>(x => x.Substring(0, 0), "substring({0},{1},{2})");
            Add<string>(x => x.Replace("", ""), "replace({0},{1},{2})");
            Add<string>(x => x.Replace('a', 'a'), "replace({0},{1},{2})");
            Add<string>(x => x.ToLower(), "tolower({0})");
            Add<string>(x => x.ToUpper(), "toupper({0})");
            Add<string>(x => x.Trim(), "trim({0})");
            Add<string>(x => x.IndexOf("a"), "indexof({0},{1})");
            Add<string>(x => x.IndexOf('a'), "indexof({0},{1})");
            Add<string>(x => string.Concat("", ""), "concat({0},{1})");
            Add<string>(x => x.Trim(), "trim({0})");

            // TODO: Concat function, this one's static

            Add<DateTime>(x => x.Day, "day({0})");
            Add<DateTime>(x => x.Hour, "hour({0})");
            Add<DateTime>(x => x.Minute, "minute({0})");
            Add<DateTime>(x => x.Month, "month({0})");
            Add<DateTime>(x => x.Second, "second({0})");
            Add<DateTime>(x => x.Year, "year({0})");

            // TODO Math functions, these are static
            Add<double>(x => Math.Sqrt(x), "sqrt({0})");

            // TODO: Multiple overloads working on different types are not yet working as it should.
            Add<double>(x => Math.Round(x), "round({0})");
            Add<decimal>(x => decimal.Round(x), "round({0})");
            Add<double>(x => Math.Floor(x), "floor({0})");
            Add<decimal>(x => decimal.Floor(x), "floor({0})");
            Add<double>(x => Math.Ceiling(x), "ceiling({0})");
            Add<decimal>(x => decimal.Ceiling(x), "ceiling({0})");

            // Custom functions, not odata standard
            Add<IEnumerable<WildcardType>>(x => x.Any(), "any({0})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Any(null), "any({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(
                x => x.Select(y => (WildcardType) null), "select({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Where(y => false), "where({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Count(), "count({0})");
            Add<IEnumerable<WildcardType>>(x => x.SelectMany(y => (IEnumerable<string>) null), "many({0},{1})",
                                           MethodCallStyle.Chained);
            Add<ICollection<WildcardType>>(x => x.Count, "count({0})");
            Add<IEnumerable<WildcardType>>(x => x.First(), "first({0})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.FirstOrDefault(), "firstdefault({0})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.First(y => true), "first({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.FirstOrDefault(y => true), "firstdefault({0},{1})",
                                           MethodCallStyle.Chained);

            Add<IEnumerable<int>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<double>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<float>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<decimal>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<int?>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<double?>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<float?>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<decimal?>>(x => x.Sum(), "sum({0})");

            Add<IEnumerable<string>>(x => string.Join("?", x), "join({1},{0})", MethodCallStyle.Chained);

            Add<IEnumerable<WildcardType>>(x => x.Sum(y => 10m), "sum({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Sum(y => 10), "sum({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Sum(y => 10.0), "sum({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Sum(y => 10f), "sum({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Sum(y => (decimal?) 10m), "sum({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Sum(y => (int?) 10), "sum({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Sum(y => (double?) 10.0), "sum({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Sum(y => (float?) 10f), "sum({0},{1})", MethodCallStyle.Chained);

            Add<IEnumerable<int>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<decimal>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<double>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<float>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<int?>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<decimal?>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<double?>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);
            Add<IEnumerable<float?>>(x => x.Max(), "max({0})", MethodCallStyle.Chained);

            Add<IEnumerable<WildcardType>>(x => x.Max(y => 10m), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => 10), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => 10.0), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => 10f), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => (decimal?) 10m), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => (int?) 10), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => (double?) 10.0), "max({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Max(y => (float?) 10f), "max({0},{1})", MethodCallStyle.Chained);

            Add<IEnumerable<int>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<decimal>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<double>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<float>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<int?>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<decimal?>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<double?>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);
            Add<IEnumerable<float?>>(x => x.Min(), "min({0})", MethodCallStyle.Chained);

            Add<IEnumerable<WildcardType>>(x => x.Min(y => 10m), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => 10), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => 10.0), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => 10f), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => (decimal?) 10m), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => (int?) 10), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => (double?) 10.0), "min({0},{1})", MethodCallStyle.Chained);
            Add<IEnumerable<WildcardType>>(x => x.Min(y => (float?) 10f), "min({0},{1})", MethodCallStyle.Chained);

            Add<IDictionary<WildcardType, WildcardType>>(
                x => x.Contains(null, null), "contains({0},{1},{2})", MethodCallStyle.Chained);
            Add<IDictionary<WildcardType, WildcardType>>(
                x => x.SafeGet(null), "safeget({0},{1})", MethodCallStyle.Chained);

            Add<WildcardType>(x => Convert.ChangeType(x, null), "convert({0},{1})",
                              postResolveHook: FixChangeTypeReturnValue);
        }

        /// <summary>
        /// This function converts result value of Convert.ChangeType to if type argument is a constant.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        private static Expression FixChangeTypeReturnValue(Expression expr)
        {
            var callExpr = (MethodCallExpression) expr;

            var typeArgExpr = callExpr.Arguments[1] as ConstantExpression;
            if (typeArgExpr == null || typeArgExpr.Value == null)
                return expr;

            var convertToType = (Type) typeArgExpr.Value;

            return Expression.Convert(expr, convertToType);
        }


        public static IEnumerable<MemberMapping> GetMemberCandidates(
            string odataFunctionName, int argCount)
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
            Func<Expression, Expression> postResolveHook = null)
        {
            var memberInfo = ReflectionHelper.GetInstanceMemberInfo(expr);

            var memberMapping = MemberMapping.Parse(memberInfo, functionFormat, preferredCallStyle, postResolveHook);
            nameToMemberMappingDict.GetOrCreate(memberMapping.Name + memberMapping.ArgumentCount).Add(memberMapping);
            metadataTokenToMemberMappingDict[memberMapping.Member.UniqueToken()] = memberMapping;
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
            private readonly IList<int> argumentOrder;
            private readonly string chainedCallFormat;
            private readonly MemberInfo member;

            private readonly string name;
            private readonly Func<Expression, Expression> postResolveHookFunction;
            private readonly MethodCallStyle preferredCallStyle;
            private readonly string staticCallFormat;


            private MemberMapping(
                MemberInfo member,
                string name,
                IList<int> argumentOrder,
                string staticCallFormat,
                string chainedCallFormat,
                MethodCallStyle preferredCallStyle,
                Func<Expression, Expression> postResolveHookFunction)
            {
                this.member = member;
                this.name = name;
                this.argumentOrder = argumentOrder;
                this.staticCallFormat = staticCallFormat;
                this.chainedCallFormat = chainedCallFormat;
                this.preferredCallStyle = preferredCallStyle;
                this.postResolveHookFunction = postResolveHookFunction;
            }


            public int ArgumentCount
            {
                get { return ArgumentOrder.Count; }
            }

            public IList<int> ArgumentOrder
            {
                get { return argumentOrder; }
            }

            public string ChainedCallFormat
            {
                get { return chainedCallFormat; }
            }

            public MemberInfo Member
            {
                get { return member; }
            }

            public string Name
            {
                get { return name; }
            }

            public MethodCallStyle PreferredCallStyle
            {
                get { return preferredCallStyle; }
            }

            public string StaticCallFormat
            {
                get { return staticCallFormat; }
            }

            public Expression PostResolveHook(Expression expression)
            {
                return postResolveHookFunction != null ? postResolveHookFunction(expression) : expression;
            }


            public static MemberMapping Parse(
                MemberInfo member, string odataMethodFormat, MethodCallStyle preferredCallStyle,
                Func<Expression, Expression> postResolveHookFunction)
            {
                var name = odataMethodFormat.Split('(').First();
                var argOrder = GetArgumentOrder(odataMethodFormat);

                var memberAsMethod = member as MethodInfo;
                if (memberAsMethod != null)
                {
                    if (memberAsMethod.IsGenericMethod
                        && memberAsMethod.GetGenericArguments().Any(x => x == typeof (WildcardType)))
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
                    member, name, argOrderArray, odataMethodFormat, extensionMethodFormatString, preferredCallStyle,
                    postResolveHookFunction);
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
                var wildcardType = typeof (WildcardType);
                return genericArguments.Any(x => x == wildcardType) || genericArguments.Any(HasWildcardArgument);
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
                if (targetList.Count != order.Count)
                    throw new ArgumentException();
                if (targetList == null)
                    throw new ArgumentNullException("targetList");
                if (order == null)
                    throw new ArgumentNullException("order");
                this.targetList = targetList;
                this.order = order;
            }


            public T this[int index]
            {
                get { return targetList[order[index]]; }
                set { throw new NotSupportedException(); }
            }


            public int Count
            {
                get { return targetList.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
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
                return targetList.Contains(item);
            }


            public void CopyTo(T[] array, int arrayIndex)
            {
                this.ToList().CopyTo(array, arrayIndex);
            }


            public IEnumerator<T> GetEnumerator()
            {
                for (var i = 0; i < Count; i++)
                    yield return this[i];
            }


            public int IndexOf(T item)
            {
                var index = targetList.IndexOf(item);
                if (index != -1)
                    index = order.IndexOf(index);
                return index;
            }


            public void Insert(int index, T item)
            {
                throw new NotSupportedException();
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

        #region Nested type: WildcardType

        private class WildcardType
        {
        }

        #endregion
    }
}