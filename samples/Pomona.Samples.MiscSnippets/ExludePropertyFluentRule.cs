#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.FluentMapping;

namespace Pomona.Samples.MiscSnippets
{
    public class MoreRules
    {
        // SAMPLE: misc-exlude-property-fluent-rule
        public void ExcludeRule(ITypeMappingConfigurator<Customer> map)
        {
            map.Exclude(x => x.Password);
        } // ENDSAMPLE

        // SAMPLE: misc-handled-by-fluent-rule
        public void HandlerRule(ITypeMappingConfigurator<Customer> map)
        {
            map.HandledBy<CustomerHandler>();
        } // ENDSAMPLE
    }
}