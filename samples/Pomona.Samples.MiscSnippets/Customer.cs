#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Samples.MiscSnippets
{
    public class Customer
    {
        public Customer(string name, string password)
        {
        }


        public Address Address { get; set; }

        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }
}