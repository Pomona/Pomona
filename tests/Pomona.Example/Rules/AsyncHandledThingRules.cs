#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.Example.Models;
using Pomona.FluentMapping;

namespace Pomona.Example.Rules
{
    public class AsyncHandledThingRules
    {
        public void Map(ITypeMappingConfigurator<HandledThing> map)
        {
            map
                .HasChildren(x => x.Children,
                             x => x.Parent,
                             t =>
                             {
                                 return
                                     t.ConstructedUsing(c => new HandledChild(c.Parent<HandledThing>()))
                                      .HandledBy<AsyncHandledThingsHandler>();
                             },
                             o => o.ExposedAsRepository())
                .Include(x => x.ETag, o => o.AsEtag())
                .AsUriBaseType()
                .DeleteAllowed().HandledBy<AsyncHandledThingsHandler>();
        }
    }
}