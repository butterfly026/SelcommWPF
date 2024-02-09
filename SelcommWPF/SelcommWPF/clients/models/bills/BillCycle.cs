using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.bills
{
    class BillCycle
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool Default { get; set; }

        public int DisplayOrder { get; set; }

        public int DayOfMonth { get; set; }

        public bool Display { get; set; }

        public string IntervalUnits { get; set; }

        public int Interval { get; set; }

        public string Source { get; set; }

        public string ConfigurationId { get; set; }

        public string CycleId { get; set; }

        public string Cycle { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string CreatedBy { get; set; }

        public string Created { get; set; }

        public string LastUpdated { get; set; }

        public string UpdatedBy { get; set; }
    }
}
