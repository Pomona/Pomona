#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using System.Text;

using Nancy;
using Nancy.Helpers;

namespace Pomona
{
    /// <summary>
    /// This was just hacked together as fast as possible.
    /// Just something simple to get more HTML friendly..
    /// </summary>
    internal static class HtmlJsonPrettifier
    {
        public static void CreatePrettifiedHtmlJsonResponse(Response response, string htmlHeaderLinks, string json, string baseUri)
        {
            const string htmlPageTemplate = @"<!DOCTYPE HTML>
<head>
  <meta charset='utf-8'>
  {0}
  <style type=""text/css"">
ul
{{
list-style-type: none;
padding: 0px;
margin: 0px;
}}
.search
{{
width: 450px;
}}
ul li
{{
padding-left: 14px; 
display: inline;
}}
</style>
</head>
<html>
<div>
<pre class=""prettyprint"">
{1}
</pre>
</div>
</body>
</html>";
            var sb = new StringBuilder();
            var jsonIndex = 0;

            while (jsonIndex < json.Length)
            {
                var linkStart = json.IndexOf("\"http://", jsonIndex, StringComparison.Ordinal);
                if (linkStart == -1)
                    linkStart = json.IndexOf("\"https://", jsonIndex, StringComparison.Ordinal);

                if (linkStart != -1)
                {
                    linkStart++; // Skip the quote (")
                    sb.Append(HttpUtility.HtmlEncode(json.Substring(jsonIndex, linkStart - jsonIndex)));

                    var linkEnd = json.IndexOf('"', linkStart);
                    linkEnd = linkEnd != -1 ? linkEnd : json.Length;

                    var link = json.Substring(linkStart, linkEnd - linkStart);
                    sb.AppendFormat("<a href=\"{0}\">{1}</a>", link, HttpUtility.HtmlEncode(link));

                    jsonIndex = linkEnd;
                }
                else
                {
                    sb.Append(HttpUtility.HtmlEncode(json.Substring(jsonIndex)));
                    jsonIndex = json.Length;
                }
            }

            var prettifiedJsonHtml = String.Format(htmlPageTemplate, htmlHeaderLinks, sb, baseUri);
            response.ContentsFromString(prettifiedJsonHtml);
            response.ContentType = "text/html; charset=utf-8";
        }
    }
}