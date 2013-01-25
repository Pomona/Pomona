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
using Antlr.Runtime.Tree;

namespace Pomona.Queries
{
    internal class PomonaQueryTreeParser
    {
        private static readonly HashSet<NodeType> binaryNodeTypes;
        private static readonly Dictionary<int, NodeType> nodeTypeDict;


        static PomonaQueryTreeParser()
        {
            binaryNodeTypes = new HashSet<NodeType>()
                {
                    NodeType.AndAlso,
                    NodeType.OrElse,
                    NodeType.Multiply,
                    NodeType.Modulo,
                    NodeType.Add,
                    NodeType.Div,
                    NodeType.Subtract,
                    NodeType.GreaterThan,
                    NodeType.LessThan,
                    NodeType.GreaterThanOrEqual,
                    NodeType.LessThanOrEqual,
                    NodeType.Equal,
                    NodeType.NotEqual,
                    NodeType.Dot,
                    NodeType.As
                };
            nodeTypeDict = new Dictionary<int, NodeType>
                {
                    {PomonaQueryParser.LT_OP, NodeType.LessThan},
                    {PomonaQueryParser.EQ_OP, NodeType.Equal},
                    {PomonaQueryParser.NE_OP, NodeType.NotEqual},
                    {PomonaQueryParser.GT_OP, NodeType.GreaterThan},
                    {PomonaQueryParser.GE_OP, NodeType.GreaterThanOrEqual},
                    {PomonaQueryParser.LE_OP, NodeType.LessThanOrEqual},
                    {PomonaQueryParser.ADD_OP, NodeType.Add},
                    {PomonaQueryParser.SUB_OP, NodeType.Subtract},
                    {PomonaQueryParser.AND_OP, NodeType.AndAlso},
                    {PomonaQueryParser.OR_OP, NodeType.OrElse},
                    {PomonaQueryParser.MUL_OP, NodeType.Multiply},
                    {PomonaQueryParser.DIV_OP, NodeType.Div},
                    {PomonaQueryParser.MOD_OP, NodeType.Modulo},
                    {PomonaQueryParser.DOT_OP, NodeType.Dot},
                    {PomonaQueryParser.AS_OP,NodeType.As},
                    {PomonaQueryParser.STRING, NodeType.StringLiteral}
                };
        }


        public static NodeBase ParseTree(ITree tree, int depth)
        {
            depth++;

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
                    case "guid":
                        return new GuidNode(Guid.Parse(value));
                    case "datetime":
                        return
                            new DateTimeNode(
                                DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
                }
            }

            if (IsReduceableBinaryOperator(tree.Type) && tree.ChildCount == 1)
                return ParseTree(tree.GetChild(0), depth);

            switch (tree.Type)
            {
                case PomonaQueryParser.METHOD_CALL:
                    return new MethodCallNode(tree.GetChild(0).Text, ParseChildren(tree, depth, 1));
                case PomonaQueryParser.INDEXER_ACCESS:
                    return new IndexerAccessNode(tree.GetChild(0).Text, ParseChildren(tree, depth, 1));
                case PomonaQueryParser.INT:
                    return new NumberNode(tree.Text);
                case PomonaQueryParser.STRING:
                    return new StringNode(tree.Text);
                case PomonaQueryParser.ID:
                    return new SymbolNode(tree.Text, ParseChildren(tree, depth));
                case PomonaQueryParser.ROOT:
                    return ParseTree(tree.GetChild(0), depth);
                case PomonaQueryParser.LAMBDA_OP:
                    return new LambdaNode(ParseChildren(tree, depth));
            }

            NodeType nodeType;
            if (IsBinaryOperator(tree.Type, out nodeType))
            {
                var childNodes = new Queue<NodeBase>(ParseChildren(tree, depth));

                var expr = childNodes.Dequeue();
                while (childNodes.Count > 0)
                    expr = new BinaryOperator(nodeType, new[] {expr, childNodes.Dequeue()});
                return expr;
            }

            return new UnhandledNode(tree);
        }


        private static IEnumerable<ITree> GetChildren(ITree tree)
        {
            var childCount = tree.ChildCount;
            for (var i = 0; i < childCount; i++)
                yield return tree.GetChild(i);
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
                case PomonaQueryParser.AS_OP:
                case PomonaQueryParser.LAMBDA_OP:
                case PomonaQueryParser.AND_OP:
                case PomonaQueryParser.OR_OP:
                    return true;
            }

            return false;
        }


        private static IEnumerable<NodeBase> ParseChildren(ITree tree, int depth, int skip = 0)
        {
            return GetChildren(tree).Skip(skip).Select(x => ParseTree(x, depth + 1));
        }


        //Expression 
    }
}