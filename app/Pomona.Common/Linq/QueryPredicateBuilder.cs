#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq
{
    internal class QueryPredicateBuilder : ExpressionVisitor
    {
        protected static readonly ReadOnlyDictionary<ExpressionType, string> binaryExpressionNodeDict;
        private static readonly Type[] enumUnderlyingTypes = { typeof(byte), typeof(int), typeof(long) };
        private static readonly HashSet<Type> nativeTypes = new HashSet<Type>(TypeUtils.GetNativeTypes());

        private static readonly HashSet<char> validSymbolCharacters =
            new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_");

        private readonly Dictionary<Expression, Expression> builderVisitedNodesCache =
            new Dictionary<Expression, Expression>();

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


        public QueryPredicateBuilder()
        {
        }


        private QueryPredicateBuilder(ParameterExpression thisParameter)
        {
            this.thisParameter = thisParameter;
        }


        protected LambdaExpression RootLambda { get; private set; }

        protected ParameterExpression ThisParameter => this.thisParameter
                                                       ?? (RootLambda != null ? RootLambda.Parameters.FirstOrDefault() : null);


        public override Expression Visit(Expression node)
        {
            if (node == null)
                return null;

            if (node is PomonaExtendedExpression)
                return node;

            Expression visited;
            if (!this.builderVisitedNodesCache.TryGetValue(node, out visited))
            {
                visited = base.Visit(node);
                if (!(visited is PomonaExtendedExpression))
                    visited = NotSupported(node, node.NodeType + " not supported server side.");
                this.builderVisitedNodesCache[node] = visited;
            }

            return visited;
        }


        protected override Expression VisitBinary(BinaryExpression node)
        {
            string opString;
            if (!binaryExpressionNodeDict.TryGetValue(node.NodeType, out opString))
                return NotSupported(node, "BinaryExpression NodeType " + node.NodeType + " not yet handled.");

            // Detect comparison with enum

            var left = node.Left;
            var right = node.Right;

            left = FixBinaryComparisonConversion(left, right);
            right = FixBinaryComparisonConversion(right, left);

            TryDetectAndConvertEnumComparison(ref left, ref right, true);
            TryDetectAndConvertNullableEnumComparison(ref left, ref right, true);

            if (node.NodeType == ExpressionType.NotEqual || node.NodeType == ExpressionType.Equal)
                TryDetectAndConvertEqualBetweenResourcesWithIdProperty(ref left, ref right);

            return Scope(Nodes(node, Visit(left), " ", opString, " ", Visit(right)));
        }


        protected override Expression VisitConditional(ConditionalExpression node)
        {
            return Format(node,
                          "iif({0},{1},{2})",
                          Visit(node.Test),
                          Visit(node.IfTrue),
                          Visit(node.IfFalse));
        }


        protected override Expression VisitConstant(ConstantExpression node)
        {
            var valueType = node.Type;
            var value = node.Value;
            var encodedConstant = GetEncodedConstant(valueType, value);

            if (encodedConstant != null)
                return Terminal(node, encodedConstant, true);

            return NotSupported(node, "Constant of this type not supported.");
        }


        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (node.Parameters.Count != 1)
                return NotSupported(node, "Only supports one parameter in lambda expression for now.");

            if (RootLambda == null)
            {
                try
                {
                    RootLambda = (LambdaExpression)node.Visit<PreBuildVisitor>();
                    return VisitRootLambda((Expression<T>)RootLambda);
                }
                finally
                {
                    RootLambda = null;
                }
            }
            else
            {
                var param = node.Parameters[0];
                var predicateBuilder = new QueryPredicateBuilder(ThisParameter);
                return Format(node, "{0}:{1}", param.Name, predicateBuilder.Visit(node));
            }
        }


        protected override Expression VisitListInit(ListInitExpression node)
        {
            return NotSupported(node, "ListInitExpression not supported by Linq provider.");
        }


        protected override Expression VisitMember(MemberExpression node)
        {
            Expression odataExpression;
            if (TryMapKnownOdataFunction(
                node,
                node.Member,
                Enumerable.Repeat(node.Expression, 1),
                out odataExpression))
                return odataExpression;

            if (node.Expression != ThisParameter)
                return Format(node, "{0}.{1}", Visit(node.Expression), GetMemberName(node.Member));
            return Terminal(node, GetMemberName(node.Member));
        }


        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            return NotSupported(node, node.NodeType + " not supported server side.");
        }


        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.UniqueToken() == QueryFunctionMapping.StringEqualsTakingComparisonTypeMethod.UniqueToken())
                return VisitStringEqualsTakingComparisonTypeCall(node);

            if (node.Method.UniqueToken() == QueryFunctionMapping.EnumerableContainsMethod.UniqueToken())
                return Format(node, "{0} in {1}", Visit(node.Arguments[1]), Visit(node.Arguments[0]));

            if (node.Method.UniqueToken() == QueryFunctionMapping.ListContainsMethod.UniqueToken())
                return Format(node, "{0} in {1}", Visit(node.Arguments[0]), Visit(node.Object));

            if (node.Method.UniqueToken() == QueryFunctionMapping.DictStringStringGetMethod.UniqueToken())
            {
                var quotedKey = Visit(node.Arguments[0]);
                //var key = DecodeQuotedString(quotedKey);
                /* 
                if (ContainsOnlyValidSymbolCharacters(key))
                    return string.Format("{0}.{1}", Build(callExpr.Object), key);*/
                return Format(node, "{0}[{1}]", Visit(node.Object), quotedKey);
            }
            if (node.Method.UniqueToken() == QueryFunctionMapping.SafeGetMethod.UniqueToken())
            {
                var constantKeyExpr = node.Arguments[1] as ConstantExpression;
                if (constantKeyExpr != null && constantKeyExpr.Type == typeof(string) &&
                    IsValidSymbolString((string)constantKeyExpr.Value))
                    return Format(node, "{0}.{1}", Visit(node.Arguments[0]), constantKeyExpr.Value);
            }

            Expression odataExpression;

            // Include this (object) parameter as first argument if not null!
            var args = node.Object != null
                ? Enumerable.Repeat(node.Object, 1).Concat(node.Arguments)
                : node.Arguments;

            if (
                !TryMapKnownOdataFunction(node, node.Method, args, out odataExpression))
            {
                return NotSupported(node,
                                    "Method " + node.Method.Name + " declared in "
                                    + node.Method.DeclaringType.FullName + " is not supported by the Pomona LINQ provider.");
            }

            return odataExpression;
        }


        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            IEnumerable<Expression> arrayElements = node.Expressions;

            if (node.Type == typeof(object[]))
            {
                // Remove redundant boxing converts
                arrayElements =
                    arrayElements.Select(
                        x =>
                            x.NodeType == ExpressionType.Convert && x.Type == typeof(object)
                                ? ((UnaryExpression)x).Operand
                                : x);
            }

            var elements = arrayElements.Select(Visit).Cast<object>().ToArray();
            var format = "["
                         + string.Join(",", Enumerable.Range(0, elements.Length).Select(x => string.Concat("{", x, "}")))
                         + "]";
            return Format(node, format, elements);
        }


        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == ThisParameter)
                return Terminal(node, "this");
            return Terminal(node, node.Name);
        }


        protected virtual Expression VisitRootLambda<T>(Expression<T> node)
        {
            var visitedBody = Visit(node.Body);
            while (visitedBody is QuerySegmentParenScopeExpression)
                visitedBody = ((QuerySegmentParenScopeExpression)visitedBody).Value;
            return visitedBody;
        }


        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.TypeIs:
                    var typeOperand = node.TypeOperand;
                    //if (!typeOperand.IsInterface || !typeof (IClientResource).IsAssignableFrom(typeOperand))
                    //{
                    //    throw new InvalidOperationException(
                    //        typeOperand.FullName
                    //        + " is not an interface and/or does not implement type IClientResource.");
                    //}
                    var jsonTypeName = GetExternalTypeName(typeOperand);
                    if (node.Expression == ThisParameter)
                        return Format(node, "isof({0})", jsonTypeName);
                    else
                    {
                        return Format(node,
                                      "isof({0},{1})",
                                      Visit(node.Expression),
                                      Visit(Expression.Constant(typeOperand)));
                    }

                    // TODO: Proper typename resolving

                default:
                    throw new NotImplementedException(
                        "Don't know how to handle TypeBinaryExpression with NodeType " + node.NodeType);
            }
        }


        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    return Format(node, "not ({0})", Visit(node.Operand));

                case ExpressionType.TypeAs:
                    return Scope(Format(node,
                                        "{0} as {1}",
                                        Visit(node.Operand),
                                        GetExternalTypeName(node.Type)));

                case ExpressionType.Convert:
                    if (node.Operand.Type.IsEnum)
                        return Visit(node.Operand);

                    if (node.Operand == ThisParameter)
                    {
                        return Format(node, "cast({0})", GetExternalTypeName(node.Type));
                        // throw new NotImplementedException("Only know how to cast `this` to something else");
                    }
                    else
                    {
                        return Format(node,
                                      "cast({0},{1})",
                                      Visit(node.Operand),
                                      GetExternalTypeName(node.Type));
                    }

                default:
                    return NotSupported(node,
                                        "NodeType " + node.NodeType + " in UnaryExpression not yet handled.");
            }
        }


        internal static QuerySegmentExpression Format(Expression origNode, string format, params object[] args)
        {
            return Format(origNode, false, format, args);
        }


        internal static QuerySegmentExpression Format(Expression origNode,
                                                      bool localExecutionPreferred,
                                                      string format,
                                                      params object[] args)
        {
            return new QueryFormattedSegmentExpression(origNode.Type, format, args, localExecutionPreferred);
        }


        internal static QuerySegmentListExpression Nodes(Expression origNode, params object[] args)
        {
            return new QuerySegmentListExpression(args, origNode.Type);
        }


        internal static QuerySegmentListExpression Nodes(Expression origNode, IEnumerable<object> args)
        {
            return new QuerySegmentListExpression(args, origNode.Type);
        }


        internal static QuerySegmentExpression Scope(QuerySegmentExpression value)
        {
            return new QuerySegmentParenScopeExpression(value);
        }


        internal static QuerySegmentExpression Terminal(Expression origNode,
                                                        string value,
                                                        bool localExecutionPreferred = false)
        {
            return new QueryTerminalSegmentExpression(value, origNode.Type, localExecutionPreferred);
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


        private Expression FixBinaryComparisonConversion(Expression expr, Expression other)
        {
            var otherIsNull = other.NodeType == ExpressionType.Constant && ((ConstantExpression)other).Value == null;
            if (expr.NodeType == ExpressionType.TypeAs && ((UnaryExpression)expr).Operand.Type == typeof(object)
                && !otherIsNull)
                return ((UnaryExpression)expr).Operand;
            else if (expr.NodeType == ExpressionType.Convert && expr.Type.IsNullable() &&
                     Nullable.GetUnderlyingType(expr.Type) == ((UnaryExpression)expr).Operand.Type)
                return ((UnaryExpression)expr).Operand;
            return expr;
        }


        private string GetEncodedConstant(Type valueType, object value)
        {
            if (value == null)
                return "null";
            var underlyingType = Nullable.GetUnderlyingType(valueType);
            if (underlyingType != null)
                return GetEncodedConstant(underlyingType, value);

            Type enumerableElementType;
            if (valueType != typeof(string) && valueType.TryGetEnumerableElementType(out enumerableElementType))
            {
                // This handles arrays in constant expressions
                var encodedElements = ((IEnumerable)value)
                    .Cast<object>()
                    .Select(x => GetEncodedConstant(enumerableElementType, x))
                    .ToList();

                if (encodedElements.Any(x => x == null))
                    return null;

                var newArrayString = string.Join(",", encodedElements);
                return $"[{newArrayString}]";
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
                case TypeCode.Int64:
                    return $"{value}L";
                case TypeCode.DateTime:
                    return $"datetime'{DateTimeToString((DateTime)value)}'";
                case TypeCode.Double:
                    return DoubleToString((double)value);
                case TypeCode.Single:
                    return ((float)value).ToString("R", CultureInfo.InvariantCulture) + "f";
                case TypeCode.Decimal:
                    return ((decimal)value).ToString(CultureInfo.InvariantCulture) + "m";
                case TypeCode.Object:
                    if (value is Guid)
                        return $"guid'{(Guid)value}'";
                    if (value is Type)
                        return GetExternalTypeName((Type)value);
                    break;
                case TypeCode.Boolean:
                    return (bool)value ? "true" : "false";
                default:
                    break;
            }

            if (typeof(IStringEnum).IsAssignableFrom(valueType))
                return EncodeString(value.ToString());
            return null;
        }


        private string GetExternalTypeName(Type typeOperand)
        {
            var postfixSymbol = string.Empty;
            if (typeOperand.UniqueToken() == typeof(Nullable<>).UniqueToken())
            {
                typeOperand = Nullable.GetUnderlyingType(typeOperand);
                postfixSymbol = "?";
            }

            string typeName;

            if (nativeTypes.Contains(typeOperand))
                typeName = $"{typeOperand.Name}{postfixSymbol}";
            else
            {
                var resourceInfoAttribute =
                    typeOperand.GetCustomAttributes(typeof(ResourceInfoAttribute), false).
                                OfType<ResourceInfoAttribute>().First();
                typeName = resourceInfoAttribute.JsonTypeName;
            }
            return EncodeString(typeName, "t");
        }


        private static Type GetFuncInExpression(Type t)
        {
            Type[] typeArgs;
            if (t.TryExtractTypeArguments(typeof(IQueryable<>), out typeArgs))
                return typeof(IEnumerable<>).MakeGenericType(typeArgs[0]);
            return t.TryExtractTypeArguments(typeof(Expression<>), out typeArgs) ? typeArgs[0] : t;
        }


        private static bool GetInterfaceWithResourceIdProperty(Type type, out PropertyInfo resourceIdProperty)
        {
            resourceIdProperty = null;
            ResourceInfoAttribute ria;
            if (type.TryGetResourceInfoAttribute(out ria))
                resourceIdProperty = ria.IdProperty;

            return resourceIdProperty != null;
        }


        private static string GetMemberName(MemberInfo member)
        {
            // Do it JSON style camelCase
            return member.Name.Substring(0, 1).ToLower() + member.Name.Substring(1);
        }


        private static object GetMemberValue(object obj, MemberInfo member)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (member == null)
                throw new ArgumentNullException(nameof(member));
            var propertyInfo = member as PropertyInfo;
            if (propertyInfo != null)
                return propertyInfo.GetValue(obj, null);

            var fieldInfo = member as FieldInfo;
            if (fieldInfo != null)
                return fieldInfo.GetValue(obj);

            throw new InvalidOperationException("Don't know how to get value from member of type " + member.GetType());
        }


        private static bool IsValidSymbolString(string text)
        {
            var containsOnlyValidSymbolCharacters = text.All(x => validSymbolCharacters.Contains(x));
            return text.Length > 0 && (!char.IsNumber(text[0])) && containsOnlyValidSymbolCharacters;
        }


        private static Expression MakeResourceIdAccess(Expression node, PropertyInfo resourceIdProperty)
        {
            var constantExpression = node as ConstantExpression;
            if (constantExpression == null)
                return Expression.Property(node, resourceIdProperty);

            var value = resourceIdProperty.GetValue(constantExpression.Value, null);
            var propertyType = resourceIdProperty.PropertyType;
            if (value == null && propertyType.IsValueType)
                return NotSupported(node, $"Can't compare {propertyType} with null.");

            return Expression.Constant(value, propertyType);
        }


        private static NotSupportedByProviderExpression NotSupported(Expression node, string message)
        {
            return new NotSupportedByProviderExpression(node, new NotSupportedException(message));
        }


        private static void ReplaceQueryableMethodWithCorrespondingEnumerableMethod(ref MemberInfo member,
                                                                                    ref IEnumerable<Expression>
                                                                                        arguments)
        {
            var firstArg = arguments.First();
            Type[] queryableTypeArgs;
            var method = member as MethodInfo;
            if (method != null && method.IsStatic &&
                firstArg.Type.TryExtractTypeArguments(typeof(IQueryable<>), out queryableTypeArgs))
            {
                // Try to find matching method taking IEnumerable instead
                var wantedArgs =
                    method.GetGenericMethodDefinition()
                          .GetParameters()
                          .Select(x => GetFuncInExpression(x.ParameterType))
                          .ToArray();

                var enumerableMethod =
                    typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                      .Where(x => x.Name == method.Name)
                                      .Select(x => new { parameters = x.GetParameters(), mi = x })
                                      .Where(x => x.parameters.Length == wantedArgs.Length &&
                                                  x.parameters
                                                   .Select(y => y.ParameterType)
                                                   .Zip(wantedArgs, (y, z) => y.IsGenericallyEquivalentTo(z))
                                                   .All(y => y))
                                      .Select(x => x.mi)
                                      .FirstOrDefault();

                if (enumerableMethod != null)
                {
                    arguments =
                        arguments.Select(x => x.NodeType == ExpressionType.Quote ? ((UnaryExpression)x).Operand : x);
                    if (enumerableMethod.IsGenericMethodDefinition)
                        member = enumerableMethod.MakeGenericMethod(((MethodInfo)member).GetGenericArguments());
                    else
                        member = enumerableMethod;
                }
            }
        }


        private void TryDetectAndConvertEnumComparison(ref Expression left, ref Expression right, bool tryAgainSwapped)
        {
            var unaryLeft = left as UnaryExpression;
            var underlyingType = left.Type;
            if (enumUnderlyingTypes.Contains(underlyingType) && unaryLeft != null
                && left.NodeType == ExpressionType.Convert &&
                unaryLeft.Operand.Type.IsEnum)
            {
                if (right.Type == underlyingType && right.NodeType == ExpressionType.Constant)
                {
                    var rightConstant = (ConstantExpression)right;
                    left = unaryLeft.Operand;
                    right = Expression.Constant(Enum.ToObject(unaryLeft.Operand.Type, rightConstant.Value),
                                                unaryLeft.Operand.Type);
                    return;
                }
            }

            if (tryAgainSwapped)
                TryDetectAndConvertEnumComparison(ref right, ref left, false);
        }


        private static void TryDetectAndConvertEqualBetweenResourcesWithIdProperty(ref Expression left,
                                                                                   ref Expression right)
        {
            if (left.Type != right.Type)
                return;

            PropertyInfo resourceIdProperty;
            if (!GetInterfaceWithResourceIdProperty(left.Type, out resourceIdProperty))
                return;

            left = MakeResourceIdAccess(left, resourceIdProperty);
            right = MakeResourceIdAccess(right, resourceIdProperty);
        }


        private void TryDetectAndConvertNullableEnumComparison(ref Expression left,
                                                               ref Expression right,
                                                               bool tryAgainSwapped)
        {
            if (left.Type != right.Type || !left.Type.IsNullable())
                return;
            var leftConvert = left.NodeType == ExpressionType.Convert ? (UnaryExpression)left : null;
            var rightConvert = left.NodeType == ExpressionType.Convert ? (UnaryExpression)right : null;
            var rightConstant = rightConvert != null ? rightConvert.Operand as ConstantExpression : null;
            var enumType = (rightConstant != null && rightConstant.Type.IsEnum) ? rightConstant.Type : null;

            if (leftConvert != null && rightConvert != null && rightConstant != null && enumType != null)
            {
                left = leftConvert.Operand;
                right = rightConstant;
                return;
            }

            if (tryAgainSwapped)
                TryDetectAndConvertNullableEnumComparison(ref right, ref left, false);
        }


        private bool TryMapKnownOdataFunction(Expression origNode,
                                              MemberInfo member,
                                              IEnumerable<Expression> arguments,
                                              out Expression odataExpression)
        {
            ReplaceQueryableMethodWithCorrespondingEnumerableMethod(ref member, ref arguments);

            QueryFunctionMapping.MemberMapping memberMapping;
            if (!QueryFunctionMapping.TryGetMemberMapping(member, out memberMapping))
            {
                odataExpression = null;
                return false;
            }

            var odataArguments = arguments.Select(Visit).Cast<object>().ToArray();
            var callFormat = memberMapping.PreferredCallStyle == QueryFunctionMapping.MethodCallStyle.Chained
                ? memberMapping.ChainedCallFormat
                : memberMapping.StaticCallFormat;

            odataExpression = Format(origNode, memberMapping.LocalExecutionPreferred, callFormat, odataArguments);
            return true;
        }


        private Expression VisitStringEqualsTakingComparisonTypeCall(MethodCallExpression node)
        {
            var compTypeConstant = node.Arguments[2] as ConstantExpression;
            if (compTypeConstant == null)
            {
                return NotSupported(node,
                                    "String.Equals taking 3 arguments is only supported when ComparisonType is constant.");
            }

            string opString;

            switch ((StringComparison)compTypeConstant.Value)
            {
                case StringComparison.CurrentCultureIgnoreCase:
                case StringComparison.InvariantCultureIgnoreCase:
                case StringComparison.OrdinalIgnoreCase:
                    opString = "ieq";
                    break;
                default:
                    opString = "eq";
                    break;
            }

            return Format(node, "{0} {1} {2}", Visit(node.Arguments[0]), opString, Visit(node.Arguments[1]));
        }

        #region Nested type: PreBuildVisitor

        private class PreBuildVisitor : EvaluateClosureMemberVisitor
        {
            private static readonly MethodInfo concatMethod;


            static PreBuildVisitor()
            {
                concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
            }


            protected override Expression VisitBinary(BinaryExpression node)
            {
                // Constant folding
                var left = Visit(node.Left);
                var right = Visit(node.Right);

                if (left.NodeType == ExpressionType.Constant
                    && right.NodeType == ExpressionType.Constant
                    && left.Type == right.Type
                    && IsFoldedType(left.Type)
                    && (node.Method == null || node.Method.DeclaringType == left.Type))
                {
                    return Expression.Constant(
                        Expression.Lambda(node, Enumerable.Empty<ParameterExpression>()).Compile().DynamicInvoke(
                            null),
                        node.Type);
                }

                if (node.NodeType == ExpressionType.Add && left.Type == typeof(string)
                    && right.Type == typeof(string))
                    return Expression.Call(concatMethod, left, right);
                return base.VisitBinary(node);
            }


            protected override Expression VisitExtension(Expression node)
            {
                if (node is PomonaExtendedExpression)
                    return node;
                return base.VisitExtension(node);
            }


            protected override Expression VisitListInit(ListInitExpression node)
            {
                // Avoid visiting NewExpression directly, we don't want constant folding in this particular case.
                var visitedInitializers = node.Initializers.Select(x => Expression.ElementInit(x.AddMethod, Visit(x.Arguments)));
                return Expression.ListInit(VisitNewExpressionNoConstantFolding(node.NewExpression), visitedInitializers);
            }


            protected override Expression VisitMemberInit(MemberInitExpression node)
            {
                // Avoid visiting NewExpression directly, we don't want constant folding in this particular case.
                return Expression.MemberInit(VisitNewExpressionNoConstantFolding(node.NewExpression),
                                             Visit(node.Bindings, VisitMemberBinding));
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


            protected override Expression VisitNew(NewExpression node)
            {
                var baseNode = base.VisitNew(node);
                if (baseNode is NewExpression)
                {
                    var nNode = baseNode as NewExpression;
                    if (nNode.Arguments.Where(x => x != null).Any(x => x.NodeType != ExpressionType.Constant))
                        return baseNode;

                    var invokeArgs = nNode.Arguments.Cast<ConstantExpression>().Select(x => x.Value);
                    return Expression.Constant(nNode.Constructor.Invoke(invokeArgs.ToArray()));
                }
                return baseNode;
            }


            protected override Expression VisitUnary(UnaryExpression node)
            {
                var visitedNode = base.VisitUnary(node);
                node = visitedNode as UnaryExpression;
                if (node != null)
                {
                    var operandAsConstant = node.Operand as ConstantExpression;
                    var nullableUnderlyingType = Nullable.GetUnderlyingType(node.Type);

                    if (enumUnderlyingTypes.Contains(nullableUnderlyingType) && operandAsConstant != null)
                    {
                        var enumNonNullableType = Nullable.GetUnderlyingType(operandAsConstant.Type);
                        if (enumNonNullableType != null && enumNonNullableType.IsEnum && operandAsConstant.Value != null)
                            return Expression.Convert(Expression.Constant(operandAsConstant.Value, enumNonNullableType), node.Type);
                    }

                    if (node.NodeType == ExpressionType.Convert && operandAsConstant != null
                        && operandAsConstant.Type == nullableUnderlyingType)
                        return Expression.Constant(operandAsConstant.Value, node.Type);
                    return node;
                }
                return visitedNode;
            }


            private bool IsFoldedType(Type type)
            {
                return type == typeof(int) || type == typeof(decimal);
            }


            private NewExpression VisitNewExpressionNoConstantFolding(NewExpression newExpression)
            {
                var visitedArguments = Visit(newExpression.Arguments);
                var visitedNewExpression = Expression.New(newExpression.Constructor, visitedArguments);
                return visitedNewExpression;
            }
        }

        #endregion
    }
}