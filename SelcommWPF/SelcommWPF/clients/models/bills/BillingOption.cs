using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.bills
{
    class BillingOption
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool Mandatory { get; set; }

        public bool Set { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public string EditMode { get; set; }

        public bool History { get; set; }

        public bool Editable { get; set; }

        public string ExclusiveTo { get; set; }
    }
}
