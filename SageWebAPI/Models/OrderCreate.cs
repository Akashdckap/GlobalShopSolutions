using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalSolutions.Models
{
    public class OrderCreate
    {

        public string ARDivisionNo { get; set; } 
        public string CustomerNo { get; set; }
        public string SalesOrderNo { get; set;}
        public string TaxSchedule { get; set; }
        public string CustomerPONo { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderType { get; set; }
        public string OrderStatus { get; set; }
        public string WarehouseCode { get; set; }
        public string SalespersonNo { get; set; }
        public DateTime ShipExpireDate { get; set; }
        public string ShipToCode { get; set; }
        public string ShipVia { get; set; }

        public string ShipToName { get; set; }

        public string ShipToAddress1 { get; set; }
        public string ShipToAddress2 { get; set; }
        public string ShipToAddress3 { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToState { get; set; }
        public string ShipToZipCode { get; set; }
        public string ShipToCountryCode { get; set; }
        public decimal FreightAmt { get; set; }
    }


    public class OrderCreate1
    {

        public string ARDivisionNo { get; set; }
        public string CustomerNo { get; set; }
        public string SalesOrderNo { get; set; }
        public string TaxSchedule { get; set; }
        public string CustomerPONo { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderType { get; set; }
        public string OrderStatus { get; set; }
        public string WarehouseCode { get; set; }
        public string SalespersonNo { get; set; }
        public DateTime ShipExpireDate { get; set; }
        public string ShipVia { get; set; }

        public string ShipToName { get; set; }

        public string ShipToAddress1 { get; set; }
        public string ShipToAddress2 { get; set; }
        public string ShipToAddress3 { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToState { get; set; }
        public string ShipToZipCode { get; set; }
        public string ShipToCountryCode { get; set; }
        public string ConfirmTo { get; set; }
        public string TermsCode { get; set; }
        public decimal FreightAmt { get; set; }
        public decimal SalesTaxAmt { get; set; }



    }

    public class Orderlist : OrderCreate
    {
        public List<ItemCreate> Itemlist { get; set; }
    }

    public class Orderlist1 : OrderCreate1
    {
        public List<ItemCreate1> Itemlist { get; set; }
    }


    public class ItemCreate
    {

        public string ItemCode { get; set; }
        public string ItemType { get; set; }

        public decimal QuantityOrdered { get; set; }
        public decimal UnitPrice { get; set; }

        public string LineNotes { get; set; }
        public string CostOfGoodsSoldAcctKey { get; set; } = "000000001";
        public string SalesAcctKey { get; set; } = "000000001";

    }

    public class ItemCreate1
    {

        public string ItemCode { get; set; }
        public string ItemType { get; set; }

        public decimal QuantityOrdered { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ExtensionAmt { get; set; }
        public string LineNotes { get; set; }
        public string CostOfGoodsSoldAcctKey { get; set; } = "000000001";
        public string SalesAcctKey { get; set; } = "000000001";

    }
}
