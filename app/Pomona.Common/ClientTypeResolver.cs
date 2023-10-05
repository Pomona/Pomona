#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

namespace Pomona.Common
{
    internal class ClientTypeResolver : IClientTypeResolver
    {
        private ClientTypeResolver()
        {
        }


        public static IClientTypeResolver Default { get; } = new ClientTypeResolver();


        public bool TryGetResourceInfoForType(Type type, out ResourceInfoAttribute resourceInfo)
        {
            return ResourceInfoAttribute.TryGet(type, out resourceInfo);
        }
    }
}

