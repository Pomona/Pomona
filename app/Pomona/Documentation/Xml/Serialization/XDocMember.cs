using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public class XDocMember : XDocElement
    {
        public XDocMember(XElement node)
            : base(node)
        {
        }


        public XDocMember()
            : base(new XElement("member"))
        {
        }

        public string Name
        {
            get { return Node.Attribute("name").Value; }
            set { Node.SetAttributeValue("name", value); }
        }

        protected XElement GetOrAddLazyAttachedElement(string name)
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


        private XDocContentContainer GetDocSection(string name)
        {
            var el = Node.Element(name);
            return el != null ? new XDocContentContainer(el) : null;
        }


        private void SetDocSection(string name, XDocContentContainer content)
        {
            Node.Elements(name).Remove();
            if (content != null)
            {
                content.Node.Name = name;
                Node.Add(content.Node);
            }
        }

        public XDocContentContainer Summary
        {
            get { return GetDocSection("summary"); }
            set { SetDocSection("summary", value); }
        }
    }
}