using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarModelFromSapToCrm
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DateTime start = DateTime.Now;
                LogManager.Logging.WriteToLog("Start: " + DateTime.Now.ToString());
                BL bl = new BL();
                bl.StartInterface(start);
               // bl.CreateNewInterfaceLog(start, "CarModelFromSapToCrm");
                LogManager.Logging.WriteToLog("End: " + DateTime.Now.ToString());
            }
            catch(Exception ex)
            {
                LogManager.Logging.WriteToLog(ex.Message + " " + ex.StackTrace);
            }

         
        }
    }
}
