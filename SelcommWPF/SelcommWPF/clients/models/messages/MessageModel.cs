using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.messages
{
    class MessageModel
    {
        public int Count { get; set; }

        public List<Item> Messages { get; set; }

        public class Item
        {
            public long Id { get; set; }

            public List<To> Addresses { get; set; }

            public List<File> Attachments { get; set; }
            
            public string Body { get; set; }

            public string BodyFormat { get; set; }

            public long? CorrelationId { get; set; }

            public string Created { get; set; }

            public string CreatedBy { get; set; }

            public bool DeliveryReceiptRequested { get; set; }

            public string Direction { get; set; }

            public string Due { get; set; }

            public string Importance { get; set; }

            public string LastUpdated { get; set; }

            public string Sender { get; set; }

            public string Status { get; set; }

            public string StatusLastUpdated { get; set; }

            public string Subject { get; set; }

            public string Type { get; set; }

            public string UpdatedBy { get; set; }

            public string AddressText { get; set; }

            public string VisiblityNew { get; set; }

            public string VisiblityOther { get; set; }
        }

        public class To
        {
            public string Address { get; set; }

            public string Created { get; set; }

            public string CreatedBy { get; set; }

            public long Id { get; set; }

            public string LastUpdated { get; set; }

            public string Status { get; set; }

            public string StatusLastUpdated { get; set; }
            
            public string Type { get; set; }

            public string UpdatedBy { get; set; }
        }

        public class File
        {
            public long? Id { get; set; }

            public long? DocumentId { get; set; }

            public string Name { get; set; }
        }

    }
}
