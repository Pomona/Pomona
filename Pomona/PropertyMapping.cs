using System;
using System.Reflection;

namespace Pomona
{
    public class PropertyMapping
    {
        public PropertyMapping(string name, IMappedType declaringType, IMappedType propertyType, PropertyInfo propertyInfo)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (declaringType == null) throw new ArgumentNullException("declaringType");
            if (propertyType == null) throw new ArgumentNullException("propertyType");
            this.name = name;
            this.declaringType = declaringType;
            this.propertyType = propertyType;
            this.propertyInfo = propertyInfo;
        }

        public IMappedType DeclaringType
        {
            get { return declaringType; }
        }

        public IMappedType PropertyType
        {
            get { return propertyType; }
        }

        protected PropertyInfo PropertyInfo
        {
            get { return this.propertyInfo; }
        }

        public string JsonName
        {
            get { return name.Substring(0, 1).ToLower() + name.Substring(1); }
        }

        public string Name
        {
            get { return this.name; }
        }

        public Action<object, object> Setter { get; set; }
        public Func<object, object> Getter { get; set; }

        private bool updateAllowed;

        private string name;
        private readonly IMappedType declaringType;
        private readonly IMappedType propertyType;
        private readonly PropertyInfo propertyInfo;

        public enum PropertyCreateMode
        {
            Excluded,  // Default for all generated properties.
            Optional, // Default for all publicly writable properties, 
            Required, // Default for properties that got a matching argument in shortest constructor
        }

        public int ConstructorArgIndex { get; set; }

        public PropertyCreateMode CreateMode { get; set; }
    }
}