#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

namespace Pomona.UnitTests.TestResources
{
    public class TestResource : ITestResource
    {
        public IDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

        public IList<ITestResource> Children { get; } = new List<ITestResource>();

        public IDictionary<string, string> Dictionary { get; } = new Dictionary<string, string>();

        public ITestResource Friend { get; set; }
        public int Id { get; set; }
        public string Info { get; set; }
        public ISet<ITestResource> Set { get; } = new HashSet<ITestResource>();
        public ITestResource Spouse { get; set; }
    }
}

