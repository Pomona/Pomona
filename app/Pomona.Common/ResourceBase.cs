#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Common
{
    public class ResourceBase : IHasSettableResourceUri
    {
        public string Uri
        {
            get { return ((IHasSettableResourceUri)this).Uri; }
        }

        string IHasSettableResourceUri.Uri { get; set; }
    }
}