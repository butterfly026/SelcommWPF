using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.auths
{
    class LoginHistory
    {
        public int Count { get; set; }

        public List<Item> Items { get; set; }

        public class Item
        {
            public long Id { get; set; }

            public bool KnownAbuser { get; set; }

            public bool KnownAttacker { get; set; }

            public bool Anonymous { get; set; }

            public bool Proxy { get; set; }

            public bool Tor { get; set; }

            public string UserNotificationSent { get; set; }

            public bool Threat { get; set; }

            public bool OTP { get; set; }

            public string Location { get; set; }

            public string ip { get; set; }

            public string Password { get; set; }

            public string Status { get; set; }

            public string Date { get; set; }

            public string LoginCode { get; set; }

            public bool MFA { get; set; }

            public bool Bogon { get; set; }
        }
    }
}
