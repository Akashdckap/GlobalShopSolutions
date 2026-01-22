using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SageWebAPI.Models;

namespace SageWebAPI.Services
{
    public interface IOrdersService
    {
        Orderlist Create(Orderlist model);

        Orderlist1 CreateMagentoOrder(Orderlist1 model);
        bool CreateSalesTax(string TaxSchedule,string  order_no, decimal SalesTaxAmt);

        IEnumerable<OrderStatus> FindSalesOrders(OrderStatusFilterParam model);
        IEnumerable<SOInvoice> FindInvoices(OrdersFilterParam model);
        IEnumerable<OrderDetails> FindOrders(OrdersFilterParam model);
        //OrderStatus OrderDetails(string invoice_no,string salesorder_no,string orderstatus,string ordertype,DateTime dateupdated,string timeupdated);
        OrderStatus GetOrderDetails(FilterParam model);
        OrderStatus GetOrderDetails1(FilterParam model);

        OrderStatus GetOrderInfo(string OrderNumber);

        IEnumerable<OrderDetails> GetOrder(OrderFilterParam model);

        SalespersonTaxSchedule GetSalespersonandTaxInfo(CustomerDetailsParam model);
    }
}
