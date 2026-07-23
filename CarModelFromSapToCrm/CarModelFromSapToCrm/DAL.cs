using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text.RegularExpressions;
using LogManager;

namespace CarModelFromSapToCrm
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
                       
                throw ex;
            }
        }

        #region GetFromSQL
        public DataTable GetAllModels()
        {
            DataTable models = new DataTable();
            try
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT *  FROM dbo.MODEL WHERE CRMImportDate IS NULL";
                SqlDataAdapter da = null;
                using (da = new SqlDataAdapter(cmd))
                {
                    da.Fill(models);
                }
                cmd.Dispose();
                conn.Close();                           
            }

            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.GetAllModels.", ex.Message));
                //throw ex;
                return null;
            }

            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
            return models;
        }


 
        #endregion


        #region UpdateToSAP


        public void UpdateCrmCRMImportDate(string code)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                //cmd.CommandText = "UPDATE dbo.MODEL SET CRMImportDate='" +DateTime.Now.Year+"-" +DateTime.Now.Month+"-"+DateTime.Now.Day+ "' WHERE Code='" + code + "'";
                cmd.CommandText = "UPDATE dbo.MODEL SET CRMImportDate='" + DateTime.Now.ToString("yyyy-MM-dd") + "' WHERE Code='" + code + "'";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }

            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in DAL.UpdateCrmCRMImportDate.", ex.Message));
                //throw ex;

            }

            finally
            {
                if (conn != null && !(conn.State == ConnectionState.Closed))
                    conn.Close();
            }
        }

        public void UpdateError(string error, string code)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"UPDATE dbo.MODEL SET CRM_ERROR_MSG='" + error.Replace("'","") + "' WHERE Code='" + code+"'";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }

            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in UpdateError", ex.Message));
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
