using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;


namespace IntoIT.GilCar.Logs
{
    public class BL
    {
        DAL dal;
        CrmBase crmBase;

        public BL()
        {
            try
            {
                dal = new DAL();
                crmBase = new CrmBase(ConfigurationManager.AppSettings["CRM_Url"]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void StartInterface(DateTime start)
        {
            int errorNum = 0;
             errorNum = dal.GetErrorNum("CloseCases", "docnum", "CRM_ERROR_MSG");
            CreateNewInterfaceLog(start, "[dbo].[CloseCases]", 0, 0, errorNum);
            errorNum = dal.GetErrorNum("CRM_ORDR", "guid", "ErrorLog");
            CreateNewInterfaceLog(start, "[dbo].[CRM_ORDR]", 0, 0, errorNum);
            errorNum = dal.GetErrorNum("DLN1", "ID", "CRM_ERROR_MSG");
            CreateNewInterfaceLog(start, "[dbo].[DLN1]", 0, 0, errorNum);
            errorNum = dal.GetErrorNum("MODEL", "Code", "CRM_ERROR_MSG");
            CreateNewInterfaceLog(start, "[dbo].[MODEL]", 0, 0, errorNum);
            errorNum = dal.GetErrorNum("ODLN", "ID", "CRM_ERROR_MSG");
            CreateNewInterfaceLog(start, "[dbo].[ODLN]", 0, 0, errorNum);
            errorNum = dal.GetErrorNum("ODLN_CUST", "ID", "CRM_ERROR_MSG");
            CreateNewInterfaceLog(start, "[dbo].[ODLN_CUST]", 0, 0, errorNum);
            errorNum = dal.GetErrorNum("ODLN2", "ID", "CRM_ERROR_MSG");
            CreateNewInterfaceLog(start, "[dbo].[ODLN2]", 0, 0, errorNum);
            errorNum = dal.GetErrorNum("OITM", "ItemCode", "CRM_ERROR_MSG");
            CreateNewInterfaceLog(start, "[dbo].[OITM]", 0, 0, errorNum);
            errorNum = dal.GetErrorNum("OITT", "FatherCode", "CRM_ERROR_MSG");
            CreateNewInterfaceLog(start, "[dbo].[OITT]", 0, 0, errorNum);
            errorNum = dal.GetErrorNum("RDN1", "ID", "CRM_ERROR_MSG");
            CreateNewInterfaceLog(start, "[dbo].[RDN1]", 0, 0, errorNum);
            errorNum = dal.GetDNL1ErrorNum();
            CreateNewInterfaceLog(start, "[SAP_CRM].[dbo].[DLN1] WHERE SAPExportDate is not NULL and CRMImportDate is null and CRM_ERROR_MSG='null'", 0, 0, errorNum);
        }
        public void CreateNewInterfaceLog(DateTime start, string name, int new_i_interface_total_lines, int new_i_success_total_lines, int new_i_error_total_lines)
        {
            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
            start = start.AddHours(offset.Hours);
            Entity new_interface_log = new Entity("new_interface_log");
            new_interface_log["new_d_start_date"] = start;
            new_interface_log["new_d_end_date"] = DateTime.Now.AddHours(offset.Hours);
            new_interface_log["new_name"] = name;
            new_interface_log["new_i_interface_total_lines"] = new_i_interface_total_lines;
            new_interface_log["new_i_success_total_lines"] = new_i_success_total_lines;
            new_interface_log["new_i_error_total_lines"] = new_i_error_total_lines;
            crmBase.XrmService.Create(new_interface_log);
        }     
    
    }

}
