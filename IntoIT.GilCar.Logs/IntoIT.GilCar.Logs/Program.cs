using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntoIT.GilCar.Logs
{
    class Program
    {
        static void Main(string[] args)
        {
            LogManager.Logging.WriteToLog("Start "+DateTime.Now);
            BL bl = new BL();
            bl.StartInterface(DateTime.Now);
            LogManager.Logging.WriteToLog("End " + DateTime.Now);
        }
    }
}
