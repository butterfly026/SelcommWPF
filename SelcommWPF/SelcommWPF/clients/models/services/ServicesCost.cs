using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class ServicesCost
    {

        public int Count { get; set; }

        public List<CostCenters> CostCenterNodes { get; set; }

        public class CostCenters
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
