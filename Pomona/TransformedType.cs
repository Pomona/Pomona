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

namespace Pomona
{
    /// <summary>
    /// Represents a type that is transformed
    /// </summary>
    public class TransformedType : IMappedType
    {
        private readonly string name;
        private readonly List<PropertyMapping> properties = new List<PropertyMapping>();
        private readonly TypeMapper typeMapper;

        private Type type;


        internal TransformedType(Type type, string name, TypeMapper typeMapper)
        {
            if (typeMapper == null)
                throw new ArgumentNullException("typeMapper");
            this.type = type;
            this.name = name;
            this.typeMapper = typeMapper;
        }


        public ConstructorInfo ConstructorInfo { get; set; }

        public IList<PropertyMapping> Properties
        {
            get { return this.properties; }
        }

        #region IMappedType Members

        public IMappedType BaseType { get; set; }

        public IList<IMappedType> GenericArguments
        {
            get { return new IMappedType[] { }; }
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

        public string Name
        {
            get { return this.name; }
        }

        #endregion

        public PropertyMapping GetPropertyByJsonName(string jsonPropertyName)
        {
            // TODO: Create a dictionary for this if suboptimal.
            return Properties.First(x => x.JsonName == jsonPropertyName);
        }


        /// <summary>
        /// Creates an newinstance of type that TransformedType targets
        /// </summary>
        /// <param name="transformedType">The type transformation spec.</param>
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
                ctorArgs[ctorProp.ConstructorArgIndex] = Convert.ChangeType(
                    initValues[ctorProp.Name.ToLower()],
                    ((SharedType)ctorProp.PropertyType).
                        TargetType);
            }

            var newInstance = Activator.CreateInstance(this.type, ctorArgs);

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
            foreach (var propInfo in type.GetProperties()
                .Where(x => x.GetGetMethod().IsPublic && x.GetIndexParameters().Count() == 0))
            {
                if (this.typeMapper.Filter != null && !this.typeMapper.Filter.PropertyIsIncluded(propInfo))
                    continue;

                IMappedType declaringType;

                if (this.typeMapper.SourceTypes.Contains(propInfo.DeclaringType))
                    declaringType = this.typeMapper.GetClassMapping(propInfo.DeclaringType);
                else
                {
                    // TODO: Find lowest base type with this property
                    declaringType = this;
                }

                var propDef = new PropertyMapping(
                    propInfo.Name,
                    declaringType,
                    this.typeMapper.GetClassMapping(propInfo.PropertyType),
                    propInfo);

                var propInfoLocal = propInfo;

                // TODO: This is not the most optimized way to set property, small code gen needed.
                propDef.Getter = x => propInfoLocal.GetValue(x, null);
                propDef.Setter = (x, value) => propInfoLocal.SetValue(x, value, null);

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
    }
}