using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class Financials
    {
        public int Count { get; set; }

        public List<TransactionDetail> FinancialTransactions { get; set; }

        public class TransactionDetail
        {
            public double Amount { get; set; }

            public string AmountText { get; set; }

            public string BillNumber { get; set; }

            public string DocumentId { get; set; }

            public string InternalDocumentId { get; set; }

            public string Date { get; set; }

            public int Id { get; set; }

            public double OpenAmount { get; set; }

            public string OpenAmountText { get; set; }

            public double RunningBalance { get; set; }

            public string RunningBalanceText { get; set; }

            public string Type { get; set; }

            public string Status { get; set; }

        }

    }
}
