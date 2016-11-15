#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.FluentMapping;

namespace Pomona.Samples.MiscSnippets
{
    // SAMPLE: misc-fluent-rules-example
    public class FluentRules
    {
        public void Map(ITypeMappingConfigurator<Customer> map)
        {
            map.Exclude(x => x.Password);
            map.Include(x => x.Name);
            map.Include(x => x.Identifier, o => o.AsPrimaryKey());
        }
    }
    // ENDSAMPLE
}