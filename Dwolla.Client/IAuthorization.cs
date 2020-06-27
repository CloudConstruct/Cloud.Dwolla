using System;
using System.Collections.Generic;
using System.Text;

namespace Dwolla.Client
{
    public class DwollaToken
    {
        public string AccessToken { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }
}
