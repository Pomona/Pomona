#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public abstract class XDocContentElement : XDocContentNode
    {
        protected XDocContentElement(XNode node)
            : base(node)
        {
        }


        public new XElement Node => (XElement)base.Node;
    }
}
