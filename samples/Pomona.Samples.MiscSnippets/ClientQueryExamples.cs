#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Linq;

using Critters.Client;

namespace Pomona.Samples.MiscSnippets
{
    public class ClientQueryExamples
    {
        public void Samples(ICritterClient client)
        {
            // TODO: It's probably best to use a single domain model across the entire documentation.
            var weapons =
// SAMPLE: client-linq-simple-query-structure
client.Weapons
      .Query()
      .Where(x => x.Price > 100.0m /* any number of where expressions */)
      .Select(x => x.Model /* any number of select expressions */)
      .Where(x => x.Name.StartsWith("NERF") /* where and select expressions can be mixed */)
      .OrderBy(x => x.Name /* ordering must come after where and select */)
      .Skip(50 /* skip after order by */)
      .Take(50 /* take after skip */)
      .ToList();
// ENDSAMPLE
// SAMPLE: client-linq-aggregate-query-structure
client.Weapons
        .Query()
        .Where(x => x.Price > 100.0m /* any number of where expressions */)
        .GroupBy(x => x.Model.Name /* group by after where */)
        .Select(x => new { modelName = x.Key, totalPrice = x.Sum(y => y.Price) })
        .OrderBy(x => x.totalPrice)
        .ToList();
// ENDSAMPLE
        }
    }
}

