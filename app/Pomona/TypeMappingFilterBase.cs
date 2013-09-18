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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Common.Logging;
using DelegateDecompiler;
using Nancy.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.FluentMapping;

namespace Pomona
{
    public abstract class TypeMappingFilterBase : ITypeMappingFilter
    {
        private static readonly HashSet<Type> jsonSupportedNativeTypes;
        private readonly ILog log = LogManager.GetLogger(typeof (TypeMappingFilterBase));
        private HashSet<Type> sourceTypesCached;


        static TypeMappingFilterBase()
        {
            jsonSupportedNativeTypes = new HashSet<Type>(TypeUtils.GetNativeTypes());
        }

        private HashSet<Type> SourceTypes
        {
            get
            {
                if (sourceTypesCached == null)
                    sourceTypesCached = new HashSet<Type>(GetSourceTypes());
                return sourceTypesCached;
            }
        }

        #region ITypeMappingFilter Members

        public virtual bool IsIndependentTypeRoot(Type type)
        {
            return false;
        }

        public abstract object GetIdFor(object entity);

        public abstract IEnumerable<Type> GetSourceTypes();


        public virtual bool ClientPropertyIsExposedAsRepository(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            return false;
        }


        public virtual string GetClientAssemblyName()
        {
            return "Client";
        }


        public virtual Type GetClientLibraryType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return null;
        }


        public virtual JsonConverter GetJsonConverterForType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (IsNullableType(type) && type.GetGenericArguments()[0].IsEnum)
                return new StringEnumConverter();
            return null;
        }


        public virtual Type GetPostReturnType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return type;
        }


        public virtual Func<object, object> GetPropertyGetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            var selfParam = Expression.Parameter(typeof (object), "x");
            var expr = Expression.Lambda<Func<object, object>>(
                Expression.Convert(
                    Expression.MakeMemberAccess(
                        Expression.Convert(selfParam, propertyInfo.DeclaringType),
                        propertyInfo
                        ),
                    typeof (object)
                    ),
                selfParam
                );

            return expr.Compile();
        }


        public virtual string GetPropertyMappedName(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            return propertyInfo.Name;
        }


        public virtual Action<object, object> GetPropertySetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            if (!propertyInfo.CanWrite)
            {
                return (obj, value) =>
                    {
                        throw new InvalidOperationException(
                            "Property " + propertyInfo.Name + " of " + propertyInfo.DeclaringType + " is not writable.");
                    };
            }

            var selfParam = Expression.Parameter(typeof (object), "x");
            var valueParam = Expression.Parameter(typeof (object), "value");
            var expr = Expression.Lambda<Action<object, object>>(
                Expression.Assign(
                    Expression.Property(
                        Expression.Convert(selfParam, propertyInfo.DeclaringType),
                        propertyInfo
                        ),
                    Expression.Convert(valueParam, propertyInfo.PropertyType)
                    ),
                selfParam,
                valueParam);

            return expr.Compile();
        }


        public virtual Type GetPropertyType(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            return propertyInfo.PropertyType;
        }


        public virtual ConstructorInfo GetTypeConstructor(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            // Find longest (most specific) public constructor
            return type.GetConstructors().OrderByDescending(x => x.GetParameters().Length).FirstOrDefault();
        }


        public virtual Type GetUriBaseType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (TypeIsMappedAsValueObject(type))
                return null;

            return type;
        }


        public virtual bool PropertyIsAlwaysExpanded(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            return propertyInfo.DeclaringType.IsAnonymous();
        }


        public virtual bool PropertyIsIncluded(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            return propertyInfo.GetGetMethod(true).IsPublic;
        }


        public virtual Type ResolveRealTypeForProxy(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            // TODO: Implement some crude heuristics to check whether a type is a proxy type,
            //       that should be treated as its base type.
            //
            //       Maybe look for whether type is public (will proxys normally be internal?), or
            //       that it resides in different assembly than base type.
            //
            //       Or maybe just see whether type is mapped.
            //

            // Lets just try this for now:
            if (SourceTypes.Contains(type))
                return type;

            if (type.BaseType != null && SourceTypes.Contains(type.BaseType))
                return type.BaseType;

            return type;
        }


        public virtual bool TypeIsMapped(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return TypeIsMappedAsTransformedType(type) || TypeIsMappedAsSharedType(type) ||
                   IsNativelySupportedType(type)
                   || TypeIsMappedAsCollection(type)
                   || type.IsAnonymous()
                   || TypeIsIGrouping(type);
        }


        public virtual bool TypeIsMappedAsCollection(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            Type _;
            return type != typeof (string) &&
                   type.UniqueToken() != typeof (IGrouping<,>).UniqueToken() &&
                   type.TryGetEnumerableElementType(out _);
        }


        public virtual bool TypeIsMappedAsSharedType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return type.IsEnum || IsNativelySupportedType(type) || TypeIsMappedAsCollection(type);
        }


        public virtual bool TypeIsMappedAsTransformedType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return SourceTypes.Contains(type) || type.IsAnonymous() || TypeIsIGrouping(type);
        }


        public virtual bool TypeIsMappedAsValueObject(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return type.IsAnonymous();
        }

        public virtual bool TypeIsExposedAsRepository(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return GetUriBaseType(type) != null;
        }


        public virtual bool PropertyIsAttributes(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            return false;
        }

        public virtual bool PropertyFormulaIsDecompiled(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            return false;
        }

        public virtual LambdaExpression GetDecompiledPropertyFormula(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            var getMethod = propertyInfo.GetGetMethod(true);
            var decompiled = getMethod.Decompile();
            return decompiled;
        }

        public virtual bool PropertyIsEtag(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            return false;
        }

        public virtual string GetPluralNameForType(Type type)
        {
            return SingularToPluralTranslator.CamelCaseToPlural(type.Name);
        }

        public virtual PropertyCreateMode GetPropertyCreateMode(PropertyInfo propertyInfo,
                                                                ParameterInfo ctorParameterInfo)
        {
            if (ctorParameterInfo != null)
            {
                return PropertyCreateMode.Required;
            }

            if (propertyInfo.PropertyType.IsCollection() ||
                (propertyInfo.CanWrite && propertyInfo.GetSetMethod() != null))
            {
                return PropertyCreateMode.Optional;
            }
            return PropertyCreateMode.Excluded;
        }

        public virtual PropertyAccessMode GetPropertyAccessMode(PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.IsCollection() ||
                (propertyInfo.CanWrite && propertyInfo.GetSetMethod() != null))
            {
                return PropertyAccessMode.ReadWrite;
            }
            return PropertyAccessMode.ReadOnly;
        }

        public virtual int? GetPropertyConstructorArgIndex(PropertyInfo propertyInfo)
        {
            return null;
        }

        public virtual bool PostOfTypeIsAllowed(Type type)
        {
            return true;
        }

        public virtual bool PatchOfTypeIsAllowed(Type type)
        {
            return true;
        }

        public virtual Action<object> GetOnDeserializedHook(Type type)
        {
            return null;
        }

        public virtual LambdaExpression GetPropertyFormula(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            return null;
        }


        public PropertyInfo GetOneToManyCollectionForeignKey(PropertyInfo collectionProperty)
        {
            if (collectionProperty == null) throw new ArgumentNullException("collectionProperty");
            Type[] genericArguments;
            if (
                !TypeUtils.TryGetTypeArguments(
                    collectionProperty.PropertyType, typeof (IEnumerable<>), out genericArguments))
                return null;

            var elementType = genericArguments[0];

            var foreignPropCandicates =
                elementType.GetProperties().Where(x => x.PropertyType == collectionProperty.DeclaringType).ToList();
            if (foreignPropCandicates.Count > 1)
            {
                log.Warn(
                    "Not mapping foreign key relation of one-to-many collection property " + collectionProperty.Name
                    + " of type "
                    + collectionProperty.DeclaringType.FullName + " since there are multiple candidates on other side: "
                    + string.Join(", ", foreignPropCandicates.Select(x => x.Name)) + " (of " + elementType.FullName);
            }

            return foreignPropCandicates.Count == 1 ? foreignPropCandicates[0] : null;
        }


        public bool PropertyIsPrimaryId(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
            return propertyInfo.Name.ToLower() == "id";
        }

        public virtual bool TypeIsMapped()
        {
            return TypeIsMapped(null);
        }


        private bool TypeIsIGrouping(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return type.UniqueToken() == typeof (IGrouping<,>).UniqueToken();
        }

        #endregion

        public virtual DefaultPropertyInclusionMode GetDefaultPropertyInclusionMode()
        {
            return DefaultPropertyInclusionMode.AllPropertiesAreIncludedByDefault;
        }


        public virtual string ApiVersion
        {
            get { return "0.1.0"; }
        }

        private static bool IsNullableType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition().Equals(typeof (Nullable<>));
        }


        private bool IsNativelySupportedType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return jsonSupportedNativeTypes.Contains(type) || IsNullableAllowedNativeType(type);
        }


        private bool IsNullableAllowedNativeType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return IsNullableType(type) &&
                   TypeIsMapped(type.GetGenericArguments()[0]);
        }
    }
}