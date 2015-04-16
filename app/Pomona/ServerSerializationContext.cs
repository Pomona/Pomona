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

using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    internal class ServerSerializationContext : ISerializationContext
    {
        private readonly IContainer container;
        private readonly bool debugMode;
        private readonly HashSet<string> expandedPaths;
        private readonly ITypeResolver typeMapper;
        private readonly IUriResolver uriResolver;


        public ServerSerializationContext(
            TypeMapper typeMapper,
            string expandedPaths,
            bool debugMode,
            IUriResolver uriResolver,
            IContainer container
            )
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            if (expandedPaths == null)
                throw new ArgumentNullException("expandedPaths");
            if (uriResolver == null)
                throw new ArgumentNullException("uriResolver");
            if (container == null)
                throw new ArgumentNullException("container");
            this.typeMapper = typeMapper;
            this.debugMode = debugMode;
            this.uriResolver = uriResolver;
            this.container = container;
            this.expandedPaths = ExpandPathsUtils.GetExpandedPaths(expandedPaths);
        }


        public bool DebugMode
        {
            get { return this.debugMode; }
        }

        public ITypeResolver TypeMapper
        {
            get { return this.typeMapper; }
        }

        internal HashSet<string> ExpandedPaths
        {
            get { return this.expandedPaths; }
        }


        private ExpandMode GetPropertyExpandMode(ISerializerNode node)
        {
            if (node.ExpectedBaseType.IsCollection && node.ExpectedBaseType.ElementType.IsAlwaysExpanded)
                return ExpandMode.Full;

            if (node.ParentNode != null && node.ParentNode.ValueType.IsCollection &&
                GetPropertyExpandMode(node.ParentNode) == ExpandMode.Full)
                return ExpandMode.Full;

            var propNode = node as PropertyValueSerializerNode;
            if (propNode == null)
                return ExpandMode.Default;
            var propMapping = propNode.Property as StructuredProperty;
            if (propMapping == null)
                return ExpandMode.Default;
            return propMapping.ExpandMode;
        }


        public TypeSpec GetClassMapping(Type type)
        {
            return this.typeMapper.FromType(type);
        }


        public T GetInstance<T>()
        {
            return this.container.GetInstance<T>();
        }


        public string GetUri(PropertySpec property, object entity)
        {
            return this.uriResolver.GetUriFor(property, entity);
        }


        public string GetUri(object value)
        {
            return this.uriResolver.GetUriFor(value);
        }


        public bool PathToBeExpanded(string path)
        {
            if (path == string.Empty)
                return true;

            return this.expandedPaths.Contains(path.ToLower());
        }


        public void Serialize(ISerializerNode node, Action<ISerializerNode> nodeSerializerAction)
        {
            var isExpanded = node.ExpectedBaseType.IsAlwaysExpanded
                             || PathToBeExpanded(node.ExpandPath)
                             || (node.ExpectedBaseType.IsCollection && node.Context.PathToBeExpanded(node.ExpandPath + "!"))
                             || (GetPropertyExpandMode(node) != ExpandMode.Default);

            node.SerializeAsReference = !isExpanded;

            nodeSerializerAction(node);
        }
    }
}