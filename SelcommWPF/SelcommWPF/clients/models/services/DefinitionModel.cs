using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.services
{
    internal class DefinitionModel
    {
        public long Id { get; set; }
        public string Template { get; set; }
        public string AutomaticCodeId { get; set; }

        public string CodePrefix { get; set; }

        public int? ArchiveDays { get; set; }

        public string DefaultPriority { get; set; }

        public string Source { get; set; }

        public string DefaultNote { get; set; }

        public string SLAIntervalPeriodType { get; set; }

        public long? TemplateId { get; set; }

        public int? InitialSLAPeriod { get; set; }

        public int? MaximumSLANotifications { get; set; }

        public int? InProgressSLAPeriod { get; set; }

        public string RecurringIntervalPeriodType { get; set; }

        public int? RecurringInterval { get; set; }

        public int? MaximumRecurringInstances { get; set; }

        public long? SetReference { get; set; }

        public string CreatedBy { get; set; }

        public string Created { get; set; }

        public int? SubsequentSLAPeriod { get; set; }

        public string LastUpdated { get; set; }

        public string ContactStatus { get; set; }

        public string Code { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string CategoryId { get; set; }

        public string Category { get; set; }

        public string Schedulable { get; set; }

        public List<Status> ServiceStatuses { get; set; }

        public bool ReSchedulable { get; set; }

        public bool DocumentUploads { get; set; }

        public List<TeamModel> Teams { get; set; }

        public bool DocumentCustomerView { get; set; }

        public bool ServiceDeskDisplay { get; set; }

        public bool SelfServiceDisplay { get; set; }

        public bool Display { get; set; }

        public bool SystemRecord { get; set; }

        public bool CustomerEditable { get; set; }

        public string ScheduleDepartmentId { get; set; }

        public bool CustomerSchedulable { get; set; }

        public string UpdatedBy { get; set; }

        public class Status
        {
            public string ScheduleStatusId { get; set; }

            public string ScheduleStatus { get; set; }

            public string ServiceTypeId { get; set; }

            public string ServiceType { get; set; }

            public string ServiceStatusId { get; set; }

            public string ServiceStatus { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }
        }
        public class TeamModel
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }
    }
}
