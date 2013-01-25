using System;
using System.Reflection;

namespace Pomona.FluentMapping
{
    internal class PropertyMappingOptions
    {
        private readonly PropertyInfo propertyInfo;

        public PropertyMappingOptions(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            this.propertyInfo = propertyInfo;
            InclusionMode = PropertyInclusionMode.Default;

            Name = propertyInfo.Name;
        }

        public PropertyInfo PropertyInfo
        {
            get { return propertyInfo; }
        }

        public int? ConstructorArgIndex { get; set; }


        public PropertyInclusionMode InclusionMode { get; internal set; }
        public string Name { get; set; }

        public bool? IsPrimaryKey { get; set; }
    }
}