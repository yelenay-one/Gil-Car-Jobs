using System;

namespace IntoIT.GilCar.IncidentItemsToSAP
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
               // bl.SendEmail();
               // bl.CreateNewInterfaceLog(start, "IncidentItemsToSAP");
            }
            catch (Exception ex) { LogManager.Logging.WriteToLog(ex.Message + Environment.NewLine + ex.StackTrace); }
            LogManager.Logging.WriteToLog("IncidentItemsToSAP Start: " + start);
            LogManager.Logging.WriteToLog("IncidentItemsToSAP End: " + DateTime.Now);
        }
    }
}
