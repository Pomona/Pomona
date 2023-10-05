#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pomona.Common.TypeSystem
{
    public class RuntimePropertySpec : PropertySpec
    {
        public RuntimePropertySpec(ITypeResolver typeResolver, PropertyInfo propertyInfo, RuntimeTypeSpec reflectedType)
            : base(typeResolver, propertyInfo, reflectedType)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));
        }


        public override IEnumerable<Attribute> InheritedAttributes
        {
            get
            {
                if (BaseDefinition == null || BaseDefinition == this)
                    return Enumerable.Empty<Attribute>();
                return BaseDefinition.Attributes;
            }
        }
    }
}
