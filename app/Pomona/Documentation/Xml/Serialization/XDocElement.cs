using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public abstract class XDocElement : XDocNode
    {
        new public XElement Node { get { return (XElement)base.Node; } }

        protected XDocElement(XElement node)
            : base(node)
        {
        }

    }
}