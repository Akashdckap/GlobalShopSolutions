using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SageWebAPI.Services;
using SageWebAPI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Data.Odbc;
using System.Data;
using Newtonsoft.Json;

namespace SageWebAPI.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Produces("application/json")]
    [Route("api/Orders")]
    public class OrdersController : Controller
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IOrdersService _ordersService;

        public OrdersController(IOrdersService ordersService, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProductsController>();
            _ordersService = ordersService;
        }

        ////[HttpPost]
        ////[Route("CreateOrder")]
        ////[ProducesResponseType(200, Type = typeof(Orderlist))]

        ////public IActionResult Post([FromBody] Orderlist model)
        ////{
        ////    if (!ModelState.IsValid)
        ////        return BadRequest(ModelState);

        ////    var data = _ordersService.Create(model);
        ////    _logger.LogDebug("Inside Post after create function" + data);
        ////    if (data != null)
        ////        return Ok(data);

        ////    return BadRequest();
        ////}


        ////[HttpPost]
        ////[Route("CreateMagentoOrder")]
        ////[ProducesResponseType(200, Type = typeof(Orderlist1))]

        ////public IActionResult PostOrder([FromBody] Orderlist1 model)
        ////{
        ////    if (!ModelState.IsValid)
        ////        return BadRequest(ModelState);

        ////    var data = _ordersService.CreateMagentoOrder(model);
        ////    _logger.LogDebug("Inside Post after create function" + data);
        ////    if (data != null)
        ////    {
        ////        string order_no = data.SalesOrderNo;
        ////        _logger.LogDebug("setting sales tax");
        ////        //add sales tax amount for the given order 
        ////        var Stax = _ordersService.CreateSalesTax(model.TaxSchedule, order_no, model.SalesTaxAmt);
        ////        return Ok(data);
        ////    }


        ////    return BadRequest();
        ////}

        ////find order and shipment status by last modified date
        ////[HttpPost("FindOrderStatus")]
        ////[ProducesResponseType(200, Type = typeof(IEnumerable<OrderStatus>))]
        ////public IActionResult FindOrderStatus([FromBody] OrderStatusFilterParam model)
        ////{
        ////var results = new List<Models.OrderStatus>();
        ////var data = _ordersService.FindSalesOrders(model);
        ////if (data != null)
        ////{
        ////    foreach (var d in data)
        ////    {
        ////        var c = _ordersService.OrderDetails(d.InvoiceNo, d.SalesOrderNo, d.Orderstatus, d.OrderType, d.DateUpdated, d.TimeUpdated);
        ////        if (c != null) results.Add(c);
        ////    }
        ////}

        //// return Ok(results);



        ////return Ok(data);
        ////}


        ////find order and shipment status by order number--not in use
        ////[HttpPost("GetOrderDetailsTest")]
        ////[ProducesResponseType(200, Type = typeof(OrderStatus))]
        ////public IActionResult GetOrderDetailsTest([FromBody] FilterParam model)
        ////{
        ////    var results = new List<Models.OrderStatus>();
        ////    var data = _ordersService.GetOrderDetails( model);
        ////    if (data!= null)
        ////    {
        ////        return Ok(data);

        ////    }
        ////    return BadRequest("No Records found");

        ////return Ok(data);
        ////}


        ////Pioneer Music
        //[HttpPost]
        //[Route("FindOrders")]
        //[ProducesResponseType(200, Type = typeof(IEnumerable<OrderDetails>))]
        //public IActionResult Find([FromBody] OrdersFilterParam model)
        //{
        //    //var results = new List<Models.Customer>();
        //    //var data = _customerService.FindCustomer(model);
        //    var data = _ordersService.FindOrders(model);
        //    return Ok(data);
        //}

        ////Pioneer Music
        ////Invoices and RMA will be returned.RMA is linked only to invoices
        //[HttpPost]
        //[Route("Invoices")]
        //[ProducesResponseType(200, Type = typeof(IEnumerable<SOInvoice>))]
        //public IActionResult Invoices([FromBody] OrdersFilterParam model)
        //{
        //    //var results = new List<Models.Customer>();
        //    //var data = _customerService.FindCustomer(model);
        //    var data = _ordersService.FindInvoices(model);
        //    return Ok(data);
        //}


        ////[HttpPost("GetOrderDetails")]
        ////[ProducesResponseType(200, Type = typeof(OrderStatus))]
        ////public IActionResult GetOrderDetails([FromBody] FilterParam model)
        ////{
        ////    var results = new List<Models.OrderStatus>();
        ////    var data = _ordersService.GetOrderDetails1(model);
        ////    if (data != null)
        ////    {
        ////        return Ok(data);

        ////    }
        ////    return BadRequest("No Records found");

        ////    //return Ok(data);
        ////}


        ////[HttpGet("GetOrderInfo/{SalesOrderNo}")]
        ////[ProducesResponseType(200, Type = typeof(OrderStatus))]
        ////public IActionResult GetOrderInfo(string SalesOrderNo)
        ////{
        ////    var results = new List<Models.OrderStatus>();
        ////    var data = _ordersService.GetOrderInfo(SalesOrderNo);
        ////    if (data != null)
        ////    {
        ////        return Ok(data);

        ////    }
        ////    return BadRequest("No Records found");

        ////    //return Ok(data);
        ////}



        ////[HttpPost("GetSalespersonNoTaxSchedule")]
        ////[ProducesResponseType(200, Type = typeof(SalespersonTaxSchedule))]
        ////public IActionResult GetSalespersonNoTaxSchedule([FromBody] CustomerDetailsParam model)
        ////{

        ////    var data = _ordersService.GetSalespersonandTaxInfo(model);
        ////    if (data != null)
        ////    {
        ////        return Ok(data);

        ////    }
        ////    return BadRequest("No Records found");

        ////    //return Ok(data);
        ////}


        //[HttpGet("OrderODBCTest")]

        //[ProducesResponseType(200, Type = typeof(Models.Product))]
        //public IActionResult OrderODBCTest()
        //{
        //    //RA_ReceiptsHistoryInvoice 
        //    //OdbcConnection DbConnection = new OdbcConnection("DSN=Sage100TEST; UID=DCKAP; PWD=xferweb2*; Company=NSI;");
        //    OdbcConnection DbConnection = new OdbcConnection("DSN=SOTAMAS90; UID=vandhitha.ap; PWD=HubDev##; Company=PMC;");

        //    DbConnection.Open();

        //    OdbcCommand DbCommand = DbConnection.CreateCommand();

        //    string Code = "0458315";
        //    string customer = "0001009";
        //    string order = "0458315";
        //    string inv = "0660125";
        //    string rma_no = "0005042";

        //    string sql00 = "select * from RA_ReceiptsHistoryHeader where RMANo ='" + inv + "' ";

        //    OdbcDataAdapter adapter00 = new OdbcDataAdapter(sql00, DbConnection);
        //    // Create the table, fill it
        //    DataTable dt00 = new DataTable();
        //    adapter00.Fill(dt00);
        //    //Console.WriteLine(dt.Rows.Count);
        //    _logger.LogDebug("RA_ReceiptsHistoryInvoice" + JsonConvert.SerializeObject(dt00));


        //    //string sql = "select * from  SO_InvoiceHeader";
        //    string sql = "SELECT * FROM AR_InvoiceHistoryHeader where CustomerNo='" + customer + "' ";
        //    OdbcDataAdapter adapter = new OdbcDataAdapter(sql, DbConnection);
        //    //Create the table, fill it
        //    DataTable dt = new DataTable();
        //    adapter.Fill(dt);
        //    //Console.WriteLine(dt.Rows.Count);
        //    _logger.LogDebug("AR_InvoiceHistoryHeader" + JsonConvert.SerializeObject(dt));

        //    /*string sql1 = "SELECT * FROM  SO_SalesOrderRecap where SalesOrderNo='" + order + "' ";
        //    adapter = new OdbcDataAdapter(sql1, DbConnection);
        //    // Create the table, fill it
        //    DataTable dt1 = new DataTable();
        //    adapter.Fill(dt1);
        //    //Console.WriteLine(dt.Rows.Count);
        //    //_logger.LogDebug("SO_SalesOrderRecap" + JsonConvert.SerializeObject(dt1));

        //    string sql11 = "SELECT * FROM   SO_SalesOrderHeader  where SalesOrderNo='" + order + "' ";
        //     adapter = new OdbcDataAdapter(sql11, DbConnection);
        //     // Create the table, fill it
        //     DataTable dt11 = new DataTable();
        //     adapter.Fill(dt11);
        //     //Console.WriteLine(dt.Rows.Count);
        //    // _logger.LogDebug("SO_SalesOrder" + JsonConvert.SerializeObject(dt11));*/

        //    /* string sql2 = "SELECT * FROM  AR_InvoiceHistoryTracking where InvoiceNo='" + inv + "' ";
        //     adapter = new OdbcDataAdapter(sql2, DbConnection);
        //     // Create the table, fill it
        //     DataTable dt2 = new DataTable();
        //     adapter.Fill(dt2);
        //     //Console.WriteLine(dt.Rows.Count);
        //     _logger.LogDebug("AR_InvoiceHistoryTracking" + JsonConvert.SerializeObject(dt2));

        //     string sql3 = "SELECT * FROM  AR_TrackingByItemHistory  where InvoiceNo='" + inv + "' ";
        //     adapter = new OdbcDataAdapter(sql3, DbConnection);
        //     // Create the table, fill it
        //     DataTable dt3 = new DataTable();
        //     adapter.Fill(dt3);
        //     //Console.WriteLine(dt.Rows.Count);
        //     _logger.LogDebug(" AR_TrackingByItemHistory " + JsonConvert.SerializeObject(dt3));*/

        //    /* foreach (DataRow o in dt.Select("InvoiceNo<> ' '"))
        //     {
        //         string invno = o["InvoiceNo"].ToString();
        //         string selectSQL1 = "select * from SO_DailyShipmentPackage where InvoiceNo=?";
        //         OdbcCommand selectCMD1 = new OdbcCommand(selectSQL1, (OdbcConnection)DbConnection);
        //         adapter.SelectCommand = selectCMD1;
        //         selectCMD1.Parameters.Add("@invno", OdbcType.VarChar).Value = invno;
        //         DataTable dt1 = new DataTable();
        //         adapter.Fill(dt1);
        //         _logger.LogDebug(JsonConvert.SerializeObject(dt1));
        //     }*/

        //    adapter00.Dispose();
        //    DbCommand.Dispose();
        //    DbConnection.Close();
        //    return Ok(dt00);
        //}



        ////[HttpGet]
        ////[Route("GetOrder")]
        ////[ProducesResponseType(200, Type = typeof(IEnumerable<Models.OrderDetails>))]

        ////public IActionResult Get ([FromBody] OrderFilterParam model)
        ////{
        ////    var data = _ordersService.GetOrder(model);
        ////    _logger.LogDebug("data" + data);
        ////    return Ok(data);

        ////}



        


    }
}
