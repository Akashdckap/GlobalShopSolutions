using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Threading.Tasks;
using SageWebAPI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Data;
using Newtonsoft.Json;

namespace SageWebAPI.Services
{
    public class CustomerService : ICustomerService
    {
        protected readonly IDbConnectionService DbConn;
        protected readonly ILogger<CustomerService> Logger;
        private IConfiguration Configuration;

        public CustomerService(ILoggerFactory loggerFactory, IConfiguration _configuration, IDbConnectionService dbConn)
        {
            Logger = loggerFactory.CreateLogger<CustomerService>();
            Configuration = _configuration;
            DbConn = dbConn;

        }

        //customer number accepts only 10 chars inclusive of 00-
        public CustomerCreate Create(CustomerCreate model)
        {

            string homePath = this.Configuration.GetSection("AppSettings")["HomePath"];
            string companyCode = this.Configuration.GetSection("AppSettings")["CompanyCode"];
            string Username = this.Configuration.GetSection("AppSettings")["UserName"];
            string password = this.Configuration.GetSection("AppSettings")["password"];
            string accDate = DateTime.Now.ToString("yyyyMMdd");


            using (DispatchObject pvx1 = new DispatchObject("ProvideX.Script"))
            {
                pvx1.InvokeMethod("Init", homePath);
                // Instantiate a new Session object and initialize the session
                // by setting the user, company, date and module
                using (DispatchObject oSS1 = new DispatchObject(pvx1.InvokeMethod("NewObject", "SY_Session")))
                {
                    Logger.LogDebug("Inside dispatchobj:" + oSS1);

                    oSS1.InvokeMethod("nLogon"); //If the Logon method returns 0 then you must use the SetUser method to allow access
                    oSS1.InvokeMethod("nSetUser", new object[] { Username, password }); //returns 1
                    oSS1.InvokeMethod("nSetCompany", companyCode);//returns 1 successful
                    oSS1.InvokeMethod("nSetDate", "A/R", accDate);
                    oSS1.InvokeMethod("nSetModule", "A/R"); //returns 1 successful
                                                            // Get the Task ID for the AR_Customer_ui program
                    int TaskID = (int)oSS1.InvokeMethod("nLookupTask", "AR_Customer_ui");
                    oSS1.InvokeMethod("nSetProgram", TaskID);

                    using (DispatchObject arCustomer = new DispatchObject(pvx1.InvokeMethod("NewObject", "AR_Customer_bus", oSS1.GetObject())))
                    {
                        try
                        {

                            if (model == null)
                            {
                                Logger.LogDebug("CustomerCreate model is required");
                                return null;
                            }
                            //prefix NS for national safety customers & GS for glovestock
                            //check the terms code to decide if termscode:00 (net30) then NS if termscode:50(credit card) then GS 
                            string prefix = string.Empty;
                            if (model.TermsCode == "00")
                            {
                                prefix = "NS";
                            }
                            else if (model.TermsCode == "50")
                            {
                                prefix = "GS";
                            }
                            else
                            {
                                prefix = "NS";
                            }

                            object[] nextCustomerNumber = new object[] { "CustomerNo$" };
                            //Getting Next Customer Number
                            arCustomer.InvokeMethodByRef("nGetNextCustomerNo", nextCustomerNumber);

                            Logger.LogDebug(nextCustomerNumber[0].ToString());
                            string snextno =  nextCustomerNumber[0].ToString();
                            string scustno = string.Empty;
                            //prefix customerno with NS or GS
                            int scustlength = snextno.Length;
                            if (scustlength >= 7)
                            {
                                string str = snextno.Substring(2);
                                scustno = prefix + str;                                
                            }

                            


                            arCustomer.InvokeMethod("nSetKeyValue", "ARDivisionNo$", model.ARDivisionNo);
                           // model.CustomerNo = nextCustomerNumber[0].ToString();
                            model.CustomerNo = scustno;
                            //arCustomer.InvokeMethodByRef("nSetKeyValue", new object[] { "CustomerNo$", nextCustomerNumber[0].ToString() });
                            arCustomer.InvokeMethodByRef("nSetKeyValue", new object[] { "CustomerNo$", scustno });
                            arCustomer.InvokeMethod("nSetKey");

                            Logger.LogDebug((string)arCustomer.GetProperty("sLastErrorMsg"));


                            arCustomer.InvokeMethod("nSetValue", "CustomerName$", model.CustomerName);
                            arCustomer.InvokeMethod("nSetValue", "AddressLine1$", model.AddressLine1);

                            if (!String.IsNullOrEmpty(model.AddressLine2))
                            {

                                arCustomer.InvokeMethod("nSetValue", "AddressLine2$", model.AddressLine2);
                            }
                            
                            if (!String.IsNullOrEmpty(model.AddressLine3))
                            {
                                //model.AddressLine3 = "No address line 3";
                                arCustomer.InvokeMethod("nSetValue", "AddressLine3$", model.AddressLine3);
                            }
                           
                            arCustomer.InvokeMethod("nSetValue", "City$", model.City);
                            arCustomer.InvokeMethod("nSetValue", "State$", model.State);
                            arCustomer.InvokeMethod("nSetValue", "ZipCode$", model.ZipCode);
                            arCustomer.InvokeMethod("nSetValue", "CountryCode$", model.CountryCode);
                            arCustomer.InvokeMethod("nSetValue", "TelephoneNo$", model.TelephoneNo);
                            arCustomer.InvokeMethod("nSetValue", "EmailAddress$", model.EmailAddress);
                            arCustomer.InvokeMethod("nSetValue", "TermsCode$", model.TermsCode);
                            arCustomer.InvokeMethod("nSetValue", "UDF_CUSTOMER_GROUP$", model.MagentoCustomerGroup);
                            if (String.IsNullOrEmpty(model.DefaultPaymentType))
                            {
                                model.DefaultPaymentType = "CHECK";
                            }
                            arCustomer.InvokeMethod("nSetValue", "DefaultPaymentType$", model.DefaultPaymentType);
                            //arCustomer.InvokeMethod("nSetValue", "SalesPersonDivisionNo$", model.SalespersonDivisionNo);
                            arCustomer.InvokeMethod("nSetValue", "SalesPersonNo$", model.SalespersonNo);
                            Logger.LogDebug((string)arCustomer.GetProperty("sLastErrorMsg"));

                            var ReturnValue = arCustomer.InvokeMethod("nWrite");  //Return value 1 successful

                            Logger.LogDebug("Return Value:" + ReturnValue);
                            Logger.LogDebug((string)arCustomer.GetProperty("sLastErrorMsg"));

                        }
                        catch (Exception ex)
                        {
                            Logger.LogDebug(ex.Message);
                        }
                        finally
                        {
                            arCustomer.Dispose();
                        }

                    }
                    oSS1.Dispose();

                }
                pvx1.Dispose();
            }


            return model;

        }

        public IEnumerable<Customer> Find(customerFilterParam model)
        {
            string homePath = this.Configuration.GetSection("AppSettings")["HomePath"];
            string companyCode = this.Configuration.GetSection("AppSettings")["CompanyCode"];
            string Username = this.Configuration.GetSection("AppSettings")["UserName"];
            string password = this.Configuration.GetSection("AppSettings")["password"];

            string accDate = DateTime.Now.ToString("yyyyMMdd");


            List<Customer> getcustomer = new List<Customer>();

            using (DispatchObject pvx1 = new DispatchObject("ProvideX.Script"))
            {
                Logger.LogDebug("pvx" + pvx1);
                pvx1.InvokeMethod("Init", homePath);
                // Instantiate a new Session object and initialize the session
                // by setting the user, company, date and module
                using (DispatchObject oSS1 = new DispatchObject(pvx1.InvokeMethod("NewObject", "SY_Session")))
                {

                    var s = oSS1.InvokeMethod("nLogon"); //If the Logon method returns 0 then you must use the SetUser method to allow access
                    Logger.LogDebug("logon" + s);
                    s = oSS1.InvokeMethod("nSetUser", new object[] { Username, password }); //returns 1
                    Logger.LogDebug("setuser" + s);
                    oSS1.InvokeMethod("nSetCompany", companyCode);//returns 1 successful
                    Logger.LogDebug("compCode" + companyCode);
                    Logger.LogDebug(accDate);
                    var d = oSS1.InvokeMethod("nSetDate", "A/R", accDate);
                    string error1 = (string)oSS1.GetProperty("sLastErrorMsg");
                    Logger.LogDebug("RC:" + error1);
                    Logger.LogDebug("RC:" + d + "date:" + accDate);
                    d = oSS1.InvokeMethod("nSetModule", "A/R"); //returns 1 successful
                    Logger.LogDebug("module:" + d);
                    // Get the Task ID for the AR_Customer_ui program
                    int TaskID = (int)oSS1.InvokeMethod("nLookupTask", "AR_Customer_ui");
                    oSS1.InvokeMethod("nSetProgram", TaskID);

                    // _svc get records 
                    using (DispatchObject arGetCustomer = new DispatchObject(pvx1.InvokeMethod("NewObject", "AR_Customer_svc", oSS1.GetObject())))
                    {
                        try
                        {  //try

                            string sCustomerNo = "0000001";//model.CustomerNo;
                            arGetCustomer.InvokeMethod("nSetKeyValue", "ARDivisionNo$", "00");
                            //arGetCustomer.InvokeMethod("nSetKeyValue", "DateUpdated$", dateValue);

                            arGetCustomer.InvokeMethod("nSetKeyValue", "CustomerNo$", sCustomerNo);
                            int returnValue = (int)arGetCustomer.InvokeMethod("nFind");
                            string error = (string)arGetCustomer.GetProperty("sLastErrorMsg");
                            if (returnValue > 0)
                            {
                                int retVal = 0;
                                string str1 = "", str2 = "";
                                var data = new object[] { str1, str2 };
                                retVal = (int)arGetCustomer.InvokeMethodByRef("nGetRecord", data);

                                //convert object array to string array 
                                string[] arr = Array.ConvertAll((object[])data, Convert.ToString);
                                //array[0] has data & array[1] has column header names  
                                string[] charArr = arr[0].Split('Š');
                                List<string> Custlist = new List<string>(charArr);
                                //you get the list of column names
                                Logger.LogDebug(arGetCustomer.InvokeMethod("sGetColumns", "MAIN").ToString().Replace(System.Convert.ToChar(352), '\n'));
                                Logger.LogDebug("total count:" + Custlist.Count);
                                Customer customer = new Customer();
                                customer.ARDivisionNo = Custlist[0].ToString();
                                customer.CustomerNo = Custlist[1].ToString();
                                customer.CustomerName = Custlist[2].ToString();
                                customer.AddressLine1 = Custlist[3].ToString();
                                customer.AddressLine2 = Custlist[4].ToString();
                                customer.City = Custlist[6].ToString();
                                customer.State = Custlist[7].ToString();
                                customer.ZipCode = Custlist[8].ToString();
                                customer.CountryCode = Custlist[9].ToString();
                                customer.TelephoneNo = Custlist[10].ToString();
                                customer.EmailAddress = Custlist[14].ToString();
                                customer.TermsCode = Custlist[24].ToString();
                                customer.ContactCode = Custlist[20].ToString();
                                //customer.SalespersonDivisionNo = Custlist[25].ToString();
                                customer.SalespersonNo = Custlist[26].ToString();
                                //customer.DefaultPaymentType = Custlist[59].ToString();
                                getcustomer.Add(customer);

                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogDebug(ex.Message);
                        }
                        finally
                        {
                            arGetCustomer.Dispose();
                        }
                    }
                    oSS1.Dispose();

                }

                pvx1.Dispose();

            }

            return getcustomer;

        }

        public IEnumerable<Customer> GetCustomerID(customerFilterParam model)
        {

            string homePath = this.Configuration.GetSection("AppSettings")["HomePath"];
            string companyCode = this.Configuration.GetSection("AppSettings")["CompanyCode"];
            string Username = this.Configuration.GetSection("AppSettings")["UserName"];
            string password = this.Configuration.GetSection("AppSettings")["password"];
            string accDate = DateTime.Now.ToString("yyyyMMdd");
            List<Customer> getcustomerid = new List<Customer>();
            using (DispatchObject pvx1 = new DispatchObject("ProvideX.Script"))
            {
                Logger.LogDebug("pvx" + pvx1);
                pvx1.InvokeMethod("Init", homePath);
                using (DispatchObject oSS1 = new DispatchObject(pvx1.InvokeMethod("NewObject", "SY_Session")))
                {
                    var s = oSS1.InvokeMethod("nLogon"); //If the Logon method returns 0 then you must use the SetUser method to allow access

                    s = oSS1.InvokeMethod("nSetUser", new object[] { Username, password }); //returns 1                   
                    oSS1.InvokeMethod("nSetCompany", companyCode);//returns 1 successful                   
                    var d = oSS1.InvokeMethod("nSetDate", "A/R", accDate);
                    string error1 = (string)oSS1.GetProperty("sLastErrorMsg");
                    d = oSS1.InvokeMethod("nSetModule", "A/R"); //returns 1 successful

                    // Get the Task ID for the AR_Customer_ui program
                    int TaskID = (int)oSS1.InvokeMethod("nLookupTask", "AR_Customer_ui");
                    oSS1.InvokeMethod("nSetProgram", TaskID);

                    using (DispatchObject arGetCustomer = new DispatchObject(pvx1.InvokeMethod("NewObject", "AR_Customer_svc", oSS1.GetObject())))
                    {
                        try
                        {
                            String columns = "CustomerNo$";
                            String keys = "CustomerNo$";
                            String returnFields = "AddressLine1$";
                            String returnAccountKeys = "AddressLine1$";
                            string sDatetime = model.DateUpdated.ToString("yyyyMMdd");
                            Logger.LogDebug(sDatetime);
                            string whereClause = "DateUpdated$>=" + Convert.ToChar(34) + sDatetime + Convert.ToChar(34);
                            // Setup the parameter list to be passed as reference to the GetResultSets method
                            object[] getResultSetParams = new object[] { columns, keys, returnFields, returnAccountKeys, whereClause, "", "" };

                            // Call the GetResultSets to return the list of Customer numbers and names
                            arGetCustomer.InvokeMethodByRef("nGetResultSets", getResultSetParams);
                            // The ProvideX SEP character is referenced by character number 352 within C#
                            // Split the customer names into string array and list them in a console window
                            string[] names = getResultSetParams[2].ToString().Split(System.Convert.ToChar(352));
                            foreach (string ch in names)
                            {
                                if (!string.IsNullOrWhiteSpace(ch) == true)
                                {
                                    Customer cust = new Customer();
                                    cust.CustomerNo = ch;
                                    getcustomerid.Add(cust);
                                }
                            }




                        }
                        catch (Exception ex)
                        {
                            Logger.LogDebug(ex.Message);
                        }
                        finally
                        {
                            arGetCustomer.Dispose();
                        }

                    }
                    oSS1.Dispose();
                }


                pvx1.Dispose();
            }


            return getcustomerid;

        }

        //public CustomerDetails GetCustomerDetails(string CustomerNo)
        //{
        //    string homePath = this.Configuration.GetSection("AppSettings")["HomePath"];
        //    string companyCode = this.Configuration.GetSection("AppSettings")["CompanyCode"];
        //    string Username = this.Configuration.GetSection("AppSettings")["UserName"];
        //    string password = this.Configuration.GetSection("AppSettings")["password"];

        //    string accDate = DateTime.Now.ToString("yyyyMMdd");

        //    List<CustomerDetails> getcustomer = new List<CustomerDetails>();
        //    CustomerDetails custDetails = new CustomerDetails();

        //    using (DispatchObject pvx1 = new DispatchObject("ProvideX.Script"))
        //    {
        //        Logger.LogDebug("pvx" + pvx1);
        //        pvx1.InvokeMethod("Init", homePath);
        //        // Instantiate a new Session object and initialize the session
        //        // by setting the user, company, date and module
        //        using (DispatchObject oSS1 = new DispatchObject(pvx1.InvokeMethod("NewObject", "SY_Session")))
        //        {

        //            var s = oSS1.InvokeMethod("nLogon"); //If the Logon method returns 0 then you must use the SetUser method to allow access
        //            Logger.LogDebug("logon" + s);
        //            s = oSS1.InvokeMethod("nSetUser", new object[] { Username, password }); //returns 1
        //            Logger.LogDebug("setuser" + s);
        //            oSS1.InvokeMethod("nSetCompany", companyCode);//returns 1 successful
        //            Logger.LogDebug("compCode" + companyCode);
        //            Logger.LogDebug(accDate);
        //            var d = oSS1.InvokeMethod("nSetDate", "A/R", accDate);
        //            string error1 = (string)oSS1.GetProperty("sLastErrorMsg");
        //            Logger.LogDebug("RC:" + error1);
        //            Logger.LogDebug("RC:" + d + "date:" + accDate);
        //            d = oSS1.InvokeMethod("nSetModule", "A/R"); //returns 1 successful
        //            Logger.LogDebug("module:" + d);
        //            // Get the Task ID for the AR_Customer_ui program
        //            int TaskID = (int)oSS1.InvokeMethod("nLookupTask", "AR_Customer_ui");
        //            oSS1.InvokeMethod("nSetProgram", TaskID);

        //            // _svc get records 
        //            using (DispatchObject arGetCustomer = new DispatchObject(pvx1.InvokeMethod("NewObject", "AR_Customer_svc", oSS1.GetObject())))
        //            {
        //                try
        //                {  //try

        //                    string sCustomerNo = CustomerNo;
        //                    arGetCustomer.InvokeMethod("nSetKeyValue", "ARDivisionNo$", "00");
        //                    //arGetCustomer.InvokeMethod("nSetKeyValue", "DateUpdated$", dateValue);

        //                    arGetCustomer.InvokeMethod("nSetKeyValue", "CustomerNo$", sCustomerNo);
        //                    int returnValue = (int)arGetCustomer.InvokeMethod("nFind");
        //                    string error = (string)arGetCustomer.GetProperty("sLastErrorMsg");
        //                    if (returnValue > 0)
        //                    {
        //                        int retVal = 0;
        //                        string str1 = "", str2 = "";
        //                        var data = new object[] { str1, str2 };
        //                        retVal = (int)arGetCustomer.InvokeMethodByRef("nGetRecord", data);

        //                        //convert object array to string array 
        //                        string[] arr = Array.ConvertAll((object[])data, Convert.ToString);
        //                        //array[0] has data & array[1] has column header names  

        //                        string[] charArr = arr[0].Split('Š');
        //                        List<string> Custlist = new List<string>(charArr);

        //                        //you get the list of column names available in AR_Customer_svc
        //                        Logger.LogDebug(arGetCustomer.InvokeMethod("sGetColumns", "MAIN").ToString().Replace(System.Convert.ToChar(352), '\n'));

        //                        Logger.LogDebug("total count:" + Custlist.Count);
        //                        CustomerDetails customer = new CustomerDetails();
        //                        customer.ARDivisionNo = Custlist[0].ToString();
        //                        customer.CustomerNo = Custlist[1].ToString();
        //                        customer.CustomerName = Custlist[2].ToString();
        //                        customer.AddressLine1 = Custlist[3].ToString();
        //                        customer.AddressLine2 = Custlist[4].ToString();
        //                        customer.City = Custlist[6].ToString();
        //                        customer.State = Custlist[7].ToString();
        //                        customer.ZipCode = Custlist[8].ToString();
        //                        customer.CountryCode = Custlist[9].ToString();
        //                        customer.TelephoneNo = Custlist[10].ToString();
        //                        customer.EmailAddress = Custlist[14].ToString();
        //                        customer.TermsCode = Custlist[24].ToString();
        //                        customer.ShipMethod = Custlist[21].ToString();
        //                        customer.TaxSchedule = Custlist[22].ToString();
        //                        customer.ContactCode = Custlist[20].ToString();
        //                        //customer.SalespersonDivisionNo = Custlist[25].ToString();
        //                        customer.SalespersonNo = Custlist[26].ToString();
        //                        //customer.DefaultPaymentType = Custlist[59].ToString();
        //                        string scontactcode = Custlist[20].ToString();
        //                        //var c = getContact( pvx1,oSS1, scontactcode, sCustomerNo);


        //                        getcustomer.Add(customer);
        //                        return customer;

        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Logger.LogDebug(ex.Message);
        //                }
        //                finally
        //                {
        //                    arGetCustomer.Dispose();
        //                }
        //            }
        //            oSS1.Dispose();

        //        }

        //        pvx1.Dispose();

        //    }
        //    return null;
        //}
        //26Dec2023: contact code will be sent by integrator 
        public ContactCreate CreateContact(ContactCreate model)
        {
            string homePath = this.Configuration.GetSection("AppSettings")["HomePath"];
            Logger.LogDebug("Homepath:" + homePath);
            string companyCode = this.Configuration.GetSection("AppSettings")["CompanyCode"];
            Logger.LogDebug("companycode:" + companyCode);
            string Username = this.Configuration.GetSection("AppSettings")["UserName"];
            Logger.LogDebug("UN:" + Username);
            string password = this.Configuration.GetSection("AppSettings")["password"];
            Logger.LogDebug("password:" + password);
            string accDate = DateTime.Now.ToString("yyyyMMdd");
            using (DispatchObject pvx1 = new DispatchObject("ProvideX.Script"))
            {
                Logger.LogDebug("inside dispatch object:");
                pvx1.InvokeMethod("Init", homePath);
                using (DispatchObject oSS1 = new DispatchObject(pvx1.InvokeMethod("NewObject", "SY_Session")))
                {
                    Logger.LogDebug("step2-----:" );
                    oSS1.InvokeMethod("nLogon"); //If the Logon method returns 0 then you must use the SetUser method to allow access
                    oSS1.InvokeMethod("nSetUser", new object[] { Username, password }); //returns 1
                    oSS1.InvokeMethod("nSetCompany", companyCode);//returns 1 successful
                    Logger.LogDebug("step3-----:");
                    oSS1.InvokeMethod("nSetDate", "A/R", accDate);
                    oSS1.InvokeMethod("nSetModule", "A/R"); //returns 1 successful
                    // Get the Task ID for the AR_Customer_ui program
                    int TaskID = (int)oSS1.InvokeMethod("nLookupTask", "AR_Customer_ui");
                    oSS1.InvokeMethod("nSetProgram", TaskID);
                    using (DispatchObject arContact = new DispatchObject(pvx1.InvokeMethod("NewObject", "AR_CustomerContact_bus", oSS1.GetObject())))
                    {
                        Logger.LogDebug("step4-----:");
                        try
                        {
                            int irunno = 0;
                            string scontactcode = string.Empty;
                            string scontactintials = model.ContactName.Substring(0, 2);
                            int ContactCount = getContactCount(model.CustomerNo.ToUpper());
                            Logger.LogDebug("Contact_Count: " + ContactCount);
                            string ss = scontactintials.ToUpper() + "{0:000}";
                            Logger.LogDebug("contact intials$ " + ss);
                            irunno = ContactCount + 1;
                            scontactcode = string.Format(ss, irunno);

                            //if (ContactCount == 0)
                            //{
                            //    irunno = ContactCount + 1;
                            //    scontactcode = string.Format(ss, irunno);
                            //    //    scontactcode = scontactintials+ "00"+ irunno;
                            //}
                            //else if (ContactCount > 0)
                            //{
                            //    irunno = ContactCount + 1;
                            //    //    scontactcode = string.Concat(scontactintials,irunno);
                            //    scontactcode = string.Format(ss, irunno);
                            //}
                            model.ContactCode = scontactcode.ToUpper();
                            model.CustomerNo = model.CustomerNo.ToUpper();
                            Logger.LogDebug("ContactCodefinal$:"+ model.ContactCode);

                            var retVal = arContact.InvokeMethod("nSetKeyValue", "ARDivisionNo$", model.ARDivisionNo);
                            retVal = arContact.InvokeMethod("nSetKeyValue", "CustomerNo$", model.CustomerNo.ToUpper());
                            retVal = arContact.InvokeMethod("nSetKeyValue", "ContactCode$", model.ContactCode);
                            retVal = arContact.InvokeMethod("nSetKey");
                            retVal = arContact.InvokeMethod("nSetValue", "ContactName$", model.ContactName.ToUpper());
                            retVal = arContact.InvokeMethod("nSetValue", "AddressLine1$", model.AddressLine1);
                            retVal = arContact.InvokeMethod("nSetValue", "City", model.City);
                            retVal = arContact.InvokeMethod("nSetValue", "State$", model.State);
                            retVal = arContact.InvokeMethod("nSetValue", "ZipCode$", model.ZipCode);
                            retVal = arContact.InvokeMethod("nSetValue", "TelephoneNo1$", model.TelephoneNo1);
                            retVal = arContact.InvokeMethod("nSetValue", "EmailAddress$", model.EmailAddress);
                            retVal = arContact.InvokeMethod("nWrite");

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        finally
                        {
                            arContact.Dispose();

                        }

                    }

                    oSS1.Dispose();
                }
                pvx1.Dispose();
            }

            return model;
        }


        //public CustomerContact getContact(DispatchObject pvx1,DispatchObject oSS1, string contactcode,string customerno)
        //{
        //    List<CustomerContact> getcontact = new List<CustomerContact>();

        //    using (DispatchObject arGetContact = new DispatchObject(pvx1.InvokeMethod("NewObject", "AR_Customer_svc", oSS1.GetObject())))
        //    {

        //        arGetContact.InvokeMethod("nSetKeyValue", "ARDivisionNo$", "00");
        //        //arGetCustomer.InvokeMethod("nSetKeyValue", "DateUpdated$", dateValue);
        //        string sContactcode = contactcode;
        //        arGetContact.InvokeMethod("nSetKeyValue", "CustomerNo$", customerno);
        //        arGetContact.InvokeMethod("nSetKeyValue", "ContactCode$", sContactcode);
        //        int returnValue = (int)arGetContact.InvokeMethod("nFind");
        //        string error = (string)arGetContact.GetProperty("sLastErrorMsg");
        //        CustomerContact custcontact = new CustomerContact();

        //        if (returnValue > 0)
        //        {
        //            string str3 = "", str4 = "";
        //            var data1 = new object[] { str3, str4 };
        //            int retVal = (int)arGetContact.InvokeMethodByRef("nGetRecord", data1);
        //            //convert object array to string array 
        //            string[] arr1 = Array.ConvertAll((object[])data1, Convert.ToString);
        //            //array[0] has data & array[1] has column header names  

        //            string[] charArr1 = arr1[0].Split('Š');
        //            List<string> Contactlist = new List<string>(charArr1);
        //            custcontact.ContactCode = Contactlist[2].ToString();
        //            custcontact.ContactName = Contactlist[3].ToString();
        //            custcontact.AddressLine1 = Contactlist[4].ToString();
        //            custcontact.AddressLine2 = Contactlist[5].ToString();
        //            custcontact.TelephoneNo1 = Contactlist[13].ToString();
        //            custcontact.EmailAddress = Contactlist[22].ToString();
        //            getcontact.Add(custcontact);
        //            return custcontact;                  

        //        }
        //        arGetContact.Dispose();
        //    }
        //    return null;
        //}

        //public IEnumerable<Customer> FindCustomer(customerFilterParam model)
        //{

        //    using (var DbConnection = DbConn.GetodbcDbConnection())
        //    {
        //        try
        //        {
        //            DbConnection.Open();
        //            IList<Customer> custList = new List<Customer>();

        //            //date filter
        //            DateTime cdt = DateTime.Now;
        //            var date = model.DateUpdated.ToString("yyyy-MM-dd");
        //            var time = model.DateUpdated.ToString("HH:mm:ss");
        //            //time filter
        //            string[] a = time.Split(new string[] { ":" }, StringSplitOptions.None);
        //            //a[0] contains the hours, a[1] contains the minutes
        //            decimal dectime = Math.Round(Convert.ToDecimal(a[0]) + (Convert.ToDecimal(a[1]) / 60), 4);


        //            int currentIndex = model.CurrentIndex;
        //            int pageSize = model.PageSize;

        //            // Assumes that connection is a valid OdbcConnection object.  
        //            OdbcDataAdapter adapter = new OdbcDataAdapter();

        //            string selectSQL = "SELECT * FROM  AR_Customer where DateUpdated>=? and TimeUpdated>=?";
        //            OdbcCommand selectCMD = new OdbcCommand(selectSQL, (OdbcConnection)DbConnection);
        //            adapter.SelectCommand = selectCMD;

        //            //Add Parameters and set values.  
        //            selectCMD.Parameters.Add("@date", OdbcType.DateTime).Value = date;
        //            selectCMD.Parameters.Add("@time", OdbcType.Double).Value = dectime;

        //            //create a dataset and fill it
        //            DataSet ds = new DataSet();
        //            adapter.Fill(ds, currentIndex, pageSize, "Products");
        //            DataTable datbl = ds.Tables[0];
        //            int TotalRecords = getCount(date, dectime);                  



        //            Logger.LogDebug(JsonConvert.SerializeObject(datbl));
                 


        //            foreach (DataRow row in datbl.Rows)
        //            {
        //                Customer c = new Customer();

        //                c.ARDivisionNo = row.Field<string>("ARDivisionNo");
        //                c.CustomerNo = row.Field<string>("CustomerNo");
        //                c.CustomerName = row.Field<string>("CustomerName");
        //                c.AddressLine1  = row.Field<string>("AddressLine1");
        //                c.AddressLine2 = row.Field<string>("AddressLine2");
        //                c.City = row.Field<string>("City");
        //                c.State = row.Field<string>("State");
        //                c.ZipCode = row.Field<string>("ZipCode");
        //                c.CountryCode = row.Field<string>("CountryCode");
        //                c.TelephoneNo = row.Field<string>("TelephoneNo");
        //                c.SalespersonNo = row.Field<string>("SalespersonNo");
        //                c.EmailAddress = row.Field<string>("EmailAddress"); 
        //                c.TermsCode = row.Field<string>("TermsCode"); 
        //                c.ShipMethod = row.Field<string>("ShipMethod"); 
        //                c.TaxSchedule = row.Field<string>("TaxSchedule"); 
        //                c.ContactCode = row.Field<string>("ContactCode");                
        //                c.TotalRecords = TotalRecords;

        //                Logger.LogDebug("customer:");

        //                custList.Add(c);

        //            }

        //            return custList;
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


        public IEnumerable<Customer>FindCustomerNew(customerFilterParam model)
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
                    var customers = GetCustomers((OdbcConnection)dbConnection, date, decimalTime, currentIndex, pageSize, out int totalRecords);
                    if (customers == null || !customers.Any())
                    {
                        return null;
                    }

                    // Fetch and map salerep details
                    foreach (var cust in customers)
                    {
                        cust.salesreps = GetSalesrepDetails((OdbcConnection)dbConnection, cust.SalespersonNo);
                        cust.contacts = GetContacts((OdbcConnection)dbConnection, cust.CustomerNo);
                        cust.shiptos = GetShippingAddress((OdbcConnection)dbConnection, cust.CustomerNo);
                        cust.TotalRecords = totalRecords;
                    }

                    return customers;

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
        private List<Customer> GetCustomers(OdbcConnection connection, string date, decimal time, int currentIndex, int pageSize, out int totalRecords)
        {
            var customers = new List<Customer>();
            totalRecords = GetTotalRecordCount(connection, date, time);

            using (var adapter = new OdbcDataAdapter())
            {
                var query = "SELECT * FROM  AR_Customer where DateUpdated >=? and TimeUpdated > ?";
                using (var command = new OdbcCommand(query, connection))
                {
                    command.Parameters.Add("@date", OdbcType.DateTime).Value = date;
                    command.Parameters.Add("@time", OdbcType.Double).Value = time;
                    adapter.SelectCommand = command;

                    var dataSet = new DataSet();
                    adapter.Fill(dataSet, currentIndex, pageSize, "customers");

                    var dataTable = dataSet.Tables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            Customer c = new Customer();

                            c.ARDivisionNo = row.Field<string>("ARDivisionNo");
                            c.CustomerNo = row.Field<string>("CustomerNo");
                            c.CustomerName = row.Field<string>("CustomerName");
                            c.AddressLine1 = row.Field<string>("AddressLine1");
                            c.AddressLine2 = row.Field<string>("AddressLine2");
                            c.URLAddress = row.Field<string>("URLAddress");
                            c.City = row.Field<string>("City");
                            c.State = row.Field<string>("State");
                            c.ZipCode = row.Field<string>("ZipCode");
                            c.CountryCode = row.Field<string>("CountryCode");
                            c.TelephoneNo = row.Field<string>("TelephoneNo");
                            c.SalespersonNo = row.Field<string>("SalespersonNo");
                            c.EmailAddress = row.Field<string>("EmailAddress");
                            c.UDF_DEALERLEVEL = row.Field<string>("UDF_DEALERLEVEL");
                            c.UDF_TERRITORY = row.Field<string>("UDF_TERRITORY");
                            c.UDF_BUYING_GROUP = row.Field<string>("UDF_BUYING_GROUP");
                            c.UDF_PERSONA = row.Field<string>("UDF_PERSONA");
                            c.UDF_SAMSUNGPLATINUM = row.Field<string>("UDF_SAMSUNGPLATINUM");
                            c.UDF_LGPINNACLENUMBER = row.Field<string>("UDF_LGPINNACLENUMBER");
                            c.UDF_SONYDIAMOND = row.Field<string>("UDF_SONYDIAMOND");
                            c.PrimaryShipToCode = row.Field<string>("PrimaryShipToCode");
                            c.TermsCode = row.Field<string>("TermsCode");
                            c.ShipMethod = row.Field<string>("ShipMethod");
                            c.TaxSchedule = row.Field<string>("TaxSchedule");
                            c.ContactCode = row.Field<string>("ContactCode");
                            c.DateUpdated = row.Field<DateTime>("DateUpdated");
                            c.TimeUpdated = row.Field<string>("TimeUpdated");
                            c.TotalRecords = totalRecords;

                            Logger.LogDebug("customer:");

                            customers.Add(c);
                        }
                    }
                }
            }

            return customers;
        }

        private Salesrep GetSalesrepDetails(OdbcConnection connection, string SalespersonNo)
        {
            var salesrep = new Salesrep();

            using (var adapter = new OdbcDataAdapter())
            {
                var query = "SELECT * FROM AR_Salesperson WHERE SalespersonNo = ?";
                using (var command = new OdbcCommand(query, connection))
                {
                    command.Parameters.Add("@repid", OdbcType.VarChar).Value = SalespersonNo;

                    adapter.SelectCommand = command;

                    var dataSet = new DataSet();
                    adapter.Fill(dataSet, "salesrep");

                    var dataTable = dataSet.Tables["salesrep"];
                    foreach (DataRow row in dataTable.Rows)
                    {

                        salesrep.SalespersonNo = row.Field<string>("SalespersonNo");
                        salesrep.SalespersonName = row.Field<string>("SalespersonName");
                        salesrep.AddressLine1 = row.Field<string>("AddressLine1");
                        salesrep.AddressLine2 = row.Field<string>("AddressLine2");
                        salesrep.EmailAddress = row.Field<string>("EmailAddress");
                        salesrep.City = row.Field<string>("City");
                        salesrep.State = row.Field<string>("State");
                        salesrep.ZipCode = row.Field<string>("ZipCode");
                        salesrep.DateUpdated = row.Field<DateTime>("DateUpdated");
                        salesrep.TimeUpdated = row.Field<string>("TimeUpdated");

                    }
                }
            }

            return salesrep;
        }

        private List<Contacts> GetContacts(OdbcConnection connection, string CustomerNo)
        {
            var contact = new List<Contacts>();


            using (var adapter = new OdbcDataAdapter())
            {
                var query = "SELECT * FROM  AR_CustomerContact where CustomerNo =? ";
                using (var command = new OdbcCommand(query, connection))
                {
                    command.Parameters.Add("@cust", OdbcType.VarChar).Value = CustomerNo;

                    adapter.SelectCommand = command;

                    var dataSet = new DataSet();
                    adapter.Fill(dataSet, "Contacts");

                    var dataTable = dataSet.Tables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            Contacts c = new Contacts();

                            c.ARDivisionNo = row.Field<string>("ARDivisionNo");
                            c.CustomerNo = row.Field<string>("CustomerNo");
                            c.ContactCode = row.Field<string>("ContactCode");
                            c.ContactName = row.Field<string>("ContactName");
                            c.AddressLine1 = row.Field<string>("AddressLine1");
                            c.AddressLine2 = row.Field<string>("AddressLine2");
                            c.AddressLine3 = row.Field<string>("AddressLine3");
                            c.City = row.Field<string>("City");
                            c.State = row.Field<string>("State");
                            c.ZipCode = row.Field<string>("ZipCode");
                            c.TelephoneNo1 = row.Field<string>("TelephoneNo1");
                            c.EmailAddress = row.Field<string>("EmailAddress");
                            c.DateUpdated = row.Field<DateTime>("DateUpdated");
                            c.TimeUpdated = row.Field<string>("TimeUpdated");
                            Logger.LogDebug("Contacts:");
                            contact.Add(c);
                        }
                    }
                }
            }

            return contact;
        }

        private List<shipTo> GetShippingAddress(OdbcConnection connection, string CustomerNo)
        {
            var shiptos = new List<shipTo>();


            using (var adapter = new OdbcDataAdapter())
            {
                var query = "SELECT * FROM  SO_ShipToAddress  where CustomerNo =? ";
                using (var command = new OdbcCommand(query, connection))
                {
                    command.Parameters.Add("@cust", OdbcType.VarChar).Value = CustomerNo;

                    adapter.SelectCommand = command;

                    var dataSet = new DataSet();
                    adapter.Fill(dataSet, "shipto");

                    var dataTable = dataSet.Tables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            shipTo s = new shipTo();

                            s.ARDivisionNo = row.Field<string>("ARDivisionNo");
                            s.CustomerNo = row.Field<string>("CustomerNo");                            
                            s.ShipToCode = row.Field<string>("ShipToCode");
                            s.ShipToName = row.Field<string>("ShipToName");
                            s.ShipToAddress1 = row.Field<string>("ShipToAddress1");
                            s.ShipToAddress2 = row.Field<string>("ShipToAddress2");
                            s.ShipToAddress3 = row.Field<string>("ShipToAddress3");
                            s.ShipToCity = row.Field<string>("ShipToCity");
                            s.ShipToState = row.Field<string>("ShipToState");
                            s.ShipToZipCode = row.Field<string>("ShipToZipCode");
                            s.TelephoneNo = row.Field<string>("TelephoneNo");
                            s.EmailAddress = row.Field<string>("EmailAddress");
                            s.ContactCode = row.Field<string>("ContactCode");
                            s.ShipVia = row.Field<string>("ShipVia");
                            s.SalespersonNo = row.Field<string>("SalespersonNo");
                            s.WarehouseCode = row.Field<string>("WarehouseCode");
                            s.DateUpdated = row.Field<DateTime>("DateUpdated");
                            s.TimeUpdated = row.Field<string>("TimeUpdated");
                            Logger.LogDebug("shipTo:");
                            shiptos.Add(s);
                        }
                    }
                }
            }

            return shiptos;
        }

        public  decimal ConvertToDecimalTime(string time)
        {
            var parts = time.Split(':');
            return Math.Round(Convert.ToDecimal(parts[0]) + (Convert.ToDecimal(parts[1]) / 60) + (Convert.ToDecimal(parts[2]) / 3600), 5);
        }
        //public int getCount(string d, decimal t)
        //{
        //    int iCount = 0;
        //    using (var DbConnection = DbConn.GetodbcDbConnection())
        //    {


        //        try
        //        {
        //            OdbcCommand DbCommand = (OdbcCommand)DbConnection.CreateCommand();
        //            DbConnection.Open();

        //            DbCommand.CommandText = "select Count(*) from AR_Customer  where DateUpdated>=? and TimeUpdated>=? ";
        //            DbCommand.Parameters.Add("@date", OdbcType.DateTime).Value = d;  // "{"+"d"+date+"}";
        //            DbCommand.Parameters.Add("@time", OdbcType.Double).Value = t;

        //            Int32 count = Convert.ToInt32(DbCommand.ExecuteScalar());
        //            iCount = count;
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
        private int GetTotalRecordCount(OdbcConnection connection, string date, decimal time)
        {
            using (var command = new OdbcCommand("select Count(*) from AR_Customer  where DateUpdated >=? and TimeUpdated > ?", connection))
            {
                command.Parameters.Add("@date", OdbcType.DateTime).Value = date;
                command.Parameters.Add("@time", OdbcType.Double).Value = time;

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public CustomerCreate UpdateAddress(CustomerCreate model)
        {

            string homePath = this.Configuration.GetSection("AppSettings")["HomePath"];
            string companyCode = this.Configuration.GetSection("AppSettings")["CompanyCode"];
            string Username = this.Configuration.GetSection("AppSettings")["UserName"];
            string password = this.Configuration.GetSection("AppSettings")["password"];
            string accDate = DateTime.Now.ToString("yyyyMMdd");


            using (DispatchObject pvx1 = new DispatchObject("ProvideX.Script"))
            {
                pvx1.InvokeMethod("Init", homePath);
                // Instantiate a new Session object and initialize the session
                // by setting the user, company, date and module
                using (DispatchObject oSS1 = new DispatchObject(pvx1.InvokeMethod("NewObject", "SY_Session")))
                {
                    Logger.LogDebug("Inside dispatchobj:" + oSS1);
                    oSS1.InvokeMethod("nLogon"); //If the Logon method returns 0 then you must use the SetUser method to allow access
                    oSS1.InvokeMethod("nSetUser", new object[] { Username, password }); //returns 1
                    oSS1.InvokeMethod("nSetCompany", companyCode);//returns 1 successful
                    oSS1.InvokeMethod("nSetDate", "A/R", accDate);
                    oSS1.InvokeMethod("nSetModule", "A/R"); //returns 1 successful
                                                            // Get the Task ID for the AR_Customer_ui program
                    int TaskID = (int)oSS1.InvokeMethod("nLookupTask", "AR_Customer_ui");
                    oSS1.InvokeMethod("nSetProgram", TaskID);

                    using (DispatchObject arCustomer = new DispatchObject(pvx1.InvokeMethod("NewObject", "AR_Customer_bus", oSS1.GetObject())))
                    {
                        try
                        {

                            if (model == null)
                            {
                                Logger.LogDebug("CustomerCreate model is required");
                                return null;
                            }

                            //keeps the record in editable state
                            arCustomer.InvokeMethod("nSetKeyValue", "ARDivisionNo$", model.ARDivisionNo);
                            arCustomer.InvokeMethod("nSetKeyValue", "CustomerNo$", model.CustomerNo);
                            arCustomer.InvokeMethod("nSetKey");

                            arCustomer.InvokeMethod("nSetValue", "AddressLine1$", model.AddressLine1);
                            arCustomer.InvokeMethod("nSetValue", "AddressLine2$", model.AddressLine2);
                            if (model.AddressLine3 == "")
                            {
                                model.AddressLine3 = "No address line 3";
                            }
                            arCustomer.InvokeMethod("nSetValue", "AddressLine3$", model.AddressLine3);
                            arCustomer.InvokeMethod("nSetValue", "City$", model.City);
                            arCustomer.InvokeMethod("nSetValue", "State$", model.State);
                            arCustomer.InvokeMethod("nSetValue", "ZipCode$", model.ZipCode);
                            arCustomer.InvokeMethod("nSetValue", "CountryCode$", model.CountryCode);
                            arCustomer.InvokeMethod("nSetValue", "TelephoneNo$", model.TelephoneNo);
                            
                            Logger.LogDebug((string)arCustomer.GetProperty("sLastErrorMsg"));

                            var ReturnValue = arCustomer.InvokeMethod("nWrite");  //Return value 1 successful

                            Logger.LogDebug("Return Value:" + ReturnValue);
                            Logger.LogDebug((string)arCustomer.GetProperty("sLastErrorMsg"));

                        }
                        catch (Exception ex)
                        {
                            Logger.LogDebug(ex.Message);
                        }
                        finally
                        {
                            arCustomer.Dispose();
                        }

                    }
                    oSS1.Dispose();

                }
                pvx1.Dispose();
            }


            return model;

        }

        public string CheckCustomerExists(string CustomerNo)
        {
            string result = string.Empty;
            using (var DbConnection = DbConn.GetodbcDbConnection())
            {


                try
                {
                    OdbcCommand DbCommand = (OdbcCommand)DbConnection.CreateCommand();
                    DbConnection.Open();

                    DbCommand.CommandText = "select CustomerNo from AR_Customer  where CustomerNo='" + CustomerNo.ToUpper() + "'  ";

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


        private string RemoveWhitespace(string str)
        {
            //string str = " NET 30 ";
            string s = string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
            return s.ToLower();
        }


        public int getContactCount(string custID)
        {
            int iCount = 0;
            using (var DbConnection = DbConn.GetodbcDbConnection())
            {


                try
                {
                    OdbcCommand DbCommand = (OdbcCommand)DbConnection.CreateCommand();
                    DbConnection.Open();

                    DbCommand.CommandText = "select Count(*) from AR_CustomerContact  where CustomerNo='" + custID + "'  ";
                   

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


        public ContactLinkDelete DeleteContactLink(ContactLinkDelete model)
        {

            string homePath = this.Configuration.GetSection("AppSettings")["HomePath"];
            string companyCode = this.Configuration.GetSection("AppSettings")["CompanyCode"];
            string Username = this.Configuration.GetSection("AppSettings")["UserName"];
            string password = this.Configuration.GetSection("AppSettings")["password"];
            string accDate = DateTime.Now.ToString("yyyyMMdd");


            using (DispatchObject pvx1 = new DispatchObject("ProvideX.Script"))
            {
                pvx1.InvokeMethod("Init", homePath);
                // Instantiate a new Session object and initialize the session
                // by setting the user, company, date and module
                using (DispatchObject oSS1 = new DispatchObject(pvx1.InvokeMethod("NewObject", "SY_Session")))
                {
                    Logger.LogDebug("Inside dispatchobj:" + oSS1);
                    oSS1.InvokeMethod("nLogon"); //If the Logon method returns 0 then you must use the SetUser method to allow access
                    oSS1.InvokeMethod("nSetUser", new object[] { Username, password }); //returns 1
                    oSS1.InvokeMethod("nSetCompany", companyCode);//returns 1 successful
                    oSS1.InvokeMethod("nSetDate", "A/R", accDate);
                    oSS1.InvokeMethod("nSetModule", "A/R"); //returns 1 successful
                                                            // Get the Task ID for the AR_Customer_ui program
                    int TaskID = (int)oSS1.InvokeMethod("nLookupTask", "AR_Customer_ui");
                    oSS1.InvokeMethod("nSetProgram", TaskID);

                    using (DispatchObject arContact = new DispatchObject(pvx1.InvokeMethod("NewObject", "AR_CustomerContact_bus", oSS1.GetObject())))
                    {
                        try
                        {

                            if (model == null)
                            {
                                Logger.LogDebug("Contact Delete model is required");
                                return null;
                            }

                            //keeps the record in editable state
                            arContact.InvokeMethod("nSetKeyValue", "ARDivisionNo$", model.ARDivisionNo);
                            arContact.InvokeMethod("nSetKeyValue", "CustomerNo$", model.CustomerNo);
                            arContact.InvokeMethod("nSetKeyValue", "ContactCode$", model.ContactCode);
                            arContact.InvokeMethod("nSetKey");



                            Logger.LogDebug((string)arContact.GetProperty("sLastErrorMsg"));

                         
                            var ReturnValue = arContact.InvokeMethod("nDelete");
                            Logger.LogDebug("Return Value:" + ReturnValue);
                            Logger.LogDebug((string)arContact.GetProperty("sLastErrorMsg"));

                        }
                        catch (Exception ex)
                        {
                            Logger.LogDebug(ex.Message);
                        }
                        finally
                        {
                            arContact.Dispose();
                        }

                    }
                    oSS1.Dispose();

                }
                pvx1.Dispose();
            }


            return model;

        }


        public int CheckCustomerContactExists(string CustomerNo, string ContactCode)
        {
            int iCount = 0;
            using (var DbConnection = DbConn.GetodbcDbConnection())
            {


                try
                {
                    OdbcCommand DbCommand = (OdbcCommand)DbConnection.CreateCommand();
                    DbConnection.Open();

                    DbCommand.CommandText = "select Count(*) from AR_CustomerContact  where CustomerNo='" + CustomerNo.ToUpper() + "' and ContactCode ='" + ContactCode.ToUpper() + "' ";


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


        public Models.Customer GetCustomerDetails(string CustomerNo)
        {

            using (var dbConnection = DbConn.GetodbcDbConnection())
            {
                try
                {
                    dbConnection.Open();

                    // Fetch customers
                    var customers = GetCustomerinfo((OdbcConnection)dbConnection, CustomerNo);
                    if (customers == null )
                    {
                        return null;
                    }

                    // Fetch and map salerep details
                    //foreach (var cust in customers)
                    //{
                    customers.salesreps = GetSalesrepDetails((OdbcConnection)dbConnection, customers.SalespersonNo);
                    customers.contacts = GetContacts((OdbcConnection)dbConnection, customers.CustomerNo);
                    customers.shiptos = GetShippingAddress((OdbcConnection)dbConnection, customers.CustomerNo);
                    customers.TotalRecords = 0;
                    //}

                    return customers;

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

        private Customer GetCustomerinfo(OdbcConnection connection, string CustomerNo)
        {
            var customers = new Customer();
            
            using (var adapter = new OdbcDataAdapter())
            {
                var query = "SELECT * FROM  AR_Customer where CustomerNo =? ";
                using (var command = new OdbcCommand(query, connection))
                {
                    command.Parameters.Add("@cust", OdbcType.VarChar).Value = CustomerNo;
                    
                    adapter.SelectCommand = command;

                    var dataSet = new DataSet();
                    adapter.Fill(dataSet, "customers");

                    var dataTable = dataSet.Tables[0];
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            

                            customers.ARDivisionNo = row.Field<string>("ARDivisionNo");
                            customers.CustomerNo = row.Field<string>("CustomerNo");
                            customers.CustomerName = row.Field<string>("CustomerName");
                            customers.AddressLine1 = row.Field<string>("AddressLine1");
                            customers.AddressLine2 = row.Field<string>("AddressLine2");
                            customers.City = row.Field<string>("City");
                            customers.State = row.Field<string>("State");
                            customers.ZipCode = row.Field<string>("ZipCode");
                            customers.URLAddress = row.Field<string>("URLAddress");
                            customers.UDF_DEALERLEVEL = row.Field<string>("UDF_DEALERLEVEL");
                            customers.UDF_TERRITORY = row.Field<string>("UDF_TERRITORY");
                            customers.UDF_BUYING_GROUP = row.Field<string>("UDF_BUYING_GROUP");
                            customers.UDF_PERSONA = row.Field<string>("UDF_PERSONA");
                            customers.UDF_SAMSUNGPLATINUM = row.Field<string>("UDF_SAMSUNGPLATINUM");
                            customers.UDF_LGPINNACLENUMBER = row.Field<string>("UDF_LGPINNACLENUMBER");
                            customers.UDF_SONYDIAMOND = row.Field<string>("UDF_SONYDIAMOND");
                            customers.PrimaryShipToCode = row.Field<string>("PrimaryShipToCode");
                            customers.CountryCode = row.Field<string>("CountryCode");
                            customers.TelephoneNo = row.Field<string>("TelephoneNo");
                            customers.SalespersonNo = row.Field<string>("SalespersonNo");
                            customers.EmailAddress = row.Field<string>("EmailAddress");
                            customers.TermsCode = row.Field<string>("TermsCode");
                            customers.ShipMethod = row.Field<string>("ShipMethod");
                            customers.TaxSchedule = row.Field<string>("TaxSchedule");
                            customers.ContactCode = row.Field<string>("ContactCode");
                            customers.DateUpdated = row.Field<DateTime>("DateUpdated");
                            customers.TimeUpdated = row.Field<string>("TimeUpdated");
                            customers.TotalRecords = 0;

                            Logger.LogDebug("customer:");

                            
                        }
                    }
                }
            }

            return customers;
        }


        public ContactCreate UpdateContact(ContactCreate model)
        {
            string homePath = this.Configuration.GetSection("AppSettings")["HomePath"];
            Logger.LogDebug("Homepath:" + homePath);
            string companyCode = this.Configuration.GetSection("AppSettings")["CompanyCode"];
            Logger.LogDebug("companycode:" + companyCode);
            string Username = this.Configuration.GetSection("AppSettings")["UserName"];
            Logger.LogDebug("UN:" + Username);
            string password = this.Configuration.GetSection("AppSettings")["password"];
            Logger.LogDebug("password:" + password);
            string accDate = DateTime.Now.ToString("yyyyMMdd");
            using (DispatchObject pvx1 = new DispatchObject("ProvideX.Script"))
            {
                Logger.LogDebug("inside dispatch object:");
                pvx1.InvokeMethod("Init", homePath);
                using (DispatchObject oSS1 = new DispatchObject(pvx1.InvokeMethod("NewObject", "SY_Session")))
                {
                    Logger.LogDebug("step2-----:");
                    oSS1.InvokeMethod("nLogon"); //If the Logon method returns 0 then you must use the SetUser method to allow access
                    oSS1.InvokeMethod("nSetUser", new object[] { Username, password }); //returns 1
                    oSS1.InvokeMethod("nSetCompany", companyCode);//returns 1 successful
                    Logger.LogDebug("step3-----:");
                    oSS1.InvokeMethod("nSetDate", "A/R", accDate);
                    oSS1.InvokeMethod("nSetModule", "A/R"); //returns 1 successful
                    // Get the Task ID for the AR_Customer_ui program
                    int TaskID = (int)oSS1.InvokeMethod("nLookupTask", "AR_Customer_ui");
                    oSS1.InvokeMethod("nSetProgram", TaskID);
                    using (DispatchObject arContact = new DispatchObject(pvx1.InvokeMethod("NewObject", "AR_CustomerContact_bus", oSS1.GetObject())))
                    {
                        Logger.LogDebug("step4-----:");
                        try
                        {


                            //model.ContactCode = model.ContactCode.ToUpper();
                            //model.CustomerNo = model.CustomerNo.ToUpper();
                            Logger.LogDebug("ContactCodefinal$:" + model.ContactCode);

                            var retVal = arContact.InvokeMethod("nSetKeyValue", "ARDivisionNo$", model.ARDivisionNo);
                            retVal = arContact.InvokeMethod("nSetKeyValue", "CustomerNo$", model.CustomerNo.ToUpper());
                            retVal = arContact.InvokeMethod("nSetKeyValue", "ContactCode$", model.ContactCode.ToUpper());
                            retVal = arContact.InvokeMethod("nSetKey");

                            retVal = arContact.InvokeMethod("nSetValue", "ContactName$", model.ContactName.ToUpper());
                            retVal = arContact.InvokeMethod("nSetValue", "AddressLine1$", model.AddressLine1);
                            retVal = arContact.InvokeMethod("nSetValue", "AddressLine2$", model.AddressLine2);
                            if (model.AddressLine3 == "")
                            {
                                model.AddressLine3 = "No address line 3";
                            }
                            retVal = arContact.InvokeMethod("nSetValue", "AddressLine3$", model.AddressLine3);
                            retVal = arContact.InvokeMethod("nSetValue", "City", model.City);
                            retVal = arContact.InvokeMethod("nSetValue", "State$", model.State);
                            retVal = arContact.InvokeMethod("nSetValue", "ZipCode$", model.ZipCode);
                            retVal = arContact.InvokeMethod("nSetValue", "TelephoneNo1$", model.TelephoneNo1);
                            retVal = arContact.InvokeMethod("nSetValue", "EmailAddress$", model.EmailAddress);
                            retVal = arContact.InvokeMethod("nWrite");

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        finally
                        {
                            arContact.Dispose();

                        }

                    }

                    oSS1.Dispose();
                }
                pvx1.Dispose();
            }

            return model;
        }
    }
    }
