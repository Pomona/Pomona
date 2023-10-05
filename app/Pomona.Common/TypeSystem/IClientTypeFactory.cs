#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

using Pomona.Common.Proxies;

namespace Pomona.Common.TypeSystem
{
    public interface IClientTypeFactory
    {
        object CreatePatchForm(Type resourceType, object original);
        IPostForm CreatePostForm(Type resourceType);
    }
}

