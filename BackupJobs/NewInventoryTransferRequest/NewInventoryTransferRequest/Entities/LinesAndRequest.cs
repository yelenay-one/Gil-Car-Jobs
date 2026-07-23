using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewInventoryTransferRequest
{
  public  class LinesAndRequest
    {
        public List<NewInventoryTransferlines> NewInventoryTransferlinesList { get; set; }
        public NewInventoryTransferRequest NewInventoryTransferRequestEntity { get; set; }
    }
}
