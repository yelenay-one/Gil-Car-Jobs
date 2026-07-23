using IntoIT.GilCar.IncidentItemsToSAP.Entities;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Configuration;
using System.IO;

namespace IntoIT.GilCar.IncidentItemsToSAP
{
    public class BL
    {
        DAL dal;
        CrmBase crmBase;
        int inventoryUpdateType = 100000001;
        int mainCount = 0;
        int exported = 0;

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

        #region StartInterface
        public void StartInterface(DateTime start)
        {
            int totalRows = 0;
            int Errors = 0;
            int success = 0;
            try
            {
                EntityCollection coll = GetIncidentItmesCRM();
                if (coll != null)
                    mainCount = coll.Entities.Count;
            
                    totalRows = mainCount; 
                foreach (var incidentItem in coll.Entities)
                {
                    inventoryUpdateType = 100000001;
                    IncidentItem currIncidentItem = new IncidentItem();
                    try
                    {
                        currIncidentItem = FillIncidentItemObject(incidentItem);



                    }
                    catch (Exception ex)
                    {
                        Errors++;
                        LogManager.Logging.WriteToLog("IncidentItemsToSAP(" + incidentItem.Id + ") Error: " + ex.Message + " " + ex.StackTrace);
                        //throw ex;
                    }

                    //if (StatusCodeValid(currIncidentItem.StatusCode))
                    //{
                    if (dal.IsIncidentItemExistsSAP(currIncidentItem.IncidentItemGUID) == 0)
                    {

                        string serviceStationName = "טכנאי שטח";
                        if (currIncidentItem.ServiceStation == serviceStationName || currIncidentItem.ItemServiceStation == serviceStationName)
                        {
                            exported++;
                            inventoryUpdateType = 100000000;
                            dal.InsertIncidentItemSAP(currIncidentItem);
                            UpdateCrmDate(currIncidentItem.IncidentItemGUID, inventoryUpdateType,"");
                            success++;
                        }
                        else
                        {

                            if (currIncidentItem.new_inventory_transfer_via_interface==true)
                            {
                                //if ( currIncidentItem.DocDate != null && currIncidentItem.Model != null && IsNotInWarranty(currIncidentItem.DocDate, currIncidentItem.Model)                                                   
                                //                       || currIncidentItem.CustomerDamage && CustomerTypeValid(currIncidentItem.CustomerType)
                                //                       || InstallationLocationValid(currIncidentItem.InstallationLocation) && currIncidentItem.DocDate != null && currIncidentItem.Model != null && !IsNotInWarranty(currIncidentItem.DocDate, currIncidentItem.Model))
                                //{
                                    exported++;
                                    inventoryUpdateType = 100000000;
                                    dal.InsertIncidentItemSAP(currIncidentItem);
                                    UpdateCrmDate(currIncidentItem.IncidentItemGUID, inventoryUpdateType, "");
                                    success++;

                                //}
                                //else
                                //{
                                //    UpdateCrmDate(currIncidentItem.IncidentItemGUID, inventoryUpdateType, "IncidentItem did not pass the condition  ", true);
                                //    Errors++;
                                //}
                            }
                            else
                            {
                                UpdateCrmDate(currIncidentItem.IncidentItemGUID, inventoryUpdateType, "IncidentItem did not pass the condition of CaseTypeCode  ", true);
                                Errors++;
                            }


                            // string serviceStationNameTLV = "סניף תל אביב";

                            //if (currIncidentItem.ServiceStation == serviceStationNameTLV || currIncidentItem.ItemServiceStation == serviceStationNameTLV)
                            //{
                            //    UpdateCrmDate(currIncidentItem.IncidentItemGUID, inventoryUpdateType,"");
                            //    success++;
                            //}
                            //else
                            //{
                            //    if (currIncidentItem.DocDate != null && currIncidentItem.Model != null && IsNotInWarranty(currIncidentItem.DocDate, currIncidentItem.Model)
                            //                                || CaseTypeCodeValid(currIncidentItem.CaseTypeCode)
                            //                                || currIncidentItem.CustomerDamage && CustomerTypeValid(currIncidentItem.CustomerType)
                            //                                || InstallationLocationValid(currIncidentItem.InstallationLocation) && currIncidentItem.DocDate != null && currIncidentItem.Model != null && !IsNotInWarranty(currIncidentItem.DocDate, currIncidentItem.Model))
                            //    {
                            //        exported++;
                            //        inventoryUpdateType = 100000000;
                            //        dal.InsertIncidentItemSAP(currIncidentItem);
                            //        UpdateCrmDate(currIncidentItem.IncidentItemGUID, inventoryUpdateType,"");
                            //        success++;

                            //    }
                            //    else
                            //    {
                            //        UpdateCrmDate(currIncidentItem.IncidentItemGUID, inventoryUpdateType, "IncidentItem did not pass the condition when is not TLV or technician ", true);
                            //        Errors++;
                            //    }
                            //}

                        }

                    }
                    else
                    {
                        UpdateCrmDate(currIncidentItem.IncidentItemGUID, inventoryUpdateType, "IncidentItem Exists in SAP", true);
                        Errors++;
                    }
                        
                }
            }
            catch (Exception ex)
            {
                Errors++;
                LogManager.Logging.WriteToLog("IncidentItemsToSAP Error: " + ex.Message + " " + ex.StackTrace);
            }
            LogManager.Logging.WriteToLog("Main collection: " + mainCount);
            LogManager.Logging.WriteToLog("Exported: " + exported);
            CreateNewInterfaceLog(start, "IncidentItemsToSAP  new_incident_items", totalRows, success, Errors);
        }
        #endregion
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
        #region Private Methods
        private bool StatusCodeValid(int statusCode)
        {
            if (statusCode == 5)
                return true;
            return false;
        }

        private bool InstallationLocationValid(int installationLocation)
        {

            if (installationLocation == 100000001)
                return true;
            return false;
        }

        private bool CustomerTypeValid(int customerType)
        {
            if (customerType == 100000000)
                return true;
            return false;
        }

        private bool CaseTypeCodeValid(int caseCodeType)
        {

            if (caseCodeType == 2 || caseCodeType == 100000006)
            {
                return true;
            }
            return false;
        }

        private bool IsNotInWarranty(DateTime docDate, string model)
        {

            DateTime dateNow = DateTime.Now;
            int daysDif = dateNow.Subtract(docDate).Days;

            QueryExpression query = new QueryExpression("new_car_model");
            query.ColumnSet.AddColumns("new_i_warranty");
            Guid modelId = Guid.Parse(model);
            query.Criteria.AddCondition("new_car_modelid", ConditionOperator.Equal, modelId);
            try
            {
                EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                if (coll.Entities.Count > 0)
                {
                    int warranty = (int)coll[0]["new_i_warranty"];
                    int endOfwarranty = warranty * 365;
                    if (daysDif > endOfwarranty)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(ex.Message + " " + ex.StackTrace);
                return false;
            }

        }

        private EntityCollection GetIncidentItmesCRM()
        {
            QueryExpression query = new QueryExpression("new_incident_items");
            query.ColumnSet.AddColumns(new string[] { "new_cost" });
            LinkEntity lk1 = new LinkEntity("new_incident_items", "incident", "new_incident_id", "incidentid", JoinOperator.Inner);
            //lk1.Columns.AddColumns("ticketnumber"); 
            lk1.Columns.AddColumns("new_car_id", "ticketnumber", "customerid", "new_car_model_id", "new_s_customer_order_number", "casetypecode", "new_b_customer_damage", "new_p_customer_type", "statuscode", "new_service_station_id", "new_for_charge", "new_p_installations_classification", "new_solution_code", "description", "new_total_paid", "new_payment_to_sap");
            lk1.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, 1);
           
            
            lk1.EntityAlias = "incident";

            LinkEntity lk12 = new LinkEntity("incident", "new_car", "new_car_id", "new_carid", JoinOperator.Inner);
            lk12.Columns.AddColumns("new_s_chassis_num", "new_d_doc_date", "new_p_installation_location");
            lk12.EntityAlias = "car";
            lk1.LinkEntities.Add(lk12);

            query.LinkEntities.Add(lk1);

            LinkEntity lk2 = new LinkEntity("new_incident_items", "systemuser", "owninguser", "systemuserid", JoinOperator.Inner);
            lk2.Columns.AddColumns("systemuserid");
            lk2.EntityAlias = "systemuser";
            query.LinkEntities.Add(lk2);

            LinkEntity lk3 = new LinkEntity("new_incident_items", "product", "new_incident_item_id", "productid", JoinOperator.LeftOuter);
            lk3.Columns.AddColumns("new_s_product_code", "name");
            lk3.EntityAlias = "product";
            query.LinkEntities.Add(lk3);

            LinkEntity lk4 = new LinkEntity("new_incident_items", "product", "new_product_alien_incident_items_id", "productid", JoinOperator.LeftOuter);
            lk4.Columns.AddColumns("new_s_product_code", "name");
            lk4.EntityAlias = "product_alter";
            query.LinkEntities.Add(lk4);

            LinkEntity lk5 = new LinkEntity("new_incident_items", "new_service_station", "new_service_station_id", "new_service_stationid", JoinOperator.Inner);
            lk5.Columns.AddColumn("new_inventory_transfer_via_interface");
            query.LinkEntities.Add(lk5);
            query.ColumnSet.AddColumns("new_i_qty", "new_s_item_sn", "createdon", "new_s_card_code", "new_s_whs_code", "new_service_station_id", "new_s_technician_card_code");
            query.Criteria.AddCondition("new_createdon_sap_db", ConditionOperator.Null);
            query.Criteria.AddCondition("new_p_inventory_update_type", ConditionOperator.NotEqual, 100000001);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            try
            {
                EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                return coll;
                
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error in GetIncidentItmesCRM: {0}", ex);
                LogManager.Logging.WriteToLog(msg);
            }
            return null;
        }

        private IncidentItem FillIncidentItemObject(Entity incidentItem)
        {
            try
            {
                IncidentItem currIncidentItem = new IncidentItem();
                currIncidentItem.IncidentItemGUID = incidentItem.Id;
                currIncidentItem.IncidentCode = incidentItem.GetAttributeValue<AliasedValue>("incident.ticketnumber").Value.ToString();
                currIncidentItem.LicenseNum = (incidentItem.GetAttributeValue<AliasedValue>("incident.new_car_id").Value as EntityReference).Name;
                //new_service_station_id
                if (incidentItem.Contains("incident.new_service_station_id"))
                    currIncidentItem.ServiceStation = (incidentItem.GetAttributeValue<AliasedValue>("incident.new_service_station_id").Value as EntityReference).Name;

                if (incidentItem.Contains("new_service_station_id"))
                    currIncidentItem.ItemServiceStation = incidentItem.GetAttributeValue<EntityReference>("new_service_station_id").Name;

                if (incidentItem.Contains("new_s_card_code"))
                    currIncidentItem.CardCode = incidentItem["new_s_card_code"].ToString();

                if (incidentItem.Contains("new_s_whs_code"))
                    currIncidentItem.WhsCode = incidentItem["new_s_whs_code"].ToString();

                if (incidentItem.Contains("new_s_technician_card_code"))
                    currIncidentItem.new_s_technician_card_code = incidentItem["new_s_technician_card_code"].ToString();

                //new_p_installations_classification
                if (incidentItem.FormattedValues.Contains("incident.new_p_installations_classification"))
                {
                    OptionSetValue new_p_installations_classificationAliasedValue = (OptionSetValue)(incidentItem.GetAttributeValue<AliasedValue>("incident.new_p_installations_classification").Value);

                    currIncidentItem.new_p_installations_classification = new_p_installations_classificationAliasedValue.Value.ToString();
                }




                if (incidentItem.Contains("product.new_s_product_code"))
                    currIncidentItem.ProductCode = incidentItem.GetAttributeValue<AliasedValue>("product.new_s_product_code").Value.ToString();
                else
                    currIncidentItem.ProductCode = incidentItem.GetAttributeValue<AliasedValue>("product_alter.new_s_product_code").Value.ToString();

                if (incidentItem.Contains("new_s_item_sn"))
                    currIncidentItem.ItemSN = incidentItem["new_s_item_sn"].ToString();

                if (incidentItem.Contains("incident.customerid"))
                    currIncidentItem.Owner = (incidentItem.GetAttributeValue<AliasedValue>("incident.customerid").Value as EntityReference).Name;

                if (incidentItem.Contains("incident.new_car_model_id"))
                {
                    currIncidentItem.Model = (incidentItem.GetAttributeValue<AliasedValue>("incident.new_car_model_id").Value as EntityReference).Id.ToString();
                    currIncidentItem.ModelName = (incidentItem.GetAttributeValue<AliasedValue>("incident.new_car_model_id").Value as EntityReference).Name;
                }

                if (incidentItem.Contains("car.new_s_chassis_num"))
                    currIncidentItem.Shilda = incidentItem.GetAttributeValue<AliasedValue>("car.new_s_chassis_num").Value.ToString();


                if (incidentItem.Contains("incident.new_s_customer_order_number"))
                    currIncidentItem.OrderNumber = incidentItem.GetAttributeValue<AliasedValue>("incident.new_s_customer_order_number").Value.ToString();
               
                   

                    if (incidentItem.Contains("incident.casetypecode"))
                    currIncidentItem.CaseTypeCode = ((Microsoft.Xrm.Sdk.OptionSetValue)(incidentItem.GetAttributeValue<AliasedValue>("incident.casetypecode").Value)).Value;

                if (incidentItem.Contains("car.new_d_doc_date"))
                {
                    DateTime timeDoc = (DateTime)((AliasedValue)incidentItem["car.new_d_doc_date"]).Value;
                    TimeZoneInfo timeInfoIsl = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
                    currIncidentItem.DocDate = TimeZoneInfo.ConvertTimeFromUtc(timeDoc, timeInfoIsl);
                }

                //incident.new_charge
                if (incidentItem.Contains("incident.new_for_charge"))
                {
                    OptionSetValue new_for_charge = (OptionSetValue)incidentItem.GetAttributeValue<AliasedValue>("incident.new_for_charge").Value;

                    if (new_for_charge.Value == 100000000)
                        currIncidentItem.NewCharge = "כן";
                    else if (new_for_charge.Value == 100000001)
                        currIncidentItem.NewCharge = "לא";
                    else currIncidentItem.NewCharge = "";
                }


                if (incidentItem.Contains("new_service_station1.new_inventory_transfer_via_interface"))
                    currIncidentItem.new_inventory_transfer_via_interface= (bool)incidentItem.GetAttributeValue<AliasedValue>("new_service_station1.new_inventory_transfer_via_interface").Value;
                if (incidentItem.Contains("incident.new_b_customer_damage"))
                    currIncidentItem.CustomerDamage = (bool)incidentItem.GetAttributeValue<AliasedValue>("incident.new_b_customer_damage").Value;

                if (incidentItem.Contains("incident.new_p_customer_type"))
                    currIncidentItem.CustomerType = ((Microsoft.Xrm.Sdk.OptionSetValue)(incidentItem.GetAttributeValue<AliasedValue>("incident.new_p_customer_type").Value)).Value;

                if (incidentItem.Contains("car.new_p_installation_location"))
                    currIncidentItem.InstallationLocation = ((Microsoft.Xrm.Sdk.OptionSetValue)(incidentItem.GetAttributeValue<AliasedValue>("car.new_p_installation_location").Value)).Value;

                if (incidentItem.Contains("incident.statuscode"))
                    currIncidentItem.StatusCode = ((Microsoft.Xrm.Sdk.OptionSetValue)(incidentItem.GetAttributeValue<AliasedValue>("incident.statuscode").Value)).Value;
                //statuscode
                //YY 26/02/24
                if (incidentItem.Contains("incident.new_solution_code"))
                    currIncidentItem.new_solution_code = ((Microsoft.Xrm.Sdk.OptionSetValue)(incidentItem.GetAttributeValue<AliasedValue>("incident.new_solution_code").Value)).Value;

                if (incidentItem.Contains("incident.description"))
                    currIncidentItem.description = incidentItem["incident.description"].ToString(); //incidentItem.GetAttributeValue<AliasedValue>("incident.description").Value.ToString();
                                                                                                    //End YY 26/02/24


                if (incidentItem.Contains("new_cost"))
                    if (incidentItem.Contains("new_payment_to_sap") && (bool)incidentItem["new_payment_to_sap"] == true)
                        currIncidentItem.price = (decimal)incidentItem["new_cost"]; //incidentItem.GetAttributeValue<AliasedValue>("new_cost").Value;
                    else
                        currIncidentItem.price = 0;

                    if (incidentItem.Contains("incident.new_total_paid"))
                    if (incidentItem.Contains("new_payment_to_sap") && (bool)incidentItem["new_payment_to_sap"] == true)
                        currIncidentItem.totalprice = (decimal)((Microsoft.Xrm.Sdk.AliasedValue)incidentItem["incident.new_total_paid"]).Value;
                    else
                        currIncidentItem.totalprice = 0;

                currIncidentItem.Quantity = Convert.ToInt32(incidentItem["new_i_qty"].ToString());

                //int? aa = RetrieveCurrentUsersTimeZoneSettings();

                //currIncidentItem.Createdon = ((DateTime)incidentItem["createdon"]);

                //if (incidentItem.Contains("new_createdon_sap_db"))
                //    currIncidentItem.SapTime = Convert.ToDateTime((DateTime)incidentItem["new_createdon_sap_db"]).ToUniversalTime(); 
                DateTime time = Convert.ToDateTime((DateTime)incidentItem["createdon"]).ToUniversalTime();
                TimeZoneInfo timeInfo = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
                DateTime isrTime = TimeZoneInfo.ConvertTimeFromUtc(time, timeInfo);

                currIncidentItem.Createdon = isrTime;

                return currIncidentItem;
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error in FillIncidentItemObject: {0}", ex);
                LogManager.Logging.WriteToLog(msg);
            }
            return null;
        }

        private void UpdateCrmDate(Guid incidentItemGuid, int inventoryUpdateType,string new_s_sap_error, bool Override = false)
        {
            Entity incidentItem_entity = new Entity("new_incident_items");
            incidentItem_entity.Id = incidentItemGuid;
            incidentItem_entity["new_p_inventory_update_type"] = new OptionSetValue(inventoryUpdateType);
            
            if (inventoryUpdateType != 100000001)
                incidentItem_entity["new_createdon_sap_db"] = DateTime.Now.ToUniversalTime();
            
            if (Override)
            {
                incidentItem_entity["new_createdon_sap_db"] = DateTime.Now.ToUniversalTime();
                incidentItem_entity["new_p_inventory_update_type"] = null;
                incidentItem_entity["new_s_sap_error"] = new_s_sap_error;
            }
            
            try
            {
                crmBase.XrmService.Update(incidentItem_entity);
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error in UpdateCrmDate: {0}", ex);
                LogManager.Logging.WriteToLog(msg);
            }
        }

        //public int? RetrieveCurrentUsersTimeZoneSettings()
        //{
        //    var currentUserSettings = crmBase.XrmService.RetrieveMultiple(
        //    new QueryExpression("usersettings")
        //    {
        //        //ColumnSet = new ColumnSet("localeid", "timezonecode"),
        //        ColumnSet=new ColumnSet(true),
        //        Criteria = new FilterExpression
        //        {
        //            Conditions =
        //{
        //    new ConditionExpression("systemuserid", ConditionOperator.EqualUserId)
        //}
        //        }
        //    }).Entities[0].ToEntity<Entity>();
        //    return (int?)currentUserSettings.Attributes["timezonecode"];
        //}

        #region email
        public void SendEmail()
        {
            // Email record id
            Guid wod_EmailId = Guid.Empty;

            // Creating Email 'to' recipient activity party entity object
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

            if (LogManager.Logging.fileName != null)
            {
                wod_EmailEntity["subject"] = "Incident Items To SAP interface error log";
                wod_EmailEntity["description"] = string.Format("Attached herewith error log for {0}", DateTime.Now);
                wod_EmailId = crmBase.XrmService.Create(wod_EmailEntity);
                AttachFileToEmail(wod_EmailId);
            }

            else
            {
                wod_EmailEntity["subject"] = string.Format("Incident Items To SAP interface error log - No errors for {0}", DateTime.Now);
                wod_EmailEntity["description"] = string.Empty;
                wod_EmailId = crmBase.XrmService.Create(wod_EmailEntity);
            }


            // Creating SendEmailRequest object for sending email
            SendEmailRequest wod_SendEmailRequest = new SendEmailRequest();

            // Creating Email tracking token request object
            GetTrackingTokenEmailRequest wod_GetTrackingTokenEmailRequest = new GetTrackingTokenEmailRequest();

            // Creating Email tracking token response object to get tracking token value
            GetTrackingTokenEmailResponse wod_GetTrackingTokenEmailResponse = null;

            // Setting email record if for sending email
            wod_SendEmailRequest.EmailId = wod_EmailId;

            wod_SendEmailRequest.IssueSend = true;

            // Getting tracking token value
            wod_GetTrackingTokenEmailResponse = (GetTrackingTokenEmailResponse)
                                                 crmBase.XrmService.Execute(wod_GetTrackingTokenEmailRequest);

            // Setting tracking token value
            wod_SendEmailRequest.TrackingToken = wod_GetTrackingTokenEmailResponse.TrackingToken;

            // Sending email
            crmBase.XrmService.Execute(wod_SendEmailRequest);
        }

        private void AttachFileToEmail(Guid wod_EmailId)
        {
            // Open a file and read its contents into a byte array.
            var fileLocation = string.Format(LogManager.Logging.fileName);
            var stream = File.OpenRead(fileLocation);
            var byteData = new byte[stream.Length];

            stream.Read(byteData, 0, byteData.Length);

            // Encode the data using base64.
            var encodedData = Convert.ToBase64String(byteData);

            string mimeType = "text/plain";
            var fileName = LogManager.Logging.fileName.ToString().Split('\\');
            string shortFileName = fileName[fileName.Length-1];

            Entity attach = new Entity("activitymimeattachment");
            attach["objectid"] = new EntityReference("email", wod_EmailId);
            attach["objecttypecode"] = "email";
            attach["filename"] = shortFileName;
            attach["mimetype"] = mimeType;
            attach["body"] = encodedData;

            crmBase.XrmService.Create(attach);
        }

        #endregion


        #endregion Private Methods 
    }
}
