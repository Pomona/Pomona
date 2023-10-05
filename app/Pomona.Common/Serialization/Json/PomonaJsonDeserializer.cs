#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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
        private readonly ISerializationContextProvider contextProvider;
        private readonly JsonSerializer jsonSerializer;


        public PomonaJsonDeserializer(ISerializationContextProvider contextProvider)
        {
            if (contextProvider == null)
                throw new ArgumentNullException(nameof(contextProvider));
            this.contextProvider = contextProvider;
            this.jsonSerializer = new JsonSerializer() { DateParseHandling = DateParseHandling.None };
            this.jsonSerializer.Converters.Add(new StringEnumConverter());
        }


        private Reader CreateReader(TextReader textReader)
        {
            return
                new Reader(JToken.ReadFrom(new JsonTextReader(textReader) { DateParseHandling = DateParseHandling.None }));
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


        private void DeserializeArrayNode(IDeserializerNode node, Reader reader)
        {
            if (TryDeserializeAsReference(node, reader))
                return;

            TypeSpec elementType;
            if (node.ExpectedBaseType != null && node.ExpectedBaseType.IsCollection)
                elementType = node.ExpectedBaseType.ElementType;
            else
                elementType = node.Context.GetClassMapping(typeof(object));

            deserializeArrayNodeGenericMethod(elementType, this, node, reader);
        }


        private void DeserializeArrayNodeGeneric<TElement>(IDeserializerNode node, Reader reader)
        {
            if (TryDeserializeAsReference(node, reader))
                return;

            var jarr = reader.Token as JArray;
            if (jarr == null)
                throw new PomonaSerializationException("Expected JSON token of type array, but was " + reader.Token.Type);

            var expectedBaseType = node.ExpectedBaseType;

            // Deserialize as object array by default
            bool asArray;
            TypeSpec elementType;
            if (expectedBaseType != null && expectedBaseType.IsCollection)
            {
                elementType = expectedBaseType.ElementType;
                asArray = expectedBaseType.IsArray;
            }
            else
            {
                elementType = node.Context.GetClassMapping(typeof(object));
                asArray = true;
            }

            bool isPatching;
            ICollection<TElement> collection;
            if (node.Value == null)
            {
                if (expectedBaseType != null && expectedBaseType == typeof(ISet<TElement>))
                    collection = new HashSet<TElement>();
                else
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
                node.CheckItemAccessRights(HttpMethod.Delete);
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
                            collection.Cast<object>().First(
                                x => identifierValue.Equals(identifyProp.GetValue(x, itemNode.Context)));
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
                        if (!(itemNode.ExpectedBaseType is StructuredType)
                            || itemNode.ExpectedBaseType.IsAnonymous()
                            || !collection.Contains((TElement)itemNode.Value))
                            collection.Add((TElement)itemNode.Value);
                    }
                }
            }

            if (node.Value == null)
                node.Value = asArray ? collection.ToArray() : collection;
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

            var propertyValueSource = new PropertyValueSource(jobj, node, this);
            propertyValueSource.Deserialize();
        }


        private void DeserializeDictionary(IDeserializerNode node, Reader reader)
        {
            if (TryDeserializeAsReference(node, reader))
                return;
            var dictType = node.ExpectedBaseType as DictionaryTypeSpec;
            if (dictType == null)
                dictType = (DictionaryTypeSpec)node.Context.GetClassMapping(typeof(Dictionary<string, object>));

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


        private void DeserializeNode(IDeserializerNode node, Reader reader)
        {
            if (reader.Token.Type == JTokenType.Null)
            {
                if (node.ExpectedBaseType != null && node.ExpectedBaseType.Type.IsValueType && !node.ExpectedBaseType.IsNullable)
                {
                    throw new PomonaSerializationException("Deserialized to null, which is not allowed value for casting to type "
                                                           + node.ValueType.FullName);
                }
                node.Value = null;
                return;
            }

            TypeSpec mappedType;
            if (node.ExpectedBaseType != null && node.ExpectedBaseType != typeof(object))
            {
                if (SetNodeValueType(node, reader.Token))
                    mappedType = node.ValueType;
                else
                    mappedType = node.ExpectedBaseType;
            }
            else if (reader.Token.Type == JTokenType.String)
            {
                node.SetValueType(node.Context.GetClassMapping(typeof(string)));
                mappedType = node.ValueType;
            }
            else
            {
                if (!SetNodeValueType(node, reader.Token))
                {
                    throw new PomonaSerializationException(
                        "No expected type to deserialize to provided, and unable to get type from incoming JSON.");
                }
                mappedType = node.ValueType;
            }

            switch (mappedType.SerializationMode)
            {
                case TypeSerializationMode.Structured:
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


        private void DeserializeThroughContext(IDeserializerNode node, Reader reader)
        {
            node.Context.Deserialize(node, n => DeserializeNode(n, reader));
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
            {
                var converter = node.ValueType.GetCustomJsonConverter();
                if (converter == null)
                    node.Value = reader.Token.ToObject(node.ValueType.Type, this.jsonSerializer);
                else
                {
                    node.Value = converter.ReadJson(new JTokenReader(reader.Token),
                                                    node.ValueType.Type,
                                                    null,
                                                    this.jsonSerializer);
                }
            }
        }


        private bool IsIdentifierProperty(JProperty jProperty)
        {
            var name = jProperty.Name;
            return (name.StartsWith("-@") || name.StartsWith("*@"));
        }


        private static bool SetNodeValueType(IDeserializerNode node, JToken jtoken)
        {
            var jobj = jtoken as JObject;
            if (jobj != null)
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

                    var typeName = explicitTypeSpec.Value<string>();
                    if (typeName == "__result__")
                    {
                        if (!(node.ExpectedBaseType is QueryResultType) && node.ExpectedBaseType is EnumerableTypeSpec)
                        {
                            Type queryResultGenericTypeDefinition;
                            if (node.ExpectedBaseType.Type.IsGenericInstanceOf(typeof(ISet<>)))
                                queryResultGenericTypeDefinition = typeof(QuerySetResult<>);
                            else
                                queryResultGenericTypeDefinition = typeof(QueryResult<>);
                            var queryResultTypeInstance = queryResultGenericTypeDefinition.MakeGenericType(node.ExpectedBaseType.ElementType);
                            if (node.ExpectedBaseType.Type.IsAssignableFrom(queryResultTypeInstance))
                                node.SetValueType(queryResultTypeInstance);
                        }
                    }
                    else
                        node.SetValueType(typeName);
                    return true;
                }
            }

            if (node.ExpectedBaseType == null || node.ExpectedBaseType == typeof(object))
            {
                switch (jtoken.Type)
                {
                    case JTokenType.String:
                        node.SetValueType(typeof(string));
                        return true;
                    case JTokenType.Boolean:
                        node.SetValueType(typeof(bool));
                        return true;
                    case JTokenType.Array:
                        node.SetValueType(typeof(object[]));
                        return true;
                    case JTokenType.Object:
                        node.SetValueType(typeof(Dictionary<string, object>));
                        return true;
                    default:
                        return false;
                }
            }

            return false;
        }


        private static bool TryDeserializeAsReference(IDeserializerNode node, Reader reader)
        {
            var jobj = reader.Token as JObject;
            if (jobj == null)
                return false;

            SetNodeValueType(node, jobj);

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


        public object Deserialize(TextReader textReader, DeserializeOptions options = null)
        {
            try
            {
                options = options ?? new DeserializeOptions();

                var returnTypeSpecified = options.ExpectedBaseType != null;

                if (returnTypeSpecified && typeof(JToken).IsAssignableFrom(options.ExpectedBaseType))
                {
                    // When asking for a JToken, just return the deserialized json object
                    using (var jr = new JsonTextReader(textReader))
                    {
                        return JToken.Load(jr);
                    }
                }

                var context = this.contextProvider.GetDeserializationContext(options);
                var expectedBaseType = returnTypeSpecified
                    ? context.GetClassMapping(options.ExpectedBaseType)
                    : null;
                return Deserialize(textReader, expectedBaseType, context, options.Target);
            }
            catch (JsonReaderException exception)
            {
                throw new PomonaSerializationException(exception.Message, exception);
            }
            catch (JsonSerializationException exception)
            {
                throw new PomonaSerializationException(exception.Message, exception);
            }
        }

        #region Nested type: IJsonPropertyValueSource

        private interface IJsonPropertyValueSource : IConstructorPropertySource
        {
            void Deserialize();
        }

        #endregion

        #region Nested type: PropertyValueSource

        private class PropertyValueSource : IJsonPropertyValueSource
        {
            private readonly PomonaJsonDeserializer deserializer;
            private readonly IDeserializerNode node;
            private readonly Dictionary<string, PropertyContainer> propertyDict;


            public PropertyValueSource(JObject jobj, IDeserializerNode node, PomonaJsonDeserializer deserializer)
            {
                this.node = node;
                this.deserializer = deserializer;
                this.propertyDict = jobj.Properties()
                                        .Select(x => new PropertyContainer(x))
                                        .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);
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
                        this.deserializer.DeserializeThroughContext(propNode, new Reader(propContainer.JProperty.Value));
                        var newValue = propNode.Value;
                        if (oldValue != newValue)
                            this.node.SetProperty(prop, newValue);
                        propContainer.Fetched = true;
                    }
                }
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


            public TContext Context<TContext>()
            {
                // This is a bit confusing. node.Context refers to the deserializationContext, ResolveContext
                // actually resolves context of type TContext, which for now is limited to mean NancyContext.
                return this.node.Context.GetInstance<TContext>();
            }


            public void Deserialize()
            {
                SetUri();
                if (this.node.Value == null || this.node.Operation == DeserializerNodeOperation.Post)
                    this.node.Value = this.node.Context.CreateResource(this.node.ValueType, this);
                DeserializeRemainingProperties();
            }


            public TProperty GetValue<TProperty>(PropertyInfo propertyInfo, Func<TProperty> defaultFactory)
            {
                var type = this.node.ValueType;
                var targetProp = type.TypeResolver.FromProperty(type, propertyInfo);
                PropertyContainer propContainer;
                if (!this.propertyDict.TryGetValue(targetProp.JsonName, out propContainer))
                {
                    if (defaultFactory == null)
                    {
                        this.node.Context.OnMissingRequiredPropertyError(this.node, targetProp);
                        throw new PomonaSerializationException("Missing required property " + targetProp.JsonName +
                                                               " in json.");
                    }
                    return defaultFactory();
                }
                var propNode = new PropertyValueDeserializerNode(this.node, targetProp);
                this.deserializer.DeserializeThroughContext(propNode, new Reader(propContainer.JProperty.Value));
                propContainer.Fetched = true;
                return (TProperty)propNode.Value;
            }


            public TParentType Parent<TParentType>()
            {
                var resourceNode = (IResourceNode)this.node;
                return
                    (TParentType)
                        resourceNode.Parent.WalkTree(x => x.Parent).Where(
                            x => x.ResultType == null || x.ResultType.Type.IsAssignableFrom(typeof(TParentType))).Select
                            (x => x.Value).OfType<TParentType>().First();
            }

            #region Nested type: PropertyContainer

            private class PropertyContainer
            {
                public PropertyContainer(JProperty jProperty)
                {
                    JProperty = jProperty;
                    if (jProperty.Name.StartsWith("!"))
                    {
                        Name = jProperty.Name.Substring(1);
                        Operation = DeserializerNodeOperation.Post;
                    }
                    else
                    {
                        Name = jProperty.Name;
                        Operation = DeserializerNodeOperation.Patch;
                    }
                }


                public bool Fetched { get; set; }

                public JProperty JProperty { get; }

                public string Name { get; }

                public DeserializerNodeOperation Operation { get; }
            }

            #endregion
        }

        #endregion

        #region Nested type: Reader

        internal class Reader
        {
            public Reader(JToken token)
            {
                Token = token;
            }


            public JToken Token { get; }
        }

        #endregion
    }
}
