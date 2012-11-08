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

using Common.Logging;

namespace Pomona
{
    public class TypeMapper
    {
        private readonly ITypeMappingFilter filter;
        private readonly Dictionary<Type, IMappedType> mappings = new Dictionary<Type, IMappedType>();
        private readonly HashSet<Type> sourceTypes;
        private ILog log = LogManager.GetLogger(typeof(TypeMapper));


        public TypeMapper(ITypeMappingFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");
            this.filter = filter;
            this.sourceTypes = new HashSet<Type>(filter.GetSourceTypes().Where(filter.TypeIsMapped));

            foreach (var sourceType in this.sourceTypes)
            {
                GetClassMapping(sourceType);
                MapForeignKeys();
            }
        }


        public ITypeMappingFilter Filter
        {
            get { return this.filter; }
        }

        public ICollection<Type> SourceTypes
        {
            get { return this.sourceTypes; }
        }

        public IEnumerable<TransformedType> TransformedTypes
        {
            get { return this.mappings.Values.OfType<TransformedType>(); }
        }


        public string ConvertToInternalPropertyPath(TransformedType rootType, string externalPath)
        {
            if (rootType == null)
                throw new ArgumentNullException("rootType");
            return rootType.ConvertToInternalPropertyPath(externalPath);
        }


        public IMappedType GetClassMapping<T>()
        {
            var type = typeof(T);

            return GetClassMapping(type);
        }


        public IMappedType GetClassMapping(Type type)
        {
            type = this.filter.ResolveRealTypeForProxy(type);

            IMappedType mappedType;
            if (!this.mappings.TryGetValue(type, out mappedType))
                mappedType = CreateClassMapping(type);

            return mappedType;
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
            if (!this.filter.TypeIsMapped(type))
                throw new InvalidOperationException("Type " + type.FullName + " is excluded from mapping.");

            if (type.IsEnum)
                return new EnumType(type, this);

            if (this.filter.TypeIsMappedAsSharedType(type))
            {
                SharedType newSharedType;
                if (type.IsGenericType)
                {
                    newSharedType = new SharedType(type.GetGenericTypeDefinition(), this);
                    foreach (var genericTypeArg in type.GetGenericArguments())
                    {
                        if (genericTypeArg == type)
                        {
                            // Special case, self referencing generics
                            newSharedType.GenericArguments.Add(newSharedType);
                        }
                        else
                            newSharedType.GenericArguments.Add(GetClassMapping(genericTypeArg));
                    }
                }
                else
                    newSharedType = new SharedType(type, this);

                newSharedType.JsonConverter = this.filter.GetJsonConverterForType(type);
                newSharedType.CustomClientLibraryType = this.filter.GetClientLibraryType(type);

                this.mappings[type] = newSharedType;
                return newSharedType;
            }

            if (this.filter.TypeIsMappedAsTransformedType(type))
            {
                var classDefinition = new TransformedType(type, type.Name, this);

                // Add to cache before filling out, in case of self-references
                this.mappings[type] = classDefinition;

                if (this.filter.TypeIsMappedAsValueObject(type))
                    classDefinition.MappedAsValueObject = true;

                var uriBaseType = this.filter.GetUriBaseType(type);
                if (uriBaseType != type)
                    classDefinition.UriBaseType = (TransformedType)GetClassMapping(uriBaseType);
                else
                    classDefinition.UriBaseType = classDefinition;

                classDefinition.UriRelativePath = NameUtils.ConvertCamelCaseToUri(
                    classDefinition.UriBaseType.PluralName);

                classDefinition.ScanProperties(type);

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

            foreach (var prop in collectionProperties.Where(x => x.PropertyInfo != null))
            {
                var foreignKeyProp = this.filter.GetOneToManyCollectionForeignKey(prop.PropertyInfo);

                if (foreignKeyProp != null)
                {
                    prop.ElementForeignKey =
                        TransformedTypes
                            .Where(x => x.SourceType == foreignKeyProp.DeclaringType)
                            .SelectMany(x => x.Properties)
                            .FirstOrDefault(x => x.PropertyInfo.Name == foreignKeyProp.Name);
                }
            }
        }
    }
}