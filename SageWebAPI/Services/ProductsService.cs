using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SageWebAPI.Models;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.Odbc;
using Newtonsoft.Json;

namespace SageWebAPI.Services
{
    public class ProductsService : IProductsService
    {
        protected readonly IDbConnectionService DbConn;
        protected readonly ILogger<ProductsService> Logger;
        private IConfiguration Configuration;
        public ProductsService(ILoggerFactory loggerFactory, IConfiguration _configuration, IDbConnectionService dbConn)
        {
            Logger = loggerFactory.CreateLogger<ProductsService>();
            Configuration = _configuration;
            DbConn = dbConn;

        }
        public ProductExists GetProduct(string ItemCode, string upc_code)
        {
            using (var DbConnection = DbConn.GetodbcDbConnection())
            {
                try
                {
                    DbConnection.Open();                      
                    //OdbcDataAdapter adapter = new OdbcDataAdapter();

                    //UDF_UPC_CODE
                    string selectSQL;
                    if (!string.IsNullOrEmpty(ItemCode) && string.IsNullOrEmpty(upc_code))
                    {
                        selectSQL = "SELECT * FROM  CI_Item where ItemCode='" + ItemCode + "' ";
                        Logger.LogDebug("$1");
                    }
                    else if (!string.IsNullOrEmpty(upc_code) && string.IsNullOrEmpty(ItemCode))
                    {
                        selectSQL = "SELECT * FROM  CI_Item where UDF_UPC_CODE='" + upc_code + "' ";
                        Logger.LogDebug("$2");
                    }
                    else if (!string.IsNullOrEmpty(upc_code) && !string.IsNullOrEmpty(ItemCode))
                    {
                        selectSQL = "SELECT * FROM  CI_Item where ItemCode='" + ItemCode + "' and UDF_UPC_CODE='" + upc_code + "' ";
                        Logger.LogDebug("$3");
                    }
                    else
                    {
                        selectSQL = "SELECT * FROM  CI_Item where ItemCode='" + ItemCode + "' ";
                        Logger.LogDebug("$4");
                    }


                    //string selectSQL = "SELECT * FROM  CI_Item where ItemCode=?";
                    //OdbcCommand selectCMD = new OdbcCommand(selectSQL, (OdbcConnection)DbConnection);
                    //adapter.SelectCommand = selectCMD;

                    //Add Parameters and set values.  
                    // selectCMD.Parameters.Add("@itemcode", OdbcType.NVarChar).Value = ItemCode;

                    OdbcDataAdapter adapter = new OdbcDataAdapter(selectSQL, (OdbcConnection)DbConnection);


                    //create a dataset and fill it
                    DataSet ds = new DataSet();
                    adapter.Fill(ds, "Products");
                    DataTable datbl = ds.Tables[0];
                    ProductExists p = new ProductExists();
                    if (datbl!=null)
                    {
                        if(datbl.Rows.Count > 0)
                        {
                         

                            foreach (DataRow row in datbl.Rows)
                            {
                                p.ItemCode = row.Field<string>("itemcode");
                                p.ItemType = row.Field<string>("ItemType");
                                p.ItemCodeDesc = row.Field<string>("ItemCodeDesc");
                                p.StandardUnitOfMeasure = row.Field<string>("StandardUnitOfMeasure");
                                p.UDF_UPC_CODE = row.Field<string>("UDF_UPC_CODE");

                                p.UDF_GS_WEBSITE_LIVE = row.Field<string>("UDF_GS_WEBSITE_LIVE");

                                p.UDF_NSI_WEBSITE_LIVE = row.Field<string>("UDF_NSI_WEBSITE_LIVE");

                                p.message = "Record Exists";
                                return p;
                            }

                        }
                        else
                        {
                            p.message = "Record does not Exists";
                            return p;
                        }

                    }
                  

                   
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



        public IEnumerable<Inventory> GetInventories(InventoryFilterParam model)
        {

            using (var DbConnection = DbConn.GetodbcDbConnection())
            {
                try
                {
                    DbConnection.Open();
                    IList<Inventory> invList = new List<Inventory>();

                    //date filter
                    DateTime cdt = DateTime.Now;
                    var date = model.DateUpdated.ToString("yyyy-MM-dd");
                    var time = model.DateUpdated.ToString("HH:mm:ss");
                    Logger.LogDebug("input time:" + time);
                    //time filter
                    string[] a = time.Split(new string[] { ":" }, StringSplitOptions.None);
                    //a[0] contains the hours, a[1] contains the minutes
                    decimal dectime = Math.Round(Convert.ToDecimal(a[0]) + (Convert.ToDecimal(a[1]) / 60), 4);
                    Logger.LogDebug("decimal time:"+ dectime);


                    int currentIndex = model.CurrentIndex;
                    int pageSize = model.PageSize;

                    // Assumes that connection is a valid OdbcConnection object.  
                    OdbcDataAdapter adapter = new OdbcDataAdapter();

                    string selectSQL = "SELECT * FROM  CI_Item where DateUpdated>=? and TimeUpdated>=?";
                    OdbcCommand selectCMD = new OdbcCommand(selectSQL, (OdbcConnection)DbConnection);
                    adapter.SelectCommand = selectCMD;

                    //Add Parameters and set values.  
                    selectCMD.Parameters.Add("@date", OdbcType.DateTime).Value = date;
                    selectCMD.Parameters.Add("@time", OdbcType.Double).Value = dectime;

                    //create a dataset and fill it
                    DataSet ds = new DataSet();
                    adapter.Fill(ds, currentIndex, pageSize, "inventory");
                    DataTable datbl = ds.Tables[0];
                    string flag = string.Empty;
                    //int TotalRecords = getCount(date, dectime,flag);


                    Logger.LogDebug(JsonConvert.SerializeObject(datbl));


                    foreach (DataRow row in datbl.Rows)
                    {
                        Inventory p = new Inventory();


                        p.ItemCode = row.Field<string>("itemcode");
                        p.ItemType = row.Field<string>("ItemType");
                        p.ItemCodeDesc = row.Field<string>("ItemCodeDesc");
                        p.StandardUnitOfMeasure = row.Field<string>("StandardUnitOfMeasure");
                        p.DefaultWarehouseCode = row.Field<string>("DefaultWarehouseCode");
                        if (!DBNull.Value.Equals(row["TotalQuantityOnHand"]))
                        { p.TotalQuantityOnHand = (Decimal?)row["TotalQuantityOnHand"]; }
                        else { p.TotalQuantityOnHand = 0M; }
                        p.DateUpdated = row.Field<DateTime>("DateUpdated");
                        p.TimeUpdated = row.Field<string>("TimeUpdated");
                        //p.TotalRecords = TotalRecords;
                        invList.Add(p);

                    }

                    return invList;
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

        public IEnumerable<Inventory> GetInventories1(InventoryFilterParam model)
        {

            using (var DbConnection = DbConn.GetodbcDbConnection())
            {
                try
                {
                    DbConnection.Open();
                    IList<Inventory> invList = new List<Inventory>();

                    //date filter
                    DateTime cdt = DateTime.Now;
                    var date = model.DateUpdated.ToString("yyyy-MM-dd");
                    var time = model.DateUpdated.ToString("HH:mm:ss");
                    Logger.LogDebug("input time-products:" + time);
                    //time filter
                    string[] a = time.Split(new string[] { ":" }, StringSplitOptions.None);
                    //a[0] contains the hours, a[1] contains the minutes
                    decimal dectime = Math.Round(Convert.ToDecimal(a[0]) + (Convert.ToDecimal(a[1]) / 60), 4);
                    Logger.LogDebug("decimal time-products:" + dectime);

                    int currentIndex = model.CurrentIndex;
                    int pageSize = model.PageSize;

                    // Assumes that connection is a valid OdbcConnection object.  
                    OdbcDataAdapter adapter = new OdbcDataAdapter();

                    string selectSQL = "SELECT * FROM  CI_Item where DateUpdated>=? and TimeUpdated>=?";
                    OdbcCommand selectCMD = new OdbcCommand(selectSQL, (OdbcConnection)DbConnection);
                    adapter.SelectCommand = selectCMD;

                    //Add Parameters and set values.  
                    selectCMD.Parameters.Add("@date", OdbcType.DateTime).Value = date;
                    selectCMD.Parameters.Add("@time", OdbcType.Double).Value = dectime;

                    //create a dataset and fill it
                    DataSet ds = new DataSet();
                    adapter.Fill(ds, currentIndex, pageSize, "Products");
                    DataTable datbl = ds.Tables[0];
                    string flag = string.Empty;
                    //int TotalRecords = getCount(date, dectime,flag);
                    foreach (DataRow o in datbl.Select("ItemCode<> ' '"))
                    {
                        string itemid = o["ItemCode"].ToString();
                        string selectSQL1 = "SELECT * FROM  IM_ItemWarehouse where  ItemCode=?";
                        OdbcCommand selectCMD1 = new OdbcCommand(selectSQL1, (OdbcConnection)DbConnection);
                        adapter.SelectCommand = selectCMD1;
                        selectCMD1.Parameters.Add("@itemid", OdbcType.VarChar).Value = itemid;
                        adapter.Fill(ds, "warehouse");
                    }

                    DataTable datbl1 = ds.Tables[1];
                    //datbl.Merge(datbl1);

                    Logger.LogDebug(JsonConvert.SerializeObject(datbl));
                    Logger.LogDebug(JsonConvert.SerializeObject(datbl1));


                    foreach (DataRow row in datbl.Rows)
                    {
                        Inventory p = new Inventory();

                        p.WarehouseDetails = new List<warehouse>();

                        p.ItemCode = row.Field<string>("itemcode");
                        p.ItemType = row.Field<string>("ItemType");
                        p.ItemCodeDesc = row.Field<string>("ItemCodeDesc");
                        p.StandardUnitOfMeasure = row.Field<string>("StandardUnitOfMeasure");
                        p.DefaultWarehouseCode = row.Field<string>("DefaultWarehouseCode");                       
                        if (!DBNull.Value.Equals(row["TotalQuantityOnHand"]))
                        { p.TotalQuantityOnHand = (Decimal?)row["TotalQuantityOnHand"]; }
                        else { p.TotalQuantityOnHand = 0M; }                       
                        
                        p.DateUpdated = row.Field<DateTime>("DateUpdated");
                        p.TimeUpdated = row.Field<string>("TimeUpdated");
                        //p.TotalRecords = TotalRecords;

                        string sItemID = row.Field<string>("itemcode").ToString();
                        Logger.LogDebug("ItemId:" + sItemID);
                        int count = 0;
                        decimal qtyhand = 0m;
                        decimal qtyso = 0m;
                        foreach (DataRow row1 in datbl1.Select().Where(e => e.ItemArray[0].ToString() == sItemID))
                        {
                            warehouse iw = new warehouse();
                            count = count + 1;
                            Logger.LogDebug("ItemId:" + sItemID + "Count:" + count);

                            iw.WarehouseCode = row1.Field<string>("WarehouseCode");

                            //if (!DBNull.Value.Equals(row1["QuantityOnBackOrder"]))
                            //{ iw.QuantityOnBackOrder = row1.Field<Decimal?>("QuantityOnBackOrder"); }
                            //else { iw.QuantityOnBackOrder = 0M; }

                            if (!DBNull.Value.Equals(row1["QuantityOnHand"]))
                            { iw.QuantityOnHand = row1.Field<Decimal?>("QuantityOnHand");
                                qtyhand = (decimal)iw.QuantityOnHand;
                            }
                            else { iw.QuantityOnHand = 0M;
                                qtyhand = (decimal)iw.QuantityOnHand;
                            }

                            if (!DBNull.Value.Equals(row1["QuantityOnSalesOrder"]))
                            { iw.QuantityOnSalesOrder = row1.Field<Decimal?>("QuantityOnSalesOrder");
                                qtyso = (decimal)iw.QuantityOnSalesOrder;
                            }
                            else { iw.QuantityOnSalesOrder = 0M;
                                qtyso = (decimal)iw.QuantityOnSalesOrder;
                            }

                            iw.QuantityAvailable = Convert.ToDecimal(qtyhand- qtyso);

                            //if (!DBNull.Value.Equals(row1["QuantityOnWorkOrder"]))
                            //{ iw.QuantityOnWorkOrder = row1.Field<Decimal?>("QuantityOnWorkOrder"); }
                            //else { iw.QuantityOnWorkOrder = 0M; }

                            //if (!DBNull.Value.Equals(row1["MinimumOrderQty"]))
                            //{ iw.MinimumOrderQty = row1.Field<Decimal?>("MinimumOrderQty"); }
                            //else { iw.MinimumOrderQty = 0M; }

                            //if (!DBNull.Value.Equals(row1["MaximumOnHandQty"]))
                            //{ iw.MaximumOnHandQty = row1.Field<Decimal?>("MaximumOnHandQty"); }
                            //else { iw.MaximumOnHandQty = 0M; }
                            p.WarehouseDetails.Add(iw);
                        }


                        invList.Add(p);

                    }

                    return invList;
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

        //public IEnumerable<Inventory> GetInventoriesNS(InventoryFilterParam model)
        //{

        //    using (var DbConnection = DbConn.GetodbcDbConnection())
        //    {
        //        try
        //        {
        //            DbConnection.Open();
        //            IList<Inventory> invList = new List<Inventory>();

        //            //date filter
        //            DateTime cdt = DateTime.Now;
        //            var date = model.DateUpdated.ToString("yyyy-MM-dd");
        //            var time = model.DateUpdated.ToString("HH:mm:ss");
        //            Logger.LogDebug("input time-products:" + time);
        //            //time filter
        //            string[] a = time.Split(new string[] { ":" }, StringSplitOptions.None);
        //            //a[0] contains the hours, a[1] contains the minutes,a[2] contains seconds
        //            //incluing seconds too in the logic
        //            decimal dectime = Math.Round(Convert.ToDecimal(a[0]) +(Convert.ToDecimal(a[1]) / 60)+(Convert.ToDecimal(a[2]) / 3600), 4);
        //            Logger.LogDebug("decimal time-products:" + dectime);

        //            int currentIndex = model.CurrentIndex;
        //            int pageSize = model.PageSize;
        //            string nsiflag = "Y";
        //            // Assumes that connection is a valid OdbcConnection object.  
        //            OdbcDataAdapter adapter = new OdbcDataAdapter();
        //            string selectSQL = "SELECT * FROM  IM_ItemWarehouse where DateUpdated >=? and TimeUpdated >=? and WarehouseCode='" + model.WarehouseCode + "'";                  
        //            OdbcCommand selectCMD = new OdbcCommand(selectSQL, (OdbcConnection)DbConnection);
        //            adapter.SelectCommand = selectCMD;

        //            //Add Parameters and set values.  
        //            selectCMD.Parameters.Add("@date", OdbcType.DateTime).Value = date;
        //            selectCMD.Parameters.Add("@time", OdbcType.Double).Value = dectime;
        //            //selectCMD.Parameters.Add("@wc", OdbcType.VarChar).Value = model.WarehouseCode;

        //            //create a dataset and fill it
        //            DataSet ds = new DataSet();
        //            adapter.Fill(ds, currentIndex, pageSize, "IM");
        //            DataTable datbl = ds.Tables[0];
        //            //DataTable results = datbl.Select("TimeUpdated >='" + dectime + "'" ).CopyToDataTable();
        //            //Logger.LogDebug("additional filter:"+JsonConvert.SerializeObject(results));
        //            string flag = "NS";
        //            int TR = getTotalCount(date, dectime, flag, model.WarehouseCode);
        //            Logger.LogDebug("testCount:" + TR);
                    
        //            foreach (DataRow o in datbl.Select("ItemCode<> ' '"))
        //            {
        //                string itemid = o["ItemCode"].ToString();
        //                string selectSQL1 = "SELECT * FROM  CI_Item where  ItemCode='" + itemid + "' and UDF_NSI_WEBSITE_LIVE ='" + nsiflag + "' ";
        //                OdbcCommand selectCMD1 = new OdbcCommand(selectSQL1, (OdbcConnection)DbConnection);
        //                adapter.SelectCommand = selectCMD1;
        //                selectCMD1.Parameters.Add("@itemid", OdbcType.VarChar).Value = itemid;
        //                adapter.Fill(ds, "product");

        //            }

        //            DataTable datbl1 = ds.Tables[1];
        //            int numberOfRecords = datbl1.AsEnumerable().Where(x => x["UDF_NSI_WEBSITE_LIVE"].ToString() == "Y").ToList().Count;
        //            int TotalRecords = TR;
        //            Logger.LogDebug("RecordCount based on pagesize:"+ numberOfRecords);

        //           // Logger.LogDebug(JsonConvert.SerializeObject(datbl));
        //            //Logger.LogDebug("CI_Item:"+JsonConvert.SerializeObject(datbl1));


        //            foreach (DataRow row in datbl1.Rows)
        //            {
        //                Inventory p = new Inventory();

        //                p.WarehouseDetails = new List<warehouse>();

        //                p.ItemCode = row.Field<string>("itemcode");
        //                p.ItemType = row.Field<string>("ItemType");
        //                p.ItemCodeDesc = row.Field<string>("ItemCodeDesc");
        //                p.StandardUnitOfMeasure = row.Field<string>("StandardUnitOfMeasure");
        //                p.DefaultWarehouseCode = row.Field<string>("DefaultWarehouseCode");
        //                if (!DBNull.Value.Equals(row["TotalQuantityOnHand"]))
        //                { p.TotalQuantityOnHand = (Decimal?)row["TotalQuantityOnHand"]; }
        //                else { p.TotalQuantityOnHand = 0M; }

        //                p.UDF_UPC_CODE = row.Field<string>("UDF_UPC_CODE");
        //                p.UDF_SKU = row.Field<string>("UDF_SKU");
        //                p.UDF_GS_WEBSITE_LIVE = row.Field<string>("UDF_GS_WEBSITE_LIVE");
        //                p.UDF_NSI_WEBSITE_LIVE = row.Field<string>("UDF_NSI_WEBSITE_LIVE");
        //                p.UDF_MPN = row.Field<string>("UDF_MPN");
        //                p.UDF_WEB_DESCRIPTION = row.Field<string>("UDF_WEB_DESCRIPTION");
        //                p.UDF_WEB_PRODUCT_NAME = row.Field<string>("UDF_WEB_PRODUCT_NAME");
        //                p.DateUpdated = row.Field<DateTime>("DateUpdated");
        //                p.TimeUpdated = row.Field<string>("TimeUpdated");
        //                p.TotalRecords = TotalRecords;

                        

        //                string sItemID = row.Field<string>("itemcode").ToString();
        //                Logger.LogDebug("ItemId:" + sItemID);
        //                //int count = 0;
        //                decimal qtyhand = 0m;
        //                decimal qtyso = 0m;
        //                foreach (DataRow row1 in datbl.Select().Where(e => e.ItemArray[0].ToString() == sItemID))
        //                {
        //                    warehouse iw = new warehouse();
        //                    //count = count + 1;
        //                    //Logger.LogDebug("ItemId:" + sItemID + "Count:" + count);

        //                    iw.WarehouseCode = row1.Field<string>("WarehouseCode");

        //                    //if (!DBNull.Value.Equals(row1["QuantityOnBackOrder"]))
        //                    //{ iw.QuantityOnBackOrder = row1.Field<Decimal?>("QuantityOnBackOrder"); }
        //                    //else { iw.QuantityOnBackOrder = 0M; }

        //                    if (!DBNull.Value.Equals(row1["QuantityOnHand"]))
        //                    {
        //                        iw.QuantityOnHand = row1.Field<Decimal?>("QuantityOnHand");
        //                        qtyhand = (decimal)iw.QuantityOnHand;
        //                    }
        //                    else
        //                    {
        //                        iw.QuantityOnHand = 0M;
        //                        qtyhand = (decimal)iw.QuantityOnHand;
        //                    }

        //                    if (!DBNull.Value.Equals(row1["QuantityOnSalesOrder"]))
        //                    {
        //                        iw.QuantityOnSalesOrder = row1.Field<Decimal?>("QuantityOnSalesOrder");
        //                        qtyso = (decimal)iw.QuantityOnSalesOrder;
        //                    }
        //                    else
        //                    {
        //                        iw.QuantityOnSalesOrder = 0M;
        //                        qtyso = (decimal)iw.QuantityOnSalesOrder;
        //                    }

        //                    iw.QuantityAvailable = Convert.ToDecimal(qtyhand - qtyso);

        //                    //if (!DBNull.Value.Equals(row1["QuantityOnWorkOrder"]))
        //                    //{ iw.QuantityOnWorkOrder = row1.Field<Decimal?>("QuantityOnWorkOrder"); }
        //                    //else { iw.QuantityOnWorkOrder = 0M; }

        //                    //if (!DBNull.Value.Equals(row1["MinimumOrderQty"]))
        //                    //{ iw.MinimumOrderQty = row1.Field<Decimal?>("MinimumOrderQty"); }
        //                    //else { iw.MinimumOrderQty = 0M; }

        //                    //if (!DBNull.Value.Equals(row1["MaximumOnHandQty"]))
        //                    //{ iw.MaximumOnHandQty = row1.Field<Decimal?>("MaximumOnHandQty"); }
        //                    //else { iw.MaximumOnHandQty = 0M; }
        //                    p.WarehouseDetails.Add(iw);
        //                }


        //                invList.Add(p);

        //            }

        //            return invList;
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

        //public IEnumerable<Inventory> GetInventoriesGS(InventoryFilterParam model)
        //{

        //    using (var DbConnection = DbConn.GetodbcDbConnection())
        //    {
        //        try
        //        {
        //            DbConnection.Open();
        //            IList<Inventory> invList = new List<Inventory>();

        //            //date filter
        //            DateTime cdt = DateTime.Now;
        //            var date = model.DateUpdated.ToString("yyyy-MM-dd");
        //            var time = model.DateUpdated.ToString("HH:mm:ss");
        //            Logger.LogDebug("input time-products:" + time);
        //            //time filter
        //            string[] a = time.Split(new string[] { ":" }, StringSplitOptions.None);
        //            //a[0] contains the hours, a[1] contains the minutes,a[2] seconds
        //            // including seconds too in the logic 
        //            decimal dectime = Math.Round(Convert.ToDecimal(a[0]) + (Convert.ToDecimal(a[1]) / 60)+ (Convert.ToDecimal(a[2]) / 3600), 4);
        //            Logger.LogDebug("decimal time-products:" + dectime);

        //            int currentIndex = model.CurrentIndex;
        //            int pageSize = model.PageSize;
        //            string gsiflag = "Y";
        //            // Assumes that connection is a valid OdbcConnection object.  
        //            OdbcDataAdapter adapter = new OdbcDataAdapter();

        //            //Check dateupdated in IM_Itemwarehouse 
        //             string selectSQLIM = "SELECT * FROM  IM_ItemWarehouse where DateUpdated>=? and TimeUpdated>=? and WarehouseCode=?";
        //             OdbcCommand selectCMDIM = new OdbcCommand(selectSQLIM, (OdbcConnection)DbConnection);
        //             adapter.SelectCommand = selectCMDIM;

        //             //Add Parameters and set values.  
        //             selectCMDIM.Parameters.Add("@date", OdbcType.DateTime).Value = date;
        //             selectCMDIM.Parameters.Add("@time", OdbcType.Double).Value = dectime;
        //            selectCMDIM.Parameters.Add("@wc", OdbcType.VarChar).Value = model.WarehouseCode;

        //            //create a dataset and fill it
        //            DataSet ds = new DataSet();
        //            adapter.Fill(ds, currentIndex, pageSize, "IM");
        //            DataTable datbl= ds.Tables[0];
        //            string flag = "GS";
        //            int TR = getTotalCount(date, dectime, flag, model.WarehouseCode);
        //            Logger.LogDebug("totalCount:" + TR);
        //            // Logger.LogDebug("count:" + Count);

        //            foreach (DataRow o in datbl.Select("ItemCode<> ' '"))
        //            {
        //                string itemid = o["ItemCode"].ToString();
        //                string selectSQL1 = "SELECT * FROM  CI_Item where  ItemCode=? and UDF_GS_WEBSITE_LIVE ='" + gsiflag + "' ";
        //                OdbcCommand selectCMD1 = new OdbcCommand(selectSQL1, (OdbcConnection)DbConnection);
        //                adapter.SelectCommand = selectCMD1;
        //                selectCMD1.Parameters.Add("@itemid", OdbcType.VarChar).Value = itemid;
        //                adapter.Fill(ds, "product");         
        //            }
        //            DataTable datbl1 = ds.Tables[1];
        //            int numberOfRecords = datbl1.AsEnumerable().Where(x => x["UDF_GS_WEBSITE_LIVE"].ToString() == "Y").ToList().Count;
        //            int TotalRecords = TR;
        //            Logger.LogDebug("TotalRecords based on pagesize:" + numberOfRecords);
        //            //Logger.LogDebug(JsonConvert.SerializeObject(datbl));
        //            //Logger.LogDebug("CI_Item:"+JsonConvert.SerializeObject(datbl1));


        //            foreach (DataRow row in datbl1.Rows)
        //            {
        //                Inventory p = new Inventory();

        //                p.WarehouseDetails = new List<warehouse>();

        //                p.ItemCode = row.Field<string>("itemcode");
        //                p.ItemType = row.Field<string>("ItemType");
        //                p.ItemCodeDesc = row.Field<string>("ItemCodeDesc");
        //                p.StandardUnitOfMeasure = row.Field<string>("StandardUnitOfMeasure");
        //                p.DefaultWarehouseCode = row.Field<string>("DefaultWarehouseCode");
        //                if (!DBNull.Value.Equals(row["TotalQuantityOnHand"]))
        //                { p.TotalQuantityOnHand = (Decimal?)row["TotalQuantityOnHand"]; }
        //                else { p.TotalQuantityOnHand = 0M; }

        //                p.UDF_UPC_CODE = row.Field<string>("UDF_UPC_CODE");
        //                p.UDF_SKU = row.Field<string>("UDF_SKU");
        //                p.UDF_GS_WEBSITE_LIVE = row.Field<string>("UDF_GS_WEBSITE_LIVE");
        //                p.UDF_NSI_WEBSITE_LIVE = row.Field<string>("UDF_NSI_WEBSITE_LIVE");
        //                p.UDF_MPN = row.Field<string>("UDF_MPN");
        //                p.UDF_WEB_DESCRIPTION = row.Field<string>("UDF_WEB_DESCRIPTION");
        //                p.UDF_WEB_PRODUCT_NAME = row.Field<string>("UDF_WEB_PRODUCT_NAME");
        //                p.DateUpdated = row.Field<DateTime>("DateUpdated");
        //                p.TimeUpdated = row.Field<string>("TimeUpdated");
        //                p.TotalRecords = TotalRecords;

        //                string sItemID = row.Field<string>("itemcode").ToString();
        //                Logger.LogDebug("ItemId:" + sItemID);
                        
        //                decimal qtyhand = 0m;
        //                decimal qtyso = 0m;
        //                foreach (DataRow row1 in datbl.Select().Where(e => e.ItemArray[0].ToString() == sItemID))
        //                {
        //                    warehouse iw = new warehouse();
                           
        //                    Logger.LogDebug("WarehousLogic-ItemId:" + sItemID );

        //                    iw.WarehouseCode = row1.Field<string>("WarehouseCode");

        //                    //if (!DBNull.Value.Equals(row1["QuantityOnBackOrder"]))
        //                    //{ iw.QuantityOnBackOrder = row1.Field<Decimal?>("QuantityOnBackOrder"); }
        //                    //else { iw.QuantityOnBackOrder = 0M; }

        //                    if (!DBNull.Value.Equals(row1["QuantityOnHand"]))
        //                    {
        //                        iw.QuantityOnHand = row1.Field<Decimal?>("QuantityOnHand");
        //                        qtyhand = (decimal)iw.QuantityOnHand;
        //                    }
        //                    else
        //                    {
        //                        iw.QuantityOnHand = 0M;
        //                        qtyhand = (decimal)iw.QuantityOnHand;
        //                    }

        //                    if (!DBNull.Value.Equals(row1["QuantityOnSalesOrder"]))
        //                    {
        //                        iw.QuantityOnSalesOrder = row1.Field<Decimal?>("QuantityOnSalesOrder");
        //                        qtyso = (decimal)iw.QuantityOnSalesOrder;
        //                    }
        //                    else
        //                    {
        //                        iw.QuantityOnSalesOrder = 0M;
        //                        qtyso = (decimal)iw.QuantityOnSalesOrder;
        //                    }

        //                    iw.QuantityAvailable = Convert.ToDecimal(qtyhand - qtyso);

        //                    //if (!DBNull.Value.Equals(row1["QuantityOnWorkOrder"]))
        //                    //{ iw.QuantityOnWorkOrder = row1.Field<Decimal?>("QuantityOnWorkOrder"); }
        //                    //else { iw.QuantityOnWorkOrder = 0M; }

        //                    //if (!DBNull.Value.Equals(row1["MinimumOrderQty"]))
        //                    //{ iw.MinimumOrderQty = row1.Field<Decimal?>("MinimumOrderQty"); }
        //                    //else { iw.MinimumOrderQty = 0M; }

        //                    //if (!DBNull.Value.Equals(row1["MaximumOnHandQty"]))
        //                    //{ iw.MaximumOnHandQty = row1.Field<Decimal?>("MaximumOnHandQty"); }
        //                    //else { iw.MaximumOnHandQty = 0M; }
        //                    p.WarehouseDetails.Add(iw);
        //                }


        //                invList.Add(p);

        //            }

        //            return invList;
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


    
        //public IEnumerable<Product> Find(ProductFilterParam model)
        //{

        //    using (var DbConnection = DbConn.GetodbcDbConnection())
        //    {
        //        try
        //        {
        //            DbConnection.Open();
        //            IList<Product> prodList = new List<Product>();

        //            //date filter
        //            DateTime cdt = DateTime.Now;
        //            var date = model.DateUpdated.ToString("yyyy-MM-dd");
        //            var time = model.DateUpdated.ToString("HH:mm:ss");
        //            Logger.LogDebug("input time-products:" + time);
        //            //time filter
        //            string[] a = time.Split(new string[] { ":" }, StringSplitOptions.None);
        //            //a[0] contains the hours, a[1] contains the minutes
        //            decimal dectime = Math.Round(Convert.ToDecimal(a[0]) + (Convert.ToDecimal(a[1]) / 60), 4);
        //            Logger.LogDebug("decimal time-products:" + dectime);

        //            int currentIndex = model.CurrentIndex;
        //            int pageSize = model.PageSize;

        //            // Assumes that connection is a valid OdbcConnection object.  
        //            OdbcDataAdapter adapter = new OdbcDataAdapter();

        //            //string selectSQL = "SELECT * FROM  CI_Item where DateUpdated>=? and TimeUpdated>?";
        //            string selectSQL = "SELECT * FROM  CI_Item where DateUpdated>=? ";
        //            OdbcCommand selectCMD = new OdbcCommand(selectSQL, (OdbcConnection)DbConnection);
        //            adapter.SelectCommand = selectCMD;

        //            //Add Parameters and set values.  
        //            selectCMD.Parameters.Add("@date", OdbcType.DateTime).Value = date;
        //            //selectCMD.Parameters.Add("@time", OdbcType.Double).Value = dectime;

        //            //create a dataset and fill it
        //            DataSet ds = new DataSet();
        //            adapter.Fill(ds, currentIndex, pageSize, "Products");
        //            DataTable datbl = ds.Tables[0];
        //            string flag = string.Empty;
        //            int TotalRecords = getCount(date, dectime,flag);

        //            if (datbl.Rows.Count <= 0) { return null; }

        //            foreach (DataRow o in datbl.Select("ItemCode<> ' '"))
        //            {
        //                string itemid = o["ItemCode"].ToString();
        //                string selectSQL1 = "SELECT * FROM  IM_ItemWarehouse where  ItemCode=?";
        //                OdbcCommand selectCMD1 = new OdbcCommand(selectSQL1, (OdbcConnection)DbConnection);
        //                adapter.SelectCommand = selectCMD1;
        //                selectCMD1.Parameters.Add("@itemid", OdbcType.VarChar).Value = itemid;
        //                adapter.Fill(ds, "warehouse");
        //            }

        //            DataTable datbl1 = ds.Tables[1];
        //            //datbl.Merge(datbl1);

        //            //Logger.LogDebug(JsonConvert.SerializeObject(datbl));
        //            //Logger.LogDebug(JsonConvert.SerializeObject(datbl1));


        //            foreach (DataRow row in datbl.Rows)
        //            {
        //                Product p = new Product();

        //                p.WarehouseDetails = new List<itemwarehouse>();

        //                p.ItemCode = row.Field<string>("itemcode");
        //                //Logger.LogDebug("1");
        //                p.ItemType = row.Field<string>("ItemType");
        //                p.ItemCodeDesc = row.Field<string>("ItemCodeDesc");
        //                p.StandardUnitOfMeasure = row.Field<string>("StandardUnitOfMeasure");
        //                p.DefaultWarehouseCode = row.Field<string>("DefaultWarehouseCode");
        //                //Logger.LogDebug("2");
        //                p.ProductType = row.Field<string>("ProductType");
        //                p.ProductLine = row.Field<string>("ProductLine");
        //                p.TaxClass = row.Field<string>("TaxClass");
        //                p.PurchasesTaxClass = row.Field<string>("PurchasesTaxClass");
        //                //Logger.LogDebug("3");
        //                p.StandardUnitCost = row.Field<Decimal?>("StandardUnitCost");
        //                p.StandardUnitPrice = row.Field<Decimal?>("StandardUnitPrice");
        //                p.Volume = row.Field<Decimal?>("Volume");
        //                //Logger.LogDebug("4");
        //                p.SalesUnitOfMeasure = row.Field<string>("SalesUnitOfMeasure");
        //                p.PurchaseUnitOfMeasure = row.Field<string>("PurchaseUnitOfMeasure");
        //                p.Valuation = row.Field<string>("Valuation");
        //                //Logger.LogDebug("5");
        //                if (!DBNull.Value.Equals(row["TotalQuantityOnHand"]))
        //                { p.TotalQuantityOnHand = (Decimal?)row["TotalQuantityOnHand"]; }
        //                else { p.TotalQuantityOnHand = 0M; }
                        
        //                p.Category1 = row.Field<string>("Category1");
        //                p.Category2 = row.Field<string>("Category2");
        //                p.Category3 = row.Field<string>("Category3");
        //                p.Category4 = row.Field<string>("Category4");
        //                p.ShipWeight = row.Field<string>("ShipWeight");
        //                //Logger.LogDebug("12");

        //                if (!DBNull.Value.Equals(row["SalesPromotionPrice"]))
        //                { p.SalesPromotionPrice = (Decimal?)row["SalesPromotionPrice"]; }
        //                else { p.SalesPromotionPrice = 0M; }

        //                if (!DBNull.Value.Equals(row["AverageUnitCost"]))
        //                { p.AverageUnitCost = (Decimal?)row["AverageUnitCost"]; }
        //                else { p.AverageUnitCost = 0M; }

        //                if (!DBNull.Value.Equals(row["SuggestedRetailPrice"]))
        //                { p.SuggestedRetailPrice = (Decimal?)row["SuggestedRetailPrice"]; }
        //                else { p.SuggestedRetailPrice = 0M; }
        //                //Logger.LogDebug("13");
        //                p.SaleStartingDate = row.Field<DateTime?>("SaleStartingDate");
        //                p.SaleEndingDate = row.Field<DateTime?>("SaleEndingDate");
        //                p.DateUpdated = row.Field<DateTime>("DateUpdated");
        //                p.TimeUpdated = row.Field<string>("TimeUpdated");
        //                p.TotalRecords = TotalRecords;
        //                //Logger.LogDebug("14");
        //                string sItemID = row.Field<string>("itemcode").ToString();
        //                Logger.LogDebug("ItemId:" + sItemID);
        //                int count = 0;
        //                decimal qtyhand = 0m;
        //                decimal qtyso = 0m;

        //                foreach (DataRow row1 in datbl1.Select().Where(e => e.ItemArray[0].ToString() == sItemID))
        //                {
        //                    itemwarehouse iw = new itemwarehouse();
        //                    count = count + 1;
        //                    Logger.LogDebug("ItemId:" + sItemID + "Count:" + count);

        //                    iw.WarehouseCode = row1.Field<string>("WarehouseCode");
        //                    //Logger.LogDebug("15");
        //                    if (!DBNull.Value.Equals(row1["QuantityOnBackOrder"]))
        //                    { iw.QuantityOnBackOrder = row1.Field<Decimal?>("QuantityOnBackOrder"); }
        //                    else { iw.QuantityOnBackOrder = 0M; }

        //                    if (!DBNull.Value.Equals(row1["QuantityOnHand"]))
        //                    { 
        //                        iw.QuantityOnHand = row1.Field<Decimal?>("QuantityOnHand");
        //                        qtyhand = (decimal)iw.QuantityOnHand;
        //                    }
        //                    else 
        //                    { 
        //                        iw.QuantityOnHand = 0M;
        //                        qtyhand = (decimal)iw.QuantityOnHand;
        //                    }
        //                    //Logger.LogDebug("16");
        //                    if (!DBNull.Value.Equals(row1["QuantityOnWorkOrder"]))
        //                    { iw.QuantityOnWorkOrder = row1.Field<Decimal?>("QuantityOnWorkOrder"); }
        //                    else { iw.QuantityOnWorkOrder = 0M; }

        //                    if (!DBNull.Value.Equals(row1["QuantityOnSalesOrder"]))
        //                    { 
        //                        iw.QuantityOnSalesOrder = row1.Field<Decimal?>("QuantityOnSalesOrder");
        //                        qtyso = (decimal)iw.QuantityOnSalesOrder;
        //                    }
        //                    else 
        //                    { 
        //                        iw.QuantityOnSalesOrder = 0M;
        //                        qtyso = (decimal)iw.QuantityOnSalesOrder;
        //                    }
        //                    //Logger.LogDebug("17");
        //                    iw.QuantityAvailable = Convert.ToDecimal(qtyhand - qtyso);

        //                    if (!DBNull.Value.Equals(row1["MinimumOrderQty"]))
        //                    { iw.MinimumOrderQty = row1.Field<Decimal?>("MinimumOrderQty"); }
        //                    else { iw.MinimumOrderQty = 0M; }

        //                    if (!DBNull.Value.Equals(row1["MaximumOnHandQty"]))
        //                    { iw.MaximumOnHandQty = row1.Field<Decimal?>("MaximumOnHandQty"); }
        //                    else { iw.MaximumOnHandQty = 0M; }

        //                    //Logger.LogDebug("18");
        //                    p.WarehouseDetails.Add(iw);
        //                }


        //                prodList.Add(p);
        //               // Logger.LogDebug("19");
        //            }

        //            return prodList;
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

        public IEnumerable<Product> Productsbydatelastmodified(ProductFilterParam model)
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

                    // Fetch products
                    var products = GetProducts((OdbcConnection)dbConnection, date, decimalTime, currentIndex, pageSize, out int totalRecords);
                    if (products == null || !products.Any())
                    {
                        return null;
                    }

                    // Fetch and map warehouse details
                    foreach (var product in products)
                    {
                        product.WarehouseDetails = GetWarehouseDetails((OdbcConnection)dbConnection, product.ItemCode);
                        product.TotalRecords = totalRecords;
                    }

                    return products;
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
        }
        public  List<Product> GetProducts(OdbcConnection connection, string date, decimal time, int currentIndex, int pageSize, out int totalRecords)
        {
            var products = new List<Product>();
            totalRecords = GetTotalRecordCount(connection, date, time);

            using (var adapter = new OdbcDataAdapter())
            {
                var query = "SELECT * FROM CI_Item WHERE DateUpdated >= ? and TimeUpdated > ? ";
                using (var command = new OdbcCommand(query, connection))
                {
                    command.Parameters.Add("@date", OdbcType.DateTime).Value = date;
                    command.Parameters.Add("@time", OdbcType.Double).Value = time;

                    adapter.SelectCommand = command;

                    var dataSet = new DataSet();
                    adapter.Fill(dataSet, currentIndex, pageSize, "Products");

                    var dataTable = dataSet.Tables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            Product p = new Product();                            

                            p.ItemCode = row.Field<string>("itemcode");
                            //Logger.LogDebug("1");
                            p.ItemType = row.Field<string>("ItemType");
                            p.ItemCodeDesc = row.Field<string>("ItemCodeDesc");
                            p.StandardUnitOfMeasure = row.Field<string>("StandardUnitOfMeasure");
                            p.DefaultWarehouseCode = row.Field<string>("DefaultWarehouseCode");
                            //Logger.LogDebug("2");
                            p.ProductType = row.Field<string>("ProductType");
                            p.ProductLine = row.Field<string>("ProductLine");
                            p.TaxClass = row.Field<string>("TaxClass");
                            p.PurchasesTaxClass = row.Field<string>("PurchasesTaxClass");
                            //Logger.LogDebug("3");
                            p.StandardUnitCost = row.Field<Decimal?>("StandardUnitCost");
                            p.StandardUnitPrice = row.Field<Decimal?>("StandardUnitPrice");
                            p.Volume = row.Field<Decimal?>("Volume");
                            //Logger.LogDebug("4");
                            p.SalesUnitOfMeasure = row.Field<string>("SalesUnitOfMeasure");
                            p.PurchaseUnitOfMeasure = row.Field<string>("PurchaseUnitOfMeasure");
                            p.Valuation = row.Field<string>("Valuation");
                            //Logger.LogDebug("5");
                            if (!DBNull.Value.Equals(row["TotalQuantityOnHand"]))
                            { p.TotalQuantityOnHand = (Decimal?)row["TotalQuantityOnHand"]; }
                            else { p.TotalQuantityOnHand = 0M; }

                            p.Category1 = row.Field<string>("Category1");
                            p.Category2 = row.Field<string>("Category2");
                            p.Category3 = row.Field<string>("Category3");
                            p.Category4 = row.Field<string>("Category4");
                            p.ShipWeight = row.Field<string>("ShipWeight");
                            //Logger.LogDebug("12");

                            if (!DBNull.Value.Equals(row["SalesPromotionPrice"]))
                            { p.SalesPromotionPrice = (Decimal?)row["SalesPromotionPrice"]; }
                            else { p.SalesPromotionPrice = 0M; }

                            if (!DBNull.Value.Equals(row["AverageUnitCost"]))
                            { p.AverageUnitCost = (Decimal?)row["AverageUnitCost"]; }
                            else { p.AverageUnitCost = 0M; }

                            if (!DBNull.Value.Equals(row["SuggestedRetailPrice"]))
                            { p.SuggestedRetailPrice = (Decimal?)row["SuggestedRetailPrice"]; }
                            else { p.SuggestedRetailPrice = 0M; }
                            //Logger.LogDebug("13");
                            p.SaleStartingDate = row.Field<DateTime?>("SaleStartingDate");
                            p.SaleEndingDate = row.Field<DateTime?>("SaleEndingDate");
                            p.DateUpdated = row.Field<DateTime>("DateUpdated");
                            p.TimeUpdated = row.Field<string>("TimeUpdated");
                            p.TotalRecords = totalRecords;
                            products.Add(p);
                        }
                    }
                }
            }

            return products;
        }
        private List<itemwarehouse> GetWarehouseDetails(OdbcConnection connection, string itemCode)
        {
            var warehouseDetails = new List<itemwarehouse>();

            using (var adapter = new OdbcDataAdapter())
            {
                var query = "SELECT * FROM IM_ItemWarehouse WHERE ItemCode = ?";
                using (var command = new OdbcCommand(query, connection))
                {
                    command.Parameters.Add("@itemid", OdbcType.VarChar).Value = itemCode;

                    adapter.SelectCommand = command;

                    var dataSet = new DataSet();
                    adapter.Fill(dataSet, "Warehouse");

                    var dataTable = dataSet.Tables["Warehouse"];
                    decimal qtyhand = 0m;
                    decimal qtyso = 0m;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        itemwarehouse iw = new itemwarehouse();
                       

                        iw.WarehouseCode = row.Field<string>("WarehouseCode");
                        //Logger.LogDebug("15");
                        if (!DBNull.Value.Equals(row["QuantityOnBackOrder"]))
                        { iw.QuantityOnBackOrder = row.Field<Decimal?>("QuantityOnBackOrder"); }
                        else { iw.QuantityOnBackOrder = 0M; }

                        if (!DBNull.Value.Equals(row["QuantityOnHand"]))
                        {
                            iw.QuantityOnHand = row.Field<Decimal?>("QuantityOnHand");
                            qtyhand = (decimal)iw.QuantityOnHand;
                        }
                        else
                        {
                            iw.QuantityOnHand = 0M;
                            qtyhand = (decimal)iw.QuantityOnHand;
                        }
                        //Logger.LogDebug("16");
                        if (!DBNull.Value.Equals(row["QuantityOnWorkOrder"]))
                        { iw.QuantityOnWorkOrder = row.Field<Decimal?>("QuantityOnWorkOrder"); }
                        else { iw.QuantityOnWorkOrder = 0M; }

                        if (!DBNull.Value.Equals(row["QuantityOnSalesOrder"]))
                        {
                            iw.QuantityOnSalesOrder = row.Field<Decimal?>("QuantityOnSalesOrder");
                            qtyso = (decimal)iw.QuantityOnSalesOrder;
                        }
                        else
                        {
                            iw.QuantityOnSalesOrder = 0M;
                            qtyso = (decimal)iw.QuantityOnSalesOrder;
                        }
                        
                        if (!DBNull.Value.Equals(row["QuantityOnPurchaseOrder"]))
                        { iw.QuantityOnPurchaseOrder = row.Field<Decimal?>("QuantityOnPurchaseOrder"); }
                        else { iw.QuantityOnPurchaseOrder = 0M; }

                        //Logger.LogDebug("17");
                        iw.QuantityAvailable = Convert.ToDecimal(qtyhand - qtyso);

                        if (!DBNull.Value.Equals(row["MinimumOrderQty"]))
                        { iw.MinimumOrderQty = row.Field<Decimal?>("MinimumOrderQty"); }
                        else { iw.MinimumOrderQty = 0M; }

                        if (!DBNull.Value.Equals(row["MaximumOnHandQty"]))
                        { iw.MaximumOnHandQty = row.Field<Decimal?>("MaximumOnHandQty"); }
                        else { iw.MaximumOnHandQty = 0M; }

                        //Logger.LogDebug("18");
                        warehouseDetails.Add(iw);
                    }
                }
            }

            return warehouseDetails;
        }
        public decimal ConvertToDecimalTime(string time)
        {
            var parts = time.Split(':');
            return Math.Round(Convert.ToDecimal(parts[0]) + (Convert.ToDecimal(parts[1]) / 60) + (Convert.ToDecimal(parts[2]) / 3600), 5);
        }

        //Get the total count of records available for the date & time
        //public int getCount(string d, decimal t,string flag)
        //{
        //    int iCount = 0;
        //    string sql = string.Empty;
        //    string nsiflag = "Y";
        //    string gsiflag = "Y";
        //    using (var DbConnection = DbConn.GetodbcDbConnection())
        //    {


        //        try
        //        {
        //            OdbcCommand DbCommand = (OdbcCommand)DbConnection.CreateCommand();
        //            DbConnection.Open();

                   
        //                sql = "select Count(*) from IM_ItemWarehouse  where DateUpdated>=? and TimeUpdated>?";
        //                Logger.LogDebug("flag empty");
                    
                   

        //            DbCommand.CommandText = sql;
        //           // DbCommand.CommandText = "select Count(*) from CI_Item  where DateUpdated>=? and TimeUpdated>=? ";
        //            DbCommand.Parameters.Add("@date", OdbcType.DateTime).Value = d;  // "{"+"d"+date+"}";
        //            DbCommand.Parameters.Add("@time", OdbcType.Double).Value = t;

        //            Int32 count = Convert.ToInt32(DbCommand.ExecuteScalar());
        //            iCount = count;
        //            Logger.LogDebug("Count:"+ iCount);
        //            return iCount;
        //        }
        //        catch (Exception ex)
        //        {
        //            {
        //                Logger.LogError(ex.ToString());
        //            }
        //        }

        //        finally
        //        {
        //            DbConnection.Close();
        //        }

        //        return iCount;
        //    }


        //}
        public int GetTotalRecordCount(OdbcConnection connection, string date, decimal time)
        {
            using (var command = new OdbcCommand("SELECT COUNT(*) FROM CI_Item WHERE DateUpdated >= ? AND TimeUpdated > ?", connection))
            {
                command.Parameters.Add("@date", OdbcType.DateTime).Value = date;
                command.Parameters.Add("@time", OdbcType.Double).Value = time;

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }


        //public int getTotalCount(string d, decimal t, string flag,string WCode)
        //{
        //    int iCount = 0;
        //    string sql = string.Empty;
        //    string nsiflag = "Y";
        //    string gsiflag = "Y";
        //    string threeMflag = "Y";
        //    using (var DbConnection = DbConn.GetodbcDbConnection())
        //    {


        //        try
        //        {
        //            OdbcCommand DbCommand = (OdbcCommand)DbConnection.CreateCommand();
        //            DbConnection.Open();

        //                OdbcDataAdapter adapter = new OdbcDataAdapter();
        //                string selectSQL = "SELECT * FROM  IM_ItemWarehouse where DateUpdated >=? and TimeUpdated >=? and WarehouseCode='" + WCode + "' ";
        //                OdbcCommand selectCMD = new OdbcCommand(selectSQL, (OdbcConnection)DbConnection);
        //                adapter.SelectCommand = selectCMD;

        //                //Add Parameters and set values.  
        //                selectCMD.Parameters.Add("@date", OdbcType.DateTime).Value = d;
        //                selectCMD.Parameters.Add("@time", OdbcType.Double).Value = t;

        //                //create a dataset and fill it
        //                DataSet ds = new DataSet();
        //                adapter.Fill(ds,  "IM");
        //                DataTable datbl = ds.Tables[0];
        //                string SQL = string.Empty;
                        

        //                foreach (DataRow o in datbl.Select("ItemCode<> ' '"))
        //                {
        //                    string itemid = o["ItemCode"].ToString();
        //                if (flag == "NS")
        //                {
        //                   // Logger.LogDebug("step1");
        //                    SQL = "SELECT * FROM  CI_Item where  ItemCode= '" + itemid + "'  and UDF_NSI_WEBSITE_LIVE ='" + nsiflag + "' ";
        //                }
        //                else if (flag == "GS")
        //                {
        //                    SQL = "SELECT * FROM  CI_Item where  ItemCode= '" + itemid + "' and UDF_GS_WEBSITE_LIVE ='" + gsiflag + "' ";
        //                   // Logger.LogDebug("step2");
        //                }
        //                else if (flag=="3M")
        //                {
        //                    SQL = "SELECT * FROM  CI_Item where  ItemCode= '" + itemid + "'  and UDF_3M_MARKETPLACE_LIVE ='" + threeMflag + "' ";
        //                    //Logger.LogDebug("step3");
        //                }
                       
                       
        //                OdbcCommand selectCMD1 = new OdbcCommand(SQL, (OdbcConnection)DbConnection);
        //                adapter.SelectCommand = selectCMD1;
        //                //selectCMD1.Parameters.Add("@itemid", OdbcType.VarChar).Value = itemid;
        //                adapter.Fill(ds, "product");

        //                }

        //                DataTable datbl1 = ds.Tables[1];
        //                Logger.LogDebug("inside count function-CI_Item:"+JsonConvert.SerializeObject(datbl1));
        //                int numberOfRecords = 0;

        //                if(flag=="NS")
        //                {
        //                    numberOfRecords = datbl1.AsEnumerable().Where(x => x["UDF_NSI_WEBSITE_LIVE"].ToString() == "Y").ToList().Count;
        //                Logger.LogDebug("NS Count:" + numberOfRecords);
        //                }
        //                else if (flag == "GS")
        //                {
        //                     numberOfRecords = datbl1.AsEnumerable().Where(x => x["UDF_GS_WEBSITE_LIVE"].ToString() == "Y").ToList().Count;
        //                Logger.LogDebug("GS Count:" + numberOfRecords);
        //                }
        //                else if (flag == "3M")
        //                {
        //                    numberOfRecords = datbl1.AsEnumerable().Where(x => x["UDF_3M_MARKETPLACE_LIVE"].ToString() == "Y").ToList().Count;
        //                     Logger.LogDebug("3M Count:" + numberOfRecords);
        //                }

        //            int TotalRecords = numberOfRecords;
                  

        //            //DbCommand.CommandText = sql;
        //            // DbCommand.CommandText = "select Count(*) from CI_Item  where DateUpdated>=? and TimeUpdated>=? ";
        //            //DbCommand.Parameters.Add("@date", OdbcType.DateTime).Value = d;  // "{"+"d"+date+"}";
        //            //DbCommand.Parameters.Add("@time", OdbcType.Double).Value = t;

        //            //Int32 count = Convert.ToInt32(DbCommand.ExecuteScalar());
        //            iCount = TotalRecords;
        //            Logger.LogDebug("Count:" + iCount);
        //            return iCount;
        //        }
        //        catch (Exception ex)
        //        {
        //            {
        //                Logger.LogError(ex.ToString());
        //            }
        //        }

        //        finally
        //        {
        //            DbConnection.Close();
        //        }

        //        return iCount;
        //    }


        //}


        public ProductFound GetProductFound(string ItemCode)
        {
            ProductFound c = new ProductFound();

            using (var DbConnection = DbConn.GetodbcDbConnection())
            {
                try
                {
                    DbConnection.Open();


                    // Assumes that connection is a valid OdbcConnection object.  
                    OdbcDataAdapter adapter = new OdbcDataAdapter();

                    string selectSQL = "SELECT * FROM  CI_Item where ItemCode ='" + ItemCode + "'"; 
                    OdbcCommand selectCMD = new OdbcCommand(selectSQL, (OdbcConnection)DbConnection);
                    adapter.SelectCommand = selectCMD;


                    //create a dataset and fill it
                    DataSet ds = new DataSet();
                    adapter.Fill(ds, "ProductFound");
                    DataTable datbl = ds.Tables[0];
                    if (datbl.Rows.Count <= 0) { return null; }
                    else
                    {


                        //Logger.LogDebug("ProductFound:"+JsonConvert.SerializeObject(datbl));



                        foreach (DataRow row in datbl.Rows)
                        {


                            c.ItemCode = row.Field<string>("ItemCode");
                            c.PriceCode = row.Field<string>("PriceCode");
                            c.StandardUnitPrice = row.Field<decimal>("StandardUnitPrice");
                            c.StandardUnitCost = row.Field<decimal>("StandardUnitCost");
                        }
                    }

                    return c;
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

        public ProductPrice GetProductPrice(string ARDivisionNo, string Company_Code, string CustomerNo, string ItemCode, decimal qty, string CustPriceLevel,int price_code_record, decimal StandardUnitPrice)
        {
            ProductPrice PP = new ProductPrice();
            PP.ItemCode = ItemCode;
            //int price_code_record = 1;
            string selectSQL = string.Empty;
            Logger.LogDebug("PriceCodeRecord:" + price_code_record);
            string ItemPriceCode = GetItemPriceCode(ItemCode);
            Logger.LogDebug("ItemPriceCode" + ItemPriceCode);
            string pricing_method = string.Empty;
            decimal unitprice = 0m;
            decimal discMarkup = 0m;
            using (var DbConnection = DbConn.GetodbcDbConnection())
            {
                try
                {
                    DbConnection.Open();


                    // Assumes that connection is a valid OdbcConnection object.  
                    OdbcDataAdapter adapter = new OdbcDataAdapter();
                    if(price_code_record == 2)
                    {
                        Logger.LogDebug("2");
                           selectSQL = "SELECT * FROM  IM_Pricecode where PriceCodeRecord='" + price_code_record + "'" +
                        "and ItemCode ='" + ItemCode + "'  and ARDivisionNo='" + ARDivisionNo + "'   and CustomerNo='" + CustomerNo + "' ";
                    }
                    else if (price_code_record == 1)
                    {
                        Logger.LogDebug("1");
                        selectSQL = "SELECT * FROM  IM_Pricecode where PriceCodeRecord='" + price_code_record + "'" +
                        "and ItemCode ='" + ItemCode + "'  and CustomerPriceLevel='" + CustPriceLevel + "' ";
                    }
                    else if (price_code_record == 0)
                    {
                        Logger.LogDebug("0");
                        selectSQL = "SELECT * FROM  IM_Pricecode where PriceCodeRecord='" + price_code_record + "'" +
                           " and PriceCode='" + ItemPriceCode + "'  and CustomerPriceLevel='" + CustPriceLevel + "' ";
                    }
                    //string selectSQL = "SELECT * FROM  IM_Pricecode where PriceCodeRecord='" + price_code_record + "'" +
                      //  "and ItemCode ='" + ItemCode + "'  and CustomerPriceLevel='" + CustPriceLevel + "' ";


                    OdbcCommand selectCMD = new OdbcCommand(selectSQL, (OdbcConnection)DbConnection);
                    adapter.SelectCommand = selectCMD;


                    //create a dataset and fill it
                    DataSet ds = new DataSet();
                    adapter.Fill(ds, "PriceCode");
                    DataTable datbl = ds.Tables[0];
                    if (datbl.Rows.Count <= 0) { return null; }
                    else
                    {
                        //Logger.LogDebug("PriceCode:"+JsonConvert.SerializeObject(datbl));
                        foreach (DataRow row in datbl.Rows)
                        {
                            int Bqty1 = Convert.ToInt32(row.Field<decimal>("BreakQuantity1"));
                            int Bqty2 = Convert.ToInt32(row.Field<decimal>("BreakQuantity2"));
                            int Bqty3 = Convert.ToInt32(row.Field<decimal>("BreakQuantity3"));
                            int Bqty4 = Convert.ToInt32(row.Field<decimal>("BreakQuantity4"));
                            int Bqty5 = Convert.ToInt32(row.Field<decimal>("BreakQuantity5"));
                            pricing_method = row.Field<string>("PricingMethod");
                            if (Bqty1 != 0 || Bqty2 != 0 || Bqty3 != 0 || Bqty4 != 0 || Bqty5 != 0)
                            {
                                Logger.LogDebug("Breakqty1:" + Bqty1 +"Bqty2:"+ Bqty2 + "Bqty3:"+ Bqty3+"Bqty4:"+ Bqty4+"Bqty5:"+Bqty5);
                                if (qty > 0 && qty <=Bqty1)
                                {
                                    //discount markup1
                                    discMarkup = row.Field<decimal>("DiscountMarkup1");
                                    //Logger.LogDebug("DiscMarkup1"+ discMarkup);
                                }
                                else if(qty > Bqty1 && qty <= Bqty2)
                                {
                                    //discount markup2
                                    discMarkup = row.Field<decimal>("DiscountMarkup2");
                                    //Logger.LogDebug("DiscMarkup2" + discMarkup);
                                }
                                else if (qty > Bqty2 && qty <= Bqty3)
                                {
                                    //discount markup3
                                    discMarkup = row.Field<decimal>("DiscountMarkup3");
                                   // Logger.LogDebug("DiscMarkup3" + discMarkup);
                                }
                                else if (qty > Bqty3 && qty <= Bqty4)
                                {
                                    //discount markup4
                                    discMarkup = row.Field<decimal>("DiscountMarkup4");
                                    //Logger.LogDebug("DiscMarkup4" + discMarkup);
                                }
                                else if (qty > Bqty4 && qty <= Bqty5)
                                {
                                    //discount markup5
                                    discMarkup = row.Field<decimal>("DiscountMarkup5");
                                   // Logger.LogDebug("DiscMarkup5" + discMarkup);
                                }
                                else if (qty > Bqty5)
                                {
                                    //discount markup5
                                    discMarkup = row.Field<decimal>("DiscountMarkup5");
                                    //Logger.LogDebug("DiscMarkup > 5:" + discMarkup);
                                }

                            }
                            //check for pricing method 
                            if (pricing_method=="C")
                            {
                                Logger.LogDebug("Method:C" );
                                //Cost Markup by Amount -- > the amount mentioned will be added to the standard price which becomes the final price for the item
                                PP.unit_price = StandardUnitPrice + discMarkup;
                            }
                            else if (pricing_method == "D")
                            {
                                Logger.LogDebug("Method:D");
                                //Discount by percentage --> the percentage mentioned will be subtracted from the standard price which becomes the final price for the item
                                PP.unit_price = (StandardUnitPrice - ((discMarkup/100)* StandardUnitPrice));
                            }
                            else if (pricing_method == "M")
                            {
                                Logger.LogDebug("Method:M");
                                //Markup by percentage --> the percentage mentioned will be added to the standard price which becomes the final price for the item
                                PP.unit_price = (StandardUnitPrice + ((discMarkup / 100) * StandardUnitPrice));

                            }
                            else if (pricing_method == "P")
                            {
                                Logger.LogDebug("Method:P");
                                //Discount by amount -->the amount mentioned will be subtracted from the standard price which becomes the final price for the item
                                PP.unit_price = StandardUnitPrice - discMarkup ;
                            }


                        }

                       
                    }

                    return PP;
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

       


        public ProductPrice GetStandardPricewithDiscount(string ARDivisionNo, string Company_Code, string CustomerNo, string ItemCode, decimal StandardUnitPrice,decimal qty, int price_code_record,string CustPriceLevel)
        {
       
            ProductPrice PP = new ProductPrice();
            PP.ItemCode = ItemCode;
            string price_level=string.Empty;
            decimal discount = 0m;
            decimal unitprice = 0m;
            decimal discMarkup = 0m;
            string ItemPriceCode = GetItemPriceCode( ItemCode);
            string pricing_method = string.Empty;
            if (string.IsNullOrEmpty(CustPriceLevel))
            {
                Logger.LogDebug("custpricelevel is null");
                if (Company_Code == "NS")
                {
                    price_level = "A";
                    Logger.LogDebug("custpricelevel A");
                }
                else if (Company_Code == "GS")
                {
                    price_level = "J";
                    Logger.LogDebug("custpricelevel J");
                }
            }
            else
            {
                price_level = CustPriceLevel;
            }
           
           

           
                using (var DbConnection = DbConn.GetodbcDbConnection())
                {
                    try
                    {
                        DbConnection.Open();


                        // Assumes that connection is a valid OdbcConnection object.  
                        OdbcDataAdapter adapter = new OdbcDataAdapter();

                        string selectSQL = "SELECT * FROM  IM_Pricecode where PriceCodeRecord='" + price_code_record + "'" +
                            "  and CustomerPriceLevel='" + price_level + "' ";
                        OdbcCommand selectCMD = new OdbcCommand(selectSQL, (OdbcConnection)DbConnection);
                        adapter.SelectCommand = selectCMD;


                        //create a dataset and fill it
                        DataSet ds = new DataSet();
                        adapter.Fill(ds, "StandardPricePlusDisc");
                        DataTable datbl = ds.Tables[0];
                        if (datbl.Rows.Count <= 0) { return null; }
                        else
                        {
                            //Logger.LogDebug("StandardPricePlusDisc:" + JsonConvert.SerializeObject(datbl));
                            foreach (DataRow row in datbl.Rows)
                            {
                                int Bqty1 = Convert.ToInt32(row.Field<decimal>("BreakQuantity1"));
                                int Bqty2 = Convert.ToInt32(row.Field<decimal>("BreakQuantity2"));
                                int Bqty3 = Convert.ToInt32(row.Field<decimal>("BreakQuantity3"));
                                int Bqty4 = Convert.ToInt32(row.Field<decimal>("BreakQuantity4"));
                                int Bqty5 = Convert.ToInt32(row.Field<decimal>("BreakQuantity5"));
                                pricing_method = row.Field<string>("PricingMethod");
                            if (Bqty1 != 0 || Bqty2 != 0 || Bqty3 != 0 || Bqty4 != 0 || Bqty5 != 0)
                                {
                                    Logger.LogDebug("Breakqty1:" + Bqty1 + "Bqty2:" + Bqty2 + "Bqty3:" + Bqty3 + "Bqty4:" + Bqty4 + "Bqty5:" + Bqty5);
                                    if (qty > 0 && qty <= Bqty1)
                                    {
                                    //discount markup1
                                     
                                    discMarkup = row.Field<decimal>("DiscountMarkup1");
                                        //Logger.LogDebug("DiscMarkup1" + discMarkup);
                                    }
                                    else if (qty > Bqty1 && qty <= Bqty2)
                                    {
                                    //discount markup2
                                    discMarkup = row.Field<decimal>("DiscountMarkup2");
                                        //Logger.LogDebug("DiscMarkup2" + discMarkup);
                                    }
                                    else if (qty > Bqty2 && qty <= Bqty3)
                                    {
                                    //discount markup3
                                    discMarkup = row.Field<decimal>("DiscountMarkup3");
                                        //Logger.LogDebug("DiscMarkup3" + discMarkup);
                                    }
                                    else if (qty > Bqty3 && qty <= Bqty4)
                                    {
                                    //discount markup4
                                    discMarkup = row.Field<decimal>("DiscountMarkup4");
                                       // Logger.LogDebug("DiscMarkup4" + discMarkup);
                                    }
                                    else if (qty > Bqty4 && qty <= Bqty5)
                                    {
                                    //discount markup5
                                    discMarkup = row.Field<decimal>("DiscountMarkup5");
                                        //Logger.LogDebug("DiscMarkup5" + discMarkup);
                                    }
                                    else if (qty > Bqty5)
                                    {
                                    //discount markup5
                                    discMarkup = row.Field<decimal>("DiscountMarkup5");
                                        //Logger.LogDebug("DiscMarkup > 5:" + discMarkup);
                                    }


                                
                                 }
                            /*discount = (discMarkup / 100);
                            Logger.LogDebug("Discount:" + discount);
                            unitprice = StandardUnitPrice - discount;
                            Logger.LogDebug("std price:" + StandardUnitPrice);
                            Logger.LogDebug("unit price:" + unitprice);
                            PP.unit_price = unitprice;*/

                            //check for pricing method 
                            if (pricing_method == "C")
                            {
                                Logger.LogDebug("Method:C");
                                //Cost Markup by Amount -- > the amount mentioned will be added to the standard price which becomes the final price for the item
                                PP.unit_price = StandardUnitPrice + discMarkup;
                            }
                            else if (pricing_method == "D")
                            {
                                Logger.LogDebug("Method:D");
                                //Discount by percentage --> the percentage mentioned will be subtracted from the standard price which becomes the final price for the item
                                PP.unit_price = (StandardUnitPrice - ((discMarkup / 100)* StandardUnitPrice));
                            }
                            else if (pricing_method == "M")
                            {
                                Logger.LogDebug("Method:M");
                                //Markup by percentage --> the percentage mentioned will be added to the standard price which becomes the final price for the item
                                PP.unit_price = (StandardUnitPrice + ((discMarkup / 100) * StandardUnitPrice));

                            }
                            else if (pricing_method == "P")
                            {
                                Logger.LogDebug("Method:P");
                                //Discount by amount -->the amount mentioned will be subtracted from the standard price which becomes the final price for the item
                                PP.unit_price = StandardUnitPrice - discMarkup;
                            }


                        }


                        }

                        return PP;
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


        public string GetCustomerPriceLevel(string CustomerNo, string ARDivisionNo)
        {
            string result = string.Empty;
            using (var DbConnection = DbConn.GetodbcDbConnection())
            {


                try
                {
                    OdbcCommand DbCommand = (OdbcCommand)DbConnection.CreateCommand();
                    DbConnection.Open();

                    DbCommand.CommandText = "select PriceLevel from AR_Customer  where CustomerNo='" + CustomerNo + "' and ARDivisionNo='" + ARDivisionNo + "'  ";

                    result = Convert.ToString(DbCommand.ExecuteScalar());

                    return result;
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

                return result;
            }


        }
        public Product GetProductDetails (string itemcode)
        {
            using (var dbConnection = DbConn.GetodbcDbConnection())
            {
                try
                {
                    dbConnection.Open();

                    // Fetch productinfo
                    var product = GetProductinfo((OdbcConnection)dbConnection, itemcode);
                    if (product == null)
                    {
                        return null;
                    }

                    product.WarehouseDetails = GetWarehouseDetails((OdbcConnection)dbConnection, product.ItemCode);
                    product.TotalRecords = 0;

                    return product;

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

        public Product GetProductinfo(OdbcConnection connection, string itemcode)
        {
            Product p = new Product();

            using (var adapter = new OdbcDataAdapter())
            {
                var query = "SELECT * FROM CI_Item WHERE ItemCode = ?  ";
                using (var command = new OdbcCommand(query, connection))
                {
                    command.Parameters.Add("@it", OdbcType.VarChar).Value = itemcode;


                    adapter.SelectCommand = command;

                    var dataSet = new DataSet();
                    adapter.Fill(dataSet, "Products");

                    var dataTable = dataSet.Tables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {


                            p.ItemCode = row.Field<string>("itemcode");
                            //Logger.LogDebug("1");
                            p.ItemType = row.Field<string>("ItemType");
                            p.ItemCodeDesc = row.Field<string>("ItemCodeDesc");
                            p.StandardUnitOfMeasure = row.Field<string>("StandardUnitOfMeasure");
                            p.DefaultWarehouseCode = row.Field<string>("DefaultWarehouseCode");
                            //Logger.LogDebug("2");
                            p.ProductType = row.Field<string>("ProductType");
                            p.ProductLine = row.Field<string>("ProductLine");
                            p.TaxClass = row.Field<string>("TaxClass");
                            p.PurchasesTaxClass = row.Field<string>("PurchasesTaxClass");
                            //Logger.LogDebug("3");
                            p.StandardUnitCost = row.Field<Decimal?>("StandardUnitCost");
                            p.StandardUnitPrice = row.Field<Decimal?>("StandardUnitPrice");
                            p.Volume = row.Field<Decimal?>("Volume");
                            //Logger.LogDebug("4");
                            p.SalesUnitOfMeasure = row.Field<string>("SalesUnitOfMeasure");
                            p.PurchaseUnitOfMeasure = row.Field<string>("PurchaseUnitOfMeasure");
                            p.Valuation = row.Field<string>("Valuation");
                            //Logger.LogDebug("5");
                            if (!DBNull.Value.Equals(row["TotalQuantityOnHand"]))
                            { p.TotalQuantityOnHand = (Decimal?)row["TotalQuantityOnHand"]; }
                            else { p.TotalQuantityOnHand = 0M; }

                            p.Category1 = row.Field<string>("Category1");
                            p.Category2 = row.Field<string>("Category2");
                            p.Category3 = row.Field<string>("Category3");
                            p.Category4 = row.Field<string>("Category4");
                            p.ShipWeight = row.Field<string>("ShipWeight");
                            //Logger.LogDebug("12");

                            if (!DBNull.Value.Equals(row["SalesPromotionPrice"]))
                            { p.SalesPromotionPrice = (Decimal?)row["SalesPromotionPrice"]; }
                            else { p.SalesPromotionPrice = 0M; }

                            if (!DBNull.Value.Equals(row["AverageUnitCost"]))
                            { p.AverageUnitCost = (Decimal?)row["AverageUnitCost"]; }
                            else { p.AverageUnitCost = 0M; }

                            if (!DBNull.Value.Equals(row["SuggestedRetailPrice"]))
                            { p.SuggestedRetailPrice = (Decimal?)row["SuggestedRetailPrice"]; }
                            else { p.SuggestedRetailPrice = 0M; }
                            //Logger.LogDebug("13");
                            p.SaleStartingDate = row.Field<DateTime?>("SaleStartingDate");
                            p.SaleEndingDate = row.Field<DateTime?>("SaleEndingDate");
                            p.DateUpdated = row.Field<DateTime>("DateUpdated");
                            p.TimeUpdated = row.Field<string>("TimeUpdated");
                            p.TotalRecords = 0;

                        }
                    }
                }
            }

            return p;
        }
        public string GetItemPriceCode(string ItemCode)
        {
            string result = string.Empty;
            using (var DbConnection = DbConn.GetodbcDbConnection())
            {


                try
                {
                    OdbcCommand DbCommand = (OdbcCommand)DbConnection.CreateCommand();
                    DbConnection.Open();

                    DbCommand.CommandText = "select PriceCode from CI_Item  where ItemCode='" + ItemCode + "'  ";

                    result = Convert.ToString(DbCommand.ExecuteScalar());

                    return result;
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

                return result;
            }


        }

        //public IEnumerable<Inventory> GetInventories3M(InventoryFilterParam model)
        //{

        //    using (var DbConnection = DbConn.GetodbcDbConnection())
        //    {
        //        try
        //        {
        //            DbConnection.Open();
        //            IList<Inventory> invList = new List<Inventory>();

        //            //date filter
        //            DateTime cdt = DateTime.Now;
        //            var date = model.DateUpdated.ToString("yyyy-MM-dd");
        //            var time = model.DateUpdated.ToString("HH:mm:ss");
        //            Logger.LogDebug("input time-products:" + time);
        //            //time filter
        //            string[] a = time.Split(new string[] { ":" }, StringSplitOptions.None);
        //            //a[0] contains the hours, a[1] contains the minutes,a[2] contains seconds
        //            //incluing seconds too in the logic
        //            decimal dectime = Math.Round(Convert.ToDecimal(a[0]) + (Convert.ToDecimal(a[1]) / 60) + (Convert.ToDecimal(a[2]) / 3600), 4);
        //            Logger.LogDebug("decimal time-products:" + dectime);
        //           // string sWarehousecode = "000";
        //            int currentIndex = model.CurrentIndex;
        //            int pageSize = model.PageSize;
                   
        //            // Assumes that connection is a valid OdbcConnection object.  
        //            OdbcDataAdapter adapter = new OdbcDataAdapter();
        //            string selectSQL = "SELECT * FROM  IM_ItemWarehouse where DateUpdated >=? and TimeUpdated >=? and WarehouseCode=?";
        //            OdbcCommand selectCMD = new OdbcCommand(selectSQL, (OdbcConnection)DbConnection);
        //            adapter.SelectCommand = selectCMD;

        //            //Add Parameters and set values.  
        //            selectCMD.Parameters.Add("@date", OdbcType.DateTime).Value = date;
        //            selectCMD.Parameters.Add("@time", OdbcType.Double).Value = dectime;
        //            selectCMD.Parameters.Add("@wc", OdbcType.VarChar).Value = model.WarehouseCode;
        //            //create a dataset and fill it
        //            DataSet ds = new DataSet();
        //            adapter.Fill(ds, currentIndex, pageSize, "IM");
        //            DataTable datbl = ds.Tables[0];

        //            string flag = "3M";
        //            int TR = getTotalCount(date, dectime, flag,model.WarehouseCode);
        //            Logger.LogDebug("totalCount:" + TR);

        //            string threeMflag = "Y";
        //            foreach (DataRow o in datbl.Select("ItemCode<> ' '"))
        //            {
        //                string itemid = o["ItemCode"].ToString();
        //                string selectSQL1 = "SELECT * FROM  CI_Item where  ItemCode=? and UDF_3M_MARKETPLACE_LIVE ='" + threeMflag + "' ";
        //                OdbcCommand selectCMD1 = new OdbcCommand(selectSQL1, (OdbcConnection)DbConnection);
        //                adapter.SelectCommand = selectCMD1;
        //                selectCMD1.Parameters.Add("@itemid", OdbcType.VarChar).Value = itemid;
        //                adapter.Fill(ds, "product");

        //            }

        //            DataTable datbl1 = ds.Tables[1];
        //            int numberOfRecords = datbl1.AsEnumerable().Where(x => x["UDF_3M_MARKETPLACE_LIVE"].ToString() == "Y").ToList().Count;
        //            int TotalRecords = TR;
        //            Logger.LogDebug("Count based on page size:" + numberOfRecords);

        //            //int numberOfRecords = datbl.AsEnumerable().ToList().Count;
        //            //int TotalRecords = numberOfRecords;



        //            //Logger.LogDebug(JsonConvert.SerializeObject(datbl));
        //            //Logger.LogDebug("CI_Item:"+JsonConvert.SerializeObject(datbl1));


        //            foreach (DataRow row in datbl1.Rows)
        //            {
        //                Inventory p = new Inventory();

        //                p.WarehouseDetails = new List<warehouse>();

        //                p.ItemCode = row.Field<string>("itemcode");
        //                p.ItemType = row.Field<string>("ItemType");
        //                p.ItemCodeDesc = row.Field<string>("ItemCodeDesc");
        //                p.StandardUnitOfMeasure = row.Field<string>("StandardUnitOfMeasure");
        //                p.DefaultWarehouseCode = row.Field<string>("DefaultWarehouseCode");
        //                if (!DBNull.Value.Equals(row["TotalQuantityOnHand"]))
        //                { p.TotalQuantityOnHand = (Decimal?)row["TotalQuantityOnHand"]; }
        //                else { p.TotalQuantityOnHand = 0M; }

                       
        //                p.DateUpdated = row.Field<DateTime>("DateUpdated");
        //                p.TimeUpdated = row.Field<string>("TimeUpdated");
        //                p.TotalRecords = TotalRecords;



        //                string sItemID = row.Field<string>("itemcode").ToString();
        //                Logger.LogDebug("ItemId:" + sItemID);
        //                //int count = 0;
        //                decimal qtyhand = 0m;
        //                decimal qtyso = 0m;
        //                foreach (DataRow row1 in datbl.Select().Where(e => e.ItemArray[0].ToString() == sItemID))
        //                {
        //                    warehouse iw = new warehouse();
        //                    //count = count + 1;
        //                    Logger.LogDebug("ItemId:" + sItemID );

        //                    iw.WarehouseCode = row1.Field<string>("WarehouseCode");

        //                    //if (!DBNull.Value.Equals(row1["QuantityOnBackOrder"]))
        //                    //{ iw.QuantityOnBackOrder = row1.Field<Decimal?>("QuantityOnBackOrder"); }
        //                    //else { iw.QuantityOnBackOrder = 0M; }

        //                    if (!DBNull.Value.Equals(row1["QuantityOnHand"]))
        //                    {
        //                        iw.QuantityOnHand = row1.Field<Decimal?>("QuantityOnHand");
        //                        qtyhand = (decimal)iw.QuantityOnHand;
        //                    }
        //                    else
        //                    {
        //                        iw.QuantityOnHand = 0M;
        //                        qtyhand = (decimal)iw.QuantityOnHand;
        //                    }

        //                    if (!DBNull.Value.Equals(row1["QuantityOnSalesOrder"]))
        //                    {
        //                        iw.QuantityOnSalesOrder = row1.Field<Decimal?>("QuantityOnSalesOrder");
        //                        qtyso = (decimal)iw.QuantityOnSalesOrder;
        //                    }
        //                    else
        //                    {
        //                        iw.QuantityOnSalesOrder = 0M;
        //                        qtyso = (decimal)iw.QuantityOnSalesOrder;
        //                    }

        //                    iw.QuantityAvailable = Convert.ToDecimal(qtyhand - qtyso);

        //                    //if (!DBNull.Value.Equals(row1["QuantityOnWorkOrder"]))
        //                    //{ iw.QuantityOnWorkOrder = row1.Field<Decimal?>("QuantityOnWorkOrder"); }
        //                    //else { iw.QuantityOnWorkOrder = 0M; }

        //                    //if (!DBNull.Value.Equals(row1["MinimumOrderQty"]))
        //                    //{ iw.MinimumOrderQty = row1.Field<Decimal?>("MinimumOrderQty"); }
        //                    //else { iw.MinimumOrderQty = 0M; }

        //                    //if (!DBNull.Value.Equals(row1["MaximumOnHandQty"]))
        //                    //{ iw.MaximumOnHandQty = row1.Field<Decimal?>("MaximumOnHandQty"); }
        //                    //else { iw.MaximumOnHandQty = 0M; }
        //                    p.WarehouseDetails.Add(iw);
        //                }


        //                invList.Add(p);

        //            }

        //            return invList;
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

        //public IEnumerable<Inventory> GetInventoriesTEST(InventoryFilterParam model)
        //{

        //    using (var DbConnection = DbConn.GetodbcDbConnection())
        //    {
        //        try
        //        {
        //            DbConnection.Open();
        //            IList<Inventory> invList = new List<Inventory>();

        //            //date filter
        //            DateTime cdt = DateTime.Now;
        //            var date = model.DateUpdated.ToString("yyyy-MM-dd");
        //            var time = model.DateUpdated.ToString("HH:mm:ss");
        //            Logger.LogDebug("input time-products:" + time);
        //            //time filter
        //            string[] a = time.Split(new string[] { ":" }, StringSplitOptions.None);
        //            //a[0] contains the hours, a[1] contains the minutes,a[2] seconds
        //            // including seconds too in the logic 
        //            decimal dectime = Math.Round(Convert.ToDecimal(a[0]) + (Convert.ToDecimal(a[1]) / 60) + (Convert.ToDecimal(a[2]) / 3600), 4);
        //            Logger.LogDebug("decimal time-products:" + dectime);

        //            int currentIndex = model.CurrentIndex;
        //            int pageSize = model.PageSize;
        //            string gsiflag = "Y";
        //            // Assumes that connection is a valid OdbcConnection object.  
        //            OdbcDataAdapter adapter = new OdbcDataAdapter();

        //            //Check dateupdated in IM_Itemwarehouse 
        //            string selectSQLIM = "SELECT * FROM  IM_ItemWarehouse where DateUpdated>=? and TimeUpdated>=? and WarehouseCode=?";
        //            OdbcCommand selectCMDIM = new OdbcCommand(selectSQLIM, (OdbcConnection)DbConnection);
        //            adapter.SelectCommand = selectCMDIM;

        //            //Add Parameters and set values.  
        //            selectCMDIM.Parameters.Add("@date", OdbcType.DateTime).Value = date;
        //            selectCMDIM.Parameters.Add("@time", OdbcType.Double).Value = dectime;
        //            selectCMDIM.Parameters.Add("@wc", OdbcType.VarChar).Value = model.WarehouseCode;

        //            //create a dataset and fill it
        //            DataSet ds = new DataSet();
        //            adapter.Fill(ds, "IM");
        //            DataTable datbl = ds.Tables[0];
        //            string flag = "GS";
        //            int TR = getTotalCount(date, dectime, flag, model.WarehouseCode);
        //            Logger.LogDebug("totalCount:" + TR);
        //            // Logger.LogDebug("count:" + Count);

        //            foreach (DataRow o in datbl.Select("ItemCode<> ' '"))
        //            {
        //                string itemid = o["ItemCode"].ToString();
        //                string selectSQL1 = "SELECT * FROM  CI_Item where  ItemCode=? and UDF_GS_WEBSITE_LIVE ='" + gsiflag + "' ";
        //                OdbcCommand selectCMD1 = new OdbcCommand(selectSQL1, (OdbcConnection)DbConnection);
        //                adapter.SelectCommand = selectCMD1;
        //                selectCMD1.Parameters.Add("@itemid", OdbcType.VarChar).Value = itemid;
        //                adapter.Fill(ds,"productfull");
                        
        //            }
        //            DataTable datbl1 = ds.Tables[1];
        //            DataTable dt2 = datbl1.Clone();
        //            for (int i = currentIndex; i <= pageSize; i++)
        //            {
        //                dt2.ImportRow(datbl1.Rows[i]);
        //            }
        //            Logger.LogDebug("dt2"+JsonConvert.SerializeObject(dt2));

        //            int numberOfRecords = datbl1.AsEnumerable().Where(x => x["UDF_GS_WEBSITE_LIVE"].ToString() == "Y").ToList().Count;
        //            int TotalRecords = TR;
        //            Logger.LogDebug("TotalRecords based on pagesize:" + numberOfRecords);
        //            //Logger.LogDebug(JsonConvert.SerializeObject(datbl));
        //            Logger.LogDebug("CI_Item:" + JsonConvert.SerializeObject(datbl1));


        //            foreach (DataRow row in dt2.Rows)
        //            {
        //                Inventory p = new Inventory();

        //                p.WarehouseDetails = new List<warehouse>();

        //                p.ItemCode = row.Field<string>("itemcode");
        //                p.ItemType = row.Field<string>("ItemType");
        //                p.ItemCodeDesc = row.Field<string>("ItemCodeDesc");
        //                p.StandardUnitOfMeasure = row.Field<string>("StandardUnitOfMeasure");
        //                p.DefaultWarehouseCode = row.Field<string>("DefaultWarehouseCode");
        //                if (!DBNull.Value.Equals(row["TotalQuantityOnHand"]))
        //                { p.TotalQuantityOnHand = (Decimal?)row["TotalQuantityOnHand"]; }
        //                else { p.TotalQuantityOnHand = 0M; }

        //                p.UDF_UPC_CODE = row.Field<string>("UDF_UPC_CODE");
        //                p.UDF_SKU = row.Field<string>("UDF_SKU");
        //                p.UDF_GS_WEBSITE_LIVE = row.Field<string>("UDF_GS_WEBSITE_LIVE");
        //                p.UDF_NSI_WEBSITE_LIVE = row.Field<string>("UDF_NSI_WEBSITE_LIVE");
        //                p.UDF_MPN = row.Field<string>("UDF_MPN");
        //                p.DateUpdated = row.Field<DateTime>("DateUpdated");
        //                p.TimeUpdated = row.Field<string>("TimeUpdated");
        //                p.TotalRecords = TotalRecords;

        //                string sItemID = row.Field<string>("itemcode").ToString();
        //                Logger.LogDebug("ItemId:" + sItemID);

        //                decimal qtyhand = 0m;
        //                decimal qtyso = 0m;
        //                foreach (DataRow row1 in datbl.Select().Where(e => e.ItemArray[0].ToString() == sItemID))
        //                {
        //                    warehouse iw = new warehouse();

        //                    Logger.LogDebug("WarehousLogic-ItemId:" + sItemID);

        //                    iw.WarehouseCode = row1.Field<string>("WarehouseCode");

        //                    //if (!DBNull.Value.Equals(row1["QuantityOnBackOrder"]))
        //                    //{ iw.QuantityOnBackOrder = row1.Field<Decimal?>("QuantityOnBackOrder"); }
        //                    //else { iw.QuantityOnBackOrder = 0M; }

        //                    if (!DBNull.Value.Equals(row1["QuantityOnHand"]))
        //                    {
        //                        iw.QuantityOnHand = row1.Field<Decimal?>("QuantityOnHand");
        //                        qtyhand = (decimal)iw.QuantityOnHand;
        //                    }
        //                    else
        //                    {
        //                        iw.QuantityOnHand = 0M;
        //                        qtyhand = (decimal)iw.QuantityOnHand;
        //                    }

        //                    if (!DBNull.Value.Equals(row1["QuantityOnSalesOrder"]))
        //                    {
        //                        iw.QuantityOnSalesOrder = row1.Field<Decimal?>("QuantityOnSalesOrder");
        //                        qtyso = (decimal)iw.QuantityOnSalesOrder;
        //                    }
        //                    else
        //                    {
        //                        iw.QuantityOnSalesOrder = 0M;
        //                        qtyso = (decimal)iw.QuantityOnSalesOrder;
        //                    }

        //                    iw.QuantityAvailable = Convert.ToDecimal(qtyhand - qtyso);

        //                    //if (!DBNull.Value.Equals(row1["QuantityOnWorkOrder"]))
        //                    //{ iw.QuantityOnWorkOrder = row1.Field<Decimal?>("QuantityOnWorkOrder"); }
        //                    //else { iw.QuantityOnWorkOrder = 0M; }

        //                    //if (!DBNull.Value.Equals(row1["MinimumOrderQty"]))
        //                    //{ iw.MinimumOrderQty = row1.Field<Decimal?>("MinimumOrderQty"); }
        //                    //else { iw.MinimumOrderQty = 0M; }

        //                    //if (!DBNull.Value.Equals(row1["MaximumOnHandQty"]))
        //                    //{ iw.MaximumOnHandQty = row1.Field<Decimal?>("MaximumOnHandQty"); }
        //                    //else { iw.MaximumOnHandQty = 0M; }
        //                    p.WarehouseDetails.Add(iw);
        //                }


        //                invList.Add(p);

        //            }

        //            return invList;
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



    }
}