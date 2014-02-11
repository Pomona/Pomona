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
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml;

using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Xml
{
    public class PomonaXmlSerializer : TextSerializerBase<PomonaXmlSerializer.Writer>
    {
        private readonly ISerializationContextProvider contextProvider;


        public PomonaXmlSerializer(ISerializationContextProvider contextProvider)
        {
            if (contextProvider == null)
                throw new ArgumentNullException("contextProvider");
            this.contextProvider = contextProvider;
        }


        public override void Serialize(TextWriter textWriter, object o, SerializeOptions options = null)
        {
            if (textWriter == null)
                throw new ArgumentNullException("textWriter");
            options = options ?? new SerializeOptions();
            var serializationContext = this.contextProvider.GetSerializationContext(options);
            Serialize(serializationContext,
                o,
                textWriter,
                options.ExpectedBaseType != null ? serializationContext.GetClassMapping(options.ExpectedBaseType) : null);
        }


        protected override Writer CreateWriter(TextWriter textWriter)
        {
            return new Writer(new XmlTextWriter(textWriter));
        }


        protected override void SerializeNode(ISerializerNode node, Writer writer)
        {
            var mappedType = node.ValueType ?? node.ExpectedBaseType;
            switch (mappedType.SerializationMode)
            {
                case TypeSerializationMode.Complex:
                    SerializeComplex(node, writer);
                    break;
                case TypeSerializationMode.Value:
                    writer.XmlWriter.WriteStartElement(writer.NextElementName);
                    if (node.Value != null)
                    {
                        var value = node.Value;

                        if (value is Guid)
                            value = value.ToString();

                        writer.XmlWriter.WriteValue(value);
                    }
                    writer.XmlWriter.WriteEndElement();
                    break;
                case TypeSerializationMode.Array:
                    SerializeCollection(node, writer);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }


        protected override void SerializeQueryResult(QueryResult queryResult,
            ISerializationContext fetchContext,
            Writer writer,
            TypeSpec elementType)
        {
            var itemNode = new ItemValueSerializerNode(queryResult,
                fetchContext.GetClassMapping(queryResult.ListType),
                string.Empty,
                fetchContext,
                null);
            SerializeThroughContext(itemNode, writer);
        }


        private static string GetXmlName(string name)
        {
            return NameUtils.ConvertCamelCaseToUri(name);
        }


        private static string GetXmlName(PropertySpec property)
        {
            return GetXmlName(property.Name);
        }


        private static string GetXmlName(TypeSpec type)
        {
            return GetXmlName(type.Name);
        }


        private static void SerializeReference(ISerializerNode node, Writer writer)
        {
            writer.XmlWriter.WriteAttributeString("ref", node.Uri);
        }


        private void SerializeCollection(ISerializerNode node, Writer writer)
        {
            var elementType = node.ExpectedBaseType.ElementType;
            var outerArrayElementName = writer.NextElementName ?? GetXmlName(((TransformedType)elementType).PluralName);

            writer.XmlWriter.WriteStartElement(outerArrayElementName);

            writer.NextElementName = GetXmlName(elementType);

            var xmlWriter = writer.XmlWriter;
            if (node.SerializeAsReference)
            {
                xmlWriter.WriteAttributeString("ref", node.Uri);
            }
            else
            {
                foreach (var item in (IEnumerable)node.Value)
                {
                    var itemNode = new ItemValueSerializerNode(item, elementType, node.ExpandPath, node.Context, node);
                    SerializeThroughContext(itemNode, writer);
                }
            }

            writer.XmlWriter.WriteEndElement();
        }


        private void SerializeComplex(ISerializerNode node, Writer writer)
        {
            var elementName = writer.NextElementName ?? GetXmlName(node.ExpectedBaseType);
            writer.XmlWriter.WriteStartElement(elementName);

            if (node.Value != null)
            {
                if (node.SerializeAsReference)
                    SerializeReference(node, writer);
                else
                    SerializeExpanded(node, writer);
            }

            writer.XmlWriter.WriteEndElement();
        }


        private void SerializeExpanded(ISerializerNode node, Writer writer)
        {
            var jsonWriter = writer.XmlWriter;

            if (node.ValueType is ResourceType && node.Uri != null)
                jsonWriter.WriteAttributeString("uri", node.Uri);
            if (node.ExpectedBaseType != node.ValueType)
                jsonWriter.WriteAttributeString("type", node.ValueType.Name);

            var propertiesToSerialize = node.ValueType.Properties.Where(x => node.Context.PropertyIsSerialized(x));

            var pomonaSerializable = node.Value as IPomonaSerializable;
            if (pomonaSerializable != null)
                propertiesToSerialize = propertiesToSerialize.Where(x => pomonaSerializable.PropertyIsSerialized(x.Name));

            foreach (var prop in propertiesToSerialize)
            {
                writer.NextElementName = GetXmlName(prop);
                var propNode = new PropertyValueSerializerNode(node, prop);
                SerializeThroughContext(propNode, writer);
            }
        }

        #region Nested type: Writer

        public class Writer : ISerializerWriter
        {
            private readonly XmlWriter xmlWriter;


            public Writer(XmlWriter xmlWriter)
            {
                this.xmlWriter = xmlWriter;
            }


            public XmlWriter XmlWriter
            {
                get { return this.xmlWriter; }
            }

            internal string NextElementName { get; set; }
        }

        #endregion
    }
}