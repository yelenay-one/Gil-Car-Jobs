using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace NewInventoryTransferRequest
{
    public class SQLHelper
    {
        SqlConnection conn;

        public SQLHelper()
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
    
        public DataTable GetDataTable(string sql)
        {
            DataTable dt = new DataTable();
            
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandType = CommandType.Text;

                using (SqlDataAdapter a = new SqlDataAdapter(cmd))
                {
                    a.Fill(dt);
                }

                conn.Close();

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
        }
        
        public void ExecuteNonQuery(string sqlText)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqlText, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
        }
        public int ExecuteScalar(string sqlText)
        {
            int res = 0;
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sqlText, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                res = (int)cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return res;
        }
        public DataTable GetData(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                SqlDataAdapter ad = new SqlDataAdapter(cmd);
                ad.Fill(dt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return dt;
        }
        public string GetRowStringValue(object o)
        {
            if (o == DBNull.Value)
                return string.Empty;
            return o.ToString().Trim();
        }
        public decimal GetRowDecimalValue(object o)
        {
            if (o == DBNull.Value)
                return 0;
            return Convert.ToDecimal(o);
        }
        public int GetRowIntValue(object o)
        {
            if (o == DBNull.Value)
                return 0;
            return Convert.ToInt32(o);
        }
        public bool GetRowBoolValue(object o)
        {
            bool res = false;
            if (o != DBNull.Value)
            {
                switch (o.ToString().ToLower())
                {
                    case "1":
                    case "y":
                    case "true":
                        res = true;
                        break;
                }
            }
            return res;
        }
        public DateTime GetRowDateTimeValue(object o)
        {
            if (o == DBNull.Value)
                return DateTime.MinValue;
            return Convert.ToDateTime(o);
        }
        public string SqlProof(string s)
        {
            string res = null;
            if (!string.IsNullOrWhiteSpace(s))
                res = s.Replace("'", "''");
            return res;
        }
    }
}
