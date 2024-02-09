using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.report
{
    class ReportDetailModel
    {

        public int Count { get; set; }

        public List<Item> Reports { get; set; }

        public class Item
        {
            public long Id { get; set; }

            public string DefinitionId { get; set; }

            public long? ScheduleId { get; set; }

            public string Requested { get; set; }

            public string Status { get; set; }

            public string StatusDateTime { get; set; }

            public string Name { get; set; }

            public string LongDescription { get; set; }

            public string CategoryId { get; set; }

            public string Category { get; set; }

            public string ContactId { get; set; }

            public bool Delete { get; set; }

            public string Created { get; set; }

            public string CreatedBy { get; set; }

            public List<Email> Emails { get; set; }

            public string EmailsText { get; set; }

            public List<Parameter> Parameters { get; set; }

            public string RequestingUser { get; set; }

            public string To { get; set; }

            public string From { get; set; }

            public int? Priority { get; set; }

            public string OutputFileName { get; set; }

            public string Comments { get; set; }

            public string Datetime { get; set; }

            public string UpdatedBy { get; set; }

            public string LastUpdated { get; set; }

            public bool Sunday { get; set; }

            public bool Saturday { get; set; }

            public bool Friday { get; set; }

            public bool Thursday { get; set; }

            public bool Wednesday { get; set; }

            public bool Tuesday { get; set; }

            public bool Monday { get; set; }

            public int? DayOfMonth { get; set; }

        }

        public class Email
        {
            public long Id { get; set; }

            public string Address { get; set; }

            public long? EmailDefinitionId { get; set; }

            public bool Log { get; set; }

            public bool Output { get; set; }
        }

        public class Parameter
        {
            public long Id { get; set; }

            public string Name { get; set; }

            public long? ParameterDefinitionId { get; set; }

            public string Value { get; set; }
        }

    }
}
