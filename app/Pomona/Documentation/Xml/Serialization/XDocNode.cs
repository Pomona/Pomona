#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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