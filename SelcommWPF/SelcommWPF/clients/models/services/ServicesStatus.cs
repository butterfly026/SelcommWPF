using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class ServicesStatus
    {

        public int Count { get; set; }

        public List<StatusModel> StatusNodes { get; set; }

        public class StatusModel
        {
            public string Id { get; set; }

            public int Count { get; set; }

            public string Status { get; set; }
        }

    }
}
