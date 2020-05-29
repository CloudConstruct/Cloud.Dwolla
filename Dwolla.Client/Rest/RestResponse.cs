using System.Linq;
using System.Net.Http;
using Dwolla.Client.Models.Responses;

namespace Dwolla.Client.Rest
{
    public class RestResponse
    {
        public ErrorResponse Error { get; }
        public string RawContent { get; }
        public string RequestId { get; }
        public HttpResponseMessage Response { get; }

        internal RestResponse(HttpResponseMessage response, string rawContent, ErrorResponse error = null)
        {
            Error = error;
            RawContent = rawContent;
            Response = response;
            RequestId = GetRequestId(response);
        }

        private static string GetRequestId(HttpResponseMessage r)
        {
            if (r == null) return null;
            r.Headers.TryGetValues("x-request-id", out var values);
            return values?.FirstOrDefault();
        }
    }

    public class RestResponse<T> : RestResponse
    {
        public T Content { get; }

        internal RestResponse(HttpResponseMessage response, T content, string rawContent, ErrorResponse error = null) :
            base(response, rawContent, error)
        {
            Content = content;
        }

    }
}
