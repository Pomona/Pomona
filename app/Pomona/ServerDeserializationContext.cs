#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Threading.Tasks;

using Pomona.Common;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;

namespace Pomona
{
    internal class ServerDeserializationContext : IDeserializationContext
    {
        private readonly IContainer container;
        private readonly IResourceResolver resourceResolver;
        private readonly ITypeResolver typeMapper;


        public ServerDeserializationContext(ITypeResolver typeMapper,
                                            IResourceResolver resourceResolver,
                                            IResourceNode targetNode,
                                            IContainer container)
        {
            this.typeMapper = typeMapper;
            this.resourceResolver = resourceResolver;
            TargetNode = targetNode;
            this.container = container;
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
            // TODO: spread async everywhere
            return this.resourceResolver.ResolveUri(node.Uri);
        }


        public object CreateResource(TypeSpec type, IConstructorPropertySource args)
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


        public T GetInstance<T>()
        {
            return this.container.GetInstance<T>();
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


        public IResourceNode TargetNode { get; }
    }
}