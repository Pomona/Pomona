using System;
using System.Reflection;

namespace Pomona
{
    public class PropertyMapping
    {
        public PropertyMapping(string name, PropertyInfo propertyInfo)
        {
            this.name = name;
            this.propertyInfo = propertyInfo;
        }


        public PropertyInfo PropertyInfo
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