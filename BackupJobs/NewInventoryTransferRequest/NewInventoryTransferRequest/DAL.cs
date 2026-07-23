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

namespace NewInventoryTransferRequest
{
    public class DAL
    {
        protected SQLHelper sql;

        public DAL()
        {
            try
            {
                sql = new SQLHelper();

            }
            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in establishing connection to SQL DB.", ex.Message));
                throw ex;
            }
        }


        public bool IsAccountExist(string accountId)
        {
            string sqlText = "SELECT COUNT(1) FROM AccountsInt WHERE [Account_CRM_ID] = N'" + accountId + "'";
            if (sql.ExecuteScalar(sqlText) > 0)
                return true;
            else
                return false;

        }


        public void InsertInventoryTransferRequest(NewInventoryTransferRequest newInventoryTransferRequest)
        {
            string sqlText = "INSERT INTO [dbo].[New_Inventory_Transfer_Request]"
                              + "("
                              + "[new_request_num]"
                              + ",[ownerid]"
                              + ",[new_warehouse_num]"
                              + ",[createdon]" 
                              + ",[Status_request]"
                              + ")";
            sqlText += "VALUES";
            sqlText += "(";
            sqlText +=sql.SqlProof(newInventoryTransferRequest.new_request_num);
            sqlText += ",N'" + sql.SqlProof(newInventoryTransferRequest.ownerid)+"'";          
            sqlText += "," + sql.SqlProof(newInventoryTransferRequest.new_warehouse_num);
            sqlText += ",'" +newInventoryTransferRequest.createdon.ToString("yyyy-MM-dd HH:mm:ss")+"'";
            sqlText += ",0)";
            sql.ExecuteNonQuery(sqlText);
        }

        public void NewInventoryTransferlines(NewInventoryTransferlines newInventoryTransferlines,int lineNum)
        {
            string sqlText = "INSERT INTO [dbo].[New_Inventory_Transfer_lines]"
                              + "("
                              + "[new_request_num]"
                              + ",[Line_Num]"
                              + ",[new_item_id]"
                              + ",[new_qty]"
                              + ",[new_serial_num]"
                              + ")";
            sqlText += "VALUES";
            sqlText += "(";
            sqlText += sql.SqlProof(newInventoryTransferlines.new_request_num);
            sqlText+=","+ lineNum;
            sqlText += ",N'" + sql.SqlProof(newInventoryTransferlines.new_item_id)+"'";
            sqlText += "," + sql.SqlProof(newInventoryTransferlines.new_qty);
            sqlText += ",N'" + sql.SqlProof(newInventoryTransferlines.new_serial_num)+"'";
            sqlText += ")";
            sql.ExecuteNonQuery(sqlText);

        }
        //private void UpdateAccount(Account acc)
        //{
        //    string sqlText = "UPDATE AccountsInt SET "
        //               + "[Account_name] = N'" + sql.SqlProof(acc.name) + "'"
        //               + ",[new_account_type] = N'" + sql.SqlProof(acc.new_account_type) + "'"
        //               + ",[new_lab_type] = N'" + sql.SqlProof(acc.new_lab_type) + "'"
        //               + ",[accountnumber] = N'" + sql.SqlProof(acc.accountnumber) + "'"
        //               + ",[parentaccountid] = N'" + sql.SqlProof(acc.parentaccountid) + "'"
        //               + ",[parentaccount_name] = N'" + sql.SqlProof(acc.parentaccountname) + "'"
        //               + ",[primarycontactid] = N'" + sql.SqlProof(acc.primarycontactid) + "'"
        //               + ",[new_belonging_group] = N'" + sql.SqlProof(acc.new_belonging_group) + "'"
        //               + ",[transactioncurrencyid] = N'" + sql.SqlProof(acc.transactioncurrencyid) + "'"
        //               //+ ",[defaultpricelevelid] = N'" + sql.SqlProof(acc.defaultpricelevelid) + "'"
        //               + ",[statecode] = N'" + sql.SqlProof(acc.statecode) + "'"
        //               + ",[address1_addresstypecode] = N'" + sql.SqlProof(acc.address1_addresstypecode) + "'"
        //               + ",[address1_line1] = N'" + sql.SqlProof(acc.address1_line1) + "'"
        //               + ",[address1_line2] = N'" + sql.SqlProof(acc.address1_line2) + "'"
        //               + ",[address1_line3] = N'" + sql.SqlProof(acc.address1_line3) + "'"
        //               + ",[address1_city] = N'" + sql.SqlProof(acc.address1_city) + "'"
        //               + ",[new_institute_id] = N'" + sql.SqlProof(acc.new_institute_id) + "'"
        //               + ",[address2_addresstypecode] = N'" + sql.SqlProof(acc.address2_addresstypecode) + "'"
        //               + ",[address2_line1] = N'" + sql.SqlProof(acc.address2_line1) + "'"
        //               + ",[address2_line2] = N'" + sql.SqlProof(acc.address2_line2) + "'"
        //               + ",[address2_line3] = N'" + sql.SqlProof(acc.address2_line3) + "'"
        //               + ",[address2_city] = N'" + sql.SqlProof(acc.address2_city) + "'"
        //               + ",[paymenttermscode] = N'" + sql.SqlProof(acc.paymenttermscode) + "'"
        //               + ",[telephone1] = N'" + sql.SqlProof(acc.telephone1) + "'"
        //               + ",[telephone2] = N'" + sql.SqlProof(acc.telephone2) + "'"
        //               + ",[fax] = N'" + sql.SqlProof(acc.fax) + "'"
        //               + ",[emailaddress1] = N'" + sql.SqlProof(acc.emailaddress1) + "'"
        //               + ",[new_site_code] = N'" + sql.SqlProof(acc.new_site_code) + "'"
        //               + ",[new_coa] = N'" + sql.SqlProof(acc.new_coa) + "'"
        //               + ",[new_orders_responsible] = N'" + sql.SqlProof(acc.new_orders_responsible) + "'"
        //               + ",[new_rt_pcr_salesrep] = N'" + sql.SqlProof(acc.new_rt_pcr_salesrep) + "'"
        //               + ",[donotbulkemail] = N'" + sql.SqlProof(acc.donotbulkemail) + "'"
        //               + ",[new_send_auto_updates] = N'" + sql.SqlProof(acc.new_send_auto_updates) + "'"
        //               + ",[new_nipendo_customer] = N'" + sql.SqlProof(acc.new_nipendo_customer) + "'"
        //               + ",[new_active_warehouse] = N'" + sql.SqlProof(acc.new_active_warehouse) + "'"
        //               + ",[new_blanket_agreement] = N'" + sql.SqlProof(acc.new_blanket_agreement) + "'"
        //               + ",[new_standing_purchase] = N'" + sql.SqlProof(acc.new_standing_purchase) + "'"
        //               + ",[new_minimum_order] = N'" + sql.SqlProof(acc.new_minimum_order) + "'"
        //               + ",[new_print_manufature_item] = N'" + sql.SqlProof(acc.new_print_manufature_item) + "'"
        //               //+ ",[new_credit_balance] = N'" + acc.new_credit_balance + "'"
        //               //+ ",[new_purchase_standing_balance] = N'" + acc.new_purchase_standing_balance + "'"
        //               //  + ",[new_export_date_from_crm] = '" + acc.new_export_date_from_crm.ToString("MM/dd/yyyy HH:mm:ss") + "'"
        //               + ",[new_export_date_from_crm] = '" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + "'"
        //               + ",[description] = N'" + sql.SqlProof(acc.description) + "'"
        //               + ",[new_bookkeeping_notes] = N'" + sql.SqlProof(acc.new_bookkeeping_notes) + "'"
        //               + ",[contact.mobilephone] = N'" + sql.SqlProof(acc.primary_contact_phone) + "'"
        //               + ",[contact.emailaddress1] = N'" + sql.SqlProof(acc.primary_contact_email) + "'"
        //               + ",[new_consumables_salesrep]=N'" + sql.SqlProof(acc.new_consumables_salesrep) + "'"
        //               + ",[new_department]=N'" + sql.SqlProof(acc.new_department) + "'"
        //               + ",[new_mobile_phone]=N'" + sql.SqlProof(acc.new_mobile_phone) + "'"
        //               + ",[new_eng_customer_name]=N'" + sql.SqlProof(acc.new_eng_customer_name) + "'"
        //               + " WHERE Account_CRM_ID = N'" + acc.accountid + "'";

        //    sql.ExecuteNonQuery(sqlText);
        //}



    }
}
