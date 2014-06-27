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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

using Pomona.Common.Internals;

namespace Pomona.Common.Linq
{
    internal class QuerySelectExpression : QuerySegmentExpression
    {
        private readonly IEnumerable<KeyValuePair<string, PomonaExtendedExpression>> selectList;
        private ReadOnlyCollection<object> children;

        public QuerySelectExpression(IEnumerable<KeyValuePair<string, PomonaExtendedExpression>> selectList, Type type)
            : base(type)
        {
            this.selectList = selectList;
        }


        private IEnumerable<object> GetChildren()
        {
            int i = 0;
            foreach (var kvp in selectList)
            {
                if (i != 0)
                    yield return ",";
                yield return kvp.Value;
                yield return " as ";
                yield return kvp.Key;
                i++;
            }
        }

        public override ReadOnlyCollection<object> Children
        {
            get
            {
                if (children == null)
                    children = new ReadOnlyCollection<object>(GetChildren().ToList());
                return children;
            }
        }

        public override IEnumerable<string> ToStringSegments()
        {
            return ToStringSegmentsRecursive(GetChildren());
        }
    }
    internal abstract class QuerySegmentExpression : PomonaExtendedExpression
    {
        private readonly bool localExecutionPreferred;
        private bool? supportedOnServer;
        private string cachedQueryString;


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

        protected QuerySegmentExpression(Type type, bool localExecutionPreferred = false)
            : base(type)
        {
            this.localExecutionPreferred = localExecutionPreferred;
        }


        public override bool LocalExecutionPreferred
        {
            get { return localExecutionPreferred; }
        }

        public override bool SupportedOnServer
        {
            get
            {
                if (!supportedOnServer.HasValue)
                {
                    supportedOnServer =
                        Children.OfType<PomonaExtendedExpression>().Flatten(
                            x => x.Children.OfType<PomonaExtendedExpression>()).All(x => x.SupportedOnServer);
                }
                return this.supportedOnServer.Value;
            }
        }


        public abstract IEnumerable<string> ToStringSegments();


        public override string ToString()
        {
            if (cachedQueryString == null)
                cachedQueryString = string.Concat(ToStringSegments());
            return cachedQueryString;
        }
    }
}