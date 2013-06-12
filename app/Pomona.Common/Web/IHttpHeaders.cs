using System.Collections.Generic;

namespace Pomona.Common.Web
{
    public interface IHttpHeaders : IDictionary<string, IList<string>>
    {
        void Add(string key, string value);
        string GetFirst(string key, string value);
        IEnumerable<string> GetValues(string key);
    }
}