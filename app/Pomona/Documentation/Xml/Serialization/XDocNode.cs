using System;
using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public abstract class XDocNode
    {
        private XNode node;
        public XNode Node { get { return this.node; } }

        protected XDocNode(XNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            this.node = node;
        }


        public override bool Equals(object obj)
        {
            return this.node.Equals(obj);
        }


        public override int GetHashCode()
        {
            return this.node.GetHashCode();
        }


        public override string ToString()
        {
            return this.node.ToString();
        }


        protected void AddOrReplaceElement(string name, XElement addedNode)
        {
            var el = (XElement)Node;
            var existingNode = el.Element(name);
            if (existingNode != null)
                existingNode.ReplaceWith(addedNode);
            else
            {
                el.Add(addedNode);
            }
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