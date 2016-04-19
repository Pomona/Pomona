#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    public class StructuredProperty : RuntimePropertySpec
    {
        private readonly Lazy<StructuredPropertyDetails> structuredPropertyDetails;


        public StructuredProperty(IStructuredTypeResolver typeResolver,
                                  PropertyInfo propertyInfo,
                                  StructuredType reflectedType)
            : base(typeResolver, propertyInfo, reflectedType)
        {
            this.structuredPropertyDetails = CreateLazy(() => typeResolver.LoadStructuredPropertyDetails(this));
        }


        public override HttpMethod AccessMode => StructuredPropertyDetails.AccessMode;

        public override ExpandMode ExpandMode => StructuredPropertyDetails.ExpandMode;

        public virtual bool ExposedOnUrl => PropertyType is ResourceType || PropertyType is EnumerableTypeSpec;

        public virtual bool IsAttributesProperty => StructuredPropertyDetails.IsAttributesProperty;

        public virtual bool IsEtagProperty => StructuredPropertyDetails.IsEtagProperty;

        public virtual bool IsPrimaryKey => StructuredPropertyDetails.IsPrimaryKey;

        public override bool IsSerialized => StructuredPropertyDetails.IsSerialized;

        public override HttpMethod ItemAccessMode => StructuredPropertyDetails.ItemAccessMode;

        public new StructuredType ReflectedType => (StructuredType)base.ReflectedType;

        protected virtual StructuredPropertyDetails StructuredPropertyDetails => this.structuredPropertyDetails.Value;
    }
}