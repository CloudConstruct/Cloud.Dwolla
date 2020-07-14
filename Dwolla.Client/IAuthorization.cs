using System;

namespace Dwolla.Client
{
    public class DwollaToken
    {
        public string AccessToken { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }
}
