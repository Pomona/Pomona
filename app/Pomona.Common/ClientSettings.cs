#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using Pomona.Common.Loading;

namespace Pomona.Common
{
    public class ClientSettings
    {
        public ClientSettings()
        {
            LazyMode = LazyMode.Enabled;
        }


        public LazyMode LazyMode { get; set; }
    }
}
