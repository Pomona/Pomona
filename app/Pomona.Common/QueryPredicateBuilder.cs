#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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
using System.Collections;
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

        private static readonly HashSet<char> validSymbolCharacters =
            new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_");

        private static readonly HashSet<Type> nativeTypes = new HashSet<Type>(TypeUtils.GetNativeTypes());

        private readonly LambdaExpression lambda;
        private readonly ParameterExpression thisParameter;


        static QueryPredicateBuilder()
        {
            var binExprDict = new Dictionary<ExpressionType, string>
                {
                    { ExpressionType.AndAlso, "and" },
                    { ExpressionType.OrElse, "or" },
                    { ExpressionType.Equal, "eq" },
                    { ExpressionType.NotEqual, "ne" },
                    { ExpressionType.GreaterThan, "gt" },
                    { ExpressionType.GreaterThanOrEqual, "ge" },
                    { ExpressionType.LessThan, "lt" },
                    { ExpressionType.LessThanOrEqual, "le" },
                    { ExpressionType.Subtract, "sub" },
                    { ExpressionType.Add, "add" },
                    { ExpressionType.Multiply, "mul" },
                    { ExpressionType.Divide, "div" },
                    { ExpressionType.Modulo, "mod" }
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


        private static bool IsValidSymbolString(string text)
        {
            var containsOnlyValidSymbolCharacters = text.All(x => validSymbolCharacters.Contains(x));
            return text.Length > 0 && (!char.IsNumber(text[0])) && containsOnlyValidSymbolCharacters;
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
            return value != (long)value
                       ? value.ToString("R", CultureInfo.InvariantCulture)
                       : string.Format(CultureInfo.InvariantCulture, "{0}.0", (long)value);
        }

        private string GetExternalTypeName(Type typeOperand)
        {
            var postfixSymbol = string.Empty;
            if (typeOperand.UniqueToken() == typeof (Nullable<>).UniqueToken())
            {
                typeOperand = Nullable.GetUnderlyingType(typeOperand);
                postfixSymbol = "?";
            }

            string typeName;

            if (nativeTypes.Contains(typeOperand))
                typeName = string.Format("{0}{1}", typeOperand.Name, postfixSymbol);
            else
            {
                var resourceInfoAttribute =
                    typeOperand.GetCustomAttributes(typeof (ResourceInfoAttribute), false).
                                OfType<ResourceInfoAttribute>().First();
                typeName = resourceInfoAttribute.JsonTypeName;
            }
            return EncodeString(typeName, "t");
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

            var conditionalExpression = expr as ConditionalExpression;
            if (conditionalExpression != null)
                return BuildFromConditionalExpression(conditionalExpression);

            var parameterExpression = expr as ParameterExpression;
            if (parameterExpression != null)
            {
                if (parameterExpression == thisParameter)
                    return "this";
                return parameterExpression.Name;
            }

            var newArrayExpression = expr as NewArrayExpression;
            if (newArrayExpression != null)
                return BuildFromNewArrayExpression(newArrayExpression);

            throw new NotImplementedException("NodeType " + expr.NodeType + " not yet handled.");
        }

        private string BuildFromConditionalExpression(ConditionalExpression conditionalExpression)
        {
            return string.Format("iif({0},{1},{2})", Build(conditionalExpression.Test),
                                 Build(conditionalExpression.IfTrue),
                                 Build(conditionalExpression.IfFalse));
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

            left = FixBinaryComparisonConversion(left);
            right = FixBinaryComparisonConversion(right);

            TryDetectAndConvertEnumComparison(ref left, ref right, true);
            TryDetectAndConvertNullableEnumComparison(ref left, ref right, true);

            return string.Format("({0} {1} {2})", Build(left), opString, Build(right));
        }

        private Expression FixBinaryComparisonConversion(Expression expr)
        {
            if (expr.NodeType == ExpressionType.TypeAs && ((UnaryExpression)expr).Operand.Type == typeof (object))
            {
                return ((UnaryExpression)expr).Operand;
            }
            else if (expr.NodeType == ExpressionType.Convert && expr.Type.IsNullable() &&
                     Nullable.GetUnderlyingType(expr.Type) == ((UnaryExpression)expr).Operand.Type)
            {
                return ((UnaryExpression)expr).Operand;
            }
            return expr;
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
            return string.Format("{0}:{1}", param.Name, predicateBuilder);
        }


        private string BuildFromMemberExpression(MemberExpression memberExpr)
        {
            string odataExpression;
            if (TryMapKnownOdataFunction(
                memberExpr.Member, Enumerable.Repeat(memberExpr.Expression, 1), out odataExpression))
                return odataExpression;

            if (memberExpr.Expression != thisParameter)
                return string.Format("{0}.{1}", Build(memberExpr.Expression), GetMemberName(memberExpr.Member));
            return GetMemberName(memberExpr.Member);
        }


        private string BuildFromMethodCallExpression(MethodCallExpression callExpr)
        {
            if (callExpr.Method.UniqueToken() == OdataFunctionMapping.EnumerableContainsMethod.UniqueToken())
                return Build(callExpr.Arguments[1]) + " in " + Build(callExpr.Arguments[0]);

            if (callExpr.Method.UniqueToken() == OdataFunctionMapping.ListContainsMethod.UniqueToken())
                return Build(callExpr.Arguments[0]) + " in " + Build(callExpr.Object);

            if (callExpr.Method.UniqueToken() == OdataFunctionMapping.DictStringStringGetMethod.UniqueToken())
            {
                var quotedKey = Build(callExpr.Arguments[0]);
                var key = DecodeQuotedString(quotedKey);
                /* 
                if (ContainsOnlyValidSymbolCharacters(key))
                    return string.Format("{0}.{1}", Build(callExpr.Object), key);*/
                return string.Format("{0}[{1}]", Build(callExpr.Object), quotedKey);
            }
            if (callExpr.Method.UniqueToken() == OdataFunctionMapping.SafeGetMethod.UniqueToken())
            {
                var constantKeyExpr = callExpr.Arguments[1] as ConstantExpression;
                if (constantKeyExpr != null && constantKeyExpr.Type == typeof (string) &&
                    IsValidSymbolString((string)constantKeyExpr.Value))
                {
                    return string.Format("{0}.{1}", Build(callExpr.Arguments[0]), constantKeyExpr.Value);
                }
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


        private string BuildFromNewArrayExpression(NewArrayExpression newArrayExpression)
        {
            return string.Format("[{0}]", string.Join(",", newArrayExpression.Expressions.Select(Build)));
        }


        private string BuildFromTypeBinaryExpression(TypeBinaryExpression typeBinaryExpression)
        {
            switch (typeBinaryExpression.NodeType)
            {
                case ExpressionType.TypeIs:
                    var typeOperand = typeBinaryExpression.TypeOperand;
                    //if (!typeOperand.IsInterface || !typeof (IClientResource).IsAssignableFrom(typeOperand))
                    //{
                    //    throw new InvalidOperationException(
                    //        typeOperand.FullName
                    //        + " is not an interface and/or does not implement type IClientResource.");
                    //}
                    var jsonTypeName = GetExternalTypeName(typeOperand);
                    if (typeBinaryExpression.Expression == thisParameter)
                    {
                        return string.Format("isof({0})", jsonTypeName);
                    }
                    else
                    {
                        return string.Format("isof({0},{1})", Build(typeBinaryExpression.Expression),
                                             Build(Expression.Constant(typeOperand)));
                    }

                    // TODO: Proper typename resolving

                default:
                    throw new NotImplementedException(
                        "Don't know how to handle TypeBinaryExpression with NodeType " + typeBinaryExpression.NodeType);
            }
        }


        private string BuildFromUnaryExpression(UnaryExpression unaryExpression)
        {
            switch (unaryExpression.NodeType)
            {
                case ExpressionType.Not:
                    return string.Format("not ({0})", Build(unaryExpression.Operand));

                case ExpressionType.TypeAs:
                    return string.Format("({0} as {1})", Build(unaryExpression.Operand),
                                         GetExternalTypeName(unaryExpression.Type));

                case ExpressionType.Convert:
                    if (unaryExpression.Operand.Type.IsEnum)
                        return Build(unaryExpression.Operand);

                    if (unaryExpression.Operand == thisParameter)
                    {
                        return string.Format("cast({0})", GetExternalTypeName(unaryExpression.Type));
                        // throw new NotImplementedException("Only know how to cast `this` to something else");
                    }
                    else
                    {
                        return string.Format("cast({0},{1})", Build(unaryExpression.Operand),
                                             GetExternalTypeName(unaryExpression.Type));
                    }

                default:
                    throw new NotImplementedException(
                        "NodeType " + unaryExpression.NodeType + " in UnaryExpression not yet handled.");
            }
        }


        private string EncodeString(string text, string prefix = "")
        {
            // TODO: IMPORTANT! Proper encoding!!
            var sb = new StringBuilder();
            sb.Append(prefix);
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
            Type enumerableElementType;
            if (valueType != typeof (string) && valueType.TryGetEnumerableElementType(out enumerableElementType))
            {
                // This handles arrays in constant expressions
                var elements = string
                    .Join(
                        ",",
                        ((IEnumerable)value)
                            .Cast<object>()
                            .Select(x => GetEncodedConstant(enumerableElementType, x)));

                return string.Format("[{0}]", elements);
            }

            if (valueType.IsEnum)
                return EncodeString(value.ToString());
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Char:
                    // Note: char will be interpreted as string on other end.
                    return EncodeString(value.ToString());
                case TypeCode.String:
                    return EncodeString((string)value);
                case TypeCode.Int32:
                    return value.ToString();
                case TypeCode.DateTime:
                    return string.Format("datetime'{0}'", DateTimeToString((DateTime)value));
                case TypeCode.Double:
                    return DoubleToString((double)value);
                case TypeCode.Single:
                    return ((float)value).ToString("R", CultureInfo.InvariantCulture) + "f";
                case TypeCode.Decimal:
                    return ((decimal)value).ToString(CultureInfo.InvariantCulture) + "m";
                case TypeCode.Object:
                    if (value == null)
                        return "null";
                    if (value is Guid)
                        return string.Format("guid'{0}'", ((Guid)value));
                    if (value is Type)
                        return GetExternalTypeName((Type)value);
                    break;
                case TypeCode.Boolean:
                    return ((bool)value) ? "true" : "false";
                default:
                    break;
            }
            throw new NotImplementedException(
                "Don't know how to send constant of type " + valueType.FullName + " yet..");
        }

        private static readonly Type[] enumUnderlyingTypes = { typeof(byte), typeof(int), typeof(long) };

        private void TryDetectAndConvertEnumComparison(ref Expression left, ref Expression right, bool tryAgainSwapped)
        {
            var unaryLeft = left as UnaryExpression;
            Type underlyingType = left.Type;
            if (enumUnderlyingTypes.Contains(underlyingType) && unaryLeft != null && left.NodeType == ExpressionType.Convert &&
                unaryLeft.Operand.Type.IsEnum)
            {
                if (right.Type == underlyingType && right.NodeType == ExpressionType.Constant)
                {
                    var rightConstant = (ConstantExpression)right;
                    left = unaryLeft.Operand;
                    right = Expression.Constant(Enum.ToObject(unaryLeft.Operand.Type, rightConstant.Value), unaryLeft.Operand.Type);
                    return;
                }
            }

            if (tryAgainSwapped)
                TryDetectAndConvertEnumComparison(ref right, ref left, false);
        }

        private void TryDetectAndConvertNullableEnumComparison(ref Expression left, ref Expression right, bool tryAgainSwapped)
        {
            if (left.Type != right.Type || !left.Type.IsNullable())
                return;
            var leftConvert = left.NodeType == ExpressionType.Convert ? (UnaryExpression)left : null;
            var rightConvert = left.NodeType == ExpressionType.Convert ? (UnaryExpression)right : null;
            var rightConstant = rightConvert != null ? rightConvert.Operand as ConstantExpression : null;
            var enumType = rightConstant != null && rightConstant.Type.IsEnum ? rightConstant.Type : null;

            if (leftConvert != null && rightConvert != null && rightConstant != null && enumType != null)
            {
                left = leftConvert.Operand;
                right = rightConstant;
                return;
            }

            if (tryAgainSwapped)
                TryDetectAndConvertNullableEnumComparison(ref right, ref left, false);
        }

        private static Type GetFuncInExpression(Type t)
        {
            Type[] typeArgs;
            if (t.TryExtractTypeArguments(typeof (IQueryable<>), out typeArgs))
            {
                return typeof (IEnumerable<>).MakeGenericType(typeArgs[0]);
            }
            return t.TryExtractTypeArguments(typeof (Expression<>), out typeArgs) ? typeArgs[0] : t;
        }

        private bool TryMapKnownOdataFunction(
            MemberInfo member, IEnumerable<Expression> arguments, out string odataExpression)
        {
            ReplaceQueryableMethodWithCorrespondingEnumerableMethod(ref member, ref arguments);

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

        private static void ReplaceQueryableMethodWithCorrespondingEnumerableMethod(ref MemberInfo member,
                                                                                    ref IEnumerable<Expression>
                                                                                        arguments)
        {
            var firstArg = arguments.First();
            Type[] queryableTypeArgs;
            var method = member as MethodInfo;
            if (method != null && method.IsStatic &&
                firstArg.Type.TryExtractTypeArguments(typeof (IQueryable<>), out queryableTypeArgs))
            {
                // Try to find matching method taking IEnumerable instead
                var wantedArgs =
                    method.GetGenericMethodDefinition()
                          .GetParameters()
                          .Select(x => GetFuncInExpression(x.ParameterType))
                          .ToArray();

                var enumerableMethod =
                    typeof (Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                       .FirstOrDefault(x => x.Name == method.Name &&
                                                            x.GetParameters()
                                                             .Select(y => y.ParameterType)
                                                             .Zip(wantedArgs, (y, z) => y.IsGenericallyEquivalentTo(z))
                                                             .All(y => y));

                if (enumerableMethod != null)
                {
                    arguments =
                        arguments.Select(x => x.NodeType == ExpressionType.Quote ? ((UnaryExpression)x).Operand : x);
                    if (enumerableMethod.IsGenericMethodDefinition)
                    {
                        member = enumerableMethod.MakeGenericMethod(((MethodInfo)member).GetGenericArguments());
                    }
                    else
                    {
                        member = enumerableMethod;
                    }
                }
            }
        }

        #region Nested type: PreBuildVisitor

        private class PreBuildVisitor : EvaluateClosureMemberVisitor
        {
            private static readonly MethodInfo concatMethod;


            static PreBuildVisitor()
            {
                concatMethod = typeof (string).GetMethod("Concat", new[] { typeof (string), typeof (string) });
            }


            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (node.NodeType == ExpressionType.Add && node.Left.Type == typeof (string)
                    && node.Right.Type == typeof (string))
                    return Expression.Call(concatMethod, Visit(node.Left), Visit(node.Right));
                return base.VisitBinary(node);
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
        }

        #endregion
    }
}