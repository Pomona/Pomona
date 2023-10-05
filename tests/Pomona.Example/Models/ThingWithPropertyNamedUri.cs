#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

namespace Pomona.Example.Models
{
    /// <summary>
    /// Test class for checking that IHasResourceUri.Url does not conflict with properties named Url on resources.
    /// </summary>
    public class ThingWithPropertyNamedUri : EntityBase
    {
        public Uri Uri { get; set; }
    }
}

