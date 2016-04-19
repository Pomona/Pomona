#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pomona.Common.Linq
{
    internal class QueryTerminalSegmentExpression : QuerySegmentExpression
    {
        private readonly string value;


        public QueryTerminalSegmentExpression(string value, Type type = null, bool localExecutionPreferred = false)
            : base(type ?? typeof(string), localExecutionPreferred)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            this.value = value;
        }


        public override ReadOnlyCollection<object> Children
        {
            get { return new ReadOnlyCollection<object>(new object[] { this.value }); }
        }


        public override string ToString()
        {
            return this.value;
        }


        public override IEnumerable<string> ToStringSegments()
        {
            yield return this.value;
        }
    }
}