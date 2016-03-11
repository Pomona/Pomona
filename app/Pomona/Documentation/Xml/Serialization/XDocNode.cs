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
using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public abstract class XDocNode
    {
        protected XDocNode(XNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            Node = node;
        }


        public XNode Node { get; private set; }


        public override bool Equals(object obj)
        {
            return Node.Equals(obj);
        }


        public override int GetHashCode()
        {
            return Node.GetHashCode();
        }


        public override string ToString()
        {
            return Node.ToString();
        }


        protected void AddOrReplaceElement(string name, XElement addedNode)
        {
            var el = (XElement)Node;
            var existingNode = el.Element(name);
            if (existingNode != null)
                existingNode.ReplaceWith(addedNode);
            else
                el.Add(addedNode);
        }


        protected string GetAttributeOrDefault(string name)
        {
            var el = (XElement)Node;
            var attribute = el.Attribute(name);
            return attribute != null ? attribute.Value : null;
        }


        protected XElement GetOrAddElement(string name)
        {
            var el = (XElement)Node;
            var childNode = el.Element(name);
            if (childNode == null)
            {
                childNode = new XElement(name);
                el.Add(childNode);
            }
            return childNode;
        }
    }
}