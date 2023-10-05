#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;
using System.Runtime.Serialization;

using Nancy;

namespace Pomona
{
    [Serializable]
    public class ReferencedResourceNotFoundException : PomonaServerException
    {
        [NonSerialized]
        private readonly PomonaResponse innerResponse;


        public ReferencedResourceNotFoundException(string resourceUrl, PomonaResponse innerResponse)
            : base("Unable to locate referenced resource at " + resourceUrl, null, HttpStatusCode.BadRequest)
        {
            ResourceUrl = resourceUrl;
            this.innerResponse = innerResponse;
        }


        protected ReferencedResourceNotFoundException(SerializationInfo info,
                                                      StreamingContext context)
            : base(info, context)
        {
        }


        public PomonaResponse InnerResponse => this.innerResponse;

        public string ResourceUrl { get; }
    }
}

