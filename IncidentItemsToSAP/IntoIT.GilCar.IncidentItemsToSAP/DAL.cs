using IntoIT.GilCar.IncidentItemsToSAP.Entities;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace IntoIT.GilCar.IncidentItemsToSAP
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
        #region Check if IncidentItem exists in SAP
        public int IsIncidentItemExistsSAP(Guid guid)
        {
            int res;

            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("IncidentItemsToSAP_IsIncidentItemExistsSAP", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@guid", guid.ToString()));

                res = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.Dispose();
                conn.Close();

                return res;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Insert IncidentItem to SAP
        public void InsertIncidentItemSAP(IncidentItem incidentItem)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("IncidentItemsToSAP_InsertIncidentItem", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(
                   new SqlParameter("@guid", incidentItem.IncidentItemGUID));
                cmd.Parameters.Add(
                    new SqlParameter("@incident_id", incidentItem.IncidentCode));
                cmd.Parameters.Add(
                    new SqlParameter("@card_code", incidentItem.CardCode));
                cmd.Parameters.Add(
                    new SqlParameter("@whs_code", incidentItem.WhsCode));
                cmd.Parameters.Add(
                    new SqlParameter("@createdon", DateTime.Now.AddMinutes(-20).ToString("MM/dd/yyyy hh:mm:ss")));//incidentItem.Createdon
                cmd.Parameters.Add(
                    new SqlParameter("@car_id", incidentItem.LicenseNum));
                cmd.Parameters.Add(
                    new SqlParameter("@item_code", incidentItem.ProductCode));
                cmd.Parameters.Add(
                    new SqlParameter("@item_sn", incidentItem.ItemSN));
                cmd.Parameters.Add(
                    new SqlParameter("@qty", incidentItem.Quantity));
                cmd.Parameters.Add(
                    new SqlParameter("@shilda", incidentItem.Shilda));
                cmd.Parameters.Add(
                    new SqlParameter("@owner", incidentItem.Owner));
                cmd.Parameters.Add(
                    new SqlParameter("@model", incidentItem.ModelName));
                cmd.Parameters.Add(
                    new SqlParameter("@NewCharge", incidentItem.NewCharge));
                cmd.Parameters.Add(
                    new SqlParameter("@New_s_technician_card_code", incidentItem.new_s_technician_card_code));
                cmd.Parameters.Add(
                   new SqlParameter("@new_p_installations_classification", incidentItem.new_p_installations_classification));
                cmd.Parameters.Add(
               new SqlParameter("@NumAtCard", incidentItem.OrderNumber));
                cmd.Parameters.Add(
             new SqlParameter("@solutioncode", incidentItem.new_solution_code));
                cmd.Parameters.Add(
             new SqlParameter("@description", incidentItem.description));
                cmd.Parameters.Add(
             new SqlParameter("@price", incidentItem.price));
                cmd.Parameters.Add(
             new SqlParameter("@totalprice", incidentItem.totalprice));
                
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}

