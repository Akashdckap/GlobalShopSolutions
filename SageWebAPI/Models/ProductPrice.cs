using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalSolutions.Models
{
    public class ProductPrice
    {
        
        public string ItemCode { get; set; }
        public decimal? unit_price { get; set; }
        public decimal? regular_price { get; set; }
        
    }

    public class CustomerPriceParam
    {
        public string ARDivisionNo { get; set; }
        public string CustomerNo { get; set; }
        public string Company_Code { get; set; } //NS or GS

        public IEnumerable<ProductPriceParam> Products { get; set; } = new List<ProductPriceParam>();

    }

    public class ProductPriceParam
    {
        [Required]
        public string ItemCode { get; set; }        
       
        [Required]
        public decimal qty { get; set; }
    }

    public class ProductFound
    {
        public string ItemCode { get; set; }
        public string PriceCode { get; set; }
        public decimal StandardUnitPrice { get; set; }
        public decimal StandardUnitCost { get; set; }

    }

}
