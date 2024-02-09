using System.Collections.Generic;

namespace SelcommWPF.clients.models
{
    class ServicesResponse
    {
        public int Count { get; set; }

        public List<ServicesModel> Services { get; set; }

        public class ServicesModel
        {
            public long ServiceReference { get; set; }

            public string ServiceId { get; set; }

            public string ServiceType { get; set; }

            public string StatusCode { get; set; }

            public string Status { get; set; }

            public string Connected { get; set; }

            public string Disconnected { get; set; }

            public int PlanId { get; set; }

            public string Plan { get; set; }

            public int PlanOptionId { get; set; }

            public string PlanOption { get; set; }

            public string UserLabel { get; set; }

            public long? ContractReference { get; set; }

            public string ContractStart { get; set; }

            public string ContractEnd { get; set; }
        }

    }
}
