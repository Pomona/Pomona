#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Routing
{
    public abstract class Route : ITreeNode<Route>
    {
        private readonly Lazy<ReadOnlyCollection<Route>> childrenSortedByPriority;
        private readonly Lazy<ReadOnlyDictionary<string, IEnumerable<Route>>> literalRouteMap;


        public Route(int priority, Route parent)
        {
            Priority = priority;
            Parent = parent;
            this.literalRouteMap = new Lazy<ReadOnlyDictionary<string, IEnumerable<Route>>>(LoadLiteralRouteMap,
                                                                                            LazyThreadSafetyMode
                                                                                                .PublicationOnly);
            this.childrenSortedByPriority =
                new Lazy<ReadOnlyCollection<Route>>(() => LoadChildren().OrderBy(x => x.Priority).ToList().AsReadOnly());
        }


        public abstract HttpMethod AllowedMethods { get; }
        public abstract TypeSpec InputType { get; }

        public bool IsRoot => Parent == null;

        public virtual bool IsSingle => !(ResultType is EnumerableTypeSpec);

        public PathNodeType NodeType => ResultType.IsCollection
            ? PathNodeType.Collection
            : ((ResultItemType is ResourceType) ? PathNodeType.Resource : PathNodeType.Custom);

        public int Priority { get; }

        public virtual TypeSpec ResultItemType => ResultType.GetItemType();

        public abstract TypeSpec ResultType { get; }

        private ReadOnlyDictionary<string, IEnumerable<Route>> LiteralRouteMap => this.literalRouteMap.Value;


        public IEnumerable<Route> MatchChildren(string pathSegment)
        {
            IEnumerable<Route> matchChildren;
            if (TryMatchLiteralChild(pathSegment, out matchChildren))
                return matchChildren;

            return Children.Where(x => x.Match(pathSegment));
        }


        public override sealed string ToString()
        {
            if (Parent != null)
                return Parent.ToString() + "/" + PathSegmentToString();
            return PathSegmentToString();
        }


        protected abstract IEnumerable<Route> LoadChildren();
        protected abstract bool Match(string pathSegment);
        protected abstract string PathSegmentToString();


        private ReadOnlyDictionary<string, IEnumerable<Route>> LoadLiteralRouteMap()
        {
            var comparer = StringComparer.InvariantCultureIgnoreCase;
            var lowestPriority = Children.Min(x => x.Priority);

            return
                Children
                    .Where(x => x.Priority == lowestPriority)
                    .OfType<ILiteralRoute>()
                    .GroupBy(x => x.MatchValue, comparer)
                    .ToDictionary(x => x.Key, x => (IEnumerable<Route>)x.Cast<Route>().ToList(), comparer)
                    .AsReadOnly();
        }


        private bool TryMatchLiteralChild(string pathSegment,
                                          out IEnumerable<Route> matchChildren)
        {
            return LiteralRouteMap.TryGetValue(pathSegment, out matchChildren);
        }


        public IEnumerable<Route> Children => this.childrenSortedByPriority.Value;

        public Route Parent { get; }
    }
}