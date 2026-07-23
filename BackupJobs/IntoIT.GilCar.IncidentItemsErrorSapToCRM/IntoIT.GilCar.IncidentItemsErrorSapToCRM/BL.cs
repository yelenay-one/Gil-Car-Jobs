using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntoIT.GilCar.IncidentItemsErrorSapToCRM
{
    class BL
    {
        DAL dal;
        CrmBase crmBase;
        public void StartInterface(DateTime start)
        {
           dal = new DAL();
           crmBase = new CrmBase(ConfigurationManager.AppSettings["CRM_Url"]);
           int count = 0;
           int totalRows = 0;
           int Errors = 0;
           int success = 0;
           List<SapError> sapErrors= dal.GetSapErrors();
           if (sapErrors != null && sapErrors.Count > 0)
           {
               totalRows = sapErrors.Count;
               foreach (SapError item in sapErrors)
               {
                   Entity incidentItem_entity = new Entity("new_incident_items");
                   incidentItem_entity.Id = item.guid;
                   incidentItem_entity["new_s_sap_error"] = item.ErrorLog;
                   try
                   {
                       crmBase.XrmService.Update(incidentItem_entity);
                       count++;
                       success++;
                   }
                   catch (Exception ex)
                   {
                       string msg = string.Format("Error in UpdateCrmDate: {0}", ex);
                       LogManager.Logging.WriteToLog(msg);
                       Errors++;
                   }
                  
                   
               }
             
           }
           LogManager.Logging.WriteToLog("IncidentItemsErrorSapToCRM num of items that were Update : " + count);
           CreateNewInterfaceLog(start, "IncidentItemsErrorSapToCRM  CRM_ORDR", totalRows, success, Errors);
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
