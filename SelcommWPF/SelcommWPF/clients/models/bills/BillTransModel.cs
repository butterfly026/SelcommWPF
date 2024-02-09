using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.bills
{
    class BillTransModel
    {

        public int Count { get; set; }

        public List<Item> Items { get; set; }

        public class Item
        {
            public long Id { get; set; }

            public long ServiceReference { get; set; }

            public string ServiceId { get; set; }

            public string ServiceNarrative { get; set; }

            public string ServiceType { get; set; }

            public string StartDateTime { get; set; }

            public string Details { get; set; }

            public string AdditionalDetails { get; set; }

            public string BParty { get; set; }

            public string BPartyDescription { get; set; }

            public string Duration { get; set; }

            public double UnitQuantity { get; set; }

            public string UnitOfMeasure { get; set; }

            public double Price { get; set; }

            public string PriceText { get; set; }

            public double Tax { get; set; }

            public string TaxText { get; set; }

            public double NonDiscountedPrice { get; set; }

            public string NonDiscountedPriceText { get; set; }

            public double NonDiscountedTax { get; set; }

            public string NonDiscountedTaxText { get; set; }

            public double Cost { get; set; }

            public string CostText { get; set; }

            public double CostTax { get; set; }

            public string CostTaxText { get; set; }

            public string UsageGroup { get; set; }

            public string UsageGroupCode { get; set; }

            public int UsageGroupOrder { get; set; }

            public string RateBandDescription { get; set; }

            public string TimeBandDescription { get; set; }

            public long TariffCode { get; set; }

            public string Tariff { get; set; }

            public string Origin { get; set; }

            public string ThirdParty { get; set; }

            public string Band1RateUnit { get; set; }

            public bool TaxFree { get; set; }

            public List<Component> TransactionComponents { get; set; }


        }

        public class Component
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string Type { get; set; }

            public double Amount { get; set; }

            public long DiscountId { get; set; }

            public string Discount { get; set; }

            public string TransactionCategory { get; set; }

            public string Tariff { get; set; }

            public long PlanId { get; set; }

            public long OverrideId { get; set; }

            public bool Taxable { get; set; }
        }

    }
}
