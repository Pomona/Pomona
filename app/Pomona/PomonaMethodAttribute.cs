#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

namespace Pomona
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PomonaMethodAttribute : Attribute
    {
        public PomonaMethodAttribute(string httpMethod)
        {
            HttpMethod = httpMethod;
        }


        public string HttpMethod { get; }

        public string UriName { get; set; }
    }
}
