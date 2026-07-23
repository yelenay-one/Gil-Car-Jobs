using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntoIT.GilCar.Interfaces.ProductsToCRM
{
    public class ProductParent
    {
        public string ParentProductName { get; set; }
        public string SonProductName { get; set; }
        public string ParentProductCode { get; set; }
        public string SonProductCode { get; set; }
        public bool IsSonLeadingItem { get; set; }
    }
}
