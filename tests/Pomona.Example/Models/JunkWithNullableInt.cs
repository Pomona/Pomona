#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Example.Models
{
    public class JunkWithNullableInt : EntityBase
    {
        public int? Maybe { get; set; }
        public string MentalState { get; set; }
    }
}