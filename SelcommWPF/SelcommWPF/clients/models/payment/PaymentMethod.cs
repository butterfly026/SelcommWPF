using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class PaymentMethod
    {
        public string AccountName { get; set; }

        public string AccountNumber { get; set; }

        public string BSB { get; set; }

        public string Category { get; set; }

        public string CategoryCode { get; set; }

        public string Created { get; set; }

        public string CreatedBy { get; set; }

        public bool CustomerOwned { get; set; }

        public bool Default { get; set; }

        public bool Enabled { get; set; }

        public string DefaultText { get; set; }

        public List<History> DefaultUsageHistory { get; set; }

        public string Description { get; set; }

        public string ExpiryDate { get; set; }

        public bool Exportable { get; set; }

        public bool Exported { get; set; }

        public object Id { get; set; }

        public string LastUpdated { get; set; }

        public string LastUsed { get; set; }

        public bool Masked { get; set; }

        public bool OnlineEnabled { get; set; }

        public string PaymentMethodCode { get; set; }

        public bool Protectable { get; set; }

        public bool Protected { get; set; }

        public string SecondaryToken { get; set; }

        public List<string> ServiceUsageHistory { get; set; }

        public string Status { get; set; }

        public string StatusCode { get; set; }

        public List<History> StatusHistory { get; set; }

        public string Token { get; set; }

        public string UpdatedBy { get; set; }

        public bool Used { get; set; }

        public string CreditCard { get; set; }

        public class History
        {
            public long Id { get; set; }

            public string Created { get; set; }

            public string CreatedBy { get; set; }

            public string From { get; set; }

            public string Note { get; set; }

            public string Status { get; set; }

            public string StatusCode { get; set; }

            public string To { get; set; }
        }

    }
}
