using System;
using System.Linq.Expressions;
using System.Text;

namespace Pomona.Common
{
    internal class UriQueryBuilder
    {
        private readonly StringBuilder stringBuilder = new StringBuilder();

        public override string ToString()
        {
            return stringBuilder.ToString();
        }

        public void AppendParameter(string key, object value)
        {
            AppendQueryParameterStart(key);
            AppendEncodedQueryValue(value.ToString());
        }

        public void AppendExpressionParameter(string queryKey, LambdaExpression predicate, Func<string, string> transform = null)
        {
            var filterString = new QueryPredicateBuilder(predicate).ToString();

            if (transform != null)
                filterString = transform(filterString);

            AppendQueryParameterStart(queryKey);
            AppendEncodedQueryValue(filterString);
        }

        private void AppendQueryParameterStart(string queryKey)
        {
            if (stringBuilder.Length > 0)
                stringBuilder.Append('&');

            AppendEncodedQueryValue(queryKey);
            stringBuilder.Append('=');
        }


        private void AppendEncodedQueryValue(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var sb = stringBuilder;

            foreach (var b in bytes)
            {
                if (b == ' ')
                    sb.Append('+');
                else if (b < 128
                         &&
                         (char.IsLetterOrDigit((char) b) || b == '\'' || b == '.' || b == '~' || b == '-' || b == '_'
                          || b == ')' || b == '(' || b == ' ' || b == '$'))
                    sb.Append((char) b);
                else
                    sb.AppendFormat("%{0:X2}", b);
            }
        }

    }
}