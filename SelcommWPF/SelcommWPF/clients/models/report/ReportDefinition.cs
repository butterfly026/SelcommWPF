using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.report
{
    class ReportDefinition
    {

        public int Count { get; set; }

        public List<Item> ReportDefinitions { get; set; }

        public class Item
        {
            public string Id { get; set; }

            public List<Email> Emails { get; set; }

            public string UpdatedBy { get; set; }

            public string LastUpdated { get; set; }

            public string Created { get; set; }

            public string CreatedBy { get; set; }

            public string Note { get; set; }

            public bool Schedulable { get; set; }

            public bool ChannelPartnerEnabled { get; set; }

            public bool CustomerEnabled { get; set; }

            public bool Enabled { get; set; }

            public bool ContactRequired { get; set; }

            public string Category { get; set; }

            public long? CategoryId { get; set; }

            public string Name { get; set; }

            public string LongDescription { get; set; }

            public List<Parameter> Parameters { get; set; }

            public List<BusinessUnit> BusinessUnits { get; set; }
        }

        public class Email
        {
            public long Id { get; set; }

            public string Address { get; set; }

            public bool Deleted { get; set; }

            public int? DisplayOrder { get; set; }
        }

        public class Parameter
        {
            public long Id { get; set; }

            public string Name { get; set; }

            public string DataType { get; set; }

            public string Default { get; set; }

            public bool Deleted { get; set; }

            public bool Locked { get; set; }

            public bool MultipleSelection { get; set; }

            public bool Optional { get; set; }

            public string URL { get; set; }

            public string Tooltip { get; set; }

            public string VisibleString { get; set; }

            public string VisibleInteger { get; set; }

            public string VisibleDecimal { get; set; }

            public string VisibleBoolean { get; set; }

            public string VisibleDate { get; set; }

            public string VisibleCurrency { get; set; }

        }

        public class BusinessUnit
        {
            public string BusinessUnitId { get; set; }

            public string Name { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

        }

    }
}
