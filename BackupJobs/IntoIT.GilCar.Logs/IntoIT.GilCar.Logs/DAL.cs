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

namespace IntoIT.GilCar.Logs
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

        public int GetErrorNum(string table, string column_name, string where_condition_column)
        {
            int res;
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT COUNT("+column_name+") FROM [SAP_CRM].[dbo].["+table+"] WHERE ["+where_condition_column+"]<>'null'", conn);
                cmd.CommandType = CommandType.Text;

                res = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.Dispose();
                conn.Close();

                return res;
            }

            catch (Exception ex)
            {

                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in "+table, ex.Message));
                return 0;
            }
        }

        public int GetDNL1ErrorNum() 
        {
            int res;
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT COUNT([ID]) FROM [SAP_CRM].[dbo].[DLN1] WHERE SAPExportDate is not NULL and CRMImportDate is null and CRM_ERROR_MSG='null'", conn);
                cmd.CommandType = CommandType.Text;

                res = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.Dispose();
                conn.Close();

                return res;
            }

            catch (Exception ex)
            {

                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in  GetDNL1ErrorNum ", ex.Message));
                return 0;
            }
        }

  
    }
}
