#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Newtonsoft.Json;

using Pomona.Common.TypeSystem;
using Pomona.Example.Models;

namespace Pomona.Example
{
    public class CritterTypeMappingFilter : TypeMappingFilterBase
    {
        public CritterTypeMappingFilter(IEnumerable<Type> sourceTypes)
            : base(sourceTypes)
        {
        }


        public override ClientMetadata ClientMetadata
        {
            get { return base.ClientMetadata.With("Critters.Client", "CritterClient", "ICritterClient", "Critters.Client"); }
        }


        public override bool ClientEnumIsGeneratedAsStringEnum(Type enumType)
        {
            return enumType == typeof(CustomStringEnum);
        }


        public override bool ClientPropertyIsExposedAsRepository(PropertyInfo propertyInfo)
        {
            if (propertyInfo.DeclaringType == typeof(Farm) && propertyInfo.Name == "Critters")
                return true;

            return base.ClientPropertyIsExposedAsRepository(propertyInfo);
        }


        public override bool GenerateIndependentClient()
        {
            return false;
        }


        public override IEnumerable<PropertyInfo> GetAllPropertiesOfType(Type type, BindingFlags bindingFlags)
        {
            return base.GetAllPropertiesOfType(type, bindingFlags).Where(x => x.Name != "PropertyExcludedByGetAllPropertiesOfType");
        }


        public override Type GetClientLibraryType(Type type)
        {
            if (type == typeof(WebColor))
                return typeof(string);

            return base.GetClientLibraryType(type);
        }


        public override JsonConverter GetJsonConverterForType(Type type)
        {
            if (type == typeof(WebColor))
                return new WebColorConverter();

            return base.GetJsonConverterForType(type);
        }


        public override ExpandMode GetPropertyExpandMode(Type type, PropertyInfo propertyInfo)
        {
            if (propertyInfo.DeclaringType == typeof(DictionaryContainer) && propertyInfo.Name == "Map")
                return ExpandMode.Full;
            if (propertyInfo.DeclaringType == typeof(OrderResponse) && propertyInfo.Name == "Order")
                return ExpandMode.Full;

            return base.GetPropertyExpandMode(type, propertyInfo);
        }


        public override PropertyFlags? GetPropertyFlags(PropertyInfo propertyInfo)
        {
            if (propertyInfo.Name == "IsNotAllowedInFilters")
                return base.GetPropertyFlags(propertyInfo) & ~PropertyFlags.AllowsFiltering;
            return base.GetPropertyFlags(propertyInfo);
        }


        public override Type GetUriBaseType(Type type)
        {
            if (typeof(Order).IsAssignableFrom(type))
                return typeof(Order);
            if (typeof(Weapon).IsAssignableFrom(type))
                return typeof(Weapon);
            if (type == typeof(MusicalCritter))
                return typeof(Critter);
            if (typeof(DictionaryContainer).IsAssignableFrom(type))
                return typeof(DictionaryContainer);

            if (type == typeof(EntityBase))
                return null;

            return base.GetUriBaseType(type);
        }


        public override Type ResolveRealTypeForProxy(Type type)
        {
            if (typeof(IExposedInterface).IsAssignableFrom(type))
                return typeof(IExposedInterface);
            return base.ResolveRealTypeForProxy(type);
        }


        public override bool TypeIsMapped(Type type)
        {
            if (type == typeof(ExcludedThing) || type == typeof(HiddenBaseInMiddle))
                return false;

            return base.TypeIsMapped(type);
        }


        public override bool TypeIsMappedAsSharedType(Type type)
        {
            if (type == typeof(WebColor))
                return true;

            return base.TypeIsMappedAsSharedType(type);
        }


        public override bool TypeIsMappedAsValueObject(Type type)
        {
            if (type == typeof(OrderResponse))
                return true;

            if (type == typeof(CrazyValueObject))
                return true;
            return base.TypeIsMappedAsValueObject(type);
        }
    }
}