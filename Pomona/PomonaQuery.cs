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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Pomona
{
    /// <summary>
    /// A default implementation of IPomonaQuery, only simple querying.
    /// </summary>
    public class PomonaQuery : IPomonaQuery
    {
        #region Operator enum

        public enum Operator
        {
            Eq,
            Like,
            In
        }

        #endregion

        private readonly TransformedType targetType;

        public PomonaQuery(TransformedType targetType)
        {
            if (targetType == null) throw new ArgumentNullException("targetType");
            this.targetType = targetType;
        }

        public int Skip { get; set; }
        public int Take { get; set; }
        public string OrderBy { get; set; }
        public IList<Condition> Conditions { get; set; }

        #region IPomonaQuery Members

        public IEnumerable<string> ExpandedPaths { get; set; }

        public TransformedType TargetType
        {
            get { return targetType; }
        }

        #endregion

        public Expression<Func<T, bool>> CreateExpression<T>()
        {
            var parameter = Expression.Parameter(TargetType.SourceType, "x");
            Expression finalExpr = Expression.Constant(true);

            foreach (var condition in Conditions)
            {
                var op = condition.Operator;

                var propExpr = targetType.CreateExpressionForExternalPropertyPath(parameter, condition.PropertyName);

                var propType = propExpr.Type;

                if (propType != typeof (string))
                {
                    throw new NotImplementedException();
                }

                if (op != Operator.Eq)
                    throw new NotImplementedException();

                var equalToExpr = Expression.Equal(propExpr, Expression.Constant(condition.Value));

                if (finalExpr == null)
                {
                    finalExpr = equalToExpr;
                }
                else
                {
                    finalExpr = Expression.AndAlso(finalExpr, equalToExpr);
                }
            }

            var lambdaExpr = Expression.Lambda<Func<T, bool>>(finalExpr, parameter);

            return lambdaExpr;
        }

        #region Nested type: Condition

        public class Condition
        {
            public string PropertyName { get; set; }
            public Operator Operator { get; set; }
            public string Value { get; set; }
        }

        #endregion
    }
}