using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.contacts
{
    class Identification
    {

        public string Id { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public int? Points { get; set; }

        public bool IsCreditCard { get; set; }

        public bool HasExpiryDate { get; set; }

        public string ExpiryDate { get; set; }

        public bool HasIssueDate { get; set; }

        public string IssueDate { get; set; }

        public string ContactTypeApplicability { get; set; }
        
        public bool AllowDuplicates { get; set; }

        public string Created { get; set; }

        public string CreatedBy { get; set; }

        public string LastUpdated { get; set; }

        public string UpdatedBy { get; set; }

    }
}
