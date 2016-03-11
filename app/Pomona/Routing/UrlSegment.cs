#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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

using Pomona.Common.Internals;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;

namespace Pomona.Routing
{
    public class UrlSegment : IResourceNode, ITreeNodeWithRoot<UrlSegment>
    {
        private readonly UrlSegment parent;
        private readonly int pathSegmentIndex;
        private readonly string[] pathSegments;
        private readonly Route route;
        private readonly RouteMatchTree tree;
        private TypeSpec actualResultType;
        private ReadOnlyCollection<UrlSegment> children;
        private UrlSegment selectedChild;
        private object value;
        private bool valueIsLoaded;


        internal UrlSegment(Route route, string[] pathSegments, RouteMatchTree tree)
            : this(route, pathSegments, -1, null, tree)
        {
        }


        private UrlSegment(Route route,
                           string[] pathSegments,
                           int pathSegmentIndex,
                           UrlSegment parent,
                           RouteMatchTree tree = null)
        {
            if (route == null)
                throw new ArgumentNullException(nameof(route));
            tree = tree ?? (parent != null ? parent.tree : null);
            if (tree == null)
                throw new ArgumentNullException(nameof(tree));
            if (parent != null && route.Parent != parent.route)
            {
                throw new ArgumentException(
                    "match child-parent relation must be same equivalent to route child-parent relation");
            }

            this.tree = tree;
            this.route = route;
            this.pathSegments = pathSegments;
            this.pathSegmentIndex = pathSegmentIndex;
            this.parent = parent;
        }


        /// <summary>
        /// The actual result type, not only the expected one.
        /// </summary>
        public TypeSpec ActualResultType
        {
            get
            {
                if (this.actualResultType == null)
                {
                    this.actualResultType = ResultItemType
                        .Maybe()
                        .Switch(x => x.Case<ResourceType>(y => !y.MergedTypes.Any()).Then(y => (TypeSpec)y))
                        .OrDefault(
                            () => Value != null ? Session.TypeMapper.FromType(Value.GetType()) : ResultType);
                }
                return this.actualResultType;
            }
        }

        public ICollection<UrlSegment> Children
        {
            get
            {
                if (this.children == null)
                    this.children = CreateChildren().ToList().AsReadOnly();
                return this.children;
            }
        }

        public string Description
        {
            get { return ToString(); }
        }

        public bool Exists
        {
            get
            {
                // TODO: BETTER WAY TO CHECK EXISTANCE
                return Value != null;
            }
        }

        public IEnumerable<UrlSegment> FinalMatchCandidates
        {
            get { return Leafs.Where(x => x.IsLastSegment); }
        }

        public TypeSpec InputType
        {
            get { return this.route.InputType; }
        }

        public bool IsLastSegment
        {
            get { return this.pathSegmentIndex == (this.pathSegments.Length - 1); }
        }

        public bool IsLeaf
        {
            get { return Children.Count == 0; }
        }

        public bool IsRoot
        {
            get { return this.parent != null; }
        }

        public IEnumerable<UrlSegment> Leafs
        {
            get
            {
                if (IsLeaf)
                    return this.WrapAsArray();
                return Children.SelectMany(x => x.Leafs);
            }
        }

        public UrlSegment NextConflict
        {
            get { return this.WalkTree(x => x.SelectedChild).Skip(1).LastOrDefault(x => !x.IsLastSegment && x.SelectedChild == null); }
        }

        public string PathSegment
        {
            get { return this.pathSegmentIndex >= 0 ? this.pathSegments[this.pathSegmentIndex] : string.Empty; }
        }

        public string RelativePath
        {
            get { return string.Join("/", this.pathSegments.Take(this.pathSegmentIndex + 1)); }
        }

        public TypeSpec ResultItemType
        {
            get { return this.route.ResultItemType; }
        }

        public Route Route
        {
            get { return this.route; }
        }

        public UrlSegment SelectedChild
        {
            get { return this.selectedChild ?? Children.SingleOrDefaultIfMultiple(); }
            set
            {
                if (this.selectedChild != null && this.selectedChild.Parent != this)
                    throw new InvalidOperationException("Parent route of selected child is incorrect.");
                this.selectedChild = value;
            }
        }

        public UrlSegment SelectedFinalMatch
        {
            get { return this.WalkTree(x => x.SelectedChild).LastOrDefault(x => x.IsLastSegment); }
        }

        public IPomonaSession Session
        {
            get { return this.tree.Session; }
        }


        public object Get()
        {
            return Session.Get(this);
        }


        public IQueryable Query()
        {
            return Session.Query(this);
        }


        public override string ToString()
        {
            return string.Join("/",
                               this.AscendantsAndSelf().Select(
                                   x =>
                                       string.Format("{0}{1}({2})",
                                                     x.GetPrefix(),
                                                     x.PathSegment,
                                                     x.GetTypeStringOfRoute())).Reverse());
        }


        private IEnumerable<UrlSegment> CreateChildren()
        {
            var childPathSegmentIndex = this.pathSegmentIndex + 1;
            if (childPathSegmentIndex < this.pathSegments.Length)
            {
                return
                    this.route.MatchChildren(this.pathSegments[childPathSegmentIndex]).Select(
                        x => new UrlSegment(x, this.pathSegments, childPathSegmentIndex, this)).Where(
                            x => x.Leafs.Any(y => y.IsLastSegment));
            }
            return Enumerable.Empty<UrlSegment>();
        }


        private string GetPrefix()
        {
            if (this.route is ResourcePropertyRoute)
                return "p:";
            if (this.route is GetByIdRoute)
                return "id:";
            return string.Empty;
        }


        private string GetTypeStringOfRoute()
        {
            return string.Format("{0}=>{1}{2}",
                                 this.route.InputType != null ? this.route.InputType.Name : "void",
                                 this.route.ResultItemType.Name,
                                 this.route.IsSingle ? "?" : "*");
        }


        public UrlSegment Parent
        {
            get { return this.parent; }
        }

        public TypeSpec ResultType
        {
            get { return this.route.ResultType; }
        }

        public UrlSegment Root
        {
            get { return this.tree.Root; }
        }

        public object Value
        {
            get
            {
                if (!this.valueIsLoaded)
                {
                    this.value = Get();
                    this.valueIsLoaded = true;
                }
                return this.value;
            }
        }

        IEnumerable<UrlSegment> ITreeNode<UrlSegment>.Children
        {
            get { return Children; }
        }

        IResourceNode IResourceNode.Parent
        {
            get { return Parent; }
        }
    }
}