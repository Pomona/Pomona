#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Nancy;

using Pomona.Common;

namespace Pomona.Nancy
{
    internal class PomonaEngine
    {
        private readonly IPomonaSession session;


        public PomonaEngine(IPomonaSession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            this.session = session;
        }


        public PomonaResponse Handle(NancyContext context, string modulePath)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (modulePath == null)
                throw new ArgumentNullException(nameof(modulePath));

            HttpMethod httpMethod =
                (HttpMethod)Enum.Parse(typeof(HttpMethod), context.Request.Method, true);

            var moduleRelativePath = context.Request.Path.Substring(modulePath.Length);
            var request = new PomonaRequest(context.Request.Url.ToString(), moduleRelativePath, httpMethod,
                                            context.Request.Headers, context.Request.Body,
                                            context.Request.Query);

            return this.session.Dispatch(request);
        }
    }
}