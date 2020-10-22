using System;
using System.Collections.Generic;
using System.Text;

namespace Dwolla.Client.Models.Requests
{
    class TransferCancelRequest
    {
        public TransferStatus Status => TransferStatus.Cancelled;
    }
}
