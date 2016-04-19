#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Example.Models
{
    /// <summary>
    /// This is a value object, an object with no identity.
    /// It should probably be immutable.
    /// </summary>
    public class CrazyValueObject
    {
        public CrazyValueObject()
        {
            Info = "Yup, this is a value object. Look.. no _ref URI.";
        }


        public string Info { get; set; }
        public string Sickness { get; set; }
    }
}