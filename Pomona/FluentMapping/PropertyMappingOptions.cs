using System;
using System.Reflection;

namespace Pomona.FluentMapping
{
    internal class PropertyMappingOptions
    {
        private readonly PropertyInfo propertyInfo;

        public PropertyInfo PropertyInfo
        {
            get { return this.propertyInfo; }
        }

        public int? ConstructorArgIndex { get; set; }

        public PropertyMappingOptions(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            this.propertyInfo = propertyInfo;

            this.Name = propertyInfo.Name;
        }


        public PropertyInclusionMode InclusionMode { get; internal set; }
        public string Name { get; set; }

        public bool? IsPrimaryKey { get; set; }
    }
}