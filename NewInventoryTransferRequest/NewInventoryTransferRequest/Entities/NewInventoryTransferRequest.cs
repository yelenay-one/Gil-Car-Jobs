using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewInventoryTransferRequest
{
   public class NewInventoryTransferRequest
    {
        public string new_request_num { get; set; }
        public string ownerid { get; set; }
        public string new_warehouse_num { get; set; }
        public DateTime createdon { get; set; }
        public Guid new_inventory_transfer_request_id { get; set; }


    }
}
