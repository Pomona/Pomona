#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;

using Pomona.Common.Internals;
using Pomona.Common.Loading;

namespace Pomona.Common
{
    public class RequestOptions : IRequestOptions
    {
        private readonly StringBuilder expandedPaths;
        private readonly List<Action<HttpRequestMessage>> requestModifyActions;


        public RequestOptions()
        {
            this.expandedPaths = new StringBuilder();
            this.requestModifyActions = new List<Action<HttpRequestMessage>>();
        }


        public RequestOptions(RequestOptions clonedOptions)
        {
            if (clonedOptions == null)
                throw new ArgumentNullException(nameof(clonedOptions));
            ExpectedResponseType = clonedOptions.ExpectedResponseType;
            this.expandedPaths = new StringBuilder(clonedOptions.expandedPaths.ToString());
            this.requestModifyActions = new List<Action<HttpRequestMessage>>(clonedOptions.requestModifyActions);
            ResourceLoader = clonedOptions.ResourceLoader;
        }


        internal RequestOptions(Type expectedResponseType = null)
            : this()
        {
            ExpectedResponseType = expectedResponseType;
        }


        public string ExpandedPaths => this.expandedPaths.ToString();

        public Type ExpectedResponseType { get; set; }
        internal IResourceLoader ResourceLoader { get; set; }


        public void ApplyRequestModifications(HttpRequestMessage request)
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


        public IRequestOptions ModifyRequest(Action<HttpRequestMessage> action)
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


        IRequestOptions<T> IRequestOptions<T>.ModifyRequest(Action<HttpRequestMessage> action)
        {
            ModifyRequest(action);
            return this;
        }
    }
}