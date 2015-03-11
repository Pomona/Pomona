#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
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
using System.Linq.Expressions;
using System.Text;

using Pomona.Common.Internals;
using Pomona.Common.Loading;
using Pomona.Common.Web;

namespace Pomona.Common
{
    public class RequestOptions : IRequestOptions
    {
        private readonly StringBuilder expandedPaths;
        private readonly List<Action<WebClientRequestMessage>> requestModifyActions;


        public RequestOptions()
        {
            this.expandedPaths = new StringBuilder();
            this.requestModifyActions = new List<Action<WebClientRequestMessage>>();
        }


        public RequestOptions(RequestOptions clonedOptions)
        {
            if (clonedOptions == null)
                throw new ArgumentNullException("clonedOptions");
            ExpectedResponseType = clonedOptions.ExpectedResponseType;
            this.expandedPaths = new StringBuilder(clonedOptions.expandedPaths.ToString());
            this.requestModifyActions = new List<Action<WebClientRequestMessage>>(clonedOptions.requestModifyActions);
            this.ResourceLoader = clonedOptions.ResourceLoader;
        }


        internal RequestOptions(Type expectedResponseType = null)
            : this()
        {
            ExpectedResponseType = expectedResponseType;
        }


        public string ExpandedPaths
        {
            get { return this.expandedPaths.ToString(); }
        }

        public Type ExpectedResponseType { get; set; }
        internal IResourceLoader ResourceLoader { get; set; }


        public void ApplyRequestModifications(WebClientRequestMessage request)
        {
            foreach (var action in this.requestModifyActions)
                action(request);
            if (!string.IsNullOrEmpty(ExpandedPaths))
                request.Headers.Add("X-Pomona-Expand", ExpandedPaths);
        }


        public static RequestOptions Create<T>(Action<IRequestOptions<T>> optionActions,
                                               Type expectedResponseType = null)
        {
            var requestOptions = new RequestOptions<T>(expectedResponseType);
            if (optionActions != null)
                optionActions(requestOptions);
            return requestOptions;
        }


        public override string ToString()
        {
            var toString = new StringBuilder("{");

            if (ExpectedResponseType != null)
                toString.AppendFormat(" Type: {0}, ", ExpectedResponseType);

            var paths = (ExpandedPaths ?? String.Empty).Trim();
            if (!String.IsNullOrEmpty(paths))
                toString.AppendFormat("Expand: {0}, ", paths);

            toString.Append("}");

            return toString.ToString();
        }


        protected void Expand(LambdaExpression expression)
        {
            if (this.expandedPaths.Length > 0)
                this.expandedPaths.Append(',');
            this.expandedPaths.Append(expression.GetPropertyPath(true));
        }


        public IRequestOptions ModifyRequest(Action<WebClientRequestMessage> action)
        {
            this.requestModifyActions.Add(action);
            return this;
        }
    }

    internal class RequestOptions<T> : RequestOptions, IRequestOptions<T>
    {
        internal RequestOptions(Type expectedResponseType = null)
            : base(expectedResponseType)
        {
        }


        IRequestOptions<T> IRequestOptions<T>.Expand<TRetValue>(Expression<Func<T, TRetValue>> expression)
        {
            Expand(expression);
            return this;
        }


        IRequestOptions<T> IRequestOptions<T>.ModifyRequest(Action<WebClientRequestMessage> action)
        {
            ModifyRequest(action);
            return this;
        }
    }
}