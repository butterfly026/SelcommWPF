using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models
{
    class ServicesSites
    {

        public int Count { get; set; }

        public List<SitesModel> SiteNodes { get; set; }

        public class SitesModel
        {

            public long Id { get; set; }

            public string Code { get; set; }

            public int Count { get; set; }

            public string Site { get; set; }

            public string Status { get; set; }

            public string Type { get; set; }

        }

    }
}
