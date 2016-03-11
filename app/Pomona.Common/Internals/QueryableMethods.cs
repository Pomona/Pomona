#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Linq;

namespace Pomona.Common.Internals
{
    public static class QueryableMethods
    {
        static QueryableMethods()
        {
            Take = GetMethodInfo(x => Queryable.Take(x, 0));
            Skip = GetMethodInfo(x => Queryable.Skip(x, 0));
            Where = GetMethodInfo(x => x.Where(y => false));
            OrderBy = GetMethodInfo(x => x.OrderBy(y => y));
            OrderByDescending = GetMethodInfo(x => x.OrderByDescending(y => y));
            ThenBy = GetMethodInfo(x => x.ThenBy(y => y));
            ThenByDescending = GetMethodInfo(x => x.ThenByDescending(y => y));
            First = GetMethodInfo(x => x.First());
            FirstOrDefault = GetMethodInfo(x => Queryable.FirstOrDefault(x));
            FirstWithPredicate = GetMethodInfo(x => x.First(y => false));
            FirstOrDefaultWithPredicate = GetMethodInfo(x => x.FirstOrDefault(y => false));
            AnyWithPredicate = GetMethodInfo(x => x.Any(null));
            Select = GetMethodInfo(x => x.Select(y => 0));
            SelectMany = GetMethodInfo(x => x.SelectMany(y => (IEnumerable<object>)null));
            GroupBy = GetMethodInfo(x => x.GroupBy(y => 0));
            Count = GetMethodInfo(x => x.Count());

            SumIntWithSelector = GetMethodInfo(x => x.Sum(y => 0));
            SumInt = GetMethodInfo<int>(x => x.Sum());
            SumDoubleWithSelector = GetMethodInfo(x => x.Sum(y => 1.1));
            SumDouble = GetMethodInfo<double>(x => x.Sum());
            SumDecimalWithSelector = GetMethodInfo(x => x.Sum(y => 1.1m));
            SumDecimal = GetMethodInfo<decimal>(x => x.Sum());

            Max = GetMethodInfo(x => x.Max());
            MaxWithSelector = GetMethodInfo(x => x.Max(y => 0));
            Min = GetMethodInfo(x => x.Min());
            MinWithSelector = GetMethodInfo(x => x.Min(y => 0));

            Expand = GetMethodInfo(x => x.Expand(y => 0));
            IncludeTotalCount = GetMethodInfo(x => x.IncludeTotalCount());
            ToUri = GetMethodInfo(x => x.ToUri());
            FirstLazy = GetMethodInfo(x => x.FirstLazy());
            OfType = GetMethodInfo(x => x.OfType<object>());
        }


        public static MethodInfo AnyWithPredicate { get; }

        public static MethodInfo Count { get; }

        public static MethodInfo Expand { get; }

        public static MethodInfo First { get; }

        public static MethodInfo FirstLazy { get; }

        public static MethodInfo FirstOrDefault { get; }

        public static MethodInfo FirstOrDefaultWithPredicate { get; }

        public static MethodInfo FirstWithPredicate { get; }

        public static MethodInfo GroupBy { get; }

        public static MethodInfo IncludeTotalCount { get; }

        public static MethodInfo Max { get; }

        public static MethodInfo MaxWithSelector { get; }

        public static MethodInfo Min { get; }

        public static MethodInfo MinWithSelector { get; }

        public static MethodInfo OfType { get; }

        public static MethodInfo OrderBy { get; }

        public static MethodInfo OrderByDescending { get; }

        public static MethodInfo Select { get; }

        public static MethodInfo SelectMany { get; }

        public static MethodInfo Skip { get; }

        public static MethodInfo SumDecimal { get; }

        public static MethodInfo SumDecimalWithSelector { get; }

        public static MethodInfo SumDouble { get; }

        public static MethodInfo SumDoubleWithSelector { get; }

        public static MethodInfo SumInt { get; }

        public static MethodInfo SumIntWithSelector { get; }

        public static MethodInfo Take { get; }

        public static MethodInfo ThenBy { get; }

        public static MethodInfo ThenByDescending { get; }

        public static MethodInfo ToUri { get; }

        public static MethodInfo Where { get; }


        private static MethodInfo GetMethodInfo<TSource>(Expression<Action<IQueryable<TSource>>> expression)
        {
            return ReflectionHelper.GetMethodDefinition(expression);
        }


        private static MethodInfo GetMethodInfo(Expression<Action<IOrderedQueryable<object>>> expression)
        {
            return ReflectionHelper.GetMethodDefinition(expression);
        }
    }
}