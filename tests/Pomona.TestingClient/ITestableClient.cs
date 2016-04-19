#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.TestingClient
{
    public interface ITestableClient
    {
        void SetupPostHandler<TResource>(
            Func<TResource, object> func);
    }
}