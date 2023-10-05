#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Pomona.Common.TypeSystem;
using Pomona.Documentation.Nodes;
using Pomona.Documentation.Xml.Serialization;

namespace Pomona.Documentation.Xml
{
    internal class XmlDocMapper
    {
        private readonly List<Assembly> assembliesToSearch;
        private readonly IResourceTypeResolver typeMapper;


        public XmlDocMapper(IResourceTypeResolver typeMapper)
        {
            this.typeMapper = typeMapper;
            this.assembliesToSearch = ((TypeMapper)typeMapper).SourceTypes.Select(x => x.Type.Assembly).Distinct().ToList();
        }


        public IDocNode Map(XDocContentNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            var textNode = node as XDocText;
            if (textNode != null)
                return new TextNode(textNode.Value);
            var container = node as XDocContentContainer;
            if (container != null)
                return new DocContainerNode(container.Select(Map));
            var seeNode = node as XDocSee;
            if (seeNode != null)
            {
                var member = ResolveMember(seeNode.Cref);
                return member != null ? (IDocNode)new SeeNode(member) : new UnresolvedSeeNode(seeNode.Cref);
            }
            throw new NotImplementedException("Does not know how to map node of type " + node.GetType());
        }


        private MemberSpec ResolveMember(string cref)
        {
            if (cref == null)
                throw new ArgumentNullException(nameof(cref));
            if (cref.Length < 3 || cref[1] != ':')
                throw new ArgumentException("cref not valid.");
            var memberName = cref.Substring(2);
            var memberPrefix = cref[0];
            switch (memberPrefix)
            {
                case 'T':
                    return ResolveType(memberName);
                case 'P':
                    return ResolveProperty(memberName);
                default:
                    //throw new InvalidOperationException("Unable to resolve member with type code " + memberPrefix);
                    return null;
            }
        }


        private PropertySpec ResolveProperty(string fullName)
        {
            var typeNameSplitIndex = fullName.LastIndexOf('.');
            var typeName = fullName.Substring(0, typeNameSplitIndex);
            var propName = fullName.Substring(typeNameSplitIndex + 1);
            var type = ResolveType(typeName);
            if (type == null)
                return null;
            var prop = type.Properties.FirstOrDefault(x => x.PropertyInfo.Name == propName);
            return prop;
        }


        private TypeSpec ResolveType(string fullName)
        {
            var t = Type.GetType(fullName) ?? this.assembliesToSearch.Select(x => x.GetType(fullName)).FirstOrDefault(x => x != null);
            return t != null ? this.typeMapper.FromType(t) : null;
        }
    }
}
