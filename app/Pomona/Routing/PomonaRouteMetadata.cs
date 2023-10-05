#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using Pomona.Common;

namespace Pomona.Routing
{
    /// <summary>
    /// Metadata object for routes defined by Pomona.
    /// </summary>
    public class PomonaRouteMetadata
    {
        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        /// <value>
        /// The type of the content.
        /// </value>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the relation.
        /// </summary>
        /// <value>
        /// The relation.
        /// </value>
        public string Relation { get; set; }
    }
}
