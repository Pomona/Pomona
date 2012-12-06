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
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Pomona.Common.Internals;

namespace Pomona.Common
{
    public class QueryPredicateBuilder
    {
        protected static readonly ReadOnlyDictionary<ExpressionType, string> binaryExpressionNodeDict;

        private static HashSet<char> validSymbolCharacters =
            new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_");

        private readonly LambdaExpression lambda;
        private ParameterExpression thisParameter;


        static QueryPredicateBuilder()
        {
            var binExprDict = new Dictionary<ExpressionType, string>()
                {
                    {ExpressionType.AndAlso, "and"},
                    {ExpressionType.OrElse, "or"},
                    {ExpressionType.Equal, "eq"},
                    {ExpressionType.NotEqual, "ne"},
                    {ExpressionType.GreaterThan, "gt"},
                    {ExpressionType.GreaterThanOrEqual, "ge"},
                    {ExpressionType.LessThan, "lt"},
                    {ExpressionType.LessThanOrEqual, "le"},
                    {ExpressionType.Subtract, "sub"},
                    {ExpressionType.Add, "add"},
                    {ExpressionType.Multiply, "mul"},
                    {ExpressionType.Divide, "div"},
                    {ExpressionType.Modulo, "mod"}
                };

            binaryExpressionNodeDict = new ReadOnlyDictionary<ExpressionType, string>(binExprDict);
        }


        public QueryPredicateBuilder(LambdaExpression lambda, ParameterExpression thisParameter = null)
        {
            if (lambda == null)
                throw new ArgumentNullException("expr");
            this.thisParameter = thisParameter ?? lambda.Parameters[0];
            this.lambda = lambda;
        }


        public static QueryPredicateBuilder Create<T>(Expression<Func<T, bool>> lambda)
        {
            return new QueryPredicateBuilder(lambda);
        }


        public static QueryPredicateBuilder Create<T, TResult>(Expression<Func<T, TResult>> lambda)
        {
            return new QueryPredicateBuilder(lambda);
        }


        public override string ToString()
        {
            // Strip away redundant parens around query
            var visitor = new PreBuildVisitor();
            var preprocessedBody = visitor.Visit(lambda.Body);
            var queryFilterString = Build(preprocessedBody);
            if (queryFilterString.Length > 1 && queryFilterString[0] == '('
                && queryFilterString[queryFilterString.Length - 1] == ')')
                queryFilterString = queryFilterString.Substring(1, queryFilterString.Length - 2);
            return queryFilterString;
        }


        private static bool ContainsOnlyValidSymbolCharacters(string text)
        {
            var containsOnlyValidSymbolCharacters = text.All(x => validSymbolCharacters.Contains(x));
            return containsOnlyValidSymbolCharacters;
        }


        private static string DateTimeToString(DateTime dt)
        {
            var roundedDt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Kind);
            if (roundedDt == dt)
            {
                if (dt.Kind == DateTimeKind.Utc)
                    return dt.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                return dt.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
            }
            return dt.ToString("O");
        }


        private static string DecodeQuotedString(string quotedString)
        {
            if (quotedString.Length < 2 || quotedString[0] != '\'' || quotedString[quotedString.Length - 1] != '\'')
                throw new ArgumentException("Quoted string needs to be enclosed with the character '");

            // TODO: decode url encoded string
            return quotedString.Substring(1, quotedString.Length - 2);
        }


        private static string DoubleToString(double value)
        {
            // We must always include . to make sure number gets interpreted as double and not int.
            // Yeah, there's probably a more elegant way to do this, but don't care about finding it out right now.
            // This should work.
            return value != (long) value
                       ? value.ToString("R", CultureInfo.InvariantCulture)
                       : string.Format(CultureInfo.InvariantCulture, "{0}.0", (long) value);
        }


        private static string GetJsonTypeName(Type typeOperand)
        {
            var resourceInfoAttribute =
                typeOperand.GetCustomAttributes(typeof (ResourceInfoAttribute), false).
                            OfType<ResourceInfoAttribute>().First();
            var jsonTypeName = resourceInfoAttribute.JsonTypeName;
            return jsonTypeName;
        }


        private static string GetMemberName(MemberInfo member)
        {
            // Do it JSON style camelCase
            return member.Name.Substring(0, 1).ToLower() + member.Name.Substring(1);
        }


        private static object GetMemberValue(object obj, MemberInfo member)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (member == null)
                throw new ArgumentNullException("member");
            var propertyInfo = member as PropertyInfo;
            if (propertyInfo != null)
                return propertyInfo.GetValue(obj, null);

            var fieldInfo = member as FieldInfo;
            if (fieldInfo != null)
                return fieldInfo.GetValue(obj);

            throw new InvalidOperationException("Don't know how to get value from member of type " + member.GetType());
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

            var callExpr = expr as MethodCallExpression;
            if (callExpr != null)
                return BuildFromMethodCallExpression(callExpr);

            var typeBinaryExpression = expr as TypeBinaryExpression;
            if (typeBinaryExpression != null)
                return BuildFromTypeBinaryExpression(typeBinaryExpression);

            var unaryExpression = expr as UnaryExpression;
            if (unaryExpression != null)
                return BuildFromUnaryExpression(unaryExpression);

            var lambdaExpression = expr as LambdaExpression;
            if (lambdaExpression != null)
                return BuildFromLambdaExpression(lambdaExpression);

            var parameterExpression = expr as ParameterExpression;
            if (parameterExpression != null)
            {
                if (parameterExpression == thisParameter)
                    return "this";
                return parameterExpression.Name;
            }

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

            // Detect comparison with enum

            var left = binaryExpr.Left;
            var right = binaryExpr.Right;

            TryDetectAndConvertEnumComparison(ref left, ref right, true);

            return string.Format("({0} {1} {2})", Build(left), opString, Build(right));
        }


        private void TryDetectAndConvertEnumComparison(ref Expression left, ref Expression right, bool tryAgainSwapped)
        {
            var unaryLeft = left as UnaryExpression;
            if (left.Type == typeof (Int32) && unaryLeft != null && left.NodeType == ExpressionType.Convert &&
                unaryLeft.Operand.Type.IsEnum)
            {
                if (right.Type == typeof (Int32) && right.NodeType == ExpressionType.Constant)
                {
                    var rightConstant = (ConstantExpression) right;
                    left = unaryLeft.Operand;
                    right = Expression.Constant(Enum.ToObject(left.Type, (int) rightConstant.Value), left.Type);
                    return;
                }
            }

            if (tryAgainSwapped)
                TryDetectAndConvertEnumComparison(ref right, ref left, false);
        }


        private string BuildFromConstantExpression(ConstantExpression constantExpr)
        {
            var valueType = constantExpr.Type;
            var value = constantExpr.Value;
            return GetEncodedConstant(valueType, value);
        }


        private string BuildFromLambdaExpression(LambdaExpression lambdaExpression)
        {
            if (lambdaExpression.Parameters.Count != 1)
                throw new NotImplementedException("Only supports one parameter in lambda expression for now.");

            var param = lambdaExpression.Parameters[0];
            var predicateBuilder = new QueryPredicateBuilder(lambdaExpression, thisParameter);
            return string.Format("{0}:{1}", param.Name, predicateBuilder.ToString());
        }


        private string BuildFromMemberExpression(MemberExpression memberExpr)
        {
            string odataExpression;
            if (TryMapKnownOdataFunction(
                memberExpr.Member, Enumerable.Repeat(memberExpr.Expression, 1), out odataExpression))
                return odataExpression;

            // This gets weird with closures, see:
            // http://blog.nexterday.com/post/Automatic-compilation-of-Linq-to-SQL-queries.aspx

            object value;
            if (IsClosureMemberAccess(memberExpr, out value))
                return GetEncodedConstant(value.GetType(), value);

            if (memberExpr.Expression != thisParameter)
                return string.Format("{0}.{1}", Build(memberExpr.Expression), GetMemberName(memberExpr.Member));
            return GetMemberName(memberExpr.Member);
        }


        private string BuildFromMethodCallExpression(MethodCallExpression callExpr)
        {
            if (callExpr.Method.MetadataToken == OdataFunctionMapping.DictGetMethod.MetadataToken)
            {
                var quotedKey = Build(callExpr.Arguments[0]);
                var key = DecodeQuotedString(quotedKey);
                /* 
                if (ContainsOnlyValidSymbolCharacters(key))
                    return string.Format("{0}.{1}", Build(callExpr.Object), key);*/
                return string.Format("{0}[{1}]", Build(callExpr.Object), quotedKey);
            }

            string odataExpression;

            // Include this (object) parameter as first argument if not null!
            var args = callExpr.Object != null
                           ? Enumerable.Repeat(callExpr.Object, 1).Concat(callExpr.Arguments)
                           : callExpr.Arguments;

            if (
                !TryMapKnownOdataFunction(callExpr.Method, args, out odataExpression))
            {
                throw new NotImplementedException(
                    "Don't know what to do with method " + callExpr.Method.Name + " declared in "
                    + callExpr.Method.DeclaringType.FullName);
            }

            return odataExpression;
        }


        private string BuildFromTypeBinaryExpression(TypeBinaryExpression typeBinaryExpression)
        {
            switch (typeBinaryExpression.NodeType)
            {
                case ExpressionType.TypeIs:
                    var typeOperand = typeBinaryExpression.TypeOperand;
                    if (!typeOperand.IsInterface || !typeof (IClientResource).IsAssignableFrom(typeOperand))
                    {
                        throw new InvalidOperationException(
                            typeOperand.FullName
                            + " is not an interface and/or does not implement type IClientResource.");
                    }
                    if (typeBinaryExpression.Expression != thisParameter)
                    {
                        throw new NotImplementedException(
                            "Only know how to do TypeIs when target is instance parameter for now..");
                    }

                    // TODO: Proper typename resolving

                    var jsonTypeName = GetJsonTypeName(typeOperand);
                    return string.Format("isof({0})", jsonTypeName);
                default:
                    throw new NotImplementedException(
                        "Don't know how to handle TypeBinaryExpression with NodeType " + typeBinaryExpression.NodeType);
            }
        }


        private string BuildFromUnaryExpression(UnaryExpression unaryExpression)
        {
            switch (unaryExpression.NodeType)
            {
                case ExpressionType.Convert:
                    if (unaryExpression.Operand.Type.IsEnum)
                        return Build(unaryExpression.Operand);

                    if (unaryExpression.Operand != thisParameter)
                        throw new NotImplementedException("Only know how to cast `this` to something else");

                    return "cast(" + GetJsonTypeName(unaryExpression.Type) + ")";
                default:
                    throw new NotImplementedException(
                        "NodeType " + unaryExpression.NodeType + " in UnaryExpression not yet handled.");
            }
        }


        private string EncodeString(string text)
        {
            // TODO: IMPORTANT! Proper encoding!!
            var sb = new StringBuilder();
            sb.Append('\'');

            foreach (var c in text)
            {
                if (c == '\'')
                    sb.Append("''");
                else
                    sb.Append(c);
            }

            sb.Append('\'');
            return sb.ToString();
        }


        private string GetEncodedConstant(Type valueType, object value)
        {
            if (valueType.IsEnum)
            {
                return EncodeString(value.ToString());
            }
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Char:
                    // Note: char will be interpreted as string on other end.
                    return EncodeString(value.ToString());
                case TypeCode.String:
                    return EncodeString((string) value);
                case TypeCode.Int32:
                    return value.ToString();
                case TypeCode.DateTime:
                    return string.Format("datetime'{0}'", DateTimeToString((DateTime) value));
                case TypeCode.Double:
                    return DoubleToString((double) value);
                case TypeCode.Single:
                    return ((float) value).ToString("R", CultureInfo.InvariantCulture) + "f";
                case TypeCode.Decimal:
                    return ((decimal) value).ToString(CultureInfo.InvariantCulture) + "m";
                case TypeCode.Object:
                    if (value == null)
                        return "null";
                    if (value is Guid)
                        return "guid'" + ((Guid) value).ToString() + "'";
                    break;
                case TypeCode.Boolean:
                    return ((bool) value) ? "true" : "false";
                default:
                    break;
            }
            throw new NotImplementedException(
                "Don't know how to send constant of type " + valueType.FullName + " yet..");
        }


        private bool IsClosureMemberAccess(MemberExpression memberExpression, out object value)
        {
            value = null;
            var member = memberExpression.Member;
            if (member.DeclaringType.Name.StartsWith("<>c__") && member.MemberType == MemberTypes.Field)
            {
                var field = (FieldInfo) member;
                var obj = ((ConstantExpression) memberExpression.Expression).Value;

                //Add the value to the extraction list
                value = field.GetValue(obj);
                return true;
            }

            var innerMemberExpr = memberExpression.Expression as MemberExpression;

            if (innerMemberExpr == null)
                return false;

            object innerValue;
            if (IsClosureMemberAccess(innerMemberExpr, out innerValue))
            {
                value = GetMemberValue(innerValue, member);
                return true;
            }

            return false;
        }


        private bool TryMapKnownOdataFunction(
            MemberInfo member, IEnumerable<Expression> arguments, out string odataExpression)
        {
            OdataFunctionMapping.MemberMapping memberMapping;
            if (!OdataFunctionMapping.TryGetMemberMapping(member, out memberMapping))
            {
                odataExpression = null;
                return false;
            }

            var odataArguments = arguments.Select(Build).Cast<object>().ToArray();
            var callFormat = memberMapping.PreferredCallStyle == OdataFunctionMapping.MethodCallStyle.Chained
                                 ? memberMapping.ChainedCallFormat
                                 : memberMapping.StaticCallFormat;

            odataExpression = string.Format(callFormat, odataArguments);
            return true;
        }

        #region Nested type: PreBuildVisitor

        private class PreBuildVisitor : ExpressionVisitor
        {
            private static readonly MethodInfo concatMethod;

            static PreBuildVisitor()
            {
                concatMethod = typeof (string).GetMethod("Concat", new[] {typeof (string), typeof (string)});
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var baseNode = base.VisitMethodCall(node);
                if (baseNode is MethodCallExpression)
                {
                    var mNode = baseNode as MethodCallExpression;
                    if (mNode.Arguments.Where(x => x != null).Any(x => x.NodeType != ExpressionType.Constant))
                        return baseNode;

                    object instance = null;

                    if (mNode.Object != null)
                    {
                        var objectConstExpr = mNode.Object as ConstantExpression;
                        if (objectConstExpr == null)
                            return baseNode;

                        instance = objectConstExpr.Value;
                    }

                    var invokeArgs = mNode.Arguments.Cast<ConstantExpression>().Select(x => x.Value);
                    return Expression.Constant(mNode.Method.Invoke(instance, invokeArgs.ToArray()), mNode.Type);
                }

                return baseNode;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression == null)
                {
                    var propInfo = node.Member as PropertyInfo;
                    if (propInfo != null)
                    {
                        return Expression.Constant(propInfo.GetValue(null, null), node.Type);
                    }
                    var fieldInfo = node.Member as FieldInfo;
                    if (fieldInfo != null)
                    {
                        return Expression.Constant(fieldInfo.GetValue(null), node.Type);
                    }
                }
                return base.VisitMember(node);
            }


            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (node.NodeType == ExpressionType.Add && node.Left.Type == typeof (string)
                    && node.Right.Type == typeof (string))
                    return Expression.Call(concatMethod, Visit(node.Left), Visit(node.Right));
                return base.VisitBinary(node);
            }
        }

        #endregion
    }
}