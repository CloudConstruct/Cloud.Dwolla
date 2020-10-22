using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dwolla.Client.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BankAccountType
    {
        Checking,
        Savings,
        [EnumMember(Value = "general-ledger")]
        GeneralLedger,
        Loan
    }
}
