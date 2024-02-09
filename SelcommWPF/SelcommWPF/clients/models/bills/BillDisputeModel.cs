using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.bills
{
    class BillDisputeModel
    {

        public int Count { get; set; }

        public List<Item> BillDisputes { get; set; }

        public class Item
        {
            public long Id { get; set; }

            public string Created { get; set; }

            public string CreatedBy { get; set; }

            public double? SettlementTax { get; set; }

            public string SettlementTaxText { get; set; }

            public double? SettlementAmount { get; set; }

            public string SettlementAmountText { get; set; }

            public string ContactDetails { get; set; }

            public string RaisedBy { get; set; }

            public string ApprovalNotes { get; set; }

            public string Updated { get; set; }

            public string ApprovedBy { get; set; }

            public string BillNumber { get; set; }

            public long BillId { get; set; }

            public double? DisputedAmount { get; set; }

            public string DisputedAmountText { get; set; }

            public string Details { get; set; }

            public string StatusLastUpdated { get; set; }

            public string Status { get; set; }

            public string Date { get; set; }

            public string BillDate { get; set; }

            public string UpdatedBy { get; set; }
        }

    }
}
