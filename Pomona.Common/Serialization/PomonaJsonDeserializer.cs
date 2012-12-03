using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Pomona.Common.TypeSystem;
using Pomona.Internals;

namespace Pomona.Common.Serialization
{
    public class PomonaJsonDeserializer : IDeserializer<PomonaJsonDeserializer.Reader>
    {
        private static readonly MethodInfo deserializeDictionaryGenericMethod;
        private readonly JsonSerializer jsonSerializer;

        public PomonaJsonDeserializer()
        {
            jsonSerializer = new JsonSerializer();
            jsonSerializer.Converters.Add(new StringEnumConverter());
        }

        public class Reader : ISerializerReader
        {
            private readonly JToken token;


            public Reader(JToken token)
            {
                this.token = token;
            }


            public JToken Token
            {
                get { return token; }
            }
        }

        #region Implementation of IDeserializer

        static PomonaJsonDeserializer()
        {
            deserializeDictionaryGenericMethod =
                ReflectionHelper.GetGenericMethodDefinition<PomonaJsonDeserializer>(
                    x => x.DeserializeDictionaryGeneric<object, object>(null, null));
        }

        ISerializerReader IDeserializer.CreateReader(TextReader textReader)
        {
            return CreateReader(textReader);
        }

        public object Deserialize(TextReader textReader, IMappedType expectedBaseType, IDeserializationContext context)
        {
            var node = new ItemValueDeserializerNode(expectedBaseType, context);
            var reader = CreateReader(textReader);
            node.Deserialize(this, reader);
            return node.Value;
        }

        public void DeserializeNode(IDeserializerNode node, Reader reader)
        {
            if (reader.Token.Type == JTokenType.Null)
            {
                node.Value = null;
                return;
            }

            switch (node.ExpectedBaseType.SerializationMode)
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
                                                      node.ExpectedBaseType.SerializationMode);
            }
        }

        public void DeserializeQueryResult(QueryResult queryResult, ISerializationContext fetchContext, Reader writer)
        {
            throw new NotImplementedException();
        }


        public Reader CreateReader(TextReader textReader)
        {
            return new Reader(JToken.ReadFrom(new JsonTextReader(textReader)));
        }


        public void DeserializeNode(IDeserializerNode node, ISerializerReader reader)
        {
            DeserializeNode(node, (Reader) reader);
        }


        public void DeserializeQueryResult(QueryResult queryResult, ISerializationContext fetchContext,
                                           ISerializerReader reader)
        {
            throw new NotImplementedException();
        }

        private object DeserializeDictionaryGeneric<TKey, TValue>(IDeserializerNode node, Reader reader)
        {
            var dict = new Dictionary<TKey, TValue>();
            var jobj = reader.Token as JObject;

            var valueType = node.ExpectedBaseType.DictionaryValueType;

            if (jobj == null)
                throw new PomonaSerializationException(
                    "Expected dictionary property to have a JSON object value, but was " + reader.Token.Type);

            foreach (var jprop in jobj.Properties())
            {
                var itemNode = new ItemValueDeserializerNode(valueType, node.Context);
                itemNode.Deserialize(this, new Reader(jprop.Value));
                object key = jprop.Name;
                dict.Add((TKey) key, (TValue) itemNode.Value);
            }

            node.Value = dict;
            return null;
        }

        private void DeserializeDictionary(IDeserializerNode node, Reader reader)
        {
            if (TryDeserializeAsReference(node, reader))
                return;

            var keyType = node.ExpectedBaseType.DictionaryKeyType;
            var valueType = node.ExpectedBaseType.DictionaryValueType;

            deserializeDictionaryGenericMethod.MakeGenericMethod(
                keyType.MappedTypeInstance, valueType.MappedTypeInstance).Invoke(this, new object[] {node, reader});
        }


        private void DeserializeArrayNode(IDeserializerNode node, Reader reader)
        {
            if (TryDeserializeAsReference(node, reader))
                return;

            var jarr = reader.Token as JArray;
            if (jarr == null)
            {
                throw new PomonaSerializationException("Expected JSON token of type array, but was " + reader.Token.Type);
            }

            var elementType = node.ExpectedBaseType.ElementType;
            var instance =
                (IList) Activator.CreateInstance(typeof (List<>).MakeGenericType(elementType.MappedTypeInstance));
            foreach (var jitem in jarr)
            {
                var itemNode = new ItemValueDeserializerNode(elementType, node.Context);
                itemNode.Deserialize(this, new Reader(jitem));
                instance.Add(itemNode.Value);
            }

            node.Value = instance;
        }


        private static bool TryDeserializeAsReference(IDeserializerNode node, Reader reader)
        {
            var jobj = reader.Token as JObject;
            if (jobj == null)
            {
                return false;
            }
            JToken refStringToken;
            if (!jobj.TryGetValue("_ref", out refStringToken) || refStringToken.Type != JTokenType.String)
            {
                return false;
            }

            var refString = (string) ((JValue) refStringToken).Value;
            node.Uri = refString;
            node.Value = node.Context.CreateReference(node.ExpectedBaseType, refString);
            return true;
        }


        private void DeserializeValueNode(IDeserializerNode node, Reader reader)
        {
            node.Value = reader.Token.ToObject(node.ExpectedBaseType.MappedTypeInstance, jsonSerializer);
        }


        private void DeserializeComplexNode(IDeserializerNode node, Reader reader)
        {
            if (TryDeserializeAsReference(node, reader))
                return;

            var jobj = reader.Token as JObject;
            if (jobj == null)
                throw new PomonaSerializationException(
                    "Trying to deserialize to complex type, expected a JSON object type but got " + reader.Token.Type);

            JToken explicitTypeSpec;
            if (jobj.TryGetValue("_type", out explicitTypeSpec))
            {
                if (explicitTypeSpec.Type != JTokenType.String)
                    throw new PomonaSerializationException(
                        "Found _type property on JSON object and expected this to be string, but got " +
                        explicitTypeSpec.Type);

                node.SetValueType(explicitTypeSpec.Value<string>());
            }

            IDictionary<IPropertyInfo, object> ctorArgs = new Dictionary<IPropertyInfo, object>();

            foreach (var jprop in jobj.Properties())
            {
                if (jprop.Name == "_type")
                    continue;
                if (jprop.Name == "_uri")
                {
                    if (jprop.Value.Type != JTokenType.String)
                        throw new PomonaSerializationException("_uri property is expected to be of JSON type string");
                    node.Uri = (string) ((JValue) jprop.Value).Value;
                    continue;
                }
                var name = jprop.Name;
                var prop = node.ValueType.Properties.First(x => x.JsonName == name);
                var propNode = new PropertyValueDeserializerNode(node, prop);

                propNode.Deserialize(this, new Reader(jprop.Value));

                ctorArgs[prop] = propNode.Value;
            }

            node.Value = node.ValueType.Create(ctorArgs);
        }

        #endregion
    }
}