using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalSolutions.Models
{
    public class Product
    {

        public string ItemCode { get; set; }
        public string ItemType { get; set; }
        public string ItemCodeDesc { get; set; }
        public string StandardUnitOfMeasure { get; set; }
        public string DefaultWarehouseCode { get; set; }
        public string ProductType { get; set; }
        public string ProductLine { get; set; }
        public Decimal? TotalQuantityOnHand { get; set; }
        public string TaxClass { get; set; }
        public string PurchasesTaxClass { get; set; }
        public Decimal? StandardUnitCost { get; set; }
        public Decimal? StandardUnitPrice { get; set; }
        public Decimal? Volume { get; set; }
        public string Valuation { get; set; }
        public string SalesUnitOfMeasure { get; set; }
        public string PurchaseUnitOfMeasure { get; set; }
        public string Category1 { get; set; }
        public string Category2 { get; set; }
        public string Category3 { get; set; }
        public string Category4 { get; set; }
        public string ShipWeight { get; set; }
        public decimal? SalesPromotionPrice { get; set; }
        public decimal? AverageUnitCost { get; set; }
        public decimal? SuggestedRetailPrice { get; set; }
        public DateTime? SaleStartingDate { get; set; }
        public DateTime? SaleEndingDate { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string TimeUpdated { get; set; }
        public Decimal? UDF_000QOH { get; set; }
        public Decimal? UDF_BELQOH { get; set; }
        public Decimal? UDF_VANQOH { get; set; }
        public string UDF_SKU { get; set; }
        public string UDF_LEAD_TIME { get; set; }
        public string Sale_Description { get; set; } //udf field
        public decimal? Sale_Disc_Percent { get; set; } //udf field
        public string UDF_3M_MARKETPLACE_LIVE { get; set; }
        public decimal? UDF_GS_BACKORDER_BUFFER { get; set; }
        public decimal? UDF_GS_SALE_DISC_PERCENT { get; set; }
        public string UDF_GS_WEBSITE_LIVE { get; set; }
        public decimal? UDF_NSI_BACKORDER_BUFFER { get; set; }
        public decimal? UDF_NSI_SALE_DISC_PERCENT { get; set; }
        public string UDF_NSI_WEBSITE_LIVE { get; set; }
        public string UDF_MPN { get; set; }
        public decimal? UDF_GS_WEB_PRICE { get; set; }
        public decimal? UDF_NSI_WEB_PRICE { get; set; }
        public string UDF_WEB_DESCRIPTION { get; set; }
        public string UDF_WEB_PRODUCT_NAME { get; set; }

        public int TotalRecords { get; set; }
        public List<Itemwarehouse> WarehouseDetails { get; set; } = new List<Itemwarehouse>();

    }



    public class Itemwarehouse
    {
        public string WarehouseCode { get; set; }
        public decimal? QuantityOnHand { get; set; }
        public decimal? QuantityOnWorkOrder { get; set; }
        public decimal? QuantityAvailable { get; set; }
        public decimal? QuantityOnSalesOrder { get; set; }
        public decimal? QuantityOnBackOrder { get; set; }
        public decimal? QuantityOnPurchaseOrder { get; set; }

        public decimal? MinimumOrderQty { get; set; }
        public decimal? MaximumOnHandQty { get; set; }

        

    }

    public class ProductFilterParam
    {
         public DateTime DateUpdated { get; set; }
        public int CurrentIndex { get; set; }
         public int  PageSize { get; set; }
    }

    public class ProductLite
    {

        public string ItemCode { get; set; }
        public string ItemType { get; set; }
        public string ItemCodeDesc { get; set; }
        public string StandardUnitOfMeasure { get; set; }
        public string DefaultWarehouseCode { get; set; }
        public string ProductType { get; set; }
        public string ProductLine { get; set; }
        public string TaxClass { get; set; }
        public string PurchasesTaxClass { get; set; }
        public string SalesUnitOfMeasure { get; set; }
        public string PurchaseUnitOfMeasure { get; set; }

    }


    public class ProductExistsFilterParam
    {
        public string ItemCode { get; set; }

        public string upc_code { get; set; }

    }
    public class ProductExists
    {
        public string ItemCode { get; set; }
        public string ItemType { get; set; }
        public string ItemCodeDesc { get; set; }
        public string StandardUnitOfMeasure { get; set; }
        public string UDF_UPC_CODE { get; set; }

        public string UDF_GS_WEBSITE_LIVE { get; set; }

        public string UDF_NSI_WEBSITE_LIVE { get; set; }


        public string message { get; set; }

    }
}
