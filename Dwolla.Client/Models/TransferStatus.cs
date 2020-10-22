using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dwolla.Client.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TransferStatus
    {
        Pending,
        Processed,
        Failed,
        Cancelled
    }
}
