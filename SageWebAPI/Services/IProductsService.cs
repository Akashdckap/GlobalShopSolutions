using System.Collections.Generic;
using SageWebAPI.Models;
using System.Data;
using System.Data.Odbc;


namespace SageWebAPI.Services
{
    public interface IProductsService
    {
        
        ProductExists GetProduct(string ItemCode, string upc_code);

        //IEnumerable<Product> FindByDateLastModified(ProductFilterParam param);
        //IEnumerable<Product> FindByDateLastModified1(ProductFilterParam param);

        //IEnumerable<Product> Find(ProductFilterParam param);
        IEnumerable<Product> Productsbydatelastmodified(ProductFilterParam param);
        IEnumerable<Inventory> GetInventories(InventoryFilterParam param);
       // IEnumerable<Inventory> GetInventories1(InventoryFilterParam param);
        //IEnumerable<Inventory> GetInventories3M(InventoryFilterParam param);
        //IEnumerable<Inventory> GetInventoriesNS(InventoryFilterParam param);
        //IEnumerable<Inventory> GetInventoriesGS(InventoryFilterParam param);
        //IEnumerable<Inventory> GetInventoriesTEST(InventoryFilterParam param);

        ProductFound GetProductFound(string ItemCode);
        ProductPrice GetProductPrice(string ARDivisionNo, string Company_Code, string CustomerNo, string ItemCode, decimal qty,string CustPriceLevel,int price_code_record, decimal StandardUnitPrice);



        ProductPrice GetStandardPricewithDiscount(string ARDivisionNo, string Company_Code, string CustomerNo, string ItemCode, decimal StandardUnitPrice, decimal qty, int price_code_record, string CustPriceLevel);
        string GetCustomerPriceLevel(string CustomerNo,string ARDivisionNo);
        Product GetProductDetails(string ItemCode);
    }
}
