using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dwolla.Client.Models.Requests
{
    class CreateFundingSourceRequest
    {
        [JsonProperty(PropertyName = "_links")]
        public Dictionary<string, Link> Links { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public BankAccountType BankAccountType { get; set; }
        public string Name { get; set; }
        public string PlaidToken { get; set; }
        public IEnumerable<string> Channels { get; set; }
    }
}
