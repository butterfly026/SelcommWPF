using SelcommWPF.clients.models.bills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class UsageModel
    {

        public int Count { get; set; }

        public List<BillTransModel.Item> Transactions { get; set; }

    }
}
