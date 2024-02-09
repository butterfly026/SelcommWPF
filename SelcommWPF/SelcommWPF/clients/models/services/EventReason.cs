using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class EventReason
    {
        public int Id { get; set; }

        public string CreatedBy { get; set; }

        public ReasonModel Reason { get; set; }

        public string Created { get; set; }

        public string LastUpdated { get; set; }
        
        public string UpdatedBy { get; set; }

        public class ReasonModel
        {
            public string Id { get; set; }

            public string Reason { get; set; }

            public Boolean Enabled { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }
        }
    }
}
