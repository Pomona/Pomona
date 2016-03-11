#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Pomona.Common.Internals;
using Pomona.Common.Linq;

namespace Pomona.Common
{
    internal class UriQueryBuilder
    {
        private readonly StringBuilder stringBuilder = new StringBuilder();


        public void AppendExpressionParameter(string queryKey, Expression expression)
        {
            AppendExpressionParameter<QueryPredicateBuilder>(queryKey, expression);
        }


        public void AppendExpressionParameter<TVisitor>(string queryKey, Expression expression)
            where TVisitor : ExpressionVisitor, new()
        {
            var pomonaExpression = (PomonaExtendedExpression)expression.Visit<TVisitor>();
            if (!pomonaExpression.SupportedOnServer)
            {
                var unsupportedExpressions = pomonaExpression.WrapAsEnumerable()
                                                             .Flatten(x => x.Children.OfType<PomonaExtendedExpression>())
                                                             .OfType<NotSupportedByProviderExpression>().ToList();

                if (unsupportedExpressions.Count == 1)
                    throw unsupportedExpressions[0].Exception;

                throw new AggregateException(unsupportedExpressions.Select(x => x.Exception));
            }
            var filterString = pomonaExpression.ToString();

            AppendQueryParameterStart(queryKey);
            AppendEncodedQueryValue(filterString);
        }


        public void AppendParameter(string key, object value)
        {
            AppendQueryParameterStart(key);
            AppendEncodedQueryValue(value.ToString());
        }


        public override string ToString()
        {
            return this.stringBuilder.ToString();
        }


        private void AppendEncodedQueryValue(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var sb = this.stringBuilder;

            foreach (var b in bytes)
            {
                if (b == ' ')
                    sb.Append('+');
                else if (b < 128
                         &&
                         (char.IsLetterOrDigit((char)b) || b == '\'' || b == '.' || b == '~' || b == '-' || b == '_'
                          || b == ')' || b == '(' || b == ' ' || b == '$'))
                    sb.Append((char)b);
                else
                    sb.AppendFormat("%{0:X2}", b);
            }
        }


        private void AppendQueryParameterStart(string queryKey)
        {
            if (this.stringBuilder.Length > 0)
                this.stringBuilder.Append('&');

            AppendEncodedQueryValue(queryKey);
            this.stringBuilder.Append('=');
        }
    }
}