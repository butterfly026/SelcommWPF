using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelcommWPF.clients.models.messages
{
    class ValidModel
    {

        public bool Valid { get; set; }

        public bool Message { get; set; }

        public List<ResultItem> Results { get; set; }

        public class ResultItem
        {
            public string Result { get; set; }

            public string Message { get; set; }
        }

    }
}
