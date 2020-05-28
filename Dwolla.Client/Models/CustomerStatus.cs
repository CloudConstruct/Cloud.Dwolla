using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dwolla.Client.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CustomerStatus
    {
        Kba,
        Verified,
        Unverified,
        Document,
        Retry,
        Suspended,
        Deactivated
    }
}
