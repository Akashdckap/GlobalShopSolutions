using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SageWebAPI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Data.Odbc;
using System.Data;
using Newtonsoft.Json;

namespace SageWebAPI.Services
{
    public class OrdersService : IOrdersService
    {
        protected readonly IDbConnectionService DbConn;
        protected readonly ILogger<OrdersService> Logger;
        private IConfiguration Configuration;

        public  OrdersService(ILoggerFactory loggerFactory, IConfiguration _configuration, IDbConnectionService dbConn)
        {

            Logger = loggerFactory.CreateLogger<OrdersService>();
            Configuration = _configuration;
            DbConn = dbConn;
        }

        public Orderlist Create(Orderlist model)
        {
            string homePath = this.Configuration.GetSection("AppSettings")["HomePath"];
            string companyCode = this.Configuration.GetSection("AppSettings")["CompanyCode"];
            string Username = this.Configuration.GetSection("AppSettings")["UserName"];
            string password = this.Configuration.GetSection("AppSettings")["password"];
            string accDate = DateTime.Now.ToString("yyyyMMdd");
            Orderlist ordcreate = new Orderlist();

            //Create shipcode in SO_ShipToAddress_BUS or SVC


            using (DispatchObject pvx1 = new DispatchObject("ProvideX.Script"))
            {
                pvx1.InvokeMethod("Init", homePath);
                using (DispatchObject oSS1 = new DispatchObject(pvx1.InvokeMethod("NewObject", "SY_Session")))
                {

                    Logger.LogDebug("Inside dispatchobj:" + oSS1);
                    oSS1.InvokeMethod("nLogon"); //If the Logon method returns 0 then you must use the SetUser method to allow access
                    oSS1.InvokeMethod("nSetUser", new object[] { Username, password }); //returns 1
                    oSS1.InvokeMethod("nSetCompany", companyCode);//returns 1 successful
                    oSS1.InvokeMethod("nSetDate", "S/O", accDate);
                    oSS1.InvokeMethod("nSetModule", "S/O"); //returns 1 successful
                    int TaskID = (int)oSS1.InvokeMethod("nLookupTask", "SO_SalesOrder_ui");
                    oSS1.InvokeMethod("nSetProgram", TaskID);
                    using (DispatchObject so_order = new DispatchObject(pvx1.InvokeMethod("NewObject", "SO_SalesOrder_bus", oSS1.GetObject()))) //Error 200 throw here
                    { 
                        try
                        {
                            string strSalesOrderNo = string.Empty;
                            object[] nxtOrderNo = new object[] { strSalesOrderNo };
                            var retVal = so_order.InvokeMethodByRef("nGetNextSalesOrderNo", nxtOrderNo);
                            Logger.LogDebug("Retval after nextorderno function" + retVal);
                            strSalesOrderNo = nxtOrderNo[0].ToString();
                            Logger.LogDebug("OrderNo:+strSalesOrderNo");
                            model.SalesOrderNo = strSalesOrderNo;
                            so_order.InvokeMethod("nSetKeyValue", "SalesOrderNo$", strSalesOrderNo);
                            so_order.InvokeMethod("nSetKey");
                            so_order.InvokeMethod("nSetValue", "ARDivisionNo$", model.ARDivisionNo);
                            so_order.InvokeMethod("nSetValue", "CustomerNo$", model.CustomerNo);
                            so_order.InvokeMethod("nSetValue", "TaxSchedule$", model.TaxSchedule);
                            so_order.InvokeMethod("nSetValue", "OrderDate$", model.OrderDate.ToString("yyyyMMdd"));
                            so_order.InvokeMethod("nSetValue", "ShipExpireDate$", model.ShipExpireDate.ToString("yyyyMMdd"));
                            so_order.InvokeMethod("nSetValue", "OrderType$", model.OrderType);
                            so_order.InvokeMethod("nSetValue", "OrderStatus$", model.OrderStatus);
                            so_order.InvokeMethod("nSetValue", "CustomerPONo$", model.CustomerPONo);
                            so_order.InvokeMethod("nSetValue", "SalespersonNo$", model.SalespersonNo);
                            so_order.InvokeMethod("nSetValue", "ShipToCode$", model.ShipToCode);
                            so_order.InvokeMethod("nSetValue", "ShipToName$", model.ShipToName);
                            so_order.InvokeMethod("nSetValue", "ShipToAddress1$", model.ShipToAddress1);
                            so_order.InvokeMethod("nSetValue", "ShipToAddress2$", model.ShipToAddress2);
                            so_order.InvokeMethod("nSetValue", "ShipToAddress3$", model.ShipToAddress3);
                            so_order.InvokeMethod("nSetValue", "ShipToCity$", model.ShipToCity);
                            so_order.InvokeMethod("nSetValue", "ShipToState$", model.ShipToState);
                            so_order.InvokeMethod("nSetValue", "ShipToZipCode$", model.ShipToZipCode);
                            so_order.InvokeMethod("nSetValue", "WarehouseCode$", model.WarehouseCode);
                            so_order.InvokeMethod("nSetValue", "ShipVia$", model.ShipVia);
                            //so_order.InvokeMethod("nSetValue", "SalesTaxAmt", model.SalesTaxAmt);
                            so_order.InvokeMethod("nSetValue", "FreightAmt", model.FreightAmt);

                            int icount = model.Itemlist.Count();
                            Logger.LogDebug("Count:"+icount);
                          // var items= ordcreate.Itemlist;
                          foreach(var items in  model.Itemlist)
                            {
                       
                            
                                using (DispatchObject so_orderlines = new DispatchObject(so_order.GetProperty("oLines")))
                                {
                                
                                    so_orderlines.InvokeMethod("nAddLine");
                                    so_orderlines.InvokeMethod("nSetValue", "ItemCode$", items.ItemCode);
                                    so_orderlines.InvokeMethod("nSetValue", "ItemType$", items.ItemType);
                                    so_orderlines.InvokeMethod("nSetValue", "QuantityOrdered", items.QuantityOrdered);
                                    so_orderlines.InvokeMethod("nSetValue", "UnitPrice", items.UnitPrice);
                                    so_orderlines.InvokeMethod("nSetValue", "CostOfGoodsSoldAcctKey$", items.CostOfGoodsSoldAcctKey);
                                    so_orderlines.InvokeMethod("nSetValue", "SalesAcctKey$", items.SalesAcctKey);
                                    retVal = so_orderlines.InvokeMethod("nWrite");

                                }
                                
                            }
                            retVal = so_order.InvokeMethod("nWrite");
                        }
                        catch (Exception ex)
                        {
                            Logger.LogDebug(ex.Message);
                        }
                        finally
                        {
                        so_order.Dispose();
                        }

                    }

                    oSS1.Dispose();
                }

                pvx1.Dispose();
            }

          return model;

        }

        public IEnumerable<OrderDetails> GetOrder(OrderFilterParam model)
        {
            string homePath = this.Configuration.GetSection("AppSettings")["HomePath"];
            string companyCode = this.Configuration.GetSection("AppSettings")["CompanyCode"];
            string Username = this.Configuration.GetSection("AppSettings")["UserName"];
            string password = this.Configuration.GetSection("AppSettings")["password"];
            string accDate = DateTime.Now.ToString("yyyyMMdd");
            List<OrderDetails> getOrder = new List<OrderDetails>();
            using (DispatchObject pvx1 = new DispatchObject("ProvideX.Script"))
            {

                pvx1.InvokeMethod("Init", homePath);
                using (DispatchObject oSS1 = new DispatchObject(pvx1.InvokeMethod("NewObject", "SY_Session")))
                {
                    oSS1.InvokeMethod("nLogon"); //If the Logon method returns 0 then you must use the SetUser method to allow access
                    oSS1.InvokeMethod("nSetUser", new object[] { "vanditha", "password" }); //returns 1
                    var ret1 = oSS1.InvokeMethod("nSetCompany", companyCode);//returns 1 successful
                    oSS1.InvokeMethod("nSetDate", "S/O", accDate);
                    var ret2 = oSS1.InvokeMethod("nSetModule", "S/O"); //returns 1 successful
                    int TaskID = (int)oSS1.InvokeMethod("nLookupTask", "AR_Customer_ui");
                    oSS1.InvokeMethod("nSetProgram", TaskID);

                    using (DispatchObject soGetOrder = new DispatchObject(pvx1.InvokeMethod("NewObject", "SO_SalesOrder_bus", oSS1.GetObject())))
                    {
                      try 
                      {
                            soGetOrder.InvokeMethod("nSetKeyValue", "ARDivisionNo$", model.ARDivisionNo);
                            soGetOrder.InvokeMethod("nSetKeyValue", "SalesOrderNo$", model.SalesOrderNo);                        
                            //soGetOrder.InvokeMethod("nSetKeyValue", "CustomerNo$", model.CustomerNo);
                            var val = soGetOrder.InvokeMethod("nSetKey");
                            int returnValue = (int)soGetOrder.InvokeMethod("nFind");
                            if (returnValue > 0)
                            {
                                string str1 = "", str2 = "";
                                var data = new object[] { str1, str2 };
                                val = soGetOrder.InvokeMethodByRef("nGetRecord", data);
                                string[] arrOrder = Array.ConvertAll((object[])data, Convert.ToString);
                                string[] charArrOrder = arrOrder[0].Split('Š');
                                List<string> OrderList = new List<string>(charArrOrder);
                                Logger.LogDebug("total count:" + OrderList.Count);
                                OrderDetails ord = new OrderDetails();
                                ord.ARDivisionNo = OrderList[6].ToString();
                                ord.SalesOrderNo = OrderList[0].ToString();
                                ord.CustomerNo = OrderList[7].ToString();
                                //ord.OrderDate = OrderList[1].ToString();
                                //ord.OrderStatus = OrderList[2].ToString();
                                ord.OrderType = OrderList[3].ToString();                        
                                getOrder.Add(ord);

                            }


                      }
                     catch(Exception ex)
                      {
                        Logger.LogDebug(ex.Message);

                      }
                     finally
                      {
                        soGetOrder.Dispose();
                      }
                            
                    }
                 oSS1.Dispose();
                }

             pvx1.Dispose();    
            }
          return getOrder;
        }

        public IEnumerable<OrderStatus> FindSalesOrders(OrderStatusFilterParam model)
        {

            using (var DbConnection = DbConn.GetodbcDbConnection())
            {
                try
                {
                    DbConnection.Open();
                    IList<OrderStatus> OrderDetails = new List<OrderStatus>();

                    //date filter
                    DateTime cdt = DateTime.Now;
                    var date = model.DateUpdated.ToString("yyyy-MM-dd");
                    var time = model.DateUpdated.ToString("HH:mm:ss");
                    
                    //time filter
                    string[] a = time.Split(new string[] { ":" }, StringSplitOptions.None);
                    //a[0] contains the hours, a[1] contains the minutes
                    decimal dectime = Math.Round(Convert.ToDecimal(a[0]) + (Convert.ToDecimal(a[1]) / 60), 4);


                    int currentIndex = model.CurrentIndex;
                    int pageSize = model.PageSize;

                    // Assumes that connection is a valid OdbcConnection object.  
                    OdbcDataAdapter adapter = new OdbcDataAdapter();
                    //string selectSQL = "SELECT * FROM  SO_SalesOrderRecap where DateUpdated>=? and TimeUpdated>=? and OrderStatus=? and OrderType=?";
                    string selectSQL = "SELECT * FROM  SO_SalesOrderRecap where DateUpdated>=? and TimeUpdated>=? and OrderType=? and CustomerNo=? ";
                    OdbcCommand selectCMD = new OdbcCommand(selectSQL, (OdbcConnection)DbConnection);
                    adapter.SelectCommand = selectCMD;

                    //Add Parameters and set values.  
                    selectCMD.Parameters.Add("@date", OdbcType.DateTime).Value = date;
                    selectCMD.Parameters.Add("@time", OdbcType.Double).Value = dectime;                    
                    selectCMD.Parameters.Add("@ordertype", OdbcType.VarChar).Value = "S";
                    selectCMD.Parameters.Add("@custno", OdbcType.VarChar).Value = model.CustomerNo;

                    //create a dataset and fill it
                    DataSet ds = new DataSet();
                    adapter.Fill(ds, currentIndex, pageSize, "OrderRecap");
                    DataTable datbl = ds.Tables[0];
                    datbl.PrimaryKey = new DataColumn[] { datbl.Columns["SalesOrderNo"] };

                    int TotalRecords = getCount(date, dectime,model.CustomerNo);
                    if (datbl.Rows.Count > 0)
                    {
                        foreach (DataRow o in datbl.Select("SalesOrderNo<> ' ' "))
                        {
                            string sOrderNo = o["SalesOrderNo"].ToString();
                            string selectSQL1 = "SELECT * FROM  SO_InvoiceHeader where SalesOrderNo=? ";
                            OdbcCommand selectCMD1 = new OdbcCommand(selectSQL1, (OdbcConnection)DbConnection);
                            adapter.SelectCommand = selectCMD1;
                            selectCMD1.Parameters.Add("@sorder", OdbcType.VarChar).Value = sOrderNo;
                            adapter.Fill(ds, "InvOrder");
                        }


                        DataTable datbl1 = ds.Tables[1];
                        datbl1.PrimaryKey = new DataColumn[] { datbl1.Columns["SalesOrderNo"] };
                        datbl.Merge(datbl1);
                        //datbl.AcceptChanges();

                        Logger.LogDebug("Find Sales Orders" + JsonConvert.SerializeObject(datbl));



                        foreach (DataRow row in datbl.Rows)
                        {
                            OrderStatus os = new OrderStatus();

                            os.SalesOrderNo = row.Field<string>("SalesOrderNo");
                            os.Orderstatus = row.Field<string>("OrderStatus");
                            os.OrderType = row.Field<string>("OrderType");
                            //os.WarehouseCode = row.Field<string>("WarehouseCode");
                            os.InvoiceNo = row.Field<string>("InvoiceNo");
                            os.DateUpdated = row.Field<DateTime>("DateUpdated");
                            os.TimeUpdated = row.Field<string>("TimeUpdated");
                           // os.TotalRecords = TotalRecords;
                            //os.ShipStatus = row.Field<string>("ShipStatus");

                            //string orderno = row.Field<string>("SalesOrderNo").ToString();

                            //foreach (DataRow row1 in datbl1.Select().Where(e => e.ItemArray[0].ToString() == orderno))
                            //{
                            //os.WarehouseCode = row1.Field<string>("WarehouseCode");
                            //os.InvoiceNo = row1.Field<string>("InvoiceNo");
                            //}

                            OrderDetails.Add(os);

                        }
                        return OrderDetails;
                    }
                    return null;


                    
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                finally
                {
                    //selectCMD.Dispose();
                    //adapter.Dispose();
                    DbConnection.Close();

                }

                return null;

            }



        }

        //public OrderStatus OrderDetails(string invoice_no, string salesorder_no, string orderstatus, string ordertype, DateTime dateupdated, string timeupdated)
        //{

        //    using (var DbConnection = DbConn.GetodbcDbConnection())
        //    {
        //        try
        //        {
                    
        //            DbConnection.Open();
        //            OdbcDataAdapter adapter = new OdbcDataAdapter();
        //            DataSet ds = new DataSet();

        //            //With sales order no get qty ordered and line key 
        //            string selectSQL0 = "SELECT * FROM  SO_SalesOrderDetail where SalesOrderNo=? ";
        //            OdbcCommand selectCMD0 = new OdbcCommand(selectSQL0, (OdbcConnection)DbConnection);
        //            adapter.SelectCommand = selectCMD0;                   
        //            selectCMD0.Parameters.Add("@sorder", OdbcType.VarChar).Value = salesorder_no;        
        //            adapter.Fill(ds, "SOrderDetail");
        //            DataTable datbl = ds.Tables[0];

        //            Logger.LogDebug("SO_SalesOrderDetail:" + JsonConvert.SerializeObject(datbl));


        //            decimal qtyOrdered = 0M;
        //            int totalQtyOrdered = 0;
        //            int totalQtyShipped = 0;
        //            decimal qtyShipped = 0M;
        //            decimal qtyBackOrdered = 0M;

        //            OrderStatus os = new OrderStatus();

        //            foreach (DataRow dr in datbl.Select().Where(e => e.ItemArray[0].ToString() == salesorder_no))
        //            {
        //                OrderLine ol = new OrderLine();
        //                ol.LineKey = dr["LineKey"].ToString();
        //                ol.ItemCode = dr["ItemCode"].ToString();
        //                ol.QuantityOrdered = (Decimal?)dr["QuantityOrdered"];
        //                ol.UnitOfMeasure = dr["UnitOfMeasure"].ToString();
                        
        //                qtyOrdered = (Decimal)dr["QuantityOrdered"];
        //                totalQtyOrdered = totalQtyOrdered + Convert.ToInt32(qtyOrdered);
        //                Logger.LogDebug("ItemCode"+ dr["ItemCode"].ToString()+"uom:"+ dr["UnitOfMeasure"].ToString());
        //                //this table will have data only if it is invoiced
        //                //going in for quantity shipped and total quantity shipped
        //                if (invoice_no != null) 
        //                { 

        //                    string selectSQL = "SELECT * FROM  SO_InvoiceDetail  where ItemCode=? and InvoiceNo=? and OrderLineKey=? ";
        //                    OdbcCommand selectCMD = new OdbcCommand(selectSQL, (OdbcConnection)DbConnection);
        //                    adapter.SelectCommand = selectCMD;
        //                    selectCMD.Parameters.Add("@itemcode", OdbcType.VarChar).Value = dr["ItemCode"].ToString();
        //                    selectCMD.Parameters.Add("@invno", OdbcType.VarChar).Value = invoice_no;
        //                    selectCMD.Parameters.Add("@itemcode", OdbcType.VarChar).Value = dr["LineKey"].ToString(); 
        //                    //DataSet ds = new DataSet();
        //                    adapter.Fill(ds, "invoicedetail");
        //                    DataTable datble = ds.Tables[1];
        //                    string test= datble.Rows[0].Field<string>("InvoiceNo");
        //                    ol.QuantityShipped = datble.Rows[0].Field<decimal>("QuantityShipped");
        //                    ol.QuantityBackordered = datble.Rows[0].Field<decimal>("QuantityBackordered");
        //                    qtyShipped = datble.Rows[0].Field<decimal>("QuantityShipped");
        //                    totalQtyShipped = totalQtyShipped + Convert.ToInt32(qtyShipped);

        //                    Logger.LogDebug("invoiceno" + test + "shipped:" + qtyShipped);

        //                }

        //                os.LineDetails.Add(ol);                        

        //            }

        //                os.TotalQuantityOrdered = totalQtyOrdered;
        //                os.TotalQuantityShipped = totalQtyShipped;
        //                os.SalesOrderNo = salesorder_no;
        //                os.Orderstatus = orderstatus;
        //                os.OrderType = ordertype;                       
        //                os.InvoiceNo = invoice_no;
        //                //os.TotalRecords = TotalRecords;
        //            os.DateUpdated = dateupdated;
        //                os.TimeUpdated = timeupdated;


        //            if (!String.IsNullOrEmpty(invoice_no))
        //            {
                       

        //                //this table has shipcomplete status
        //                string selectSQL1 = "SELECT * FROM  SO_InvoiceAppliedSalesOrders where InvoiceNo=?";
        //                OdbcCommand selectCMD1 = new OdbcCommand(selectSQL1, (OdbcConnection)DbConnection);
        //                adapter.SelectCommand = selectCMD1;
        //                //Add Parameters and set values.  
        //                selectCMD1.Parameters.Add("@invno", OdbcType.VarChar).Value = invoice_no;                        
        //                adapter.Fill(ds, "invappliedSO");
        //                DataTable datbl1 = ds.Tables[2];
        //                Logger.LogDebug("SO_InvoiceAppliedSalesOrders:" + JsonConvert.SerializeObject(datbl1));
        //                datbl1.PrimaryKey = new DataColumn[] { datbl1.Columns["InvoiceNo"] };                        

        //                Logger.LogDebug(JsonConvert.SerializeObject(datbl1));

        //                //this table holds tracking nos
        //                string selectSQL2 = "SELECT * FROM  SO_DailyShipmentPackage where InvoiceNo=?";
        //                OdbcCommand selectCMD2 = new OdbcCommand(selectSQL2, (OdbcConnection)DbConnection);
        //                adapter.SelectCommand = selectCMD2;
        //                //Add Parameters and set values.  
        //                selectCMD2.Parameters.Add("@invno", OdbcType.VarChar).Value = invoice_no;
        //                adapter.Fill(ds, "dailyshipmentpackage");
        //                DataTable datbl2 = ds.Tables[3];
        //                Logger.LogDebug("SO_DailyShipmentPackage:" + JsonConvert.SerializeObject(datbl2));
        //                datbl2.PrimaryKey = new DataColumn[] { datbl2.Columns["InvoiceNo"] };
        //                datbl1.Merge(datbl2);

        //                foreach (DataRow drow in datbl1.Rows)
        //                {
        //                    os.ShipComplete = drow.Field<string>("ShipComplete");
        //                    //os.TrackingID = drow.Field<string>("TrackingID");
        //                }
                                               
                        

        //            }
                    
        //                /*decimal qtyOrdered = 0M;
        //                int totalQtyOrdered = 0;
        //                int totalQtyShipped = 0;
        //                decimal qtyShipped = 0M;
        //                decimal qtyBackOrdered = 0M;

        //                if (invoice_no != null && orderstatus!= "Z")
        //                { 
        //                    foreach (DataRow dr in datbl.Select().Where(e => e.ItemArray[0].ToString() == salesorder_no))
        //                    {
        //                        //OrderLine ol = new OrderLine();
        //                        //ol.LineKey = Convert.ToInt32(dr["LineKey"]);
        //                        //ol.ItemCode = dr["ItemCode"].ToString();
        //                       // ol.QuantityOrdered = (Decimal?)dr["QuantityOrdered"];
        //                        qtyOrdered = (Decimal)dr["QuantityOrdered"];
        //                        //ol.QuantityShipped = (Decimal?)dr["QuantityShipped"];
        //                        qtyShipped = (Decimal)dr["QuantityShipped"];
        //                        //ol.QuantityBackordered = (Decimal?)dr["QuantityBackordered"]; //how to handle backorder
        //                        qtyBackOrdered = (Decimal)dr["QuantityBackordered"];
        //                        //os.LineDetails.Add(ol);
        //                    totalQtyOrdered = totalQtyOrdered + Convert.ToInt32(qtyOrdered) + Convert.ToInt32(qtyBackOrdered);
        //                    os.TotalQuantityOrdered = totalQtyOrdered;
        //                    totalQtyShipped = totalQtyShipped + Convert.ToInt32(qtyShipped);
        //                    os.TotalQuantityShipped = totalQtyShipped;
        //                    }
                           
        //                    if (totalQtyOrdered > totalQtyShipped)
        //                    {
        //                        os.StatusDesc = "Partial Shipment";
        //                    }
        //                    else if(totalQtyOrdered == totalQtyShipped)
        //                    {
        //                        os.StatusDesc = "Shipped";
        //                    }
        //                }
        //                else if (invoice_no ==null && orderstatus == "O")
        //                {
        //                    os.StatusDesc = "Processing";
        //                }

        //                else if (invoice_no == null && orderstatus == "Z")
        //                {
        //                    os.StatusDesc = "Cancelled";
        //                }*/
                    


        //            return os;
                   
                    
                    


                    
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.LogError(ex.ToString());
        //        }
        //        finally
        //        {
        //            //selectCMD.Dispose();
        //            //adapter.Dispose();
        //            DbConnection.Close();

        //        }

        //        return null;

        //    }



        //}

        public OrderStatus GetOrderDetails(FilterParam param)
        {

            using (var DbConnection = DbConn.GetodbcDbConnection())
            {
                try
                {

                    DbConnection.Open();
                    OdbcDataAdapter adapter = new OdbcDataAdapter();
                    DataSet ds = new DataSet();
                    string sInvoiceno=string.Empty;
                    decimal qtyOrdered = 0M;
                    int totalQtyOrdered = 0;
                    int totalQtyShipped = 0;
                    decimal qtyShipped = 0M;
                    //decimal qtyBackOrdered = 0M;
                    List<string> invoicenolist = new List<string>();
                    List<string> trackingnos = new List<string>();
                    OrderStatus os = new OrderStatus();

                    //get salesorder status from salesorder recap
                    string SQL = "SELECT * FROM  SO_SalesOrderRecap where CustomerNo=? and SalesOrderNo=? ";
                    OdbcCommand CMD = new OdbcCommand(SQL, (OdbcConnection)DbConnection);
                    adapter.SelectCommand = CMD;
                    CMD.Parameters.Add("@custno", OdbcType.VarChar).Value = param.CustomerNo;
                    CMD.Parameters.Add("@sorder", OdbcType.VarChar).Value = param.SalesOrderNo;
                    //create a dataset and fill it
                    
                    adapter.Fill(ds, "OrderRecap");
                    DataTable dtrecap = ds.Tables[0];
                    if (dtrecap.Rows.Count <= 0) { return null; }
                    else { 
                        foreach (DataRow row in dtrecap.Rows)
                        { 
                        os.Orderstatus= row["Orderstatus"].ToString();
                        os.OrderType = row["OrderType"].ToString();
                        os.DateUpdated = (DateTime)row["DateUpdated"];
                        os.TimeUpdated = row["TimeUpdated"].ToString();
                        os.SalesOrderNo = param.SalesOrderNo;
                        }
                    }

                    //With sales order no get qty ordered and line key 
                    string SQL2 = "SELECT * FROM  SO_SalesOrderDetail where SalesOrderNo=? ";
                    OdbcCommand CMD2 = new OdbcCommand(SQL2, (OdbcConnection)DbConnection);
                    adapter.SelectCommand = CMD2;
                    CMD2.Parameters.Add("@sorder", OdbcType.VarChar).Value = param.SalesOrderNo;
                    adapter.Fill(ds, "SOrderDetail");
                    DataTable dtSODetail = ds.Tables[1];

                    Logger.LogDebug("SO_SalesOrderDetail:" + JsonConvert.SerializeObject(dtSODetail));


                    if (dtSODetail.Rows.Count > 0)
                    {
                        Logger.LogDebug("dtSODetail.Rows.Count > 0");
                        foreach (DataRow dr in dtSODetail.Rows)
                        {
                            qtyOrdered = (decimal)dr["QuantityOrdered"];
                            totalQtyOrdered = totalQtyOrdered + Convert.ToInt32(qtyOrdered);
                                                  
                        }
                    }

                    //os.TotalQuantityOrdered = totalQtyOrdered;
                    //os.TotalQuantityShipped = totalQtyShipped;



                    //Check for multiple invoice nos 
                    //this table will have data only if it is invoiced
                    string SQL1 = "SELECT * FROM  SO_InvoiceHeader where SalesOrderNo=? ";
                        OdbcCommand CMD1 = new OdbcCommand(SQL1, (OdbcConnection)DbConnection);
                        adapter.SelectCommand = CMD1;
                        CMD1.Parameters.Add("@sorder", OdbcType.VarChar).Value = param.SalesOrderNo;
                        adapter.Fill(ds, "InvOrder");
                        DataTable dtinvheader = ds.Tables[2];
                        
                        if (dtinvheader.Rows.Count > 0) 
                        {

                            Logger.LogDebug("dtinvheader.Rows.Count > 0");
                       
                            foreach(DataRow invrow in dtinvheader.Rows)
                            {
                                    InvoiceInfo invInfo = new InvoiceInfo();
                                    invoicenolist.Add(invrow.Field<string>("InvoiceNo"));
                                    sInvoiceno = invrow.Field<string>("InvoiceNo");

                                     invInfo.InvoiceNo = sInvoiceno;

                                    //get invoice details
                                    string SQL4 = "SELECT * FROM  SO_InvoiceDetail where InvoiceNo=?";
                                    OdbcCommand CMD4 = new OdbcCommand(SQL4, (OdbcConnection)DbConnection);
                                    adapter.SelectCommand = CMD4;
                                    //Add Parameters and set values.  
                                    CMD4.Parameters.Add("@invno", OdbcType.VarChar).Value = sInvoiceno;
                                    adapter.Fill(ds, "invdetail");
                                    DataTable dtinvdetail = ds.Tables[3];
                                
                                    decimal invqtyOrdered = 0m;
                                    decimal invTotalOrdered = 0m;
                                    decimal invTotalShipped = 0m;
                                    decimal invqtyShipped = 0m;

                                    if (dtinvdetail.Rows.Count > 0)
                                    {
                                        foreach (DataRow invdetailrow in dtinvdetail.Rows)
                                        {
                                  
                                            invqtyOrdered = invdetailrow.Field<decimal>("QuantityOrdered");
                                            invTotalOrdered = invTotalOrdered + Convert.ToInt32(invqtyOrdered);
                                            invInfo.QuantityOrdered = invTotalOrdered;
                                            invqtyShipped = invdetailrow.Field<decimal>("QuantityShipped");
                                            invTotalShipped = invTotalShipped + Convert.ToInt32(invqtyShipped);
                                            invInfo.QuantityShipped = invTotalShipped;

                                            string itemid= invdetailrow.Field<string>("itemcode");
                                            string uom = invdetailrow.Field<string>("UnitOfMeasure");

                                   
                                                string SQL5 = "SELECT * FROM  SO_PackageTrackingByItem where InvoiceNo=? and ItemCode=? ";
                                                OdbcCommand CMD5 = new OdbcCommand(SQL5, (OdbcConnection)DbConnection);
                                                adapter.SelectCommand = CMD5;
                                                //Add Parameters and set values.  
                                                CMD5.Parameters.Add("@invno", OdbcType.VarChar).Value = sInvoiceno;
                                                CMD5.Parameters.Add("@itemno", OdbcType.VarChar).Value = itemid;
                                                //CMD5.Parameters.Add("@packno", OdbcType.VarChar).Value = spackageno;
                                                adapter.Fill(ds, "sotrackingpackage1");
                                                DataTable dttrackitem = ds.Tables[4];
                                       

                                                foreach (DataRow drtrack in dttrackitem.Rows)
                                                {

                                                     ShipmentTrackingDetails std = new ShipmentTrackingDetails();
                                                    std.ItemCode = itemid;
                                                    std.UnitOfMeasure = uom; 
                                                    std.PackageNo = drtrack.Field<string>("PackageNo"); 
                                                    std.Quantity = drtrack.Field<decimal>("Quantity");  
                                                    //std.TrackingID = dtinvtracking.Rows[0].Field<string>("TrackingID");
                                                    invInfo.ShipmentTrackingNos.Add(std);

                                                }
                                                dttrackitem.Clear();
                                    

                                        }//adding for-each 
                                            if ( (invTotalShipped > 0))
                                            {
                                                if (invTotalOrdered > invTotalShipped)
                                                {
                                                    invInfo.ShipStatus = "Partially Shipped";

                                                }
                                                else if (invTotalOrdered == invTotalShipped)
                                                {
                                                    invInfo.ShipStatus = "Shipped";
                                                }
                                            }

                                //this table holds tracking nos
                                string SQL6 = "SELECT * FROM  SO_InvoiceTracking where InvoiceNo=?";
                                OdbcCommand CMD6 = new OdbcCommand(SQL6, (OdbcConnection)DbConnection);
                                adapter.SelectCommand = CMD6;
                                //Add Parameters and set values.  
                                CMD6.Parameters.Add("@invno", OdbcType.VarChar).Value = sInvoiceno;
                                adapter.Fill(ds, "shipmentpackage");
                                DataTable dtshiptracking = ds.Tables[5];
                                foreach(DataRow drtrackship in dtshiptracking.Rows)
                                {
                                    string s = drtrackship.Field<string>("PackageNo") + "-" + drtrackship.Field<string>("TrackingID");
                                    trackingnos.Add(s);
                                }
                                var shipresult = String.Join(", ", trackingnos.ToArray());
                                invInfo.TrackingID = shipresult;

                            }  // if loop


                            var InvNoresult = String.Join(", ", invoicenolist.ToArray());
                                        os.InvoiceNo = InvNoresult;  //appended invoice nos
                                        os.InvoiceDetails.Add(invInfo);
                            }

                                
                                                       

                        }
                    

                   

                    return os;

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                finally
                {
                   
                    DbConnection.Close();

                }

                return null;

            }



        }

        //Get the total count of records available for the date & time and customer number
        public int getCount(string d, decimal t,string customerno)
        {
            int iCount = 0;
            using (var DbConnection = DbConn.GetodbcDbConnection())
            {


                try
                {
                    OdbcCommand DbCommand = (OdbcCommand)DbConnection.CreateCommand();
                    DbConnection.Open();

                    DbCommand.CommandText = "select Count(*) from SO_SalesOrderRecap  where DateUpdated>=? and TimeUpdated>=? and CustomerNo=? ";
                    DbCommand.Parameters.Add("@date", OdbcType.DateTime).Value = d;  // "{"+"d"+date+"}";
                    DbCommand.Parameters.Add("@time", OdbcType.Double).Value = t;
                    DbCommand.Parameters.Add("@custno", OdbcType.VarChar).Value = customerno;

                    Int32 count = Convert.ToInt32(DbCommand.ExecuteScalar());
                    iCount = count;
                    return iCount;
                }
                catch (Exception ex)
                {
                    {
                        Logger.LogError(ex.ToString());
                    }
                }

                finally
                {
                    DbConnection.Close();
                }

                return iCount;
            }


        }

        //
        public OrderStatus GetOrderDetails1(FilterParam param)
        {

            using (var DbConnection = DbConn.GetodbcDbConnection())
            {
                try
                {

                    DbConnection.Open();
                    OdbcDataAdapter adapter = new OdbcDataAdapter();
                    DataSet ds = new DataSet();
                    string sInvoiceno = string.Empty;
                    decimal qtyOrdered = 0M;
                    int totalQtyOrdered = 0;
                    int totalQtyShipped = 0;
                    decimal qtyShipped = 0M;
                    //decimal qtyBackOrdered = 0M;
                    List<string> invoicenolist = new List<string>();
                    List<string> trackingnos = new List<string>();
                    OrderStatus os = new OrderStatus();

                    //get salesorder status from salesorder recap
                    string SQL = "SELECT * FROM  SO_SalesOrderRecap where CustomerNo=? and SalesOrderNo=? ";
                    OdbcCommand CMD = new OdbcCommand(SQL, (OdbcConnection)DbConnection);
                    adapter.SelectCommand = CMD;
                    CMD.Parameters.Add("@custno", OdbcType.VarChar).Value = param.CustomerNo;
                    CMD.Parameters.Add("@sorder", OdbcType.VarChar).Value = param.SalesOrderNo;
                    //create a dataset and fill it
                    Logger.LogDebug("SO_SalesOrderRecap"+ param.SalesOrderNo);
                    adapter.Fill(ds, "OrderRecap");
                    DataTable dtrecap = ds.Tables[0];
                    if (dtrecap.Rows.Count <= 0) { return null; }
                    else
                    {
                        foreach (DataRow row in dtrecap.Rows)
                        {
                            os.Orderstatus = row["Orderstatus"].ToString();
                            os.OrderType = row["OrderType"].ToString();
                            os.DateUpdated = (DateTime)row["DateUpdated"];
                            os.TimeUpdated = row["TimeUpdated"].ToString();
                            os.SalesOrderNo = param.SalesOrderNo;
                        }
                    }

                    //With sales order no get qty ordered and line key 
                    //string SQL2 = "SELECT * FROM  SO_SalesOrderDetail where SalesOrderNo=? ";
                    //OdbcCommand CMD2 = new OdbcCommand(SQL2, (OdbcConnection)DbConnection);
                    //adapter.SelectCommand = CMD2;
                    //CMD2.Parameters.Add("@sorder", OdbcType.VarChar).Value = param.SalesOrderNo;
                    //adapter.Fill(ds, "SOrderDetail");
                    //DataTable dtSODetail = ds.Tables[1];

                    //Logger.LogDebug("SO_SalesOrderDetail:" + JsonConvert.SerializeObject(dtSODetail));


                    //if (dtSODetail.Rows.Count > 0)
                    //{
                    //    Logger.LogDebug("dtSODetail.Rows.Count > 0");
                    //    foreach (DataRow dr in dtSODetail.Rows)
                    //    {
                    //        qtyOrdered = (decimal)dr["QuantityOrdered"];
                    //        totalQtyOrdered = totalQtyOrdered + Convert.ToInt32(qtyOrdered);

                    //    }
                    //}

                    //os.TotalQuantityOrdered = totalQtyOrdered;
                    //os.TotalQuantityShipped = totalQtyShipped;



                    //Check for multiple invoice nos 
                    //this table will have data only if it is invoiced
                    string SQL1 = "SELECT * FROM  AR_InvoiceHistoryHeader where SalesOrderNo=? ";
                    OdbcCommand CMD1 = new OdbcCommand(SQL1, (OdbcConnection)DbConnection);
                    adapter.SelectCommand = CMD1;
                    CMD1.Parameters.Add("@sorder", OdbcType.VarChar).Value = param.SalesOrderNo;
                    adapter.Fill(ds, "arInvhistOrder");
                    DataTable dtinvheader = ds.Tables[1];

                    if (dtinvheader.Rows.Count > 0)
                    {

                        Logger.LogDebug("dtinvheader.Rows.Count > 0");

                        foreach (DataRow invrow in dtinvheader.Rows)
                        {
                            InvoiceInfo invInfo = new InvoiceInfo();
                            invoicenolist.Add(invrow.Field<string>("InvoiceNo"));
                            sInvoiceno = invrow.Field<string>("InvoiceNo");

                            invInfo.InvoiceNo = sInvoiceno;

                            //get invoice details
                            string SQL4 = "SELECT * FROM  AR_InvoiceHistoryDetail where InvoiceNo=?";
                            OdbcCommand CMD4 = new OdbcCommand(SQL4, (OdbcConnection)DbConnection);
                            adapter.SelectCommand = CMD4;
                            //Add Parameters and set values.  
                            CMD4.Parameters.Add("@invno", OdbcType.VarChar).Value = sInvoiceno;
                            adapter.Fill(ds, "arinvhistdetail");
                            DataTable dtinvdetail = ds.Tables[2];

                            decimal invqtyOrdered = 0m;
                            decimal invTotalOrdered = 0m;
                            decimal invTotalShipped = 0m;
                            decimal invqtyShipped = 0m;

                            if (dtinvdetail.Rows.Count > 0)
                            {
                                

                                foreach (DataRow invdetailrow in dtinvdetail.Rows)
                                {
                                   

                                    invqtyOrdered = invdetailrow.Field<decimal>("QuantityOrdered");
                                    invTotalOrdered = invTotalOrdered + Convert.ToInt32(invqtyOrdered);
                                    invInfo.QuantityOrdered = invTotalOrdered;
                                    invqtyShipped = invdetailrow.Field<decimal>("QuantityShipped");
                                    invTotalShipped = invTotalShipped + Convert.ToInt32(invqtyShipped);
                                    invInfo.QuantityShipped = invTotalShipped;

                                    string itemid = invdetailrow.Field<string>("itemcode");
                                    string uom = invdetailrow.Field<string>("UnitOfMeasure");


                                    string SQL5 = "SELECT * FROM  AR_TrackingByItemHistory where InvoiceNo=? and ItemCode=? ";
                                    OdbcCommand CMD5 = new OdbcCommand(SQL5, (OdbcConnection)DbConnection);
                                    adapter.SelectCommand = CMD5;
                                    //Add Parameters and set values.  
                                    CMD5.Parameters.Add("@invno", OdbcType.VarChar).Value = sInvoiceno;
                                    CMD5.Parameters.Add("@itemno", OdbcType.VarChar).Value = itemid;
                                    //CMD5.Parameters.Add("@packno", OdbcType.VarChar).Value = spackageno;
                                    adapter.Fill(ds, "artrackingpackage1");
                                    DataTable dttrackitem = ds.Tables[3];


                                    foreach (DataRow drtrack in dttrackitem.Rows)
                                    {

                                        ShipmentTrackingDetails std = new ShipmentTrackingDetails();
                                        std.ItemCode = itemid;
                                        std.UnitOfMeasure = uom;
                                        std.PackageNo = drtrack.Field<string>("PackageNo");
                                        std.Quantity = drtrack.Field<decimal>("Quantity");
                                        //std.TrackingID = dtinvtracking.Rows[0].Field<string>("TrackingID");
                                        invInfo.ShipmentTrackingNos.Add(std);

                                    }
                                    dttrackitem.Clear();

                                   


                                }//adding for-each 
                                if ((invTotalShipped > 0))
                                {
                                    if (invTotalOrdered > invTotalShipped)
                                    {
                                        invInfo.ShipStatus = "Partially Shipped";

                                    }
                                    else if (invTotalOrdered == invTotalShipped)
                                    {
                                        invInfo.ShipStatus = "Shipped";
                                    }
                                }

                                //this table holds tracking nos
                                string SQL6 = "SELECT * FROM  AR_InvoiceHistoryTracking where InvoiceNo=?";
                                OdbcCommand CMD6 = new OdbcCommand(SQL6, (OdbcConnection)DbConnection);
                                adapter.SelectCommand = CMD6;
                                //Add Parameters and set values.  
                                CMD6.Parameters.Add("@invno", OdbcType.VarChar).Value = sInvoiceno;
                                adapter.Fill(ds, "shipmentpackage");
                                DataTable dtshiptracking = ds.Tables[4];
                                foreach (DataRow drtrackship in dtshiptracking.Rows)
                                {
                                    string s = drtrackship.Field<string>("PackageNo") + "-" + drtrackship.Field<string>("TrackingID");
                                    trackingnos.Add(s);
                                }
                                var shipresult = String.Join(", ", trackingnos.ToArray());
                                invInfo.TrackingID = shipresult;

                            }  // if loop


                            var InvNoresult = String.Join(", ", invoicenolist.ToArray());
                            os.InvoiceNo = InvNoresult;  //appended invoice nos
                            os.InvoiceDetails.Add(invInfo);
                        }
                        



                    }




                    return os;

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                finally
                {

                    DbConnection.Close();

                }

                return null;

            }



        }



        public Orderlist1 CreateMagentoOrder(Orderlist1 model)
        {
            string homePath = this.Configuration.GetSection("AppSettings")["HomePath"];
            string companyCode = this.Configuration.GetSection("AppSettings")["CompanyCode"];
            string Username = this.Configuration.GetSection("AppSettings")["UserName"];
            string password = this.Configuration.GetSection("AppSettings")["password"];
            string accDate = DateTime.Now.ToString("yyyyMMdd");
            Orderlist1 ordcreate = new Orderlist1();

            //Create shipcode in SO_ShipToAddress_BUS or SVC


            using (DispatchObject pvx1 = new DispatchObject("ProvideX.Script"))
            {
                pvx1.InvokeMethod("Init", homePath);
                using (DispatchObject oSS1 = new DispatchObject(pvx1.InvokeMethod("NewObject", "SY_Session")))
                {

                    Logger.LogDebug("Inside dispatchobj:" + oSS1);
                    oSS1.InvokeMethod("nLogon"); //If the Logon method returns 0 then you must use the SetUser method to allow access
                    oSS1.InvokeMethod("nSetUser", new object[] { Username, password }); //returns 1
                    oSS1.InvokeMethod("nSetCompany", companyCode);//returns 1 successful
                    oSS1.InvokeMethod("nSetDate", "S/O", accDate);
                    oSS1.InvokeMethod("nSetModule", "S/O"); //returns 1 successful
                    int TaskID = (int)oSS1.InvokeMethod("nLookupTask", "SO_SalesOrder_ui");
                    oSS1.InvokeMethod("nSetProgram", TaskID);
                    using (DispatchObject so_order = new DispatchObject(pvx1.InvokeMethod("NewObject", "SO_SalesOrder_bus", oSS1.GetObject()))) //Error 200 throw here
                    {
                        try
                        {
                            string strSalesOrderNo = string.Empty;
                            object[] nxtOrderNo = new object[] { strSalesOrderNo };
                            var retVal = so_order.InvokeMethodByRef("nGetNextSalesOrderNo", nxtOrderNo);
                            Logger.LogDebug("Retval after nextorderno function" + retVal);
                            strSalesOrderNo = nxtOrderNo[0].ToString();
                            Logger.LogDebug("OrderNo:+strSalesOrderNo");
                            model.SalesOrderNo = strSalesOrderNo;
                            so_order.InvokeMethod("nSetKeyValue", "SalesOrderNo$", strSalesOrderNo);
                            so_order.InvokeMethod("nSetKey");
                            so_order.InvokeMethod("nSetValue", "ARDivisionNo$", model.ARDivisionNo);
                            so_order.InvokeMethod("nSetValue", "CustomerNo$", model.CustomerNo);
                            so_order.InvokeMethod("nSetValue", "TaxSchedule$", model.TaxSchedule);
                            so_order.InvokeMethod("nSetValue", "OrderDate$", model.OrderDate.ToString("yyyyMMdd"));
                            so_order.InvokeMethod("nSetValue", "ShipExpireDate$", model.ShipExpireDate.ToString("yyyyMMdd"));
                            so_order.InvokeMethod("nSetValue", "OrderType$", model.OrderType);
                            so_order.InvokeMethod("nSetValue", "OrderStatus$", model.OrderStatus);
                            so_order.InvokeMethod("nSetValue", "CustomerPONo$", model.CustomerPONo.ToUpper());
                            so_order.InvokeMethod("nSetValue", "SalespersonNo$", model.SalespersonNo);
                            so_order.InvokeMethod("nSetValue", "ConfirmTo$", model.ConfirmTo.ToUpper());  
                            //so_order.InvokeMethod("nSetValue", "Comment$", model.Comment);
                            so_order.InvokeMethod("nSetValue", "TermsCode$", model.TermsCode);
                            so_order.InvokeMethod("nSetValue", "ShipToName$", model.ShipToName.ToUpper());
                            so_order.InvokeMethod("nSetValue", "ShipToAddress1$", model.ShipToAddress1.ToUpper());
                            so_order.InvokeMethod("nSetValue", "ShipToAddress2$", model.ShipToAddress2.ToUpper());
                            so_order.InvokeMethod("nSetValue", "ShipToAddress3$", model.ShipToAddress3.ToUpper());
                            so_order.InvokeMethod("nSetValue", "ShipToCity$", model.ShipToCity.ToUpper());
                            so_order.InvokeMethod("nSetValue", "ShipToState$", model.ShipToState.ToUpper());
                            so_order.InvokeMethod("nSetValue", "ShipToZipCode$", model.ShipToZipCode.ToUpper());
                            so_order.InvokeMethod("nSetValue", "WarehouseCode$", model.WarehouseCode);
                            so_order.InvokeMethod("nSetValue", "ShipVia$", model.ShipVia);
                            //so_order.InvokeMethod("nSetValue", "SalesTaxAmt", model.SalesTaxAmt);
                            so_order.InvokeMethod("nSetValue", "FreightAmt", model.FreightAmt);

                            int icount = model.Itemlist.Count();
                            Logger.LogDebug("Count:" + icount);
                            // var items= ordcreate.Itemlist;
                            foreach (var items in model.Itemlist)
                            {


                                using (DispatchObject so_orderlines = new DispatchObject(so_order.GetProperty("oLines")))
                                {

                                    so_orderlines.InvokeMethod("nAddLine");
                                    so_orderlines.InvokeMethod("nSetValue", "ItemCode$", items.ItemCode);
                                    so_orderlines.InvokeMethod("nSetValue", "ItemType$", items.ItemType);
                                    if(!string.IsNullOrEmpty(items.LineNotes))
                                    {
                                        so_orderlines.InvokeMethod("nSetValue", "CommentText$", items.LineNotes.ToUpper());  //2048  chars
                                    }
                                    //so_orderlines.InvokeMethod("nSetValue", "CommentText$", items.LineNotes);  //2048  chars
                                    so_orderlines.InvokeMethod("nSetValue", "QuantityOrdered", items.QuantityOrdered);
                                    so_orderlines.InvokeMethod("nSetValue", "UnitPrice", items.UnitPrice);
                                    decimal extamount = 0M;
                                    if (items.ItemCode == "/DISCOUNT")
                                    {
                                        extamount = items.UnitPrice;
                                    }
                                    else
                                    {
                                        extamount = Math.Round(items.QuantityOrdered * items.UnitPrice, 2);
                                    }

                                    items.ExtensionAmt = extamount;
                                    so_orderlines.InvokeMethod("nSetValue", "ExtensionAmt", extamount);
                                    //so_orderlines.InvokeMethod("nSetValue", "Discount$", items.Discount);
                                    so_orderlines.InvokeMethod("nSetValue", "CostOfGoodsSoldAcctKey$", items.CostOfGoodsSoldAcctKey);
                                    so_orderlines.InvokeMethod("nSetValue", "SalesAcctKey$", items.SalesAcctKey);
                                    retVal = so_orderlines.InvokeMethod("nWrite");

                                }

                            }
                            retVal = so_order.InvokeMethod("nWrite");
                        }
                        catch (Exception ex)
                        {
                            Logger.LogDebug(ex.Message);
                        }
                        finally
                        {
                            so_order.Dispose();
                        }

                    }

                    oSS1.Dispose();
                }

                pvx1.Dispose();
            }

            return model;

        }


        public OrderStatus GetOrderInfo(string OrderNumber)
        {

            using (var DbConnection = DbConn.GetodbcDbConnection())
            {
                try
                {

                    DbConnection.Open();
                    OdbcDataAdapter adapter = new OdbcDataAdapter();
                    DataSet ds = new DataSet();
                    string sInvoiceno = string.Empty;
                    decimal qtyOrdered = 0M;
                    int totalQtyOrdered = 0;
                    int totalQtyShipped = 0;
                    decimal qtyShipped = 0M;
                    //decimal qtyBackOrdered = 0M;
                    List<string> invoicenolist = new List<string>();
                    List<string> trackingnos = new List<string>();
                    OrderStatus os = new OrderStatus();

                    //get salesorder status from salesorder recap
                    string SQL = "SELECT * FROM  SO_SalesOrderRecap where  SalesOrderNo=? ";
                    OdbcCommand CMD = new OdbcCommand(SQL, (OdbcConnection)DbConnection);
                    adapter.SelectCommand = CMD;
                   // CMD.Parameters.Add("@custno", OdbcType.VarChar).Value = param.CustomerNo;
                    CMD.Parameters.Add("@sorder", OdbcType.VarChar).Value = OrderNumber;
                    //create a dataset and fill it
                    Logger.LogDebug("SO_SalesOrderRecap" + OrderNumber);
                    adapter.Fill(ds, "OrderRecap");
                    DataTable dtrecap = ds.Tables[0];
                    if (dtrecap.Rows.Count <= 0) { return null; }
                    else
                    {
                        foreach (DataRow row in dtrecap.Rows)
                        {
                            os.Orderstatus = row["Orderstatus"].ToString();
                            os.OrderType = row["OrderType"].ToString();
                            os.DateUpdated = (DateTime)row["DateUpdated"];
                            os.TimeUpdated = row["TimeUpdated"].ToString();
                            os.SalesOrderNo = OrderNumber;
                        }
                    }



                    //Check for multiple invoice nos 
                    //this table will have data only if it is invoiced
                    string SQL1 = "SELECT * FROM  AR_InvoiceHistoryHeader where SalesOrderNo=? ";
                    OdbcCommand CMD1 = new OdbcCommand(SQL1, (OdbcConnection)DbConnection);
                    adapter.SelectCommand = CMD1;
                    CMD1.Parameters.Add("@sorder", OdbcType.VarChar).Value = OrderNumber;
                    adapter.Fill(ds, "arInvhistOrder");
                    DataTable dtinvheader = ds.Tables[1];

                    if (dtinvheader.Rows.Count > 0)
                    {

                        Logger.LogDebug("dtinvheader.Rows.Count > 0");

                        foreach (DataRow invrow in dtinvheader.Rows)
                        {
                            InvoiceInfo invInfo = new InvoiceInfo();
                            invoicenolist.Add(invrow.Field<string>("InvoiceNo"));
                            sInvoiceno = invrow.Field<string>("InvoiceNo");

                            invInfo.InvoiceNo = sInvoiceno;
                            Logger.LogDebug("Invoice number" + sInvoiceno);
                            //get invoice details
                            string SQL4 = "SELECT * FROM  AR_InvoiceHistoryDetail where InvoiceNo=?";
                            OdbcCommand CMD4 = new OdbcCommand(SQL4, (OdbcConnection)DbConnection);
                            adapter.SelectCommand = CMD4;
                            //Add Parameters and set values.  
                            CMD4.Parameters.Add("@invno", OdbcType.VarChar).Value = sInvoiceno;
                            adapter.Fill(ds, "arinvhistdetail");
                            DataTable dtinvdetail = ds.Tables[2];

                            decimal invqtyOrdered = 0m;
                            decimal invTotalOrdered = 0m;
                            decimal invTotalShipped = 0m;
                            decimal invqtyShipped = 0m;

                            if (dtinvdetail.Rows.Count > 0)
                            {
                                foreach (DataRow invdetailrow in dtinvdetail.Rows)
                                {
                                    Logger.LogDebug("Invoice numberinside foreach" + sInvoiceno);
                                    Logger.LogDebug("numbers" + invTotalOrdered +"number2"+ invTotalShipped);
                                    invqtyOrdered += invdetailrow.Field<decimal>("QuantityOrdered");
                                    Logger.LogDebug("qtyO" + invqtyOrdered);
                                    invTotalOrdered =  Convert.ToInt32(invqtyOrdered);
                                    invInfo.QuantityOrdered = invTotalOrdered;
                                    invqtyShipped += invdetailrow.Field<decimal>("QuantityShipped");
                                    Logger.LogDebug("qtyS" + invqtyShipped);
                                    invTotalShipped =  Convert.ToInt32(invqtyShipped);
                                    invInfo.QuantityShipped = invTotalShipped;

                                    string itemid = invdetailrow.Field<string>("itemcode");
                                    string uom = invdetailrow.Field<string>("UnitOfMeasure");


                                    string SQL5 = "SELECT * FROM  AR_TrackingByItemHistory where InvoiceNo=? and ItemCode=? ";
                                    OdbcCommand CMD5 = new OdbcCommand(SQL5, (OdbcConnection)DbConnection);
                                    adapter.SelectCommand = CMD5;
                                    //Add Parameters and set values.  
                                    CMD5.Parameters.Add("@invno", OdbcType.VarChar).Value = sInvoiceno;
                                    CMD5.Parameters.Add("@itemno", OdbcType.VarChar).Value = itemid;
                                    //CMD5.Parameters.Add("@packno", OdbcType.VarChar).Value = spackageno;
                                    adapter.Fill(ds, "artrackingpackage1");
                                    DataTable dttrackitem = ds.Tables[3];


                                    foreach (DataRow drtrack in dttrackitem.Rows)
                                    {

                                        ShipmentTrackingDetails std = new ShipmentTrackingDetails();
                                        std.ItemCode = itemid;
                                        std.UnitOfMeasure = uom;
                                        std.PackageNo = drtrack.Field<string>("PackageNo");
                                        std.Quantity = drtrack.Field<decimal>("Quantity");
                                        //std.TrackingID = dtinvtracking.Rows[0].Field<string>("TrackingID");
                                        Logger.LogDebug("package number:" + std.PackageNo+"item id:"+ itemid);
                                        invInfo.ShipmentTrackingNos.Add(std);

                                    }
                                    dttrackitem.Clear();

                                    
                                }//adding for-each 
                                if ((invTotalShipped > 0))
                                {
                                    if (invTotalOrdered > invTotalShipped)
                                    {
                                        invInfo.ShipStatus = "Partially Shipped";

                                    }
                                    else if (invTotalOrdered == invTotalShipped)
                                    {
                                        invInfo.ShipStatus = "Shipped";
                                    }
                                }
                                dtinvdetail.Clear();  //cleared the dt bcoz of data not getting cleared


                                //this table holds tracking nos
                                string SQL6 = "SELECT * FROM  AR_InvoiceHistoryTracking where InvoiceNo=?";
                                OdbcCommand CMD6 = new OdbcCommand(SQL6, (OdbcConnection)DbConnection);
                                adapter.SelectCommand = CMD6;
                                //Add Parameters and set values.  
                                CMD6.Parameters.Add("@invno", OdbcType.VarChar).Value = sInvoiceno;
                                adapter.Fill(ds, "shipmentpackage");
                                DataTable dtshiptracking = ds.Tables[4];
                                foreach (DataRow drtrackship in dtshiptracking.Rows)
                                {
                                    string s = drtrackship.Field<string>("PackageNo") + "-" + drtrackship.Field<string>("TrackingID");
                                    trackingnos.Add(s);
                                    invInfo.TrackingID = s;
                                }
                                //var shipresult = String.Join(", ", trackingnos.ToArray());
                                //invInfo.TrackingID = shipresult;
                                

                            }  // if loop


                            var InvNoresult = String.Join(", ", invoicenolist.ToArray());
                            os.InvoiceNo = InvNoresult;  //appended invoice nos
                            os.InvoiceDetails.Add(invInfo);
                            
                        }
                        



                    }

                    return os;

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                finally
                {

                    DbConnection.Close();

                }

                return null;

            }



        }

        //This API fetches TOP 1 salesperson no and tax schedule from ar-customer
        public SalespersonTaxSchedule GetSalespersonandTaxInfo(CustomerDetailsParam model)
        {

            using (var DbConnection = DbConn.GetodbcDbConnection())
            {
                try
                {

                    DbConnection.Open();
                    OdbcDataAdapter adapter = new OdbcDataAdapter();
                    DataSet ds = new DataSet();

                    SalespersonTaxSchedule os = new SalespersonTaxSchedule();

                    //get tax schedule from AR_Customer 
                    string SQL = "select * from AR_Customer  where CustomerNo= ? and ARDivisionNo= ?";
                    OdbcCommand CMD = new OdbcCommand(SQL, (OdbcConnection)DbConnection);
                    adapter.SelectCommand = CMD;
                    CMD.Parameters.Add("@custno", OdbcType.VarChar).Value = model.CustomerNo.ToUpper();
                    CMD.Parameters.Add("@ardiv", OdbcType.VarChar).Value = model.ARDivisionNo;

                    //create a dataset and fill it

                    adapter.Fill(ds, "ArCustomer");
                    DataTable dtcust = ds.Tables[0];
                    if (dtcust.Rows.Count <= 0) { return null; }
                    else
                    {
                        foreach (DataRow row in dtcust.Rows)
                        {
                            os.Customer_TaxSchedule = row["TaxSchedule"].ToString();
                        }
                    }

                    //Get Salesperson number from so_shipping address table
                    string SQL2 = "select * from SO_ShipToAddress where " +
                " CustomerNo='" + model.CustomerNo + "' and (ShipToCity='" + model.ShipToCity + "' and ShipToState='" + model.ShipToState + "' and ShipToZipCode='" + model.ShipToZipCode + "')";
                    OdbcCommand CMD2 = new OdbcCommand(SQL2, (OdbcConnection)DbConnection);
                    adapter.SelectCommand = CMD2;
                   // CMD2.Parameters.Add("@sorder", OdbcType.VarChar).Value = param.SalesOrderNo;
                    adapter.Fill(ds, "Shippingaddress");
                    DataTable dtShipaddr = ds.Tables[1];

                    Logger.LogDebug("SO_ShippingAddress:" + JsonConvert.SerializeObject(dtShipaddr));


                    if (dtShipaddr.Rows.Count > 0)
                    {
                        Logger.LogDebug("dtShipaddr.Rows.Count > 0");
                        foreach (DataRow dr in dtShipaddr.Rows)
                        {
                            os.SalespersonNo = dr["SalespersonNo"].ToString();
                            os.ShipTo_TaxSchedule = dr["TaxSchedule"].ToString();
                            break;

                        }
                    }


                    return os;

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                finally
                {

                    DbConnection.Close();

                }

                return null;

            }



        }

        public bool CreateSalesTax(string TaxSchedule, string order_no, decimal SalesTaxAmt)
        {
            string homePath = this.Configuration.GetSection("AppSettings")["HomePath"];
            string companyCode = this.Configuration.GetSection("AppSettings")["CompanyCode"];
            string Username = this.Configuration.GetSection("AppSettings")["UserName"];
            string password = this.Configuration.GetSection("AppSettings")["password"];
            string accDate = DateTime.Now.ToString("yyyyMMdd");
            //Orderlist1 ordcreate = new Orderlist1();

            bool flag_check = false;
            Logger.LogDebug("1");


            using (DispatchObject pvx1 = new DispatchObject("ProvideX.Script"))
            {
                pvx1.InvokeMethod("Init", homePath);
                using (DispatchObject oSS1 = new DispatchObject(pvx1.InvokeMethod("NewObject", "SY_Session")))
                {
                    Logger.LogDebug("2");
                    Logger.LogDebug("Inside dispatchobj:" + oSS1);
                    oSS1.InvokeMethod("nLogon"); //If the Logon method returns 0 then you must use the SetUser method to allow access
                    oSS1.InvokeMethod("nSetUser", new object[] { Username, password }); //returns 1
                    oSS1.InvokeMethod("nSetCompany", companyCode);//returns 1 successful
                    oSS1.InvokeMethod("nSetDate", "S/O", accDate);
                    oSS1.InvokeMethod("nSetModule", "S/O"); //returns 1 successful
                    int TaskID = (int)oSS1.InvokeMethod("nLookupTask", "SO_SalesOrder_ui");
                    oSS1.InvokeMethod("nSetProgram", TaskID);
                    using (DispatchObject so_order = new DispatchObject(pvx1.InvokeMethod("NewObject", "SO_SalesOrderTaxSummary_bus", oSS1.GetObject()))) //Error 200 throw here
                    {
                        try
                        {
                            Logger.LogDebug("3");

                            string strSeqNo = "1";
                            so_order.InvokeMethod("nSetKeyValue", "SalesOrderNo$", order_no);
                            so_order.InvokeMethod("nSetKeyValue", "ScheduleSeqNo$", strSeqNo);
                            so_order.InvokeMethod("nSetKeyValue", "TaxCode$", TaxSchedule);
                            so_order.InvokeMethod("nSetKey");
                            Logger.LogDebug("4");
                            so_order.InvokeMethod("nSetValue", "SalesTaxAmt", SalesTaxAmt);
                           

                            var retVal = so_order.InvokeMethod("nWrite");
                            Logger.LogDebug("5");
                            //if ((int)(retVal == 1))
                            //{
                                Logger.LogDebug("6");
                                flag_check = true;
                            //}
                        }
                        catch (Exception ex)
                        {
                            Logger.LogDebug(ex.Message);
                        }
                        finally
                        {
                            so_order.Dispose();
                        }

                    }

                    oSS1.Dispose();
                }

                pvx1.Dispose();
            }

            return flag_check;

        }

        public IEnumerable<OrderDetails> FindOrders(OrdersFilterParam model)
        {

            using (var dbConnection = DbConn.GetodbcDbConnection())
            {
                try
                {
                    dbConnection.Open();
                    // Prepare filters
                    var date = model.DateUpdated.ToString("yyyy-MM-dd");
                    date = date + "T00:00:00";
                    var time = model.DateUpdated.ToString("HH:mm:ss");
                    Logger.LogDebug($"Input time-products: {time}");

                    decimal decimalTime = ConvertToDecimalTime(time);
                    Logger.LogDebug($"Decimal time-products: {decimalTime}");
                    int currentIndex = model.CurrentIndex;
                    int pageSize = model.PageSize;
                    // Fetch customers
                    var orders = GetOrderbyDLM((OdbcConnection)dbConnection, date, decimalTime, currentIndex, pageSize, out int totalRecords);
                    if (orders == null || !orders.Any())
                    {
                        return null;
                    }

                    // Fetch and map order line details
                    foreach (var ord in orders)
                    {
                        ord.LineDetails = GetOrderLineDetails((OdbcConnection)dbConnection, ord.SalesOrderNo);
                        //Logger.LogDebug("into invoices");
                        //ord.Invoices = GetInvoices((OdbcConnection)dbConnection, ord.SalesOrderNo);
                        //Logger.LogDebug("outside invoices");
                        ord.TotalRecords = totalRecords;
                    }

                    return orders;

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                    return null;
                }
                finally
                {
                    dbConnection.Close();
                }

            }

            return null;
        }


        public IEnumerable<SOInvoice> FindInvoices(OrdersFilterParam model)
        {
            Logger.LogDebug("starting");
            using (var dbConnection = DbConn.GetodbcDbConnection())
            {
                try
                {
                    dbConnection.Open();
                    // Prepare filters
                    var date = model.DateUpdated.ToString("yyyy-MM-dd");
                    date = date + "T00:00:00";
                    var time = model.DateUpdated.ToString("HH:mm:ss");
                    Logger.LogDebug($"Input time-products: {time}");

                    decimal decimalTime = ConvertToDecimalTime(time);
                    Logger.LogDebug($"Decimal time-products: {decimalTime}");
                    int currentIndex = model.CurrentIndex;
                    int pageSize = model.PageSize;
                    Logger.LogDebug("step 2");
                    // Fetch invoice
                    var invoice = GetInvoicebyDLM((OdbcConnection)dbConnection, date, decimalTime, currentIndex, pageSize, out int totalRecords);
                    if (invoice == null || !invoice.Any())
                    {
                        Logger.LogDebug("invoice is null");
                        return null;
                    }

                    
                    // Fetch and map inv line details
                    foreach (var inv in invoice)
                    {
                        if(!string.IsNullOrEmpty(inv.RMANo))
                        {
                            //inv.invoiceappliedSO = GetInvoiceappliedSO((OdbcConnection)dbConnection, inv.InvoiceNo);
                            inv.RMA = GetRMAHeader((OdbcConnection)dbConnection, inv.InvoiceNo,inv.RMANo);
                        }
                        //inv.InvoiceLines = GetInvoiceLines((OdbcConnection)dbConnection, inv.InvoiceNo);                       
                        inv.TotalRecords = totalRecords;
                    }

                    return invoice;

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                    return null;
                }
                finally
                {
                    dbConnection.Close();
                }

            }

            return null;
        }

        private List<salesorderdetails> GetInvoiceappliedSO(OdbcConnection connection, string InvoiceNo)
        {
            var so = new List<salesorderdetails>();


            using (var adapter = new OdbcDataAdapter())
            {
                var query = "SELECT * FROM  SO_DailyShipment where InvoiceNo=? ";
                using (var command = new OdbcCommand(query, connection))
                {
                    command.CommandTimeout = 120;
                    command.Parameters.Add("@inv", OdbcType.VarChar).Value = InvoiceNo;

                    adapter.SelectCommand = command;

                    var dataSet = new DataSet();
                    adapter.Fill(dataSet, "so");

                    var dataTable = dataSet.Tables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            salesorderdetails s = new salesorderdetails();

                           
                            s.SalesOrderNo = row.Field<string>("SalesOrderNo");
                            s.InvoiceNo = row.Field<string>("InvoiceNo");

                            Logger.LogDebug("SO:");
                            so.Add(s);
                        }
                    }
                }
            }

            return so;
        }
        private List<RMAHeader> GetRMAHeader(OdbcConnection connection, string InvoiceNo,string RMANo)
        {
            var rma = new List<RMAHeader>();


            using (var adapter = new OdbcDataAdapter())
            {
                var query = "SELECT * FROM  RA_ReceiptsHistoryHeader where RMANo=? ";
                using (var command = new OdbcCommand(query, connection))
                {
                    command.CommandTimeout = 120;
                    command.Parameters.Add("@rma", OdbcType.VarChar).Value = RMANo;

                    adapter.SelectCommand = command;

                    var dataSet = new DataSet();
                    adapter.Fill(dataSet, "line");

                    var dataTable = dataSet.Tables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            RMAHeader s = new RMAHeader();

                            s.RMANo = row.Field<string>("RMANo");
                            s.ReturnShipVia = row.Field<string>("ReturnShipVia");
                            s.CustomerNo = row.Field<string>("CustomerNo");
                            s.RMAStatus = row.Field<string>("RMAStatus");
                            s.CrossShip = row.Field<string>("CrossShip");
                            s.RMADate = row.Field<DateTime?>("RMADate");
                            s.FaxNo = row.Field<string>("FaxNo");
                            s.ReturnToName = row.Field<string>("ReturnToName");
                            s.ReturnToAddress1 = row.Field<string>("ReturnToAddress1");
                            s.ReturnToAddress2 = row.Field<string>("ReturnToAddress2");
                            s.ReturnToAddress3 = row.Field<string>("ReturnToAddress3");
                            s.ReturnToCity = row.Field<string>("ReturnToCity");
                            s.ReturnToState = row.Field<string>("ReturnToState");
                            s.ReturnToZipCode = row.Field<string>("ReturnToZipCode");
                            s.ReturnToCountryCode = row.Field<string>("ReturnToCountryCode");
                            
                            // s.DateUpdated = row.Field<DateTime>("DateUpdated");
                            //s.TimeUpdated = row.Field<string>("TimeUpdated");
                            Logger.LogDebug("RAdetails:");
                            rma.Add(s);
                        }
                    }
                }
            }

            return rma;
        }


        private List<SOInvoice> GetInvoicebyDLM(OdbcConnection connection, string date, decimal time, int currentIndex, int pageSize, out int totalRecords)
        {
            var invoice = new List<SOInvoice>();
            string inv_type = "IN";
            string ModuleCode = "S/O";
            totalRecords = GetInvoiceTotalRecordCount(connection, date, time, inv_type, ModuleCode);
            Logger.LogDebug("step 3");
            
            using (var adapter = new OdbcDataAdapter())
            {
                var query = "SELECT * FROM  AR_InvoiceHistoryHeader  where DateUpdated >=? and TimeUpdated > ? and InvoiceType = '" + inv_type + "'  and ModuleCode = '" + ModuleCode + "' ";
                using (var command = new OdbcCommand(query, connection))
                {
                    command.CommandTimeout = 300;

                    command.Parameters.Add("@date", OdbcType.DateTime).Value = date;
                    command.Parameters.Add("@time", OdbcType.Double).Value = time;
                    adapter.SelectCommand = command;
                    Logger.LogDebug("step 4");
                    var dataSet = new DataSet();
                    adapter.Fill(dataSet, currentIndex, pageSize, "Inv");

                    var dataTable = dataSet.Tables[0];
                    Logger.LogDebug("step 5");
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            Logger.LogDebug("step 6");
                            SOInvoice s = new SOInvoice();

                            s.InvoiceNo = row.Field<string>("InvoiceNo");
                            s.SalesOrderNo = row.Field<string>("SalesOrderNo");
                            s.InvoiceDate = row.Field<DateTime?>("InvoiceDate");
                            s.ModuleCode = row.Field<string>("ModuleCode");
                            s.InvoiceType = row.Field<string>("InvoiceType");
                            s.OrderType = row.Field<string>("OrderType");
                            s.RMANo = row.Field<string>("RMANo");
                            //s.ShipperID = row.Field<int>("ShipperID");
                            s.OrderDate = row.Field<DateTime?>("OrderDate");
                            s.CustomerNo = row.Field<string>("CustomerNo");
                            Logger.LogDebug("invoice:");
                            invoice.Add(s);
                        }
                    }
                }
            }

            return invoice;
        }

        private List<OrderDetails> GetOrderbyDLM(OdbcConnection connection, string date, decimal time, int currentIndex, int pageSize, out int totalRecords)
        {
            var orders = new List<OrderDetails>();
            totalRecords = GetTotalRecordCount(connection, date, time);
            

            using (var adapter = new OdbcDataAdapter())
            {
                var query = "SELECT * FROM  SO_SalesOrderHeader  where DateUpdated >=? and TimeUpdated > ?";
                using (var command = new OdbcCommand(query, connection))
                {
                    command.Parameters.Add("@date", OdbcType.DateTime).Value = date;
                    command.Parameters.Add("@time", OdbcType.Double).Value = time;
                    adapter.SelectCommand = command;

                    var dataSet = new DataSet();
                    adapter.Fill(dataSet, currentIndex, pageSize, "SO");

                    var dataTable = dataSet.Tables[0];
                    decimal taxableamt = 0m;
                    decimal STamt = 0m;
                    decimal NontaxAmt = 0m;
                    decimal freightamt = 0m;
                    decimal discountamt = 0m;
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            OrderDetails c = new OrderDetails();
                         
                            c.ARDivisionNo = row.Field<string>("ARDivisionNo");
                            c.OrderDate = row.Field<DateTime>("OrderDate");
                            c.CustomerNo = row.Field<string>("CustomerNo");
                            c.SalesOrderNo = row.Field<string>("SalesOrderNo");
                            c.OrderType = row.Field<string>("OrderType");
                            c.Comment = row.Field<string>("Comment");
                            c.ConfirmTo = row.Field<string>("ConfirmTo");
                            c.SalespersonNo = row.Field<string>("SalespersonNo");
                            c.TaxSchedule = row.Field<string>("TaxSchedule");
                            c.EmailAddress = row.Field<string>("EmailAddress");
                            c.RMANo = row.Field<string>("RMANo");
                            c.BillToName = row.Field<string>("BillToName");
                            c.BillToAddress1 = row.Field<string>("BillToAddress1");
                            c.BillToAddress2 = row.Field<string>("BillToAddress2");
                            c.BillToAddress3 = row.Field<string>("BillToAddress3");
                            c.BillToCity = row.Field<string>("BillToCity");
                            c.BillToState = row.Field<string>("BillToState");
                            c.BillToZipCode = row.Field<string>("BillToZipCode");
                            c.BillToCountryCode = row.Field<string>("BillToCountryCode");
                            c.ShipToCode = row.Field<string>("ShipToCode");
                            c.ShipToName = row.Field<string>("ShipToName");
                            c.ShipToAddress1 = row.Field<string>("ShipToAddress1");
                            c.ShipToAddress2 = row.Field<string>("ShipToAddress2");
                            c.ShipToAddress3 = row.Field<string>("ShipToAddress3");
                            c.ShipToCity = row.Field<string>("ShipToCity");
                            c.ShipToState = row.Field<string>("ShipToState");
                            c.ShipToZipCode = row.Field<string>("ShipToZipCode");
                            c.ShipToCountryCode = row.Field<string>("ShipToCountryCode");
                            c.ShipExpireDate = row.Field<DateTime>("ShipExpireDate");
                            c.WarehouseCode = row.Field<string>("WarehouseCode");
                            c.DateCreated = row.Field<DateTime>("DateCreated");
                            c.TimeCreated = row.Field<string>("TimeCreated");
                            if (!DBNull.Value.Equals(row["TaxableAmt"]))
                            {
                                c.TaxableAmt = row.Field<Decimal?>("TaxableAmt");
                                taxableamt = (decimal)c.TaxableAmt;
                            }
                            else
                            {
                                c.TaxableAmt = 0M;
                                taxableamt = (decimal)c.TaxableAmt;
                            }

                            if (!DBNull.Value.Equals(row["SalesTaxAmt"]))
                            {
                                c.SalesTaxAmt = row.Field<Decimal?>("SalesTaxAmt");
                                STamt = (decimal)c.SalesTaxAmt;
                            }
                            else
                            {
                                c.SalesTaxAmt = 0M;
                                STamt = (decimal)c.SalesTaxAmt;
                            }
                            if (!DBNull.Value.Equals(row["NonTaxableAmt"]))
                            {
                                c.NonTaxableAmt = row.Field<Decimal?>("NonTaxableAmt");
                                NontaxAmt = (decimal)c.NonTaxableAmt;
                            }
                            else
                            {
                                c.NonTaxableAmt = 0M;
                                NontaxAmt = (decimal)c.NonTaxableAmt;
                            }

                            if (!DBNull.Value.Equals(row["FreightAmt"]))
                            {
                                c.FreightAmt = row.Field<Decimal?>("FreightAmt");
                                freightamt = (decimal)c.FreightAmt;
                            }
                            else
                            {
                                c.FreightAmt = 0M;
                                freightamt = (decimal)c.FreightAmt;
                            }
                            if (!DBNull.Value.Equals(row["DiscountAmt"]))
                            {
                                c.DiscountAmt = row.Field<Decimal?>("DiscountAmt");
                                discountamt = (decimal)c.DiscountAmt;
                            }
                            else
                            {
                                c.DiscountAmt = 0M;
                                discountamt = (decimal)c.DiscountAmt;
                            }
                            c.Amount = taxableamt + STamt + NontaxAmt + freightamt - discountamt;

                            c.DateUpdated = row.Field<DateTime>("DateUpdated");
                            c.TimeUpdated = row.Field<string>("TimeUpdated");
                            c.ContactCode = GetContactID(connection, c.CustomerNo);
                            c.TotalRecords = totalRecords;

                            Logger.LogDebug("order:");

                            orders.Add(c);
                        }
                    }
                }
            }

            return orders;
        }

        private List<SOLineDetail> GetOrderLineDetails(OdbcConnection connection, string salesordNo)
        {
            var line = new List<SOLineDetail>();


            using (var adapter = new OdbcDataAdapter())
            {
                var query = "SELECT * FROM  SO_SalesOrderDetail where SalesOrderNo=? ";
                using (var command = new OdbcCommand(query, connection))
                {
                    command.Parameters.Add("@so", OdbcType.VarChar).Value = salesordNo;

                    adapter.SelectCommand = command;

                    var dataSet = new DataSet();
                    adapter.Fill(dataSet, "line");

                    var dataTable = dataSet.Tables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            SOLineDetail s = new SOLineDetail();

                            s.ItemCode = row.Field<string>("ItemCode");
                            s.ItemType = row.Field<string>("ItemType");
                            s.QuantityOrdered = row.Field<Decimal>("QuantityOrdered");
                            s.UnitPrice = row.Field<Decimal>("UnitPrice");
                            s.QuantityBackordered = row.Field<Decimal>("QuantityBackordered");
                            s.QuantityShipped = row.Field<Decimal>("QuantityShipped");
                            s.UnitOfMeasure = row.Field<string>("UnitOfMeasure");                            
                           // s.DateUpdated = row.Field<DateTime>("DateUpdated");
                            //s.TimeUpdated = row.Field<string>("TimeUpdated");
                            Logger.LogDebug("order line:");
                            line.Add(s);
                        }
                    }
                }
            }

            return line;
        }

        public decimal ConvertToDecimalTime(string time)
        {
            var parts = time.Split(':');
            return Math.Round(Convert.ToDecimal(parts[0]) + (Convert.ToDecimal(parts[1]) / 60) + (Convert.ToDecimal(parts[2]) / 3600), 5);
        }

        private int GetTotalRecordCount(OdbcConnection connection, string date, decimal time)
        {
            using (var command = new OdbcCommand("select Count(*) from SO_SalesOrderHeader  where DateUpdated >=? and TimeUpdated > ?", connection))
            {
                command.Parameters.Add("@date", OdbcType.DateTime).Value = date;
                command.Parameters.Add("@time", OdbcType.Double).Value = time;

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private int GetInvoiceTotalRecordCount(OdbcConnection connection, string date, decimal time,string inv_type,string ModuleCode)
        {
            using (var command = new OdbcCommand("select Count(*) from AR_InvoiceHistoryHeader  where DateUpdated >=? and TimeUpdated > ?  and InvoiceType = '" + inv_type + "'  and ModuleCode = '" + ModuleCode + "' ", connection))
            {
                command.CommandTimeout = 300;
                command.Parameters.Add("@date", OdbcType.DateTime).Value = date;
                command.Parameters.Add("@time", OdbcType.Double).Value = time;

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private string GetContactID(OdbcConnection connection, string customerNo)
        {
            using (var command = new OdbcCommand("select ContactCode from AR_Customer  where CustomerNo =? ", connection))
            {
                command.Parameters.Add("@cust", OdbcType.VarChar).Value = customerNo;
                

                return Convert.ToString(command.ExecuteScalar());
            }
        }

    }
}
