
using LogManager;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarModelFromSapToCrm
{
    class BL
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
            int index=0;
            int totalRows = 0;
            int Errors = 0;
            int success = 0;
            DataTable models = dal.GetAllModels();

            totalRows = models.Rows.Count;
            if (models != null)
            {
                foreach (DataRow row in models.Rows)
                {
                    try
                    {
                        QueryExpression query;
                        EntityCollection coll;
                        query = new QueryExpression("new_importer");
                        coll = crmBase.XrmService.RetrieveMultiple(query);
                        Entity model = new Entity("new_car_model");
                        model["new_s_code"] = row["Code"].ToString();
                        model["new_s_description"] = row["Description"].ToString();
                      
                        if (!string.IsNullOrEmpty(row["Mnf_CRM"].ToString()))
                        {
                            query = new QueryExpression("new_importer");
                            query.Criteria.AddCondition("new_sap_customer_id", ConditionOperator.Equal, row["Mnf_CRM"].ToString());
                            coll = crmBase.XrmService.RetrieveMultiple(query);
                            if (coll != null && coll.Entities.Count > 0)
                            {
                                model["new_importer_id"] = new EntityReference("new_importer", coll.Entities[0].Id);
                            }
                        }
                        if (!string.IsNullOrEmpty(row["importer_brand_id"].ToString()))
                        {
                            // new_car_categoryId
                            query = new QueryExpression("new_importer_brand");
                            query.Criteria.AddCondition("new_name", ConditionOperator.Equal, row["importer_brand_id"].ToString().Trim());
                            coll = crmBase.XrmService.RetrieveMultiple(query);
                            if (coll != null && coll.Entities.Count > 0)
                            {

                                model["new_importer_brand_id"] = new EntityReference("new_importer_brand", coll.Entities[0].Id);
                            }

                        }

                        if (!string.IsNullOrEmpty(row["new_car_categoryId"].ToString()))
                        {
                            query = new QueryExpression("new_car_category");
                            query.Criteria.AddCondition("new_code_car_categoryid", ConditionOperator.Equal, row["new_car_categoryId"].ToString());
                            coll = crmBase.XrmService.RetrieveMultiple(query);
                            if (coll != null && coll.Entities.Count > 0)
                            {
                                model["new_car_category_id"] = new EntityReference("new_car_category", coll.Entities[0].Id);
                            }
                        }


                        if (!string.IsNullOrEmpty(row["new_car_familyId"].ToString()))
                        {
                            query = new QueryExpression("new_car_family");
                            query.Criteria.AddCondition("new_code_car_familyid", ConditionOperator.Equal, row["new_car_familyId"].ToString());
                            coll = crmBase.XrmService.RetrieveMultiple(query);
                            if (coll != null && coll.Entities.Count > 0)
                            {

                                model["new_car_family_id"] = new EntityReference("new_car_family", coll.Entities[0].Id);
                            }
                        }




                        if (!string.IsNullOrEmpty(row["P"].ToString()))
                        {
                            string p = row["P"].ToString();
                            if (p == "Y")
                                model["new_premium"] = true;
                            else
                                model["new_premium"] = false;

                        }


                        if (!string.IsNullOrEmpty(row["A"].ToString()))
                            model["new_i_warranty"] = int.Parse(row["A"].ToString());

                        model["new_u_center"] = row["center"].ToString();
                         
                        // model["new_i_warranty"] = row["A"].ToString();   Description
                        query = new QueryExpression("new_car_model");
                        query.Criteria.AddCondition("new_s_code", ConditionOperator.Equal, model["new_s_code"].ToString());
                       // query.Criteria.AddCondition("new_s_description", ConditionOperator.Equal, model["new_s_description"].ToString());
                        query.ColumnSet = new ColumnSet("new_s_code", "new_s_description");
                       
                        coll = crmBase.XrmService.RetrieveMultiple(query);
                        if (coll != null && coll.Entities.Count > 0)
                        {
                            model.Id = coll.Entities[0].Id;
                            crmBase.XrmService.Update(model);
                            dal.UpdateCrmCRMImportDate(row["Code"].ToString());
                        }
                        else
                        {
                            crmBase.XrmService.Create(model);
                        }
                        success++;
                       // Console.WriteLine(index);
                    }
                    catch (Exception ex)
                    {
                        Errors++;
                        dal.UpdateError(ex.Message, row["Code"].ToString());
                        LogManager.Logging.WriteToLog("on code : " + row["Code"].ToString() + " "+ex.Message + " " + ex.StackTrace);
                    }                 
                }
            }
            CreateNewInterfaceLog(start, "CarModelFromSapToCrm dbo.MODEL", totalRows, success, Errors);

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
