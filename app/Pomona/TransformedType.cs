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
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Pomona.Common;
using Pomona.Common.Internals;
using Pomona.Common.TypeSystem;
using Pomona.Handlers;
using Pomona.Internals;

namespace Pomona
{
    /// <summary>
    /// Represents a type that is transformed
    /// </summary>
    public class TransformedType : IMappedType
    {
        private static readonly MethodInfo addItemsToDictionaryMethod =
            ReflectionHelper.GetMethodDefinition<TransformedType>(
                x => x.AddItemsToDictionary<object, object>(null, null));

        private static readonly MethodInfo addValuesToCollectionMethod =
            ReflectionHelper.GetMethodDefinition<TransformedType>(
                x => x.AddValuesToCollection<object>(null, null));

        private readonly List<HandlerInfo> declaredPostHandlers = new List<HandlerInfo>();
        private readonly Dictionary<string, object> extraData = new Dictionary<string, object>();

        private readonly Type mappedType;
        private readonly string name;

        private readonly List<PropertyMapping> properties = new List<PropertyMapping>();

        private readonly TypeMapper typeMapper;

        internal TransformedType(Type mappedType, string name, TypeMapper typeMapper)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.mappedType = mappedType;
            this.name = name;
            this.typeMapper = typeMapper;

            UriBaseType = this;
            PluralName = typeMapper.Filter.GetPluralNameForType(mappedType);
            PostReturnType = this;
        }

        public Dictionary<string, object> ExtraData
        {
            get { return extraData; }
        }

        public IList<HandlerInfo> DeclaredPostHandlers
        {
            get { return declaredPostHandlers; }
        }

        public IEnumerable<HandlerInfo> PostHandlers
        {
            get
            {
                var baseType = BaseType as TransformedType;
                return (IsUriBaseType || baseType == null)
                           ? declaredPostHandlers
                           : declaredPostHandlers.Concat(baseType.PostHandlers);
            }
        }


        public ConstructorInfo ConstructorInfo { get; set; }

        public bool IsUriBaseType
        {
            get { return UriBaseType == this; }
        }

        public IEnumerable<TransformedType> SubTypes
        {
            get
            {
                return
                    typeMapper.TransformedTypes.Where(x => x.BaseType == this).SelectMany(x => x.SubTypes.Concat(x));
            }
        }

        /// <summary>
        /// Other types having the same URI as this type. (in same inheritance chain)
        /// </summary>
        public IEnumerable<TransformedType> MergedTypes
        {
            get
            {
                if (UriBaseType == null)
                    return Enumerable.Empty<TransformedType>();
                return typeMapper.TransformedTypes.Where(x => x != this && x.UriBaseType == UriBaseType);
            }
        }

        public PropertyMapping ETagProperty
        {
            get { return properties.FirstOrDefault(x => x.IsEtagProperty); }
        }

        /// <summary>
        /// Action to be called after deserialization has completed.
        /// </summary>
        public Action<object> OnDeserialized { get; set; }

        public bool PatchAllowed { get; set; }

        public bool PostAllowed { get; set; }

        /// <summary>
        /// What type will be returned when this type is POST'ed.
        /// By default this will refer to itself.
        /// </summary>
        public TransformedType PostReturnType { get; set; }

        public TypeMapper TypeMapper
        {
            get { return typeMapper; }
        }

        public bool IsExposedAsRepository { get; set; }

        public TransformedType UriBaseType { get; set; }

        public string UriRelativePath { get; set; }

        #region IMappedType Members

        public IMappedType BaseType { get; set; }

        public Type CustomClientLibraryType
        {
            get { return null; }
        }

        public IMappedType DictionaryKeyType
        {
            get { throw new NotSupportedException(); }
        }

        public IMappedType DictionaryType
        {
            get { throw new NotSupportedException(); }
        }

        public IMappedType DictionaryValueType
        {
            get { throw new NotSupportedException(); }
        }

        public IMappedType ElementType
        {
            get
            {
                throw new InvalidOperationException(
                    "TransformedType is never a collection, so it won't have a element type.");
            }
        }

        public IList<IMappedType> GenericArguments
        {
            get { return new IMappedType[] { }; }
        }

        public bool IsAlwaysExpanded
        {
            get { return MappedAsValueObject; }
        }

        public bool IsBasicWireType
        {
            get { return false; }
        }

        public bool IsCollection
        {
            get { return false; }
        }

        public bool IsNullable
        {
            get { return false; }
        }

        public bool IsDictionary
        {
            get { return false; }
        }

        public bool IsGenericType
        {
            get { return false; }
        }

        public bool IsGenericTypeDefinition
        {
            get { return false; }
        }

        public bool IsValueType
        {
            get { return false; }
        }

        public JsonConverter JsonConverter
        {
            get { return null; }
        }

        public Type MappedTypeInstance
        {
            get { return mappedType; }
        }

        public string Name
        {
            get { return name; }
        }


        public object Create(IDictionary<IPropertyInfo, object> args)
        {
            var propValues = new List<KeyValuePair<PropertyMapping, object>>(args.Count);
            //var ctorValues = new List<KeyValuePair<PropertyMapping, object>>(args.Count);
            var ctorParamCount = ConstructorInfo.GetParameters().Length;
            var ctorArgs = new object[ctorParamCount];
            var ctorDirtyMap = new bool[ctorParamCount];
            var ctorArgDirtyCount = 0;

            foreach (var kvp in args)
            {
                var propMapping = (PropertyMapping)kvp.Key;
                var ctorArgIndex = propMapping.ConstructorArgIndex;

                if (ctorArgIndex == -1)
                    propValues.Add(new KeyValuePair<PropertyMapping, object>(propMapping, kvp.Value));
                else
                {
                    ctorArgs[ctorArgIndex] = kvp.Value;
                    ctorDirtyMap[ctorArgIndex] = true;
                    ctorArgDirtyCount++;
                }
            }

            if (ctorArgDirtyCount < ctorParamCount)
            {
                // Set non-dirty ctor args to default value
                for (var i = 0; i < ctorParamCount; i++)
                {
                    if (!ctorDirtyMap[i])
                    {
                        // Set to default value
                        var prop = properties.First(x => x.ConstructorArgIndex == i);
                        if (prop.CreateMode == PropertyCreateMode.Required)
                            throw new ResourceValidationException(
                                string.Format("Property {0} is required when creating resource {1}", prop.Name, Name),
                                prop.Name, Name, null);
                    }
                }
            }

            var instance = Activator.CreateInstance(MappedTypeInstance, ctorArgs);

            foreach (var kvp in propValues)
            {
                var prop = kvp.Key;

                // Special handling for lists and dictionaries:
                // In certain circumstances setting collection property is not what we want to do.
                // If the instance has a pre-initialized collection we instead want to fill it with
                // values.

                var setPropertyToValue = true;

                if (prop.PropertyType.IsDictionary)
                {
                    var dict = prop.Getter(instance);
                    if (dict != null)
                    {
                        addItemsToDictionaryMethod.MakeGenericMethod(
                            prop.PropertyType.MappedTypeInstance.GetGenericArguments())
                                                  .Invoke(this, new[] { kvp.Value, dict });
                        setPropertyToValue = false;
                    }
                }
                else if (prop.PropertyType.IsCollection)
                {
                    var collection = prop.Getter(instance);
                    if (collection != null)
                    {
                        addValuesToCollectionMethod
                            .MakeGenericMethod(prop.PropertyType.ElementType.MappedTypeInstance)
                            .Invoke(this, new[] { kvp.Value, collection });
                        setPropertyToValue = false;
                    }
                }


                if (setPropertyToValue)
                    prop.Setter(instance, kvp.Value);
            }

            return instance;
        }

        private object AddItemsToDictionary<TKey, TValue>(IDictionary<TKey, TValue> source,
                                                          IDictionary<TKey, TValue> target)
        {
            foreach (var item in source)
                target.Add(item);
            return null;
        }

        private object AddValuesToCollection<TElement>(IEnumerable<TElement> source, ICollection<TElement> target)
        {
            foreach (var item in source)
                target.Add(item);
            return null;
        }

        #endregion

        /// <summary>
        /// When true this type is considered a value object, which will affect serialization.
        /// It also means that it don't have an URL or identity.
        /// </summary>
        public bool MappedAsValueObject { get; set; }

        public string PluralName { get; set; }

        public bool HasUri
        {
            get { return !MappedAsValueObject && UriBaseType != null; }
        }

        public Type MappedType
        {
            get { return mappedType; }
        }

        public IPropertyInfo PrimaryId { get; set; }

        public IList<IPropertyInfo> Properties
        {
            get { return new CastingListWrapper<IPropertyInfo>(properties); }
        }

        public TypeSerializationMode SerializationMode
        {
            get { return TypeSerializationMode.Complex; }
        }

        public string ConvertToInternalPropertyPath(string externalPath)
        {
            // TODO: Fix for lists, but first gotta find out how that would work..

            string externalPropertyName, remainingExternalPath;
            TakeLeftmostPathPart(externalPath, out externalPropertyName, out remainingExternalPath);

            // TODO: Fix for multiple inherited types with property of same name..
            var prop =
                Properties.Concat(MergedTypes.SelectMany(x => x.Properties)).OfType<PropertyMapping>().FirstOrDefault(
                    x => x.Name.ToLowerInvariant() == externalPropertyName.ToLowerInvariant());
            if (prop == null)
            {
                throw new PomonaMappingException(
                    string.Format(
                        "Could not find property with name {0} on type {1} while resolving path {2}",
                        externalPropertyName,
                        Name,
                        externalPath));
            }

            var internalPropertyName = prop.PropertyInfo.Name;

            if (remainingExternalPath != null)
            {
                var pathType = prop.PropertyType;
                if (pathType.IsCollection)
                    pathType = pathType.ElementType;

                var nextType = (TransformedType)pathType;
                return internalPropertyName + "." + nextType.ConvertToInternalPropertyPath(remainingExternalPath);
            }
            return internalPropertyName;
        }

        public override string ToString()
        {
            if (!IsGenericType)
            {
                return Name;
            }

            return string.Format("{0}<{1}>", Name, string.Join(",", GenericArguments));
        }

        public Expression CreateExpressionForExternalPropertyPath(string externalPath)
        {
            if (MappedType == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Can't convert an external property path to expression with source-less type {0} as root!",
                        Name));
            }

            var parameter = Expression.Parameter(MappedType, "x");
            var propertyAccessExpression = CreateExpressionForExternalPropertyPath(parameter, externalPath);

            return Expression.Lambda(propertyAccessExpression, parameter);
        }


        public Expression CreateExpressionForExternalPropertyPath(Expression instance, string externalPath)
        {
            string externalPropertyName, remainingExternalPath;
            TakeLeftmostPathPart(externalPath, out externalPropertyName, out remainingExternalPath);

            var prop =
                Properties.OfType<PropertyMapping>().First(x => x.Name.ToLower() == externalPropertyName.ToLower());

            var propertyAccessExpression = prop.CreateGetterExpression(instance);

            if (remainingExternalPath != null)
            {
                // TODO Error handling here when remaningpath does not represents a TransformedType
                var transformedPropType = prop.PropertyType as TransformedType;
                if (transformedPropType == null)
                {
                    throw new InvalidOperationException(
                        "Can not filter by subproperty when property is not TransformedType");
                }
                return transformedPropType.CreateExpressionForExternalPropertyPath(
                    propertyAccessExpression, remainingExternalPath);
            }
            return propertyAccessExpression;
        }


        public object GetId(object entity)
        {
            return typeMapper.Filter.GetIdFor(entity);
        }


        public PropertyMapping GetPropertyByJsonName(string jsonPropertyName)
        {
            // TODO: Create a dictionary for this if suboptimal.
            return properties.First(x => x.JsonName == jsonPropertyName);
        }


        public PropertyMapping GetPropertyByName(string propertyName, bool ignoreCase)
        {
            if (ignoreCase)
                propertyName = propertyName.ToLower();

            // TODO: Possible to optimize here by putting property names in a dictionary
            return properties.First(x => x.Name == propertyName);
        }


        public void TakeLeftmostPathPart(string path, out string leftName, out string remainingPropPath)
        {
            var leftPathSeparatorIndex = path.IndexOf('.');
            if (leftPathSeparatorIndex == -1)
            {
                leftName = path;
                remainingPropPath = null;
            }
            else
            {
                leftName = path.Substring(0, leftPathSeparatorIndex);
                remainingPropPath = path.Substring(leftPathSeparatorIndex + 1);
            }
        }


        private Type GetKnownDeclaringType(PropertyInfo propertyInfo)
        {
            // Hack, IGrouping

            var propBaseDefinition = propertyInfo.GetBaseDefinition();
            var reflectedType = propertyInfo.ReflectedType;
            return reflectedType.GetFullTypeHierarchy()
                                .Where(x => propBaseDefinition.DeclaringType.IsAssignableFrom(x))
                                .TakeUntil(x => typeMapper.Filter.IsIndependentTypeRoot(x))
                                .LastOrDefault(x => typeMapper.SourceTypes.Contains(x)) ??
                   propBaseDefinition.DeclaringType;
        }

        internal void ScanProperties(Type type)
        {
            var filter = typeMapper.Filter;

            var scannedProperties = GetPropertiesToScanOrderedByName(type).ToList();

            // Find longest (most specific) public constructor
            var constructor = typeMapper.Filter.GetTypeConstructor(type);
            var ctorParams = constructor != null ? constructor.GetParameters() : null;
            ConstructorInfo = constructor;

            foreach (var propInfo in scannedProperties)
            {
                if (!filter.PropertyIsIncluded(propInfo))
                    continue;

                var declaringType = typeMapper.GetClassMapping(GetKnownDeclaringType(propInfo));

                var propInfoLocal = propInfo;
                var getter = filter.GetPropertyGetter(propInfo);
                var setter = filter.GetPropertySetter(propInfo);
                var propertyType = filter.GetPropertyType(propInfo);
                var propertyTypeMapped = typeMapper.GetClassMapping(propertyType);

                var propDef = new PropertyMapping(
                    typeMapper.Filter.GetPropertyMappedName(propInfo),
                    this,
                    (TransformedType)declaringType,
                    propertyTypeMapped,
                    propInfo);

                propDef.Getter = getter;
                propDef.Setter = setter;
                propDef.AlwaysExpand = filter.PropertyIsAlwaysExpanded(propInfo);
                if (filter.PropertyIsPrimaryId(propInfo))
                    PrimaryId = propDef;
                propDef.IsAttributesProperty = filter.PropertyIsAttributes(propInfo);

                ParameterInfo matchingCtorArg = null;
                if (constructor != null)
                {
                    var constructorArgIndex = filter.GetPropertyConstructorArgIndex(propInfo);
                    if (constructorArgIndex.HasValue)
                    {
                        matchingCtorArg = ctorParams.FirstOrDefault(x => x.Position == constructorArgIndex.Value);
                        if (matchingCtorArg == null)
                            throw new InvalidOperationException(
                                string.Format("Unable to locate parameter with position {0} in ctor.",
                                              constructorArgIndex.Value));
                        propDef.ConstructorArgIndex = constructorArgIndex.Value;
                    }
                    else
                    {
                        matchingCtorArg = ctorParams.FirstOrDefault(x => x.Name.ToLower() == propDef.LowerCaseName);
                        if (matchingCtorArg != null)
                        {
                            propDef.ConstructorArgIndex = matchingCtorArg.Position;
                        }
                    }
                }

                // TODO: Fix this for transformed properties with custom get/set methods.
                propDef.CreateMode = filter.GetPropertyCreateMode(propInfoLocal, matchingCtorArg);
                propDef.AccessMode = filter.GetPropertyAccessMode(propInfoLocal);
                propDef.ExposedAsRepository = filter.ClientPropertyIsExposedAsRepository(propInfoLocal);
                propDef.IsEtagProperty = filter.PropertyIsEtag(propInfo);

                var formula = filter.GetPropertyFormula(propInfo);

                if (formula == null && filter.PropertyFormulaIsDecompiled(propInfo))
                {
                    formula = filter.GetDecompiledPropertyFormula(propInfo);
                }
                propDef.Formula = formula;

                var propertyDescription = filter.GetPropertyDescription(propInfo);
                if (propertyDescription != null)
                {
                    propDef.Description = propertyDescription;
                }

                properties.Add(propDef);
            }

            // TODO: Support not including the whole type hierarchy. Remember that base type might be allowed to be a shared type.
            var exposedBaseType = type.BaseType;

            while (exposedBaseType != null && !filter.TypeIsMapped(exposedBaseType))
                exposedBaseType = exposedBaseType.BaseType;

            if (exposedBaseType != null)
            {
                BaseType = typeMapper.GetClassMapping(exposedBaseType);
            }
        }


        private static IEnumerable<PropertyInfo> GetPropertiesToScanOrderedByName(Type type)
        {
            return
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(
                    x => x.Name).Where(x => x.GetIndexParameters().Count() == 0);
        }
    }
}