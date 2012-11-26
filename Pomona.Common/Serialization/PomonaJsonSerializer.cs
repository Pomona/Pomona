#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization
{
    public class PomonaJsonSerializer : ISerializer<PomonaJsonSerializerState>
    {
        #region Implementation of ISerializer<PomonaJsonSerializerState>

        public void SerializeNode(ISerializerNode node, PomonaJsonSerializerState state)
        {
            if (node.Value == null)
            {
                state.Writer.WriteNull();
                return;
            }
            switch (node.ExpectedBaseType.SerializationMode)
            {
                case TypeSerializationMode.Dictionary:
                    SerializeDictionary(node, state);
                    break;
                case TypeSerializationMode.Complex:
                    SerializeComplex(node, state);
                    break;
                case TypeSerializationMode.Array:
                    SerializeCollection(node, state);
                    break;
                case TypeSerializationMode.Value:
                    var jsonConverter = node.ValueType.JsonConverter;
                    if (jsonConverter != null)
                        jsonConverter.WriteJson(state.Writer, node.Value, null);
                    else
                        state.Writer.WriteValue(node.Value);
                    break;
            }
        }


        public void SerializeQueryResult(
            QueryResult queryResult, ISerializationContext fetchContext, PomonaJsonSerializerState state)
        {
            var jsonWriter = state.Writer;
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("_type");
            jsonWriter.WriteValue("__result__");

            jsonWriter.WritePropertyName("totalCount");
            jsonWriter.WriteValue(queryResult.TotalCount);

            jsonWriter.WritePropertyName("count");
            jsonWriter.WriteValue(queryResult.Count);

            Uri previousPageUri;
            if (queryResult.TryGetPage(-1, out previousPageUri))
            {
                jsonWriter.WritePropertyName("previous");
                jsonWriter.WriteValue(previousPageUri.ToString());
            }

            Uri nextPageUri;
            if (queryResult.TryGetPage(1, out nextPageUri))
            {
                jsonWriter.WritePropertyName("next");
                jsonWriter.WriteValue(nextPageUri.ToString());
            }

            jsonWriter.WritePropertyName("items");
            SerializeNode(
                new ItemValueSerializerNode(
                    queryResult,
                    fetchContext.GetClassMapping(queryResult.ListType),
                    string.Empty,
                    fetchContext),
                state);

            jsonWriter.WriteEndObject();
            jsonWriter.Flush();
        }


        private static void SerializeReference(ISerializerNode node, PomonaJsonSerializerState state)
        {
            var writer = state.Writer;

            writer.WriteStartObject();
            writer.WritePropertyName("_ref");
            writer.WriteValue(node.Uri);
            if (node.ExpectedBaseType != node.ValueType)
            {
                writer.WritePropertyName("_type");
                writer.WriteValue(node.ValueType.Name);
            }

            writer.WriteEndObject();
        }


        private void SerializeCollection(ISerializerNode node, PomonaJsonSerializerState state)
        {
            var writer = state.Writer;
            if (node.SerializeAsReference)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("_ref");
                writer.WriteValue(node.Uri);
                writer.WriteEndObject();
            }
            else
            {
                writer.WriteStartArray();
                var elementType = node.ExpectedBaseType.CollectionElementType;
                foreach (var item in (IEnumerable)node.Value)
                {
                    SerializeNode(
                        new ItemValueSerializerNode(item, elementType, node.ExpandPath, node.FetchContext), state);
                }
                writer.WriteEndArray();
            }
        }


        private void SerializeComplex(ISerializerNode node, PomonaJsonSerializerState state)
        {
            if (node.Value == null)
                state.Writer.WriteNull();
            else if (node.SerializeAsReference)
                SerializeReference(node, state);
            else
                SerializeExpanded(node, state);
        }


        private void SerializeDictionary(ISerializerNode node, PomonaJsonSerializerState state)
        {
            var keyMappedType = node.ExpectedBaseType.DictionaryKeyType.MappedTypeInstance;
            var valueMappedType = node.ExpectedBaseType.DictionaryValueType.MappedTypeInstance;
            typeof(PomonaJsonSerializer)
                .GetMethod("SerializeDictionaryGeneric", BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(keyMappedType, valueMappedType)
                .Invoke(this, new object[] { node, state });
        }


        private void SerializeDictionaryGeneric<TKey, TValue>(
            ISerializerNode node, PomonaJsonSerializerState state)
        {
            var writer = state.Writer;
            var dict = (IDictionary<TKey, TValue>)node.Value;
            var expectedValueType = node.ExpectedBaseType.DictionaryValueType;

            writer.WriteStartObject();
            foreach (var kvp in dict)
            {
                // TODO: Support other key types than string
                writer.WritePropertyName((string)((object)kvp.Key));
                SerializeNode(
                    new ItemValueSerializerNode(kvp.Value, expectedValueType, node.ExpandPath, node.FetchContext),
                    state);
            }
            writer.WriteEndObject();
        }


        private void SerializeExpanded(ISerializerNode node, PomonaJsonSerializerState state)
        {
            var writer = state.Writer;

            writer.WriteStartObject();
            if (node.ValueType.HasUri)
            {
                writer.WritePropertyName("_uri");
                writer.WriteValue(node.Uri);
            }
            if (node.ExpectedBaseType != node.ValueType)
            {
                writer.WritePropertyName("_type");
                writer.WriteValue(node.ValueType.Name);
            }

            foreach (var prop in node.ValueType.Properties)
            {
                writer.WritePropertyName(prop.JsonName);
                node.SerializeProperty(this, state, prop);
            }
            writer.WriteEndObject();
        }

        #endregion
    }
}