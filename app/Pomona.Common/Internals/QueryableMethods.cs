using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common.Linq;
using Pomona.Internals;

namespace Pomona.Common.Internals
{
    public static class QueryableMethods
    {
        private static readonly MethodInfo anyWithPredicate;
        private static readonly MethodInfo count;
        private static readonly MethodInfo expand;
        private static readonly MethodInfo first;
        private static readonly MethodInfo firstOrDefault;
        private static readonly MethodInfo firstOrDefaultWithPredicate;
        private static readonly MethodInfo firstWithPredicate;
        private static readonly MethodInfo groupBy;
        private static readonly MethodInfo orderBy;
        private static readonly MethodInfo orderByDescending;
        private static readonly MethodInfo @select;
        private static readonly MethodInfo skip;
        private static readonly MethodInfo take;
        private static readonly MethodInfo where;


        static QueryableMethods()
        {
            take = GetMethodInfo(x => x.Take(0));
            skip = GetMethodInfo(x => x.Skip(0));
            where = GetMethodInfo(x => x.Where(y => false));
            orderBy = GetMethodInfo(x => x.OrderBy(y => y));
            orderByDescending = GetMethodInfo(x => x.OrderByDescending(y => y));
            first = GetMethodInfo(x => x.First());
            firstOrDefault = GetMethodInfo(x => x.FirstOrDefault());
            firstWithPredicate = GetMethodInfo(x => x.First(y => false));
            firstOrDefaultWithPredicate = GetMethodInfo(x => x.FirstOrDefault(y => false));
            anyWithPredicate = GetMethodInfo(x => x.Any(null));
            select = GetMethodInfo(x => x.Select(y => 0));
            groupBy = GetMethodInfo(x => x.GroupBy(y => 0));
            count = GetMethodInfo(x => x.Count());

            expand = GetMethodInfo(x => x.Expand(y => 0));
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

        public static MethodInfo Skip
        {
            get { return skip; }
        }

        public static MethodInfo Take
        {
            get { return take; }
        }

        public static MethodInfo Where
        {
            get { return @where; }
        }


        private static MethodInfo GetMethodInfo(Expression<Func<IQueryable<object>, object>> expression)
        {
            return ReflectionHelper.GetGenericMethodDefinition(expression);
        }
    }
}