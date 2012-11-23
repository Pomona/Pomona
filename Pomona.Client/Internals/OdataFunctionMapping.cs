#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Internals;

namespace Pomona.Client.Internals
{
    public class OdataFunctionMapping
    {
        public static readonly MethodInfo DictGetMethod;

        private static readonly Dictionary<MemberInfo, string> memberToOdataFunctionMap =
            new Dictionary<MemberInfo, string>();

        private static readonly Dictionary<string, MemberInfo> odataFunctionToMemberMap =
            new Dictionary<string, MemberInfo>();

        private static readonly object[] questionStrings;

        private static Dictionary<int, MemberMapping> metadataTokenToMemberMappingDict =
            new Dictionary<int, MemberMapping>();

        private static Dictionary<string, List<MemberMapping>> nameToMemberMappingDict =
            new Dictionary<string, List<MemberMapping>>();


        static OdataFunctionMapping()
        {
            DictGetMethod = ReflectionHelper.GetInstanceMethodInfo<IDictionary<string, string>>(x => x[null]);

            // Just some empty strings used for creating odata method signature
            questionStrings = Enumerable.Repeat("?", 100).Cast<object>().ToArray();

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
            Add<ICollection<WildcardType>>(x => x.Count, "count({0})");
            Add<IEnumerable<WildcardType>>(x => x.Any(null), "any({0},{1})");
            Add<IEnumerable<WildcardType>>(x => x.Select(y => (WildcardType)null), "select({0},{1})");
            Add<IEnumerable<WildcardType>>(x => x.Where(y => false), "where({0},{1})");
            Add<IEnumerable<WildcardType>>(x => x.Count(), "count({0})");
            Add<IEnumerable<int>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<double>>(x => x.Sum(), "sum({0})");
            Add<IEnumerable<decimal>>(x => x.Sum(), "sum({0})");
        }


        public static MemberInfo ChangeDeclaringGenericTypeArguments(MemberInfo member, Type[] genericParameters)
        {
            var declaringType = member.DeclaringType;
            var genericTypeDefinition = declaringType.GetGenericTypeDefinition();
            var wildcardTypeArguments = genericParameters;
            var wildcardTypeInstance = genericTypeDefinition.MakeGenericType(wildcardTypeArguments);
            var wildcardMemberCandidates = wildcardTypeInstance.GetMember(
                member.Name, member.MemberType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (wildcardMemberCandidates.Length > 1)
            {
                throw new NotImplementedException(
                    "Unable to map method of generic type with wildcards that have multiple members with same name and same member type.");
            }

            var changedMember = wildcardMemberCandidates[0];

            return changedMember;
        }


        public static IEnumerable<MemberMapping> GetMemberCandidates(
            string odataFunctionName, int argCount)
        {
            return nameToMemberMappingDict.GetValueOrDefault(odataFunctionName + argCount)
                   ?? Enumerable.Empty<MemberMapping>();
        }


        public static bool TryConvertToExpression(
            string odataFunctionName, int argCount, IEnumerable<Expression> arguments, out Expression expression)
        {
            var argumentsList = arguments.ToList();

            expression = null;
            var odataFunctionSpec = string.Format(
                "{0}({1})>({2})",
                odataFunctionName,
                string.Join(",", Enumerable.Repeat("?", argCount)),
                string.Join(",", argumentsList.Select(x => Type.GetTypeCode(x.Type))));
            //var odataFunctionSpec = string.Format(
            //    "{0}({1})", odataFunctionName, string.Join(",", Enumerable.Repeat("?", argCount)));
            MemberInfo memberInfo;
            var memberfound = odataFunctionToMemberMap.TryGetValue(odataFunctionSpec, out memberInfo);
            if (!memberfound)
                return false;

            var odataOrderedArglist = argumentsList;

            var formatString = memberToOdataFunctionMap[memberInfo];
            var argOrder = GetArgumentOrder(formatString).ToArray();
            var fixedArgList = argOrder.Select(x => odataOrderedArglist[x]);

            if (IsInstanceMember(memberInfo))
            {
                var instance = fixedArgList.First();
                fixedArgList = fixedArgList.Skip(1);

                var instanceType = instance.Type;

                var memberDeclaringType = memberInfo.DeclaringType;
                if (!memberDeclaringType.IsAssignableFrom(instanceType))
                {
                    // First check whether we're dealing with a wildcarded generic method
                    Type[] instanceGenericArguments;
                    if (memberDeclaringType.IsGenericType
                        &&
                        TypeUtils.TryGetTypeArguments(
                            instanceType, memberDeclaringType.GetGenericTypeDefinition(), out instanceGenericArguments))
                    {
                        // Get member of correct generic type instance
                        memberInfo = ChangeDeclaringGenericTypeArguments(memberInfo, instanceGenericArguments);
                    }
                    else
                    {
                        // TODO: Proper error here..
                        return false;
                    }
                }

                var methodInfo = memberInfo as MethodInfo;
                if (methodInfo != null)
                {
                    expression = Expression.Call(instance, methodInfo, fixedArgList);
                    return true;
                }

                expression = Expression.MakeMemberAccess(instance, memberInfo);
                return true;
            }
            else
            {
                // Static method
                var methodInfo = memberInfo as MethodInfo;
                if (methodInfo != null)
                {
                    expression = Expression.Call(methodInfo, fixedArgList);
                    return true;
                }
            }

            return false;
        }


        public static bool TryGetOdataFunctionFormatString(MemberInfo member, out string functionFormat)
        {
            functionFormat = null;
            MemberMapping memberMapping;
            if (!metadataTokenToMemberMappingDict.TryGetValue(member.MetadataToken, out memberMapping))
                return false;

            functionFormat = memberMapping.MethodFormatString;
            return true;
        }


        private static void Add<T>(Expression<Func<T, object>> expr, string functionFormat)
        {
            var memberInfo = ReflectionHelper.GetInstanceMemberInfo(expr);

            var memberMapping = MemberMapping.Parse(memberInfo, functionFormat);
            nameToMemberMappingDict.GetOrCreate(memberMapping.Name + memberMapping.ArgumentCount).Add(memberMapping);
            metadataTokenToMemberMappingDict[memberMapping.Member.MetadataToken] = memberMapping;

            memberToOdataFunctionMap[memberInfo] = functionFormat;

            // We might have multiple overloads of one function with same argument count, so we add the type to the key
            IEnumerable<Type> args = null;

            var propInfo = memberInfo as PropertyInfo;
            if (propInfo != null)
            {
                if (propInfo.GetGetMethod().IsStatic)
                    throw new NotImplementedException("Don't know what to do with a static property here :(");

                args = propInfo.DeclaringType.WrapAsEnumerable();
            }
            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null)
            {
                if (methodInfo.IsStatic)
                    args = methodInfo.GetParameters().Select(x => x.ParameterType);
                else
                {
                    args = methodInfo.DeclaringType.WrapAsEnumerable()
                        .Concat(methodInfo.GetParameters().Select(x => x.ParameterType));
                }
            }

            var methodSpecString = string.Format(functionFormat, questionStrings) + string.Format(
                ">({0})",
                string.Join(",", args.Select(Type.GetTypeCode)));

            odataFunctionToMemberMap[methodSpecString] = memberInfo;
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


        private static bool IsInstanceMember(MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Method)
                return (((MethodInfo)memberInfo).Attributes & MethodAttributes.Static) == 0;
            if (memberInfo.MemberType == MemberTypes.Property)
                return (((PropertyInfo)memberInfo).GetGetMethod().Attributes & MethodAttributes.Static) == 0;
            throw new NotImplementedException();
        }

        #region Nested type: MemberMapping

        public class MemberMapping
        {
            private readonly IList<int> argumentOrder;
            private readonly MemberInfo member;
            private readonly string methodFormatString;
            private readonly string name;


            private MemberMapping(MemberInfo member, string name, IList<int> argumentOrder, string methodFormatString)
            {
                this.member = member;
                this.name = name;
                this.argumentOrder = argumentOrder;
                this.methodFormatString = methodFormatString;
            }


            public int ArgumentCount
            {
                get { return ArgumentOrder.Count; }
            }

            public IList<int> ArgumentOrder
            {
                get { return this.argumentOrder; }
            }

            public MemberInfo Member
            {
                get { return this.member; }
            }

            public string MethodFormatString
            {
                get { return this.methodFormatString; }
            }

            public string Name
            {
                get { return this.name; }
            }


            public static MemberMapping Parse(MemberInfo member, string odataMethodFormat)
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
                            .First(x => x.MetadataToken == memberLocal.MetadataToken);
                }

                return new MemberMapping(member, name, argOrder.ToArray(), odataMethodFormat);
            }


            public IList<T> ReorderArguments<T>(IList<T> arguments)
            {
                return new ReorderedList<T>(arguments, ArgumentOrder);
            }


            private static bool HasWildcardArgument(Type type)
            {
                if (!type.IsGenericType)
                    return false;

                var genericArguments = type.GetGenericArguments();
                var wildcardType = typeof(WildcardType);
                return genericArguments.Any(x => x == wildcardType) || genericArguments.Any(HasWildcardArgument);
            }
        }

        #endregion

        #region Nested type: ReorderedList

        private class ReorderedList<T> : IList<T>
        {
            private IList<int> order;
            private IList<T> targetList;


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
                get { return this.targetList[this.order[index]]; }
                set { throw new NotSupportedException(); }
            }


            public int Count
            {
                get { return this.targetList.Count; }
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
                return this.targetList.Contains(item);
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
                var index = this.targetList.IndexOf(item);
                if (index != -1)
                    index = this.order.IndexOf(index);
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