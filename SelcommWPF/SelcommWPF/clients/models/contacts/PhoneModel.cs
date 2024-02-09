using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.contacts
{
    class PhoneModel
    {

        public List<PhoneUsage> ContactPhoneUsages { get; set; }

        public List<PhoneMandatoryRule> ContactPhoneMandatoryRules { get; set; }

        public List<PhoneType> ContactPhoneTypes { get; set; }

        public class PhoneUsage
        {
            public long Id { get; set; }

            public string PhoneNumber { get; set; }

            public List<PhoneType> PhoneTypes { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }
        }

        public class PhoneMandatoryRule
        {
            public string TypeCode { get; set; }

            public string Type { get; set; }
        }

        public class PhoneType
        {
            public string Code { get; set; }

            public string Name { get; set; }

            public string FromDateTime { get; set; }

            public string DefaultFormat { get; set; }
        }

        public class History
        {
            public long Id { get; set; }

            public string PhoneTypeCode { get; set; }

            public string PhoneType { get; set; }

            public string PhoneNumber { get; set; }

            public string FromDateTime { get; set; }

            public string ToDateTime { get; set; }

            public string UpdatedBy { get; set; }

        }

    }
}
