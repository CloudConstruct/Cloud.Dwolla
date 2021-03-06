﻿using Newtonsoft.Json;

namespace Dwolla.Client.Models.Responses
{
    /// <summary>
    ///     Implemented by any model returned by DwollaClient
    /// </summary>
    public interface IDwollaResponse
    {
    }

    public class BaseResponse : IDwollaResponse
    {
        [JsonProperty(PropertyName = "_links")]
        public LinkDictionary Links { get; set; }
    }
}
