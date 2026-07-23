using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
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
        public DataTable GetAllCases()
        {
            DataTable cases = new DataTable();
            try
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT docnum,newcarid,Branch,casetypecode,scheduledstart,new_solution_code,description,SAPExportDate,CRMImportDate,CRM_ERROR_MSG, ServiceCallNum FROM [SAP_CRM].[dbo].[CloseCases] WHERE CRMImportDate IS NULL AND CRM_ERROR_MSG IS NULL ";
                SqlDataAdapter da = null;
                using (da = new SqlDataAdapter(cmd))
                {
                    da.Fill(cases);
                }
                cmd.Dispose();
                conn.Close();                           
            }

            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.GetAllCases.", ex.Message));
                //throw ex;
                return null;
            }

            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
            return cases;
        }
 
        #endregion


        #region UpdateToSAP

        public void UpdateCase(int docNum)
        {           
            try
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                DateTime now = DateTime.Now;
                cmd.CommandText = "UPDATE [SAP_CRM].[dbo].[CloseCases] SET CRMImportDate=CAST('"+now.ToString("yyyy-MM-dd HH:mm:ss")+"' AS DATETIME) WHERE docnum=" + docNum;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }

            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.GetAllCases.", ex.Message));
                //throw ex;
              
            }

            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
          
        }

        public void UpdateError(string error, int docNum)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "UPDATE [SAP_CRM].[dbo].[CloseCases] SET CRM_ERROR_MSG='" + error.Replace("'", "") + "' WHERE docnum=" + docNum;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }

            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.GetAllCases.", ex.Message));
                //throw ex;

            }

            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
        }

        #endregion

        //#region UpdateErrorToSAP
     
        //#endregion
    }
}
