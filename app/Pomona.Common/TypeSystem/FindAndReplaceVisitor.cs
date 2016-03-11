#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Linq.Expressions;

namespace Pomona.Common.TypeSystem
{
    public class FindAndReplaceVisitor : ExpressionVisitor
    {
        private readonly Expression expressionToReplace;
        private readonly Expression replacementExpression;


        public FindAndReplaceVisitor(Expression expressionToReplace, Expression replacementExpression)
        {
            this.expressionToReplace = expressionToReplace;
            this.replacementExpression = replacementExpression;
        }


        public static Expression Replace(Expression searchedExpression,
                                         Expression expressionToReplace,
                                         Expression replacementExpression)
        {
            var visitor = new FindAndReplaceVisitor(expressionToReplace, replacementExpression);
            return visitor.Visit(searchedExpression);
        }


        public override Expression Visit(Expression node)
        {
            return node == this.expressionToReplace ? this.replacementExpression : base.Visit(node);
        }
    }
}