#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq
{
    internal abstract class QuerySegmentExpression : PomonaExtendedExpression
    {
        private string cachedQueryString;
        private bool? supportedOnServer;


        protected QuerySegmentExpression(Type type, bool localExecutionPreferred = false)
            : base(type)
        {
            LocalExecutionPreferred = localExecutionPreferred;
        }


        public override bool LocalExecutionPreferred { get; }

        public override bool SupportedOnServer
        {
            get
            {
                if (!this.supportedOnServer.HasValue)
                {
                    this.supportedOnServer =
                        Children.OfType<PomonaExtendedExpression>().Flatten(
                            x => x.Children.OfType<PomonaExtendedExpression>()).All(x => x.SupportedOnServer);
                }
                return this.supportedOnServer.Value;
            }
        }


        public override string ToString()
        {
            if (this.cachedQueryString == null)
                this.cachedQueryString = string.Concat(ToStringSegments());
            return this.cachedQueryString;
        }


        public abstract IEnumerable<string> ToStringSegments();


        protected static IEnumerable<string> ToStringSegmentsRecursive(IEnumerable<object> children)
        {
            foreach (var child in children)
            {
                var exprChild = child as QuerySegmentExpression;
                if (exprChild != null)
                {
                    foreach (var grandChild in exprChild.ToStringSegments())
                        yield return grandChild;
                }
                else
                    yield return child.ToString();
            }
        }
    }
}

