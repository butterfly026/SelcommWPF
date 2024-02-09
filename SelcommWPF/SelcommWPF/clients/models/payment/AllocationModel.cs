using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.payment
{
    class AllocationModel
    {
        public long Id { get; set; }

        public string Created { get; set; }

        public string CreatedBy { get; set; }

        public bool Credit { get; set; }

        public string Bill { get; set; }

        public string TypeCode { get; set; }

        public string OtherReference { get; set; }

        public double TaxAmount { get; set; }

        public string TaxAmountText { get; set; }

        public double AllocateAmount { get; set; }

        public string AllocateAmountText { get; set; }

        public double Amount { get; set; }

        public string AmountText { get; set; }

        public string Category { get; set; }

        public string Source { get; set; }

        public string Status { get; set; }

        public string Type { get; set; }

        public double OpenAmount { get; set; }

        public string OpenAmountText { get; set; }

        public string Date { get; set; }

        public string Number { get; set; }

        public string LastUpdated { get; set; }

        public string UpdatedBy { get; set; }

    }
}
