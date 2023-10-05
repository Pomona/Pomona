#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Antlr.Runtime;
using Antlr.Runtime.Tree;

using Pomona.CodeGen;
using Pomona.Common;
using Pomona.Common.Internals;

namespace Pomona.Queries
{
    public class QueryExpressionParser
    {
        private readonly IQueryTypeResolver queryPropertyResolver;


        public QueryExpressionParser(IQueryTypeResolver queryPropertyResolver)
        {
            if (queryPropertyResolver == null)
                throw new ArgumentNullException(nameof(queryPropertyResolver));
            this.queryPropertyResolver = queryPropertyResolver;
        }


        public Expression<Func<T, bool>> Parse<T>(string odataExpression)
        {
            return (Expression<Func<T, bool>>)Parse(typeof(T), odataExpression);
        }


        public LambdaExpression Parse(Type thisType, string odataExpression)
        {
            var tempTree = ParseSymbolTree(odataExpression);
            return TransformTreeNodeToExpression(thisType, odataExpression, tempTree);
        }


        public List<Tuple<LambdaExpression, SortOrder>> ParseOrderBy(Type thisType, string odataExpression)
        {
            var expressions = new List<Tuple<LambdaExpression, SortOrder>>();
            var parser = CreateParser(odataExpression);
            var tree = (CommonTree)parser.parse().Tree;
            foreach (var node in tree.Children)
            {
                if (node.Type != PomonaQueryParser.ORDERBY_ASC)
                    throw new InvalidOperationException("Unexpected node in parser tree.");

                var tempTree = PomonaQueryTreeParser.ParseTree(node.GetChild(0), 0, odataExpression);
                var sortOrder = node.ChildCount > 1
                                && string.Equals(node.GetChild(1).Text,
                                                 "desc",
                                                 StringComparison.InvariantCultureIgnoreCase)
                    ? SortOrder.Descending
                    : SortOrder.Ascending;

                expressions.Add(
                    new Tuple<LambdaExpression, SortOrder>(TransformTreeNodeToExpression(thisType,
                                                                                         odataExpression,
                                                                                         tempTree), sortOrder));
            }
            return expressions;
        }


        public LambdaExpression ParseSelectList(Type thisType, string selectListExpression, bool useAnonymousType = true)
        {
            // By enclosing the selectListExpression in a method call, we can use the same parser
            // as for filter queries, then we convert each part of the select statement to a
            // lambda expression.
            var symbolTree = ParseSymbolTree($"select({selectListExpression})");
            var thisParam = Expression.Parameter(thisType, "_this");
            var selectParts =
                symbolTree.Children.Select(x => ParseSelectPart(thisType, x, thisParam, selectListExpression)).ToList();

            if (selectParts.Count == 1 && selectParts[0].Key.ToLower() == "this")
            {
                // This is a way to select a list of one property, without encapsulating it inside anon objects..
                return Expression.Lambda(selectParts[0].Value, thisParam);
            }

            if (useAnonymousType)
            {
                Type anonTypeInstance;
                var expr = AnonymousTypeBuilder.CreateNewExpression(selectParts, out anonTypeInstance);
                return Expression.Lambda(expr, thisParam);
            }

            var addMethod = ReflectionHelper.GetMethodDefinition<Dictionary<string, object>>(x => x.Add("", null));
            var dictExpr =
                Expression.ListInit(Expression.New(typeof(Dictionary<string, object>)),
                                    selectParts.Select(
                                        x => Expression.ElementInit(
                                            addMethod,
                                            Expression.Constant(x.Key),
                                            x.Value.Type.IsValueType ? Expression.Convert(x.Value, typeof(object)) : x.Value)));

            return Expression.Lambda(dictExpr, thisParam);
        }


        private static PomonaQueryParser CreateParser(string odataExpression)
        {
            var input = new ANTLRStringStream(odataExpression);

            var lexer = new PomonaQueryLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new PomonaQueryParser(tokens);
            return parser;
        }


        private KeyValuePair<string, Expression> ParseSelectPart(Type thisType,
                                                                 NodeBase node,
                                                                 ParameterExpression thisParam,
                                                                 string parsedString)
        {
            string propertyName = null;
            if (node.NodeType == NodeType.As)
            {
                var asNode = (BinaryOperatorNode)node;
                if (asNode.Right.NodeType != NodeType.Symbol)
                {
                    throw QueryParseException.Create(node.ParserNode,
                                                     "Need to specify a property name on left side of as in select expression.",
                                                     parsedString, null, QueryParseErrorReason.GenericError);
                }
                propertyName = ((SymbolNode)asNode.Right).Name;
                node = asNode.Left;
            }
            else
            {
                if (!TryGetImplicitPropertyName(node, out propertyName))
                {
                    throw QueryParseException.Create(node.ParserNode,
                                                     $"Unable to infer property name of select expression ({node})",
                                                     parsedString, null, QueryParseErrorReason.GenericError);
                }
            }

            var exprParser = new NodeTreeToExpressionConverter(this.queryPropertyResolver, parsedString);
            var lambdaExpression = exprParser.ToLambdaExpression(thisParam, thisParam.WrapAsEnumerable(),
                                                                 Enumerable.Empty<ParameterExpression>(), node);

            return new KeyValuePair<string, Expression>(propertyName, lambdaExpression.Body);
        }


        private static NodeBase ParseSymbolTree(string odataExpression)
        {
            if (odataExpression == null)
                throw new ArgumentNullException(nameof(odataExpression));
            var parser = CreateParser(odataExpression);
            var parseReturn = parser.parse();
            var tree = (CommonTree)parseReturn.Tree;

            var tempTree = PomonaQueryTreeParser.ParseTree(tree, 0, odataExpression);
            return tempTree;
        }


        private LambdaExpression TransformTreeNodeToExpression(Type thisType, string odataExpression, NodeBase tempTree)
        {
            var nodeTreeToExpressionConverter = new NodeTreeToExpressionConverter(this.queryPropertyResolver, odataExpression);

            var lambdaExpression = nodeTreeToExpressionConverter.ToLambdaExpression(thisType, tempTree);
            return lambdaExpression;
        }


        private bool TryGetImplicitPropertyName(NodeBase node, out string propertyName)
        {
            propertyName = null;
            while (node.NodeType == NodeType.Dot)
                node = ((BinaryOperatorNode)node).Right;

            if (node.NodeType != NodeType.Symbol)
                return false;

            propertyName = ((SymbolNode)node).Name;
            return true;
        }
    }
}

