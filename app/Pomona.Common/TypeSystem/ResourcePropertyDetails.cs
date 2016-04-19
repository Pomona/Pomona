#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Common.TypeSystem
{
    public class ResourcePropertyDetails
    {
        public ResourcePropertyDetails(bool exposedAsRepository, string uriName)
        {
            ExposedAsRepository = exposedAsRepository;
            UriName = uriName;
        }


        public bool ExposedAsRepository { get; }

        public string UriName { get; }
    }
}