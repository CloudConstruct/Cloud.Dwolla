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
        public Guid? Id => ParseId(Href);

        public static Guid? ParseId(Uri url)
            => url == null || url.Segments.Length == 0 ? (Guid?)null : Guid.Parse(url.Segments[^1]);
    }

    public class LinkDictionary : Dictionary<string, Link> { }
}
