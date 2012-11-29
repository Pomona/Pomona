using System;

namespace Pomona.FluentMapping
{
    internal class PropertyOptionsBuilder<TDeclaringType, TPropertyType>
        : IPropertyOptionsBuilder<TDeclaringType, TPropertyType>
    {
        private readonly PropertyMappingOptions propertyMappingOptions;


        public PropertyOptionsBuilder(PropertyMappingOptions propertyMappingOptions)
        {
            if (propertyMappingOptions == null)
                throw new ArgumentNullException("propertyMappingConfigurator");
            this.propertyMappingOptions = propertyMappingOptions;
        }

        #region Implementation of IPropertyOptionsBuilder<TDeclaringType,TPropertyType>

        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> AsPrimaryKey()
        {
            propertyMappingOptions.IsPrimaryKey = true;
            return this;
        }


        public IPropertyOptionsBuilder<TDeclaringType, TPropertyType> Named(string name)
        {
            this.propertyMappingOptions.Name = name;
            return this;
        }

        #endregion
    }
}