namespace Dwolla.Client.Models.Requests
{
    internal class TransferCancelRequest
    {
        public TransferStatus Status => TransferStatus.Cancelled;
    }
}
