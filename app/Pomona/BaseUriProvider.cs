#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Nancy;

namespace Pomona
{
    public class BaseUriProvider : IBaseUriProvider
    {
        private readonly NancyContext context;
        private readonly string pomonaroot;


        public BaseUriProvider(NancyContext context, string pomonaRoot)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            this.context = context;
            this.pomonaroot = pomonaRoot;
        }


        public Uri BaseUri
        {
            get
            {
                var request = this.context.Request;
                var appUrl = request.Url.BasePath ?? string.Empty;
                var uriString = String.Format("{0}://{1}:{2}{3}{4}",
                                              request.Url.Scheme,
                                              request.Url.HostName,
                                              request.Url.Port,
                                              appUrl, this.pomonaroot);

                return new Uri(uriString);
            }
        }
    }
}