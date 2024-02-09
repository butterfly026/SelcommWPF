using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.bills
{
    class BillTerms
    {
        public int CreditLimit { get; set; }

        public List<History> TermsProfilesHistory { get; set; }

        public class History
        {
            public object Id { get; set; }

            public double? CreditLimit { get; set; }

            public string CreditLimitText { get; set; }

            public string TermProfileId { get; set; }

            public string Name { get; set; }

            public string Start { get; set; }

            public string From { get; set; }

            public string To { get; set; }

            public string Type { get; set; }

            public string TermUnit { get; set; }

            public int Term { get; set; }

            public double DepositInterest { get; set; }

            public double OverdueInterest { get; set; }

            public double LatePaymentFee { get; set; }

            public bool AllowWeekendDueDate { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }

            public bool? Default { get; set; }

            public int? DisplayOrder { get; set; }

            public bool? Enabled { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }
        }

        public class Item
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string Type { get; set; }

            public string TermUnit { get; set; }

            public int Term { get; set; }

            public double DepositInterest { get; set; }

            public double OverdueInterest { get; set; }

            public double LatePaymentFee { get; set; }

            public bool AllowWeekendDueDate { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }

            public bool Default { get; set; }

            public int DisplayOrder { get; set; }

            public bool Enabled { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }
        }

    }
}
