#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Collections.Generic;
using System.IO;

using Nancy;

using Pomona.Common;

namespace Pomona
{
    public class PomonaRequest
    {
        private RequestHeaders headers;


        public PomonaRequest(string url,
                             string relativePath,
                             HttpMethod method = HttpMethod.Get,
                             RequestHeaders headers = null,
                             Stream body = null,
                             DynamicDictionary query = null)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            Method = method;
            Url = url;
            RelativePath = relativePath;
            Body = body;
            this.headers = headers;
            Query = query ?? new DynamicDictionary();
        }


        public Stream Body { get; internal set; }

        public RequestHeaders Headers => this.headers ?? (this.headers = new RequestHeaders(new Dictionary<string, IEnumerable<string>>()));

        public HttpMethod Method { get; }

        public DynamicDictionary Query { get; internal set; }

        public string RelativePath { get; }

        public string Url { get; }
    }
}

