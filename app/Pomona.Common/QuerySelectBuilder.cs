using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Pomona.Common
{
    public class QuerySelectBuilder
    {
        private readonly LambdaExpression lambda;

        public QuerySelectBuilder(LambdaExpression lambda)
        {
            if (lambda == null) throw new ArgumentNullException("lambda");
            this.lambda = lambda;
        }


        public override string ToString()
        {
            var sb = new StringBuilder();

            var newExprBody = lambda.Body as NewExpression;

            if (newExprBody != null)
            {
                foreach (
                    var arg in
                        newExprBody.Arguments.Zip(
                            newExprBody.Members, (e, p) => new { p.Name, Expr = e }))
                {
                    if (sb.Length > 0)
                        sb.Append(',');
                    var argLambda = Expression.Lambda(arg.Expr, lambda.Parameters);
                    var predicateBuilder = new QueryPredicateBuilder(argLambda);
                    sb.Append(predicateBuilder);
                    sb.Append(" as ");
                    sb.Append(arg.Name);
                }
            }
            else
            {
                var predicateBuilder = new QueryPredicateBuilder(lambda);
                sb.Append(predicateBuilder);
                sb.Append(" as this");
            }

            return sb.ToString();
        }
    }
}