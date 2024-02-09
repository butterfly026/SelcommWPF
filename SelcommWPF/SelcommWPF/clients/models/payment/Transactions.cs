using SelcommWPF.clients.models.bills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class Transactions
    {
        public int Count { get; set; }

        public List<TransactionDetail> FinancialTransactions { get; set; }

        public class TransactionDetail
        {
            public double Amount { get; set; }

            public string AmountText { get; set; }

            public string BillNumber { get; set; }

            public long? BillReference { get; set; }

            public string BusinessUnitCode { get; set; }

            public string Category { get; set; }

            public string CategoryCode { get; set; }

            public string Created { get; set; }

            public string CreatedBy { get; set; }

            public string Date { get; set; }

            public string DueDate { get; set; }

            public int Id { get; set; }

            public string LastUpdated { get; set; }

            public string Number { get; set; }

            public double OpenAmount { get; set; }

            public string OpenAmountText { get; set; }

            public string OtherReference { get; set; }

            public long? ParentId { get; set; }

            public string Reason { get; set; }

            public string ReasonCode { get; set; }

            public double RoundingAmount { get; set; }

            public string RoundingAmountText { get; set; }

            public double RoundingTaxAmount { get; set; }

            public string RoundingTaxAmountText { get; set; }

            public string ServiceId { get; set; }

            public string ServiceReference { get; set; }

            public string Source { get; set; }

            public string SourceCode { get; set; }

            public string Status { get; set; }

            public string StatusCode { get; set; }

            public double TaxAmount { get; set; }

            public string TaxAmountText { get; set; }

            public string Type { get; set; }

            public string TypeCode { get; set; }

            public string UpdatedBy { get; set; }

        }
        
        public class Item
        {
            public long Id { get; set; }

            public string ReasonCode { get; set; }

            public string Reason { get; set; }

            public string OtherReference { get; set; }

            public long? ParentId { get; set; }

            public long? ServiceReference { get; set; }

            public string ServiceId { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }

            public List<ItemDetail> Details { get; set; }

            public List<Allocation> Allocations { get; set; }

            public List<Distribution> Distributions { get; set; }

            public List<Split> Splits { get; set; }

            public List<Event> Events { get; set; }

            public List<PayRequest> PayRequests { get; set; }

            public List<External> ExternalTransactions { get; set; }

            public string Category { get; set; }

            public string CategoryCode { get; set; }

            public string Source { get; set; }

            public string SourceCode { get; set; }

            public string Type { get; set; }

            public string BusinessUnitCode { get; set; }

            public string StatusCode { get; set; }

            public string Status { get; set; }

            public string ContactCode { get; set; }

            public string ContactName { get; set; }

            public string Number { get; set; }

            public string Date { get; set; }

            public BillServices ServiceSummary { get; set; }

            public string DueDate { get; set; }

            public double TaxAmount { get; set; }

            public double OpenAmount { get; set; }

            public double RoundingAmount { get; set; }

            public double RoundingTaxAmount { get; set; }

            public long? BillId { get; set; }

            public string BillNumber { get; set; }

            public string Note { get; set; }

            public string TypeCode { get; set; }

            public double Amount { get; set; }

            public List<Charge> Charges { get; set; }

        }

        public class ItemDetail
        {
            public int Sequence { get; set; }

            public string Detail { get; set; }
        }

        public class Allocation
        {
            public long Id { get; set; }

            public string Created { get; set; }

            public string CreatedBy { get; set; }

            public string BillNumber { get; set; }

            public string TypeCode { get; set; }

            public string OtherReference { get; set; }

            public double TaxAmount { get; set; }

            public double Amount { get; set; }

            public string LastUpdated { get; set; }

            public string Category { get; set; }

            public string Status { get; set; }

            public string Type { get; set; }

            public double AllocatedAmount { get; set; }

            public double OpenAmount { get; set; }

            public string OpenAmountText { get; set; }

            public string Date { get; set; }

            public string Number { get; set; }

            public string AssignmentDirection { get; set; }

            public string Source { get; set; }

            public string UpdatedBy { get; set; }
        }

        public class Distribution
        {
            public long Id { get; set; }

            public string ServiceId { get; set; }

            public long? ServiceReference { get; set; }

            public string ServiceTypeCode { get; set; }

            public double AdjustedDailyAmount { get; set; }

            public string AdjustedDailyAmountText { get; set; }

            public bool Control { get; set; }

            public string Comment { get; set; }

            public string Sign { get; set; }

            public string To { get; set; }

            public string From { get; set; }

            public double AdjustedAmount { get; set; }

            public string AdjustedAmountText { get; set; }

            public double TaxAmount { get; set; }

            public string TaxAmountText { get; set; }

            public double Amount { get; set; }

            public string AmountText { get; set; }

            public string Period { get; set; }

            public string AccountName { get; set; }

            public string AccountCode { get; set; }

            public string Created { get; set; }

            public string CreatedBy { get; set; }
        }

        public class Split
        {
            public long Id { get; set; }

            public string SplitDirection { get; set; }

            public long? OtherTransactionId { get; set; }

            public string Number { get; set; }

            public string Type { get; set; }

            public string Date { get; set; }

            public double Amount { get; set; }

            public string AmountText { get; set; }

            public long? RefundId { get; set; }

            public string RefundNumber { get; set; }

            public string RefundDate { get; set; }

            public double RefundAmount { get; set; }

            public string RefundAmountText { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }
        }

        public class Event
        {
            public long Id { get; set; }

            public string Description { get; set; }

            public string Created { get; set; }

            public string CreatedBy { get; set; }
        }

        public class PayRequest
        {
            public long Id { get; set; }

            public long? OriginalRequestId { get; set; }

            public string MerchantNumber { get; set; }

            public string RequestingUser { get; set; }

            public string LastError { get; set; }

            public string LastErrorCode { get; set; }

            public string LastPolled { get; set; }

            public int? NumberOfPolls { get; set; }

            public string LastSubmitted { get; set; }

            public int? NumberOfSubmits { get; set; }

            public string ProviderReference { get; set; }

            public string STAN { get; set; }

            public bool ManuallyAuthorised { get; set; }

            public string PreAuthorisationNumber { get; set; }

            public string AuthorisationNumber { get; set; }

            public long? PaymentMethodId { get; set; }

            public string ReferenceNumber { get; set; }

            public string SettlementDate { get; set; }

            public string Reason { get; set; }

            public long? ReasonId { get; set; }

            public string Status { get; set; }

            public long? StatusId { get; set; }

            public string Date { get; set; }

            public string Provider { get; set; }

            public long? ProviderId { get; set; }

            public double TaxAmount { get; set; }

            public string TaxAmountText { get; set; }

            public double Amount { get; set; }

            public string AmountText { get; set; }

            public string Source { get; set; }

            public long? SourceId { get; set; }

            public string Type { get; set; }

            public long? TypeId { get; set; }

            public bool Reconciled { get; set; }

            public List<Log> Logs { get; set; }
        }

        public class Log
        {
            public long Id { get; set; }

            public string Date { get; set; }

            public string LogEntry { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }

        }

        public class External
        {
            public long Id { get; set; }

            public string ServiceProviderBankAccount { get; set; }

            public string ServiceProviderBankBSB { get; set; }

            public string CustomerProviderBankAccount { get; set; }

            public string CustomerProviderBankBSB { get; set; }

            public string ChequeNumber { get; set; }

            public string CreditCardType { get; set; }

            public string CardHolderName { get; set; }

            public string ExpiryDate { get; set; }

            public string MerchantId { get; set; }

            public long? BankAccountId { get; set; }

            public long? BankDepositId { get; set; }

            public string ReasonCode { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

            public string FinancialTransactionNumber { get; set; }

            public string TransactionType { get; set; }

            public string TransactionTypeCode { get; set; }

            public string Notes { get; set; }

            public long? StatusId { get; set; }

            public string Status { get; set; }

            public string Date { get; set; }

            public double Amount { get; set; }

            public string AmountText { get; set; }

            public string Reference { get; set; }

            public string DebtorCode { get; set; }

            public string OriginalReference { get; set; }

            public string LastUpdated { get; set; }

            public string OriginalDate { get; set; }

            public string SettlementDate { get; set; }

            public string AdditionalInformation { get; set; }

            public bool DefaultPayerId { get; set; }

            public string File { get; set; }

            public long? FileId { get; set; }

            public string FileType { get; set; }

            public string FileDate { get; set; }

            public string PayerId { get; set; }

            public string UpdatedBy { get; set; }
        }

        public class Charge
        {
            public int Count { get; set; }

            public List<ChargeList> List { get; set; }

        }

        public class ChargeList
        {
            public long Id { get; set; }

            public string ChargeCode { get; set; }

            public string Description { get; set; }

            public double AmountTaxEx { get; set; }

            public string AmountTaxExText { get; set; }

            public double Tax { get; set; }

            public string TaxText { get; set; }

            public string From { get; set; }

            public string To { get; set; }

            public string ServiceTypeCode { get; set; }

            public long? ServiceReference { get; set; }

            public string ServiceId { get; set; }

            public string GeneralLedgerCode { get; set; }
        }

    }
}
