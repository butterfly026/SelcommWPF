using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.costcenter
{
    class CostCenterModel
    {

        public int Count { get; set; }

        public List<CostCenter> Items { get; set; }

        public class CostCenter
        {
            public int Id { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

            public string StatusUpdated { get; set; }

            public string EFXId { get; set; }
            
            public string Email { get; set; }
            
            public string Status { get; set; }
            
            public string AllocationType { get; set; }

            public bool AggregationPoint { get; set; }

            public string GeneralLedgerAccountCode { get; set; }

            public string AdditionalInformation3 { get; set; }

            public string AdditionalInformation2 { get; set; }

            public string AdditionalInformation1 { get; set; }
            
            public string Name { get; set; }
            
            public string CustomerReference { get; set; }

            public int? ParentId { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }

        }

        public class CostCenterListItem
        {
            public int Id { get; set; }
            
            public string Name { get; set; }
            
            public string Status { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

        }
    }
}
