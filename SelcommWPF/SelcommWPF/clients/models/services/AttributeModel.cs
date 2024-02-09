using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.services
{
    class AttributeModel
    {
        public long Id { get; set; }

        public long DefinitionId { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public long EventId { get; set; }

        public bool Editable { get; set; }

        public string LastUpdated { get; set; }

        public string UpdatedBy { get; set; }
    }

}
