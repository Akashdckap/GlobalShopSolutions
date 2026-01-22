using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GlobalSolutions.Services;
using GlobalSolutions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Newtonsoft.Json;

namespace GlobalSolutions.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Produces("application/json")]
    [Route("api/Products")]

    public class ProductsController : Controller
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IProductService productService;
        public const string AppJson = "application/json";
        public ProductsController( ILoggerFactory loggerFactory, IProductService _productService)
        {
            _logger = loggerFactory.CreateLogger<ProductsController>();
            productService = _productService;
        }

        [HttpPost("GetShipment")]
        public IActionResult GetShipment([FromBody] Inputparam model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = productService.GetShipmentDetails(model.page_size.Value, model.page_no.Value);
            if (response == null || !response.Any())
            {
                return BadRequest();
            }
            return Content(response, AppJson);

        }

        [HttpPost("GetShipmentLines")]
        public IActionResult GetShipmentlines([FromBody] Inputparam model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogDebug("controller", JsonConvert.SerializeObject(model));
            var ShipmentLines = productService.GetShipMentLines(model.page_size.Value, model.page_no.Value);
            if (ShipmentLines == null || !ShipmentLines.Any())
            {
                return BadRequest();
            }
            return Content(ShipmentLines, AppJson);
        }

        [HttpGet("GetShipmentLinesbyid/{order_no}")]
        public IActionResult GetShipmentlinesbyid(string order_no)
        {
            if (string.IsNullOrWhiteSpace(order_no))
            {
                return BadRequest(order_no);
            }
            var ShipmentLines = productService.GetShipMentLinesById(order_no);
            if (ShipmentLines == null || !ShipmentLines.Any())
            {
                return BadRequest();
            }
            return Content(ShipmentLines, AppJson);
        }


        [HttpPost("GetRecordStatus")]
        public IActionResult GetRecordStatus([FromBody] Recorddate model)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var recordSts = productService.GetRecordStatus(model.date_last_modified.Value);
            if(recordSts == null || !recordSts.Any())
            {
                return BadRequest();
            }
            return Content(recordSts, AppJson);
        }

        [HttpPost("InsertRecord")]
        public IActionResult InsertRecord([FromBody] Inputrequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var res = productService.CreateRecord(model.table_Name, model.jsonreq);
            if(res==0)
            {
                return Ok("Failed");
            }
            else
            {
                return Ok("Record Inserted Successfully");
            }
        }

        [HttpPost("GetContact")]

        public IActionResult GetContact([FromBody] Inputparam model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contacts = productService.GetContact(model.page_size.Value,model.page_no.Value);
            if(contacts == null || !contacts.Any())
            {
                return BadRequest();
            }

            return Content(contacts, AppJson);
        }
        [HttpPost("GetCustomer")]
        public IActionResult GetCustomer([FromBody] Inputparam model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var Customers = productService.GetCustomers(model.page_size.Value, model.page_no.Value);
            if (Customers == null || !Customers.Any())
            {
                return BadRequest();
            }
            return Content(Customers,"application/json");
        }

        [HttpPost("Getproduct")]
        public IActionResult Testproduct([FromBody] Inputparam model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var products = productService.ProductTest(model.page_size.Value, model.page_no.Value);
                if (products == null || !products.Any())
                {
                    return BadRequest();
                }
                return Content(products, AppJson);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occured while processing {ex.Message}");
            }  
        }
    }
}
