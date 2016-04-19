#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.FluentMapping
{
    internal class PropertyMappingOptions
    {
        public PropertyMappingOptions(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));
            PropertyInfo = propertyInfo;
            InclusionMode = PropertyInclusionMode.Default;

            Name = propertyInfo.Name;
            AddedAttributes = new List<Attribute>();
        }


        public List<Attribute> AddedAttributes { get; set; }
        public PropertyCreateMode? CreateMode { get; internal set; }
        public bool? ExposedAsRepository { get; internal set; }
        public LambdaExpression Formula { get; set; }
        public PropertyInclusionMode InclusionMode { get; internal set; }
        public bool? IsAttributesProperty { get; set; }
        public bool? IsEtagProperty { get; set; }
        public bool? IsPrimaryKey { get; set; }
        public HttpMethod ItemMethod { get; internal set; }
        public HttpMethod ItemMethodMask { get; internal set; }
        public HttpMethod Method { get; internal set; }
        public HttpMethod MethodMask { get; internal set; }
        public string Name { get; set; }
        public Func<object, IContainer, object> OnGetDelegate { get; set; }
        public Action<object, object, IContainer> OnSetDelegate { get; set; }
        public ExpandMode? PropertyExpandMode { get; set; }

        public PropertyInfo PropertyInfo { get; }

        public Type PropertyType { get; set; }


        internal void ClearAccessModeFlag(HttpMethod method)
        {
            Method &= ~method;
            MethodMask |= method;
        }


        internal void ClearItemAccessModeFlag(HttpMethod method)
        {
            ItemMethod &= ~method;
            ItemMethodMask |= method;
        }


        internal void SetAccessModeFlag(HttpMethod method)
        {
            Method |= method;
            MethodMask |= method;
        }


        internal void SetItemAccessModeFlag(HttpMethod method)
        {
            ItemMethod |= method;
            ItemMethodMask |= method;
        }
    }
}