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


namespace NewInventoryTransferRequest
{
    public class BL
    {
        DAL dal;
        CrmBase crmBase;
        int allLinesCounter = 0;
        int errorLineCounter = 0;
        int succeededLineCounter = 0;
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
            List<LinesAndRequest> linesAndRequestsList= GetNewInventoryTransferRequest();
            if (linesAndRequestsList != null && linesAndRequestsList.Count > 0)
            {
                foreach (LinesAndRequest linesAndRequest in linesAndRequestsList)
                {
                    try
                    {
                        allLinesCounter += linesAndRequest.NewInventoryTransferlinesList.Count;
                        dal.InsertInventoryTransferRequest(linesAndRequest.NewInventoryTransferRequestEntity);
                        int lineNum = 0;                      
                        foreach (NewInventoryTransferlines newInventoryTransferlines in linesAndRequest.NewInventoryTransferlinesList)
                        {
                            lineNum++;
                            try
                            {
                                dal.NewInventoryTransferlines(newInventoryTransferlines, lineNum);
                                succeededLineCounter++;
                            }
                            catch (Exception ex)
                            {
                                LogManager.Logging.WriteToLog(ex.Message + Environment.NewLine + ex.StackTrace);
                                errorLineCounter++;
                            }
                         

                        }
                        Entity new_inventory_transfer_request = new Entity("new_inventory_transfer_request");
                        new_inventory_transfer_request.Id = linesAndRequest.NewInventoryTransferRequestEntity.new_inventory_transfer_request_id;
                        new_inventory_transfer_request["statuscode"] = new OptionSetValue(100000001);
                        crmBase.XrmService.Update(new_inventory_transfer_request);

                    }
                    catch (Exception ex)
                    {
                        LogManager.Logging.WriteToLog(ex.Message + Environment.NewLine + ex.StackTrace);
                        errorLineCounter += linesAndRequest.NewInventoryTransferlinesList.Count;
                    }
                    CreateNewInterfaceLog(start, "NewInventoryTransferRequest", allLinesCounter, succeededLineCounter, errorLineCounter);
                }


            }



        }


        public List<LinesAndRequest> GetNewInventoryTransferRequest()
        {
            List<LinesAndRequest> linesAndRequestsList = new List<LinesAndRequest>();
            QueryExpression new_inventory_transfer_requestQuery = new QueryExpression("new_inventory_transfer_request");
            new_inventory_transfer_requestQuery.ColumnSet = new ColumnSet("new_request_num", "ownerid", "new_warehouse_num", "createdon");
            new_inventory_transfer_requestQuery.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 100000000);
            LinkEntity lk1 = new LinkEntity("new_inventory_transfer_request", "systemuser", "ownerid", "systemuserid", JoinOperator.Inner);
            lk1.Columns.AddColumns("new_card_code");
            lk1.EntityAlias = "systemuser";
            new_inventory_transfer_requestQuery.LinkEntities.Add(lk1);
            EntityCollection coll = crmBase.XrmService.RetrieveMultiple(new_inventory_transfer_requestQuery);
            if (coll != null && coll.Entities.Count > 0)
            {
                foreach (Entity ent in coll.Entities)
                {
                    List<NewInventoryTransferlines> newInventoryTransferlinesList=new List<NewInventoryTransferlines>();
                    NewInventoryTransferRequest newInventoryTransferRequest = SetNewInventoryTransferRequest(ent);
                    if (ent.Contains("new_request_num") && ent["new_request_num"] != null)
                    {
                        newInventoryTransferlinesList = GetNewInventoryTransferlines(ent.Id, ent["new_request_num"].ToString());
                    }
                    LinesAndRequest linesAndRequest = new LinesAndRequest();
                    linesAndRequest.NewInventoryTransferRequestEntity = newInventoryTransferRequest;
                    linesAndRequest.NewInventoryTransferlinesList = newInventoryTransferlinesList;
                    linesAndRequestsList.Add(linesAndRequest);
                }
               
            }
            return linesAndRequestsList;
        }

        public List<NewInventoryTransferlines> GetNewInventoryTransferlines(Guid newInventoryTransferID, string new_request_num)
        {
            List<NewInventoryTransferlines> newInventoryTransferlinesList = new List<NewInventoryTransferlines>();
            QueryExpression new_inventory_transfer_linesQuery = new QueryExpression("new_inventory_transfer_lines");
            new_inventory_transfer_linesQuery.ColumnSet = new ColumnSet("new_request_num", "new_item_id", "new_qty", "new_serial_num");
            new_inventory_transfer_linesQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            new_inventory_transfer_linesQuery.Criteria.AddCondition("new_request_num", ConditionOperator.Equal, newInventoryTransferID);
            LinkEntity lk1 = new LinkEntity("new_inventory_transfer_lines", "product", "new_item_id", "productid", JoinOperator.Inner);
            lk1.Columns.AddColumns("new_s_product_code");
            lk1.EntityAlias = "product";
            new_inventory_transfer_linesQuery.LinkEntities.Add(lk1);
            EntityCollection coll = crmBase.XrmService.RetrieveMultiple(new_inventory_transfer_linesQuery);
            if (coll != null && coll.Entities.Count > 0)
            {
                foreach (Entity new_inventory_transfer_lines in coll.Entities)
                {
                    NewInventoryTransferlines newInventoryTransferlines = SetNewInventoryTransferlines(new_inventory_transfer_lines, new_request_num);
                    newInventoryTransferlinesList.Add(newInventoryTransferlines);
                }
              
            }
            return newInventoryTransferlinesList;

        }

        public NewInventoryTransferlines SetNewInventoryTransferlines(Entity new_inventory_transfer_lines,string new_request_num)
        {
            NewInventoryTransferlines newInventoryTransferlines = new NewInventoryTransferlines();
            newInventoryTransferlines.new_request_num = new_request_num;
            if (new_inventory_transfer_lines.Contains("product.new_s_product_code") && new_inventory_transfer_lines["product.new_s_product_code"] != null)
            {
                newInventoryTransferlines.new_item_id = ((AliasedValue)new_inventory_transfer_lines["product.new_s_product_code"]).Value.ToString();
            }
          
            if (new_inventory_transfer_lines.Contains("new_qty") && new_inventory_transfer_lines["new_qty"] != null)
                newInventoryTransferlines.new_qty = new_inventory_transfer_lines["new_qty"].ToString();
            if (new_inventory_transfer_lines.Contains("new_serial_num") && new_inventory_transfer_lines["new_serial_num"] != null)
                newInventoryTransferlines.new_serial_num = new_inventory_transfer_lines["new_serial_num"].ToString();

            return newInventoryTransferlines;

        }

        public NewInventoryTransferRequest SetNewInventoryTransferRequest(Entity new_inventory_transfer_request)
        {
            NewInventoryTransferRequest NewInventoryTransferRequest = new NewInventoryTransferRequest();
            NewInventoryTransferRequest.new_inventory_transfer_request_id = new_inventory_transfer_request.Id;
            if (new_inventory_transfer_request.Contains("new_request_num") && new_inventory_transfer_request["new_request_num"]!=null)
            NewInventoryTransferRequest.new_request_num = new_inventory_transfer_request["new_request_num"].ToString();

            if (new_inventory_transfer_request.Contains("systemuser.new_card_code") && new_inventory_transfer_request["systemuser.new_card_code"] != null)
            {
                NewInventoryTransferRequest.ownerid = ((AliasedValue)new_inventory_transfer_request["systemuser.new_card_code"]).Value.ToString();
            }

            if (new_inventory_transfer_request.Contains("new_warehouse_num") && new_inventory_transfer_request["new_warehouse_num"] != null)
                NewInventoryTransferRequest.new_warehouse_num = new_inventory_transfer_request["new_warehouse_num"].ToString();

            if (new_inventory_transfer_request.Contains("createdon") && new_inventory_transfer_request["createdon"] != null)
                NewInventoryTransferRequest.createdon = (DateTime)new_inventory_transfer_request["createdon"];

            return NewInventoryTransferRequest;
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
