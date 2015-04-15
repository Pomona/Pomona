#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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

using Nancy;

using Pomona.Common;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    internal class ServerDeserializationContext : ServerContainer, IDeserializationContext
    {
        private readonly IResourceResolver resourceResolver;
        private readonly IResourceNode targetNode;
        private readonly ITypeResolver typeMapper;


        public ServerDeserializationContext(ITypeResolver typeMapper,
            IResourceResolver resourceResolver,
            IResourceNode targetNode,
            NancyContext nancyContext)
            : base(nancyContext)
        {
            this.typeMapper = typeMapper;
            this.resourceResolver = resourceResolver;
            this.targetNode = targetNode;
        }


        public IResourceNode TargetNode
        {
            get { return this.targetNode; }
        }


        public void CheckAccessRights(PropertySpec property, HttpMethod method)
        {
            if (!property.AccessMode.HasFlag(method))
                throw new PomonaSerializationException("Unable to deserialize because of missing access: " + method);
        }


        public void CheckPropertyItemAccessRights(PropertySpec property, HttpMethod method)
        {
            if (!property.ItemAccessMode.HasFlag(method))
                throw new PomonaSerializationException("Unable to deserialize because of missing access: " + method);
        }


        public object CreateReference(IDeserializerNode node)
        {
            return this.resourceResolver.ResolveUri(node.Uri);
        }


        public object CreateResource<T>(StructuredType type, IConstructorPropertySource<T> args)
        {
            try
            {
                return type.Create(args);
            }
            catch (ArgumentException argumentException)
            {
                PropertySpec propertySpec;
                if (type.TryGetPropertyByName(argumentException.ParamName, true, out propertySpec))
                {
                    throw new ResourceValidationException(argumentException.Message,
                        propertySpec.Name,
                        propertySpec.ReflectedType.Name,
                        argumentException);
                }
                throw;
            }
        }


        public void Deserialize(IDeserializerNode node, Action<IDeserializerNode> nodeDeserializeAction)
        {
            nodeDeserializeAction(node);

            var transformedType = node.ValueType as StructuredType;
            if (transformedType != null && transformedType.OnDeserialized != null && node.Value != null)
                transformedType.OnDeserialized(node.Value);
        }


        public TypeSpec GetClassMapping(Type type)
        {
            return this.typeMapper.FromType(type);
        }


        public TypeSpec GetTypeByName(string typeName)
        {
            return this.typeMapper.FromType(typeName);
        }


        public void OnMissingRequiredPropertyError(IDeserializerNode node, PropertySpec targetProp)
        {
            throw new ResourceValidationException(
                string.Format("Property {0} is required when creating resource {1}",
                    targetProp.Name,
                    node.ValueType.Name),
                targetProp.Name,
                node.ValueType.Name,
                null);
        }


        public void SetProperty(IDeserializerNode targetNode, PropertySpec property, object propertyValue)
        {
            if (targetNode.Operation == DeserializerNodeOperation.Default)
                throw new InvalidOperationException("Invalid deserializer node operation default");
            if ((targetNode.Operation == DeserializerNodeOperation.Post
                 && property.AccessMode.HasFlag(HttpMethod.Post)) ||
                (targetNode.Operation == DeserializerNodeOperation.Patch
                 && property.AccessMode.HasFlag(HttpMethod.Put)))
                property.SetValue(targetNode.Value, propertyValue, targetNode.Context);
            else
            {
                var propPath = string.IsNullOrEmpty(targetNode.ExpandPath)
                    ? property.Name
                    : targetNode.ExpandPath + "." + property.Name;
                throw new ResourceValidationException(
                    string.Format("Property {0} of resource {1} is not writable.",
                        property.Name,
                        targetNode.ValueType.Name),
                    propPath,
                    targetNode.ValueType.Name,
                    null);
            }
        }
    }
}