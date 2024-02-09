using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.contacts
{
    class SearchModel
    {
        public int RecordCount { get; set; }

        public List<Item> Items { get; set; }

        public class Item
        {
            public string Type { get; set; }

            public string SubType { get; set; }

            public string BusinessUnitCode { get; set; }

            public string BusinessUnit { get; set; }

            public string Status { get; set; }

            public string StatusCode { get; set; }

            public string ContactCode { get; set; }

            public string Name { get; set; }

            public string DateOfBirth { get; set; }

            public List<Phone> ContactPhones { get; set; }

            public List<AddressModel> Addresses { get; set; }

            public List<ServiceType> ServiceTypes { get; set; }

            public List<EmailModel> Emails { get; set; }
        }

        public class Phone
        {
            public string Type { get; set; }

            public string Number { get; set; }
        }

        public class AddressModel
        {
            public string Type { get; set; }

            public string Address { get; set; }
        }

        public class ServiceType
        {
            public string Type { get; set; }

            public string Count { get; set; }
        }

        public class EmailModel
        {
            public string Email { get; set; }
        }


    }
}
