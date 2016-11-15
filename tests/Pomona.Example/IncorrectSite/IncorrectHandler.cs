#region License
// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/
#endregion

using System.Linq;

namespace Pomona.Example.IncorrectSite
{
    public class IncorrectHandler
    {
        public IQueryable<IncorrectResource> Query()
        {
            return new[]
            {
                new IncorrectResource() { Id = 1 },
                new IncorrectResource() { Id = 2 },
                new IncorrectResource() { Id = 3 }
            }.AsQueryable();
        }
    }
}