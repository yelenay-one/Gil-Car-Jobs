using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntoIT.GilCar.Interfaces.CustomersCarInstallations
{
    public class Return
    {
        public int ID { get; set; }
        public int DocNum { get; set; }
        public string ProductCode { get; set; }
        public DateTime? DocDate { get; set; }
        public int Quantity { get; set; }
        public string WharehouseCode { get; set; }
        public string SerialNum { get; set; }
        public string TelNum { get; set; }
        public string IMEINum { get; set; }
        public string MACNum { get; set; }
        public string LicenseNum { get; set; }  // for returns only
        public string ODLN_cardname { get; set; }
        public string ODLN_cardcode { get; set; }
        ///////////////////////////////////updated by gil 09/01/2020 19:34
        public DateTime? ODLN_docdate { get; set; }
        public int? ODLN_docnum { get; set; }
      
        public string ST_ID { get; set; }
        
      
        //public string ParentProductCode { get; set; }  // להשאיר??
        public string ParentProductCode { get; set; }
    
        ///////////////////////////////////
    }
}
