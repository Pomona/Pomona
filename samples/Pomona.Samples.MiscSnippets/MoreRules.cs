#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using Pomona.FluentMapping;

namespace Pomona.Samples.MiscSnippets
{
    public class MoreRules
    {
        #region sample

        // SAMPLE: misc-constructed-using-fluent-rule
        public void ConstructedUsingRule(ITypeMappingConfigurator<Customer> customer)
        {
            customer.ConstructedUsing(c => new Customer(c.Requires().Name, c.Optional().Password));
        }
        // ENDSAMPLE

        #endregion

        #region sample

        // SAMPLE: misc-contextful-construction-fluent-rule
        public void ContextfulConstructionRule(ITypeMappingConfigurator<Customer> customer)
        {
            customer.ConstructedUsing(
                c => c.Context<ICustomerFactory>()
                      .CreateCustomer(c.Requires().Name, c.Optional().Password));
        }
        // ENDSAMPLE

        #endregion

        #region sample

        // SAMPLE: misc-exclude-property-fluent-rule
        public void ExcludeRule(ITypeMappingConfigurator<Customer> customer)
        {
            customer.Exclude(x => x.Password);
        }
        // ENDSAMPLE

        #endregion

        #region sample

        // SAMPLE: misc-handled-by-fluent-rule
        public void HandlerRule(ITypeMappingConfigurator<Customer> customer)
        {
            customer.HandledBy<CustomerHandler>();
        }
        // ENDSAMPLE

        #endregion

        #region sample

        // SAMPLE: misc-plural-name-fluent-rule
        public void PluralNameRule(ITypeMappingConfigurator<Mouse> mouse)
        {
            mouse.WithPluralName("Mice");
        }
        // ENDSAMPLE

        #endregion

        #region sample

        // SAMPLE: misc-value-object-fluent-rule
        public void ValueObjectRule(ITypeMappingConfigurator<Customer> customer)
        {
            customer.AsValueObject();
        }
        // ENDSAMPLE

        #endregion

        #region sample

        // SAMPLE: misc-include-property-named
        public void IncludePropertyNamed(ITypeMappingConfigurator<Customer> customer)
        {
            customer.Include(x => x.Identifier, x => x.Named("Id"));
        }
        // ENDSAMPLE

        #endregion

        #region sample

        // SAMPLE: misc-include-property-onget-onset-onquery
        public void IncludePropertyWithCustomAccessors(ITypeMappingConfigurator<Customer> customer)
        {
            customer
                .Include(x => x.Name, x => x
                    .OnGet(y => y.Name.ToUpper())
                    .OnSet((c, v) => c.Name = v.ToLower())
                    .OnQuery(y => y.Name.ToUpper()));
        }
        // ENDSAMPLE

        #endregion
    }
}