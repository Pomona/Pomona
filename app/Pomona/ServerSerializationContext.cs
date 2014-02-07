#region License

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

#endregion

using System;
using System.Collections.Generic;

using Pomona.Common;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    public class ServerSerializationContext : ISerializationContext
    {
        private readonly bool debugMode;
        private readonly HashSet<string> expandedPaths;

        private readonly ITypeMapper typeMapper;
        private readonly IUriResolver uriResolver;


        public ServerSerializationContext(
            string expandedPaths,
            bool debugMode,
            IUriResolver uriResolver)
        {
            this.debugMode = debugMode;
            this.uriResolver = uriResolver;
            typeMapper = uriResolver.TypeMapper;
            this.expandedPaths = ExpandPathsUtils.GetExpandedPaths(expandedPaths);
        }


        public bool DebugMode
        {
            get { return debugMode; }
        }

        public ITypeMapper TypeMapper
        {
            get { return typeMapper; }
        }

        internal HashSet<string> ExpandedPaths
        {
            get { return expandedPaths; }
        }


        public TypeSpec GetClassMapping(Type type)
        {
            return typeMapper.GetClassMapping(type);
        }


        public string GetUri(PropertySpec property, object entity)
        {
            return uriResolver.GetUriFor(property, entity);
        }


        public string GetUri(object value)
        {
            return uriResolver.GetUriFor(value);
        }


        public bool PathToBeExpanded(string path)
        {
            if (path == string.Empty)
                return true;

            return expandedPaths.Contains(path.ToLower());
        }


        public void Serialize(ISerializerNode node, Action<ISerializerNode> nodeSerializerAction)
        {
            var isExpanded = node.ExpectedBaseType.IsAlwaysExpanded ||
                             PathToBeExpanded(node.ExpandPath) ||
                             (node.ExpectedBaseType.IsCollection && node.Context.PathToBeExpanded(node.ExpandPath + "!")) ||
                             IsAlwaysExpandedPropertyNode(node);

            node.SerializeAsReference = !isExpanded;

            nodeSerializerAction(node);
        }

        public bool PropertyIsSerialized(PropertySpec property)
        {
            var propMapping = property as PropertyMapping;
            if (propMapping != null && !propMapping.AccessMode.HasFlag(HttpMethod.Get))
                return false;
            return property.IsSerialized;
        }

        private bool IsAlwaysExpandedPropertyNode(ISerializerNode node)
        {
            if (node.ExpectedBaseType.IsCollection && node.ExpectedBaseType.ElementType.IsAlwaysExpanded)
                return true;

            if (node.ParentNode != null && node.ParentNode.ValueType.IsCollection &&
                IsAlwaysExpandedPropertyNode(node.ParentNode))
            {
                return true;
            }

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