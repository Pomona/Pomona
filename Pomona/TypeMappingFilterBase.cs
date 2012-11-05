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

using Common.Logging;

using Newtonsoft.Json;

namespace Pomona
{
    public abstract class TypeMappingFilterBase : ITypeMappingFilter
    {
        private static readonly HashSet<Type> jsonSupportedNativeTypes;
        private ILog log = LogManager.GetLogger(typeof(TypeMappingFilterBase));
        private HashSet<Type> sourceTypesCached;


        static TypeMappingFilterBase()
        {
            jsonSupportedNativeTypes = new HashSet<Type>()
            {
                typeof(string),
                typeof(int),
                typeof(long),
                typeof(double),
                typeof(float),
                typeof(decimal),
                typeof(DateTime),
                typeof(object),
                typeof(bool),
                typeof(Guid),
                typeof(Uri)
            };
        }


        private HashSet<Type> SourceTypes
        {
            get
            {
                if (this.sourceTypesCached == null)
                    this.sourceTypesCached = new HashSet<Type>(GetSourceTypes());
                return this.sourceTypesCached;
            }
        }

        #region ITypeMappingFilter Members

        public abstract object GetIdFor(object entity);

        public abstract IEnumerable<Type> GetSourceTypes();


        public virtual string GetClientLibraryFilename()
        {
            return "Client";
        }


        public virtual Type GetClientLibraryType(Type type)
        {
            return null;
        }


        public virtual JsonConverter GetJsonConverterForType(Type type)
        {
            return null;
        }


        public virtual Func<object, object> GetPropertyGetter(PropertyInfo propertyInfo)
        {
            return x => propertyInfo.GetValue(x, null);
        }


        public virtual string GetPropertyMappedName(PropertyInfo propertyInfo)
        {
            return propertyInfo.Name;
        }


        public virtual Action<object, object> GetPropertySetter(PropertyInfo propertyInfo)
        {
            return (x, value) => propertyInfo.SetValue(x, value, null);
        }


        public virtual Type GetPropertyType(PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType;
        }


        public virtual Type GetUriBaseType(Type type)
        {
            return type;
        }


        public virtual bool PropertyIsIncluded(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetGetMethod(true).IsPublic;
        }


        public virtual Type ResolveRealTypeForProxy(Type type)
        {
            // TODO: Implement some crude heuristics to check whether a type is a proxy type,
            //       that should be treated as its base type.
            //
            //       Maybe look for whether type is public (will proxys normally be internal?), or
            //       that it resides in different assembly than base type.
            //
            //       Or maybe just see whether type is mapped.
            //

            // Lets just try this for now:
            if (this.sourceTypesCached.Contains(type))
                return type;

            if (type.BaseType != null && this.sourceTypesCached.Contains(type.BaseType))
                return type.BaseType;

            return type;
        }


        public virtual bool TypeIsMapped(Type type)
        {
            return TypeIsMappedAsTransformedType(type) || TypeIsMappedAsSharedType(type) ||
                   IsNativelySupportedType(type)
                   || TypeIsMappedAsCollection(type);
        }


        public virtual bool TypeIsMappedAsCollection(Type type)
        {
            return
                type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));
        }


        public virtual bool TypeIsMappedAsSharedType(Type type)
        {
            return type.IsEnum || IsNativelySupportedType(type) || TypeIsMappedAsCollection(type);
        }


        public virtual bool TypeIsMappedAsTransformedType(Type type)
        {
            return SourceTypes.Contains(type);
        }


        public virtual bool TypeIsMappedAsValueObject(Type type)
        {
            return false;
        }


        public PropertyInfo GetOneToManyCollectionForeignKey(PropertyInfo collectionProperty)
        {
            Type[] genericArguments;
            if (
                !TypeUtils.TryGetTypeArguments(
                    collectionProperty.PropertyType, typeof(IEnumerable<>), out genericArguments))
                return null;

            var elementType = genericArguments[0];

            var foreignPropCandicates =
                elementType.GetProperties().Where(x => x.PropertyType == collectionProperty.DeclaringType).ToList();
            if (foreignPropCandicates.Count > 1)
            {
                this.log.Warn(
                    "Not mapping foreign key relation of one-to-many collection property " + collectionProperty.Name
                    + " of type "
                    + collectionProperty.DeclaringType.FullName + " since there are multiple candidates on other side: "
                    + string.Join(", ", foreignPropCandicates.Select(x => x.Name)) + " (of " + elementType.FullName);
            }

            return foreignPropCandicates.Count == 1 ? foreignPropCandicates[0] : null;
        }

        #endregion

        private static bool IsNativelySupportedType(Type type)
        {
            return jsonSupportedNativeTypes.Contains(type) || IsNullableAllowedNativeType(type);
        }


        private static bool IsNullableAllowedNativeType(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)) &&
                   jsonSupportedNativeTypes.Contains(type.GetGenericArguments()[0]);
        }


        // TODO: Replace this with a way to find out what property has the Id.
    }
}