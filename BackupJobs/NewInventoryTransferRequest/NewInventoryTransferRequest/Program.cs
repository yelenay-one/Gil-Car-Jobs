using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewInventoryTransferRequest
{
    class Program
    {
        static void Main(string[] args)
        {
            
            DateTime start = DateTime.Now;
            LogManager.Logging.WriteToLog("NewInventoryTransferRequest Start: " + start);
            try
            {



                BL bl = new BL();
                bl.StartInterface(start);
              

            }
            catch (Exception ex) { LogManager.Logging.WriteToLog(ex.Message + Environment.NewLine + ex.StackTrace); }
          
            LogManager.Logging.WriteToLog("NewInventoryTransferRequest End: " + DateTime.Now);
        }
    }
}
