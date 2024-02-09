using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.payment
{
    class ProductModel
    {

        public int Count { get; set; }

        public List<Item> Items { get; set; }

        public class Item
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public double DefaultPrice { get; set; }

            public double DefaultTax { get; set; }

            public bool Serialised { get; set; }
        }

        public class Data
        {
            public string ProductId { get; set; }

            public string ProductName { get; set; }

            public string OverrideDescription { get; set; }

            public int Quantity { get; set; }

            public double UnitPrice { get; set; }

            public string UnitPriceText { get; set; }

            public double TotalPrice { get; set; }

            public string TotalPriceText { get; set; }
             
            public double Tax { get; set; }

            public List<Dictionary<string, string>> Serials { get; set; }

            public string SerialsText { get; set; }

            public string Note { get; set; }

        }

    }
}
