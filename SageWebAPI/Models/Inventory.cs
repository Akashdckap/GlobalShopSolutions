using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalSolutions.Models
{
    public class Inventory
    {
        public string ItemCode { get; set; }
        public string ItemType { get; set; }
        public string ItemCodeDesc { get; set; }
        public string StandardUnitOfMeasure { get; set; }
        public string DefaultWarehouseCode { get; set; }
        public Decimal? TotalQuantityOnHand { get; set; }
        public string UDF_SKU { get; set; }

        public string UDF_UPC_CODE { get; set; }

        public string UDF_GS_WEBSITE_LIVE { get; set; }

        public string UDF_NSI_WEBSITE_LIVE { get; set; }
        public string UDF_MPN { get; set; }
        public string UDF_WEB_DESCRIPTION { get; set; }
        public string UDF_WEB_PRODUCT_NAME { get; set; }
        public DateTime? DateUpdated { get; set; }

        public string TimeUpdated { get; set; }
        public int TotalRecords { get; set; }

        
        public List<Warehouse> WarehouseDetails { get; set; } = new List<Warehouse>();
    }

    public class InventoryFilterParam
    {
        public DateTime DateUpdated { get; set; }
        public int CurrentIndex { get; set; }
        public int PageSize { get; set; }
        public string WarehouseCode { get; set; } = "000";

    }

    public class Warehouse
    {
        public string WarehouseCode { get; set; }
        public decimal? QuantityOnHand { get; set; }
        public decimal? QuantityOnSalesOrder { get; set; }
        public decimal? QuantityAvailable { get; set; }
    }
}
