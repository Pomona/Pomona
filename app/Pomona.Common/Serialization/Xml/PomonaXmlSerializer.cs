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
using System.Xml;
using Pomona.Common.TypeSystem;

namespace Pomona.Common.Serialization.Xml
{
    internal class PomonaXmlSerializer : ISerializer<PomonaXmlSerializer.Writer>
    {
        ISerializerWriter ISerializer.CreateWriter(TextWriter textWriter)
        {
            return CreateWriter(textWriter);
        }


        public Writer CreateWriter(TextWriter textWriter)
        {
            return new Writer(new XmlTextWriter(textWriter));
        }

        public void SerializeNode(ISerializerNode node, ISerializerWriter writer)
        {
            SerializeNode(node, CastWriter(writer));
        }


        public void SerializeNode(ISerializerNode node, Writer writer)
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


        public void SerializeQueryResult(QueryResult queryResult, ISerializationContext fetchContext, Writer writer,
                                         IMappedType elementType)
        {
            var itemNode = new ItemValueSerializerNode(queryResult, fetchContext.GetClassMapping(queryResult.ListType),
                                                       string.Empty, fetchContext);
            itemNode.Serialize(this, writer);
        }

        public void SerializeQueryResult(QueryResult queryResult, ISerializationContext fetchContext,
                                         ISerializerWriter writer, IMappedType elementType)
        {
            SerializeQueryResult(queryResult, fetchContext, CastWriter(writer), elementType);
        }

        private void SerializeCollection(ISerializerNode node, Writer writer)
        {
            var elementType = node.ExpectedBaseType.ElementType;
            var outerArrayElementName = writer.NextElementName ?? GetXmlName(elementType.PluralName);

            writer.XmlWriter.WriteStartElement(outerArrayElementName);

            writer.NextElementName = GetXmlName(elementType);

            var xmlWriter = writer.XmlWriter;
            if (node.SerializeAsReference)
            {
                xmlWriter.WriteAttributeString("ref", node.Uri);
            }
            else
            {
                foreach (var item in (IEnumerable) node.Value)
                {
                    var itemNode = new ItemValueSerializerNode(item, elementType, node.ExpandPath, node.Context);
                    itemNode.Serialize(this, writer);
                }
            }

            writer.XmlWriter.WriteEndElement();
        }

        private static string GetXmlName(string name)
        {
            return NameUtils.ConvertCamelCaseToUri(name);
        }

        private static string GetXmlName(IPropertyInfo property)
        {
            return GetXmlName(property.Name);
        }

        private static string GetXmlName(IMappedType type)
        {
            return GetXmlName(type.Name);
        }

        private static void SerializeReference(ISerializerNode node, Writer writer)
        {
            writer.XmlWriter.WriteAttributeString("ref", node.Uri);
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

            if (node.ValueType.HasUri)
            {
                jsonWriter.WriteAttributeString("uri", node.Uri);
            }
            if (node.ExpectedBaseType != node.ValueType)
            {
                jsonWriter.WriteAttributeString("type", node.ValueType.Name);
            }

            IEnumerable<IPropertyInfo> propertiesToSerialize = node.ValueType.Properties;

            var pomonaSerializable = node.Value as IPomonaSerializable;
            if (pomonaSerializable != null)
            {
                propertiesToSerialize = propertiesToSerialize.Where(x => pomonaSerializable.PropertyIsSerialized(x.Name));
            }

            foreach (var prop in propertiesToSerialize)
            {
                writer.NextElementName = GetXmlName(prop);
                var propNode = new PropertyValueSerializerNode(node, prop);
                propNode.Serialize(this, writer);
            }
        }

        private Writer CastWriter(ISerializerWriter writer)
        {
            var castedWriter = writer as Writer;
            if (castedWriter == null)
                throw new ArgumentException("Writer required to be of type PomonaJsonSerializationWriter", "writer");
            return castedWriter;
        }

        public class Writer : ISerializerWriter
        {
            private readonly XmlWriter xmlWriter;

            public Writer(XmlWriter xmlWriter)
            {
                this.xmlWriter = xmlWriter;
            }

            public XmlWriter XmlWriter
            {
                get { return xmlWriter; }
            }

            internal string NextElementName { get; set; }
        }
    }
}