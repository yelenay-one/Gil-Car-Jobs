using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntoIT.GilCar.Interfaces.CustomersCarInstallations
{
    public class Car
    {
        //public Guid CustAccountID { get; set; }
        public int ID { get; set; }
        public int DocNum { get; set; }
        //public string ImporterName_1 { get; set; }
        //public string ImporterName_2 { get; set; }
        public string ImporterCode { get; set; }
        public DateTime? DocDate { get; set; }
        public string Numatcard { get; set; }
        public string ChasisNum { get; set; }
        public string LicenseNum { get; set; }
        public string ColorCode { get; set; }
        //public string _oldModel { get; set; }
        //public string _newModel { get; set; }
        public string CarModel { get; set; }
        public string Agency { get; set; }
        public string ManufacturerDesc { get; set; }
        //public string Cellular { get; set; }
        public string MobileeyeNum { get; set; }
        //public string SIMNum { get; set; }
        //public string IMEINum { get; set; }
        //public string FLYNum { get; set; }
        public string AgencyDesc { get; set; }
    }
}
