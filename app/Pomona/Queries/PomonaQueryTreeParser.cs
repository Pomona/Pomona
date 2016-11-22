#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using Antlr.Runtime.Tree;

namespace Pomona.Queries
{
    internal class PomonaQueryTreeParser
    {
        private static readonly HashSet<NodeType> binaryNodeTypes;
        private static readonly Dictionary<int, NodeType> nodeTypeDict;

        private static readonly Regex dateTimeOffsetRegex;


        static PomonaQueryTreeParser()
        {
            binaryNodeTypes = new HashSet<NodeType>
            {
                NodeType.AndAlso,
                NodeType.OrElse,
                NodeType.Multiply,
                NodeType.Modulo,
                NodeType.Add,
                NodeType.Divide,
                NodeType.Subtract,
                NodeType.GreaterThan,
                NodeType.LessThan,
                NodeType.GreaterThanOrEqual,
                NodeType.LessThanOrEqual,
                NodeType.Equal,
                NodeType.NotEqual,
                NodeType.Dot,
                NodeType.As,
                NodeType.In,
                NodeType.CaseInsensitiveEqual
            };
            nodeTypeDict = new Dictionary<int, NodeType>
            {
                { PomonaQueryParser.LT_OP, NodeType.LessThan },
                { PomonaQueryParser.EQ_OP, NodeType.Equal },
                { PomonaQueryParser.NE_OP, NodeType.NotEqual },
                { PomonaQueryParser.GT_OP, NodeType.GreaterThan },
                { PomonaQueryParser.GE_OP, NodeType.GreaterThanOrEqual },
                { PomonaQueryParser.LE_OP, NodeType.LessThanOrEqual },
                { PomonaQueryParser.ADD_OP, NodeType.Add },
                { PomonaQueryParser.SUB_OP, NodeType.Subtract },
                { PomonaQueryParser.AND_OP, NodeType.AndAlso },
                { PomonaQueryParser.OR_OP, NodeType.OrElse },
                { PomonaQueryParser.MUL_OP, NodeType.Multiply },
                { PomonaQueryParser.DIV_OP, NodeType.Divide },
                { PomonaQueryParser.MOD_OP, NodeType.Modulo },
                { PomonaQueryParser.DOT_OP, NodeType.Dot },
                { PomonaQueryParser.AS_OP, NodeType.As },
                { PomonaQueryParser.STRING, NodeType.StringLiteral },
                { PomonaQueryParser.IN_OP, NodeType.In },
                { PomonaQueryParser.IEQ_OP, NodeType.CaseInsensitiveEqual }
            };

            dateTimeOffsetRegex = new Regex(@"\+\d{2}:\d{2}$", RegexOptions.Compiled);
        }


        public static NodeBase ParseTree(ITree tree, int depth, string parsedString)
        {
            var node = ParseTreeInner(tree, depth, parsedString);
            node.ParserNode = tree;
            return node;
        }


        private static IEnumerable<ITree> GetChildren(ITree tree)
        {
            var childCount = tree.ChildCount;
            for (var i = 0; i < childCount; i++)
                yield return tree.GetChild(i);
        }


        private static NodeBase GetDateTimeNode(string value)
        {
            if (dateTimeOffsetRegex.IsMatch(value))
            {
                const DateTimeStyles dateTimeStyles = DateTimeStyles.AssumeUniversal | DateTimeStyles.RoundtripKind;
                var dateTimeOffset = DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, dateTimeStyles);

                return new DateTimeOffsetNode(dateTimeOffset);
            }

            var dateTime = DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            return new DateTimeNode(dateTime);
        }


        private static bool IsBinaryOperator(int type, out NodeType nodeType)
        {
            nodeType = nodeTypeDict[type];
            return binaryNodeTypes.Contains(nodeType);
        }


        private static bool IsReduceableBinaryOperator(int type)
        {
            switch (type)
            {
                case PomonaQueryParser.ORDERBY_ASC:
                case PomonaQueryParser.AS_OP:
                case PomonaQueryParser.LAMBDA_OP:
                case PomonaQueryParser.AND_OP:
                case PomonaQueryParser.OR_OP:
                    return true;
            }

            return false;
        }


        private static IEnumerable<NodeBase> ParseChildren(ITree tree, int depth, string parsedString, int skip = 0)
        {
            return GetChildren(tree).Skip(skip).Select(x => ParseTree(x, depth + 1, parsedString));
        }


        private static NodeBase ParseTreeInner(ITree tree, int depth, string parsedString)
        {
            depth++;

            if (tree.Type == 0)
                throw QueryParseException.Create(tree, "Parse error", parsedString, null);

            if (tree.Type == PomonaQueryParser.PREFIXED_STRING)
            {
                var text = tree.Text;
                var stringPartStart = text.IndexOf('\'');
                if (stringPartStart == -1)
                    throw new InvalidOperationException("Unable to parse prefixed literal.");

                var prefix = text.Substring(0, stringPartStart);
                var value = text.Substring(stringPartStart + 1, text.Length - stringPartStart - 2);

                switch (prefix)
                {
                    case "t":
                        return new TypeNameNode(value);
                    case "guid":
                        return new GuidNode(Guid.Parse(value));
                    case "datetime":
                        return GetDateTimeNode(value);
                }
            }

            if (IsReduceableBinaryOperator(tree.Type) && tree.ChildCount == 1)
                return ParseTree(tree.GetChild(0), depth, parsedString);

            switch (tree.Type)
            {
                case PomonaQueryParser.NOT_OP:
                    return new NotNode(ParseChildren(tree, depth, parsedString));
                case PomonaQueryParser.METHOD_CALL:
                    return new MethodCallNode(tree.GetChild(0).Text, ParseChildren(tree, depth, parsedString, 1));
                case PomonaQueryParser.INDEXER_ACCESS:
                    return new IndexerAccessNode(tree.GetChild(0).Text, ParseChildren(tree, depth, parsedString, 1));
                case PomonaQueryParser.INT:
                    return new NumberNode(tree.Text);
                case PomonaQueryParser.STRING:
                    return new StringNode(tree.Text);
                case PomonaQueryParser.ID:
                    return new SymbolNode(tree.Text, ParseChildren(tree, depth, parsedString));
                case PomonaQueryParser.ROOT:
                    return ParseTree(tree.GetChild(0), depth, parsedString);
                case PomonaQueryParser.LAMBDA_OP:
                    return new LambdaNode(ParseChildren(tree, depth, parsedString));
                case PomonaQueryParser.ARRAY_LITERAL:
                    return new ArrayNode(ParseChildren(tree, depth, parsedString));
            }

            NodeType nodeType;
            if (IsBinaryOperator(tree.Type, out nodeType))
            {
                var childNodes = new Queue<NodeBase>(ParseChildren(tree, depth, parsedString));

                var expr = childNodes.Dequeue();
                while (childNodes.Count > 0)
                    expr = new BinaryOperatorNode(nodeType, new[] { expr, childNodes.Dequeue() });
                return expr;
            }

            return new UnhandledNode(tree);
        }
    }
}