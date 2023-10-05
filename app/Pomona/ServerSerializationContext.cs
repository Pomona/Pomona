#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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
        private readonly IUriResolver uriResolver;


        public ServerSerializationContext(
            ITypeResolver typeMapper,
            string expandedPaths,
            bool debugMode,
            IUriResolver uriResolver,
            IContainer container
            )
        {
            if (typeMapper == null)
                throw new ArgumentNullException(nameof(typeMapper));
            if (expandedPaths == null)
                throw new ArgumentNullException(nameof(expandedPaths));
            if (uriResolver == null)
                throw new ArgumentNullException(nameof(uriResolver));
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            TypeMapper = typeMapper;
            DebugMode = debugMode;
            this.uriResolver = uriResolver;
            this.container = container;
            ExpandedPaths = ExpandPathsUtils.GetExpandedPaths(expandedPaths);
        }


        public bool DebugMode { get; }

        public ITypeResolver TypeMapper { get; }

        internal HashSet<string> ExpandedPaths { get; }


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
            return propNode.Property.ExpandMode;
        }


        public TypeSpec GetClassMapping(Type type)
        {
            return TypeMapper.FromType(type);
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

            return ExpandedPaths.Contains(path.ToLower());
        }


        public void Serialize(ISerializerNode node, Action<ISerializerNode> nodeSerializerAction)
        {
            var isExpanded = (node.ExpectedBaseType != typeof(object) && node.ExpectedBaseType != null
                              && node.ExpectedBaseType.IsAlwaysExpanded)
                             || PathToBeExpanded(node.ExpandPath)
                             || (node.ExpectedBaseType != null && node.ExpectedBaseType.IsCollection
                                 && node.Context.PathToBeExpanded(node.ExpandPath + "!"))
                             || (GetPropertyExpandMode(node) != ExpandMode.Default)
                             || (node.Value != null && node.ValueType != null && node.ValueType.IsAlwaysExpanded);

            node.SerializeAsReference = !isExpanded;

            nodeSerializerAction(node);
        }
    }
}

