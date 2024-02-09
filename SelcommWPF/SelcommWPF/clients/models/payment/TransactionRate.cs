using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.payment
{
    class TransactionRate
    {
        public int Count { get; set; }

        public List<Item> Items { get; set; }

        public class Item
        {
            public long TariffClassId { get; set; }

            public string RevenueAccountId { get; set; }

            public string ServiceProviderTariffCode { get; set; }

            public string Unit { get; set; }

            public string UnitId { get; set; }

            public int TariffNumber { get; set; }

            public string CapType { get; set; }

            public int CostMultiple { get; set; }

            public int CapDuration { get; set; }

            public int CapAmount { get; set; }

            public string Capping { get; set; }

            public int MinimumDuration { get; set; }

            public double MinimumPrice { get; set; }

            public bool UseFinalBand { get; set; }

            public string Band4 { get; set; }

            public string Band3 { get; set; }

            public string Band2 { get; set; }

            public string Band1 { get; set; }

            public double ConnectPrice { get; set; }

            public string TimeBand { get; set; }

            public string TimeBandId { get; set; }

            public string TariffGroup { get; set; }

            public string TariffGroupId { get; set; }

            public string TariffClass { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }
        }
        
    }
}
