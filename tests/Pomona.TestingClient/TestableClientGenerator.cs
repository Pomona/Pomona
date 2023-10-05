#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using Pomona.Common;
using Pomona.Common.Proxies;

namespace Pomona.TestingClient
{
    public class TestableClientGenerator
    {
        public static TClient CreateClient<TClient>()
            where TClient : IPomonaClient
        {
            return RuntimeProxyFactory<TestableClientProxyBase, TClient>.Create();
        }


        public static TClient CreateClient<TClient, TTestableClientProxy>()
            where TClient : IPomonaClient
            where TTestableClientProxy : TestableClientProxyBase
        {
            return RuntimeProxyFactory<TTestableClientProxy, TClient>.Create();
        }
    }
}

