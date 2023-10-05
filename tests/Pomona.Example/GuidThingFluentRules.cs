#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using Pomona.Example.Models;
using Pomona.FluentMapping;

namespace Pomona.Example
{
    public class GuidThingFluentRules
    {
        public void Map(ITypeMappingConfigurator<GuidThing> map)
        {
            map.HandledBy<GuidThingHandler>().ConstructedUsing(c => new GuidThing(null));
        }
    }
}
