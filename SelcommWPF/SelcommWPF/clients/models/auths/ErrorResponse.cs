
namespace SelcommWPF.clients.models
{
    class ErrorResponse
    {
        public string Type { get; set; }

        public string Title { get; set; }

        public string Status { get; set; }

        public object Errors { get; set; }

        public string TraceId { get; set; }

        public class ErrorBody
        {
            public string ErrorCode { get; set; }

            public string ErrorMessage { get; set; }
        }

    }
}
