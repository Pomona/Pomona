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
                    throw new ArgumentOutOfRangeException("index", "Index must be greater than or equal to zero.");
                var nodeAt = Node.Nodes().ElementAtOrDefault(index);
                if (nodeAt == null)
                    throw new ArgumentOutOfRangeException("index", "Index out of range.");
                return Wrap(nodeAt);
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
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