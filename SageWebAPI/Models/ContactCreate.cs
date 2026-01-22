using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalSolutions.Models
{
    public class ContactCreate
    {
        public string ARDivisionNo { get; set; }
        public string CustomerNo { get; set; }        
        public string ContactCode { get; set; }
        public string ContactName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string TelephoneNo1 { get; set; }
        public string EmailAddress { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class ContactLinkDelete
    {
        public string ARDivisionNo { get; set; }
        public string CustomerNo { get; set; }
        public string ContactCode { get; set; }

        public string Status { get; set; }
        public string Message { get; set; }

    }

    public class ContactLinkDeleteStatus
    {
       
        public string Status { get; set; }
        public string Message { get; set; }

    }


}
