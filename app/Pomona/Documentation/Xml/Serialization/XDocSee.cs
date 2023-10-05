#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public class XDocSee : XDocContentElement
    {
        public XDocSee(string cref)
            : base(new XElement("see"))
        {
            if (cref == null)
                throw new ArgumentNullException(nameof(cref));
            Cref = cref;
        }


        internal XDocSee(XElement node)
            : base(node)
        {
        }


        public string Cref
        {
            get { return GetAttributeOrDefault("cref"); }
            set { Node.SetAttributeValue("cref", value); }
        }
    }
}

