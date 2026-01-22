using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SageWebAPI.Services;
using SageWebAPI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Newtonsoft.Json;

namespace SageWebAPI.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Produces("application/json")]
    [Route("api/Customers")]
    public class CustomerController : Controller
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly ICustomerService _customerService;
        private IConfiguration Configuration;

        public CustomerController(ICustomerService customerService, 
            ILoggerFactory loggerFactory, IConfiguration _configuration)
        {
            _logger = loggerFactory.CreateLogger<CustomerController>();
            _customerService = customerService;
             Configuration = _configuration;
        }

        ////[HttpPost]
        ////[Route("CreateCustomer")]
        ////[ProducesResponseType(200, Type = typeof(CustomerCreate))]
        ////public IActionResult Post([FromBody] CustomerCreate model)
        ////{
           
        ////    if (!ModelState.IsValid)
        ////        return BadRequest(ModelState);

        ////    //check if customer already exists in sage 
        ////        string customer_id = string.Empty;
        ////    if (!string.IsNullOrEmpty(model.CustomerNo))
        ////    {
        ////         customer_id = _customerService.CheckCustomerExists(model.CustomerNo);
        ////        _logger.LogDebug("customerid"+ customer_id);
        ////    }
          

        ////    if(!string.IsNullOrEmpty(customer_id))
        ////    {
        ////        _logger.LogDebug("go for update");
        ////        //if customer exists go for update customer address
        ////        var updaddress= _customerService.UpdateAddress(model);
        ////        if (updaddress != null)
        ////            return Ok(updaddress);
        ////        return BadRequest();

        ////    }
        ////    else
        ////    {
        ////        //go for create customer 
        ////        _logger.LogDebug("go for create");
        ////        var data = _customerService.Create(model);

        ////        if (data != null)
        ////            return Ok(data);
        ////        return BadRequest();
        ////    }


           
        ////}

        


        ////Find customer exists or not         
        //[HttpGet("CheckCustomerExists/{CustomerCode}")]

        //[ProducesResponseType(200, Type = typeof(CheckForCustomer))]
        //public IActionResult CheckCustomerExists(string CustomerCode)
        //{


        //    CheckForCustomer checkCust = new CheckForCustomer();
        //    var data = _customerService.CheckCustomerExists(CustomerCode);
        //    if (!string.IsNullOrEmpty(data))
        //    {
        //        checkCust.CustomerNo = data.ToString();
        //        checkCust.message = "Customer Exists";                

        //    }
        //    else
        //    {
        //        checkCust.CustomerNo = data.ToString();
        //        checkCust.message = "Customer Does Not Exist";
        //    }

        //    _logger.LogDebug("data" + data);
        //    return Ok(checkCust);

        //}

        
        ////[HttpPost]
        ////[Route("CreateContact")]
        ////[ProducesResponseType(200, Type = typeof(ContactCreate))]
        ////public IActionResult PostContact([FromBody] ContactCreate model)
        ////{

        ////    // _logger.LogDebug("Inside Post");
        ////    if (!ModelState.IsValid)
        ////        return BadRequest(ModelState);

        ////    var data = _customerService.CreateContact(model);
        ////    // _logger.LogDebug("Inside Post after create function" + data);
        ////    if (data != null)
        ////        return Ok(data);
        ////    return BadRequest();
        ////}


        ////Pioneer music 
        //[HttpPost]
        //[Route("UpdateContact")]
        //[ProducesResponseType(200, Type = typeof(ContactCreate))]
        //public IActionResult UpdateContact([FromBody] ContactCreate model)
        //{

        //    // _logger.LogDebug("Inside Post");
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);
        //    //check if contact  already exists in sage 
        //    int contact_id = 0;
        //    if (!string.IsNullOrEmpty(model.ContactCode))
        //    {
        //        contact_id = _customerService.CheckCustomerContactExists(model.CustomerNo, model.ContactCode);
        //        _logger.LogDebug("contactid" + contact_id);
        //    }
        //    _logger.LogDebug("step1------");
        //    if (contact_id == 1)
        //    {
        //        _logger.LogDebug("step2------");
        //        _logger.LogDebug("go for update");
        //        //if contact exists go for update 
        //        var data = _customerService.UpdateContact(model);
        //        if (data != null)
        //        {
        //            data.Status = "Success";
        //            data.Message = "Updated successfully";
        //        }
        //        else
        //        {
        //            data.Status = "Failed";
        //            data.Message = "Updated failed";
        //        }
                     
        //          return Ok(data);
                

        //    }
        //    else
        //    {               
        //        model.Status = "failed";
        //        model.Message = "Contact does not exist";
        //        return Ok(model);
        //    }

        //    return BadRequest();
        //}
        ////[HttpPost]
        ////[Route("DeleteContactLink")]
        ////[ProducesResponseType(200, Type = typeof(ContactLinkDeleteStatus))]
        ////public IActionResult UpdateContact([FromBody] ContactLinkDelete model)
        ////{

        ////    _logger.LogDebug("Inside UpdateContact");
        ////    if (!ModelState.IsValid)
        ////        return BadRequest(ModelState);

        ////    ContactLinkDeleteStatus ctds = new ContactLinkDeleteStatus();
        ////    //Check if the relationship exists
        ////    var check = _customerService.CheckCustomerContactExists(model.CustomerNo,model.ContactCode);
        ////    if(check==0)
        ////    {
        ////        _logger.LogDebug("no record found"+model.CustomerNo);
        ////        ctds.Status = "FAIL";
        ////        ctds.Message ="Record does not exist";
        ////        return Ok(ctds);
        ////    }
        ////    else if (check==1)
        ////    {
        ////        _logger.LogDebug(" record found" + model.CustomerNo);
        ////        var data = _customerService.DeleteContactLink(model);

        ////        if (data != null)
        ////        {
        ////            _logger.LogDebug("Delete Success" );
        ////            //delete contact link
        ////            ctds.Status = "SUCCESS";
        ////            ctds.Message = "Contact link record deleted ";
        ////            return Ok(ctds);
        ////        }


        ////    }

        ////    return null;

        ////}
        ////[HttpPost]
        ////[Route("Find")]
        ////[ProducesResponseType(200, Type = typeof(IEnumerable<Models.Customer>))]
        ////public IActionResult Find([FromBody] customerFilterParam model)
        ////{
        ////    var results = new List<Models.Customer>();

        ////    var data = _customerService.GetCustomerID(model);
        ////    if (data != null)
        ////    {
        ////        foreach (var d in data)
        ////        {
        ////            var c = _customerService.GetCustomerDetails(d.CustomerNo);
        ////            if (c != null) results.Add(c);
        ////        }
        ////    }

        ////    //var data = _customerService.Find(model);
        ////    //_logger.LogDebug("data" + data);
        ////    //return Ok(data);
        ////    return Ok(results);
        ////}


        ////[HttpPost("CheckCustomerContactExists")]

        ////[ProducesResponseType(200, Type = typeof(CustomerContactCheck))]
        ////public IActionResult CheckCustomerContactExists([FromBody] CustomerContactCheckParam model)
        ////{


        ////    CustomerContactCheck check = new CustomerContactCheck();
        ////    var data = _customerService.CheckCustomerContactExists(model.CustomerNo, model.ContactCode);
        ////    if (data == 0)
        ////    {
        ////        _logger.LogDebug("no record found" + model.CustomerNo);
        ////        check.Status = "FAIL";
        ////        check.message = "Contact Code: " + model.ContactCode + " does not exist under the customer:" + model.CustomerNo;
        ////        return Ok(check);
        ////    }
        ////    else
        ////    {
        ////        check.Status = "SUCCESS";
        ////        check.message = "Contact Code: " + model.ContactCode + " exists under the customer: " + model.CustomerNo;
        ////        return Ok(check);
        ////    }
        ////}

        //[HttpGet("testCustomer")]

        //[ProducesResponseType(200, Type = typeof(Models.Product))]
        //public IActionResult testCustomer()
        //{
        //    //OdbcConnection DbConnection = new OdbcConnection("DSN=Sage100TEST; UID=DCKAP; PWD=xferweb2*; Company=NSI;");
        //    OdbcConnection DbConnection = new OdbcConnection("DSN=SOTAMAS90; UID=vandhitha.ap; PWD=HubDev##; Company=PMC;"); 

        //    DbConnection.Open();

        //    OdbcCommand DbCommand = DbConnection.CreateCommand();

        //    string CustomerNo = "0000215";
        //    string ContactCode = "Ay001";

        //    string custno = "FLA131";
        //    //string sql = "select * from AR_CustomerContact  where CustomerNo='" + CustomerNo.ToUpper() + "' and ContactCode ='" + ContactCode.ToUpper() + "' ";
        //    //string sql = "select * from AR_CustomerListingWrk ";
        //    //string sql1 = "select * from AR_CustomerContact where CustomerNo='" + CustomerNo + "'";
        //    string sql = "select * from  AR_Customer  where CustomerNo='" + CustomerNo + "'";

        //    OdbcDataAdapter adapter = new OdbcDataAdapter(sql, DbConnection);
        //    // Create the table, fill it
        //    DataTable dt = new DataTable();
        //    adapter.Fill(dt);
        //    //Console.WriteLine(dt.Rows.Count);
        //    _logger.LogDebug("AR_Customer:" + JsonConvert.SerializeObject(dt));



        //    adapter.Dispose();
        //    DbCommand.Dispose();
        //    DbConnection.Close();
        //    return Ok(dt);
        //}


        ////Pioneer Music
        //[HttpPost]
        //[Route("Find")]
        //[ProducesResponseType(200, Type = typeof(IEnumerable<Models.Customer>))]
        //public IActionResult Find([FromBody] customerFilterParam model)
        //{
        //    //var results = new List<Models.Customer>();
        //    //var data = _customerService.FindCustomer(model);
        //    var data = _customerService.FindCustomerNew(model);
        //    return Ok(data);
        //}

        ////Pioneer music
        //[HttpGet("GetCustomerDetails/{CustomerNo}")]
        //[ProducesResponseType(200, Type = typeof(Models.Customer))]
        //public IActionResult GetOrderInfo(string CustomerNo)
        //{
        //    var results = new List<Models.OrderStatus>();
        //    var data = _customerService.GetCustomerDetails(CustomerNo);
        //    if (data != null)
        //    {
        //        return Ok(data);

        //    }
        //    return BadRequest("No Records found");

        //    //return Ok(data);
        //}


    }
}
