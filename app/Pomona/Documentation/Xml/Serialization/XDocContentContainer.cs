#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public class XDocContentContainer : XDocContentNode, IList<XDocContentNode>
    {
        public XDocContentContainer()
            : this(new XElement("detached-content-container"))
        {
        }


        internal XDocContentContainer(XElement node)
            : base(node)
        {
        }


        public new XElement Node
        {
            get { return (XElement)base.Node; }
        }


        private IEnumerable<XDocContentNode> WrapAll()
        {
            return Node.Nodes().Select(Wrap);
        }


        public void Add(XDocContentNode item)
        {
            Node.Add(item.Node);
        }


        public void Clear()
        {
            Node.RemoveNodes();
        }


        public bool Contains(XDocContentNode item)
        {
            if (item == null)
                return false;
            return item.Node.Parent.Equals(Node);
        }


        public void CopyTo(XDocContentNode[] array, int arrayIndex)
        {
            WrapAll().ToList().CopyTo(array, arrayIndex);
        }


        public int Count
        {
            get { return Node.Nodes().Count(); }
        }


        public IEnumerator<XDocContentNode> GetEnumerator()
        {
            return WrapAll().GetEnumerator();
        }


        public int IndexOf(XDocContentNode item)
        {
            if (Contains(item))
                return Node.Nodes().TakeWhile(x => !x.Equals(item.Node)).Count();
            return -1;
        }


        public void Insert(int index, XDocContentNode item)
        {
            var nodeCurrentlyAtIndex = Node.Nodes().ElementAtOrDefault(index);
            if (nodeCurrentlyAtIndex != null)
                nodeCurrentlyAtIndex.AddBeforeSelf(item.Node);
            Node.Add(item.Node);
        }


        public bool IsReadOnly
        {
            get { return false; }
        }

        public XDocContentNode this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index must be greater than or equal to zero.");
                var nodeAt = Node.Nodes().ElementAtOrDefault(index);
                if (nodeAt == null)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index out of range.");
                return Wrap(nodeAt);
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                this[index].Node.ReplaceWith(value.Node);
            }
        }


        public bool Remove(XDocContentNode item)
        {
            if (Contains(item))
            {
                item.Node.Remove();
                return true;
            }
            return false;
        }


        public void RemoveAt(int index)
        {
            var node = this[index];
            Remove(node);
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}