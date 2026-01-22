using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SageWebAPI.Models;

namespace SageWebAPI.Services
{
    public interface ICustomerService
    {
       
        //IEnumerable<Models.Customer> FindCustomer(customerFilterParam param);
        IEnumerable<Models.Customer> FindCustomerNew(customerFilterParam param);
        //IEnumerable<Models.Customer> GetCustomerID(customerFilterParam model);
        //Models.CustomerDetails GetCustomerDetails(string CustomerNo);

        //Models.CustomerDetails GetCustomer(string CustomerNo);
        CustomerCreate Create(CustomerCreate model);
        ContactCreate CreateContact(ContactCreate model);
        ContactCreate UpdateContact(ContactCreate model);
        Models.Customer GetCustomerDetails(string CustomerNo);
        string CheckCustomerExists(string CustomerNo);
        int CheckCustomerContactExists(string CustomerNo, string ContactCode);
        CustomerCreate UpdateAddress(CustomerCreate model);
        int getContactCount(string custID);
        ContactLinkDelete DeleteContactLink(ContactLinkDelete model);

        
    }
}
