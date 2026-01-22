using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using GlobalSolutions.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GlobalSolutions.Services
{
    public class ProductService: IProductService
    {
        protected readonly IDbConnectionService DbConn;
        protected readonly ILogger<ProductService> logger;
        public const string NoData = "No data found";
        public ProductService(
            IDbConnectionService dbConnectionService,
            ILogger<ProductService> _logger
            )
        {
            DbConn = dbConnectionService;
            logger = _logger;
        }

        public string GetShipmentDetails(int page_size, int page_no)
        {
            string sql = @"SELECT 
     * 
FROM 
   V_SHIPMENT_HEADER A left join V_SHIPMENT_BILLTO B on A.ORDER_NO = B.ORDER_NO and A.ORDER_SUFFIX = B.ORDER_SUFFIX left join V_SHIPMENT_SHIPTO C on A.ORDER_NO = C.ORDER_NO and A.ORDER_SUFFIX = C.ORDER_SUFFIX 
LIMIT ? OFFSET ?";

            try
            {
                using (var conn = DbConn.GetodbcDbConnection())
                {
                    conn.Open();
                    //fetching the data using query and connect
                    using (var cmd1 = new OdbcCommand(sql, conn))
                    {
                        cmd1.Parameters.AddWithValue("", page_size);
                        cmd1.Parameters.AddWithValue("", page_no);
                        using (var adapter = new OdbcDataAdapter(cmd1))
                        {
                            DataTable dt1 = new DataTable();
                            adapter.Fill(dt1);
                            logger.LogDebug("DataTable", JsonConvert.SerializeObject(dt1));

                            if(dt1.Rows.Count > 0)
                            {
                                var result1 = JsonConvert.SerializeObject(dt1);
                                return result1;
                            }
                            return NoData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"An error occured while processing {ex.Message}";
            }
        }


        public string GetShipMentLines(int page_size,int page_no)
        {
            string sql = @"SELECT 
     * 
FROM 
   V_SHIPMENT_LINES 
LIMIT ? OFFSET ? ";

        
                try
                {
                    using (var conn = DbConn.GetodbcDbConnection())
                    {
                        conn.Open();
                        using (var cmd2 = new OdbcCommand(sql, conn))
                        {
                            cmd2.Parameters.AddWithValue("",page_size);
                            cmd2.Parameters.AddWithValue("",page_no);
                            using(var adapter = new OdbcDataAdapter(cmd2))
                            {
                                DataTable dt2 = new DataTable();
                                adapter.Fill(dt2);
                                logger.LogDebug("DataTable", JsonConvert.SerializeObject(dt2));
                                if (dt2.Rows.Count > 0)
                                {
                                    var result2 = JsonConvert.SerializeObject(dt2);
                                    return result2;
                                }
                                return NoData;
                            }
                           
                        }
                    }
                }
                catch (Exception ex)
                {
                    return $"An error occured while processing {ex.Message}";
                }
           
        }


        public string GetShipMentLinesById(string order_no)
        {
            logger.LogDebug(order_no);
            string sql = @"SELECT 
     * 
FROM 
   V_SHIPMENT_LINES Where ORDER_NO = ?";

            try
            {
                using (var conn = DbConn.GetodbcDbConnection())
                {
                    conn.Open();
                    using (var cmd3 = new OdbcCommand(sql, conn))
                    {
                        cmd3.Parameters.AddWithValue("", order_no);
                        using (var adapter = new OdbcDataAdapter(cmd3))
                        {
                            DataTable dt3 = new DataTable();
                            logger.LogDebug("DataTable", JsonConvert.SerializeObject(dt3));
                            adapter.Fill(dt3);
                            if (dt3.Rows.Count > 0)
                            {
                                var result3 = JsonConvert.SerializeObject(dt3);
                                return result3;
                            }
                            return NoData;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                return $"An error occured while processing {ex.Message}";
            }
        }

        public string GetRecordStatus(DateTime date_last_modified)
        {
            string result = date_last_modified.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            string sql = @"SELECT 
     * 
FROM 
  GCG_5807_INPUT Where RECORD_TIMESTAMP>= ? OR PROCESSED_TIMESTAMP>= ? ";
            try
            {
                using (var conn = DbConn.GetodbcDbConnection())
                {
                    conn.Open();
                    using(var cmd4 = new OdbcCommand(sql,conn))
                    {
                        cmd4.Parameters.AddWithValue("", result);
                        cmd4.Parameters.AddWithValue("", result);
                        using(var adapter = new OdbcDataAdapter(cmd4))
                        {
                            DataTable dt4 = new DataTable();
                            adapter.Fill(dt4);
                            if (dt4.Rows.Count > 0)
                            {
                                var response = JsonConvert.SerializeObject(dt4);
                                return response;
                            }
                            return NoData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"An error occured while processing {ex.Message}";
            }
        }

        public int CreateRecord(string table_name, string jsonreq)
        {
            string sql = @"insert into GCG_5807_INPUT (TYPE, DATA) values (?, ?)";

            try
            {
                using (var conn = DbConn.GetodbcDbConnection())
                {
                    conn.Open();
                    using(var cmd = new OdbcCommand(sql,conn))
                    {
                        cmd.Parameters.AddWithValue("", table_name);
                        cmd.Parameters.AddWithValue("", jsonreq);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return rowsAffected;
                        }
                        return rowsAffected;
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public string GetContact(int page_size, int page_no)
        {
            string sql = @"SELECT 
     * 
FROM 
  V_CONTACT 
LIMIT ? OFFSET ?";

            try
            {
                using (var conn = DbConn.GetodbcDbConnection())
                {
                    conn.Open();
                    using(var cmd6 = new OdbcCommand(sql,conn))
                    {
                        cmd6.Parameters.AddWithValue("", page_size);
                        cmd6.Parameters.AddWithValue("", page_no);

                        using(var adapter = new OdbcDataAdapter(cmd6))
                        {
                            DataTable dt6 = new DataTable();
                            adapter.Fill(dt6);
                            if (dt6.Rows.Count > 0)
                            {
                                var response = JsonConvert.SerializeObject(dt6);
                                return response;
                            }
                            return NoData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"An error occured while processing {ex.Message}";
            }
        }

        public string GetCustomers(int page_size,int page_no)
        {
            string sql = @"SELECT 
     * 
FROM 
    V_CUSTOMER_MASTER A left join V_CUSTOMER_INTL B on A.CUSTOMER = B.CUSTOMER left join V_CUSTOMER_SHIPTO C on A.CUSTOMER = C.CUSTOMER left join V_CUSTOMER_SALES D on A.CUSTOMER = D.CUSTOMER left join V_CUST_FORM_INFO E on A.CUSTOMER = E.CUSTOMER 
LIMIT ? OFFSET ?";
            try
            {
                using (var conn = DbConn.GetodbcDbConnection())
                {
                    conn.Open();
                    using(var cmd7 = new OdbcCommand(sql, conn))
                    {
                        cmd7.Parameters.AddWithValue("", page_size);
                        cmd7.Parameters.AddWithValue("", page_no);

                        using(var adapter = new OdbcDataAdapter(cmd7))
                        {
                            DataTable dt7 = new DataTable();
                            adapter.Fill(dt7);
                            if (dt7.Rows.Count > 0)
                            {
                                var response = JsonConvert.SerializeObject(dt7);
                                return response;
                            }
                            return NoData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"An error occured while processing {ex.Message}";
            }
        }

        public string ProductTest(int page_size,int page_no)
        {

            string sql = @"SELECT 
     * 
FROM 
    V_INVENTORY_ALL 
LIMIT ? OFFSET ?";
            try
            {
                using (var conn = DbConn.GetodbcDbConnection())
                {
                    conn.Open();
                    using(var cmd8 = new OdbcCommand(sql,conn))
                    {
                        cmd8.Parameters.AddWithValue("",page_size);
                        cmd8.Parameters.AddWithValue("", page_no);
                        using(var adapter = new OdbcDataAdapter(cmd8))
                        {
                            DataTable dt8 = new DataTable();
                            adapter.Fill(dt8);
                            if (dt8.Rows.Count > 0)
                            {
                                var response = JsonConvert.SerializeObject(dt8);
                                return response;
                            }
                            return NoData;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                return $"An error occured while processing {ex.Message}";
            }
        }

    }
}
