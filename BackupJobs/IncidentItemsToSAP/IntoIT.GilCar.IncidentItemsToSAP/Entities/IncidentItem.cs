using System;

namespace IntoIT.GilCar.IncidentItemsToSAP.Entities
{
    public class IncidentItem
    {
        public Guid IncidentItemGUID { get; set; }
        public string IncidentCode { get; set; }
        public string LicenseNum { get; set; }
        public string CardCode { get; set; }
        public string WhsCode { get; set; }
        public string ProductCode { get; set; }
        public DateTime Createdon { set; get; }
        public DateTime DocDate { set; get; }
        public string ItemSN { get; set; }
        public int Quantity { get; set; }
        public string Shilda { get; set; }
        public string Owner { get; set; }
        public string Model { get; set; }
        public string ModelName { get; set; }
        public string OrderNumber { get; set; }
        public int CaseTypeCode { get; set; }
        public bool CustomerDamage { get; set; }
        public int CustomerType { get; set; }
        public int InstallationLocation { get; set; }
        public int StatusCode { get; set; }
        public int new_solution_code { get; set; }
        public string description { get; set; }
        public string ServiceStation { get; set; }
        public string ItemServiceStation { get; set; }
        public DateTime SapTime { get; set; }
        public bool new_inventory_transfer_via_interface { get; set; }
        public string NewCharge { get; set; }
        public string new_p_installations_classification { get; set; }
        public string new_s_technician_card_code { get; set; }
        public decimal price { get; set; }
        public decimal totalprice { get; set; }
    }
}
