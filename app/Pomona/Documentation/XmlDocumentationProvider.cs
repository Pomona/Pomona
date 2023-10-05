#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

using Pomona.Common;
using Pomona.Common.TypeSystem;
using Pomona.Documentation.Nodes;
using Pomona.Documentation.Xml;
using Pomona.Documentation.Xml.Serialization;

namespace Pomona.Documentation
{
    public class XmlDocumentationProvider : IDocumentationProvider
    {
        private readonly XmlDocMapper mapper;
        private readonly Dictionary<string, XDoc> xmlDocs = new Dictionary<string, XDoc>();


        public XmlDocumentationProvider(IResourceTypeResolver typeMapper)
        {
            this.mapper = new XmlDocMapper(typeMapper);
        }


        private IDocNode GetMemberSummary(MemberInfo member)
        {
            var xdoc = this.xmlDocs.GetOrCreate(member.Module.Assembly.FullName, () => LoadXmlDoc(member));
            if (xdoc == null)
                return null;
            var xDocContentContainer = xdoc.GetSummary(member);
            if (xDocContentContainer == null)
                return null;
            return this.mapper.Map(xDocContentContainer);
        }


        private static XDoc LoadXmlDoc(MemberInfo member)
        {
            var xmlDocFileName = member.Module.Assembly.GetName().Name + ".xml";
            if (File.Exists(xmlDocFileName))
            {
                using (var stream = File.OpenRead(xmlDocFileName))
                {
                    return new XDoc(XDocument.Load(stream).Root);
                }
            }
            return null;
        }


        public IDocNode GetSummary(MemberSpec member)
        {
            return GetMemberSummary(member.Member);
        }
    }
}

