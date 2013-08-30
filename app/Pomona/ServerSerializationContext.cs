// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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

using System;
using System.Collections.Generic;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class ServerSerializationContext : ISerializationContext
    {
        private readonly bool debugMode;
        private readonly HashSet<string> expandedPaths;
        private readonly IPomonaSession session;

        private readonly TypeMapper typeMapper;


        public ServerSerializationContext(
            string expandedPaths,
            bool debugMode,
            IPomonaSession session)
        {
            this.debugMode = debugMode;
            this.session = session;
            typeMapper = session.TypeMapper;
            this.expandedPaths = ExpandPathsUtils.GetExpandedPaths(expandedPaths);
        }


        public bool DebugMode
        {
            get { return debugMode; }
        }

        public TypeMapper TypeMapper
        {
            get { return typeMapper; }
        }

        internal HashSet<string> ExpandedPaths
        {
            get { return expandedPaths; }
        }


        public IMappedType GetClassMapping(Type type)
        {
            return typeMapper.GetClassMapping(type);
        }


        public string GetUri(IPropertyInfo property, object entity)
        {
            return session.GetUri(property, entity);
        }


        public string GetUri(object value)
        {
            return session.GetUri(value);
        }


        public bool PathToBeExpanded(string path)
        {
            if (path == string.Empty)
                return true;

            return expandedPaths.Contains(path.ToLower());
        }


        public void Serialize<TWriter>(ISerializerNode node, ISerializer<TWriter> serializer, TWriter writer)
            where TWriter : ISerializerWriter
        {
            var isExpanded = node.ExpectedBaseType.IsAlwaysExpanded ||
                             PathToBeExpanded(node.ExpandPath) ||
                             (node.ExpectedBaseType.IsCollection && node.Context.PathToBeExpanded(node.ExpandPath + "!")) ||
                             IsAlwaysExpandedPropertyNode(node);

            node.SerializeAsReference = !isExpanded;

            serializer.SerializeNode(node, writer);
        }

        public bool PropertyIsSerialized(IPropertyInfo property)
        {
            return property.IsSerialized;
        }

        private bool IsAlwaysExpandedPropertyNode(ISerializerNode node)
        {
            var propNode = node as PropertyValueSerializerNode;
            if (propNode == null)
                return false;
            var propMapping = propNode.Property as PropertyMapping;
            if (propMapping == null)
                return false;
            return propMapping.AlwaysExpand;
        }
    }
}