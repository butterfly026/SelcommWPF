using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class TaskModel
    {

        public int Count { get; set; }

        public List<Item> Items { get; set; }


        public class Item
        {
            public string ContactCode { get; set; }

            public string ServiceId { get; set; }

            public bool FAQ { get; set; }

            public string FAQTag { get; set; }

            public string LastCustomerPrompt { get; set; }

            public int NumberOfCustomerPrompts { get; set; }

            public string LastResourcePrompt { get; set; }

            public int NumberOfResourcePrompts { get; set; }

            public string LastSLAPrompt { get; set; }

            public int NumberOfSLAPrompts { get; set; }

            public int? QuotedHours { get; set; }

            public double? QuotedPrice { get; set; }

            public string QuotedPriceText { get; set; }

            public bool? QuoteFixedPrice { get; set; }

            public long? InvoiceId { get; set; }

            public string InvoiceNumber { get; set; }

            public int PercentComplete { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

            public string UpdatedBy { get; set; }

            public string Updated { get; set; }

            public string Type { get; set; }

            public string Group { get; set; }

            public string Status { get; set; }

            public bool Ticket { get; set; }

            public string Priority { get; set; }

            public bool VisibleToCustomer { get; set; }

            public string SLAData { get; set; }

            public long? ServiceReference { get; set; }

            public string ServiceType { get; set; }

            public long Id { get; set; }

            public long TypeId { get; set; }

            public long GroupId { get; set; }

            public string Number { get; set; }

            public string Reference { get; set; }

            public string RequestedBy { get; set; }

            public List<Email> Emails { get; set; }

            public string EmailsText { get; set; }

            public long? StatusId { get; set; }

            public long? ParentId { get; set; }

            public long? ResolutionId { get; set; }

            public long? PriorityId { get; set; }

            public string ShortDescription { get; set; }

            public string Description { get; set; }

            public string ShortResolution { get; set; }

            public string ResolutionDetail { get; set; }

            public string RequiredDate { get; set; }

            public string CustomerRequestedDate { get; set; }

            public string CompletedDate { get; set; }

            public string EstimatedDate { get; set; }

            public string NextFollowupDate { get; set; }

            public string Resolution { get; set; }

            public bool? Internal { get; set; }

            public string Comment { get; set; }

            public bool? Chargeable { get; set; }

            public int? Minutes { get; set; }

            public string Note { get; set; }

        }

        public class Email
        {
            public long? Id { get; set; }

            public string Address { get; set; }
        }

    }
}
