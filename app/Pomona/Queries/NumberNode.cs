#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Globalization;
using System.Linq;

namespace Pomona.Queries
{
    internal class NumberNode : NodeBase
    {
        public NumberNode(string value)
            : base(NodeType.NumberLiteral, Enumerable.Empty<NodeBase>())
        {
            Value = value;
        }


        public string Value { get; }


        public object Parse()
        {
            var lastCharacter = Value[Value.Length - 1];
            if (lastCharacter == 'm' || lastCharacter == 'M')
                return decimal.Parse(Value.Substring(0, Value.Length - 1), CultureInfo.InvariantCulture);
            if (lastCharacter == 'f' || lastCharacter == 'F')
                return float.Parse(Value.Substring(0, Value.Length - 1), CultureInfo.InvariantCulture);
            if (lastCharacter == 'L')
                return Int64.Parse(Value.Substring(0, Value.Length - 1), CultureInfo.InvariantCulture);

            var parts = Value.Split('.');
            if (parts.Length == 1)
                return int.Parse(parts[0], CultureInfo.InvariantCulture);
            if (parts.Length == 2)
                return double.Parse(Value, CultureInfo.InvariantCulture);

            throw new InvalidOperationException("Unable to parse " + Value);
        }


        public override string ToString()
        {
            return base.ToString() + " " + Value;
        }
    }
}

