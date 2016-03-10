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
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Pomona.Common.Web
{
    public static class HttpMessageExtensions
    {
        public static async Task<HttpRequestMessage> Clone(this HttpRequestMessage source)
        {
            if (source == null)
                return null;

            var destination = new HttpRequestMessage(source.Method, source.RequestUri);
            destination.Version = source.Version;
            CopyHeadersFrom(destination.Headers, source.Headers);
            destination.Content = await Clone(source.Content);
            return destination;
        }


        public static async Task<HttpContent> Clone(this HttpContent source)
        {
            if (source == null)
                return null;

            await source.LoadIntoBufferAsync();
            var destination = new ByteArrayContent(await source.ReadAsByteArrayAsync());
            CopyHeadersFrom(destination.Headers, source.Headers);
            return destination;
        }


        public static async Task<HttpResponseMessage> Clone(this HttpResponseMessage source, bool cloneReferencedRequestMessage = false)
        {
            var destination = new HttpResponseMessage(source.StatusCode)
            {
                ReasonPhrase = source.ReasonPhrase,
                Version = source.Version
            };

            CopyHeadersFrom(destination.Headers, source.Headers);
            destination.Content = await Clone(source.Content);

            if (cloneReferencedRequestMessage)
                destination.RequestMessage = await Clone(source.RequestMessage);

            return destination;
        }


        public static string ToStringWithContent(this HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            return request.ToStringWithContentAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }


        public static string ToStringWithContent(this HttpResponseMessage response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));
            return response.ToStringWithContentAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }


        public static async Task<string> ToStringWithContentAsync(this HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            StringWriter sw = new StringWriter();
            sw.WriteLine("{0} {1} HTTP/{2}", request.Method, request.RequestUri.PathAndQuery, request.Version);
            if (!request.Headers.Contains("Host"))
                sw.WriteLine("Host: {0}", request.RequestUri.Authority);
            WriteHeaders(sw, request.Headers);
            await WriteContentHeadersAndBody(sw, request.Content);
            return sw.ToString();
        }


        public static async Task<string> ToStringWithContentAsync(this HttpResponseMessage response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            StringWriter sw = new StringWriter();
            sw.WriteLine("HTTP/{0} {1} {2}", response.Version, (int)response.StatusCode, response.ReasonPhrase);
            WriteHeaders(sw, response.Headers);
            await WriteContentHeadersAndBody(sw, response.Content);
            return sw.ToString();
        }


        public static void CopyHeadersFrom(this HttpHeaders destination, HttpHeaders source)
        {
            foreach (var header in source)
                destination.Add(header.Key, header.Value);
        }


        private static async Task WriteContentHeadersAndBody(StringWriter sw, HttpContent content)
        {
            if (content != null)
            {
                await content.LoadIntoBufferAsync();
                WriteHeaders(sw, content.Headers);
                sw.WriteLine();
                sw.Write(await content.ReadAsStringAsync());
                sw.WriteLine();
            }
            else
                sw.WriteLine();
        }


        private static void WriteHeaders(StringWriter sw, HttpHeaders headers)
        {
            foreach (var header in headers)
                sw.WriteLine("{0}: {1}", header.Key, string.Join(",", header.Value));
        }
    }
}