using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public class XDocText : XDocContentNode
    {
        public XDocText(string text) : this(new XText(text))
        {
        }

        public XDocText(XText node)
            : base(node)
        {
        }

        public string Value { get { return ((XText)Node).Value; } }

    }
}