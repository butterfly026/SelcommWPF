using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.bills
{
    class BillReturns
    {
        public object Id { get; set; }

        public string ReasonId { get; set; }

        public string Reason { get; set; }

        public long? AddressId { get; set; }

        public bool InvalidAddress { get; set; }

        public string Action { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string CreatedBy { get; set; }

        public string Created { get; set; }

        public string LastUpdated { get; set; }

        public string UpdatedBy { get; set; }
    }
}
