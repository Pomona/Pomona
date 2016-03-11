#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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
        private readonly int pathSegmentIndex;
        private readonly string[] pathSegments;
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
            if (parent != null && route.Parent != parent.Route)
            {
                throw new ArgumentException(
                    "match child-parent relation must be same equivalent to route child-parent relation");
            }

            this.tree = tree;
            Route = route;
            this.pathSegments = pathSegments;
            this.pathSegmentIndex = pathSegmentIndex;
            Parent = parent;
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
                            () => Value != null ? Session.TypeResolver.FromType(Value.GetType()) : ResultType);
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
            get { return Route.InputType; }
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
            get { return Parent != null; }
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
            get { return Route.ResultItemType; }
        }

        public Route Route { get; }

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
                    Route.MatchChildren(this.pathSegments[childPathSegmentIndex]).Select(
                        x => new UrlSegment(x, this.pathSegments, childPathSegmentIndex, this)).Where(
                            x => x.Leafs.Any(y => y.IsLastSegment));
            }
            return Enumerable.Empty<UrlSegment>();
        }


        private string GetPrefix()
        {
            if (Route is ResourcePropertyRoute)
                return "p:";
            if (Route is GetByIdRoute)
                return "id:";
            return string.Empty;
        }


        private string GetTypeStringOfRoute()
        {
            return string.Format("{0}=>{1}{2}",
                                 Route.InputType != null ? Route.InputType.Name : "void",
                                 Route.ResultItemType.Name,
                                 Route.IsSingle ? "?" : "*");
        }


        public UrlSegment Parent { get; }

        public TypeSpec ResultType
        {
            get { return Route.ResultType; }
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