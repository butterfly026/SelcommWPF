using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.contacts
{
    class AddressModel
    {

        public List<AddressUsage> ContactAddressUsage { get; set; }

        public List<AddressMandatoryRule> ContactAddressMandatoryRules { get; set; }

        public List<AddressType> ContactAddressTypes { get; set; }

        public class AddressUsage
        {
            public long Id { get; set; }

            public string AddressLine1 { get; set; }

            public string AddressLine2 { get; set; }

            public string Suburb { get; set; }

            public string City { get; set; }

            public string State { get; set; }

            public string PostCode { get; set; }

            public string CountryCode { get; set; }

            public string Country { get; set; }

            public List<AddressType> AddressTypes { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }

        }


        public class AddressMandatoryRule
        {
            public string TypeCode { get; set; }

            public string Type { get; set; }
        }

        public class AddressType
        {
            public string Code { get; set; }

            public string Name { get; set; }

            public string FromDateTime { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }
        }
        
        public class Country
        {
            public string Code { get; set; }

            public string Name { get; set; }

            public string ISD { get; set; }

            public int DisplayOrder { get; set; }

            public bool Default { get; set; }
        }

        public class History
        {
            public long Id { get; set; }

            public long AddressId { get; set; }

            public string AddressTypeCode { get; set; }

            public string AddressType { get; set; }

            public string AddressLine1 { get; set; }

            public string AddressLine2 { get; set; }

            public string Suburb { get; set; }

            public string City { get; set; }

            public string State { get; set; }

            public string PostCode { get; set; }

            public string CountryCode { get; set; }

            public string Country { get; set; }

            public string FromDateTime { get; set; }

            public string ToDateTime { get; set; }

            public string LastUpdated { get; set; }

            public string UpdatedBy { get; set; }

        }

    }
}
