using System.Collections.Generic;

namespace SelcommWPF.clients.models
{
    class ServicesType
    {

        public int Count { get; set; }

        public List<ServicesModel> ServiceTypeNodes { get; set; }

        public class ServicesModel
        {
            public int Count { get; set; }

            public int DisplayOrder { get; set; }

            public string Icon { get; set; }

            public long InstanceMenuId { get; set; }

            public long MenuId { get; set; }

            public string ServiceType { get; set; }

            public string ServiceTypeCode { get; set; }
            
            public string StatusCountsText { get; set; }

            public List<StatusCountModel> StatusCounts { get; set; }

            public class StatusCountModel
            {
                public int Count { get; set; }

                public string Status { get; set; }

                public string StatusCode { get; set; }
            }

        }

        public class Item
        {
            public string Id { get; set; }
            
            public string Name { get; set; }

            public bool Display { get; set; }

            public int DisplayOrder { get; set; }

            public string BillingDescription { get; set; }

            public string AdditionalInformationField1 { get; set; }

            public string AdditionalInformationField2 { get; set; }

            public string AdditionalInformationField3 { get; set; }

            public bool Used { get; set; }

            public string GroupName { get; set; }
        }

        public class Available
        {
            public string ServiceId { get; set; }

            public string Ranking { get; set; } 

            public double Fee { get; set; }

            public string FeeText { get; set; }
        }

    }
}
