using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalSolutions.Models
{
    public class Order
    {

        public string ARDivisionNo { get; set; }
        public string CustomerNo { get; set; }
        public string SalesOrderNo { get; set; }
        public string ContactCode { get; set; } 
        public string TaxSchedule { get; set; }
        public string Comment { get; set; }
        public string ConfirmTo { get; set; }
        public string EmailAddress { get; set; }
        public string RMANo { get; set; }
        public string BillToName { get; set; }
        public string BillToAddress1 { get; set; }
        public string BillToAddress2 { get; set; }
        public string BillToAddress3 { get; set; }
        public string BillToCity { get; set; }
        public string BillToState { get; set; }
        public string BillToZipCode { get; set; }
        public string BillToCountryCode { get; set; }
        public string ShipToCode { get; set; }
        public string ShipToName { get; set; }
        public string ShipToAddress1 { get; set; }
        public string ShipToAddress2 { get; set; }
        public string ShipToAddress3 { get; set; }
        public string ShipToCity { get; set; }

        public string ShipToState { get; set; }
        public string ShipToZipCode { get; set; }
        public string ShipToCountryCode { get; set; }
        public DateTime ShipExpireDate { get; set; }
        public string WarehouseCode { get; set; }
        public DateTime? DateUpdated { get; set; }

        public string TimeUpdated { get; set; }
        public DateTime? DateCreated { get; set; }
        public string TimeCreated { get; set; }
        public decimal Amount { get; set; }
        public decimal? TaxableAmt { get; set; }
        public decimal? SalesTaxAmt { get; set; }
        public decimal? NonTaxableAmt { get; set; }
        public decimal? FreightAmt { get; set; }
        public decimal? DiscountAmt { get; set; }

    }

    public class OrderFilterParam
    {
        public string ARDivisionNo { get; set; }
        public string CustomerNo { get; set; }
        public string SalesOrderNo { get; set; }

    }

    public class LineDetail
    {

        public string ItemCode { get; set; }
        public string ItemType { get; set; }
        public decimal QuantityOrdered { get; set; }
        public string CostOfGoodsSoldAcctKey { get; set; } 
        public string SalesAcctKey { get; set; }

    }
    public class SOLineDetail
    {

        public string ItemCode { get; set; }
        public string ItemType { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal QuantityOrdered { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal QuantityBackordered { get; set; }
        public decimal QuantityShipped { get; set; }
    }

    public class SOInvoice
    {
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string SalesOrderNo { get; set; }
        public string CustomerNo { get; set; }
        public string RMANo { get; set; }
        public string ModuleCode { get; set; }
        public string InvoiceType { get; set; }
        public string OrderType { get; set; }
        public DateTime? OrderDate { get; set; }
        public int TotalRecords { get; set; }
        public List<RmaHeader> RMA { get; set; } = new List<RmaHeader>();
    }

    public class Salesorderdetails
    {
        public string InvoiceNo { get; set; }
        public string SalesOrderNo { get; set; }
    }

    public class SOInvoiceLine

    {

        public string InvoiceNo { get; set; }
        public string ItemCode { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal QuantityOrdered { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal QuantityBackordered { get; set; }
        public decimal QuantityShipped { get; set; }

    }

    public class RmaHeader
    {
        public string RMANo { get; set; }
        public string CustomerNo { get; set; }
        public string ReturnShipVia { get; set; }
        public string RMAStatus { get; set; }
        public string CrossShip { get; set; }
        public DateTime? RMADate { get; set; }
        public string FaxNo { get; set; }
        public string ReturnToName { get; set; }
        public string ReturnToAddress1 { get; set; }
        public string ReturnToAddress2 { get; set; }
        public string ReturnToAddress3 { get; set; }

        public string ReturnToCity { get; set; }

        public string ReturnToState { get; set; }

        public string ReturnToZipCode { get; set; }

        public string ReturnToCountryCode { get; set; }


    }

    public class OrderDetails : Order
    {

        public DateTime OrderDate { get; set; }
        public int TotalRecords { get; set; }
        public string OrderType { get; set; }
        public string SalespersonNo { get; set; }
        public List<SOLineDetail> LineDetails { get; set; } = new List<SOLineDetail>() ;
    }
}
