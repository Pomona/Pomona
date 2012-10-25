#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;

using Nancy.Helpers;

using Newtonsoft.Json;

namespace Pomona.Queries
{
    public class QueryResultJsonConverter<T>
    {
        private readonly PomonaSession session;


        public QueryResultJsonConverter(PomonaSession session)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            this.session = session;
        }


        public void ToJson(PomonaQuery query, QueryResult<T> queryResult, TextWriter writer)
        {
            //var elementType = query.TargetType;
            var mappedType = this.session.TypeMapper.GetClassMapping(typeof(IList<T>));
            var rootPath = mappedType.GenericArguments.First().Name.ToLower(); // We want paths to be case insensitive
            var expand = query.ExpandedPaths.Aggregate(string.Empty, (a, b) => a + "," + b);
            var context = new FetchContext(string.Format("{0},{1}", rootPath, expand), false, this.session);
            var wrapper = new ObjectWrapper(queryResult, string.Empty, context, mappedType);

            var jsonWriter = new JsonTextWriter(writer) { Formatting = Formatting.Indented };
            jsonWriter.WriteStartObject();

            jsonWriter.WritePropertyName("_type");
            jsonWriter.WriteValue("__result__");

            jsonWriter.WritePropertyName("totalCount");
            jsonWriter.WriteValue(queryResult.TotalCount);

            jsonWriter.WritePropertyName("count");
            jsonWriter.WriteValue(queryResult.Count);

            Uri previousPageUri;
            if (TryGetPage(query, queryResult, -1, out previousPageUri))
            {
                jsonWriter.WritePropertyName("previous");
                jsonWriter.WriteValue(previousPageUri.ToString());
            }

            Uri nextPageUri;
            if (TryGetPage(query, queryResult, 1, out nextPageUri))
            {
                jsonWriter.WritePropertyName("next");
                jsonWriter.WriteValue(nextPageUri.ToString());
            }

            jsonWriter.WritePropertyName("items");
            wrapper.ToJson(jsonWriter);

            jsonWriter.WriteEndObject();
            jsonWriter.Flush();
        }


        private bool TryGetPage(PomonaQuery query, QueryResult<T> queryResult, int offset, out Uri pageUri)
        {
            var newSkip = Math.Max(query.Skip + (query.Top * offset), 0);
            var uriBuilder = new UriBuilder(query.Url);

            if (query.Skip == newSkip || newSkip >= queryResult.TotalCount)
            {
                pageUri = null;
                return false;
            }

            NameValueCollection parameters;
            if (!string.IsNullOrEmpty(uriBuilder.Query))
            {
                parameters = HttpUtility.ParseQueryString(uriBuilder.Query);
                parameters["skip"] = newSkip.ToString(CultureInfo.InvariantCulture);
                uriBuilder.Query = parameters.ToString();
            }
            else
                uriBuilder.Query = "skip=" + newSkip;

            pageUri = uriBuilder.Uri;

            return true;
        }
    }
}