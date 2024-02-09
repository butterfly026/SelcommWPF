using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.bills
{
    class TaxExemption
    {
        public int Id { get; set; }

        public int TaxAppliedId { get; set; }

        public string TransactionTypeId { get; set; }

        public string TransactionType { get; set; }

        public int TaxId { get; set; }

        public string Tax { get; set; }

        public string TaxCode { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public bool NewAllowed { get; set; }

        public bool UpdateAllowed { get; set; }

        public bool DeleteAllowed { get; set; }

        public string EarliestFrom { get; set; }

        public string EarliestTo { get; set; }

        public string LastUpdated { get; set; }

        public string UpdatedBy { get; set; }
    }
}
