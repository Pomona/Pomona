#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Security.Authentication
{
    public class DefaultPreAuthenticatedUriResolver : IPreAuthenticatedUriResolver
    {
        private readonly IPreAuthenticatedUriProvider authenticatedUrlHelper;
        private readonly IUriResolver uriResolver;


        public DefaultPreAuthenticatedUriResolver(IUriResolver uriResolver,
                                                  IPreAuthenticatedUriProvider authenticatedUrlHelper)
        {
            if (uriResolver == null)
                throw new ArgumentNullException(nameof(uriResolver));
            if (authenticatedUrlHelper == null)
                throw new ArgumentNullException(nameof(authenticatedUrlHelper));
            this.uriResolver = uriResolver;
            this.authenticatedUrlHelper = authenticatedUrlHelper;
        }


        public string GetPreAuthenticatedUriFor(object entity, DateTime? expiration = null)
        {
            return this.authenticatedUrlHelper.CreatePreAuthenticatedUrl(this.uriResolver.GetUriFor(entity), expiration);
        }
    }
}