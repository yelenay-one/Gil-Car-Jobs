using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntoIT.GilCar.Interfaces.ProductsToCRM
{
    public class Product
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        //public string ParentProductNum { get; set; }
        //public string ParentProductName { get; set; }
        public string ProductFamily { get; set; }
        public bool IsLeadingItem { get; set; }
        public bool ValidFor { get; set; }
        public bool SerialManaged { get; set; }
    }
}
