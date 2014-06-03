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
using System.Reflection;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Expressions
{
    public class TreePatternMatcher : ExpressionVisitor
    {
        private static readonly MethodInfo groupMethod =
            ReflectionHelper.GetMethodDefinition<IMatchContext>(x => x.Group<object>(null));

        private static readonly UniqueMemberToken groupMethodToken =
            ReflectionHelper.GetMethodDefinition<IMatchContext>(x => x.Group<object>(null)).UniqueToken();

        private static readonly UniqueMemberToken groupUnnamedMethodToken =
            ReflectionHelper.GetMethodDefinition<IMatchContext>(x => x.Group<object>()).UniqueToken();

        private readonly Expression replacePattern;
        private readonly Expression searchPattern;
        private IDictionary<string, Expression> captures;


        public TreePatternMatcher(Expression searchPattern, Expression replacePattern = null)
        {
            if (searchPattern == null)
                throw new ArgumentNullException("rootPattern");
            this.searchPattern = searchPattern;
            this.replacePattern = replacePattern;
        }


        public static TreePatternMatcher FromLambda(LambdaExpression searchLambda, LambdaExpression replaceLambda)
        {
            if (!searchLambda.Parameters.Select(x => x.Type).SequenceEqual(replaceLambda.Parameters.Select(x => x.Type)))
            {
                throw new InvalidOperationException(
                    "searchLambda and replaceLambda must have same number and type of parameters");
            }

            return new TreePatternMatcher(ReplaceParametersWithCaptureGroup(searchLambda),
                ReplaceParametersWithCaptureGroup(replaceLambda));
        }


        public static TreePatternMatcher FromLambda<T, TRet>(Expression<Func<T, IMatchContext, TRet>> searchLambda,
            Expression<Func<T, IMatchContext, TRet>> replaceLambda)
        {
            return FromLambda((LambdaExpression)searchLambda, replaceLambda);
        }


        public bool Match(Expression expr)
        {
            this.captures = null;
            return Match(this.searchPattern, expr);
        }


        public Expression MatchAndRewrite(Expression expression)
        {
            if (this.replacePattern == null)
                throw new InvalidOperationException("No replacement pattern provided in matcher.");
            if (Match(expression))
                expression = new RewriteVisitor(this).Visit(this.replacePattern);
            return expression;
        }


        private static string GetCaptureGroupKey(MethodCallExpression pattern)
        {
            var groupKey = "$$t_" + pattern.Method.GetGenericArguments()[0].FullName;
            if (pattern.Arguments.Count > 0)
            {
                var constExpr = pattern.Arguments[0] as ConstantExpression;
                if (constExpr == null || constExpr.Type != typeof(string))
                {
                    throw new InvalidOperationException(
                        "Expression tree capture argument has to be a ConstantExpression of type string.");
                }
                if (constExpr.Value != null)
                    groupKey = (string)constExpr.Value;
            }
            return groupKey;
        }


        private static bool IsCaptureGroupMethod(MethodCallExpression methodCallExpr)
        {
            var methodToken = methodCallExpr.Method.UniqueToken();
            return methodToken == groupMethodToken || methodToken == groupUnnamedMethodToken;
        }


        private static Expression ReplaceParametersWithCaptureGroup(LambdaExpression expr)
        {
            var contextParam = expr.Parameters.FirstOrDefault(x => x.Type == typeof(IMatchContext))
                               ?? Expression.Parameter(typeof(IMatchContext));
            var visitedBody =
                new ReplaceParametersWithCaptureGroupsVisitor(
                    expr.Parameters.Where(x => x.Type != typeof(IMatchContext)),
                    contextParam).Visit(expr.Body);

            return visitedBody;
        }


        private bool Match(Expression pattern, Expression expr)
        {
            return (pattern == null && expr == null) ||
                   MatchCapture(pattern, expr) ||
                   pattern
                       .Maybe()
                       .Where(x => x.NodeType == expr.NodeType)
                       .Switch(y => y
                           .Case<MemberExpression>().Then(x => MatchMember(x, (MemberExpression)expr))
                           .Case<MethodCallExpression>().Then(x => MatchMethodCall(x, (MethodCallExpression)expr))
                           .Case<BinaryExpression>().Then(x => MatchBinary(x, (BinaryExpression)expr))
                           .Case<ConstantExpression>().Then(x => MatchConstant(x, (ConstantExpression)expr))
                           .Case<LambdaExpression>().Then(x => MatchLambda(x, (LambdaExpression)expr))
                           .Case<ParameterExpression>().Then(x => x.Type == expr.Type)
                           .Case<UnaryExpression>().Then(x => MatchUnary(x, (UnaryExpression)expr))
                           .Case<NewExpression>().Then(x => MatchNew(x, (NewExpression)expr))
                           .Case<NewArrayExpression>().Then(x => MatchNewArray(x, (NewArrayExpression)expr))
                           .Case<ConditionalExpression>().Then(x => MatchConditional(x, (ConditionalExpression)expr))
                           // TODO make sure parameters originates from correct place.
                           .Case(x => true /* Default case */).Then(
                               x =>
                               {
                                   throw new NotImplementedException("Match of Expression type " + x.GetType().FullName
                                                                     + " is not implemented.");
                               })
                       ).OrDefault();
        }


        private bool MatchBinary(BinaryExpression pattern, BinaryExpression expr)
        {
            return Match(pattern.Left, expr.Left) && Match(pattern.Right, expr.Right);
        }


        private bool MatchCapture(Expression pattern, Expression expr)
        {
            var methodCallExpr = pattern as MethodCallExpression;
            if (methodCallExpr != null && IsCaptureGroupMethod(methodCallExpr))
            {
                var groupKey = GetCaptureGroupKey(methodCallExpr);
                if (this.captures == null)
                    this.captures = new Dictionary<string, Expression>();
                this.captures[groupKey] = expr;
                return true;
            }
            return false;
        }


        private bool MatchConditional(ConditionalExpression pattern, ConditionalExpression expr)
        {
            return Match(pattern.Test, expr.Test) &&
                   Match(pattern.IfTrue, expr.IfTrue) &&
                   Match(pattern.IfFalse, pattern.IfFalse);
        }


        private bool MatchConstant(ConstantExpression pattern, ConstantExpression expr)
        {
            if (pattern.Value == null)
                return expr.Value == null;

            return pattern.Type == expr.Type && pattern.Value.Equals(expr.Value);
        }


        private bool MatchLambda(LambdaExpression pattern, LambdaExpression expr)
        {
            if (pattern.Type != expr.Type)
                return false;
            return Match(pattern.Body, expr.Body);
        }


        private bool MatchMember(MemberExpression pattern, MemberExpression expr)
        {
            if (pattern.Member != expr.Member)
                return false;
            return Match(pattern.Expression, expr.Expression);
        }


        private bool MatchMethodCall(MethodCallExpression pattern, MethodCallExpression expr)
        {
            return MatchCapture(pattern, expr) ||
                   pattern
                       .Maybe(x => x.Method == expr.Method)
                       .Where(x => Match(x.Object, expr.Object))
                       .Where(x => x.Arguments.Count == expr.Arguments.Count)
                       .Select(x => x.Arguments.Zip(expr.Arguments, Match).All(y => y))
                       .OrDefault();
        }


        private bool MatchNew(NewExpression pattern, NewExpression expr)
        {
            return
                pattern
                    .Maybe(x => x.Constructor == expr.Constructor)
                    .Where(x => x.Arguments.Count == expr.Arguments.Count)
                    .Select(x => x.Arguments.Zip(expr.Arguments, Match).All(y => y))
                    .OrDefault();
        }


        private bool MatchNewArray(NewArrayExpression pattern, NewArrayExpression expr)
        {
            return pattern.Expressions.Count == expr.Expressions.Count &&
                   pattern.Expressions.Zip(expr.Expressions, Match).All(x => x);
        }


        private bool MatchUnary(UnaryExpression pattern, UnaryExpression expr)
        {
            return pattern.Method == expr.Method && Match(pattern.Operand, expr.Operand);
        }

        #region Nested type: ReplaceParametersWithCaptureGroupsVisitor

        private class ReplaceParametersWithCaptureGroupsVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression matchContextParameter;

            private readonly IDictionary<ParameterExpression, Expression> paramGroups =
                new Dictionary<ParameterExpression, Expression>();

            private readonly List<ParameterExpression> replacedParameters = new List<ParameterExpression>();


            public ReplaceParametersWithCaptureGroupsVisitor(IEnumerable<ParameterExpression> replacedParameters,
                ParameterExpression matchContextParameter)
            {
                this.matchContextParameter = matchContextParameter;
                this.replacedParameters = new List<ParameterExpression>(replacedParameters);
            }


            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (this.matchContextParameter == node)
                    return node;

                var replacedParameterIndex = this.replacedParameters.IndexOf(node);

                if (replacedParameterIndex >= 0)
                {
                    return this.paramGroups.GetOrCreate(node,
                        () =>
                        {
                            var groupKey = "$$param_" + replacedParameterIndex;
                            return Expression.Call(this.matchContextParameter,
                                groupMethod.MakeGenericMethod(node.Type),
                                Expression.Constant(groupKey));
                        });
                }
                return base.VisitParameter(node);
            }
        }

        #endregion

        #region Nested type: RewriteVisitor

        private class RewriteVisitor : ExpressionVisitor
        {
            private readonly TreePatternMatcher parent;


            public RewriteVisitor(TreePatternMatcher parent)
            {
                if (parent == null)
                    throw new ArgumentNullException("matcher");
                this.parent = parent;
            }


            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (IsCaptureGroupMethod(node))
                {
                    var groupKey = GetCaptureGroupKey(node);
                    if (this.parent.captures == null)
                        throw new InvalidOperationException("No capture group found");
                    return this.parent.captures[groupKey];
                }
                return base.VisitMethodCall(node);
            }
        }

        #endregion
    }
}