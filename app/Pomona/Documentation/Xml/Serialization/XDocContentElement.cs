using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public abstract class XDocContentElement : XDocContentNode
    {
        new public XElement Node { get { return (XElement)base.Node; } }

        protected XDocContentElement(XNode node)
            : base(node)
        {
        }
    }
}