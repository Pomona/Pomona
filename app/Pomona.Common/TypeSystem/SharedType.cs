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
using System.Reflection;
using Newtonsoft.Json;

namespace Pomona.Common.TypeSystem
{
    /// <summary>
    /// Represents a type that is shared between server and client.
    /// strings, integers etc.. mapped like this
    /// </summary>
    public class SharedType : IMappedType
    {
        private static Type[] basicWireTypes =
            {
                typeof (int), typeof (double), typeof (float), typeof (string),
                typeof (bool), typeof (decimal), typeof (DateTime), typeof (Uri)
            };

        private readonly Type mappedType;
        private readonly Type mappedTypeInstance;
        private readonly ITypeMapper typeMapper;

        private bool isCollection;
        private bool isDictionary;
        private IList<IPropertyInfo> properties;
        private TypeSerializationMode serializationMode;


        public SharedType(Type mappedTypeInstance, ITypeMapper typeMapper)
        {
            if (mappedTypeInstance == null)
                throw new ArgumentNullException("mappedTypeInstance");
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");

            this.mappedTypeInstance = mappedTypeInstance;
            mappedType = mappedTypeInstance.IsGenericType
                             ? mappedTypeInstance.GetGenericTypeDefinition()
                             : mappedTypeInstance;
            this.typeMapper = typeMapper;

            var dictMetadataToken = typeof (IDictionary<,>).MetadataToken;
            isDictionary = mappedTypeInstance.MetadataToken == dictMetadataToken ||
                           mappedTypeInstance.GetInterfaces().Any(x => x.MetadataToken == dictMetadataToken);

            if (!isDictionary)
            {
                if (mappedType != typeof (string))
                {
                    var collectionMetadataToken = typeof (ICollection<>).MetadataToken;
                    isCollection =
                        mappedTypeInstance.MetadataToken == collectionMetadataToken ||
                        mappedTypeInstance.GetInterfaces().Any(x => x.MetadataToken == collectionMetadataToken);
                }
            }

            if (isDictionary)
                serializationMode = TypeSerializationMode.Dictionary;
            else if (isCollection)
                serializationMode = TypeSerializationMode.Array;
            else
                serializationMode = TypeSerializationMode.Value;

            GenericArguments = new List<IMappedType>();

            InitializeGenericArguments();
        }

        public ITypeMapper TypeMapper
        {
            get { return typeMapper; }
        }

        #region IMappedType Members

        public IMappedType BaseType
        {
            get { return (SharedType) typeMapper.GetClassMapping(mappedType.BaseType); }
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
                if (!isDictionary)
                    throw new InvalidOperationException("Type does not implement IDictionary<,>");

                var dictMetadataToken = typeof (IDictionary<,>).MetadataToken;

                if (mappedTypeInstance.MetadataToken == dictMetadataToken)
                    return this;

                return
                    typeMapper.GetClassMapping(
                        mappedTypeInstance.GetInterfaces().First(x => x.MetadataToken == dictMetadataToken));
            }
        }

        public IMappedType DictionaryValueType
        {
            get { return DictionaryType.GenericArguments[1]; }
        }

        public IMappedType ElementType
        {
            get
            {
                if (!isCollection && !IsNullable)
                    throw new InvalidOperationException("Type is not a collection, so it doesn't have an element type.");

                if (MappedType.IsArray)
                    return typeMapper.GetClassMapping(MappedType.GetElementType());

                if (GenericArguments.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Does not know how to find out what element type this collection is of, it has no generic arguement!");
                }

                return GenericArguments[0];
            }
        }

        public IList<IMappedType> GenericArguments { get; private set; }

        public bool IsAlwaysExpanded
        {
            get { return !isCollection; }
        }

        public bool IsBasicWireType
        {
            get { return basicWireTypes.Contains(mappedType); }
        }

        public bool IsCollection
        {
            get { return isCollection; }
        }

        public bool IsNullable
        {
            get { return Nullable.GetUnderlyingType(mappedTypeInstance) != null; }
        }

        public bool IsGenericType
        {
            get { return mappedType.IsGenericType; }
        }

        public bool IsGenericTypeDefinition
        {
            get { return false; }
        }

        public bool IsValueType
        {
            get { return mappedType.IsValueType; }
        }

        public JsonConverter JsonConverter { get; set; }

        public virtual string Name
        {
            get { return mappedType.Name; }
        }


        public virtual object Create(IDictionary<IPropertyInfo, object> args)
        {
            if (MappedTypeInstance.IsAnonymous())
            {
                var ctor = mappedTypeInstance.GetConstructors().Single();
                var ctorArgs = ctor.GetParameters().Select(x => args.First(y => y.Key.Name == x.Name).Value).ToArray();
                return ctor.Invoke(ctorArgs);
            }
            throw new NotImplementedException();
        }

        #endregion

        public bool HasUri
        {
            get { return false; }
        }

        public bool IsDictionary
        {
            get { return isDictionary; }
        }

        public Type MappedType
        {
            get { return mappedType; }
        }

        public Type MappedTypeInstance
        {
            get { return mappedTypeInstance; }
        }

        public IPropertyInfo PrimaryId
        {
            get { return null; }
        }

        public IList<IPropertyInfo> Properties
        {
            get { return GetPropertiesLazy(); }
        }

        public TypeSerializationMode SerializationMode
        {
            get { return serializationMode; }
            set { serializationMode = value; }
        }

        protected virtual IEnumerable<IPropertyInfo> OnGetProperties()
        {
            return mappedTypeInstance.GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(
                x => new SharedPropertyInfo(x, typeMapper));
        }


        private IList<IPropertyInfo> GetPropertiesLazy()
        {
            if (properties == null)
                properties = OnGetProperties().ToList();
            return properties;
        }


        private void InitializeGenericArguments()
        {
            if (mappedTypeInstance.IsGenericType)
            {
                foreach (var genericTypeArg in mappedTypeInstance.GetGenericArguments())
                {
                    if (genericTypeArg == mappedTypeInstance)
                    {
                        // Special case, self referencing generics
                        GenericArguments.Add(this);
                    }
                    else
                        GenericArguments.Add(typeMapper.GetClassMapping(genericTypeArg));
                }
            }
        }
    }
}