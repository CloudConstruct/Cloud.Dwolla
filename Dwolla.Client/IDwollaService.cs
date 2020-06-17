using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dwolla.Client.Models.Requests;
using Dwolla.Client.Models.Responses;

namespace Dwolla.Client
{
    public interface IDwollaService
    {
        Task<Customer> GetCustomerAsync(Guid customerId);
        Task<GetDocumentsResponse> GetCustomerDocumentsAsync(Guid customerId);

        Task<DocumentResponse> GetDocumentAsync(Guid documentId);
        Task<FundingSource> GetFundingSourceAsync(Guid fundingSourceId);
        Task<BalanceResponse> GetFundingSourceBalanceAsync(Guid fundingSourceId);
        Task<TransferResponse> GetTransferAsync(Guid transferId);
        Task<Uri> UploadCustomerDocumentAsync(Guid customerId, UploadDocumentRequest document);
    }
}
