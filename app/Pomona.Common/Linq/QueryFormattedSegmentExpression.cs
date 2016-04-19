#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Pomona.Common.Linq
{
    internal class QueryFormattedSegmentExpression : QuerySegmentExpression
    {
        private readonly object[] children;
        private readonly string format;


        public QueryFormattedSegmentExpression(Type type, string format, object[] args, bool localExecutionPreferred)
            : base(type, localExecutionPreferred)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            this.format = format;
            this.children = (object[])args.Clone();
        }


        public override ReadOnlyCollection<object> Children
        {
            get { return new ReadOnlyCollection<object>(this.children); }
        }


        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, this.format, this.children);
        }


        public override IEnumerable<string> ToStringSegments()
        {
            yield return ToString();
        }
    }
}