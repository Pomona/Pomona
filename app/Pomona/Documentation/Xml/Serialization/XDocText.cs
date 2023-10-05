#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public class XDocText : XDocContentNode
    {
        public XDocText(string text)
            : this(new XText(text))
        {
        }


        public XDocText(XText node)
            : base(node)
        {
        }


        public string Value => ((XText)Node).Value;
    }
}

