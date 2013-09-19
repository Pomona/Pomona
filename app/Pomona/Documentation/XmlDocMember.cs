using System.Xml.Serialization;

namespace Pomona.Documentation
{
    public class XmlDocMember
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("summary")]
        public string Summary { get; set; }
    }
}