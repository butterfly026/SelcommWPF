using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.bills
{
    class BillExclusions
    {
        public long? Id { get; set; }

        public long? AccountLastPeriodBilledNormally { get; set; }

        public string AccountLastCycleBilledNormally { get; set; }

        public long? AccountLastPeriodBilled { get; set; }

        public string AccountLastCycleBilled { get; set; }

        public long? LastPeriodBilled { get; set; }

        public long? CurrentPeriod { get; set; }

        public List<History> BillRunExclusionHistory { get; set; }

        public class History
        {
            public long Id { get; set; }

            public long StartPeriod { get; set; }

            public long EndPeriod { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }
        }

    }
}
