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
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Linq;

namespace Pomona.Common.Internals
{
    public static class QueryableMethods
    {
        private static readonly MethodInfo anyWithPredicate;
        private static readonly MethodInfo count;
        private static readonly MethodInfo expand;
        private static readonly MethodInfo first;
        private static readonly MethodInfo firstLazy;
        private static readonly MethodInfo firstOrDefault;
        private static readonly MethodInfo firstOrDefaultWithPredicate;
        private static readonly MethodInfo firstWithPredicate;
        private static readonly MethodInfo groupBy;
        private static readonly MethodInfo includeTotalCount;
        private static readonly MethodInfo max;
        private static readonly MethodInfo maxWithSelector;
        private static readonly MethodInfo min;
        private static readonly MethodInfo minWithSelector;
        private static readonly MethodInfo ofType;
        private static readonly MethodInfo orderBy;
        private static readonly MethodInfo orderByDescending;
        private static readonly MethodInfo @select;
        private static readonly MethodInfo selectMany;
        private static readonly MethodInfo skip;
        private static readonly MethodInfo sumDecimal;
        private static readonly MethodInfo sumDecimalWithSelector;
        private static readonly MethodInfo sumDouble;
        private static readonly MethodInfo sumDoubleWithSelector;
        private static readonly MethodInfo sumInt;
        private static readonly MethodInfo sumIntWithSelector;
        private static readonly MethodInfo take;
        private static readonly MethodInfo toUri;
        private static readonly MethodInfo where;
        private static readonly MethodInfo thenBy;
        private static readonly MethodInfo thenByDescending;


        static QueryableMethods()
        {
            take = GetMethodInfo(x => Queryable.Take(x, 0));
            skip = GetMethodInfo(x => Queryable.Skip(x, 0));
            where = GetMethodInfo(x => x.Where(y => false));
            orderBy = GetMethodInfo(x => x.OrderBy(y => y));
            orderByDescending = GetMethodInfo(x => x.OrderByDescending(y => y));
            thenBy = GetMethodInfo(x => x.ThenBy(y => y));
            thenByDescending = GetMethodInfo(x => x.ThenByDescending(y => y));
            first = GetMethodInfo(x => x.First());
            firstOrDefault = GetMethodInfo(x => Queryable.FirstOrDefault(x));
            firstWithPredicate = GetMethodInfo(x => x.First(y => false));
            firstOrDefaultWithPredicate = GetMethodInfo(x => x.FirstOrDefault(y => false));
            anyWithPredicate = GetMethodInfo(x => x.Any(null));
            select = GetMethodInfo(x => x.Select(y => 0));
            selectMany = GetMethodInfo(x => x.SelectMany(y => (IEnumerable<object>)null));
            groupBy = GetMethodInfo(x => x.GroupBy(y => 0));
            count = GetMethodInfo(x => x.Count());

            sumIntWithSelector = GetMethodInfo(x => x.Sum(y => 0));
            sumInt = GetMethodInfo<int>(x => x.Sum());
            sumDoubleWithSelector = GetMethodInfo(x => x.Sum(y => 1.1));
            sumDouble = GetMethodInfo<double>(x => x.Sum());
            sumDecimalWithSelector = GetMethodInfo(x => x.Sum(y => 1.1m));
            sumDecimal = GetMethodInfo<decimal>(x => x.Sum());

            max = GetMethodInfo(x => x.Max());
            maxWithSelector = GetMethodInfo(x => x.Max(y => 0));
            min = GetMethodInfo(x => x.Min());
            minWithSelector = GetMethodInfo(x => x.Min(y => 0));

            expand = GetMethodInfo(x => x.Expand(y => 0));
            includeTotalCount = GetMethodInfo(x => x.IncludeTotalCount());
            toUri = GetMethodInfo(x => x.ToUri());
            firstLazy = GetMethodInfo(x => x.FirstLazy());
            ofType = GetMethodInfo(x => x.OfType<object>());
        }


        public static MethodInfo AnyWithPredicate
        {
            get { return anyWithPredicate; }
        }

        public static MethodInfo Count
        {
            get { return count; }
        }

        public static MethodInfo Expand
        {
            get { return expand; }
        }

        public static MethodInfo First
        {
            get { return first; }
        }

        public static MethodInfo FirstLazy
        {
            get { return firstLazy; }
        }

        public static MethodInfo FirstOrDefault
        {
            get { return firstOrDefault; }
        }

        public static MethodInfo FirstOrDefaultWithPredicate
        {
            get { return firstOrDefaultWithPredicate; }
        }

        public static MethodInfo FirstWithPredicate
        {
            get { return firstWithPredicate; }
        }

        public static MethodInfo GroupBy
        {
            get { return groupBy; }
        }

        public static MethodInfo IncludeTotalCount
        {
            get { return includeTotalCount; }
        }

        public static MethodInfo Max
        {
            get { return max; }
        }

        public static MethodInfo MaxWithSelector
        {
            get { return maxWithSelector; }
        }

        public static MethodInfo Min
        {
            get { return min; }
        }

        public static MethodInfo MinWithSelector
        {
            get { return minWithSelector; }
        }

        public static MethodInfo OfType
        {
            get { return ofType; }
        }

        public static MethodInfo OrderBy
        {
            get { return orderBy; }
        }

        public static MethodInfo OrderByDescending
        {
            get { return orderByDescending; }
        }

        public static MethodInfo Select
        {
            get { return @select; }
        }

        public static MethodInfo SelectMany
        {
            get { return selectMany; }
        }

        public static MethodInfo Skip
        {
            get { return skip; }
        }

        public static MethodInfo SumDecimal
        {
            get { return sumDecimal; }
        }

        public static MethodInfo SumDecimalWithSelector
        {
            get { return sumDecimalWithSelector; }
        }

        public static MethodInfo ThenBy
        {
            get { return thenBy; }
        }

        public static MethodInfo ThenByDescending
        {
            get { return thenByDescending; }
        }

        public static MethodInfo SumDouble
        {
            get { return sumDouble; }
        }

        public static MethodInfo SumDoubleWithSelector
        {
            get { return sumDoubleWithSelector; }
        }

        public static MethodInfo SumInt
        {
            get { return sumInt; }
        }

        public static MethodInfo SumIntWithSelector
        {
            get { return sumIntWithSelector; }
        }

        public static MethodInfo Take
        {
            get { return take; }
        }

        public static MethodInfo ToUri
        {
            get { return toUri; }
        }

        public static MethodInfo Where
        {
            get { return @where; }
        }


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