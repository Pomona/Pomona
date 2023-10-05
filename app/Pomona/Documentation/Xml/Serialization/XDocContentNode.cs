#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public abstract class XDocContentNode : XDocNode
    {
        protected XDocContentNode(XNode node)
            : base(node)
        {
        }


        internal static XDocContentNode Wrap(XNode node)
        {
            var element = node as XElement;
            if (element != null)
            {
                switch (element.Name.ToString())
                {
                    case "see":
                        return new XDocSee(element);
                    default:
                        return new XDocText(element.ToString());
                }
            }
            var text = node as XText;
            if (text != null)
                return new XDocText(text);
            throw new NotImplementedException("Don't know what to do with node " + node);
        }
    }
}

