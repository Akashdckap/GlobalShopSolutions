using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalSolutions.Models
{
    public class CustomerCreate
    {

        public string ARDivisionNo { get; set; }
        public string CustomerNo { get; set; }
        public string CustomerName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string CountryCode { get; set; }
        public string TelephoneNo { get; set; }
        public string EmailAddress { get; set; }
        public string TermsCode { get; set; }
        public string SalespersonDivisionNo { get; set; }
        public string SalespersonNo { get; set; }
        public string DefaultPaymentType { get; set; }
        public string MagentoCustomerGroup { get; set; }

    }

    public class AddressUpdate
    {
        public string ARDivisionNo { get; set; }
        public string CustomerNo { get; set; }
        public string CustomerName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string CountryCode { get; set; }
        public string TelephoneNo { get; set; }
        public string EmailAddress { get; set; }

    }
}
