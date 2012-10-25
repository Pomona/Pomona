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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Pomona.Client
{
    public abstract class QueryPredicateBuilder
    {
        protected static readonly ReadOnlyDictionary<ExpressionType, string> binaryExpressionNodeDict;


        static QueryPredicateBuilder()
        {
            var binExprDict = new Dictionary<ExpressionType, string>()
            {
                { ExpressionType.AndAlso, "and" },
                { ExpressionType.OrElse, "or" },
                { ExpressionType.Equal, "eq" },
                { ExpressionType.GreaterThan, "gt" },
                { ExpressionType.GreaterThanOrEqual, "ge" },
                { ExpressionType.LessThan, "lt" },
                { ExpressionType.LessThanOrEqual, "le" }
            };

            binaryExpressionNodeDict = new ReadOnlyDictionary<ExpressionType, string>(binExprDict);
        }
    }

    public class QueryPredicateBuilder<T> : QueryPredicateBuilder
    {
        private readonly Expression<Func<T, bool>> lambda;


        public QueryPredicateBuilder(Expression<Func<T, bool>> lambda)
        {
            if (lambda == null)
                throw new ArgumentNullException("expr");
            this.lambda = lambda;
        }


        private ParameterExpression InstanceParameter
        {
            get { return this.lambda.Parameters[0]; }
        }


        public override string ToString()
        {
            return Build(this.lambda.Body);
        }


        private static string GetMemberName(MemberInfo member)
        {
            // Do it JSON style camelCase
            return member.Name.Substring(0, 1).ToLower() + member.Name.Substring(1);
        }


        private string Build(Expression expr)
        {
            var binaryExpr = expr as BinaryExpression;
            if (binaryExpr != null)
                return BuildFromBinaryExpression(binaryExpr);

            var memberExpr = expr as MemberExpression;
            if (memberExpr != null)
                return BuildFromMemberExpression(memberExpr);

            var constantExpr = expr as ConstantExpression;
            if (constantExpr != null)
                return BuildFromConstantExpression(constantExpr);

            throw new NotImplementedException("NodeType " + expr.NodeType + " not yet handled.");
        }


        private string BuildFromBinaryExpression(BinaryExpression binaryExpr)
        {
            string opString;
            if (!binaryExpressionNodeDict.TryGetValue(binaryExpr.NodeType, out opString))
            {
                throw new NotImplementedException(
                    "BinaryExpression NodeType " + binaryExpr.NodeType + " not yet handled.");
            }
            return string.Format("({0} {1} {2})", Build(binaryExpr.Left), opString, Build(binaryExpr.Right));
        }


        private string BuildFromConstantExpression(ConstantExpression constantExpr)
        {
            var valueType = constantExpr.Type;
            var value = constantExpr.Value;
            return GetEncodedConstant(valueType, value);
        }


        private string BuildFromMemberExpression(MemberExpression memberExpr)
        {
            // This gets weird with closures, see:
            // http://blog.nexterday.com/post/Automatic-compilation-of-Linq-to-SQL-queries.aspx

            if (memberExpr.Member.DeclaringType.Name.StartsWith("<>c__")
                && memberExpr.Member.MemberType == MemberTypes.Field)
            {
                var field = memberExpr.Member as FieldInfo;
                if (field != null)
                {
                    var obj = ((ConstantExpression)memberExpr.Expression).Value;

                    //Add the value to the extraction list
                    var value = field.GetValue(obj);
                    return GetEncodedConstant(field.FieldType, value);
                }
            }

            if (memberExpr.Expression != InstanceParameter)
                throw new NotImplementedException("Only support one level of properties for now..");
            return GetMemberName(memberExpr.Member);
        }


        private string EncodeString(string text)
        {
            // TODO: IMPORTANT! Proper encoding!!

            return "'" + text + "'";
        }


        private string GetEncodedConstant(Type valueType, object value)
        {
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.String:
                    return EncodeString((string)value);
                case TypeCode.Int32:
                    return value.ToString();
                default:
                    throw new NotImplementedException(
                        "Don't know how to send constant of type " + valueType.FullName + " yet..");
            }
        }
    }
}