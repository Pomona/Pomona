#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

namespace Pomona.Queries
{
    internal class DateTimeOffsetNode : LiteralNode<DateTimeOffset>
    {
        public DateTimeOffsetNode(DateTimeOffset value)
            : base(NodeType.DateTimeOffsetLiteral, value)
        {
        }
    }
}

