using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;


namespace IntoIT.GilCar.IncidentItemsErrorSapToCRM
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



        public List<SapError> GetSapErrors()
        {
            try
            {
                List<SapError> sapErrors = new List<SapError>();

                SqlDataReader rdr = null;
                conn.Open();
                SqlCommand cmd = new SqlCommand("IncidentItemsToSAP_GetSapError", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                //for test only 
               // DateTime errorDate = new DateTime(2016, 9, 8);
                DateTime errorDate = DateTime.Today;
                cmd.Parameters.Add(new SqlParameter("@ErrorDate", errorDate));
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    SapError se = new SapError();
                    se.ErrorLog = rdr["ErrorLog"].ToString();
                  se.SAPFailedDate =(DateTime) rdr["SAPFailedDate"];
                   se.guid= new Guid(rdr["guid"].ToString());
                   sapErrors.Add(se);
                }
                cmd.Dispose();
                conn.Close();
                return sapErrors;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
