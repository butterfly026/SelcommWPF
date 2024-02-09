using System.Collections.Generic;

namespace SelcommWPF.clients.models
{
    class BillHistoryModel
    {
        public int Count { get; set; }

        public List<BillDetail> Bills { get; set; }

        public class BillDetail
        {

            public long Id { get; set; }

            public int Sequence { get; set; }

            public string BillPeriod { get; set; }

            public string BillCycle { get; set; }

            public string Source { get; set; }

            public string BillNumber { get; set; }

            public string BillDate { get; set; }

            public string DueDate { get; set; }

            public double AmountDue { get; set; }

            public string AmountDueText { get; set; }

            public double PreviousBalance { get; set; }

            public string PreviousBalanceText { get; set; }

            public double PaymentAdjustmentAmount { get; set; }

            public string PaymentAdjustmentAmountText { get; set; }

            public double NewCharges { get; set; }

            public string NewChargesText { get; set; }

            public double InstallmentAmount { get; set; }

            public string InstallmentAmountText { get; set; }

            public double DepositAmount { get; set; }

            public string DepositAmountText { get; set; }

            public double RepaymentPlanAmount { get; set; }

            public string RepaymentPlanAmountText { get; set; }

            public double DisputedAmount { get; set; }

            public string DisputedAmountText { get; set; }

            public double Balance { get; set; }

            public string BalanceText { get; set; }

            public string CurrencyCode { get; set; }

            public string Currency { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }

            public List<FinancialDocs> FinancialDocuments { get; set; }

            public string Visiblity { get; set; }

            public MaterialDesignThemes.Wpf.PackIcon ButtonIcon { get; set; }

            public class FinancialDocs
            {

                public long Id { get; set; }

                public string Type { get; set; }

                public string Number { get; set; }

                public string Date { get; set; }

                public string DueDate { get; set; }

                public double Amount { get; set; }

                public string AmountText { get; set; }

                public double TaxAmount { get; set; }

                public string TaxAmountText { get; set; }

                public string Source { get; set; }

                public long? ParentId { get; set; }

                public string CreatedBy { get; set; }

                public string Created { get; set; }

                public string LastUpdated { get; set; }

                public string UpdatedBy { get; set; }
            }

        }
        
    }
}
