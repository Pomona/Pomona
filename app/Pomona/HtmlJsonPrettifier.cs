#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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

