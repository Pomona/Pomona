#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

namespace Pomona.Example.Models
{
    public class ThingWithNullableDateTime : EntityBase
    {
        public DateTime? MaybeDateTime { get; set; }
    }
}
