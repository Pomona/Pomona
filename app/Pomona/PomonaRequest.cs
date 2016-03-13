#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Nancy;

using Pomona.Common;
using Pomona.Common.Internals;

namespace Pomona
{
    public class PomonaRequest
    {
        public PomonaRequest(string url,
                             string relativePath,
                             HttpMethod method = HttpMethod.Get,
                             IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = null,
                             Stream body = null,
                             DynamicDictionary query = null)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            Method = method;
            Url = url;
            RelativePath = relativePath;
            Body = body;
            Headers = headers.EmptyIfNull().ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            Query = query ?? new DynamicDictionary();
        }


        public Stream Body { get; internal set; }

        public IDictionary<string, IEnumerable<string>> Headers { get; }

        public HttpMethod Method { get; }

        public DynamicDictionary Query { get; }

        public string RelativePath { get; }

        public string Url { get; }
    }
}