#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

namespace Pomona.Queries
{
    public enum NodeType
    {
        Unhandled,
        AndAlso,
        OrElse,
        Add,
        Subtract,
        Multiply,
        Divide,
        NumberLiteral,
        LessThan,
        Equal,
        CaseInsensitiveEqual,
        Root,
        Symbol,
        GreaterThan,
        StringLiteral,
        GuidLiteral,
        GreaterThanOrEqual,
        LessThanOrEqual,
        DateTimeLiteral,
        DateTimeOffsetLiteral,
        Dot,
        Modulo,
        NotEqual,
        MethodCall,
        IndexerAccess,
        Lambda,
        As,
        In,
        ArrayLiteral,
        Not,
        TypeNameLiteral
    }
}
