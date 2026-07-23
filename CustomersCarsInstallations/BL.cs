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


namespace IntoIT.GilCar.Interfaces.CustomersCarInstallations
{
    public class BL
    {
        DAL dal;
        CrmBase crmBase;
        Dictionary<string, EntityReference> importersDict = new Dictionary<string, EntityReference>();
        Dictionary<string, EntityReference> carModelsDict = new Dictionary<string, EntityReference>();
        //Dictionary<string, EntityReference> carsDict = new Dictionary<string, EntityReference>();
        //Dictionary<string, EntityReference> productsDict = new Dictionary<string, EntityReference>();
        int customerCounter = 0;
        int carCounter = 0;
        int insCounter = 0;
        int returnsCounter = 0;
        int customerAllCounter = 0;
        int carsAllCounter = 0;
        int insAllCounter = 0;
        int currCarsCount = 0;
        int currInstallationsCount = 0;

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


            #region pre run (remove installations pre update)

            List<string> tmp = dal.getDistincedDocNums();
            long docnumToremove = tmp.LongCount();

            foreach (var docnum in tmp)
            {
                //Console.SetCursorPosition(0, 0);
               // Console.WriteLine("docnumToremove: " + docnumToremove + "        ");
                docnumToremove--;
                QueryExpression query = new QueryExpression("new_installation");
                query.ColumnSet.AddColumns("new_installationid", "new_s_docnum");
                //////////////////added by gil  shalem 09/01/2020 19:43

                query.Criteria.AddCondition("new_s_docnum", ConditionOperator.Equal, docnum);


                EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);

                if (coll.Entities.Count > 0)
                {


                    try
                    {
                        foreach (Entity ent in coll.Entities)
                            crmBase.XrmService.Delete("new_installation", ent.Id);
                    }
                    catch (Exception ex)
                    {
                        Errors++;
                      //  LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in StartInterface. delete  new_installationid,{1}", ex.Message, coll.Entities[0].Id.ToString()));
                    }
                }
            }


            #endregion
            #region first run (customer + cars + installations)
            if (ConfigurationManager.AppSettings["firstrun"] == "true")
            {

                List<Customer> customers = dal.GetAllCustomers(-1);
                totalRows = customers.Count;
                foreach (var currCustomer in customers)
                {
                    try
                    {

                        List<Car> currCars = dal.GetCarBySapOrder(currCustomer.DocNum);
                        if (currCars != null)
                            currCarsCount += currCars.Count;
                        List<Installation> currInstallations = dal.GetInstallationBySapOrder(currCustomer.DocNum);
                        if (currInstallations != null)
                            currInstallationsCount += currInstallations.Count;
                        //List<Installation> currReturns = dal.GetReturnBySapOrder(currCustomer.DocNum);
                      
                        Guid customerGuid = CheckCustomerInCRM(currCustomer);
                        Guid carGuid = Guid.Empty;
                        Guid installationGuid = Guid.Empty;
                       // YY  if (customerGuid != Guid.Empty) // if customer exists in CRM
                        if (customerGuid != Guid.Empty && currCustomer.U_XIS_CustomerID != string.Empty) // if customer exists in CRM
                        {
                            UpdateCustomer(currCustomer, customerGuid);
                            customerCounter++;
                            success++;
                            Console.SetCursorPosition(0, 0);
                            Console.WriteLine("Customers: " + customerCounter);
                        }
                        else
                        {
                            customerGuid = CreateCustomer(currCustomer);
                            if (customerGuid != Guid.Empty)
                            {
                                success++;
                                customerCounter++;
                                Console.SetCursorPosition(0, 0);
                                Console.WriteLine("Customers: " + customerCounter);
                            }
                            else
                            {
                                Errors++;
                            }
                        }



                        if (customerGuid != Guid.Empty)
                        {
                            foreach (Car currCar in currCars)
                            {
                                CheckCarInCRMResp res = CheckCarInCRM(currCar);
                                carGuid = res.CarId;
                                if (res.IsOk == false)
                                {
                                    //yy
                                   // Errors++;
                                    LogManager.Logging.WriteToLog(string.Format(" Error in StartInterface_5. currCar(SAP) ID,{0} " + " ", currCar.ID));
                                   // dal.UpdateCarErrorSAP(currCar.ID, string.Format(" Error in StartInterface_5. currCar(SAP) ID,{0} " + " ", currCar.ID));
                                   //continue;

                                }
                                if (carGuid != Guid.Empty) // check if car exists in CRM
                                {
                                    if (!UpdateCar(customerGuid, currCar, carGuid))
                                        continue;
                                    carCounter++;
                                    Console.SetCursorPosition(0, 1);
                                    Console.WriteLine("Cars: " + carCounter);
                                }
                                else
                                {
                                    carGuid = CreateCar(customerGuid, currCar);
                                    if (carGuid == Guid.Empty)
                                        continue;
                                    carCounter++;
                                    Console.SetCursorPosition(0, 1);
                                    Console.WriteLine("Cars: " + carCounter);
                                }
                                if (carGuid != Guid.Empty)
                                {
                                    foreach (Installation currInstallation in currInstallations)
                                    {
                                      //added by gil  if (currInstallation.Quantity < 0) continue;
                                        bool needUpdate = false;
                                        installationGuid = CheckInstallationInCRM(currCar, currInstallation, out needUpdate);
                                        if (installationGuid == Guid.Empty) // check if installation exists in CRM
                                        {
                                            if (CreateInstallation(carGuid, currInstallation) == Guid.Empty)
                                                continue;
                                            insCounter++;
                                            Console.SetCursorPosition(0, 2);
                                            Console.WriteLine("Installations: " + insCounter);
                                        }
                                        //else
                                        //{
                                        //    //set state to inactive and create new one 

                                        //    crmBase.XrmService.Delete("new_installation", installationGuid);
                                        //    if (CreateInstallation(carGuid, currInstallation) == Guid.Empty)
                                        //        continue;
                                        //    insCounter++;
                                        //    Console.SetCursorPosition(0, 2);
                                        //    Console.WriteLine("Installations: " + insCounter);
                                        //}
                                        ///////////////////////////////////updated by gil 09/01/2020 19:34
                                        //else if (needUpdate)
                                        //{
                                        //    UpdateInstallationDocnum(currInstallation, installationGuid);
                                        //    dal.UpdateInstallationDateSAP(currInstallation.ST_ID);
                                        //    insCounter++;
                                        //    Console.SetCursorPosition(0, 2);
                                        //    Console.WriteLine("Installations: " + insCounter);
                                        //}

                                        //else
                                        //{
                                        //    dal.UpdateInstallationDateSAP(currInstallation.ST_ID);
                                        //    insCounter++;
                                        //    Console.SetCursorPosition(0, 2);
                                        //    Console.WriteLine("Installations: " + insCounter);

                                        //}
                                        /////////////////////////////////////////////
                                        insAllCounter++;
                                        Console.SetCursorPosition(0, 17);
                                        Console.WriteLine("insAllCounter: " + insAllCounter);
                                    }
                                }
                                carsAllCounter++;
                                Console.SetCursorPosition(0, 16);
                                Console.WriteLine("carsAllCounter: " + carsAllCounter);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Errors++;
                        LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in StartInterface. CurrCustomer(SAP) ID,{1}", ex.Message, currCustomer.ID));
                    }
                    customerAllCounter++;
                    Console.SetCursorPosition(0, 15);
                    Console.WriteLine("customerAllCounter: " + customerAllCounter);
                }
                LogManager.Logging.WriteToLog("first run total cars from DB: " + currCarsCount);
                LogManager.Logging.WriteToLog("first run total cars to CRM : " + carCounter);
                LogManager.Logging.WriteToLog("first run total Installations from DB : " + currInstallationsCount);
                LogManager.Logging.WriteToLog("first run total Installations to CRM : " + insCounter);
                CreateNewInterfaceLog(start, "CustomersCarInstallations -first run (customer + cars + installations) [dbo].[ODLN_CUST]", totalRows, success, Errors);
            }
            #endregion

            #region second run (cars + installations)
            totalRows = 0;
            Errors = 0;
            success = 0;
            start = DateTime.Now;
            if (ConfigurationManager.AppSettings["secondrun"] == "true")
            {
                currCarsCount = 0;
                currInstallationsCount = 0;
                insCounter = 0;
                carCounter = 0;

                List<Car> currCars1 = dal.GetCarBySapOrder(-1);
                totalRows = currCars1.Count;
                if (currCars1 != null)
                    currCarsCount = currCars1.Count;

                foreach (Car currCar in currCars1)
                {
                    try
                    {
                        Guid customerGuid = Guid.Empty;
                        List<Customer> customer = dal.GetAllCustomers(currCar.DocNum);
                        if (customer.Count > 0)
                        {
                            customerGuid = CheckCustomerInCRM(customer[0]);
                        }
                        CheckCarInCRMResp res = CheckCarInCRM(currCar);
                        Guid carGuid = res.CarId;
                        if (res.IsOk == false)
                        {
                            //YY
                            //Errors++;
                            LogManager.Logging.WriteToLog(string.Format(" Error in StartInterface_6. currCar(SAP) ID,{0} " + "   ", currCar.ID));
                            //dal.UpdateCarErrorSAP(currCar.ID, string.Format(" Error in StartInterface_6. currCar(SAP) ID,{0} " + "  ", currCar.ID));
                            //continue;

                        }
                        if (carGuid != Guid.Empty) // check if car exists in CRM
                        {
                            if (!UpdateCar(customerGuid, currCar, carGuid))
                            {
                                Errors++;
                                continue;

                            }

                            carCounter++;
                            success++;
                            Console.SetCursorPosition(0, 8);
                            Console.WriteLine("Cars (Second run): " + carCounter);
                        }
                        else
                        {
                            carGuid = CreateCar(customerGuid, currCar);
                            if (carGuid == Guid.Empty)
                            {
                                Errors++;
                                continue;
                            }

                            success++;
                            carCounter++;
                            Console.SetCursorPosition(0, 8);
                            Console.WriteLine("Cars (Second run): " + carCounter);
                        }
                        if (carGuid != Guid.Empty)
                        {
                            List<Installation> currInstallations = dal.GetInstallationBySapOrder(currCar.DocNum);
                            if (currInstallations != null)
                                currInstallationsCount = currInstallations.Count;
                            foreach (Installation currInstallation in currInstallations)
                            {
                                bool needUpdate = false;
                                Guid installationGuid = CheckInstallationInCRM(currCar, currInstallation, out needUpdate);

                                if (installationGuid == Guid.Empty) // check if installation exists in CRM
                                {
                                    if (CreateInstallation(carGuid, currInstallation) == Guid.Empty)
                                        continue;
                                    insCounter++;
                                    Console.SetCursorPosition(0, 9);
                                    Console.WriteLine("Installations (Second run): " + insCounter);
                                }
                                else if (needUpdate)
                                {
                                    UpdateInstallationDocnum(currInstallation, installationGuid);
                                    dal.UpdateInstallationDateSAP(currInstallation.ST_ID);
                                    insCounter++;
                                    Console.SetCursorPosition(0, 9);
                                    Console.WriteLine("Installations (Second run): " + insCounter);
                                }

                                else
                                {
                                    dal.UpdateInstallationDateSAP(currInstallation.ST_ID);
                                    insCounter++;
                                    Console.SetCursorPosition(0, 9);
                                    Console.WriteLine("Installations (Second run): " + insCounter);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Errors++;
                        LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in StartInterface. currCar(SAP) ID,{1}", ex.Message, currCar.ID));

                    }

                }
                LogManager.Logging.WriteToLog("Second run total cars from DB: " + currCarsCount);
                LogManager.Logging.WriteToLog("Second run total cars to CRM : " + carCounter);
                LogManager.Logging.WriteToLog("Second run total Installations from DB : " + currInstallationsCount);
                LogManager.Logging.WriteToLog("Second run total Installations to CRM : " + insCounter);
                CreateNewInterfaceLog(start, "CustomersCarInstallations -second run (cars + installations) [dbo].[ODLN]", totalRows, success, Errors);
            }
            #endregion

            #region third run (installations)
            if (ConfigurationManager.AppSettings["thirdrun"] == "true")
            {

                totalRows = 0;
                Errors = 0;
                success = 0;
                start = DateTime.Now;
                currInstallationsCount = 0;
                int secondRunInstallations = insCounter;
                List<Installation> currInstallations1 = dal.GetInstallationBySapOrder(-1);
                totalRows = currInstallations1.Count;
                if (currInstallations1 != null)
                    currInstallationsCount = currInstallations1.Count;
                foreach (Installation currInstallation in currInstallations1)
                {
                    try
                    {
                        Car car = new Car();
                        car.LicenseNum = currInstallation.LicenseNum;
                        CheckCarInCRMResp res = CheckCarInCRM(car);
                        if (res.IsOk == false)
                        {
                            //YY
                            Errors++;
                            LogManager.Logging.WriteToLog(string.Format(" Error in StartInterface_2. currCar(SAP) ID,{0} " + " מספר שילדה שונה", car.ID));
                            dal.UpdateCarErrorSAP(car.ID, string.Format(" Error in StartInterface_2. currCar(SAP) ID,{0} " + " מספר שילדה שונה", car.ID));
                            continue;

                        }
                        Guid carGuid = res.CarId;

                        if (carGuid == Guid.Empty)
                        {
                            List<Car> currCars2 = dal.GetCarBySapOrder(currInstallation.DocNum);
                            if (currCars2.Count == 0)
                            {
                                string msg = string.Format("Error:Can't create installation.There's error in Car,{0},docnum,{1},ID,{2}.", currInstallation.LicenseNum, currInstallation.DocNum, currInstallation.ID);
                                LogManager.Logging.WriteToLog(msg);
                                dal.UpdateInstallationErrorSAP(currInstallation.ST_ID, msg);
                                Errors++;
                                continue;
                            }
                            else
                            {
                                car = currCars2[0];
                                CheckCarInCRMResp res2 = CheckCarInCRM(car);
                                carGuid = res2.CarId;
                                if (res2.IsOk == false)
                                {
                                    //YY
                                    Errors++;
                                    LogManager.Logging.WriteToLog(string.Format(" Error in StartInterface_1. currCar(SAP) ID,{0} " + " מספר שילדה שונה", car.ID));
                                    dal.UpdateCarErrorSAP(car.ID, string.Format(" Error in StartInterface_1. currCar(SAP) ID,{0} " + " מספר שילדה שונה", car.ID));
                                    continue;

                                }

                            }
                        }


                        if (carGuid == Guid.Empty)
                        {
                            string msg = string.Format("Error:Can't create installation.Car with LicenseNum,{0},was not found in CRM.ID,{1}.", currInstallation.LicenseNum, currInstallation.ID);
                            LogManager.Logging.WriteToLog(msg);
                            dal.UpdateInstallationErrorSAP(currInstallation.ST_ID, msg);
                            Errors++;
                            continue;
                        }


                        bool needUpdate = false;

                        Guid installationGuid = Guid.Empty;
                        if (currInstallation.Quantity > 0)
                            CheckInstallationInCRM(car, currInstallation, out needUpdate);
                        if (installationGuid == Guid.Empty) // check if installation exists in CRM
                        {

                            if (CreateInstallation(carGuid, currInstallation) == Guid.Empty)
                            {
                                Errors++;
                                continue;
                            }

                            insCounter++;
                            success++;
                            Console.SetCursorPosition(0, 12);
                            Console.WriteLine("Installations (Third run): " + insCounter);
                        }
                        else if (needUpdate)
                        {
                            UpdateInstallationDocnum(currInstallation, installationGuid);
                            dal.UpdateInstallationDateSAP(currInstallation.ST_ID);
                            insCounter++;
                            success++;
                            Console.SetCursorPosition(0, 12);
                            Console.WriteLine("Installations (Third run): " + insCounter);
                        }

                        else
                        {
                            dal.UpdateInstallationDateSAP(currInstallation.ST_ID);
                            insCounter++;
                            success++;
                            Console.SetCursorPosition(0, 12);
                            Console.WriteLine("Installations (Third run): " + insCounter);
                        }
                        insAllCounter++;
                        Console.SetCursorPosition(0, 17);
                        Console.WriteLine("insAllCounter: " + insAllCounter);
                    }
                    catch (Exception ex)
                    {
                        Errors++;
                        LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in StartInterface. currInstallation(SAP) ID,{1}", ex.Message, currInstallation.ID));

                    }

                }
                LogManager.Logging.WriteToLog("Third run total Installations from DB : " + currInstallationsCount);
                LogManager.Logging.WriteToLog("Third run total Installations to CRM : " + (insCounter - secondRunInstallations));
                CreateNewInterfaceLog(start, "CustomersCarInstallations -third run (installations)  DLN1+ODLN", totalRows, success, Errors);
            }
            #endregion


            #region fourth run (returns)
            if (ConfigurationManager.AppSettings["fourthrun"] == "true")
            {

                totalRows = 0;
                Errors = 0;
                success = 0;
                int returnsCount = 0;
                start = DateTime.Now;
              //  int secondRunInstallations = insCounter;
                List<Installation> returns = dal.GetAllReturns();
                totalRows = returns.Count;
                  if (returns != null)
                {
                   // returnsCount = returns.Count;
                    totalRows = returns.Count;
                }
             
                
                foreach (Installation currInstallation in returns)
                {
                    try
                    {
                        Car car = new Car();
                        car.LicenseNum = currInstallation.LicenseNum;
                        CheckCarInCRMResp res = CheckCarInCRM(car);
                        if (res.IsOk == false)
                        {
                            //YY
                            Errors++;
                            LogManager.Logging.WriteToLog(string.Format(" Error in StartInterface_3. currCar(SAP) ID,{0} " + " מספר שילדה שונה", car.ID));
                            dal.UpdateCarErrorSAP(car.ID, string.Format(" Error in StartInterface_3. currCar(SAP) ID,{0} " + " מספר שילדה שונה", car.ID));
                            continue;

                        }
                        Guid carGuid = res.CarId;

                        if (carGuid == Guid.Empty)
                        {
                            List<Car> currCars2 = dal.GetCarBySapOrder(currInstallation.DocNum);
                            if (currCars2.Count == 0)
                            {
                                string msg = string.Format("Error:Can't create returns.There's error in Car,{0},docnum,{1},ID,{2}.", currInstallation.LicenseNum, currInstallation.DocNum, currInstallation.ID);
                                LogManager.Logging.WriteToLog(msg);
                                dal.UpdateInstallationErrorSAP(currInstallation.ST_ID, msg);
                                Errors++;
                                continue;
                            }
                            else
                            {
                                car = currCars2[0];
                                CheckCarInCRMResp res2 = CheckCarInCRM(car);
                                carGuid = res2.CarId;
                                if (res2.IsOk == false)
                                {
                                    //YY
                                    Errors++;
                                    LogManager.Logging.WriteToLog(string.Format(" Error in StartInterface_4. currCar(SAP) ID,{0} " + " מספר שילדה שונה", car.ID));
                                    dal.UpdateCarErrorSAP(car.ID, string.Format(" Error in StartInterface_4. currCar(SAP) ID,{0} " + " מספר שילדה שונה", car.ID));
                                    continue;

                                }

                            }
                        }


                        if (carGuid == Guid.Empty)
                        {
                            string msg = string.Format("Error:Can't create return.Car with LicenseNum,{0},was not found in CRM.ID,{1}.", currInstallation.LicenseNum, currInstallation.ID);
                            LogManager.Logging.WriteToLog(msg);
                            dal.UpdateInstallationErrorSAP(currInstallation.ST_ID, msg);
                            Errors++;
                            continue;
                        }


                        bool needUpdate = false;

                        Guid installationGuid = Guid.Empty;
                        if (currInstallation.Quantity > 0)
                            CheckInstallationInCRM(car, currInstallation, out needUpdate);
                        if (installationGuid == Guid.Empty) // check if installation exists in CRM
                        {

                            if (CreateInstallation(carGuid, currInstallation) == Guid.Empty)
                            {
                                Errors++;
                                continue;
                            }

                            returnsCount++;
                            success++;
                            Console.SetCursorPosition(0, 15);
                            dal.UpdateInstallationDateSAP(currInstallation.ST_ID);
                            Console.WriteLine("fourth run (returns): " + totalRows);
                        }
                        else if (needUpdate)
                        {
                            UpdateInstallationDocnum(currInstallation, installationGuid);
                            dal.UpdateInstallationDateSAP(currInstallation.ST_ID);
                            returnsCount++;
                            success++;
                            Console.SetCursorPosition(0, 15);
                            Console.WriteLine("fourth run (returns): " + returnsCount);
                        }

                        else
                        {
                            dal.UpdateInstallationDateSAP(currInstallation.ST_ID);
                            returnsCount++;
                            success++;
                            Console.SetCursorPosition(0, 16);
                            Console.WriteLine("fourth run (returns): " +(totalRows- returnsCount));
                        }
                        dal.UpdateInstallationDateSAP(currInstallation.ST_ID);
                        insAllCounter++;
                        Console.SetCursorPosition(0, 17);
                        Console.WriteLine("returnsCounter: " + returnsCount);
                    }
                    catch (Exception ex)
                    {
                        Errors++;
                        LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in StartInterface. currInstallation(SAP) ID,{1}", ex.Message, currInstallation.ID));

                    }

                }
                LogManager.Logging.WriteToLog("fourth run total returns from DB : " + totalRows);
                LogManager.Logging.WriteToLog("fourth run total returns to CRM : " + (totalRows - returnsCount));
                CreateNewInterfaceLog(start, "CustomersCarInstallations fourth run total returns", totalRows, success, Errors);
            }
            #endregion
            #region //fourth run (returns)
            //if (ConfigurationManager.AppSettings["fourthrun"] == "true")
            //{
            //    totalRows = 0;
            //    Errors = 0;
            //    success = 0;
            //    int returnsCount = 0;
            //    start = DateTime.Now;
            //    try
            //    {

            //        List<Installation> returns = dal.GetAllReturns();
            //        if (returns != null)
            //        {
            //            returnsCount = returns.Count;
            //            totalRows = returns.Count;
            //        }


            //        foreach (Installation currReturn in returns)
            //        {
            //            Car car = new Car();
            //            car.LicenseNum = currReturn.LicenseNum;
            //            CheckCarInCRMResp res = CheckCarInCRM(car);
            //            if (res.IsOk == false)
            //            {
            //                Errors++;
            //                LogManager.Logging.WriteToLog(string.Format(" Error in StartInterface. currCar(SAP) ID,{0} " + " מספר שילדה שונה", car.ID));
            //                dal.UpdateCarErrorSAP(car.ID, string.Format(" Error in StartInterface. currCar(SAP) ID,{0} " + " מספר שילדה שונה", car.ID));
            //                continue;

            //            }
            //            Guid carGuid = res.CarId;

            //            if (carGuid == Guid.Empty)
            //            {
            //                List<Car> currCars2 = dal.GetCarBySapOrder(currReturn.DocNum);
            //                if (currCars2.Count == 0)
            //                {
            //                    string msg = string.Format("Error:Can't create returns.There's error in Car,{0},docnum,{1},ID,{2}.", currReturn.LicenseNum, currReturn.DocNum, currReturn.ID);
            //                    LogManager.Logging.WriteToLog(msg);
            //                    dal.UpdateInstallationErrorSAP(currReturn.ST_ID, msg);
            //                    Errors++;
            //                    continue;
            //                }
            //                else
            //                {
            //                    car = currCars2[0];
            //                    CheckCarInCRMResp res2 = CheckCarInCRM(car);
            //                    carGuid = res2.CarId;
            //                    if (res2.IsOk == false)
            //                    {
            //                        Errors++;
            //                        LogManager.Logging.WriteToLog(string.Format(" Error in StartInterface. currCar(SAP) ID,{0} " + " מספר שילדה שונה", car.ID));
            //                        dal.UpdateCarErrorSAP(car.ID, string.Format(" Error in StartInterface. currCar(SAP) ID,{0} " + " מספר שילדה שונה", car.ID));
            //                        continue;

            //                    }

            //                }
            //            }


            //            if (carGuid == Guid.Empty)
            //            {
            //                string msg = string.Format("Error:Can't create returns.Car with LicenseNum,{0},was not found in CRM.ID,{1}.", currReturn.LicenseNum, currReturn.ID);
            //                LogManager.Logging.WriteToLog(msg);
            //                dal.UpdateInstallationErrorSAP(currReturn.ST_ID, msg);
            //                Errors++;
            //                continue;
            //            }




            //            Guid installationGuid = Guid.Empty;

            //                if (CreateInstallation(currReturn, leadingProduct))
            //                {
            //                    success++;
            //                    returnsCounter++;
            //                    Console.SetCursorPosition(0, 15);
            //                    Console.WriteLine("Returns (Fourth run): " + returnsCounter);
            //                }
            //                else
            //                {
            //                    Errors++;
            //                }
            //            }

            //            else
            //            {
            //                Errors++;
            //                string msg = string.Format("Error in Returns. Product,{0},is not leading or doesn't exist. ID,{1} ", currReturn.ProductCode, currReturn.ID);
            //                LogManager.Logging.WriteToLog(msg);
            //                dal.UpdateReturnErrorSAP(currReturn.ID, msg);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Errors++;
            //        LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in StartInterface (Returns) {1}", ex.Message, ex.StackTrace));
            //    }

            //    LogManager.Logging.WriteToLog("total Returns from DB : " + returnsCount);
            //    LogManager.Logging.WriteToLog("total Returns  to CRM : " + returnsCounter);
            //    CreateNewInterfaceLog(start, "CustomersCarInstallations -fourth run (returns) [dbo].[RDN1]", totalRows, success, Errors);

            //}
            #endregion
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

        #region CheckInCrm
        private Guid CheckCustomerInCRM(Customer customer)
        {
            Guid id = Guid.Empty;

            QueryExpression query = new QueryExpression();
            query.EntityName = "account";
            query.ColumnSet.AddColumns("accountid");
            query.Criteria.AddCondition("new_s_xis_customerid", ConditionOperator.Equal, customer.U_XIS_CustomerID);

            try
            {
                EntityCollection coll2 = crmBase.XrmService.RetrieveMultiple(query);
                if (coll2 != null && coll2.Entities.Count > 0)
                    id = coll2.Entities[0].Id;
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                string msg = string.Format("Error: {0}. Error in CheckCustomerInCRM. ID,{1}", ex.Message, customer.ID);
                LogManager.Logging.WriteToLog(string.Format(msg));
                if (!ex.Message.Contains("Invalid URI") && !ex.Message.Contains("unsecured"))
                    dal.UpdateCustomerErrorSAP(customer.ID, msg);
            }

            if (id == Guid.Empty)
            {
                query = new QueryExpression();
                query.EntityName = "account";
                query.ColumnSet.AddColumns("accountid");
                if (!(customer.Phone1 == null && customer.Phone2 == null) && !(customer.Phone1 == string.Empty && customer.Phone2 == string.Empty))
                {
                    query.Criteria.AddCondition("name", ConditionOperator.Equal, customer.Name);   // condition 1

                    FilterExpression mainFilter = new FilterExpression(LogicalOperator.Or);

                    FilterExpression filter1 = new FilterExpression(LogicalOperator.Or);
                    filter1.AddCondition("telephone1", ConditionOperator.Equal, customer.Phone1);
                    filter1.AddCondition("telephone1", ConditionOperator.Equal, customer.Phone2);
                    FilterExpression filter1_ = new FilterExpression();
                    filter1_.AddCondition("telephone1", ConditionOperator.NotNull);
                    filter1.AddFilter(filter1_);


                    FilterExpression filter2 = new FilterExpression(LogicalOperator.Or);
                    filter2.AddCondition("telephone2", ConditionOperator.Equal, customer.Phone1);
                    filter2.AddCondition("telephone2", ConditionOperator.Equal, customer.Phone2);
                    FilterExpression filter2_ = new FilterExpression();
                    filter2_.AddCondition("telephone2", ConditionOperator.NotNull);
                    filter2.AddFilter(filter2_);

                    mainFilter.AddFilter(filter1);
                    mainFilter.AddFilter(filter2);
                    query.Criteria.AddFilter(mainFilter);
                }
            }



            else return id;

            try
            {
                EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                if (coll.Entities.Count > 0)
                    id = coll.Entities[0].Id;
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                string msg = string.Format("Error: {0}. Error in CheckCustomerInCRM. ID,{1}", ex.Message, customer.ID);
                LogManager.Logging.WriteToLog(string.Format(msg));
                if (!ex.Message.Contains("Invalid URI") && !ex.Message.Contains("unsecured"))
                    dal.UpdateCustomerErrorSAP(customer.ID, msg);
            }

            return id;
        }

        public class CheckCarInCRMResp
        {
            public bool IsOk;
            public Guid CarId;
        }

        private CheckCarInCRMResp CheckCarInCRM(Car car)
        {
            bool isOk = false;
            Guid id = Guid.Empty;
            QueryExpression query = new QueryExpression();
            query.EntityName = "new_car";
            query.ColumnSet.AddColumns("new_carid");
            if (car.ChasisNum != null && !string.IsNullOrWhiteSpace(car.ChasisNum))
                query.Criteria.AddCondition("new_s_chassis_num", ConditionOperator.Equal, car.ChasisNum);
            if (car.LicenseNum != null && !string.IsNullOrWhiteSpace(car.LicenseNum))
                query.Criteria.AddCondition("new_s_license_num", ConditionOperator.Equal, car.LicenseNum);

            try
            {
                EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                if (coll.Entities.Count > 0)
                {
                    isOk = true;
                    id = coll.Entities[0].Id;
                }

                else
                {
                    query = new QueryExpression("new_car");
                    query.ColumnSet.AddColumns("new_carid");
                    query.Criteria.AddCondition("new_s_chassis_num", ConditionOperator.Equal, car.ChasisNum);
                    coll = crmBase.XrmService.RetrieveMultiple(query);
                    if (coll.Entities.Count > 0)
                    {
                        isOk = true;
                        id = coll.Entities[0].Id;
                        Entity carToupdate = new Entity("new_car");
                        carToupdate.Id = id;
                        carToupdate["new_s_license_num"] = car.LicenseNum;
                        crmBase.XrmService.Update(carToupdate);

                    }
                    //else
                    //{
                    //    query = new QueryExpression("new_car");
                    //    query.ColumnSet.AddColumns("new_s_chassis_num");
                    //    query.Criteria.AddCondition("new_s_license_num", ConditionOperator.Equal, car.LicenseNum);
                    //    coll = crmBase.XrmService.RetrieveMultiple(query);
                    //    if (coll.Entities.Count > 0)
                    //    {
                    //        Entity carForchassis_num = coll.Entities[0];
                    //        if (carForchassis_num.Contains("new_s_chassis_num") && carForchassis_num["new_s_chassis_num"] != null)
                    //        {
                    //            if (car.ChasisNum.Trim() != carForchassis_num["new_s_chassis_num"].ToString().Trim())
                    //            {
                    //                Entity carToupdate = new Entity("new_car");
                    //                carToupdate.Id = coll.Entities[0].Id;
                    //                carToupdate["new_s_chassis_num"] = car.ChasisNum;
                    //                crmBase.XrmService.Update(carToupdate);
                    //                id = coll.Entities[0].Id;
                    //            }


                    //        }
                    //        else
                    //        {
                    //            Entity carToupdate = new Entity("new_car");
                    //            carToupdate.Id = coll.Entities[0].Id;
                    //            carToupdate["new_s_chassis_num"] = car.ChasisNum;
                    //            crmBase.XrmService.Update(carToupdate);
                    //            id = coll.Entities[0].Id;

                    //        }
                    //    }

                    //}


                }
            }
            catch (Exception ex)
            {
                try
                {
                    CheckErrorAndCreateNewCrmBase(ex);
                    string msg = string.Format("Error: {0}. Error in CheckCarInCRM. ID,{1}", ex.Message, car.ID);
                    LogManager.Logging.WriteToLog(msg);
                    if (!ex.Message.Contains("Invalid URI") && !ex.Message.Contains("unsecured"))
                        dal.UpdateCarErrorSAP(car.ID, msg);
                   
                    // res1.CarId =Guid.Empty;
                }
                catch (Exception e)
                {
                    CheckCarInCRMResp res1 = new CheckCarInCRMResp();
                    res1.IsOk = false;
                    return res1;
                }
            }

            CheckCarInCRMResp res = new CheckCarInCRMResp();
            res.CarId = id;
            res.IsOk = isOk;
            return res;
        }

        private Guid CheckInstallationInCRM(Car car, Installation currInstallation, out bool needUpdate)
        {
            needUpdate = false;
            Guid id = Guid.Empty;
            QueryExpression query = new QueryExpression("new_installation");
            query.ColumnSet.AllColumns = true;//AddColumns("new_installationid", "new_s_docnum");
            query.Criteria.AddCondition("new_s_docnum", ConditionOperator.Equal, currInstallation.DocNum.ToString());
            if (!string.IsNullOrEmpty(currInstallation.SerialNum))
                query.Criteria.AddCondition("new_s_item_sn", ConditionOperator.Equal, currInstallation.SerialNum.ToString());
            //////////////////added by gil  shalem 09/01/2020 19:43
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            //////////////////
            LinkEntity lk1 = new LinkEntity("new_installation", "new_car", "new_car_id", "new_carid", JoinOperator.Inner);
            lk1.LinkCriteria.AddCondition("new_s_license_num", ConditionOperator.Equal, car.LicenseNum);

            LinkEntity lk2 = new LinkEntity("new_installation", "product", "new_product_id", "productid", JoinOperator.Inner);
            lk2.LinkCriteria.AddCondition("new_s_product_code", ConditionOperator.Equal, currInstallation.ProductCode);

            if (!string.IsNullOrWhiteSpace(currInstallation.ParentProductCode))
            {
                LinkEntity lk3 = new LinkEntity("product", "product", "new_parent_item_id", "productid", JoinOperator.Inner);
                lk3.LinkCriteria.AddCondition("new_s_product_code", ConditionOperator.Equal, currInstallation.ParentProductCode);
                lk2.LinkEntities.Add(lk3);
            }
            else
                lk2.LinkCriteria.AddCondition("new_parent_item_id", ConditionOperator.Null);

            query.LinkEntities.Add(lk1);
            query.LinkEntities.Add(lk2);

            try
            {
                EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                if (coll.Entities.Count > 0)
                {
                    id = coll.Entities[0].Id;
                    if (!coll.Entities[0].Contains("new_s_docnum") || coll.Entities[0]["new_s_docnum"] == null)
                        needUpdate = true;
                }
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                string msg = string.Format("Error: {0}. Error in CheckInstallationInCRM. Docnum,{1},ID,{2}", ex.Message, currInstallation.DocNum, currInstallation.ID);
                LogManager.Logging.WriteToLog(msg);
                if (!ex.Message.Contains("Invalid URI") && !ex.Message.Contains("unsecured"))
                    dal.UpdateInstallationErrorSAP(currInstallation.ST_ID, msg);
            }

            return id;
        }
        #endregion

        #region CreateInCrm
        private Guid CreateCustomer(Customer customer)
        {
            Entity customer_entity = new Entity("account");

            if (customer.Name == null || customer.Name == string.Empty)
            {
                string msg = string.Format("Error: Customer Name is null. ID,{0}", customer.ID);
                LogManager.Logging.WriteToLog(msg);
                dal.UpdateCustomerErrorSAP(customer.ID, msg);
                return Guid.Empty;
            }

            customer_entity["new_i_id"] = customer.ID;
            customer_entity["name"] = customer.Name;
            customer_entity["telephone1"] = customer.Phone1;
            customer_entity["telephone2"] = customer.Phone2;
            customer_entity["new_address"] = customer.Address;
            customer_entity["new_s_xis_customerid"] = customer.U_XIS_CustomerID;

            Guid res = Guid.Empty;
            try
            {
                res = crmBase.XrmService.Create(customer_entity);
                dal.UpdateCustomerDateSAP(customer.ID);
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                string msg = string.Format("Error: {0}. Error in CreateCustomer. ID,{1}", ex.Message, customer.ID);
                LogManager.Logging.WriteToLog(msg);
                if (!ex.Message.Contains("Invalid URI") && !ex.Message.Contains("unsecured"))
                    dal.UpdateCustomerErrorSAP(customer.ID, msg);
            }

            return res;
        }


        private Guid CreateCar(Guid customerGuid, Car currCar)
        {

            Entity car_entity = new Entity();
            car_entity.LogicalName = "new_car";
            car_entity["new_s_license_num"] = currCar.LicenseNum;
            //int temp;
            //if (int.TryParse(currCar.LicenseNum, out temp))
            //    car_entity["new_s_license_num"] = currCar.LicenseNum;
            //else
            //{
            //    string msg = string.Format("Error in CreateCar. LicenseNum is not numeric. LicenseNum,{0},ID,{1}", currCar.LicenseNum, currCar.ID);
            //    LogManager.Logging.WriteToLog(msg);
            //    dal.UpdateCarErrorSAP(currCar.ID, msg);
            //    return Guid.Empty;
            //}

            if (customerGuid != Guid.Empty)
                car_entity["new_account_id"] = new EntityReference("account", customerGuid); // new_account_id lookup

            if (currCar.LicenseNum == null || currCar.LicenseNum == "")
            {
                string msg = string.Format("Error in CreateCar. LicenseNum is null. ID,{0}", currCar.ID);
                LogManager.Logging.WriteToLog(msg);
                dal.UpdateCarErrorSAP(currCar.ID, msg);
                return Guid.Empty;
            }

            EntityReference importerRef = GetImporter(currCar.ImporterCode);
            if (importerRef == null)
            {
                string msg = string.Format("Error in CreateCar. Could not find reference to importer name,{0},in car,{1},ID,{2}", currCar.ImporterCode, currCar.LicenseNum, currCar.ID);
                LogManager.Logging.WriteToLog(msg);
                dal.UpdateCarErrorSAP(currCar.ID, msg);
                return Guid.Empty;
            }
            car_entity["new_importer_name_id"] = importerRef;          // new_importer_name_id lookup


            EntityReference carModelRef = GetCarModel(currCar.CarModel);
            if (carModelRef == null)
            {
                string msg = string.Format("Error in CreateCar. Could not find reference to car model,{0},in car,{1},ID,{2}", currCar.CarModel, currCar.LicenseNum, currCar.ID);
                LogManager.Logging.WriteToLog(msg);
                dal.UpdateCarErrorSAP(currCar.ID, msg);
                return Guid.Empty;
            }
            car_entity["new_car_model_id"] = carModelRef;           // new_car_model_id lookup

            car_entity["new_i_id"] = currCar.ID;
            car_entity["new_d_doc_date"] = currCar.DocDate;
            car_entity["new_s_chassis_num"] = currCar.ChasisNum;
            //car_entity["new_s_license_num"] = currCar.LicenseNum;
            car_entity["new_s_color_code"] = currCar.ColorCode;
            car_entity["new_s_agency"] = currCar.Agency;
            car_entity["new_s_mnf_desc"] = currCar.ManufacturerDesc;
            //car_entity["new_car_model_id"] = currCar.CarModel;
            car_entity["new_s_mobileye_num"] = currCar.MobileeyeNum;
            car_entity["new_s_agency_desc"] = currCar.AgencyDesc;

            Guid res = Guid.Empty;
            try
            {
                res = crmBase.XrmService.Create(car_entity);
                dal.UpdateCarDateSAP(currCar.ID);
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                string msg = string.Format("Error: {0}. Error in CreateCar. ID,{1} ", ex.Message, currCar.ID);
                LogManager.Logging.WriteToLog(msg);
                if (!ex.Message.Contains("Invalid URI") && !ex.Message.Contains("unsecured"))
                    dal.UpdateCarErrorSAP(currCar.ID, msg);
            }

            return res;
        }
        private bool updateInstallationforReturn(Installation currReturn, Entity currReturnCrmEntity)
        {
            bool result = true;
            try
            {
                Guid id = Guid.Empty;
                QueryExpression query = new QueryExpression("new_installation");
                query.ColumnSet.AllColumns = true;// AddColumns("new_installationid", "new_s_docnum");
                //////////////////added by gil  shalem 09/01/2020 19:43
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                query.Criteria.AddCondition("new_s_docnum", ConditionOperator.NotEqual, currReturn.DocNum.ToString());
                query.Criteria.AddCondition("new_car_id", ConditionOperator.Equal, (currReturnCrmEntity["new_car_id"] as EntityReference).Id);
                query.Criteria.AddCondition("new_i_qty", ConditionOperator.GreaterThan, 0);
                LinkEntity lk2 = new LinkEntity("new_installation", "product", "new_product_id", "productid", JoinOperator.Inner);
                lk2.LinkCriteria.AddCondition("new_s_product_code", ConditionOperator.Equal, currReturn.ProductCode);

                if (!string.IsNullOrWhiteSpace(currReturn.ParentProductCode))
                {
                    LinkEntity lk3 = new LinkEntity("product", "product", "new_parent_item_id", "productid", JoinOperator.Inner);
                    lk3.LinkCriteria.AddCondition("new_s_product_code", ConditionOperator.Equal, currReturn.ParentProductCode);
                    lk2.LinkEntities.Add(lk3);
                }
                // else
                //     lk2.LinkCriteria.AddCondition("new_parent_item_id", ConditionOperator.Null);


                query.LinkEntities.Add(lk2);

                if (!string.IsNullOrEmpty(currReturn.SerialNum))//have a sireal need to find this specific serial
                {
                    query.Criteria.AddCondition("new_s_item_sn", ConditionOperator.Equal, currReturnCrmEntity.GetAttributeValue<string>("new_s_item_sn"));
                }

                EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                if (coll.Entities.Count > 0)
                {
                    SetStateRequest req1 = new SetStateRequest();
                    req1.EntityMoniker = new EntityReference("new_installation", coll.Entities[0].Id);
                    req1.State = new OptionSetValue(1);
                    req1.Status = new OptionSetValue(2);
                    crmBase.XrmService.Execute(req1);
                    // crmBase.XrmService.Delete("new_installation", coll.Entities[0].Id);

                }
                //else
                //{
                //    string msg = string.Format("Error: {0}. Error in updateInstallationforReturn. ID,{1} ", "not found same product in qty>0", currReturn.ST_ID);
                //    LogManager.Logging.WriteToLog(msg);
                //    dal.UpdateInstallationErrorSAP(currReturn.ST_ID, msg);
                //    result = false;
                //}

            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                string msg = string.Format("Error: {0}. Error in updateInstallationforReturn. ID,{1} ", ex.Message, currReturn.ST_ID);
                LogManager.Logging.WriteToLog(msg);
                if (!ex.Message.Contains("Invalid URI") && !ex.Message.Contains("unsecured"))
                    dal.UpdateInstallationErrorSAP(currReturn.ST_ID, msg);
                result = false;
            }
            return result;
        }
        private Guid CreateInstallation_old(Guid carGuid, Installation currInstallation)
        {
            Entity installation_entity = new Entity("new_installation");

            EntityReference childProductRef = GetChildProduct(currInstallation.ProductCode, currInstallation.ParentProductCode);
            if (childProductRef == null)
            {
                string msg = string.Format("Error in CreateInstallation. Could not find reference to child product,{0},with parent,{1},ID,{2} ", currInstallation.ProductCode, currInstallation.ParentProductCode, currInstallation.ID);
                LogManager.Logging.WriteToLog(msg);
                dal.UpdateInstallationErrorSAP(currInstallation.ST_ID, msg);
                return Guid.Empty;
            }
            else
            {
                installation_entity["new_product_id"] = childProductRef;          // new_product_id lookup
                installation_entity["new_parent_item_id"] = GetParentProduct(childProductRef.Id);  // new_parent_item_id lookup
            }

            EntityReference carRef = new EntityReference("new_car", carGuid);
            if (carRef == null)
            {
                string msg = string.Format("Error in CreateInstallation. Could not find reference to car. ID,{0}", currInstallation.ID);
                LogManager.Logging.WriteToLog(msg);
                dal.UpdateInstallationErrorSAP(currInstallation.ST_ID, msg);
                return Guid.Empty;
            }
            installation_entity["new_car_id"] = carRef;           // new_car_id lookup

            installation_entity["new_i_id"] = currInstallation.ID;
            installation_entity["new_d_doc_date"] = currInstallation.DocDate;
            installation_entity["new_i_qty"] = currInstallation.Quantity;
            installation_entity["new_s_swhs_code"] = currInstallation.WharehouseCode;
            installation_entity["new_s_item_sn"] = currInstallation.SerialNum;
            installation_entity["new_s_tel_num"] = currInstallation.TelNum;
            installation_entity["new_s_imei_num"] = currInstallation.IMEINum;
            installation_entity["new_s_mac_address"] = currInstallation.MACNum;
            installation_entity["new_s_docnum"] = currInstallation.DocNum.ToString();
            installation_entity["new_s_installation_location"] = currInstallation.ODLN_cardname;
            installation_entity["new_s_installation_code"] = currInstallation.ODLN_cardcode;

            Guid res = Guid.Empty;
            try
            {
                res = crmBase.XrmService.Create(installation_entity);
                dal.UpdateInstallationDateSAP(currInstallation.ST_ID);
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                string msg = string.Format("Error: {0}. Error in CreateInstallation. ID,{1}", ex.Message, currInstallation.ID);
                LogManager.Logging.WriteToLog(msg);
                if (!ex.Message.Contains("Invalid URI") && !ex.Message.Contains("unsecured"))
                    dal.UpdateInstallationErrorSAP(currInstallation.ST_ID, msg);
            }

            return res;
        }


        private Guid CreateInstallation(Guid carGuid, Installation currInstallation)
        {
            Entity installation_entity = new Entity("new_installation");

            EntityReference childProductRef = GetChildProduct(currInstallation.ProductCode, currInstallation.ParentProductCode);
            if (childProductRef == null)
            {
                string msg = string.Format("Error in CreateInstallationWithReturns. Could not find reference to child product,{0},with parent,{1},ID,{2} ", currInstallation.ProductCode, currInstallation.ParentProductCode, currInstallation.ID);
                LogManager.Logging.WriteToLog(msg);
                dal.UpdateInstallationErrorSAP(currInstallation.ST_ID, msg);
                return Guid.Empty;
            }
            else
            {
                installation_entity["new_product_id"] = childProductRef;          // new_product_id lookup
                installation_entity["new_parent_item_id"] = GetParentProduct(childProductRef.Id);  // new_parent_item_id lookup
            }

            EntityReference carRef = new EntityReference("new_car", carGuid);
            if (carRef == null)
            {
                string msg = string.Format("Error in CreateInstallationWithReturns. Could not find reference to car. ID,{0}", currInstallation.ID);
                LogManager.Logging.WriteToLog(msg);
                dal.UpdateInstallationErrorSAP(currInstallation.ST_ID, msg);
                return Guid.Empty;
            }
            installation_entity["new_car_id"] = carRef;           // new_car_id lookup

            installation_entity["new_i_id"] = currInstallation.ID;
            installation_entity["new_st_id"] = currInstallation.ST_ID;
            installation_entity["new_d_doc_date"] = currInstallation.DocDate;
            installation_entity["new_i_qty"] = currInstallation.Quantity;
            installation_entity["new_s_swhs_code"] = currInstallation.WharehouseCode;
            installation_entity["new_s_item_sn"] = currInstallation.SerialNum;
            installation_entity["new_s_tel_num"] = currInstallation.TelNum;
            installation_entity["new_s_imei_num"] = currInstallation.IMEINum;
            installation_entity["new_s_mac_address"] = currInstallation.MACNum;
            installation_entity["new_s_docnum"] = currInstallation.DocNum.ToString();
            installation_entity["new_s_installation_location"] = currInstallation.ODLN_cardname;
            installation_entity["new_s_installation_code"] = currInstallation.ODLN_cardcode;

            Guid res = Guid.Empty;
            try
            {
                res = crmBase.XrmService.Create(installation_entity);
                if (currInstallation.Quantity < 0)
                {
                    SetStateRequest req1 = new SetStateRequest();
                    req1.EntityMoniker = new EntityReference("new_installation", res);
                    req1.State = new OptionSetValue(1);
                    req1.Status = new OptionSetValue(2);
                    crmBase.XrmService.Execute(req1);
                    updateInstallationforReturn(currInstallation, installation_entity);

                }
                dal.UpdateInstallationDateSAP(currInstallation.ST_ID);
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                string msg = string.Format("Error: {0}. Error in CreateInstallationWithReturns. ID,{1}", ex.Message, currInstallation.ID);
                LogManager.Logging.WriteToLog(msg);
                if (!ex.Message.Contains("Invalid URI") && !ex.Message.Contains("unsecured"))
                    dal.UpdateInstallationErrorSAP(currInstallation.ST_ID, msg);
            }

            return res;
        }

        private bool CreateReturn_disabled(Return currReturn, EntityReference returnProduct)
        {
            Entity return_installation_entity = new Entity("new_installation");

            EntityReference carRef = GetCar(currReturn.LicenseNum);
            if (carRef == null)
            {
                string msg = string.Format("Error in CreateReturn. Could not find reference to car,{0},ID,{1}", currReturn.LicenseNum, currReturn.ID);
                LogManager.Logging.WriteToLog(msg);
                dal.UpdateReturnErrorSAP_disabled(currReturn.ID, msg);
                return false;
            }
            return_installation_entity["new_car_id"] = carRef;

            return_installation_entity["new_i_id"] = currReturn.ID;

            ///////////////////////////////////updated by gil 09/01/2020 19:34
            return_installation_entity["new_d_doc_date"] = currReturn.DocDate;
            //return_installation_entity["new_d_doc_date"] = currReturn.ODLN_docdate;
            ///////////////////////////////////
            return_installation_entity["new_i_qty"] = currReturn.Quantity * (-1);
            return_installation_entity["new_s_swhs_code"] = currReturn.WharehouseCode;
            return_installation_entity["new_s_item_sn"] = currReturn.SerialNum;
            return_installation_entity["new_s_tel_num"] = currReturn.TelNum;
            return_installation_entity["new_s_imei_num"] = currReturn.IMEINum;
            return_installation_entity["new_s_mac_address"] = currReturn.MACNum;
            return_installation_entity["new_s_docnum"] = currReturn.DocNum.ToString();
            return_installation_entity["new_s_installation_location"] = currReturn.ODLN_cardname;
            return_installation_entity["new_s_installation_code"] = currReturn.ODLN_cardcode;
            //return_installation_entity["new_car_id"]=currReturn.LicenseNum;
            return_installation_entity["new_product_id"] = returnProduct;

            try
            {
                Guid res = crmBase.XrmService.Create(return_installation_entity);


                if (res != Guid.Empty)
                {
                    Guid id = Guid.Empty;
                    QueryExpression query = new QueryExpression("new_installation");
                    query.ColumnSet.AddColumns("new_installationid", "new_s_docnum");
                    //////////////////added by gil  shalem 09/01/2020 19:43
                    query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                    query.Criteria.AddCondition("new_s_docnum", ConditionOperator.Equal, currReturn.ODLN_docnum);

                    //////////////////
                    LinkEntity lk1 = new LinkEntity("new_installation", "new_car", "new_car_id", "new_carid", JoinOperator.Inner);
                    lk1.LinkCriteria.AddCondition("new_s_license_num", ConditionOperator.Equal, carRef.Id);

                    LinkEntity lk2 = new LinkEntity("new_installation", "product", "new_product_id", "productid", JoinOperator.Inner);
                    lk2.LinkCriteria.AddCondition("new_s_product_code", ConditionOperator.Equal, currReturn.ProductCode);


                    query.LinkEntities.Add(lk1);
                    query.LinkEntities.Add(lk2);


                    EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                    if (coll.Entities.Count > 0)
                    {
                        // SetStateRequest req1 = new SetStateRequest();
                        // req1.EntityMoniker = new EntityReference("new_installation", coll.Entities[0].Id);
                        // req1.State = new OptionSetValue(1);
                        // req1.Status = new OptionSetValue(2);
                        // crmBase.XrmService.Execute(req1);
                        // crmBase.XrmService.Delete("new_installation", coll.Entities[0].Id);

                    }




                }




                SetStateRequest req = new SetStateRequest();
                req.EntityMoniker = new EntityReference("new_installation", res);
                req.State = new OptionSetValue(1);
                req.Status = new OptionSetValue(2);
                crmBase.XrmService.Execute(req);

                dal.UpdateReturnDateSAP_disabled(currReturn.ID);
                return true;
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                string msg = string.Format("Error: {0}. Error in CreateReturn. ID,{1}", ex.Message, currReturn.ID);
                LogManager.Logging.WriteToLog(msg);
                if (!ex.Message.Contains("Invalid URI") && !ex.Message.Contains("unsecured"))
                    dal.UpdateReturnErrorSAP_disabled(currReturn.ID, msg);
            }
            return false;
        }
        #endregion

        #region UpdateInCrm
        private void UpdateCustomer(Customer customer, Guid customerGuid)
        {
            Entity customer_entity = new Entity("account");
            customer_entity.Id = customerGuid;

            customer_entity["name"] = customer.Name;
            if (customer.Name == null || customer.Name == "")
            {
                string msg = string.Format("Error: Customer Name is null. ID,{0}", customer.ID);
                LogManager.Logging.WriteToLog(msg);
                dal.UpdateCustomerErrorSAP(customer.ID, msg);
                return;
            }
            customer_entity["new_i_id"] = customer.ID;
            customer_entity["telephone1"] = customer.Phone1;
            customer_entity["telephone2"] = customer.Phone2;
            customer_entity["new_address"] = customer.Address;
            customer_entity["new_s_xis_customerid"] = customer.U_XIS_CustomerID;

            try
            {
                crmBase.XrmService.Update(customer_entity);
                dal.UpdateCustomerDateSAP(customer.ID);
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                string msg = string.Format("Error: {0}. Error in UpdateCustomer. ID,{1}", ex.Message, customer.ID);
                LogManager.Logging.WriteToLog(msg);
                if (!ex.Message.Contains("Invalid URI") && !ex.Message.Contains("unsecured"))
                    dal.UpdateCustomerErrorSAP(customer.ID, msg);
            }
        }


        private bool UpdateCar(Guid customerGuid, Car currCar, Guid carGuid)
        {

            Entity car_entity = new Entity();
            car_entity.LogicalName = "new_car";
            car_entity.Id = carGuid;



            //int temp;
            //if (int.TryParse(currCar.LicenseNum, out temp))
            //    car_entity["new_s_license_num"] = currCar.LicenseNum;
            //else
            //{

            //  //  string msg = string.Format("Error in UpdateCar. LicenseNum is not numeric. LicenseNum,{0},ID,{1}", currCar.LicenseNum, currCar.ID);
            //    string msg = string.Format("Car is already in CRM");
            //    LogManager.Logging.WriteToLog(msg);
            //    dal.UpdateCarErrorSAP(currCar.ID, msg);
            //    return false;
            //}

            //if (currCar.LicenseNum == null || currCar.LicenseNum == "")
            //{
            //    //string msg = string.Format("Error in UpdateCar. LicenseNum is null. ID,{0}", currCar.ID);
            //    string msg = string.Format("Car is already in the CRM");
            //    LogManager.Logging.WriteToLog(msg);
            //    dal.UpdateCarErrorSAP(currCar.ID, msg);
            //    return false;
            //}

            //EntityReference importerRef = GetImporter(currCar.ImporterCode);
            //if (importerRef == null)
            //{
            //    //string msg = string.Format("Error in UpdateCar. Could not find reference to importer name,{0},ID,{1}", currCar.ImporterCode, currCar.ID);
            //    string msg = string.Format("Car is already in the CRM");
            //    LogManager.Logging.WriteToLog(msg);
            //    dal.UpdateCarErrorSAP(currCar.ID, msg);
            //    return false;
            //}
            //car_entity["new_importer_name_id"] = importerRef;          // new_importer_name_id lookup


            //EntityReference carModelRef = GetCarModel(currCar.CarModel);
            //if (carModelRef == null)
            //{
            //    //string msg = string.Format("Error in UpdateCar. Could not find reference to car model,{0},ID,{1} ", currCar.CarModel, currCar.ID);
            //    string msg = string.Format("Car is already in the CRM");
            //    LogManager.Logging.WriteToLog(msg);
            //    dal.UpdateCarErrorSAP(currCar.ID, msg);
            //    return false;
            //}

            //car_entity["new_car_model_id"] = carModelRef;           // new_car_model_id lookup
            //car_entity["new_i_id"] = currCar.ID;
            //car_entity["new_d_doc_date"] = currCar.DocDate;
            //car_entity["new_s_chassis_num"] = currCar.ChasisNum;
            ////car_entity["new_s_license_num"] = currCar.LicenseNum;
            //car_entity["new_s_color_code"] = currCar.ColorCode;
            //car_entity["new_s_agency"] = currCar.Agency;
            //car_entity["new_s_mnf_desc"] = currCar.ManufacturerDesc;
            car_entity["new_s_mobileye_num"] = currCar.MobileeyeNum;
            //car_entity["new_s_agency_desc"] = currCar.AgencyDesc;
            if (customerGuid != Guid.Empty)
                car_entity["new_account_id"] = new EntityReference("account", customerGuid); // new_account_id lookup

            try
            {
                crmBase.XrmService.Update(car_entity);
                dal.UpdateCarDateSAP(currCar.ID);
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                string msg = string.Format("Error: {0}. Error in UpdateCar. ID,{1}", ex.Message, currCar.ID);
                LogManager.Logging.WriteToLog(msg);
                if (!ex.Message.Contains("Invalid URI") && !ex.Message.Contains("unsecured"))
                    dal.UpdateCarErrorSAP(currCar.ID, msg);
            }

            return true;
        }


        private void UpdateInstallationDocnum(Installation currInstallation, Guid installationId)
        {
            // find relevant guid in new_installation (CRM)
            Entity item = new Entity("new_installation");
            item.Id = installationId;
            item["new_s_docnum"] = currInstallation.DocNum.ToString();

            try
            {
                crmBase.XrmService.Update(item);
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                string msg = string.Format("Error: {0}. Error in UpdateInstallationDocnum. Docnum,{1},ID,{2}", ex.Message, currInstallation.DocNum, currInstallation.ST_ID);
                LogManager.Logging.WriteToLog(msg);
                if (!ex.Message.Contains("Invalid URI") && !ex.Message.Contains("unsecured"))
                    dal.UpdateInstallationErrorSAP(currInstallation.ST_ID, msg);
            }

        }

        #endregion

        #region LookupFields
        private EntityReference GetImporter(string importerCode)
        {
            EntityReference er = null;

            if (importersDict.ContainsKey(importerCode))
                er = importersDict[importerCode];

            else
            {
                QueryExpression query = new QueryExpression();
                query.EntityName = "new_importer";
                query.ColumnSet.AddColumns("new_sap_customer_id");

                try
                {
                    EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                    importersDict = new Dictionary<string, EntityReference>();

                    foreach (Entity item in coll.Entities)
                    {
                        if (item.Contains("new_sap_customer_id") && item["new_sap_customer_id"] != null && !importersDict.ContainsKey(item["new_sap_customer_id"].ToString()))
                            importersDict.Add(item["new_sap_customer_id"].ToString(), item.ToEntityReference());
                    }
                    if (importersDict.ContainsKey(importerCode))
                        er = importersDict[importerCode];
                }

                catch (Exception ex)
                {
                    CheckErrorAndCreateNewCrmBase(ex);
                    LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in GetImporter. importerCode,{1}", ex.Message, importerCode));
                }
            }
            return er;
        }

        private EntityReference GetChildProduct(string productCode, string parentProductCode)
        {
            EntityReference er = null;
            QueryExpression query = new QueryExpression();
            query.EntityName = "product";
            query.ColumnSet.AddColumns("productid");
            query.Criteria.AddCondition("new_s_product_code", ConditionOperator.Equal, productCode);

            if (!string.IsNullOrWhiteSpace(parentProductCode))
            {
                LinkEntity lk = new LinkEntity("product", "product", "new_parent_item_id", "productid", JoinOperator.Inner);
                lk.LinkCriteria.AddCondition("new_s_product_code", ConditionOperator.Equal, parentProductCode);

                query.LinkEntities.Add(lk);
            }
            else
                query.Criteria.AddCondition("new_parent_item_id", ConditionOperator.Null);

            try
            {
                EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                if (coll.Entities.Count > 0)
                    er = coll.Entities[0].ToEntityReference();

            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in GetChildProduct. productCode,{1},parentProductCode,{2}", ex.Message, productCode, parentProductCode));
            }

            return er;
        }

        private EntityReference GetProductIfLeading(string productCode)
        {
            if (productCode == "52437")
            {

            }
            EntityReference er = null;
            QueryExpression query = new QueryExpression();
            query.EntityName = "product";
            query.ColumnSet.AddColumns("productid");
            query.Criteria.AddCondition("new_s_product_code", ConditionOperator.Equal, productCode);
            query.Criteria.AddCondition("new_parent_item_id", ConditionOperator.Null);
            query.Criteria.AddCondition("new_b_leading_product", ConditionOperator.Equal, true);

            try
            {
                EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                if (coll.Entities.Count > 0)
                    er = coll.Entities[0].ToEntityReference();

            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in GetProductIfLeading. productCode,{1}", ex.Message, productCode));
            }

            return er;
        }

        private EntityReference GetParentProduct(Guid childProductId)
        {
            EntityReference er = null;

            try
            {
                Entity ent = crmBase.XrmService.Retrieve("product", childProductId, new ColumnSet("new_parent_item_id"));
                if (ent != null && ent.Contains("new_parent_item_id") && ent["new_parent_item_id"] != null)
                    er = ent.ToEntityReference();
            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in GetParentProduct. childProductId,{1}", ex.Message, childProductId));
            }


            return er;
        }

        private EntityReference GetCarModel(string carModel)
        {
            EntityReference er = null;

            if (carModelsDict.ContainsKey(carModel.ToLower()))
                er = carModelsDict[carModel.ToLower()];

            else
            {
                QueryExpression query = new QueryExpression();
                query.EntityName = "new_car_model";
                query.ColumnSet.AddColumns("new_s_code");

                try
                {
                    EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                    carModelsDict = new Dictionary<string, EntityReference>();

                    foreach (Entity item in coll.Entities)
                    {
                        if (item.Contains("new_s_code") && item["new_s_code"] != null && !carModelsDict.ContainsKey(item["new_s_code"].ToString().ToLower()))
                            carModelsDict.Add(item["new_s_code"].ToString().ToLower(), item.ToEntityReference());
                    }




                    if (carModelsDict.ContainsKey(carModel.ToLower()))
                        er = carModelsDict[carModel.ToLower()];
                }

                catch (Exception ex)
                {
                    CheckErrorAndCreateNewCrmBase(ex);
                    LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in GetCarModel. carModel,{1}", ex.Message, carModel));
                }
            }
            return er;
        }

        private EntityReference GetCar(string rishuiNum)
        {
            EntityReference er = null;
            QueryExpression query = new QueryExpression("new_car");
            query.Criteria.AddCondition("new_s_license_num", ConditionOperator.Equal, rishuiNum);

            try
            {
                EntityCollection coll = crmBase.XrmService.RetrieveMultiple(query);
                if (coll.Entities.Count > 0)
                    er = coll.Entities[0].ToEntityReference();

            }
            catch (Exception ex)
            {
                CheckErrorAndCreateNewCrmBase(ex);
                LogManager.Logging.WriteToLog(string.Format("Error: {0}. Error in GetCar. LicenseNum,{1}", ex.Message, rishuiNum));
            }

            return er;
        }
        #endregion



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




            if (LogManager.Logging.fileName != null)
            {
                LogManager.Logging.WriteToLog("CustomersCarsInstallations send mail test 5 : " + DateTime.Now);
                wod_EmailEntity["subject"] = string.Format("Customers/Cars/Installations interface error log from {0} to {1}", System.Configuration.ConfigurationManager.AppSettings["from"], System.Configuration.ConfigurationManager.AppSettings["to"]);
                LogManager.Logging.WriteToLog("CustomersCarsInstallations send mail test 6 : " + DateTime.Now);
                wod_EmailEntity["description"] = string.Format("Attached herewith error log for {0}", DateTime.Now);
                LogManager.Logging.WriteToLog("CustomersCarsInstallations send mail test 7 : " + DateTime.Now);
                wod_EmailId = crmBase.XrmService.Create(wod_EmailEntity);

                AttachFileToEmail(wod_EmailId);


            }

            else
            {
                LogManager.Logging.WriteToLog("CustomersCarsInstallations send mail test 10 : " + DateTime.Now);
                wod_EmailEntity["subject"] = string.Format("Customers/Cars/Installations interface error log from {0} to {1}", System.Configuration.ConfigurationManager.AppSettings["from"], System.Configuration.ConfigurationManager.AppSettings["to"]);
                LogManager.Logging.WriteToLog("CustomersCarsInstallations send mail test 11 : " + DateTime.Now);
                wod_EmailEntity["description"] = string.Format("No errors were detected");
                LogManager.Logging.WriteToLog("CustomersCarsInstallations send mail test 12 : " + DateTime.Now);
                wod_EmailId = crmBase.XrmService.Create(wod_EmailEntity);
                LogManager.Logging.WriteToLog("CustomersCarsInstallations send mail test 13 : " + DateTime.Now);
            }
            LogManager.Logging.WriteToLog("CustomersCarsInstallations send mail test 14 : " + DateTime.Now);
            // Creating SendEmailRequest object for sending email
            SendEmailRequest wod_SendEmailRequest = new SendEmailRequest();
            LogManager.Logging.WriteToLog("CustomersCarsInstallations send mail test 15 : " + DateTime.Now);
            // Creating Email tracking token request object
            //GetTrackingTokenEmailRequest wod_GetTrackingTokenEmailRequest = new GetTrackingTokenEmailRequest();

            // Creating Email tracking token response object to get tracking token value
            //GetTrackingTokenEmailResponse wod_GetTrackingTokenEmailResponse = null;

            // Setting email record if for sending email
            wod_SendEmailRequest.EmailId = wod_EmailId;
            LogManager.Logging.WriteToLog("CustomersCarsInstallations send mail test 16 : " + DateTime.Now);
            wod_SendEmailRequest.IssueSend = true;
            LogManager.Logging.WriteToLog("CustomersCarsInstallations send mail test 17 : " + DateTime.Now);
            // Getting tracking token value
            //wod_GetTrackingTokenEmailResponse = (GetTrackingTokenEmailResponse)crmBase.XrmService.Execute(wod_GetTrackingTokenEmailRequest);

            // Setting tracking token value
            //wod_SendEmailRequest.TrackingToken = wod_GetTrackingTokenEmailResponse.TrackingToken;
            wod_SendEmailRequest.TrackingToken = "";
            LogManager.Logging.WriteToLog("CustomersCarsInstallations send mail test 18 : " + DateTime.Now);
            // Sending email
            crmBase.XrmService.Execute(wod_SendEmailRequest);
            LogManager.Logging.WriteToLog("CustomersCarsInstallations send mail test 19 : " + DateTime.Now);
        }

        private void AttachFileToEmail(Guid wod_EmailId)
        {
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
            if (ex.Message.Contains("unsecured or incorrectly secured fault"))
            {
                System.Threading.Thread.Sleep(500);
                crmBase = new CrmBase();
            }
            if (ex.Message.Contains("Invalid URI") || ex.Message.Contains("unsecured"))
            {
                System.Threading.Thread.Sleep(1800000);
                crmBase = new CrmBase();
            }
        }
    }

}
