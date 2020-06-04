using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dwolla.Client.Models
{
    public class Link
    {
        public Uri Href { get; set; }
        public string Type { get; set; }

        [JsonProperty(PropertyName = "resource-type")]
        public string ResourceType { get; set; }
        public Guid? Id => Href == null || Href.Segments.Length == 0 ? (Guid?) null : Guid.Parse(Href.Segments[Href.Segments.Length - 1]);
    }

    public class LinkDictionary : Dictionary<string, Link>
    {
        public Link Source => this["source"];
        public Link FundingSources => this["funding-sources"];
        public Link FundedTransfer => this["funded-transfer"];
        public Link FundingTransfer => this["funding-transfer"];
    }
}
