#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

namespace Pomona.Common
{
    public class PomonaResourceInfo
    {
        public Type InterfaceType { get; set; }
        public Type LaxyProxyType { get; set; }
        public Type PatchFormType { get; set; }
        public Type PocoType { get; set; }
        public Type PostFormType { get; set; }
    }
}

