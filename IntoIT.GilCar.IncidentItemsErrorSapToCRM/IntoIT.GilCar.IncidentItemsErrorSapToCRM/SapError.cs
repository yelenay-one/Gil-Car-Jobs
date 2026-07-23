using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntoIT.GilCar.IncidentItemsErrorSapToCRM
{
   public class SapError
    {
        public string ErrorLog { get; set; }
        public DateTime SAPFailedDate { get; set; }
        public Guid guid { get; set; }
    }
}
