using System.Collections.Generic;

namespace SelcommWPF.clients.models
{
    class NoteResponse
    {
        public int Count { get; set; }

        public List<NoteModel> Notes { get; set; }

        public class NoteModel
        {

            public string Body { get; set; }

            public string Created { get; set; }

            public string CreatedBy { get; set; }

            public bool Editable { get; set; }

            public bool HistoryExists { get; set; }

            public long Id { get; set; }

            public bool VisibleToCustomer { get; set; }

        }

    }
}
