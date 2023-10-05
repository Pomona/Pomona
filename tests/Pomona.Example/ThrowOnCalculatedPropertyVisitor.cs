#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Linq;
using System.Linq.Expressions;

using Pomona.Example.Models;

namespace Pomona.Example
{
    /// <summary>
    /// This is used to simulate the fact that generated properties can't be queried from DB.
    /// </summary>
    public class ThrowOnCalculatedPropertyVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            if (typeof(EntityBase).IsAssignableFrom(node.Expression.Type))
            {
                var memberInfo = node.Member;
                if (memberInfo.GetCustomAttributes(true).OfType<NotKnownToDataSourceAttribute>().Any())
                {
                    throw new InvalidOperationException("Property " + memberInfo.Name + " of entity " +
                                                        memberInfo.DeclaringType.Name +
                                                        " is read-only, and shouldn't be used in search.");
                }
            }
            return base.VisitMember(node);
        }
    }
}

