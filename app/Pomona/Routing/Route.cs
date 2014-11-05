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

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Routing
{
    public abstract class Route : ITreeNode<Route>
    {
        private readonly Lazy<ReadOnlyCollection<Route>> childrenSortedByPriority;
        private readonly Route parent;
        private readonly int priority;
        private Dictionary<string, Route> literalRouteDict;


        public Route(int priority, Route parent)
        {
            this.priority = priority;
            this.parent = parent;
            this.literalRouteDict = new Dictionary<string, Route>();
            this.childrenSortedByPriority =
                new Lazy<ReadOnlyCollection<Route>>(() => LoadChildren().OrderBy(x => x.priority).ToList().AsReadOnly());
        }


        public virtual bool IsSingle
        {
            get { return !(ResultType is EnumerableTypeSpec); }
        }

        public virtual TypeSpec ResultItemType
        {
            get { return ResultType.GetItemType(); }
        }

        public abstract HttpMethod AllowedMethods { get; }

        public IEnumerable<Route> Children
        {
            get { return this.childrenSortedByPriority.Value; }
        }

        public abstract TypeSpec InputType { get; }

        public bool IsRoot
        {
            get { return this.parent == null; }
        }

        public PathNodeType NodeType
        {
            get
            {
                return ResultType.IsCollection
                    ? PathNodeType.Collection
                    : ((ResultItemType is ResourceType) ? PathNodeType.Resource : PathNodeType.Custom);
            }
        }

        public Route Parent
        {
            get { return this.parent; }
        }

        public int Priority
        {
            get { return this.priority; }
        }

        public abstract TypeSpec ResultType { get; }


        public override sealed string ToString()
        {
            if (this.parent != null)
                return this.parent.ToString() + "/" + PathSegmentToString();
            return PathSegmentToString();
        }


        public IEnumerable<Route> MatchChildren(string pathSegment)
        {
            IEnumerable<Route> matchChildren;
            if (TryMatchLiteralChild(pathSegment, out matchChildren))
                return matchChildren;

            return Children.Where(x => x.Match(pathSegment));
        }


        protected abstract IEnumerable<Route> LoadChildren();
        protected abstract bool Match(string pathSegment);

        protected abstract string PathSegmentToString();


        private bool TryMatchLiteralChild(string pathSegment,
                                          out IEnumerable<Route> matchChildren)
        {
            matchChildren = null;
            if (this.literalRouteDict == null && Children.Any())
            {
                this.literalRouteDict = new Dictionary<string, Route>(StringComparer.InvariantCultureIgnoreCase);
                var lowestPriority = Children.Min(x => x.Priority);
                foreach (var literalRoute in Children.Where(x => x.Priority == lowestPriority).OfType<ILiteralRoute>())
                    this.literalRouteDict[literalRoute.MatchValue] = (Route)literalRoute;
            }

            if (this.literalRouteDict != null)
            {
                Route route;
                if (this.literalRouteDict.TryGetValue(pathSegment, out route))
                {
                    matchChildren = route.WrapAsArray();
                    return true;
                }
            }
            return false;
        }
    }
}