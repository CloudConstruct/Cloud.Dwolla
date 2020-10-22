using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dwolla.Client.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BusinessType
    {
        LLC,
        Corporation,
        Partnership,
        [Display(Name = "Sole Proprietorship")]
        SoleProprietorship,
    }
}
