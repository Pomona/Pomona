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
using System.Linq;
using Common.Logging;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.Serialization;
using Pomona.Common.TypeSystem;
using Pomona.FluentMapping;
using Pomona.Handlers;

namespace Pomona
{
    public class TypeMapper : ITypeMapper
    {
        private readonly PomonaConfigurationBase configuration;
        private readonly ITypeMappingFilter filter;
        private readonly Dictionary<Type, IMappedType> mappings = new Dictionary<Type, IMappedType>();
        private readonly ISerializerFactory serializerFactory;
        private readonly HashSet<Type> sourceTypes;
        private readonly Dictionary<string, IMappedType> typeNameMap;
        private ILog log = LogManager.GetLogger(typeof (TypeMapper));

        public TypeMapper(PomonaConfigurationBase configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");
            this.configuration = configuration;

            filter = configuration.TypeMappingFilter;
            var fluentRuleObjects = configuration.FluentRuleObjects.ToArray();
            if (fluentRuleObjects.Length > 0)
                filter = new FluentTypeMappingFilter(filter, fluentRuleObjects);

            if (filter == null)
                throw new ArgumentNullException("filter");

            sourceTypes = new HashSet<Type>(filter.GetSourceTypes().Where(filter.TypeIsMapped));

            typeNameMap = new Dictionary<string, IMappedType>();

            foreach (var sourceType in sourceTypes.Concat(TypeUtils.GetNativeTypes()))
            {
                var type = GetClassMapping(sourceType);
                typeNameMap[type.Name.ToLower()] = type;
            }
            MapForeignKeys();

            serializerFactory = configuration.SerializerFactory;

            ScanHandlerForPomonaMethods();
            configuration.OnMappingComplete(this);
        }


        public IEnumerable<EnumType> EnumTypes
        {
            get { return mappings.Values.OfType<EnumType>(); }
        }

        public ITypeMappingFilter Filter
        {
            get { return filter; }
        }

        /// <summary>
        /// The Json serializer factory.
        /// TODO: This should be moved out of here..
        /// </summary>
        public ISerializerFactory SerializerFactory
        {
            get { return serializerFactory; }
        }

        public ICollection<Type> SourceTypes
        {
            get { return sourceTypes; }
        }

        public IEnumerable<TransformedType> TransformedTypes
        {
            get { return mappings.Values.OfType<TransformedType>(); }
        }


        public IMappedType GetClassMapping(Type type)
        {
            type = filter.ResolveRealTypeForProxy(type);

            IMappedType mappedType;
            if (!mappings.TryGetValue(type, out mappedType))
                mappedType = CreateClassMapping(type);

            return mappedType;
        }


        public IMappedType GetClassMapping(string typeName)
        {
            IMappedType type;
            if (!typeNameMap.TryGetValue(typeName.ToLower(), out type))
            {
                throw new UnknownTypeException("Type with name " + typeName + " not recognized.");
            }
            return type;
        }

        private void ScanHandlerForPomonaMethods()
        {
            var scanner = new PomonaMethodScanner(this);
            foreach (var handlerClass in configuration.HandlerTypes)
            {
                scanner.ScanPostToResourceHandlers(handlerClass);
            }
        }

        public string ConvertToInternalPropertyPath(TransformedType rootType, string externalPath)
        {
            if (rootType == null)
                throw new ArgumentNullException("rootType");
            return rootType.ConvertToInternalPropertyPath(externalPath);
        }


        public IMappedType GetClassMapping<T>()
        {
            var type = typeof (T);

            return GetClassMapping(type);
        }


        public bool IsSerializedAsArray(IMappedType mappedType)
        {
            if (mappedType == null)
                throw new ArgumentNullException("mappedType");
            return mappedType.IsCollection;
        }


        public bool IsSerializedAsArray(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return IsSerializedAsArray(GetClassMapping(type));
        }


        public bool IsSerializedAsDictionary(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return IsSerializedAsDictionary(GetClassMapping(type));
        }


        public bool IsSerializedAsDictionary(IMappedType mappedType)
        {
            //mappedType.IsGenericType && mappedType.
            throw new NotImplementedException("NOCOMMIT!");
        }


        public bool IsSerializedAsObject(IMappedType mappedType)
        {
            if (mappedType == null)
                throw new ArgumentNullException("mappedType");
            return mappedType is TransformedType;
        }


        public bool IsSerializedAsObject(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            return IsSerializedAsObject(GetClassMapping(type));
        }


        private IMappedType CreateClassMapping(Type type)
        {
            if (!filter.TypeIsMapped(type))
                throw new InvalidOperationException("Type " + type.FullName + " is excluded from mapping.");

            if (type.IsEnum)
            {
                var values = Enum.GetValues(type).Cast<object>().ToDictionary(x => x.ToString(), x => (int)x);
                var newEnumType = new EnumType(type, this, values);
                mappings[type] = newEnumType;
                return newEnumType;
            }

            if (filter.TypeIsMappedAsSharedType(type))
            {
                var newSharedType = new SharedType(type, this);

                newSharedType.JsonConverter = filter.GetJsonConverterForType(type);
                newSharedType.CustomClientLibraryType = filter.GetClientLibraryType(type);

                mappings[type] = newSharedType;
                return newSharedType;
            }

            if (filter.TypeIsMappedAsTransformedType(type))
            {
                var classDefinition = new TransformedType(type, type.Name, this);

                // Add to cache before filling out, in case of self-references
                mappings[type] = classDefinition;

                if (filter.TypeIsMappedAsValueObject(type))
                    classDefinition.MappedAsValueObject = true;

                var uriBaseType = filter.GetUriBaseType(type);
                if (uriBaseType == null)
                {
                    classDefinition.UriBaseType = null;
                }
                else
                {
                    if (uriBaseType != type)
                        classDefinition.UriBaseType = (TransformedType)GetClassMapping(uriBaseType);
                    else
                        classDefinition.UriBaseType = classDefinition;

                    classDefinition.UriRelativePath =
                        NameUtils.ConvertCamelCaseToUri(classDefinition.UriBaseType.PluralName);
                }


                classDefinition.PostReturnType = (TransformedType)GetClassMapping(filter.GetPostReturnType(type));
                classDefinition.IsExposedAsRepository = filter.TypeIsExposedAsRepository(type);

                classDefinition.PostAllowed = filter.PostOfTypeIsAllowed(type);
                classDefinition.PatchAllowed = filter.PatchOfTypeIsAllowed(type);

                classDefinition.OnDeserialized = filter.GetOnDeserializedHook(type);

                classDefinition.ScanProperties(type);

                if (filter.IsIndependentTypeRoot(type))
                    classDefinition.BaseType = null;

                return classDefinition;
            }

            throw new InvalidOperationException("Don't know how to map " + type.FullName);
        }


        private void MapForeignKeys()
        {
            // This method maps all properties representing foreign keys for one-to-many collections
            // TODO: Make this configurable through filter

            var collectionProperties =
                TransformedTypes
                    .SelectMany(x => x.Properties)
                    .Where(x => x.PropertyType.IsCollection);

            foreach (var prop in collectionProperties.OfType<PropertyMapping>().Where(x => x.PropertyInfo != null))
            {
                var foreignKeyProp = filter.GetOneToManyCollectionForeignKey(prop.PropertyInfo);

                if (foreignKeyProp != null)
                {
                    prop.ElementForeignKey =
                        TransformedTypes
                            .Where(x => x.MappedType == foreignKeyProp.DeclaringType)
                            .SelectMany(x => x.Properties.OfType<PropertyMapping>())
                            .FirstOrDefault(x => x.PropertyInfo.Name == foreignKeyProp.Name);
                }
            }
        }
    }
}