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

namespace IntoIT.GilCar.Interfaces.ProductsToCRM
{
    public class BL
    {
        int OITM_Counter = 0;
        int OITT_Counter = 0;
        HashSet<string> logLines = new HashSet<string>();
        DAL dal;
        CrmBase crmBase;
        List<Dictionary<string, List<Tuple<Guid, Guid>>>> productsCRM;
        Dictionary<string, List<Tuple<Guid, Guid>>> productsCRM_sons;
        Dictionary<string, List<Tuple<Guid, Guid>>> productsCRM_sons_parents;
        Dictionary<string, EntityReference> productFamilies = new Dictionary<string, EntityReference>();
        List<string> leadingProducts;
        int productsCount = 0;
        int crmCount = 0;

        int productnumberIdx;

        public BL()
        {

            try
            {
                dal = new DAL();
                crmBase = new CrmBase(ConfigurationManager.AppSettings["CRM_Url"]);
                string latestProductnumberIndex = GetLatestProductnumberIndex();
                productnumberIdx = int.Parse(latestProductnumberIndex) + 1;
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

            // step 1 - get all products from OITM + create/update in CRM
            List<Product> productsSAP = dal.GetAllProducts();
            if (productsSAP != null)
                productsCount = productsSAP.Count;
            productsCRM = GetAllProductsWithoutParentFromCRM();
            productsCRM_sons = productsCRM[0];
            productsCRM_sons_parents = productsCRM[1];

            try
            {
                foreach (Product currProductSAP in productsSAP)
                {
                    if (!productsCRM_sons.ContainsKey(currProductSAP.ProductCode))
                    {

                    //    List<Tuple<Guid, Guid>> currProductList = productsCRM_sons[currProductSAP.ProductCode];
                    //    foreach (Tuple<Guid, Guid> currProduct in currProductList)
                    //    {
                    //        totalRows++;
                    //        if (UpdateProduct(currProduct.Item1, currProductSAP))
                    //        {
                    //            dal.UpdateExportDateProducts(currProductSAP.ProductCode);
                    //            OITM_Counter++;
                    //            crmCount++;
                    //            success++;
                    //            Console.SetCursorPosition(0, 0);
                    //            Console.WriteLine("OITM (First run): " + OITM_Counter);
                    //        }
                    //        else
                    //            Errors++;

                    //    }
                    //}
                    //else
                    //{
                        Guid currNewProduct = CreateProduct(currProductSAP);
                        if (currNewProduct != Guid.Empty)
                        {
                            totalRows++;
                            dal.UpdateExportDateProducts(currProductSAP.ProductCode);
                            OITM_Counter++;
                            success++;
                            Console.SetCursorPosition(0, 0);
                            Console.WriteLine("OITM (First run): " + OITM_Counter);
                        }
                        else
                            Errors++;
                    }
                }
            }
            catch (Exception ex)
            {
                Errors++;
                LogManager.Logging.WriteToLog("ProductsToCRM Error: " + ex.Message + " " + ex.StackTrace);
            }
            CreateNewInterfaceLog(start, "ProductsToCRM get all products from OITM + create/update in CRM   OITM ", totalRows, success, Errors);


            LogManager.Logging.WriteToLog("products total from DB : " + productsCount);
            LogManager.Logging.WriteToLog("products total to CRM : " + crmCount);

            //step 2 - getl all records from OITT + create sons "copys" when same son has few parents + update the parent to the newly created sons
            productsCRM = GetAllProductsFromCRM();
            productsCRM_sons = productsCRM[0];
            productsCRM_sons_parents = productsCRM[1];
            productsCount = 0;
            int updateproducts = 0;
            totalRows = 0;
            Errors = 0;
            success = 0;
            start = DateTime.Now;
            List<ProductParent> productParentsSAP = dal.GetAllParentProducts();
            if (productParentsSAP != null)
                productsCount = productParentsSAP.Count;
           
            foreach (ProductParent currProductParentSAP in productParentsSAP)
            {
                try
                {
                    if (currProductParentSAP.ParentProductCode == "CIT-B")
                    {
                        int t = 5;

                    }
                    string currProductParentSAP_str = currProductParentSAP.ParentProductCode + "^" + currProductParentSAP.SonProductCode;

                    if (!productsCRM_sons_parents.ContainsKey(currProductParentSAP_str))
                    {
                        if (productsCRM_sons.ContainsKey(currProductParentSAP.SonProductCode) && productsCRM_sons.ContainsKey(currProductParentSAP.ParentProductCode))
                        {
                            bool found = false;
                            for (int i = 0; i < productsCRM_sons[currProductParentSAP.SonProductCode].Count; i++)
                            {
                                totalRows++;
                                Tuple<Guid, Guid> sonProduct = productsCRM_sons[currProductParentSAP.SonProductCode][i];
                                if (sonProduct.Item2 == Guid.Empty)
                                {
                                    bool isSuccess = false;
                                    found = true;
                                    Guid ParentId = Guid.Empty;
                                    bool resUpdateProductParent = UpdateProductParent(sonProduct.Item1, currProductParentSAP.ParentProductCode, out ParentId);
                                    productsCRM_sons[currProductParentSAP.SonProductCode][i] = new Tuple<Guid, Guid>(sonProduct.Item1, ParentId);
                                    //sonProduct.Item2 = ParentId;
                                    if (resUpdateProductParent)
                                    {
                                        dal.UpdateExportDateParentProducts(currProductParentSAP.SonProductCode, currProductParentSAP.ParentProductCode);
                                        isSuccess=true; 
                                    }
                                   
                                    updateproducts++;
                                   
                                    if (currProductParentSAP.IsSonLeadingItem)
                                    {
                                        Guid currSonProductToCreate = productsCRM_sons[currProductParentSAP.SonProductCode][0].Item1; // gets a first guid we found with the son we need to "copy"
                                        Guid newCreatedProductGuid = CreateProduct(GetProductByGuidCRM(currSonProductToCreate));
                                        updateproducts++;
                                        isSuccess = true; 
                                        // create a "copy" of son (with no parent yet)
                                    }
                                    if (isSuccess == true)
                                        success++;
                                }
                                else
                                    Errors++;

                            }
                            //foreach (Tuple<Guid, Guid> sonProduct in productsCRM_sons[currProductParentSAP.SonProductCode])
                            //{
                            //    if (sonProduct.Item2 == Guid.Empty)
                            //    {
                            //        found = true;
                            //        Guid ParentId = Guid.Empty;
                            //        UpdateProductParent(sonProduct.Item1, currProductParentSAP.ParentProductCode, out ParentId);
                            //        productsCRM_sons[currProductParentSAP.SonProductCode]
                            //        sonProduct.Item2 = ParentId;
                            //        dal.UpdateExportDateParentProducts(currProductParentSAP.SonProductCode, currProductParentSAP.ParentProductCode);
                            //    }
                            //}
                            if (!found)
                            {
                                totalRows++;
                                Guid currSonProductToCreate = productsCRM_sons[currProductParentSAP.SonProductCode][0].Item1; // gets a first guid we found with the son we need to "copy"
                                Guid newCreatedProductGuid = CreateProduct(GetProductByGuidCRM(currSonProductToCreate));      // create a "copy" of son (with no parent yet)
                                Guid ParentId = Guid.Empty;
                                if (UpdateProductParent(newCreatedProductGuid, currProductParentSAP.ParentProductCode, out ParentId)) // update the parent to the "copy" of son created above
                                {
                                    dal.UpdateExportDateParentProducts(currProductParentSAP.SonProductCode, currProductParentSAP.ParentProductCode);
                                    success++;

                                }
                                else
                                    Errors++;
                                   
                                updateproducts++;
                                
                            }
                        }

                        else
                        {
                            totalRows++;
                            Errors++;
                            if (!productsCRM_sons.ContainsKey(currProductParentSAP.SonProductCode))
                            {
                                string msg = string.Format("Can't create son product,{0},with parent product,{1}, Son product doesn't exist.", currProductParentSAP.SonProductCode, currProductParentSAP.ParentProductCode);
                                LogManager.Logging.WriteToLog(msg);
                                dal.UpdateErrorParentProducts(currProductParentSAP.ParentProductCode, currProductParentSAP.SonProductCode, msg);                            
                            }
                            if (!productsCRM_sons.ContainsKey(currProductParentSAP.ParentProductCode))
                            {
                                string msg = string.Format("Cant create son product,{0},with parent product,{1}, Parent product doesnt exist.", currProductParentSAP.SonProductCode, currProductParentSAP.ParentProductCode);
                                LogManager.Logging.WriteToLog(msg);
                                dal.UpdateErrorParentProducts(currProductParentSAP.ParentProductCode, currProductParentSAP.SonProductCode, msg);                          
                            }
                        }
                    }
                    else
                    {
                        dal.UpdateExportDateParentProducts(currProductParentSAP.SonProductCode, currProductParentSAP.ParentProductCode);
                        totalRows++;
                        updateproducts++;
                        success++;
                    }

                }
                catch (Exception ex)
                {
                    Errors++;
                    LogManager.Logging.WriteToLog("ProductsToCRM Error: " + ex.Message + " " + ex.StackTrace);
                }
              
            }
            CreateNewInterfaceLog(start, "ProductsToCRM getl all records from OITT + create sons 'copys' when same son has few parents + update the parent to the newly created sons product+OITT ", totalRows, success, Errors);
            LogManager.Logging.WriteToLog("step 2 products total from DB : " + productsCount);
            LogManager.Logging.WriteToLog("step 2 products update to DB : " + updateproducts);
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

        #region GetProductsCRM
        private List<Dictionary<string, List<Tuple<Guid, Guid>>>> GetAllProductsFromCRM()
        {
            Dictionary<string, List<Tuple<Guid, Guid>>> allProductsCRM_sons = new Dictionary<string, List<Tuple<Guid, Guid>>>();
            Dictionary<string, List<Tuple<Guid, Guid>>> allProductsCRM_sons_parents = new Dictionary<string, List<Tuple<Guid, Guid>>>();
            leadingProducts = new List<string>();

            QueryExpression query = new QueryExpression();
            query.EntityName = "product";
            query.ColumnSet.AddColumns("new_s_product_code", "name", "new_parent_item_id", "new_b_leading_product");

            LinkEntity lk = new LinkEntity("product", "product", "new_parent_item_id", "productid", JoinOperator.LeftOuter);
            lk.Columns.AddColumns("new_s_product_code", "productid");
            lk.EntityAlias = "parent";
            query.LinkEntities.Add(lk);

            int pageNumber = 1;
            query.PageInfo = new PagingInfo();
            query.PageInfo.Count = 5000;
            query.PageInfo.PageNumber = pageNumber;
            query.PageInfo.PagingCookie = null;


            try
            {
                EntityCollection coll = new EntityCollection();
                do
                {
                    //query.PageInfo.Count = 5000;
                    query.PageInfo.PageNumber = pageNumber++;
                    query.PageInfo.PagingCookie = coll.PagingCookie;

                    coll = crmBase.XrmService.RetrieveMultiple(query);
                    foreach (Entity ent in coll.Entities)
                    {

                        if (ent.Contains("new_s_product_code") && ent["new_s_product_code"] != null)
                        {
                            string productCode = ent["new_s_product_code"].ToString();
                           
                            Guid son = ent.Id;

                            if ((bool)ent["new_b_leading_product"])
                            {
                                leadingProducts.Add(ent["new_s_product_code"].ToString());
                            }

                            Guid parent = (ent.Contains("new_parent_item_id") && ent["new_parent_item_id"] != null) ? (ent["new_parent_item_id"] as EntityReference).Id : Guid.Empty;
                            Tuple<Guid, Guid> pair = new Tuple<Guid, Guid>(son, parent);

                            if (allProductsCRM_sons.ContainsKey(productCode))
                            {
                                List<Tuple<Guid, Guid>> list = allProductsCRM_sons[productCode];
                                list.Add(pair);
                                allProductsCRM_sons[productCode] = list;
                            }
                            else
                            {
                                List<Tuple<Guid, Guid>> list = new List<Tuple<Guid, Guid>>();
                                list.Add(pair);
                                allProductsCRM_sons.Add(productCode, list);
                            }

                            if (ent.Contains("parent.new_s_product_code") && ent["parent.new_s_product_code"] != null)
                            {
                                if((ent["parent.new_s_product_code"] as AliasedValue).Value.ToString()== "CIT-B")
                                {
                                    int t = 5;
                                }
                                string productCodeFather = (ent["parent.new_s_product_code"] as AliasedValue).Value.ToString() + "^" + productCode;

                                if (allProductsCRM_sons_parents.ContainsKey(productCodeFather))
                                {
                                    List<Tuple<Guid, Guid>> list = allProductsCRM_sons_parents[productCodeFather];
                                    list.Add(new Tuple<Guid, Guid>(parent, Guid.Empty));
                                    allProductsCRM_sons_parents[productCodeFather] = list;
                                }
                                else
                                {
                                    List<Tuple<Guid, Guid>> list = new List<Tuple<Guid, Guid>>();
                                    list.Add(new Tuple<Guid, Guid>(parent, Guid.Empty));
                                    allProductsCRM_sons_parents.Add(productCodeFather, list);
                                }
                            }
                        }
                    }
                }
                while (coll.MoreRecords);
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in GetAllProductsFromCRM. ", ex.Message));
            }

            List<Dictionary<string, List<Tuple<Guid, Guid>>>> res = new List<Dictionary<string, List<Tuple<Guid, Guid>>>> { allProductsCRM_sons, allProductsCRM_sons_parents };
            return res;
        }

        private List<Dictionary<string, List<Tuple<Guid, Guid>>>> GetAllProductsWithoutParentFromCRM()
        {
            Dictionary<string, List<Tuple<Guid, Guid>>> allProductsCRM_sons = new Dictionary<string, List<Tuple<Guid, Guid>>>();
            Dictionary<string, List<Tuple<Guid, Guid>>> allProductsCRM_sons_parents = new Dictionary<string, List<Tuple<Guid, Guid>>>();
            leadingProducts = new List<string>();

            QueryExpression query = new QueryExpression();
            query.EntityName = "product";
            query.ColumnSet.AddColumns("new_s_product_code", "name", "new_parent_item_id", "new_b_leading_product");
            query.Criteria.AddCondition(new ConditionExpression("new_parent_item_id", ConditionOperator.Null));

            int pageNumber = 1;
            query.PageInfo = new PagingInfo();
            query.PageInfo.Count = 5000;
            query.PageInfo.PageNumber = pageNumber;
            query.PageInfo.PagingCookie = null;


            try
            {
                EntityCollection coll = new EntityCollection();
                do
                {
                    //query.PageInfo.Count = 5000;
                    query.PageInfo.PageNumber = pageNumber++;
                    query.PageInfo.PagingCookie = coll.PagingCookie;

                    coll = crmBase.XrmService.RetrieveMultiple(query);
                    foreach (Entity ent in coll.Entities)
                    {

                        if (ent.Contains("new_s_product_code") && ent["new_s_product_code"] != null)
                        {
                            string productCode = ent["new_s_product_code"].ToString();

                            Guid son = ent.Id;

                            if ((bool)ent["new_b_leading_product"])
                            {
                                leadingProducts.Add(ent["new_s_product_code"].ToString());
                            }

                            Guid parent = (ent.Contains("new_parent_item_id") && ent["new_parent_item_id"] != null) ? (ent["new_parent_item_id"] as EntityReference).Id : Guid.Empty;
                            Tuple<Guid, Guid> pair = new Tuple<Guid, Guid>(son, parent);

                            if (allProductsCRM_sons.ContainsKey(productCode))
                            {
                                List<Tuple<Guid, Guid>> list = allProductsCRM_sons[productCode];
                                list.Add(pair);
                                allProductsCRM_sons[productCode] = list;
                            }
                            else
                            {
                                List<Tuple<Guid, Guid>> list = new List<Tuple<Guid, Guid>>();
                                list.Add(pair);
                                allProductsCRM_sons.Add(productCode, list);
                            }

                            if (ent.Contains("parent.new_s_product_code") && ent["parent.new_s_product_code"] != null)
                            {
                                if ((ent["parent.new_s_product_code"] as AliasedValue).Value.ToString() == "CIT-B")
                                {
                                    int t = 5;
                                }
                                string productCodeFather = (ent["parent.new_s_product_code"] as AliasedValue).Value.ToString() + "^" + productCode;

                                if (allProductsCRM_sons_parents.ContainsKey(productCodeFather))
                                {
                                    List<Tuple<Guid, Guid>> list = allProductsCRM_sons_parents[productCodeFather];
                                    list.Add(new Tuple<Guid, Guid>(parent, Guid.Empty));
                                    allProductsCRM_sons_parents[productCodeFather] = list;
                                }
                                else
                                {
                                    List<Tuple<Guid, Guid>> list = new List<Tuple<Guid, Guid>>();
                                    list.Add(new Tuple<Guid, Guid>(parent, Guid.Empty));
                                    allProductsCRM_sons_parents.Add(productCodeFather, list);
                                }
                            }
                        }
                    }
                }
                while (coll.MoreRecords);
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in GetAllProductsFromCRM. ", ex.Message));
            }

            List<Dictionary<string, List<Tuple<Guid, Guid>>>> res = new List<Dictionary<string, List<Tuple<Guid, Guid>>>> { allProductsCRM_sons, allProductsCRM_sons_parents };
            return res;
        }

        private Product GetProductByGuidCRM(Guid productGuid)
        {
            Product currProduct = new Product();

            try
            {
                //Entity ent = crmBase.XrmService.Retrieve("product", productGuid, new ColumnSet("new_s_product_code", "name", "new_product_family_id", "new_b_leading_product"));
                QueryExpression query = new QueryExpression("product");
                query.ColumnSet.AddColumns("new_s_product_code", "name", "new_product_family_id", "new_b_leading_product");

                query.Criteria.AddCondition("productid", ConditionOperator.Equal, productGuid);

                LinkEntity lk = new LinkEntity("product", "new_product_family", "new_product_family_id", "new_product_familyid", JoinOperator.LeftOuter);
                lk.Columns.AddColumns("new_code");
                lk.EntityAlias = "family";
                query.LinkEntities.Add(lk);

                EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                if (coll.Entities.Count > 0)
                {
                    Entity ent = coll.Entities[0];

                    currProduct.ProductCode = (ent.Contains("new_s_product_code")) ? ent["new_s_product_code"].ToString() : null;
                    currProduct.ProductName = (ent.Contains("name")) ? ent["name"].ToString() : null;
                    currProduct.ProductFamily = (ent.Contains("family.new_code") && ent["family.new_code"] != null) ? (ent["family.new_code"] as AliasedValue).Value.ToString() : null;
                    currProduct.IsLeadingItem = (ent.Contains("new_b_leading_product") && (bool)ent["new_b_leading_product"]) ? true : false;
                }


            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in GetProductByGuidCRM. ", ex.Message));
            }
            return currProduct;
        }
        #endregion

        #region Create
        private Guid CreateProduct(Product currProduct)
        {
            productnumberIdx++;
            if (currProduct.ProductName == null || currProduct.ProductName == "")
            {
                string msg = string.Format("Error in CreateProduct. ProductName is null. Product Code,{0}.", currProduct.ProductCode);
                LogManager.Logging.WriteToLog(msg);
                dal.UpdateErrorProducts(currProduct.ProductCode, msg);
                return Guid.Empty;
            }

            Entity product_entity = new Entity("product");

            product_entity["productnumber"] = productnumberIdx.ToString();
         
            product_entity["new_s_product_code"] = currProduct.ProductCode;
            product_entity["name"] = currProduct.ProductName;
            product_entity["new_b_leading_product"] = currProduct.IsLeadingItem;
            //product_entity["new_product_family_id"] = currProduct.ProductFamily != null ? GetProductFamily(currProduct.ProductFamily) : null;             // new_product_family_id lookup
            product_entity["new_b_serial_managed"] = currProduct.SerialManaged;
            product_entity["new_product_family_id"] = null;
            if (currProduct.IsLeadingItem == true)
            {
                if (currProduct.ProductFamily == null || currProduct.ProductFamily == "")
                {
                    string msg = string.Format("Error in CreateProduct. Leading Product,{0},product family is null", currProduct.ProductCode);
                    LogManager.Logging.WriteToLog(msg);
                    dal.UpdateErrorProducts(currProduct.ProductCode, msg);
                    return Guid.Empty;
                }
                else
                {
                    EntityReference productFamilyRef = GetProductFamily(currProduct.ProductFamily);
                    if (productFamilyRef == null)
                    {
                        string msg = string.Format("Error in CreateProduct. Leading Product,{0},product family,{1},doesn't exist in CRM.", currProduct.ProductCode, currProduct.ProductFamily);
                        LogManager.Logging.WriteToLog(msg);
                        dal.UpdateErrorProducts(currProduct.ProductCode, msg);
                        return Guid.Empty;
                    }
                    product_entity["new_product_family_id"] = productFamilyRef;
                }
            }

            Guid res = Guid.Empty;
            try
            {
                // default values must be set
                product_entity["defaultuomid"] = new EntityReference("uom", new Guid(System.Configuration.ConfigurationManager.AppSettings["uom"]));
                product_entity["defaultuomscheduleid"] = new EntityReference("uomschedule", new Guid(System.Configuration.ConfigurationManager.AppSettings["uomschedule"]));
                product_entity["quantitydecimal"] = 2;

                //product_entity["statecode"] = new OptionSetValue(0);
                //product_entity["statuscode"] = new OptionSetValue(1); didn't work

                res = crmBase.XrmService.Create(product_entity);

                SetStateRequest req = new SetStateRequest();
                req.EntityMoniker = new EntityReference("product", res);
                if (currProduct.ValidFor)
                {
                    req.State = new OptionSetValue(0);
                }
                else
                    req.State = new OptionSetValue(1);

                req.Status = new OptionSetValue(-1);
                crmBase.XrmService.Execute(req);
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                string msg = string.Format("Error: {0}. Error in CreateProduct. Product,{1} productnumber{2} ", ex.Message, currProduct.ProductCode, productnumberIdx);
                LogManager.Logging.WriteToLog(msg);
                dal.UpdateErrorProducts(currProduct.ProductCode, msg);
            }

            return res;
        }

        //private bool AddNewAsLeadingProduct(Product currProduct)
        //{
        //    Entity product_entity = new Entity();
        //    product_entity.LogicalName = "product";

        //    //product_entity["new_product_family_id"] = currProduct.ProductFamily != null ? GetProductFamily(currProduct.ProductFamily) : null;             // new_product_family_id lookup
        //    //product_entity["new_s_product_code"] = currProduct.ProductCode;
        //    //product_entity["new_b_leading_product"] = currProduct.IsLeadingItem;

        //    product_entity["new_product_family_id"] = currProduct.ProductFamily != null ? GetProductFamily(currProduct.ProductFamily) : null;             // new_product_family_id lookup
        //    product_entity["productnumber"] = productnumberIdx.ToString();
        //    productnumberIdx++;
        //    product_entity["new_s_product_code"] = currProduct.ProductCode;
        //    product_entity["name"] = currProduct.ProductName;
        //    product_entity["new_b_leading_product"] = currProduct.IsLeadingItem;

        //    if (currProduct.IsLeadingItem == true && !productsCRM_sons_parents.ContainsKey(currProduct.ProductCode + "^"))
        //    {
        //        try
        //        {
        //            crmBase.XrmService.Create(product_entity);
        //        }
        //        catch (Exception ex)
        //        {
        //            LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in UpdateProduct (Create leading item). Product: {1}", ex.Message, currProduct.ProductCode));
        //            return false;
        //        }
        //    }

        //    try
        //    {
        //        crmBase.XrmService.Update(product_entity);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        CheckErrorAndCreateNewCrmBase(ex);
        //        LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in UpdateProduct. Product: {1}", ex.Message, currProduct.ProductCode));
        //        return false;
        //    }
        //}

        #endregion

        #region Update
        private bool UpdateProduct(Guid productGuid, Product currProduct)
        {
            if (currProduct.ProductName == null || currProduct.ProductName == "")
            {
                string message = string.Format("Error in UpdateProduct. ProductName is null. Product Code,{0}", currProduct.ProductCode);
                if (!logLines.Contains(message))
                {
                    LogManager.Logging.WriteToLog(message);
                    logLines.Add(message);
                    dal.UpdateErrorProducts(currProduct.ProductCode, message);
                }

                //LogManager.Logging.WriteToLog(string.Format("Error in UpdateProduct. ProductName is null. Product Code,{0}", currProduct.ProductCode));
                return false;
            }

            Entity product_entity = new Entity();
            product_entity.LogicalName = "product";
            product_entity.Id = productGuid;

            //product_entity["new_product_family_id"] = currProduct.ProductFamily != null ? GetProductFamily(currProduct.ProductFamily) : null;             // new_product_family_id lookup
            product_entity["new_s_product_code"] = currProduct.ProductCode;
            product_entity["new_b_leading_product"] = currProduct.IsLeadingItem;
            product_entity["name"] = currProduct.ProductName;
            product_entity["new_product_family_id"] = null;
            product_entity["new_b_serial_managed"] = currProduct.SerialManaged;
            if (currProduct.IsLeadingItem == true)
            {
                if (currProduct.ProductFamily == null || currProduct.ProductFamily == "")
                {
                    //LogManager.Logging.WriteToLog(string.Format("Error in UpdateProduct. Leading Product,{0},product family is null", currProduct.ProductCode));
                    string message = string.Format("Error in UpdateProduct. Leading Product,{0},product family is null", currProduct.ProductCode);
                    if (!logLines.Contains(message))
                    {
                        LogManager.Logging.WriteToLog(message);
                        logLines.Add(message);
                        dal.UpdateErrorProducts(currProduct.ProductCode, message);
                    }
                    return false;
                }
                else
                {
                    EntityReference productFamilyRef = GetProductFamily(currProduct.ProductFamily);
                    if (productFamilyRef == null)
                    {
                        //LogManager.Logging.WriteToLog(string.Format("Error in UpdateProduct. Leading Product,{0},product family,{1},doesn't exist in CRM", currProduct.ProductCode, currProduct.ProductFamily));
                        string message = string.Format("Error in UpdateProduct. Leading Product,{0},product family,{1},doesn't exist in CRM", currProduct.ProductCode, currProduct.ProductFamily);
                        if (!logLines.Contains(message))
                        {
                            LogManager.Logging.WriteToLog(message);
                            logLines.Add(message);
                            dal.UpdateErrorProducts(currProduct.ProductCode, message);
                        }
                        return false;
                    }
                    product_entity["new_product_family_id"] = productFamilyRef;
                }
            }

            if (currProduct.IsLeadingItem && !leadingProducts.Contains(currProduct.ProductCode))
            {
                //create the product in CRM
                try
                {
                    CreateProduct(currProduct);
                    leadingProducts.Add(currProduct.ProductCode);
                }
                catch (Exception ex)
                {
                    string msg = string.Format("Error: {0}. Error in UpdateProduct (Create leading item). Product,{1}", ex.Message, currProduct.ProductCode);
                    LogManager.Logging.WriteToLog(msg);
                    dal.UpdateErrorProducts(currProduct.ProductCode, msg);
                    return false;
                }

            }


            try
            {
                crmBase.XrmService.Update(product_entity);
                SetStateRequest req = new SetStateRequest();
                req.EntityMoniker = new EntityReference("product", product_entity.Id);
                if (currProduct.ValidFor)
                {
                    req.State = new OptionSetValue(0);
                }
                else req.State = new OptionSetValue(1);

                req.Status = new OptionSetValue(-1);
                crmBase.XrmService.Execute(req);
                return true;
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                string msg = string.Format("Error: {0}. Error in UpdateProduct. Product,{1}", ex.Message, currProduct.ProductCode);
                LogManager.Logging.WriteToLog(msg);
                dal.UpdateErrorProducts(currProduct.ProductCode, msg);
                return false;
            }
        }

        private bool UpdateProductParent(Guid productGuid, string productParentCode, out Guid ParentId)
        {
            Entity product_entity = new Entity();
            product_entity.LogicalName = "product";
            product_entity.Id = productGuid;
            ParentId = Guid.Empty;
            if (productParentCode != null)
            {
                EntityReference parent = GetProductParent(productParentCode);
                if (parent != null)
                {
                    product_entity["new_parent_item_id"] = parent;
                    ParentId = parent.Id;
                    try
                    {
                        crmBase.XrmService.Update(product_entity);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        CheckErrorAndCreateNewCrmBase(ex);
                        LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in UpdateProductParent. ProductParent,{1}", ex.Message, productParentCode));
                        return false;
                    }
                }
            }
            return false;
        }
        #endregion

        #region LookupFields
        private EntityReference GetProductFamily(string productFamilyCode)
        {
            EntityReference res = null;
            if (productFamilies.ContainsKey(productFamilyCode))
                res = productFamilies[productFamilyCode];
            else
            {
                QueryExpression query = new QueryExpression();
                query.EntityName = "new_product_family";
                query.ColumnSet.AddColumn("new_code");

                try
                {
                    EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                    productFamilies = new Dictionary<string, EntityReference>();
                    foreach (Entity item in coll.Entities)
                    {
                        if (item.Contains("new_code") && item["new_code"] != null)
                            productFamilies.Add(item["new_code"].ToString(), item.ToEntityReference());

                    }
                    if (productFamilies.ContainsKey(productFamilyCode))
                        res = productFamilies[productFamilyCode];
                }
                catch (Exception ex)
                {
                    CheckErrorAndCreateNewCrmBase(ex);
                    LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in GetProductFamily.", ex.Message));
                }
            }
            return res;
        }

        private EntityReference GetProductParent(string productParentCode)
        {
            EntityReference res = null;
            if (productsCRM_sons.ContainsKey(productParentCode))
                res = new EntityReference("product", productsCRM_sons[productParentCode][0].Item1);
            else
            {
                QueryExpression query = new QueryExpression();
                query.EntityName = "product";

                query.Criteria.AddCondition("new_s_product_code", ConditionOperator.Equal, productParentCode);

                try
                {
                    EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                    if (coll.Entities.Count > 0)
                    {
                        Entity ent = coll.Entities[0];
                        Tuple<Guid, Guid> t = new Tuple<Guid, Guid>(ent.Id, Guid.Empty);
                        List<Tuple<Guid, Guid>> l = new List<Tuple<Guid, Guid>>();
                        l.Add(t);
                        productsCRM_sons.Add(productParentCode, l);
                        res = ent.ToEntityReference();
                    }

                }
                catch (Exception ex)
                {
                    CheckErrorAndCreateNewCrmBase(ex);
                    LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in GetProductParent.", ex.Message));
                }
            }

            return res;
        }
        #endregion

        #region productnumber
        private string GetLatestProductnumberIndex()
        {
            QueryExpression query = new QueryExpression("product");
            query.ColumnSet.AddColumns("productnumber");
            //QueryExpression query = new QueryExpression("incident");
            //query.ColumnSet.AddColumns("incidentid");


            query.TopCount = 1;
            OrderExpression order = new OrderExpression("createdon", OrderType.Descending);
            query.Orders.Add(order);

            try
            {
                
                EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                if (coll.Entities.Count == 0)
                    return string.Format("0");
                else
                    return coll.Entities[0]["productnumber"].ToString();
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. GetLatestProductnumberIndex.", ex.Message));
                throw ex;
            }

        }
        #endregion

        #region email
        public void SendEmail()
        {
            // Email record id
            Guid wod_EmailId = Guid.Empty;

            // Creating Email 'to' recipient activity party entity object
            //Entity wod_EmailToReciepent_1 = new Entity("activityparty");
            //Entity wod_EmailToReciepent_2 = new Entity("activityparty");
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
            // Assigning receiver email address to activity party addressused attribute
            //wod_EmailToReciepent_1["addressused"] = System.Configuration.ConfigurationManager.AppSettings["ToRecipientEmailAddress_1"];
            //wod_EmailToReciepent_2["addressused"] = System.Configuration.ConfigurationManager.AppSettings["ToRecipientEmailAddress_2"];


            //wod_EmailToReciepents.Entities.Add(wod_EmailToReciepent_2);

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
                wod_EmailEntity["subject"] = "Product interface error log";
                wod_EmailEntity["description"] = string.Format("Attached herewith error log for {0}", DateTime.Now);
                wod_EmailId = crmBase.XrmService.Create(wod_EmailEntity);
                AttachFileToEmail(wod_EmailId);
            }

            else
            {
                wod_EmailEntity["subject"] = string.Format("Product interface error log - No errors for {0}", DateTime.Now);
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
            string shortFileName = LogManager.Logging.fileName.ToString().Split('\\').Last();

            Entity attach = new Entity("activitymimeattachment");
            attach["objectid"] = new EntityReference("email", wod_EmailId);
            attach["objecttypecode"] = "email";
            attach["filename"] = shortFileName;
            attach["mimetype"] = mimeType;
            attach["body"] = encodedData;

            crmBase.XrmService.Create(attach);
        }

        #endregion
        private void CheckErrorAndCreateNewCrmBase(Exception ex)
        {
            if (ex.Message.Contains("unsecured or incorrectly secured fault") || ex.Message.Contains("Invalid URI"))
            {
                System.Threading.Thread.Sleep(500);
                crmBase = new CrmBase();
            }
        }
    }
}
