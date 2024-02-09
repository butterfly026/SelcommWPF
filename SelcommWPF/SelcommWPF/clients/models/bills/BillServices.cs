using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.bills
{
    class BillServices
    {

        public int Count { get; set; }

        public List<Item> ServiceSummaries { get; set; }


        public class Item
        {
            public string ServiceTypeCode { get; set; }

            public string ServiceType { get; set; }

            public long? ServiceReference { get; set; }

            public string ServiceId { get; set; }

            public double ChargeAmount { get; set; }

            public string ChargeAmountText { get; set; }

            public double ChargeAmountInc { get; set; }

            public string ChargeAmountIncText { get; set; }

            public double UsageAmount { get; set; }

            public string UsageAmountText { get; set; }

            public double UsageAmountInc { get; set; }

            public string UsageAmountIncText { get; set; }
        }

    }
}
