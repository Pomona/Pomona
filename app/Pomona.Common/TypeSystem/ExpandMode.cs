#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

namespace Pomona.Common.TypeSystem
{
    public enum ExpandMode
    {
        /// <summary>
        /// Do nothing, use default expand rules configured on API endpoint.
        /// </summary>
        Default,

        /// <summary>
        /// Full expand.
        /// For properties pointing to a single resource, it means expand that resource.
        /// For properties having a list of resources this means expand the list itself and every item.
        /// </summary>
        Full,

        /// <summary>
        /// Expands as list of references to resources. Only applicable to properties having a collection of resources.
        /// </summary>
        Shallow
    }
}

