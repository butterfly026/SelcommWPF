using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class EventResponse
    {
        public int Count { get; set; }

        public List<EventModel> Events { get; set; }

        public class EventModel
        {

            public long Count { get; set; }

            public string Name { get; set; }

            public string Due { get; set; }

            public string ScheduleStatus { get; set; }

            public string StatusDateTime { get; set; }

            public string ScheduledBy { get; set; }

            public string ScheduledTo { get; set; }

            public string DepartmentScheduledTo { get; set; }

            public string Reason { get; set; }

            public string Note { get; set; }

            public long DefinitionId { get; set; }

            public string Code { get; set; }

            public string Type { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

            public List<Atturibute> Attributes { get; set; }

            public string Service { get; set; }
        }

        public class Atturibute
        {
            public long Id { get; set; }

            public long DefinitionId { get; set; }

            public string Name { get; set; }

            public string From { get; set; }

            public string To { get; set; }

            public string Value { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }

        }

    }
}
