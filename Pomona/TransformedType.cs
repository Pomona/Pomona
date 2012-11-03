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
using System.Linq.Expressions;
using System.Reflection;

using Newtonsoft.Json;

namespace Pomona
{
    /// <summary>
    /// Represents a type that is transformed
    /// </summary>
    public class TransformedType : IMappedType
    {
        private readonly string name;
        private readonly List<PropertyMapping> properties = new List<PropertyMapping>();

        private readonly Type sourceType;
        private readonly TypeMapper typeMapper;


        internal TransformedType(Type sourceType, string name, TypeMapper typeMapper)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.sourceType = sourceType;
            this.name = name;
            this.typeMapper = typeMapper;

            UriBaseType = this;
        }


        public ConstructorInfo ConstructorInfo { get; set; }
        public bool MappedAsValueObject { get; set; }
        public string PluralName { get; set; }

        public bool PostAllowed
        {
            get { return true; }
        }

        public IList<PropertyMapping> Properties
        {
            get { return this.properties; }
        }

        public Type SourceType
        {
            get { return this.sourceType; }
        }

        public TransformedType UriBaseType { get; set; }

        public string UriRelativePath
        {
            get
            {
                // TODO: Make it possible to modify path
                return UriBaseType.Name.ToLower();
            }
        }

        #region IMappedType Members

        public IMappedType BaseType { get; set; }

        public IMappedType CollectionElementType
        {
            get
            {
                throw new InvalidOperationException(
                    "TransformedType is never a collection, so it won't have a element type.");
            }
        }

        public Type CustomClientLibraryType
        {
            get { return null; }
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

        public string Name
        {
            get { return this.name; }
        }

        #endregion

        public string ConvertToInternalPropertyPath(string externalPath)
        {
            // TODO: Fix for inherited types..
            // TODO: Fix for lists, but first gotta find out how that would work..

            string externalPropertyName, remainingExternalPath;
            TakeLeftmostPathPart(externalPath, out externalPropertyName, out remainingExternalPath);

            var prop =
                Properties.FirstOrDefault(x => x.Name.ToLowerInvariant() == externalPropertyName.ToLowerInvariant());
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
                    pathType = pathType.CollectionElementType;

                var nextType = (TransformedType)pathType;
                return internalPropertyName + "." + nextType.ConvertToInternalPropertyPath(remainingExternalPath);
            }
            return internalPropertyName;
        }


        public Expression CreateExpressionForExternalPropertyPath(string externalPath)
        {
            if (SourceType == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Can't convert an external property path to expression with source-less type {0} as root!",
                        Name));
            }

            var parameter = Expression.Parameter(SourceType, "x");
            var propertyAccessExpression = CreateExpressionForExternalPropertyPath(parameter, externalPath);

            return Expression.Lambda(propertyAccessExpression, parameter);
        }


        public Expression CreateExpressionForExternalPropertyPath(Expression instance, string externalPath)
        {
            string externalPropertyName, remainingExternalPath;
            TakeLeftmostPathPart(externalPath, out externalPropertyName, out remainingExternalPath);

            var prop = Properties.First(x => x.Name.ToLower() == externalPropertyName.ToLower());

            if (prop.PropertyInfo == null)
            {
                throw new NotImplementedException(
                    "Can only make expression paths for PropertyMappings to a specific internal property (with PropertyInfo)");
            }

            var propertyAccessExpression = Expression.Property(instance, prop.PropertyInfo);

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
            return this.typeMapper.Filter.GetIdFor(entity);
        }


        public PropertyMapping GetPropertyByJsonName(string jsonPropertyName)
        {
            // TODO: Create a dictionary for this if suboptimal.
            return Properties.First(x => x.JsonName == jsonPropertyName);
        }


        public PropertyMapping GetPropertyByName(string propertyName, bool ignoreCase)
        {
            if (ignoreCase)
                propertyName = propertyName.ToLower();

            // TODO: Possible to optimize here by putting property names in a dictionary
            return Properties.First(x => x.Name == propertyName);
        }


        /// <summary>
        /// Creates an newinstance of type that TransformedType targets
        /// </summary>
        /// <param name="initValues">Dictionary of initial values, prop names must be lowercased!</param>
        /// <returns></returns>
        public object NewInstance(IDictionary<string, object> initValues)
        {
            // Initvalues must be lowercased!
            // HACK attack! This must probably be rethought..

            var requiredCtorArgCount = ConstructorInfo.GetParameters().Count();
            var ctorArgs = new object[requiredCtorArgCount];

            foreach (
                var ctorProp in
                    Properties.Where(
                        x => x.CreateMode == PropertyMapping.PropertyCreateMode.Required && x.ConstructorArgIndex >= 0))
            {
                // TODO: Proper validation here!
                var value = initValues[ctorProp.Name.ToLower()];

                if (ctorProp.PropertyType.IsBasicWireType)
                    value = Convert.ChangeType(value, ((SharedType)ctorProp.PropertyType).TargetType);

                ctorArgs[ctorProp.ConstructorArgIndex] = value;
            }

            var newInstance = Activator.CreateInstance(this.sourceType, ctorArgs);

            foreach (var optProp in Properties.Where(x => x.CreateMode == PropertyMapping.PropertyCreateMode.Optional))
            {
                object propSetValue;
                if (initValues.TryGetValue(optProp.Name.ToLower(), out propSetValue))
                    optProp.Setter(newInstance, propSetValue);
            }

            return newInstance;
        }


        public void ScanProperties(Type type)
        {
            var filter = this.typeMapper.Filter;

            foreach (
                var propInfo in
                    type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(
                        x => x.Name)
                        .Where(x => x.GetIndexParameters().Count() == 0))
            {
                if (!filter.PropertyIsIncluded(propInfo))
                    continue;

                IMappedType declaringType;

                if (this.typeMapper.SourceTypes.Contains(propInfo.DeclaringType))
                    declaringType = this.typeMapper.GetClassMapping(propInfo.DeclaringType);
                else
                {
                    // TODO: Find lowest base type with this property
                    declaringType = this;
                }

                var propInfoLocal = propInfo;
                var getter = filter.GetPropertyGetter(propInfo);
                var setter = filter.GetPropertySetter(propInfo);
                var propertyType = filter.GetPropertyType(propInfo);

                var propDef = new PropertyMapping(
                    this.typeMapper.Filter.GetPropertyMappedName(propInfo),
                    declaringType,
                    this.typeMapper.GetClassMapping(propertyType),
                    propInfo);

                // TODO: This is not the most optimized way to set property, small code gen needed.
                propDef.Getter = getter;
                propDef.Setter = setter;

                // TODO: Fix this for transformed properties with custom get/set methods.
                // TODO: This should rather be configured by filter.
                if (propInfoLocal.CanWrite && propInfoLocal.GetSetMethod() != null)
                {
                    propDef.CreateMode = PropertyMapping.PropertyCreateMode.Optional;
                    propDef.AccessMode = PropertyMapping.PropertyAccessMode.ReadWrite;
                }
                else
                {
                    propDef.CreateMode = PropertyMapping.PropertyCreateMode.Excluded;
                    propDef.AccessMode = PropertyMapping.PropertyAccessMode.ReadOnly;
                }

                this.properties.Add(propDef);
            }

            // TODO: Support not including the whole type hierarchy. Remember that base type might be allowed to be a shared type.
            BaseType = this.typeMapper.GetClassMapping(type.BaseType);

            // Find longest (most specific) public constructor
            var longestCtor = type.GetConstructors().OrderByDescending(x => x.GetParameters().Length).FirstOrDefault();
            ConstructorInfo = longestCtor;

            if (longestCtor != null)
            {
                // TODO: match constructor arguments
                foreach (var ctorParam in longestCtor.GetParameters())
                {
                    var matchingProperty = this.properties.FirstOrDefault(x => x.JsonName.ToLower() == ctorParam.Name);
                    if (matchingProperty != null)
                    {
                        matchingProperty.CreateMode = PropertyMapping.PropertyCreateMode.Required;
                        matchingProperty.ConstructorArgIndex = ctorParam.Position;
                    }
                }
            }
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
    }
}