#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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