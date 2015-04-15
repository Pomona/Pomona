using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public class XDocContentContainer : XDocContentNode, IList<XDocContentNode>
    {
        new public XElement Node { get { return (XElement)base.Node; } }

        public XDocContentContainer() : this(new XElement("detached-content-container"))
        {
        }

        internal XDocContentContainer(XElement node)
            : base(node)
        {
        }


        private IEnumerable<XDocContentNode> WrapAll()
        {
            return Node.Nodes().Select(XDocContentNode.Wrap);
        }


        public IEnumerator<XDocContentNode> GetEnumerator()
        {
            return WrapAll().GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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


        public bool Remove(XDocContentNode item)
        {
            if (Contains(item))
            {
                item.Node.Remove();
                return true;
            }
            return false;
        }


        public int Count
        {
            get { return Node.Nodes().Count(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
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


        public void RemoveAt(int index)
        {
            var node = this[index];
            Remove(node);
        }


        public XDocContentNode this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException("index", "Index must be greater than or equal to zero.");
                var nodeAt = Node.Nodes().ElementAtOrDefault(index);
                if (nodeAt == null)
                    throw new ArgumentOutOfRangeException("index", "Index out of range.");
                return XDocContentNode.Wrap(nodeAt);
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this[index].Node.ReplaceWith(value.Node);
            }
        }
    }
}