#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

namespace Pomona.Queries
{
    internal class DateTimeNode : LiteralNode<DateTime>
    {
        public DateTimeNode(DateTime value)
            : base(NodeType.DateTimeLiteral, value)
        {
        }
    }
}

