#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pomona.Samples.MiscSnippets
{
    // SAMPLE: crazy-conventions
    public class CrazyConventions : DefaultTypeMappingFilter
    {
        public CrazyConventions(IEnumerable<Type> sourceTypes)
            : base(sourceTypes)
        {
        }


        public override string GetPropertyMappedName(Type type, PropertyInfo propertyInfo)
        {
            // No cursing in the api
            return base.GetPropertyMappedName(type, propertyInfo).Replace("Shit", "Poo");
        }


        public override IEnumerable<Type> GetResourceHandlers(Type type)
        {
            // All our handlers follow the same naming convention
            return new[] { type.Assembly.GetType($"{type.FullName}Handler") };
        }


        public override bool PostOfTypeIsAllowed(Type type)
        {
            // No post of type allowed by default
            return false;
        }
    }

    // ENDSAMPLE
}