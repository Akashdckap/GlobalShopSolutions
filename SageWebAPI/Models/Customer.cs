using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalSolutions.Models
{
    public class Customer
    {
        public string ARDivisionNo { get; set; }
        public string CustomerNo { get; set; }
        public string CustomerName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string CountryCode { get; set; }
        public string TelephoneNo { get; set; }
        public string EmailAddress { get; set; }
        public string TermsCode { get; set; }
        public string ContactCode { get; set; }
        public string PrimaryShipToCode { get; set; }
        public string ShipMethod { get; set; }
        public string TaxSchedule { get; set; }
        public string URLAddress { get; set; }
        public string SalespersonNo { get; set; }
        public string UDF_DEALERLEVEL { get; set; }
        public string UDF_TERRITORY { get; set; }
        public string UDF_BUYING_GROUP { get; set; }
        public string UDF_PERSONA { get; set; }
        public string UDF_SAMSUNGPLATINUM { get; set; }
        public string UDF_LGPINNACLENUMBER { get; set; }
        public string UDF_SONYDIAMOND { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string TimeUpdated { get; set; }
        public int? TotalRecords { get; set; }
        public Salesrep salesreps { get; set; } = new Salesrep();
        public List<Contacts> contacts { get; set; } = new List<Contacts>();
        public List<ShipTo> shiptos { get; set; } = new List<ShipTo>();
        


    }
    public class Salesrep
    {
        public string SalespersonNo { get; set; }
        public string SalespersonName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string EmailAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string TimeUpdated { get; set; }
    }
    public class Contacts
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
        public DateTime? DateUpdated { get; set; }
        public string TimeUpdated { get; set; }

    }
    public class ShipTo
    {
        public string ARDivisionNo { get; set; }
        public string CustomerNo { get; set; }
        public string ShipToCode { get; set; }
        public string ShipToName { get; set; }
        public string ShipToAddress1 { get; set; }
        public string ShipToAddress2 { get; set; }
        public string ShipToAddress3 { get; set; }
        public string ShipToCity { get; set; }

        public string ShipToState { get; set; }
        public string ShipToZipCode { get; set; }
        public string WarehouseCode { get; set; }
        public string TelephoneNo { get; set; }
        public string EmailAddress { get; set; }
        public string ShipVia { get; set; }
        public string ContactCode { get; set; }
        public string SalespersonNo { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string TimeUpdated { get; set; }


    }

    public class CustomerFilterParam
    {
        public DateTime DateUpdated { get; set; }
        public int? CurrentIndex { get; set; }
        public int? PageSize { get; set; }
    }



    public class CheckForCustomer
    {
        public string CustomerNo { get; set; }
        public string message { get; set; }
    }

    public class CustomerContactCheck
    {
        public string Status { get; set; }
       
        public string message { get; set; } 


    }

    public class CustomerContactCheckParam
    {
        public string CustomerNo { get; set; }
        public string ContactCode { get; set; }



    }
    public class Inputparam
    {
        [Required]
        public int? page_size { get; set; }
        [Required]
        public int? page_no { get; set; }
    }
    public class Inputrequest
    {
        [Required]
        public string table_Name { get; set; }
        [Required]
        public string jsonreq { get; set; }
    }

    public class Recorddate
    {
        [Required]
        public DateTime? date_last_modified { get; set; }
      
    }

    public class CustomerDetails : Customer
    {
        public List<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();
    }
    public class CustomerContact
    {
        public string ContactCode { get; set; }
        public string ContactName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; } = string.Empty;
        public string TelephoneNo1 { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
    }
}
