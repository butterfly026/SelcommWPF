using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.contacts
{
    class NameModel
    {
        public string ContactType { get; set; }

        public string Name { get; set; }

        public string FirstName { get; set; }

        public string Initials { get; set; }

        public string Title { get; set; }

        public string ContactKey { get; set; }

        public List<ContactAliase> ContactAliases { get; set; }

        public List<Dictionary<string, string>> AliasTypes { get; set; }

        public List<Dictionary<string, string>> Titles { get; set; }

        public class ContactAliase
        {
            public string Id { get; set; }

            public string TypeCode { get; set; }

            public string Type { get; set; }

            public string Alias { get; set; }

            public string UpdatedBy { get; set; }

            public string LastUpdated { get; set; }
        }

        public class History
        {
            public long Id { get; set; }

            public string OldDetails { get; set; }

            public string TypeCode { get; set; }

            public string Type { get; set; }

            public string Alias { get; set; }

            public string FromDateTime { get; set; }

            public string ToDateTime { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }
        }

    }
}
