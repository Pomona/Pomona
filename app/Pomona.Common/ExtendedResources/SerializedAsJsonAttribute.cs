using System;

namespace Pomona.Common.ExtendedResources
{
    /// <summary>
    /// For extended properties stored in attributes of resource, serialized as JSON
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SerializedAsJsonAttribute : Attribute
    {
    }
}