using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloseCases
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime start = DateTime.Now;
            LogManager.Logging.WriteToLog(string.Format("")); 
            BL bl = new BL();
            bl.StartInterface(start);
            try
            {
                bl.SendEmail();
            }
            catch (Exception ex)
            {
                LogManager.Logging.WriteToLog(ex.Message); 
            }
            
          //  LogManager.Logging.WriteToLog(string.Format("end :" + DateTime.Now)); 
        }
    }
}
