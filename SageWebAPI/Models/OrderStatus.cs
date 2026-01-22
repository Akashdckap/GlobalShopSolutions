using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalSolutions.Models
{
    public class OrderStatus
    {

        public string SalesOrderNo { get; set; }
        public string OrderType { get; set; }
        public string Orderstatus { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime DateUpdated { get; set; }
        public string TimeUpdated { get; set; }
        public List<InvoiceInfo> InvoiceDetails { get; set; } = new List<InvoiceInfo>();
    }


    public class OrderLine
    { 
        public string ItemCode { get; set; } 
        public string UnitOfMeasure { get; set; }    

    }

    public class InvoiceInfo
    {
        public string InvoiceNo { get; set; }
        public decimal? QuantityOrdered { get; set; }
        public decimal? QuantityShipped { get; set; }
        public string ShipStatus { get; set; }
        public string TrackingID { get; set; }
        public List<ShipmentTrackingDetails> ShipmentTrackingNos { get; set; } = new List<ShipmentTrackingDetails>();
    }

    public class ShipmentTrackingDetails
    {
        public string ItemCode { get; set; }
        public string UnitOfMeasure { get; set; }
        public string PackageNo { get; set; }
        public decimal Quantity { get; set; }
    }

    public class Items
    {
        public string ItemCode { get; set; }
        public string UnitOfMeasure { get; set; }
    }

    public class OrderStatusFilterParam
    {

        public DateTime DateUpdated { get; set; }
        public int CurrentIndex { get; set; }
        public int PageSize { get; set; }
        public string CustomerNo { get; set; }
    }

    public class FilterParam
    {
        public string CustomerNo { get; set; }
        public string SalesOrderNo { get; set; }
    }

   public class CustomerDetailsParam
    {
        public string ARDivisionNo { get; set; }
        public string CustomerNo { get; set; }

        public string ShipToCity { get; set; }
        public string ShipToState { get; set; }
        public string ShipToZipCode { get; set; }


    }

    public class SalespersonTaxSchedule
    {        
        public string Customer_TaxSchedule { get; set; }
        public string SalespersonNo { get; set; }
        public string ShipTo_TaxSchedule { get; set; }

    }

    public class OrdersFilterParam
    {
        public DateTime DateUpdated { get; set; } 
        public int CurrentIndex { get; set; }
        public int PageSize { get; set; }
    }
}
