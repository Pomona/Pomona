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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Pomona.Common.Internals;
using Pomona.Common.Serialization.Patch;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Json
{
    // Provides custom rules for serialization

    public class PomonaJsonSerializer : TextSerializerBase<PomonaJsonSerializer.Writer>
    {
        // TODO enable type cache for faster serialization, disabled for now need to think a bit more about this. [KNS]
        public static bool TypeCacheEnabled = false;

        private static readonly ConcurrentDictionary<TypeSpec, PomonaJsonSerializerTypeEntry> typeEntryDict =
            new ConcurrentDictionary<TypeSpec, PomonaJsonSerializerTypeEntry>();

        private static readonly MethodInfo serializeDictionaryGenericMethod =
            ReflectionHelper.GetMethodDefinition<PomonaJsonSerializer>(
                x => x.SerializeDictionaryGeneric<object>(null, null, null));

        private static readonly char[] reservedFirstCharacters = "^-*!".ToCharArray();
        private readonly ISerializationContextProvider contextProvider;
        private readonly ThreadLocal<int> loopDetector = new ThreadLocal<int>();


        public PomonaJsonSerializer(ISerializationContextProvider contextProvider)
        {
            if (contextProvider == null)
                throw new ArgumentNullException("contextProvider");

            this.contextProvider = contextProvider;
        }


        public override void Serialize(TextWriter textWriter, object o, SerializeOptions options)
        {
            if (textWriter == null)
                throw new ArgumentNullException("textWriter");

            options = options ?? new SerializeOptions();
            var serializationContext = this.contextProvider.GetSerializationContext(options);
            var expectedBaseType = options.ExpectedBaseType != null
                ? serializationContext.GetClassMapping(options.ExpectedBaseType)
                : null;

            Serialize(serializationContext, o, textWriter, expectedBaseType);
        }


        protected override Writer CreateWriter(TextWriter textWriter)
        {
            return new Writer(textWriter);
        }


        protected override void SerializeNode(ISerializerNode node, Writer writer)
        {
            try
            {
                if (this.loopDetector.Value++ > 300)
                    throw new InvalidOperationException("Deep recursion detected, trying to avoid stack overflow.");

                SerializeNodeInner(node, writer);
            }
            finally
            {
                this.loopDetector.Value--;
            }
        }


        private string EscapePropertyName(string propName)
        {
            if (propName.Length > 0 && reservedFirstCharacters.Contains(propName[0]))
                return "^" + propName;

            return propName;
        }


        private void SerializeCollection(ISerializerNode node, Writer writer)
        {
            var jsonWriter = writer.JsonWriter;
            if (node.SerializeAsReference)
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("_ref");
                jsonWriter.WriteValue(node.Uri);
                jsonWriter.WriteEndObject();
            }
            else
            {
                jsonWriter.WriteStartArray();
                var baseElementType = node.ExpectedBaseType != null
                    ? node.ExpectedBaseType.ElementType
                    : null;

                var delta = node.Value as ICollectionDelta;
                if (delta != null)
                {
                    foreach (var item in delta.AddedItems.Concat(delta.ModifiedItems))
                    {
                        var itemNode = new ItemValueSerializerNode(item, baseElementType, node.ExpandPath, node.Context, node);
                        SerializeThroughContext(itemNode, writer);
                    }
                    foreach (var item in delta.RemovedItems)
                    {
                        var itemNode = new ItemValueSerializerNode(item, baseElementType, node.ExpandPath, node.Context, node, true);
                        SerializeThroughContext(itemNode, writer);
                    }
                }
                else
                {
                    foreach (var item in (IEnumerable)node.Value)
                    {
                        var itemNode = new ItemValueSerializerNode(item, baseElementType, node.ExpandPath, node.Context, node);
                        SerializeThroughContext(itemNode, writer);
                    }
                }
                jsonWriter.WriteEndArray();
            }
        }


        private void SerializeComplex(ISerializerNode node, Writer writer)
        {
            if (node.Value == null)
                writer.JsonWriter.WriteNull();
            else if (node.SerializeAsReference)
                SerializeReference(node, writer);
            else
                SerializeExpanded(node, writer);
        }


        private void SerializeDictionary(ISerializerNode node, Writer writer)
        {
            var dictType = node.ExpectedBaseType as DictionaryTypeSpec;
            if (dictType == null)
            {
                throw new PomonaSerializationException("Unable to serialize dictionary of typespec "
                                                       + (node.ExpectedBaseType != null
                                                           ? node.ExpectedBaseType.GetType().ToString()
                                                           : " (unknown)"));
            }

            var keyMappedType = dictType.KeyType.Type;

            if (keyMappedType != typeof(string))
            {
                throw new NotImplementedException(
                    "Does not support serialization of dictionaries where key is not string.");
            }

            var valueMappedType = dictType.ValueType.Type;
            serializeDictionaryGenericMethod
                .MakeGenericMethod(valueMappedType)
                .Invoke(this, new object[] { node, writer, dictType });
        }


        private void SerializeDictionaryGeneric<TValue>(ISerializerNode node,
                                                        Writer writer,
                                                        DictionaryTypeSpec dictType)
        {
            var jsonWriter = writer.JsonWriter;
            var dict = (IDictionary<string, TValue>)node.Value;
            var expectedValueType = dictType.ValueType;

            jsonWriter.WriteStartObject();
            var dictDelta = dict as IDictionaryDelta<string, TValue>;
            var serializedKeyValuePairs = dictDelta != null ? dictDelta.ModifiedItems : dict;

            if (dictDelta != null && dictDelta.RemovedKeys.Any())
            {
                foreach (var removed in dictDelta.RemovedKeys)
                {
                    jsonWriter.WritePropertyName("-" + EscapePropertyName(removed));
                    jsonWriter.WriteStartObject();
                    jsonWriter.WriteEndObject();
                }
            }

            foreach (var kvp in serializedKeyValuePairs)
            {
                // TODO: Support other key types than string
                jsonWriter.WritePropertyName(EscapePropertyName(kvp.Key));
                var itemNode = new ItemValueSerializerNode(kvp.Value, expectedValueType, node.ExpandPath, node.Context, node);
                SerializeThroughContext(itemNode, writer);
            }

            jsonWriter.WriteEndObject();
        }


        private void SerializeExpanded(ISerializerNode node, Writer writer)
        {
            var jsonWriter = writer.JsonWriter;
            var serializingDelta = node.Value is IDelta;

            jsonWriter.WriteStartObject();
            if (node.ValueType is ResourceType && node.Uri != null)
            {
                jsonWriter.WritePropertyName("_uri");
                jsonWriter.WriteValue(node.Uri);
            }
            if (node.ExpectedBaseType != node.ValueType && !node.ValueType.IsAnonymous() && !serializingDelta && !node.IsRemoved)
                SerializeTypeProperty(jsonWriter, node.ValueType);

            if (node.IsRemoved || (serializingDelta && node.ParentNode != null && node.ParentNode.ValueType.IsCollection))
            {
                var primaryId = node.ValueType.Maybe().OfType<StructuredType>().Select(x => x.PrimaryId).OrDefault();
                if (primaryId == null)
                    throw new PomonaSerializationException("When we are removing complex object a primary id is required.");

                jsonWriter.WritePropertyName((node.IsRemoved ? "-@" : "*@") + primaryId.JsonName);
                jsonWriter.WriteValue(primaryId.GetValue(node.Value, node.Context));
                if (node.IsRemoved)
                {
                    jsonWriter.WriteEndObject();
                    return;
                }
            }

            PomonaJsonSerializerTypeEntry cacheTypeEntry;

            IEnumerable<PropertySpec> propertiesToSerialize = null;
            var pomonaSerializable = node.Value as IPomonaSerializable;
            if (pomonaSerializable == null && TryGetTypeEntry(node.ValueType, out cacheTypeEntry))
            {
                cacheTypeEntry.WritePropertiesFunc(jsonWriter, node.Value, node.Context);
                propertiesToSerialize = cacheTypeEntry.ManuallyWrittenProperties;
            }

            propertiesToSerialize = propertiesToSerialize ?? node.ValueType.Properties;

            propertiesToSerialize = propertiesToSerialize.Where(x => x.IsSerialized);

            if (pomonaSerializable != null)
                propertiesToSerialize = propertiesToSerialize.Where(x => pomonaSerializable.PropertyIsSerialized(x.Name));

            foreach (var prop in propertiesToSerialize)
            {
                var propNode = new PropertyValueSerializerNode(node, prop);
                if (serializingDelta && propNode.ValueType.SerializationMode == TypeSerializationMode.Structured &&
                    !(propNode.Value is IDelta))
                    jsonWriter.WritePropertyName("!" + prop.JsonName);
                else if (propNode.ValueType.SerializationMode == TypeSerializationMode.Array
                         && propNode.Value.Maybe().OfType<ICollectionDelta>().Select(x => x.Cleared).OrDefault())
                    jsonWriter.WritePropertyName("!" + prop.JsonName);
                else
                    jsonWriter.WritePropertyName(prop.JsonName);
                SerializeThroughContext(propNode, writer);
            }
            jsonWriter.WriteEndObject();
        }


        private void SerializeNodeInner(ISerializerNode node, Writer writer)
        {
            if (node.Value == null)
            {
                writer.JsonWriter.WriteNull();
                return;
            }

            var mappedType = node.ExpectedBaseType ?? node.ValueType;
            if (mappedType == typeof(object))
                mappedType = node.ValueType;

            switch (mappedType.SerializationMode)
            {
                case TypeSerializationMode.Dictionary:
                    SerializeDictionary(node, writer);
                    break;
                case TypeSerializationMode.Structured:
                    SerializeComplex(node, writer);
                    break;
                case TypeSerializationMode.Array:
                    SerializeCollection(node, writer);
                    break;
                case TypeSerializationMode.Value:
                    SerializeValue(node, writer);
                    break;
            }
        }


        private static void SerializeReference(ISerializerNode node, Writer writer)
        {
            var jsonWriter = writer.JsonWriter;

            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("_ref");
            jsonWriter.WriteValue(node.Uri);
            var valueType = node.ValueType;
            if (node.ExpectedBaseType != valueType)
                SerializeTypeProperty(jsonWriter, valueType);

            jsonWriter.WriteEndObject();
        }


        private static void SerializeTypeProperty(JsonWriter jsonWriter, TypeSpec valueType)
        {
            jsonWriter.WritePropertyName("_type");
            if (valueType is QueryResultType)
                jsonWriter.WriteValue("__result__");
            else
                jsonWriter.WriteValue(valueType.NameWithGenericArguments);
        }


        private static void SerializeValue(ISerializerNode node, Writer writer)
        {
            var value = node.Value;

            var boxValueWithTypeSpec = node.ExpectedBaseType != null &&
                                       node.ExpectedBaseType.Type == typeof(object) &&
                                       ValueBoxingRequired(node.Value.GetType());

            if (boxValueWithTypeSpec)
            {
                writer.JsonWriter.WriteStartObject();
                SerializeTypeProperty(writer.JsonWriter, node.ValueType);
                writer.JsonWriter.WritePropertyName("value");
            }

            var jsonConverter = node.ValueType.GetCustomJsonConverter();
            if (jsonConverter == null && node.ValueType is EnumTypeSpec)
                jsonConverter = new StringEnumConverter();

            if (jsonConverter != null)
                jsonConverter.WriteJson(writer.JsonWriter, value, null);
            else
                writer.JsonWriter.WriteValue(value);

            if (boxValueWithTypeSpec)
                writer.JsonWriter.WriteEndObject();
        }


        private static bool TryGetTypeEntry(TypeSpec mappedType, out PomonaJsonSerializerTypeEntry typeEntry)
        {
            typeEntry = null;
            if (TypeCacheEnabled)
            {
                typeEntry = typeEntryDict.GetOrAdd(mappedType, mt => new PomonaJsonSerializerTypeEntry(mt));
                return true;
            }
            return false;
        }


        private static bool ValueBoxingRequired(Type type)
        {
            // string and bool does not require boxing of value;
            return !(type == typeof(string) || type == typeof(bool));
        }


        public class Writer : ISerializerWriter, IDisposable
        {
            private readonly JsonTextWriter jsonWriter;


            public Writer(TextWriter textWriter)
            {
                if (textWriter == null)
                    throw new ArgumentNullException("textWriter");

                this.jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented
                };
            }


            public JsonWriter JsonWriter
            {
                get { return this.jsonWriter; }
            }

            #region Implementation of IDisposable

            public void Dispose()
            {
                // NOTE: Not sure if this is correct
                this.jsonWriter.Flush();
            }

            #endregion
        }
    }
}