using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dwolla.Client.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CustomerType
    {
        Personal,
        Business,
        Unverified
    }
}
