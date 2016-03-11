#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PomonaConfigurationAttribute : Attribute
    {
        public PomonaConfigurationAttribute(Type configurationType)
        {
            if (configurationType == null)
                throw new ArgumentNullException(nameof(configurationType));
            ConfigurationType = configurationType;
        }


        public Type ConfigurationType { get; }
    }
}