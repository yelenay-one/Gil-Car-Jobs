using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using IntoIT.GilCar.Interfaces.CustomersCarInstallations;
using Microsoft.Crm.Sdk.Messages;

namespace IntoIT.GilCar.Interfaces.CustomersCarsInstallations
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime start = DateTime.Now;
            try
            {



              
            BL bl = new BL();
                bl.StartInterface(start);
               // bl.CreateNewInterfaceLog(start, "CustomersCarsInstallations");
               // LogManager.Logging_General.WriteToLog(string.Format("Ended: ID {0} to {1}", System.Configuration.ConfigurationManager.AppSettings["from"], System.Configuration.ConfigurationManager.AppSettings["to"]));
                bl.SendEmail();

                Console.SetCursorPosition(0, 20);            
                Console.WriteLine("Press any key to continue...");
            //    Console.ReadKey();
            }
            catch (Exception ex) { LogManager.Logging.WriteToLog(ex.Message + Environment.NewLine + ex.StackTrace); }
            LogManager.Logging.WriteToLog("CustomersCarsInstallations Start: " + start);
            LogManager.Logging.WriteToLog("CustomersCarsInstallations End: " + DateTime.Now);

            //catch (Exception ex)
            //{
            //   /// LogManager.Logging_General.WriteToLog(string.Format("Closed/Error: ID {0} to {1}", System.Configuration.ConfigurationManager.AppSettings["from"], System.Configuration.ConfigurationManager.AppSettings["to"]));
            //    throw ex;
            //}
        }
    }
}
