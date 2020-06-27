using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dwolla.Client.Models.Requests
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UpdateCustomerStatus
    {
        Suspended,
        Deactivated,
        Reactivated
    }
}
