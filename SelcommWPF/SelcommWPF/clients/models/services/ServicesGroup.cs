using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class ServicesGroup
    {

        public int Count { get; set; }

        public List<ServiceGroup> ServiceGroupNodes { get; set; }

        public class ServiceGroup
        {

            public long Id { get; set; }

            public string Code { get; set; }

            public int Count { get; set; }

            public string Name { get; set; }

            public string Status { get; set; }

            public string StatusCountsText { get; set; }

            public List<ServicesType.ServicesModel.StatusCountModel> StatusCounts { get; set; }
        }

    }
}
