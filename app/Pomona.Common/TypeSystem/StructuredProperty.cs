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


        public override HttpMethod AccessMode
        {
            get { return StructuredPropertyDetails.AccessMode; }
        }

        public override ExpandMode ExpandMode
        {
            get { return StructuredPropertyDetails.ExpandMode; }
        }

        public virtual bool ExposedOnUrl
        {
            get
            {
                // TODO: Make this configurable
                return PropertyType is ResourceType || PropertyType is EnumerableTypeSpec;
            }
        }

        public virtual bool IsAttributesProperty
        {
            get { return StructuredPropertyDetails.IsAttributesProperty; }
        }

        public virtual bool IsEtagProperty
        {
            get { return StructuredPropertyDetails.IsEtagProperty; }
        }

        public virtual bool IsPrimaryKey
        {
            get { return StructuredPropertyDetails.IsPrimaryKey; }
        }

        public override bool IsSerialized
        {
            get { return StructuredPropertyDetails.IsSerialized; }
        }

        public override HttpMethod ItemAccessMode
        {
            get { return StructuredPropertyDetails.ItemAccessMode; }
        }

        public new StructuredType ReflectedType
        {
            get { return (StructuredType)base.ReflectedType; }
        }

        protected virtual StructuredPropertyDetails StructuredPropertyDetails
        {
            get { return this.structuredPropertyDetails.Value; }
        }
    }
}