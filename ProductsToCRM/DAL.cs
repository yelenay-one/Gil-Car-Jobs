using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
//using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;

namespace IntoIT.GilCar.Interfaces.ProductsToCRM
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

        public List<Product> GetAllProducts()
        {
            List<Product> products = new List<Product>();
            SqlDataReader rdr = null;

            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("ProductsToCRM_GetAllProducts", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Product currProduct = new Product();
                    currProduct.ProductCode = rdr["ItemCode"].ToString();
                    currProduct.ProductName = rdr["itemName"].ToString();
                    currProduct.ProductFamily = rdr["ItmsGrpCod"].ToString();
                    currProduct.IsLeadingItem = rdr["LeadingItem"].ToString() == "Y" ? true : false;
                    currProduct.ValidFor = rdr["ValidFor"].ToString() == "Y" ? true : false;
                    currProduct.SerialManaged = rdr["SerNum"].ToString() == "Y" ? true : false;

                    products.Add(currProduct);
                }
                rdr.Close();
                cmd.Dispose();
                conn.Close();

                return products;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<ProductParent> GetAllParentProducts()
        {
            List<ProductParent> parentProducts = new List<ProductParent>();
            SqlDataReader rdr = null;

            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("ProductsToCRM_GetAllParentProducts", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    ProductParent currParentProduct = new ProductParent();
                    currParentProduct.SonProductName = rdr["SonName"].ToString();
                    currParentProduct.ParentProductName = rdr["FatherName"].ToString();
                    currParentProduct.SonProductCode = rdr["SonCode"].ToString();
                    currParentProduct.ParentProductCode = rdr["FatherCode"].ToString();
                    currParentProduct.IsSonLeadingItem = rdr["LeadingItem"].ToString() == "Y" ? true : false;

                    parentProducts.Add(currParentProduct);
                }
                rdr.Close();
                cmd.Dispose();
                conn.Close();

                return parentProducts;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateExportDateProducts(string productCode)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("ProductsToCRM_UpdateExportDateProducts", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@itemCode", productCode));
                cmd.Parameters.Add(
                    new SqlParameter("@updateDate", DateTime.Now));

                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public void UpdateExportDateParentProducts(string parentProductCode, string sonProductCode)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("ProductsToCRM_UpdateExportDateParentProducts", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@sonCode", parentProductCode));
                cmd.Parameters.Add(
                   new SqlParameter("@fatherCode", sonProductCode));
                cmd.Parameters.Add(
                    new SqlParameter("@updateDate", DateTime.Now));

                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public void UpdateErrorProducts(string productCode, string msg)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("ProductsToCRM_UpdateExportDateProducts", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@itemCode", productCode));
                cmd.Parameters.Add(
                    new SqlParameter("@msg", msg.Replace("'","")));

                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void UpdateErrorParentProducts(string parentProductCode, string sonProductCode,string msg)
        {
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("ProductsToCRM_UpdateExportDateParentProducts", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(
                    new SqlParameter("@sonCode", sonProductCode));
                cmd.Parameters.Add(
                   new SqlParameter("@fatherCode",parentProductCode ));
                cmd.Parameters.Add(
                    new SqlParameter("@msg", msg.Replace("'","")));

                cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
