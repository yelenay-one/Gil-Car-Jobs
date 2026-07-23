using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
//using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Text.RegularExpressions;

namespace IntoIT.GilCar.Interfaces.CustomersCarInstallations
{
    public class DAL
    {
        SqlConnection conn;

        public DAL()
        {
            try
            {
                conn = new SqlConnection(ConfigurationManager.ConnectionStrings["GilCar_CN"].ConnectionString);
            }
            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in establishing connection to SQL DB.", ex.Message));
                throw ex;
            }
        }

        #region GetFromSQL

        public List<string> getDistincedDocNums()
        {
            List<string> tmp = new List<string>();

            SqlDataReader rdr = null;

            
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select distinct docnum from[SAP_CRM].[dbo].[ErpToCrm] where CRMImportDate is null and CRM_ERROR_MSG is null and convert(varchar, SAPExportDate, 111)  >=DATEADD(DAY, -0, convert(varchar, DATEADD(DAY, -7, GETDATE()), 111))", conn);
                
                cmd.CommandType = CommandType.Text;
                
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                   

                    tmp.Add(rdr["docnum"].ToString());
                }
                rdr.Close();
                cmd.Dispose();
                conn.Close();

                return tmp;
            }

            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.getDistincedDocNums.", ex.Message));
                throw ex;
            }

            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }

           
        }
        public List<Customer> GetAllCustomers(int docNum)
        {

            List<Customer> customers = new List<Customer>();
            SqlDataReader rdr = null;

            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("CustomersCarInstallations_GetAllCustomers", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                        new SqlParameter("@idFrom", System.Configuration.ConfigurationManager.AppSettings["from"]));
                cmd.Parameters.Add(
                        new SqlParameter("@idTo", System.Configuration.ConfigurationManager.AppSettings["to"]));
                cmd.Parameters.Add(new SqlParameter("@docNum", docNum));
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Customer currCustomer = new Customer();
                    currCustomer.ID = int.Parse(rdr["ID"].ToString());
                    currCustomer.DocNum = int.Parse(rdr["docnum"].ToString());
                    currCustomer.Name = rdr["U_Owner"].ToString();
                    string phone1 = Regex.Replace(rdr["U_XIS_tel1"].ToString(), @"[^\d]", String.Empty);
                    currCustomer.Phone1 = Regex.IsMatch(phone1, @"^(?:0(?!(5|7))(?:2|3|4|8|9))(?:-?\d){7}$|^(0(?=5|7)(?:-?\d){9})$") ? phone1 : String.Empty;
                    string phone2 = Regex.Replace(rdr["U_XIS_tel2"].ToString(), @"[^\d]", String.Empty);
                    currCustomer.Phone2 = Regex.IsMatch(phone2, @"^(?:0(?!(5|7))(?:2|3|4|8|9))(?:-?\d){7}$|^(0(?=5|7)(?:-?\d){9})$") ? phone2 : String.Empty;
                    currCustomer.Address = rdr["address"].ToString();
                    currCustomer.U_XIS_CustomerID = rdr["U_XIS_CustomerID"].ToString();

                    customers.Add(currCustomer);
                }
                rdr.Close();
                cmd.Dispose();
                conn.Close();

                return customers;
            }

            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.GetAllCustomers.", ex.Message));
                throw ex;
            }

            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
        }

        public List<Installation> GetAllReturns()
        {
            List<Installation> returns = new List<Installation>();
            SqlDataReader rdr = null;

            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("CustomersCarInstallations_GetAllReturns", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@idFrom", System.Configuration.ConfigurationManager.AppSettings["from_4th_run"]));
                cmd.Parameters.Add(
                    new SqlParameter("@idTo", System.Configuration.ConfigurationManager.AppSettings["to_4th_run"]));

                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    //Return currReturn = new Return();

                    //currReturn.ID = int.Parse(rdr["ID"].ToString());
                    //currReturn.LicenseNum = rdr["U_Rishui"].ToString();
                    //currReturn.ProductCode = rdr["ItemCode"].ToString();

                    //int num1;
                    //if (int.TryParse(rdr["docnum"].ToString(), out num1))
                    //    currReturn.DocNum = int.Parse(rdr["docnum"].ToString());
                    //else currReturn.DocNum = 0;

                    //returns.Add(currReturn);
                    Installation currReturn = new Installation();
                    currReturn.ID = int.Parse(rdr["ID"].ToString());
                    currReturn.ProductCode = rdr["itemcode"].ToString();
                    currReturn.ST_ID = rdr["ST_ID"].ToString();
                    currReturn.ODLN_cardname = rdr["ODLN_CardName"].ToString();
                    currReturn.ODLN_cardcode = rdr["ODLN_Cardcode"].ToString();
                    ///////////////////////////////////updated by gil 09/01/2020 19:34
              //      currReturn.ODLN_docdate = (DateTime?)rdr["ODLN_docdate"];
              //      currReturn.ODLN_docnum =(int?)rdr["ODLN_docnum"];
                    ///////////////////////////////////
                    currReturn.WharehouseCode = rdr["WhsCode"].ToString();
                    currReturn.SerialNum = rdr["DistNumber"].ToString();
                    string TelNum = Regex.Replace(rdr["U_TEL"].ToString(), @"[^\d]", String.Empty);
                    currReturn.TelNum = Regex.IsMatch(TelNum, @"^(?:0(?!(5|7))(?:2|3|4|8|9))(?:-?\d){7}$|^(0(?=5|7)(?:-?\d){9})$") ? TelNum : String.Empty;
                    currReturn.IMEINum = rdr["U_IMEI"].ToString();
                    currReturn.MACNum = rdr["U_MAC"].ToString();
                    currReturn.LicenseNum = rdr["U_Rishui"].ToString();

                    int num1;
                    if (int.TryParse(rdr["docnum"].ToString(), out num1))
                        currReturn.DocNum = int.Parse(rdr["docnum"].ToString());
                    else
                    {
                        int num2;
                        if (int.TryParse(rdr["ODLN_docnum"].ToString(), out num2))
                            currReturn.DocNum = int.Parse(rdr["ODLN_docnum"].ToString());
                        else
                            currReturn.DocNum = 0;
                    }

                    DateTime tempDate;
                    if (DateTime.TryParse(rdr["docdate"].ToString(), out tempDate))
                    {
                         currReturn.DocDate = tempDate;
                    }
                    else
                    {
                        DateTime tempDate1;
                        if (DateTime.TryParse(rdr["ODLN_docdate"].ToString(), out tempDate1))
                            currReturn.DocDate = tempDate1;
                    }


                    double tempInt;
                    bool quantitySuccess = double.TryParse(rdr["Quantity"].ToString(), out tempInt);
                    if (quantitySuccess) currReturn.Quantity = (int)tempInt;

                    
                   
                    returns.Add(currReturn);
                }
                rdr.Close();
                cmd.Dispose();
                conn.Close();

                return returns;
            }

            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.GetAllReturns.", ex.Message));
                throw ex;
            }

            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
        }


        public List<Car> GetCarBySapOrder(int docNum)
        {
            List<Car> cars = new List<Car>();
            SqlDataReader rdr = null;

            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("CustomersCarInstallations_GetCarBySapOrder", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@docNum", docNum));
                cmd.Parameters.Add(
                    new SqlParameter("@idFrom", System.Configuration.ConfigurationManager.AppSettings["from_2nd_run"]));
                cmd.Parameters.Add(
                    new SqlParameter("@idTo", System.Configuration.ConfigurationManager.AppSettings["to_2nd_run"]));
                //SqlCommand cmd = new SqlCommand("SELECT * FROM [dbo].[ODLN] WHERE CRMImportDate IS NULL and [SAPExportDate]>= '2018-01-01 00:00:00.000' and  Id >= " +
                // System.Configuration.ConfigurationManager.AppSettings["from_2nd_run"] + "  and id <= " + System.Configuration.ConfigurationManager.AppSettings["to_2nd_run"], conn);
                rdr = cmd.ExecuteReader();


                while (rdr.Read())
                {
                    Car currCar = new Car();
                    currCar.ID = int.Parse(rdr["ID"].ToString());
                    currCar.DocNum = int.Parse(rdr["docnum"].ToString());
                    currCar.ImporterCode = rdr["cardcode"].ToString().Trim();
                    //currCar.ImporterName_2 = rdr["Importer"].ToString().Trim();
                    currCar.Numatcard = rdr["numatcard"].ToString();
                    currCar.ChasisNum = rdr["U_Shilda"].ToString();
                    currCar.LicenseNum = rdr["U_Rishui"].ToString();
                    currCar.ColorCode = rdr["U_Color"].ToString();
                    currCar.Agency = rdr["U_Dealer"].ToString();
                    currCar.ManufacturerDesc = rdr["U_XIS_MNFDesc"].ToString();
                    currCar.MobileeyeNum = rdr["U_MobileyeNo"].ToString();
                    currCar.AgencyDesc = rdr["U_XISDealer"].ToString();

                    DateTime tempDate;
                    bool dateSuccess = DateTime.TryParse(rdr["docdate"].ToString(), out tempDate);
                    if (dateSuccess) currCar.DocDate = tempDate;

                    currCar.CarModel = rdr["U_Model"].ToString().Trim();
                    cars.Add(currCar);
                }
                rdr.Close();
                cmd.Dispose();
                conn.Close();

                return cars;
            }

            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.GetCarBySapOrder.", ex.Message));
                throw ex;
            }
            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
        }

        public List<Installation> GetInstallationBySapOrder(int docNum)
        {
            List<Installation> installations = new List<Installation>();
            SqlDataReader rdr = null;

            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("CustomersCarInstallations_GetInstallationBySapOrder", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@docNum", docNum));
                cmd.Parameters.Add(
                    new SqlParameter("@idFrom", System.Configuration.ConfigurationManager.AppSettings["from_3rd_run"]));
                cmd.Parameters.Add(
                    new SqlParameter("@idTo", System.Configuration.ConfigurationManager.AppSettings["to_3rd_run"]));
                //SqlCommand cmd = new SqlCommand("SELECT distinct d.*,o.U_Rishui FROM DLN1 d inner join ODLN o on d.docnum=o.docnum WHERE d.CRMImportDate IS NULL and d.SAPExportDate >= '2018-01-01 00:00:00.000' "+
                //"and d.Id >="+System.Configuration.ConfigurationManager.AppSettings["from_2nd_run"] +" and d.id <="+ System.Configuration.ConfigurationManager.AppSettings["to_2nd_run"], conn);

                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Installation currInstallation = new Installation();
                    currInstallation.ID = int.Parse(rdr["ID"].ToString());
                    currInstallation.ST_ID =rdr["ST_ID"].ToString();
                    currInstallation.DocNum = int.Parse(rdr["docnum"].ToString());
                    currInstallation.ProductCode = rdr["itemcode"].ToString();

                    currInstallation.ODLN_cardname = rdr["ODLN_CardName"].ToString();
                    currInstallation.ODLN_cardcode = rdr["ODLN_Cardcode"].ToString();
                    currInstallation.WharehouseCode = rdr["WhsCode"].ToString();
                    currInstallation.SerialNum = rdr["DistNumber"].ToString();
                    string TelNum = Regex.Replace(rdr["U_TEL"].ToString(), @"[^\d]", String.Empty);
                    currInstallation.TelNum = Regex.IsMatch(TelNum, @"^(?:0(?!(5|7))(?:2|3|4|8|9))(?:-?\d){7}$|^(0(?=5|7)(?:-?\d){9})$") ? TelNum : String.Empty;
                    currInstallation.IMEINum = rdr["U_IMEI"].ToString();
                    currInstallation.MACNum = rdr["U_MAC"].ToString();

                    //currInstallation.ParentProductCode = rdr["marketingcode"].ToString();
                    currInstallation.ParentProductCode = rdr["MarketingCode"].ToString();

                    double tempInt;
                    bool quantitySuccess = double.TryParse(rdr["Quantity"].ToString(), out tempInt);
                    if (quantitySuccess) currInstallation.Quantity = (int)tempInt;


                    DateTime tempDate;
                    bool dateSuccess = DateTime.TryParse(rdr["docdate"].ToString(), out tempDate);
                    if (dateSuccess) currInstallation.DocDate = tempDate;

                    currInstallation.LicenseNum = rdr["U_Rishui"].ToString();

                    installations.Add(currInstallation);
                }

                rdr.Close();
                cmd.Dispose();
                conn.Close();

                return installations;
            }

            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.GetInstallationBySapOrder.", ex.Message));
                throw ex;
            }
            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
        }
        #endregion

        #region UpdateSQL
        public void UpdateCustomerDateSAP(int customerID)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("CustomersCarInstallations_UpdateCustomer", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@id", customerID));
                cmd.Parameters.Add(
                    new SqlParameter("@updateDate", DateTime.Now));

                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }
            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.UpdateCustomerDateSAP.", ex.Message));
            }
            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
        }

        public void UpdateCarDateSAP(int carID)
        {
            //SqlDataReader rdr = null;
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("CustomersCarInstallations_UpdateCar", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@id", carID));
                cmd.Parameters.Add(
                    new SqlParameter("@updateDate", DateTime.Now));

                //rdr = cmd.ExecuteReader();
                //rdr.Close();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }
            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.UpdateCarDateSAP.", ex.Message));
            }

            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
        }

        public void UpdateInstallationDateSAP(string installationID)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("CustomersCarInstallations_UpdateInstallation", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@id", installationID));
                cmd.Parameters.Add(
                    new SqlParameter("@updateDate", DateTime.Now));

                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }
            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.UpdateInstallationDateSAP.", ex.Message));
            }

            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
        }

        public void UpdateReturnDateSAP_disabled(int returnID)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("CustomersCarInstallations_UpdateReturn", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@id", returnID));
                cmd.Parameters.Add(
                    new SqlParameter("@updateDate", DateTime.Now));

                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }
            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.UpdateReturnDateSAP.", ex.Message));
            }
            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
        }

        #endregion

        #region UpdateErrorToSAP
        public void UpdateCustomerErrorSAP(int ID, string message)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("CustomersCarInstallations_UpdateCustomer", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@id", ID));
                cmd.Parameters.Add(
                    new SqlParameter("@msg", message.Replace("'", "")));

                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }
            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.UpdateCustomerErrorSAP.", ex.Message));
            }
            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
        }

        public void UpdateCarErrorSAP(int ID, string message)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("CustomersCarInstallations_UpdateCar", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@id", ID));
                cmd.Parameters.Add(
                    new SqlParameter("@msg", message.Replace("'", "")));

                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }
            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.UpdateCarErrorSAP.", ex.Message));
            }
            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
        }

        public void UpdateInstallationErrorSAP(string ID, string message)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("CustomersCarInstallations_UpdateInstallation", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@id", ID));
                cmd.Parameters.Add(
                    new SqlParameter("@msg", message.Replace("'", "")));

                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }
            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.UpdateInstallationErrorSAP.", ex.Message));
            }
            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
        }

        public void UpdateReturnErrorSAP_disabled(int ID, string message)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("CustomersCarInstallations_UpdateReturn", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@id", ID));
                cmd.Parameters.Add(
                    new SqlParameter("@msg", message.Replace("'", "")));

                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }
            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.UpdateReturnErrorSAP.", ex.Message));
            }
            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
        }
        #endregion
    }
}
