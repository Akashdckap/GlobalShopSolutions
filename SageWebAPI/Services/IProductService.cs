using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalSolutions.Models;

namespace GlobalSolutions.Services
{
    public interface IProductService
    {
        string GetShipmentDetails(int page_size, int page_no);
        string GetShipMentLines(int page_size, int page_no);
        string GetShipMentLinesById(string order_no);
        string GetRecordStatus(DateTime date_last_modified);
        int CreateRecord(string table_name, string jsonreq);
        string GetContact(int page_size, int page_no);
        string GetCustomers(int page_size, int page_no);
        string ProductTest(int page_size, int page_no);
    }
}
