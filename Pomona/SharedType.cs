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
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Pomona.Common.TypeSystem;

namespace Pomona
{
    /// <summary>
    /// Represents a type that is shared between server and client.
    /// strings, integers etc.. mapped like this
    /// </summary>
    public class SharedType : IMappedType
    {
        private static Type[] basicWireTypes =
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(bool), typeof(decimal), typeof(DateTime), typeof(Uri)
            };

        private readonly Type mappedType;
        private readonly Type mappedTypeInstance;
        private readonly TypeSerializationMode serializationMode;
        private readonly TypeMapper typeMapper;

        private bool isCollection;
        private bool isDictionary;


        public SharedType(Type mappedType, Type mappedTypeInstance, TypeMapper typeMapper)
        {
            if (mappedType == null)
                throw new ArgumentNullException("targetType");
            if (mappedTypeInstance == null)
                throw new ArgumentNullException("mappedTypeInstance");
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.mappedType = mappedType;
            this.mappedTypeInstance = mappedTypeInstance;
            this.typeMapper = typeMapper;

            var dictMetadataToken = typeof(IDictionary<,>).MetadataToken;
            this.isDictionary = mappedTypeInstance.MetadataToken == dictMetadataToken ||
                                mappedTypeInstance.GetInterfaces().Any(x => x.MetadataToken == dictMetadataToken);

            if (!this.isDictionary)
            {
                if (mappedType != typeof(string))
                {
                    var collectionMetadataToken = typeof(ICollection<>).MetadataToken;
                    this.isCollection =
                        mappedTypeInstance.MetadataToken == collectionMetadataToken ||
                        mappedTypeInstance.GetInterfaces().Any(x => x.MetadataToken == collectionMetadataToken);
                }
            }

            if (this.isDictionary)
                this.serializationMode = TypeSerializationMode.Dictionary;
            else if (this.isCollection)
                this.serializationMode = TypeSerializationMode.Array;
            else
                this.serializationMode = TypeSerializationMode.Value;

            GenericArguments = new List<IMappedType>();
        }


        public bool HasUri
        {
            get { return false; }
        }

        public bool IsDictionary
        {
            get { return this.isDictionary; }
        }

        public Type MappedType
        {
            get { return this.mappedType; }
        }

        public Type MappedTypeInstance
        {
            get { return this.mappedTypeInstance; }
        }

        public IList<IPropertyInfo> Properties
        {
            get { throw new NotImplementedException(); }
        }

        public TypeSerializationMode SerializationMode
        {
            get { return this.serializationMode; }
        }

        #region IMappedType Members

        public IMappedType BaseType
        {
            get { return (SharedType)this.typeMapper.GetClassMapping(this.mappedType.BaseType); }
        }

        public IMappedType CollectionElementType
        {
            get
            {
                if (!this.isCollection)
                    throw new InvalidOperationException("Type is not a collection, so it doesn't have an element type.");

                if (MappedType.IsArray)
                    return this.typeMapper.GetClassMapping(MappedType.GetElementType());

                if (GenericArguments.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Does not know how to find out what element type this collection is of, it has no generic arguement!");
                }

                return GenericArguments[0];
            }
        }

        public Type CustomClientLibraryType { get; set; }

        public IMappedType DictionaryKeyType
        {
            get { return DictionaryType.GenericArguments[0]; }
        }

        public IMappedType DictionaryType
        {
            get
            {
                if (!this.isDictionary)
                    throw new InvalidOperationException("Type does not implement IDictionary<,>");

                var dictMetadataToken = typeof(IDictionary<,>).MetadataToken;

                if (this.mappedTypeInstance.MetadataToken == dictMetadataToken)
                    return this;

                return
                    this.typeMapper.GetClassMapping(
                        this.mappedTypeInstance.GetInterfaces().First(x => x.MetadataToken == dictMetadataToken));
            }
        }

        public IMappedType DictionaryValueType
        {
            get { return DictionaryType.GenericArguments[1]; }
        }

        public IList<IMappedType> GenericArguments { get; private set; }

        public bool IsAlwaysExpanded
        {
            get { return !this.isCollection; }
        }

        public bool IsBasicWireType
        {
            get { return basicWireTypes.Contains(this.mappedType); }
        }

        public bool IsCollection
        {
            get { return this.isCollection; }
        }

        public bool IsGenericType
        {
            get { return this.mappedType.IsGenericType; }
        }

        public bool IsGenericTypeDefinition
        {
            get { return false; }
        }

        public bool IsValueType
        {
            get { return this.mappedType.IsValueType; }
        }

        public JsonConverter JsonConverter { get; set; }

        public string Name
        {
            get { return this.mappedType.Name; }
        }


        public virtual void WriteJson(
            JsonWriter jsonWriter,
            object value,
            IMappedType expectedType,
            string parentPath,
            string propName,
            FetchContext context)
        {
            if (JsonConverter != null)
                JsonConverter.WriteJson(jsonWriter, value, null);
            else
                jsonWriter.WriteValue(value);
        }

        #endregion
    }
}