using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntoIT.GilCar.Interfaces.ProductsToCRM
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
                bl.SendEmail();
                //bl.CreateNewInterfaceLog(start,"test",0,0,0);
            }
            catch (Exception ex) { LogManager.Logging.WriteToLog(ex.Message + Environment.NewLine + ex.StackTrace); }
            LogManager.Logging.WriteToLog("ProductsToCRM Start: " + start);
            LogManager.Logging.WriteToLog("ProductsToCRM End: " + DateTime.Now);
           

        }
    }
}
