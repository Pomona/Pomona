#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Queries
{
    internal class NodeTreeToExpressionConverter
    {
        private readonly string parsedString;
        private readonly IQueryTypeResolver propertyResolver;
        private Dictionary<string, ParameterExpression> parameters;
        private ParameterExpression thisParam;


        public NodeTreeToExpressionConverter(IQueryTypeResolver propertyResolver, string parsedString)
        {
            if (propertyResolver == null)
                throw new ArgumentNullException(nameof(propertyResolver));
            this.propertyResolver = propertyResolver;
            this.parsedString = parsedString;
        }


        public Expression ParseExpression(NodeBase node)
        {
            return ParseExpression(node, null, null);
        }


        public Expression ParseExpression(NodeBase node, Expression memberExpression, Type expectedType)
        {
            try
            {
                return ParseExpressionUnsafe(node, memberExpression, expectedType);
            }
            catch (QueryParseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw CreateParseException(node, "Unexpected parse error!", ex);
            }
        }


        public LambdaExpression ToLambdaExpression(Type thisType, NodeBase node)
        {
            var param = Expression.Parameter(thisType, "_this");
            return ToLambdaExpression(param, param.WrapAsEnumerable(), null, node);
        }


        public LambdaExpression ToLambdaExpression(
            ParameterExpression thisParam,
            IEnumerable<ParameterExpression> lamdbaParameters,
            IEnumerable<ParameterExpression> outerParameters,
            NodeBase node)
        {
            if (thisParam == null)
                throw new ArgumentNullException(nameof(thisParam));
            if (lamdbaParameters == null)
                throw new ArgumentNullException(nameof(lamdbaParameters));
            try
            {
                this.thisParam = thisParam;
                this.parameters =
                    lamdbaParameters
                        .Where(x => x != thisParam)
                        .Concat(outerParameters ?? Enumerable.Empty<ParameterExpression>())
                        .ToDictionary(x => x.Name, x => x);

                return Expression.Lambda(ParseExpression(node), lamdbaParameters);
            }
            finally
            {
                this.thisParam = null;
            }
        }


        private Exception CreateParseException(NodeBase node,
                                               string message,
                                               Exception innerException = null,
                                               QueryParseErrorReason? errorReason = null,
                                               string memberName = null)
        {
            if (node != null && node.ParserNode != null && this.parsedString != null)
            {
                return QueryParseException.Create(node.ParserNode,
                                                  message,
                                                  this.parsedString,
                                                  innerException,
                                                  errorReason,
                                                  memberName);
            }

            return new QueryParseException(message, innerException, QueryParseErrorReason.GenericError, null);
        }


        private Expression MakePropertyAccess(SymbolNode currentNode, Expression target, string expressionPath = null)
        {
            expressionPath = expressionPath ?? currentNode.Name;
            StructuredProperty prop;
            if (!this.propertyResolver.TryResolveProperty(target.Type, expressionPath, out prop))
                throw CreateParseException(currentNode, "Property not recognized.");

            if (!prop.Flags.HasFlag(PropertyFlags.AllowsFiltering) || !prop.AccessMode.HasFlag(HttpMethod.Get))
            {
                throw CreateParseException(currentNode,
                                           "Property " + prop.JsonName + " is not allowed for query.",
                                           errorReason : QueryParseErrorReason.MemberNotAllowedInQuery,
                                           memberName : prop.JsonName);
            }

            return prop.CreateGetterExpression(target);
        }


        private Expression ParseArrayLiteral(ArrayNode node, Type expectedElementType = null)
        {
            var arrayElements = node.Children.Select(x => ParseExpression(x, this.thisParam, expectedElementType)).ToList();

            if (arrayElements.Count == 0 && expectedElementType == null)
                throw new NotSupportedException("Does not support empty arrays.");

            var elementType = expectedElementType ?? typeof(object);

            // TODO: Check that all array members are of same type

            if (arrayElements.All(x => x is ConstantExpression))
            {
                var array = Array.CreateInstance(elementType, arrayElements.Count);
                var index = 0;
                foreach (var elementValue in arrayElements.OfType<ConstantExpression>().Select(x => x.Value))
                    array.SetValue(elementValue, index++);
                return Expression.Constant(array);
            }

            // Box value elements
            if (elementType == typeof(object))
            {
                arrayElements =
                    arrayElements.Select(x => x.Type.IsValueType ? Expression.Convert(x, elementType) : x).ToList();
            }

            return Expression.NewArrayInit(elementType, arrayElements);
        }


        private Expression ParseBinaryOperator(BinaryOperatorNode binaryOperatorNode, Expression memberExpression)
        {
            var rightNode = binaryOperatorNode.Right;
            var leftNode = binaryOperatorNode.Left;

            if (binaryOperatorNode.NodeType == NodeType.Dot)
            {
                if (rightNode.NodeType == NodeType.MethodCall)
                {
                    var origCallNode = (MethodCallNode)rightNode;
                    // Rewrite extension method call to static method call of tree:
                    // We do this by taking inserting the first node before arg nodes of extension method call.
                    var staticMethodArgs = leftNode.WrapAsEnumerable()
                                                   .Concat(rightNode.Children);
                    var staticMethodCall = new MethodCallNode(origCallNode.Name, staticMethodArgs);

                    return ParseExpression(staticMethodCall);
                }
                var left = ParseExpression(leftNode);
                return ParseExpression(rightNode, left, null);
            }

            if (binaryOperatorNode.NodeType == NodeType.As)
            {
                if (rightNode.NodeType != NodeType.TypeNameLiteral)
                    throw CreateParseException(binaryOperatorNode, "Right side of as operator is required to be a type literal.");

                return Expression.TypeAs(ParseExpression(leftNode),
                                         ResolveType((TypeNameNode)rightNode));
            }
            if (binaryOperatorNode.NodeType == NodeType.In)
                return ParseInOperator(binaryOperatorNode);

            var leftChild = ParseExpression(leftNode);
            var rightChild = ParseExpression(rightNode);

            PeformImplicitConversionForNodePair(ref leftChild, ref rightChild);

            switch (binaryOperatorNode.NodeType)
            {
                case NodeType.AndAlso:
                    return Expression.AndAlso(leftChild, rightChild);
                case NodeType.OrElse:
                    return Expression.OrElse(leftChild, rightChild);
                case NodeType.Add:
                    return Expression.Add(leftChild, rightChild);
                case NodeType.Subtract:
                    return Expression.Subtract(leftChild, rightChild);
                case NodeType.Multiply:
                    return Expression.Multiply(leftChild, rightChild);
                case NodeType.Modulo:
                    return Expression.Modulo(leftChild, rightChild);
                case NodeType.Divide:
                    return Expression.Divide(leftChild, rightChild);
                case NodeType.Equal:
                    return ParseBinaryOperator(leftChild, rightChild, ExpressionType.Equal);
                case NodeType.LessThan:
                    return Expression.LessThan(leftChild, rightChild);
                case NodeType.GreaterThan:
                    return Expression.GreaterThan(leftChild, rightChild);
                case NodeType.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(leftChild, rightChild);
                case NodeType.LessThanOrEqual:
                    return Expression.LessThanOrEqual(leftChild, rightChild);
                case NodeType.NotEqual:
                    return ParseBinaryOperator(leftChild, rightChild, ExpressionType.NotEqual);
                case NodeType.CaseInsensitiveEqual:
                    return ParseCaseInsensitiveEqualOperator(leftChild, rightChild);
                default:
                    throw new NotImplementedException(
                        "Don't know how to handle node type " + binaryOperatorNode.NodeType);
            }
        }


        private Expression ParseBinaryOperator(Expression leftChild, Expression rightChild, ExpressionType expressionType)
        {
            return ParseBinaryOperator(leftChild,
                                       rightChild,
                                       (l, r) => Expression.MakeBinary(expressionType, l, r));
        }


        private Expression ParseBinaryOperator(Expression leftChild,
                                               Expression rightChild,
                                               Func<Expression, Expression, Expression> exprCreator)
        {
            TryDetectAndConvertEnumComparison(ref leftChild, ref rightChild, true);
            TryDetectAndConvertNullableEnumComparison(ref leftChild, ref rightChild, true);
            return exprCreator(leftChild, rightChild);
        }


        private Expression ParseCaseInsensitiveEqualOperator(Expression leftChild, Expression rightChild)
        {
            return ParseBinaryOperator(leftChild,
                                       rightChild,
                                       (l, r) =>
                                           Expression.Call(OdataFunctionMapping.StringEqualsTakingComparisonTypeMethod,
                                                           l,
                                                           r,
                                                           Expression.Constant(StringComparison.InvariantCultureIgnoreCase)));
        }


        private void ParseConditionalOperator(MethodCallNode node, ref Expression expression)
        {
            if (node.Children.Count != 3)
            {
                throw CreateParseException(node,
                                           "Conditional requires three arguments: iif(test, iftrue, iffalse).");
            }

            var testExpr = ParseExpression(node.Children[0]);
            var ifTrue = ParseExpression(node.Children[1]);
            var ifFalse = ParseExpression(node.Children[2]);

            if (ifTrue.Type != ifFalse.Type)
                PeformImplicitConversionForNodePair(ref ifTrue, ref ifFalse);

            expression = Expression.Condition(testExpr,
                                              ifTrue,
                                              ifFalse);
        }


        private Expression ParseExpressionUnsafe(NodeBase node, Expression memberExpression, Type expectedType)
        {
            if (memberExpression == null)
                memberExpression = this.thisParam;

            if (node.NodeType == NodeType.ArrayLiteral)
                return ParseArrayLiteral((ArrayNode)node, expectedType != null ? expectedType.GetElementType() : null);

            if (node.NodeType == NodeType.MethodCall)
                return ParseMethodCallNode((MethodCallNode)node, memberExpression);

            if (node.NodeType == NodeType.IndexerAccess)
                return ParseIndexerAccessNode((IndexerAccessNode)node, memberExpression);

            if (node.NodeType == NodeType.Symbol)
            {
                var dictionaryInterface =
                    memberExpression.Type.GetInterfacesOfGeneric(typeof(IDictionary<,>)).FirstOrDefault();
                if (node.Children.Count == 0 && dictionaryInterface != null)
                    return ResolveDictionaryAccess((SymbolNode)node, memberExpression, dictionaryInterface);

                return ResolveSymbolNode((SymbolNode)node, memberExpression, expectedType);
            }

            var binaryOperatorNode = node as BinaryOperatorNode;

            if (binaryOperatorNode != null)
                return ParseBinaryOperator(binaryOperatorNode, memberExpression);

            if (node.NodeType == NodeType.GuidLiteral)
            {
                var guidNode = (GuidNode)node;
                return Expression.Constant(guidNode.Value);
            }

            if (node.NodeType == NodeType.DateTimeLiteral)
            {
                var dateTimeNode = (DateTimeNode)node;
                return Expression.Constant(dateTimeNode.Value);
            }

            if (node.NodeType == NodeType.StringLiteral)
            {
                var stringNode = (StringNode)node;

                if (expectedType != null && !expectedType.IsAssignableFrom(typeof(string)))
                {
                    if (expectedType.IsEnum)
                        return Expression.Constant(Enum.Parse(expectedType, stringNode.Value, true));
                    throw CreateParseException(stringNode,
                                               "Don't know what to do with string node when expected type is " +
                                               expectedType.FullName);
                }
                return Expression.Constant(stringNode.Value);
            }

            if (node.NodeType == NodeType.NumberLiteral)
            {
                var intNode = (NumberNode)node;
                return Expression.Constant(intNode.Parse());
            }

            if (node.NodeType == NodeType.TypeNameLiteral)
            {
                var typeNameNode = (TypeNameNode)node;
                return Expression.Constant(ResolveType(typeNameNode), typeof(Type));
            }

            if (node.NodeType == NodeType.Lambda)
            {
                var lambdaNode = (LambdaNode)node;
                return ParseLambda(lambdaNode, memberExpression, expectedType);
            }

            if (node.NodeType == NodeType.Not)
            {
                var notNode = (NotNode)node;
                return Expression.Not(ParseExpression(notNode.Children[0]));
            }

            throw new NotImplementedException();
        }


        private Expression ParseIndexerAccessNode(IndexerAccessNode node, Expression memberExpression)
        {
            var property = MakePropertyAccess(node, memberExpression);
            if (typeof(IDictionary<string, string>).IsAssignableFrom(property.Type))
            {
                return Expression.Call(
                    property, OdataFunctionMapping.DictStringStringGetMethod, ParseExpression(node.Children[0]));
            }
            throw new NotImplementedException();
        }


        private Expression ParseInOperator(BinaryOperatorNode node)
        {
            var leftExpr = ParseExpression(node.Left);

            var rightExpr = ParseExpression(node.Right, null, leftExpr.Type.MakeArrayType());

            var arrayElementType = rightExpr.Type.GetElementType();
            var compareType = arrayElementType;
            if (leftExpr.Type != arrayElementType)
            {
                if (leftExpr.Type == typeof(object))
                {
                    if (compareType.IsValueType && !compareType.IsNullable())
                    {
                        compareType = typeof(Nullable<>).MakeGenericType(arrayElementType);
                        if (rightExpr.NodeType == ExpressionType.Constant)
                        {
                            // Recreate array with nullable type..
                            var sourceArray =
                                ((IEnumerable)((ConstantExpression)rightExpr).Value).Cast<object>().ToArray();
                            var destArray = Array.CreateInstance(compareType, sourceArray.Length);
                            for (var i = 0; i < sourceArray.Length; i++)
                                destArray.SetValue(sourceArray[i], i);
                            rightExpr = Expression.Constant(destArray);
                        }
                        else if (rightExpr.NodeType == ExpressionType.NewArrayInit)
                        {
                            // Recreate array using Convert
                            rightExpr = Expression.NewArrayInit(compareType,
                                                                ((NewArrayExpression)rightExpr).Expressions.Select(
                                                                    x => Expression.Convert(x, compareType)));
                        }
                        else
                        {
                            // Have no idea how to do this
                            throw CreateParseException(node,
                                                       "Using in only works for constant arrays when left side is of type object.");
                        }
                    }

                    leftExpr = Expression.TypeAs(leftExpr, compareType);
                }
                else
                    throw CreateParseException(node, "Left and right side of in operator does not have matching types.");
            }

            return Expression.Call(OdataFunctionMapping.EnumerableContainsMethod.MakeGenericMethod(compareType),
                                   rightExpr, leftExpr);
        }


        private Expression ParseLambda(LambdaNode lambdaNode, Expression memberExpression, Type expectedType)
        {
            if (expectedType.UniqueToken() == typeof(Expression<>).UniqueToken())
            {
                // Quote if expression
                return Expression.Quote(
                    ParseLambda(lambdaNode, memberExpression, expectedType.GetGenericArguments()[0]));
            }

            var nestedConverter = new NodeTreeToExpressionConverter(this.propertyResolver, this.parsedString);

            // TODO: Check that we don't already have a arg with same name.

            // TODO: Proper check that we have a func here
            if (expectedType.UniqueToken() != typeof(Func<,>).UniqueToken())
                throw CreateParseException(lambdaNode, "Can't parse lambda to expected type that is not a Func delegate..");

            if (expectedType.GetGenericArguments()[0].IsGenericParameter)
                throw CreateParseException(lambdaNode, "Unable to resolve generic type for parsing lambda.");

            var funcTypeArgs = expectedType.GetGenericArguments();

            // TODO: Support multiple lambda args..(?)
            var lambdaParams =
                lambdaNode.Argument.WrapAsEnumerable().Select(
                    (x, idx) => Expression.Parameter(funcTypeArgs[idx], x.Name)).ToList();

            return nestedConverter.ToLambdaExpression(this.thisParam, lambdaParams, this.parameters.Values, lambdaNode.Body);
        }


        private Expression ParseMethodCallNode(MethodCallNode node, Expression memberExpression)
        {
            if (memberExpression == null)
                throw new ArgumentNullException(nameof(memberExpression));
            if (memberExpression == this.thisParam)
            {
                if (node.HasArguments)
                {
                    Expression expression;
                    if (TryResolveOdataExpression(node, memberExpression, out expression))
                        return expression;
                }
            }
            throw CreateParseException(node, "Could not recognize method " + node.Name);
        }


        private void PeformImplicitConversionForNodePair(ref Expression left,
                                                         ref Expression right,
                                                         bool callSwappedRecursively = true)
        {
            var leftType = left.Type;

            if (leftType.IsNullable() && Nullable.GetUnderlyingType(leftType) == right.Type)
                right = Expression.Convert(right, leftType);
            else if (leftType == typeof(object) && right.Type != typeof(object))
            {
                var newType = right.Type;
                if (newType.IsValueType && !newType.IsNullable())
                {
                    newType = typeof(Nullable<>).MakeGenericType(newType);
                    right = Expression.Convert(right, newType);
                }
                if (left.NodeType != ExpressionType.Constant || ((ConstantExpression)left).Value != null)
                    left = Expression.TypeAs(left, newType);
            }
            else if (callSwappedRecursively)
                PeformImplicitConversionForNodePair(ref right, ref left, callSwappedRecursively : false);
        }


        private Expression ResolveDictionaryAccess(SymbolNode node, Expression memberExpression, Type dictionaryType)
        {
            var key = node.Name;

            var method = OdataFunctionMapping.SafeGetMethod.MakeGenericMethod(dictionaryType.GetGenericArguments());

            return Expression.Call(method, memberExpression, Expression.Constant(key));
        }


        private Expression ResolveSymbolNode(SymbolNode node, Expression memberExpression, Type expectedType)
        {
            if (memberExpression == null)
                throw new ArgumentNullException(nameof(memberExpression));
            if (memberExpression == this.thisParam)
            {
                if (node.Name == "this")
                    return this.thisParam;
                if (node.Name == "true")
                    return Expression.Constant(true);
                if (node.Name == "false")
                    return Expression.Constant(false);
                if (node.Name == "null")
                    return Expression.Constant(null, expectedType ?? typeof(object));
                ParameterExpression parameter;
                if (this.parameters.TryGetValue(node.Name, out parameter))
                    return parameter;
            }

            return MakePropertyAccess(node, memberExpression);
        }


        private Type ResolveType(TypeNameNode typeNameNode)
        {
            return this.propertyResolver.ResolveType(typeNameNode.Value);
        }


        private void TryDetectAndConvertEnumComparison(ref Expression left, ref Expression right, bool tryAgainSwapped)
        {
            if (left.Type.IsEnum && right.NodeType == ExpressionType.Constant && right.Type == typeof(string))
            {
                var enumType = left.Type;
                var enumUnderlyingType = enumType.GetEnumUnderlyingType();
                left = Expression.Convert(left, enumUnderlyingType);
                var enumStringValue = (string)((ConstantExpression)right).Value;
                var enumIntvalue = Convert.ChangeType(
                    Enum.Parse(enumType, enumStringValue),
                    enumUnderlyingType);
                right = Expression.Constant(enumIntvalue, enumUnderlyingType);
                return;
            }

            if (tryAgainSwapped)
                TryDetectAndConvertEnumComparison(ref right, ref left, false);
        }


        private void TryDetectAndConvertNullableEnumComparison(ref Expression left, ref Expression right, bool tryAgainSwapped)
        {
            Type[] nullableTypeArgs;
            if (right.NodeType == ExpressionType.Constant && right.Type == typeof(string)
                && left.Type.TryExtractTypeArguments(typeof(Nullable<>), out nullableTypeArgs) && nullableTypeArgs[0].IsEnum)
            {
                var enumType = nullableTypeArgs[0];
                var enumUnderlyingType = typeof(Nullable<>).MakeGenericType(enumType.GetEnumUnderlyingType());
                left = Expression.Convert(left, enumUnderlyingType);
                var enumStringValue = (string)((ConstantExpression)right).Value;
                var enumIntvalue = Convert.ChangeType(
                    Enum.Parse(enumType, enumStringValue),
                    enumType);
                right = Expression.Convert(Expression.Constant(enumIntvalue, enumType), enumUnderlyingType);
                return;
            }

            if (tryAgainSwapped)
                TryDetectAndConvertNullableEnumComparison(ref right, ref left, false);
        }


        private bool TryResolveGenericInstanceMethod<TMemberInfo>(Expression instance, ref TMemberInfo member)
            where TMemberInfo : MemberInfo
        {
            var declaringType = member.DeclaringType;
            if (declaringType.IsGenericTypeDefinition)
            {
                Type[] typeArgs;
                if (instance.Type.TryExtractTypeArguments(declaringType, out typeArgs))
                {
                    var memberLocal = member;
                    member = declaringType
                        .MakeGenericType(typeArgs)
                        .GetMember(memberLocal.Name)
                        .OfType<TMemberInfo>()
                        .Single(x => x.UniqueToken() == memberLocal.UniqueToken());
                }
                else
                {
                    // Neither type nor any of interfaces of instance matches declaring type.
                    return false;
                }
            }
            return true;
        }


        private bool TryResolveMemberMapping(
            OdataFunctionMapping.MemberMapping memberMapping,
            MethodCallNode node,
            out Expression expression)
        {
            expression = null;
            var reorderedArgs = memberMapping.ReorderArguments(node.Children);
            var method = memberMapping.Member as MethodInfo;
            var property = memberMapping.Member as PropertyInfo;
            if (method != null)
            {
                Expression instance = null;

                if (!method.IsStatic)
                {
                    instance = ParseExpression(reorderedArgs[0], this.thisParam, null);
                    if (!TryResolveGenericInstanceMethod(instance, ref method))
                        return false;
                }

                // Convert each node and check whether argument matches..
                var argArrayOffset = method.IsStatic ? 0 : 1;
                var methodParameters = method.GetParameters();
                if (methodParameters.Length != reorderedArgs.Count - argArrayOffset)
                {
                    var message =
                        string.Format(
                            "Number parameters count ({0}) for method {1}.{2} does not match provided argument count ({3})",
                            methodParameters.Length, method.DeclaringType.FullName, method.Name,
                            (reorderedArgs.Count - argArrayOffset));
                    throw CreateParseException(node, message);
                }

                var argExprArray = new Expression[methodParameters.Length];

                if (!method.IsGenericMethodDefinition)
                {
                    for (var i = 0; i < methodParameters.Length; i++)
                    {
                        var param = methodParameters[i];
                        var argNode = reorderedArgs[i + argArrayOffset];
                        var argExpr = ParseExpression(argNode, this.thisParam, param.ParameterType);

                        if (!param.ParameterType.IsAssignableFrom(argExpr.Type))
                            return false;

                        argExprArray[i] = argExpr;
                    }
                }
                else
                {
                    var methodDefinition = method;
                    var methodTypeArgs = method.GetGenericArguments();

                    for (var i = 0; i < methodParameters.Length; i++)
                    {
                        var param = methodParameters[i];
                        var argNode = reorderedArgs[i + argArrayOffset];
                        var argExpr = ParseExpression(argNode, this.thisParam, param.ParameterType);

                        bool typeArgsWasResolved;
                        if (
                            !TypeExtensions.TryFillGenericTypeParameters(param.ParameterType, argExpr.Type, methodTypeArgs,
                                                                         out typeArgsWasResolved))
                            return false;

                        if (typeArgsWasResolved)
                        {
                            // Upgrade to real method when all type args are resolved!!
                            method = methodDefinition.MakeGenericMethod(methodTypeArgs);
                            methodParameters = method.GetParameters();
                        }

                        argExprArray[i] = argExpr;
                    }
                }

                expression = Expression.Call(instance, method, argExprArray);
                expression = memberMapping.PostResolveHook(expression);
                return true;
            }
            if (property != null)
            {
                var instance = ParseExpression(reorderedArgs[0], this.thisParam, null);
                if (!TryResolveGenericInstanceMethod(instance, ref property))
                    return false;

                expression = Expression.MakeMemberAccess(instance, property);
                expression = memberMapping.PostResolveHook(expression);
                return true;
            }
            return false;
        }


        private bool TryResolveOdataExpression(
            MethodCallNode node,
            Expression memberExpression,
            out Expression expression)
        {
            expression = null;

            switch (node.Name)
            {
                case "isof":
                case "cast":
                    //var 
                    if (node.Children.Count > 2 || node.Children.Count < 1)
                        throw CreateParseException(node, "Only one or two arguments to cast operator is allowed.");
                    TypeNameNode castTypeArg;
                    Expression operand;

                    if (node.Children.Count == 1)
                    {
                        castTypeArg = node.Children[0] as TypeNameNode;
                        operand = this.thisParam;
                    }
                    else
                    {
                        operand = ParseExpression(node.Children[0]);
                        castTypeArg = node.Children[1] as TypeNameNode;
                    }

                    if (castTypeArg == null)
                        throw CreateParseException(node, "Argument to cast is required to be a type literal.");

                    var type = ResolveType(castTypeArg);
                    if (node.Name == "cast")
                        expression = Expression.Convert(operand, type);
                    else if (node.Name == "isof")
                        expression = Expression.TypeIs(operand, type);
                    return true;

                case "iif":
                    ParseConditionalOperator(node, ref expression);
                    return true;
            }

            var memberCandidates = OdataFunctionMapping.GetMemberCandidates(node.Name, node.Children.Count);

            foreach (var memberMapping in memberCandidates)
            {
                if (TryResolveMemberMapping(memberMapping, node, out expression))
                    return true;
            }

            return false;
        }
    }
}