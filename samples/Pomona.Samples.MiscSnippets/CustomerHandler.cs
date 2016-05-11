#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Samples.MiscSnippets
{
    public class CustomerHandler
    {
        private dynamic customerRepo;

        #region

        // SAMPLE: misc-get-customer-handler-method
        public Customer GetCustomer(int id)
        {
            return this.customerRepo.GetById(id);
        }
        // ENDSAMPLE

        #endregion

        #region

        // SAMPLE: misc-post-customer-handler-method
        public Customer Post(Customer customer)
        {
            this.customerRepo.Save(customer);
            return customer;
        }
        // ENDSAMPLE

        #endregion
        #region

        // SAMPLE: misc-patch-customer-handler-method
        public Customer Patch(Customer customer)
        {
            this.customerRepo.Save(customer);
            return customer;
        }
        // ENDSAMPLE

        #endregion
    }
}