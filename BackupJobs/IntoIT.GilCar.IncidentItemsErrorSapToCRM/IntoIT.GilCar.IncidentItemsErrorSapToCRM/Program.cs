using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntoIT.GilCar.IncidentItemsErrorSapToCRM
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
               // bl.CreateNewInterfaceLog(start, "IncidentItemsErrorSapToCRM");
            }
            catch (Exception ex) { LogManager.Logging.WriteToLog(ex.Message + Environment.NewLine + ex.StackTrace); }
            LogManager.Logging.WriteToLog("IncidentItemsErrorSapToCRM Start: " + start);
            LogManager.Logging.WriteToLog("IncidentItemsErrorSapToCRM  End: " + DateTime.Now);
        }
    }
}
