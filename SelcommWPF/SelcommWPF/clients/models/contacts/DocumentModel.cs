using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.contacts
{
    class DocumentModel
    {

        public string Name { get; set; }

        public string Extension { get; set; }

        public double Size { get; set; }

        public string SizeText { get; set; }

        public string Content { get; set; }

        public string CreationTime { get; set; }

        public string CreationTimeUtc { get; set; }

        public class Item
        {
            public long Id { get; set; }

            public string Type { get; set; }

            public string Name { get; set; }

            public string Date { get; set; }

            public bool Selected { get; set; }
        }

        public class Account
        {
            public int Count { get; set; }

            public List<Detail> Documents { get; set; }
        }

        public class Detail
        {
            public long Id { get; set; }
            
            public string Name { get; set; }

            public string Category { get; set; }

            public string FileType { get; set; }

            public string Note { get; set; }

            public string Author { get; set; }

            public string DateAuthored { get; set; }

            public bool UserEditable { get; set; }

            public bool ContactEditable { get; set; }

            public bool ContactVisible { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

            public string UpdatedBy { get; set; }

            public string Updated { get; set; }
        }

        public class FullDetail
        {

            public long Id { get; set; }

            public string Name { get; set; }

            public string Category { get; set; }

            public string FileType { get; set; }

            public string Note { get; set; }

            public string Author { get; set; }

            public string DateAuthored { get; set; }

            public bool UserEditable { get; set; }

            public bool ContactEditable { get; set; }

            public bool ContactVisible { get; set; }

            public string Content { get; set; }

            public string CreatedBy { get; set; }

            public string Created { get; set; }

            public string UpdatedBy { get; set; }

            public string Updated { get; set; }
        }
    }
}
