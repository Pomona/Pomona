#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Pomona.Common.Internals
{
    public static class TreeNodeExtensions
    {
        public static IEnumerable<T> Ascendants<T>(this T node)
            where T : class, ITreeNode<T>
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            return node.Parent != null ? node.Parent.WalkTree(x => x.Parent) : Enumerable.Empty<T>();
        }


        public static IEnumerable<T> AscendantsAndSelf<T>(this T node)
            where T : class, ITreeNode<T>
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            return node.WalkTree(x => x.Parent);
        }


        public static IEnumerable<T> Descendants<T>(this T node)
            where T : class, ITreeNode<T>
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            return node.Children.SelectMany(x => x.WrapAsArray().Concat(x.Descendants()));
        }


        public static IEnumerable<T> DescendantsAndSelf<T>(this T node)
            where T : class, ITreeNode<T>
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            return node.WrapAsArray().Concat(node.Descendants());
        }


        public static T NextFork<T>(this T node)
            where T : class, ITreeNode<T>
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            return node.WalkTree(x => x.Children.SingleOrDefaultIfMultiple()).LastOrDefault(x => x.Children.Any());
        }


        public static T Root<T>(this T node)
            where T : class, ITreeNode<T>
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            var nodeWithRoot = node as ITreeNodeWithRoot<T>;
            if (nodeWithRoot != null)
                return nodeWithRoot.Root;
            return node.WalkTree(x => x.Parent).Last();
        }
    }
}

