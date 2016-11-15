#region License
// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/
#endregion

using System.Collections.Generic;

namespace Pomona.Example.IncorrectSite
{
    public class IncorrectResource
    {
        public int Id { get; set; }
        public IList<IncorrectChildResource> Children { get; set; }
    }
}