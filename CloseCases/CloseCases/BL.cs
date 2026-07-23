using IntoIT.GilCar.Interfaces.CustomersCarInstallations;
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

namespace CloseCases
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
            int totalRows = 0;
            int Errors = 0;
            int success = 0;
            DataTable cases = dal.GetAllCases();
            totalRows = cases.Rows.Count;
            //new_name new_service_station_id
            QueryExpression query = new QueryExpression("new_service_station");
            query.Criteria.AddCondition("new_name", ConditionOperator.Equal, "סניף תל אביב");
            EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
            Guid tlv = coll.Entities[0].Id;
            query = new QueryExpression("new_service_station");
            query.Criteria.AddCondition("new_name", ConditionOperator.Equal, "סניף חיפה");
            coll = crmBase.XrmService.RetrieveMultiple(query);
            Guid haifa = coll.Entities[0].Id;
            if (cases != null)
            {
                foreach (DataRow row in cases.Rows)
                {
                    try
                    {
                        if (row["ServiceCallNum"] != null)
                        {
                            query = new QueryExpression("incident");
                            query.ColumnSet.AddColumns("title");
                            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                            query.Criteria.AddCondition("ticketnumber", ConditionOperator.Equal, row["ServiceCallNum"] );
                            FilterExpression Filter3 = new FilterExpression(LogicalOperator.Or);
                            Filter3.AddCondition("new_service_station_id", ConditionOperator.Equal, tlv);
                            Filter3.AddCondition("new_service_station_id", ConditionOperator.Equal, haifa);
                            query.Criteria.Filters.Add(Filter3);
                          
                            coll = crmBase.XrmService.RetrieveMultiple(query);
                            if (coll != null && coll.Entities.Count > 0)
                            {
                                foreach (Entity incident in coll.Entities)
                                {
                                    incident["description"] = row["description"].ToString();
                                    incident["new_solution_code"] = new OptionSetValue(int.Parse(row["new_solution_code"].ToString()));
                                    incident["new_change_serviceappointment_status_reason"] = new OptionSetValue(100000003);
                                    crmBase.XrmService.Update(incident);
                                    dal.UpdateCase((int)row["docnum"]);

                                }
                                success++;

                            }
                            else
                            {
                                string newIncIdError = row["ServiceCallNum"].ToString();
                                int docnum = (int)row["docnum"];
                                dal.UpdateError(string.Format("{0}. ", "FSdocNum: " + docnum + " FSIncident: " + newIncIdError + " incident didnt fount "), docnum);
                                LogManager.Logging.WriteToLog(string.Format("{0}. ", "docNum: " + docnum + " incident : " + newIncIdError + " incident didnt fount "));
                                Errors++;
                            }
                        }
                        else if (row["newcarid"] != null && row["scheduledstart"] != null)
                        {
                            string newcarId = row["newcarid"].ToString();
                            query = new QueryExpression("new_car");
                            query.ColumnSet.AddColumns("new_s_license_num");
                            query.Criteria.AddCondition("new_s_license_num", ConditionOperator.Equal, newcarId);
                            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                            coll = crmBase.XrmService.RetrieveMultiple(query);
                            if (coll != null && coll.Entities.Count > 0)
                            {
                                Guid carId = coll.Entities[0].Id;
                                DateTime scheduledstart = (DateTime)row["scheduledstart"];
                                DateTime scheduledstartEnd = scheduledstart.AddDays(1);
                                
                                query = new QueryExpression("incident");
                                query.ColumnSet.AddColumns("title");
                                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                                query.Criteria.AddCondition("new_car_id", ConditionOperator.Equal, carId);
                                FilterExpression Filter3 = new FilterExpression(LogicalOperator.Or);
                                Filter3.AddCondition("new_service_station_id", ConditionOperator.Equal, tlv);
                                Filter3.AddCondition("new_service_station_id", ConditionOperator.Equal, haifa);
                                query.Criteria.Filters.Add(Filter3);
                                LinkEntity lk3 = new LinkEntity("incident", "bookableresourcebooking", "incidentid", "new_incident_id", JoinOperator.Inner);
                                lk3.LinkCriteria.AddCondition("starttime", ConditionOperator.GreaterEqual, scheduledstart);
                                lk3.LinkCriteria.AddCondition("endtime", ConditionOperator.LessThan, scheduledstartEnd);

                                query.LinkEntities.Add(lk3);
                                coll = crmBase.XrmService.RetrieveMultiple(query);
                                if (coll != null && coll.Entities.Count > 0)
                                {
                                    foreach (Entity incident in coll.Entities)
                                    {
                                        incident["description"] = row["description"].ToString();
                                        incident["new_solution_code"] = new OptionSetValue(int.Parse(row["new_solution_code"].ToString()));
                                        incident["new_change_serviceappointment_status_reason"] = new OptionSetValue(100000003);
                                        crmBase.XrmService.Update(incident);
                                        dal.UpdateCase((int)row["docnum"]);

                                    }
                                    success++;

                                }
                                else
                                {
                                    string newcarIdError = row["newcarid"].ToString();
                                    int docnum = (int)row["docnum"];
                                    dal.UpdateError(string.Format("{0}. ", "FSdocNum: " + docnum + " FScar: " + newcarIdError + " incident didnt fount "), docnum);
                                    LogManager.Logging.WriteToLog(string.Format("{0}. ", "docNum: " + docnum + " car: " + newcarIdError + " incident didnt fount "));
                                    Errors++;
                                }


                            }
                            else
                            {
                                Errors++;
                           
                                int docnum = (int)row["docnum"];
                                dal.UpdateError(string.Format("{0}.", " car " + newcarId + " didnt fount "), docnum);
                                LogManager.Logging.WriteToLog(string.Format("{0}.", " car " + newcarId + " didnt fount "));
                            }
                        }
                        else
                        {
                            int docnum = (int)row["docnum"];                        
                            dal.UpdateError("newcarid or scheduledstart or ServiceCallNum where null ", docnum);
                            Errors++;
                        }
                    }
                    catch (Exception ex)
                    {
                        string newcarIdError = row["newcarid"].ToString();
                        int docnum = (int)row["docnum"];
                       // dal.UpdateError("car " + newcarIdError + " incident didnt fount", docnum);
                        dal.UpdateError(ex.Message, docnum);
                        LogManager.Logging.WriteToLog(string.Format("{0}.", "docNum: " + docnum + " car: " + newcarIdError + ex.Message +" " ));
                        Errors++;
                    }


                }
            }
            CreateNewInterfaceLog(start, "CloseCases [dbo].[CloseCases]", totalRows, success, Errors);
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

        #region email
        public void SendEmail()
        {
            Guid wod_EmailId = Guid.Empty;


            EntityCollection wod_EmailToReciepents = new EntityCollection();
            // Creating Email 'from' recipient activity party entity object
            Entity wod_EmailFromReciepent = new Entity("activityparty");
            string emailaddres = System.Configuration.ConfigurationManager.AppSettings["ToRecipientEmailAddress"];
            string[] emailaddress = emailaddres.Split(';');
            foreach (var email in emailaddress)
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    Entity wod_EmailToReciepent = new Entity("activityparty");
                    wod_EmailToReciepent["addressused"] = email;
                    wod_EmailToReciepents.Entities.Add(wod_EmailToReciepent);
                }
            }
            // Setting from user account
            wod_EmailFromReciepent["partyid"] = new EntityReference("systemuser", Guid.Parse(System.Configuration.ConfigurationManager.AppSettings["SenderUserId"]));

            // Creating Email entity object
            Entity wod_EmailEntity = new Entity("email");

            // Setting email entity 'to' attribute value
            wod_EmailEntity["to"] = wod_EmailToReciepents;

            // Setting email entity 'from' attribute value
            wod_EmailEntity["from"] = new Entity[] { wod_EmailFromReciepent };

            wod_EmailEntity["subject"] = string.Format("{0}", System.Configuration.ConfigurationManager.AppSettings["subject"]);
            wod_EmailEntity["description"] = "<div style='direction:rtl'>" + "שלום" + "<br/>" + "מצורף קובץ שגוים של ממשק סגירת פניות." + "<br/>" + "להלן הפניות של תל אביב שנותרו פתוחות : " + "<br/>" + string.Format("{0} ", System.Configuration.ConfigurationManager.AppSettings["Email_Url"]) + "<div/>";
            wod_EmailId = crmBase.XrmService.Create(wod_EmailEntity);
            Entity attachment = new Entity("activitymimeattachment");

            attachment["subject"] = "My Subject";

            string fileName = Logging.fileName;

            attachment["filename"] = fileName;
            //
            string path = ConfigurationManager.AppSettings["LogPath"];
            byte[] fileStream = File.ReadAllBytes(fileName);

            attachment["body"] = Convert.ToBase64String(fileStream);

            attachment["mimetype"] = "text/plain";

            attachment["attachmentnumber"] = 1;


            attachment["objectid"] = new EntityReference("email", wod_EmailId);

            attachment["objecttypecode"] = "email";

            crmBase.XrmService.Create(attachment);

            // Creating SendEmailRequest object for sending email
            SendEmailRequest wod_SendEmailRequest = new SendEmailRequest();


            // Setting email record if for sending email
            wod_SendEmailRequest.EmailId = wod_EmailId;

            wod_SendEmailRequest.IssueSend = true;
            // wod_SendEmailRequest.TrackingToken = "";
            // Sending email
            crmBase.XrmService.Execute(wod_SendEmailRequest);
        }



        #endregion

        private DateTime RetrieveLocalTimeFromUTCTime(DateTime utcDateTime, IOrganizationService service)
        {
            var request = new LocalTimeFromUtcTimeRequest
            {
                TimeZoneCode = 135,
                UtcTime = utcDateTime.ToUniversalTime()
            };

            var response = (LocalTimeFromUtcTimeResponse)service.Execute(request);
            return response.LocalTime;
        }
    }
}
