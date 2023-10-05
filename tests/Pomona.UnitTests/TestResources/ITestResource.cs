#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Collections.Generic;

using Pomona.Common;

namespace Pomona.UnitTests.TestResources
{
    [AllowedMethods(HttpMethod.Get)]
    [ResourceInfo(InterfaceType = typeof(ITestResource), JsonTypeName = "TestResource",
        PocoType = typeof(TestResource), PostFormType = typeof(TestResourcePostForm),
        UriBaseType = typeof(ITestResource), UrlRelativePath = "test-resources")]
    public interface ITestResource : IClientResource
    {
        [ResourceAttributesProperty]
        IDictionary<string, object> Attributes { get; set; }

        IList<ITestResource> Children { get; }
        IDictionary<string, string> Dictionary { get; }
        ITestResource Friend { get; set; }

        [ResourceIdProperty]
        int Id { get; set; }

        string Info { get; set; }
        ISet<ITestResource> Set { get; }
        ITestResource Spouse { get; set; }
    }
}

