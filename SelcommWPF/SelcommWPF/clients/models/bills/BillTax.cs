using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.bills
{
    class BillTax
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int DisplayOrder { get; set; }

        public bool Default { get; set; }

        public bool Display { get; set; }

        public double NewChargeMinimum { get; set; }

        public double NewChargeMaximum { get; set; }

        public double BalanceMinimum { get; set; }

        public double BalanceMaximum { get; set; }

        public int DefaultTaxId { get; set; }

        public string DefaultTax { get; set; }

        public int TaxId { get; set; }

        public string Tax { get; set; }

        public string TaxCode { get; set; }

        private double Percentage { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string CreatedBy { get; set; }

        public string Created { get; set; }

        public string LastUpdated { get; set; }

        public string UpdatedBy { get; set; }
    }
}
