using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.contacts
{
    class EmailModel
    {
        public List<EmailUsage> contactEmailUsages;

        public List<EmailMandatoryRule> contactEmailMandatoryRules;

        public List<EmailType> contactEmailTypes;

        public class EmailUsage
        {
            public long Id { get; set; }

            public string emailAddress { get; set; }

            public List<EmailType> emailTypes { get; set; }

            public string fromDateTime { get; set; }

            public string created { get; set; }

            public string createdBy { get; set; }

            public string lastUpdated { get; set; }

            public string updatedBy { get; set; }
        }

        public class EmailMandatoryRule
        {

        }

        public class EmailType
        {

        }

    }
}
