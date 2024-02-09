using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class AccountCharge
    {

        public int Count { get; set; }

        public List<History> Items { get; set; }

        public class History
        {
            public int AdvancePeriods { get; set; }

            public string Anniversary { get; set; }

            public bool AttributeBased { get; set; }

            public long? AutoSourceId { get; set; }

            public string BillingCycle { get; set; }

            public string BillingCycleId { get; set; }

            public string BillDescription { get; set; }

            public bool ChargeInAdvance { get; set; }

            public double? Cost { get; set; }

            public string CostText { get; set; }

            public string Created { get; set; }

            public string CreatedBy { get; set; }

            public string CustomerReference { get; set; }

            public string DefinitionId { get; set; }
            
            public string DefinitionFrequencyId { get; set; }

            public string Description { get; set; }

            public double DiscountAmount { get; set; }

            public string DiscountAmountText { get; set; }

            public bool DiscountBased { get; set; }

            public double DiscountPercentage { get; set; }

            public string DiscountType { get; set; }

            public bool DisplayEndDate { get; set; }

            public bool ETF { get; set; }

            public bool Editable { get; set; }

            public string ExternalSource { get; set; }

            public string ExternalTableName { get; set; }

            public long? ExternalTransactionId { get; set; }

            public string FinancialDocument { get; set; }

            public string Frequency { get; set; }

            public string FrequencyId { get; set; }

            public string From { get; set; }

            public bool GeoBased { get; set; }

            public long Id { get; set; }

            public string InvoicedTo { get; set; }

            public string Note { get; set; }

            public int NumberOfInstances { get; set; }

            public string LastUpdated { get; set; }

            public string OverrideDescription { get; set; }

            public string OtherReference { get; set; }

            public double? OverMarkUp { get; set; }

            public long? OverRideId { get; set; }

            public double? OverRidePrice { get; set; }

            public string OverRidePriceText { get; set; }

            public long? Period { get; set; }

            public string Plan { get; set; }

            public long? PlanId { get; set; }

            public long? PlanOptionId { get; set; }

            public double Price { get; set; }

            public string PriceText { get; set; }

            public double PriceTaxEx { get; set; }

            public string PriceTaxExText { get; set; }

            public double PriceTaxInc { get; set; }

            public string PriceTaxIncText { get; set; }

            public long? ProfileId { get; set; }

            public bool Prorated { get; set; }

            public string ProviderCode { get; set; }

            public double Quantity { get; set; }

            public string Reference { get; set; }

            public string RevenueAccount { get; set; }

            public bool Reversal { get; set; }

            public string ServiceId { get; set; }

            public long? ServiceReference { get; set; }

            public string ServiceType { get; set; }

            public string ServiceTypeId { get; set; }

            public string Source { get; set; }

            public string Status { get; set; }

            public string Start { get; set; }

            public string To { get; set; }

            public string ToDescription { get; set; }

            public string Type { get; set; }

            public double UndiscountedPriceTaxEx { get; set; }

            public string UndiscountedPriceTaxExText { get; set; }

            public double UndiscountedPriceTaxInc { get; set; }

            public string UndiscountedPriceTaxIncText { get; set; }

            public string Unit { get; set; }

            public string UpdatedBy { get; set; }

        }

    }
}
