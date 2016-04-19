#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq.Expressions;

namespace Pomona.Common.Linq.Queries
{
    public static class QueryExpressionExtensions
    {
        public static DefaultIfEmptyExpression DefaultIfEmpty(this QueryExpression source)
        {
            return DefaultIfEmptyExpression.Create(source);
        }


        public static DistinctExpression Distinct(this QueryExpression source)
        {
            return DistinctExpression.Create(source);
        }


        public static GroupByExpression GroupBy(this QueryExpression source, LambdaExpression keySelector)
        {
            return GroupByExpression.Create(source, keySelector);
        }


        public static OfTypeExpression OfType(this QueryExpression source, Type type)
        {
            return OfTypeExpression.Create(source, type);
        }


        public static SelectExpression Select(this QueryExpression source, LambdaExpression selector)
        {
            return SelectExpression.Create(source, selector);
        }


        public static SelectManyExpression SelectMany(this QueryExpression source, LambdaExpression selector)
        {
            return SelectManyExpression.Create(source, selector);
        }


        public static SkipExpression Skip(this QueryExpression source, int count)
        {
            return SkipExpression.Create(source, count);
        }


        public static TakeExpression Take(this QueryExpression source, int count)
        {
            return TakeExpression.Create(source, count);
        }


        public static WhereExpression Where(this QueryExpression source, LambdaExpression predicate)
        {
            return WhereExpression.Create(source, predicate);
        }


        public static ZipExpression Zip(this QueryExpression source,
                                        QueryExpression source2,
                                        LambdaExpression resultSelector)
        {
            return ZipExpression.Create(source, source2, resultSelector);
        }
    }
}