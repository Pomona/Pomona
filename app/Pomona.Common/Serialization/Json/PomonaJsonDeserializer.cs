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

namespace Pomona.Common.Serialization.Json
{
    public class PomonaJsonDeserializer : IDeserializer<PomonaJsonDeserializer.Reader>
    {
        private static readonly MethodInfo deserializeDictionaryGenericMethod;
        private readonly JsonSerializer jsonSerializer;


        static PomonaJsonDeserializer()
        {
            deserializeDictionaryGenericMethod =
                ReflectionHelper.GetGenericMethodDefinition<PomonaJsonDeserializer>(
                    x => x.DeserializeDictionaryGeneric<object, object>(null, null));
        }


        public PomonaJsonDeserializer()
        {
            jsonSerializer = new JsonSerializer();
            jsonSerializer.Converters.Add(new StringEnumConverter());
        }


        public Reader CreateReader(TextReader textReader)
        {
            return new Reader(JToken.ReadFrom(new JsonTextReader(textReader)));
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

            IMappedType mappedType;
            if (node.ExpectedBaseType != null)
                mappedType = node.ExpectedBaseType;
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


        public void DeserializeNode(IDeserializerNode node, ISerializerReader reader)
        {
            DeserializeNode(node, (Reader) reader);
        }


        public void DeserializeQueryResult(QueryResult queryResult, ISerializationContext fetchContext, Reader writer)
        {
            throw new NotImplementedException();
        }


        public void DeserializeQueryResult(QueryResult queryResult,
                                           ISerializationContext fetchContext,
                                           ISerializerReader reader)
        {
            throw new NotImplementedException();
        }


        ISerializerReader IDeserializer.CreateReader(TextReader textReader)
        {
            return CreateReader(textReader);
        }

        private static bool TryDeserializeAsReference(IDeserializerNode node, Reader reader)
        {
            var jobj = reader.Token as JObject;
            if (jobj == null)
                return false;
            JToken refStringToken;
            if (!jobj.TryGetValue("_ref", out refStringToken) || refStringToken.Type != JTokenType.String)
                return false;

            var refString = (string) ((JValue) refStringToken).Value;
            node.Uri = refString;
            node.Value = node.Context.CreateReference(node.ExpectedBaseType, refString);
            return true;
        }


        private void DeserializeArrayNode(IDeserializerNode node, Reader reader)
        {
            if (TryDeserializeAsReference(node, reader))
                return;

            var jarr = reader.Token as JArray;
            if (jarr == null)
                throw new PomonaSerializationException("Expected JSON token of type array, but was " + reader.Token.Type);

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


        private void DeserializeDictionary(IDeserializerNode node, Reader reader)
        {
            if (TryDeserializeAsReference(node, reader))
                return;

            var keyType = node.ExpectedBaseType.DictionaryKeyType;
            var valueType = node.ExpectedBaseType.DictionaryValueType;

            deserializeDictionaryGenericMethod
                .MakeGenericMethod(keyType.MappedTypeInstance, valueType.MappedTypeInstance)
                .Invoke(this, new object[] {node, reader});
        }


        private object DeserializeDictionaryGeneric<TKey, TValue>(IDeserializerNode node, Reader reader)
        {
            var dict = new Dictionary<TKey, TValue>();
            var jobj = reader.Token as JObject;

            var valueType = node.ExpectedBaseType.DictionaryValueType;

            if (jobj == null)
            {
                throw new PomonaSerializationException(
                    "Expected dictionary property to have a JSON object value, but was " + reader.Token.Type);
            }

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


        private void DeserializeValueNode(IDeserializerNode node, Reader reader)
        {
            if (reader.Token.Type == JTokenType.Object)
            {
                var jobj = reader.Token as JObject;
                JToken typeNameToken;
                if (!jobj.TryGetValue("_type", out typeNameToken))
                    throw new PomonaSerializationException(
                        "Trying to deserialize boxed value, but lacks _type property.");

                JToken valueToken;
                if (!jobj.TryGetValue("value", out valueToken))
                {
                    throw new PomonaSerializationException(
                        "Trying to deserialize boxed value, but lacks value property.");
                }

                var typeName = typeNameToken.ToString();
                node.SetValueType(typeName);
                node.Value = valueToken.ToObject(node.ValueType.MappedTypeInstance, jsonSerializer);
            }
            else
            {
                node.Value = reader.Token.ToObject(node.ValueType.MappedTypeInstance, jsonSerializer);
            }
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

        #region Nested type: Reader

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

        #endregion
    }
}