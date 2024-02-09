using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class ServicesPlans
    {

        public int Count { get; set; }

        public List<PlanModel> PlanNodes { get; set; }

        public class PlanModel
        {
            public int Id { get; set; }

            public int Count { get; set; }

            public string Plan { get; set; }
        }

        public class Item
        {
            public int Count { get; set; }

            public List<Detail> Plans { get; set; }
        }

        public class Detail
        {
            public int PlanId { get; set; }

            public string Plan { get; set; }

            public string DisplayName { get; set; }

            public string GroupId { get; set; }

            public string Group { get; set; }

            public string TypeId { get; set; }

            public string Type { get; set; }

            public bool TypeDefault { get; set; }

            public bool CycleLocked { get; set; }

            public string From { get; set; }

            public string To { get; set; }

            public List<Option> Options { get; set; }
        }

        public class Option
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public bool Default { get; set; }

            public int Order { get; set; }

        }

    }
}
