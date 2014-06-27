#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
using System.Linq;
using System.Linq.Expressions;

using Pomona.Common.Linq;

namespace Pomona.Common
{
    internal class SelectClientServerPartitionerVisitor : ExpressionVisitor
    {
        private readonly QueryPredicateBuilder builder;

        private readonly List<KeyValuePair<TupleItemPlaceholderExpression, PomonaExtendedExpression>> serverSelectParts
            =
            new List<KeyValuePair<TupleItemPlaceholderExpression, PomonaExtendedExpression>>();

        private readonly Dictionary<string, TupleItemPlaceholderExpression> serverSelectReuseLookup =
            new Dictionary<string, TupleItemPlaceholderExpression>();

        private LambdaExpression rootLambda;


        public SelectClientServerPartitionerVisitor(QueryPredicateBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
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
                throw new ArgumentNullException("node");
            this.rootLambda = node;
            try
            {
                var visited = (LambdaExpression)Visit(node);

                var tupleType =
                    Type.GetType("System.Tuple`" + this.serverSelectParts.Count, true)
                        .MakeGenericType(this.serverSelectParts.Select(x => x.Key.Type).ToArray());

                var serverExpr =
                    new QuerySelectBuilder().Visit(
                        Expression.Lambda(Expression.New(tupleType.GetConstructors().Single(),
                            this.serverSelectParts.Select(x => x.Value)),
                            node.Parameters));

                var clientExprParam = Expression.Parameter(tupleType, "_this");
                var clientExprBody =
                    new ReplaceWithTuplePropertyVisitor(this, clientExprParam, tupleType).Visit(visited.Body);
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

            TupleItemPlaceholderExpression p;
            var exprSegment = pomonaExtendedExpression.ToString();
            if (!this.serverSelectReuseLookup.TryGetValue(exprSegment, out p))
            {
                p = new TupleItemPlaceholderExpression(node.Type, this.serverSelectParts.Count + 1);
                this.serverSelectParts.Add(new KeyValuePair<TupleItemPlaceholderExpression, PomonaExtendedExpression>(p,
                    pomonaExtendedExpression));
                this.serverSelectReuseLookup[exprSegment] = p;
            }

            return p;
        }

        #region Nested type: ReplaceWithTuplePropertyVisitor

        private class ReplaceWithTuplePropertyVisitor : ExpressionVisitor
        {
            private readonly SelectClientServerPartitionerVisitor owner;
            private readonly ParameterExpression tupleParameter;
            private readonly Type tupleType;


            public ReplaceWithTuplePropertyVisitor(SelectClientServerPartitionerVisitor owner,
                ParameterExpression tupleParameter,
                Type tupleType)
            {
                this.owner = owner;
                this.tupleParameter = tupleParameter;
                this.tupleType = tupleType;
            }


            protected override Expression VisitExtension(Expression node)
            {
                var nodeExt = node as TupleItemPlaceholderExpression;
                if (node is TupleItemPlaceholderExpression)
                {
                    var prop = this.tupleType.GetProperty("Item" + nodeExt.ItemNumber);
                    return Expression.Property(this.tupleParameter, prop);
                }
                return base.VisitExtension(node);
            }
        }

        #endregion

        #region Nested type: TupleItemPlaceholderExpression

        private class TupleItemPlaceholderExpression : Expression
        {
            private readonly int itemNumber;
            private readonly Type type;


            public TupleItemPlaceholderExpression(Type type, int itemNumber)
            {
                this.type = type;
                this.itemNumber = itemNumber;
            }


            public int ItemNumber
            {
                get { return this.itemNumber; }
            }

            public override ExpressionType NodeType
            {
                get { return ExpressionType.Extension; }
            }

            public override Type Type
            {
                get { return this.type; }
            }
        }

        #endregion
    }
}