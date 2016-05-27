#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

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


        public ICollection<UrlSegment> Children
        {
            get
            {
                if (this.children == null)
                    this.children = CreateChildren().ToList().AsReadOnly();
                return this.children;
            }
        }

        public string Description => ToString();

        public bool Exists => Value != null;

        public IEnumerable<UrlSegment> FinalMatchCandidates
        {
            get { return Leafs.Where(x => x.IsLastSegment); }
        }

        public TypeSpec InputType => Route.InputType;

        public bool IsLastSegment => this.pathSegmentIndex == (this.pathSegments.Length - 1);

        public bool IsLeaf => Children.Count == 0;

        public bool IsRoot => Parent != null;

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

        public string PathSegment => this.pathSegmentIndex >= 0 ? this.pathSegments[this.pathSegmentIndex] : string.Empty;

        public string RelativePath => string.Join("/", this.pathSegments.Take(this.pathSegmentIndex + 1));

        public TypeSpec ResultItemType => Route.ResultItemType;

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

        public IPomonaSession Session => this.tree.Session;


        public object Get()
        {
            // TODO: Spread out async, return Task<object> instead
            return Task.Run(() => Session.Get(this)).Result;
        }


        public async Task<object> GetValueAsync()
        {
            if (!this.valueIsLoaded)
            {
                this.value = await Session.Get(this);
                this.valueIsLoaded = true;
            }
            return this.value;
        }


        public IQueryable Query()
        {
            // TODO: Spread out async, return Task<IQueryable> instead
            return Task.Run(() => Session.Query(this)).Result;
        }


        public override string ToString()
        {
            return string.Join("/",
                               this.AscendantsAndSelf().Select(
                                   x =>
                                       $"{x.GetPrefix()}{x.PathSegment}({x.GetTypeStringOfRoute()})").Reverse());
        }


        /// <summary>
        /// The actual result type, not only the expected one.
        /// </summary>
        internal async Task<TypeSpec> GetActualResultType()
        {
            if (this.actualResultType == null)
            {
                var resultItemTypeAsResourceType = ResultItemType as ResourceType;
                if (resultItemTypeAsResourceType != null && !resultItemTypeAsResourceType.MergedTypes.Any())
                {
                    this.actualResultType = ResultItemType;
                }
                else
                {
                    var val = await GetValueAsync();
                    this.actualResultType = val != null ? Session.TypeResolver.FromType(val.GetType()) : ResultType;
                }
            }
            return this.actualResultType;
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
            return $"{(Route.InputType != null ? Route.InputType.Name : "void")}=>{Route.ResultItemType.Name}{(Route.IsSingle ? "?" : "*")}";
        }


        public UrlSegment Parent { get; }

        public TypeSpec ResultType => Route.ResultType;

        public UrlSegment Root => this.tree.Root;

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

        IEnumerable<UrlSegment> ITreeNode<UrlSegment>.Children => Children;

        IResourceNode IResourceNode.Parent => Parent;
    }
}