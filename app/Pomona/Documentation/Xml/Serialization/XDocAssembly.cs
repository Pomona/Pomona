#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Xml.Linq;

namespace Pomona.Documentation.Xml.Serialization
{
    public class XDocAssembly : XDocElement
    {
        public XDocAssembly(XElement node)
            : base(node)
        {
        }


        public string Name
        {
            get { return GetOrAddElement("name").Value; }
            set { GetOrAddElement("name").Value = value; }
        }
    }
}