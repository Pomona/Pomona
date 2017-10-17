﻿#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Nancy;

namespace Pomona.Nancy
{
    public class BaseUriProvider : IBaseUriProvider
    {
        private readonly NancyContext context;
        private readonly string modulePath;


        public BaseUriProvider(NancyContext context, string modulePath)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            this.context = context;
            this.modulePath = modulePath;
        }


        public Uri BaseUri
        {
            get
            {
                var request = this.context.Request;
                var appUrl = request.Url.BasePath ?? string.Empty;
                var uriString = $"{request.Url.Scheme}://{request.Url.HostName}:{request.Url.Port}{appUrl}{this.modulePath}";

                return new Uri(uriString);
            }
        }
    }
}