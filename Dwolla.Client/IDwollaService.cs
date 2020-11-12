using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dwolla.Client.Models;
using Dwolla.Client.Models.Requests;
using Dwolla.Client.Models.Responses;

namespace Dwolla.Client
{
    public interface IDwollaService
    {
        Task<Customer> GetCustomerAsync(Guid customerId);
        Task<RootResponse> GetRootAsync();
        Task<Uri> CreateCustomerAsync(CreateCustomerRequest customerRequest);
        Task<Customer> UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest customersRequest);
        Task<Uri> CreateBeneficialOwnerAsync(Guid customerId, CreateBeneficialOwnerRequest createBeneficialOwnerRequest);
        Task<BeneficialOwnershipResponse> CertifyBeneficialOwnershipAsync(Guid customerId);

        Task<GetDocumentsResponse> GetCustomerDocumentsAsync(Guid customerId);

        Task<DocumentResponse> GetDocumentAsync(Guid documentId);
        Task<FundingSource> GetFundingSourceAsync(Guid fundingSourceId);
        Task<BalanceResponse> GetFundingSourceBalanceAsync(Guid fundingSourceId);
        Task<TransferResponse> GetTransferAsync(Guid transferId);
        Task<Uri> UploadDocumentAsync(Guid customerId, UploadDocumentRequest document);
        Task<GetEventsResponse> GetEventsAsync();
        Task<GetBusinessClassificationsResponse> GetBusinessClassificationsAsync();
        Task<WebhookSubscription> GetWebhookSubscriptionAsync(Guid webhookSubscriptionId);
        Task<GetWebhookSubscriptionsResponse> GetWebhookSubscriptionsAsync();
        Task<Uri> CreateWebhookSubscriptionAsync(string url, string secret);
        Task<WebhookSubscription> DeleteWebhookSubscriptionAsync(Guid webhookSubscriptionId);
        Task<GetCustomersResponse> GetCustomersAsync();
        Task<BeneficialOwnershipResponse> GetBeneficialOwnershipAsync(Guid customerId);
        Task<GetBeneficialOwnersResponse> GetBeneficialOwnersAsync(Guid customerId);
        Task<BeneficialOwnerResponse> GetBeneficialOwnerAsync(Guid beneficialOwnerId);
        Task<BeneficialOwnerResponse> DeleteBeneficialOwnerAsync(Guid beneficialOwnerId);
        Task<GetFundingSourcesResponse> GetCustomerFundingSourcesAsync(Guid customerId, bool includeRemoved = true);
        Task<IavTokenResponse> GetCustomerIavTokenAsync(Guid customerId);
        Task<MicroDepositsResponse> GetMicroDepositsAsync(Guid fundingSourceId);
        Task<Uri> InitiateMicroDepositsAsync(Guid fundingSourceId);
        Task<BaseResponse> VerifyMicroDepositsAsync(Guid fundingSourceId, decimal amount1, decimal amount2);
        Task<TransferFailureResponse> GetTransferFailureAsync(Guid transferId);
        Task<Uri> CreateTransferAsync(Guid sourceFundingSourceId, Guid destinationFundingSourceId, decimal amount,
            decimal? fee, Guid? chargeToCustomer, string sourceAddenda, string destinationAddenda,
            string correlationId, Clearing clearing = null);
        Task<TransferResponse> CancelTransferAsync(Guid transferId);
        Task<FundingSource> RemoveFundingSourceAsync(Guid fundingSourceId);
        Task<Uri> CreateFundingSourceAsync(Guid customerId, string routingNumber, string accountNumber, BankAccountType bankAccountType, string name, string plaidToken = null, IEnumerable<string> channels = null, Guid? onDemandAuthorization = null);
        Task<Uri> CreateFundingSourceAsync(Guid customerId, string plaidToken, string name, Guid? onDemandAuthorization = null);
        Task<FundingSource> UpdateFundingSourceAsync(Guid fundingSourceId, string name);
    }
}
