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
using System.IO;
using System.Linq;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Json
{
    public class PomonaJsonDeserializer : ITextDeserializer
    {
        private readonly ISerializationContextProvider contextProvider;

        private static readonly Action<Type, PomonaJsonDeserializer, IDeserializerNode, Reader>
            deserializeArrayNodeGenericMethod =
                GenericInvoker.Instance<PomonaJsonDeserializer>().CreateAction1<IDeserializerNode, Reader>(
                    x => x.DeserializeArrayNodeGeneric<object>(null, null));

        private static readonly Action<Type, PomonaJsonDeserializer, IDeserializerNode, Reader, DictionaryTypeSpec>
            deserializeDictionaryGenericMethod =
                GenericInvoker.Instance<PomonaJsonDeserializer>()
                    .CreateAction1<IDeserializerNode, Reader, DictionaryTypeSpec>(
                        x => x.DeserializeDictionaryGeneric<object>(null, null, null));

        private static readonly char[] reservedFirstCharacters = "^-*!".ToCharArray();

        private static Func<Type, PomonaJsonDeserializer, JObject, IDeserializerNode, IJsonPropertyValueSource>
            createPropertyValueSourceMethod =
                GenericInvoker.Instance<PomonaJsonDeserializer>().CreateFunc1<JObject, IDeserializerNode, IJsonPropertyValueSource>(
                    x => x.CreatePropertyValueSource<object>(null, null));

        private readonly JsonSerializer jsonSerializer;


        public PomonaJsonDeserializer(ISerializationContextProvider contextProvider)
        {
            if (contextProvider == null)
                throw new ArgumentNullException("contextProvider");
            this.contextProvider = contextProvider;
            this.jsonSerializer = new JsonSerializer();
            this.jsonSerializer.Converters.Add(new StringEnumConverter());
        }


        private Reader CreateReader(TextReader textReader)
        {
            return new Reader(JToken.ReadFrom(new JsonTextReader(textReader)));
        }


        private object Deserialize(TextReader textReader,
            TypeSpec expectedBaseType,
            IDeserializationContext context,
            object patchedObject)
        {
            var node = new ItemValueDeserializerNode(expectedBaseType, context);
            node.Operation = patchedObject != null ? DeserializerNodeOperation.Patch : DeserializerNodeOperation.Post;
            if (patchedObject != null)
                node.Value = patchedObject;

            var reader = CreateReader(textReader);
            DeserializeThroughContext(node, reader);
            return node.Value;
        }


        private void DeserializeThroughContext(IDeserializerNode node, Reader reader)
        {
            node.Context.Deserialize(node, n => DeserializeNode(n, reader));
        }


        private void DeserializeNode(IDeserializerNode node, Reader reader)
        {
            if (reader.Token.Type == JTokenType.Null)
            {
                node.Value = null;
                return;
            }

            TypeSpec mappedType;
            if (node.ExpectedBaseType != null)
                mappedType = node.ExpectedBaseType;
            else if (reader.Token.Type == JTokenType.String)
            {
                node.SetValueType(node.Context.GetClassMapping(typeof(string)));
                mappedType = node.ValueType;
            }
            else
            {
                if (!SetNodeValueType(node, reader.Token as JObject))
                {
                    throw new PomonaSerializationException(
                        "No expected type to deserialize to provided, and unable to get type from incoming JSON.");
                }
                mappedType = node.ValueType;
            }

            switch (mappedType.SerializationMode)
            {
                case TypeSerializationMode.Complex:
                    DeserializeComplexNode(node, reader);
                    break;
                case TypeSerializationMode.Value:
                    DeserializeValueNode(node, reader);
                    break;
                case TypeSerializationMode.Array:
                    DeserializeArrayNode(node, reader);
                    break;
                case TypeSerializationMode.Dictionary:
                    DeserializeDictionary(node, reader);
                    break;
                default:
                    throw new NotImplementedException("Don't know how to deserialize node with mode " +
                                                      mappedType.SerializationMode);
            }
        }

        private PropertyValueSource<T> CreatePropertyValueSource<T>(JObject jobj,
            IDeserializerNode node)
        {
            return new PropertyValueSource<T>(jobj, node, this);
        }


        private static bool TryDeserializeAsReference(IDeserializerNode node, Reader reader)
        {
            var jobj = reader.Token as JObject;
            if (jobj == null)
                return false;
            JToken refStringToken;
            if (!jobj.TryGetValue("_ref", out refStringToken) || refStringToken.Type != JTokenType.String)
                return false;

            var refString = (string)((JValue)refStringToken).Value;
            node.Uri = refString;

            try
            {
                node.Value = node.Context.CreateReference(node);
            }
            catch (Exception ex)
            {
                throw new PomonaSerializationException("Failed to deserialize: " + ex.Message, ex);
            }
            return true;
        }


        private static string UnescapePropertyName(string value)
        {
            if (value.StartsWith("^"))
                return value.Substring(1);
            return value;
        }

        private void DeserializeArrayNode(IDeserializerNode node, Reader reader)
        {
            if (TryDeserializeAsReference(node, reader))
                return;

            var elementType = node.ExpectedBaseType.ElementType;

            deserializeArrayNodeGenericMethod(elementType, this, node, reader);
        }


        private void DeserializeArrayNodeGeneric<TElement>(IDeserializerNode node, Reader reader)
        {
            // Return type should be void, but ReflectionHelper.GetMethodDefinition only works with methods with non-void return type.

            if (TryDeserializeAsReference(node, reader))
                return;

            var jarr = reader.Token as JArray;
            if (jarr == null)
                throw new PomonaSerializationException("Expected JSON token of type array, but was " + reader.Token.Type);

            var elementType = node.ExpectedBaseType.ElementType;

            bool isPatching;
            ICollection<TElement> collection;
            if (node.Value == null)
            {
                collection = new List<TElement>();
                isPatching = false;
            }
            else
            {
                collection = (ICollection<TElement>)node.Value;
                isPatching = true;
            }

            if (isPatching && node.Operation == DeserializerNodeOperation.Post)
            {
                // Clear list and add new items
                collection.Clear();
            }

            foreach (var jitem in jarr)
            {
                var jobj = jitem as JObject;
                var itemNode = new ItemValueDeserializerNode(elementType, node.Context, node.ExpandPath, node);
                if (jobj != null)
                {
                    foreach (var jprop in jobj.Properties().Where(IsIdentifierProperty))
                    {
                        // Starts with "-@" or "*@"
                        var identifyPropName = jprop.Name.Substring(2);
                        var identifyProp =
                            itemNode.ValueType.Properties.FirstOrDefault(x => x.JsonName == identifyPropName);
                        if (identifyProp == null)
                        {
                            throw new PomonaSerializationException("Unable to find predicate property " + jprop.Name
                                                                   + " in object");
                        }

                        var identifierNode = new ItemValueDeserializerNode(identifyProp.PropertyType,
                            itemNode.Context,
                            parent : itemNode);
                        DeserializeThroughContext(identifierNode, new Reader(jprop.Value));
                        var identifierValue = identifierNode.Value;

                        if (jprop.Name[0] == '-')
                            itemNode.Operation = DeserializerNodeOperation.Delete;
                        else if (jprop.Name[0] == '*')
                            itemNode.Operation = DeserializerNodeOperation.Patch;
                        else
                            throw new PomonaSerializationException("Unexpected json patch identifier property.");
                        itemNode.Value =
                            collection.Cast<object>().First(x => identifierValue.Equals(identifyProp.GetValue(x, itemNode.Context)));
                    }
                }

                if (itemNode.Operation == DeserializerNodeOperation.Delete)
                {
                    node.CheckItemAccessRights(HttpMethod.Delete);
                    collection.Remove((TElement)itemNode.Value);
                }
                else
                {
                    if (itemNode.Operation == DeserializerNodeOperation.Patch)
                        node.CheckItemAccessRights(HttpMethod.Patch);
                    else if (isPatching)
                        node.CheckAccessRights(HttpMethod.Post);

                    DeserializeThroughContext(itemNode, new Reader(jitem));
                    if (itemNode.Operation != DeserializerNodeOperation.Patch)
                    {
                        if (!(itemNode.ExpectedBaseType is TransformedType)
                            || itemNode.ExpectedBaseType.IsAnonymous()
                            || !collection.Contains((TElement)itemNode.Value))
                        {
                            collection.Add((TElement)itemNode.Value);
                        }
                    }
                }
            }
            
            if (node.Value == null)
                node.Value = collection;
        }


        private void DeserializeComplexNode(IDeserializerNode node, Reader reader)
        {
            if (TryDeserializeAsReference(node, reader))
                return;

            var jobj = reader.Token as JObject;
            if (jobj == null)
            {
                throw new PomonaSerializationException(
                    "Trying to deserialize to complex type, expected a JSON object type but got " + reader.Token.Type);
            }

            SetNodeValueType(node, jobj);

            if (node.Operation == DeserializerNodeOperation.Default)
                node.Operation = node.Value == null ? DeserializerNodeOperation.Post : DeserializerNodeOperation.Patch;

            var propertyValueSource = createPropertyValueSourceMethod(node.ValueType, this, jobj, node);
            propertyValueSource.Deserialize();
        }


        private void DeserializeDictionary(IDeserializerNode node, Reader reader)
        {
            if (TryDeserializeAsReference(node, reader))
                return;
            var dictType = (DictionaryTypeSpec)node.ExpectedBaseType;

            var keyType = dictType.KeyType.Type;

            if (keyType != typeof(string))
            {
                throw new NotImplementedException(
                    "Only supports deserialization to IDictionary<TKey,TValue> where TKey is of type string.");
            }

            var valueType = dictType.ValueType.Type;

            deserializeDictionaryGenericMethod(valueType, this, node, reader, dictType);
        }


        private void DeserializeDictionaryGeneric<TValue>(IDeserializerNode node,
            Reader reader,
            DictionaryTypeSpec dictType)
        {
            IDictionary<string, TValue> dict;

            if (node.Value != null)
                dict = (IDictionary<string, TValue>)node.Value;
            else
                dict = new Dictionary<string, TValue>();

            var jobj = reader.Token as JObject;

            var valueType = dictType.ValueType;

            if (jobj == null)
            {
                throw new PomonaSerializationException(
                    "Expected dictionary property to have a JSON object value, but was " + reader.Token.Type);
            }

            foreach (var jprop in jobj.Properties())
            {
                var jpropName = jprop.Name;
                if (jpropName.Length > 0 && reservedFirstCharacters.Contains(jpropName[0]))
                {
                    if (jpropName[0] == '-')
                    {
                        // Removal operation
                        var unescapedPropertyName = UnescapePropertyName(jpropName.Substring(1));
                        dict.Remove(unescapedPropertyName);
                    }
                    else
                    {
                        throw new PomonaSerializationException(
                            "Unexpected character in json property name. Have propertie names been correctly escaped?");
                    }
                }
                else
                {
                    var unescapedPropertyName = UnescapePropertyName(jpropName);
                    var itemNode = new ItemValueDeserializerNode(valueType,
                        node.Context,
                        node.ExpandPath + "." + unescapedPropertyName);
                    DeserializeThroughContext(itemNode, new Reader(jprop.Value));
                    dict[unescapedPropertyName] = (TValue)itemNode.Value;
                }
            }

            if (node.Value == null)
                node.Value = dict;
        }


        private void DeserializeValueNode(IDeserializerNode node, Reader reader)
        {
            if (reader.Token.Type == JTokenType.Object)
            {
                var jobj = reader.Token as JObject;
                JToken typeNameToken;
                if (!jobj.TryGetValue("_type", out typeNameToken))
                {
                    throw new PomonaSerializationException(
                        "Trying to deserialize boxed value, but lacks _type property.");
                }

                JToken valueToken;
                if (!jobj.TryGetValue("value", out valueToken))
                {
                    throw new PomonaSerializationException(
                        "Trying to deserialize boxed value, but lacks value property.");
                }

                var typeName = typeNameToken.ToString();
                node.SetValueType(typeName);
                node.Value = valueToken.ToObject(node.ValueType.Type, this.jsonSerializer);
            }
            else
                node.Value = reader.Token.ToObject(node.ValueType.Type, this.jsonSerializer);
        }


        private bool IsIdentifierProperty(JProperty jProperty)
        {
            var name = jProperty.Name;
            return (name.StartsWith("-@") || name.StartsWith("*@"));
        }


        private bool SetNodeValueType(IDeserializerNode node, JObject jobj)
        {
            JToken explicitTypeSpec;
            if (jobj.TryGetValue("_type", out explicitTypeSpec))
            {
                if (explicitTypeSpec.Type != JTokenType.String)
                {
                    throw new PomonaSerializationException(
                        "Found _type property on JSON object and expected this to be string, but got " +
                        explicitTypeSpec.Type);
                }

                node.SetValueType(explicitTypeSpec.Value<string>());
                return true;
            }
            return false;
        }

        #region Nested type: IJsonPropertyValueSource

        private interface IJsonPropertyValueSource
        {
            void Deserialize();
        }

        #endregion

        #region Nested type: PropertyValueSource

        private class PropertyValueSource<T> : IConstructorPropertySource<T>, IJsonPropertyValueSource
        {
            private readonly PomonaJsonDeserializer deserializer;
            private readonly IDeserializerNode node;
            private JObject jobj;

            private Dictionary<string, PropertyContainer> propertyDict;


            public PropertyValueSource(JObject jobj, IDeserializerNode node, PomonaJsonDeserializer deserializer)
            {
                this.jobj = jobj;
                this.node = node;
                this.deserializer = deserializer;
                this.propertyDict = jobj.Properties()
                    .Select(x => new PropertyContainer(x))
                    .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);
            }


            public TContext Context<TContext>()
            {
                // This is a bit confusing. node.Context refers to the deserializationContext, ResolveContext
                // actually resolves context of type TContext, which for now is limited to mean NancyContext.
                return node.Context.GetContext<TContext>();
            }


            public void Deserialize()
            {
                SetUri();
                if (this.node.Value == null)
                {
                    if (!(this.node.ValueType is TransformedType))
                        throw new NotSupportedException("Only knows how to deserialize TransformedType.");
                    this.node.Value = ((TransformedType)this.node.ValueType).Create(this);
                }
                DeserializeRemainingProperties();
            }


            public TProperty GetValue<TProperty>(PropertyInfo propertyInfo, Func<TProperty> defaultFactory)
            {
                var type = this.node.ValueType;
                var targetProp = type.TypeResolver.FromProperty(propertyInfo);
                PropertyContainer propContainer;
                if (!this.propertyDict.TryGetValue(targetProp.JsonName, out propContainer))
                {
                    if (defaultFactory == null)
                    {
                        node.Context.OnMissingRequiredPropertyError(node, targetProp);
                        throw new PomonaSerializationException("Missing required property " + targetProp.JsonName +
                                                               " in json.");
                    }
                    return defaultFactory();
                }
                var propNode = new PropertyValueDeserializerNode(this.node, targetProp);
                deserializer.DeserializeThroughContext(propNode, new Reader(propContainer.JProperty.Value));
                propContainer.Fetched = true;
                return (TProperty)propNode.Value;
            }


            public T Optional()
            {
                throw new InvalidOperationException("Unexpected error: Should be replaced by a call to GetValue.");
            }


            public TParentType Parent<TParentType>()
            {
                var resourceNode = (IResourceNode)this.node;
                return
                    (TParentType)
                        resourceNode.Parent.WalkTree(x => x.Parent).Select(x => x.Value).OfType<TParentType>().First();
            }


            public T Requires()
            {
                throw new InvalidOperationException("Unexpected error: Should be replaced by a call to GetValue.");
            }


            private void SetUri()
            {
                PropertyContainer propertyContainer;
                if (this.propertyDict.TryGetValue("_uri", out propertyContainer))
                {
                    var jprop = propertyContainer.JProperty;
                    if (jprop.Value.Type != JTokenType.String)
                        throw new PomonaSerializationException("_uri property is expected to be of JSON type string");
                    this.node.Uri = (string)((JValue)jprop.Value).Value;
                }
            }


            private void DeserializeRemainingProperties()
            {
                foreach (var prop in this.node.ValueType.Properties)
                {
                    PropertyContainer propContainer;
                    if (this.propertyDict.TryGetValue(prop.JsonName, out propContainer) && !propContainer.Fetched)
                    {
                        var propNode = new PropertyValueDeserializerNode(this.node, prop)
                        {
                            Operation = propContainer.Operation
                        };
                        var oldValue = propNode.Value = propNode.Property.GetValue(this.node.Value, propNode.Context);
                        deserializer.DeserializeThroughContext(propNode, new Reader(propContainer.JProperty.Value));
                        var newValue = propNode.Value;
                        if (oldValue != newValue)
                            this.node.SetProperty(prop, newValue);
                    }
                }
            }

            #region Nested type: PropertyContainer

            private class PropertyContainer
            {
                private readonly JProperty jProperty;
                private readonly string name;
                private readonly DeserializerNodeOperation operation;


                public PropertyContainer(JProperty jProperty)
                {
                    this.jProperty = jProperty;
                    if (jProperty.Name.StartsWith("!"))
                    {
                        this.name = jProperty.Name.Substring(1);
                        this.operation = DeserializerNodeOperation.Post;
                    }
                    else
                    {
                        this.name = jProperty.Name;
                        this.operation = DeserializerNodeOperation.Patch;
                    }
                }


                public bool Fetched { get; set; }

                public JProperty JProperty
                {
                    get { return this.jProperty; }
                }

                public string Name
                {
                    get { return this.name; }
                }

                public DeserializerNodeOperation Operation
                {
                    get { return this.operation; }
                }
            }

            #endregion
        }

        #endregion

        #region Nested type: Reader

        internal class Reader
        {
            private readonly JToken token;


            public Reader(JToken token)
            {
                this.token = token;
            }


            public JToken Token
            {
                get { return this.token; }
            }
        }

        #endregion

        public object Deserialize(TextReader textReader, DeserializeOptions options = null)
        {
            options = options ?? new DeserializeOptions();
            var context = contextProvider.GetDeserializationContext(options);
            return Deserialize(textReader,
                options.ExpectedBaseType != null ? context.GetClassMapping(options.ExpectedBaseType) : null,
                context,
                options.Target);
        }
    }
}