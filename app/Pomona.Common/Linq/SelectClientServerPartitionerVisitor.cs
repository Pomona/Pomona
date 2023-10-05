#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Pomona.Common.Linq
{
    internal class SelectClientServerPartitionerVisitor : ExpressionVisitor
    {
        private readonly QueryPredicateBuilder builder;

        private readonly List<KeyValuePair<ArrayItemPlaceholderExpression, PomonaExtendedExpression>> serverSelectParts
            =
            new List<KeyValuePair<ArrayItemPlaceholderExpression, PomonaExtendedExpression>>();

        private readonly Dictionary<string, ArrayItemPlaceholderExpression> serverSelectReuseLookup =
            new Dictionary<string, ArrayItemPlaceholderExpression>();

        private LambdaExpression rootLambda;


        public SelectClientServerPartitionerVisitor(QueryPredicateBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            this.builder = builder;
        }


        public override Expression Visit(Expression node)
        {
            if (node == null)
                return null;

            var visited = VisitInner(node);
            if (visited is PomonaExtendedExpression)
                throw new InvalidOperationException("Should NOT return pomona expr.");
            return visited;
        }


        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (node == this.rootLambda)
                return node.Update(Visit(node.Body), node.Parameters);
            return node;
        }


        internal ClientServerSplitSelectExpression SplitExpression(LambdaExpression node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (this.rootLambda != null)
                throw new InvalidOperationException();
            this.rootLambda = node;
            try
            {
                var visited = (LambdaExpression)Visit(node);

                var serverExpr =
                    new QuerySelectorBuilder().Visit(
                        Expression.Lambda(Expression.NewArrayInit(typeof(object),
                                                                  this.serverSelectParts.Select(
                                                                      x =>
                                                                          x.Value.Type.IsValueType
                                                                              ? (Expression)
                                                                          Expression.Convert(x.Value, typeof(object))
                                                                              : x.Value)),
                                          node.Parameters));

                var clientExprParam = Expression.Parameter(typeof(object[]), "_this");
                var clientExprBody =
                    new ReplaceWithArrayAccessFromServerResultVisitor(clientExprParam).Visit(visited.Body);
                var clientExpr = Expression.Lambda(clientExprBody, clientExprParam);

                return new ClientServerSplitSelectExpression((PomonaExtendedExpression)serverExpr, clientExpr);
            }
            finally
            {
                this.rootLambda = null;
            }
        }


        private Expression VisitInner(Expression node)
        {
            if (node is PomonaExtendedExpression)
                throw new InvalidOperationException("Should not get PomonaExtendedExpression in.");

            if (node.NodeType == ExpressionType.Lambda && node != this.rootLambda)
                return node;

            var visited = this.builder.Visit(node);

            var pomonaExtendedExpression = visited as PomonaExtendedExpression;
            if (pomonaExtendedExpression == null)
                throw new InvalidOperationException("Expected PomonaExtendedExpression, got another type of Expression.");

            //if (pomonaExtendedExpression.SupportedOnServer)
            //{
            //    Console.WriteLine("EXPR " + node + " IS SUPPORTED ON SERVER AS " + pomonaExtendedExpression + ", EXECUTES ON " + (pomonaExtendedExpression.LocalExecutionPreferred ? "CLIENT" : "SERVER"));
            //}

            if (!pomonaExtendedExpression.SupportedOnServer || pomonaExtendedExpression.LocalExecutionPreferred)
                return base.Visit(node);

            ArrayItemPlaceholderExpression p;
            var exprSegment = pomonaExtendedExpression.ToString();
            if (!this.serverSelectReuseLookup.TryGetValue(exprSegment, out p))
            {
                p = new ArrayItemPlaceholderExpression(node.Type, this.serverSelectParts.Count);
                this.serverSelectParts.Add(new KeyValuePair<ArrayItemPlaceholderExpression, PomonaExtendedExpression>(p,
                                                                                                                      pomonaExtendedExpression));
                this.serverSelectReuseLookup[exprSegment] = p;
            }

            return p;
        }

        #region Nested type: ArrayItemPlaceholderExpression

        private class ArrayItemPlaceholderExpression : Expression
        {
            public ArrayItemPlaceholderExpression(Type type, int index)
            {
                Type = type;
                Index = index;
            }


            public int Index { get; }

            public override ExpressionType NodeType => ExpressionType.Extension;

            public override Type Type { get; }
        }

        #endregion

        #region Nested type: ReplaceWithArrayAccessFromServerResultVisitor

        private class ReplaceWithArrayAccessFromServerResultVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression arrayParameter;


            public ReplaceWithArrayAccessFromServerResultVisitor(
                ParameterExpression arrayParameter)
            {
                this.arrayParameter = arrayParameter;
            }


            protected override Expression VisitExtension(Expression node)
            {
                var nodeExt = node as ArrayItemPlaceholderExpression;
                if (node is ArrayItemPlaceholderExpression)
                {
                    return
                        Expression.Convert(
                            Expression.ArrayIndex(this.arrayParameter, Expression.Constant(nodeExt.Index)),
                            nodeExt.Type);
                }
                return base.VisitExtension(node);
            }
        }

        #endregion
    }
}

