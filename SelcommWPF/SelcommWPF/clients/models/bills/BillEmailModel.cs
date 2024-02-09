using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.bills
{
    class BillEmailModel
    {
        public string BodyFormat { get; set; }

        public string Importance { get; set; }

        public List<Recipient> Recipients { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public bool AttachPDF { get; set; }

        public bool AttachXLS { get; set; }

        public List<Image> Images { get; set; }

       
        public class Recipient
        {
            public string Type { get; set; }

            public long Id { get; set; }

            public string Address { get; set; }

            public bool RequestDeliveryReceipt { get; set; }
        }
        
        public class Image
        {
            public long Id { get; set; }
        }
    

    }
}
