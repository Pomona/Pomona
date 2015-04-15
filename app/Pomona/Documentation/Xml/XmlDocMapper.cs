#region License
// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Pomona.Common.TypeSystem;
using Pomona.Documentation.Nodes;
using Pomona.Documentation.Xml.Serialization;

namespace Pomona.Documentation.Xml
{
    internal class XmlDocMapper
    {
        private readonly IResourceTypeResolver typeMapper;
        private readonly List<Assembly> assembliesToSearch;


        public XmlDocMapper(IResourceTypeResolver typeMapper)
        {
            this.typeMapper = typeMapper;
            this.assembliesToSearch = ((TypeMapper)typeMapper).SourceTypes.Select(x => x.Type.Assembly).Distinct().ToList();
        }


        public IDocNode Map(XDocContentNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");
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
                throw new ArgumentNullException("cref");
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
            var t = Type.GetType(fullName) ?? assembliesToSearch.Select(x => x.GetType(fullName)).FirstOrDefault(x => x != null);
            return t != null ? typeMapper.FromType(t) : null;
        }
    }
}