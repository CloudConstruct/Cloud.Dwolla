using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dwolla.Client.Models;
using Dwolla.Client.Models.Requests;
using Dwolla.Client.Models.Responses;
using Microsoft.Extensions.DependencyInjection;

namespace Dwolla.Client
{
    class DwollaService : IDwollaService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IDwollaClient dwollaClient;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly Func<IServiceProvider, Task<DwollaToken>> initializeToken;
        private readonly Func<IServiceProvider, DwollaToken, Task> saveToken;

        private DwollaToken token;

        internal DwollaService(
            IServiceProvider serviceProvider,
            IDwollaClient dwollaClient,
            DwollaCredentials dwollaCredentials,
            Func<IServiceProvider, Task<DwollaToken>> initializeToken,
            Func<IServiceProvider, DwollaToken, Task> saveToken)
        {
            this.serviceProvider = serviceProvider;
            this.dwollaClient = dwollaClient;
            this.initializeToken = initializeToken;
            this.saveToken = saveToken;
            clientId = dwollaCredentials.ClientId;
            clientSecret = dwollaCredentials.ClientSecret;
        }

        private async Task<string> GetTokenAsync(bool force = false)
        {
            if (token == null && initializeToken != null)
            {
                token = await initializeToken(serviceProvider);
            }

            if (force || token?.AccessToken == null || token.Expiration <= DateTimeOffset.UtcNow)
            {
                var tokenResponse = await dwollaClient.PostAuthAsync<TokenResponse>(
                    "/token",
                    new AppTokenRequest
                    {
                        Key = clientId,
                        Secret = clientSecret
                    });

                if (tokenResponse.Error != null)
                {
                    throw new DwollaException(tokenResponse.Error);
                }

                DateTimeOffset responseDate = DateTimeOffset.UtcNow;
                // Try to get response header
                if (tokenResponse.Response.Headers.TryGetValues("Date", out IEnumerable<string> values) && values.Count() > 0)
                {
                    responseDate = DateTimeOffset.Parse(values.First());
                }

                // Save token in memory
                token = new DwollaToken
                {
                    AccessToken = tokenResponse.Content.Token,
                    Expiration = responseDate.AddSeconds(tokenResponse.Content.ExpiresIn)
                };

                // Save the token to DB if we can
                if (saveToken != null)
                {
                    await saveToken(serviceProvider, token);
                }
            }

            return token.AccessToken;
        }

        private async Task<TResponse> GetAsync<TResponse>(string url, bool forceTokenRefresh = false)
            where TResponse : IDwollaResponse
        {
            var response = await dwollaClient.GetAsync<TResponse>(
                url,
                new Headers { { "Authorization", $"Bearer {await GetTokenAsync(forceTokenRefresh)}" } });

            if (response.Error != null)
            {
                // Try to refresh the token once
                if (response.Error.Code == "ExpiredAccessToken" && forceTokenRefresh == false)
                {
                    return await GetAsync<TResponse>(url, true);
                }
                throw new DwollaException(response.Error);
            }

            return response.Content;
        }

        private async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest body, bool forceTokenRefresh = false)
            where TResponse : IDwollaResponse
        {
            var response = await dwollaClient.PostAsync<TRequest, TResponse>(
                url, body, new Headers { { "Authorization", $"Bearer {await GetTokenAsync(forceTokenRefresh)}" } });

            if (response.Error != null)
            {
                // Try to refresh the token once
                if (response.Error.Code == "ExpiredAccessToken" && forceTokenRefresh == false)
                {
                    return await PostAsync<TRequest, TResponse>(url, body, true);
                }
                throw new DwollaException(response.Error);
            }

            return response.Content;
        }

        private async Task<Uri> PostAsync<TRequest>(string url, TRequest body, bool forceTokenRefresh = false)
        {
            var response = await dwollaClient.PostAsync<TRequest, EmptyResponse>(url, body,
                new Headers { { "Authorization", $"Bearer {await GetTokenAsync(forceTokenRefresh)}" } });

            if (response.Error != null)
            {
                // Try to refresh the token once
                if (response.Error.Code == "ExpiredAccessToken" && forceTokenRefresh == false)
                {
                    return await PostAsync(url, body, true);
                }
                throw new DwollaException(response.Error);
            }

            return response.Response.Headers.Location;
        }

        private async Task<Uri> UploadAsync(string url, UploadDocumentRequest content, bool forceTokenRefresh = false)
        {
            var response = await dwollaClient.UploadAsync(
                url,
                content,
                new Headers { { "Authorization", $"Bearer {await GetTokenAsync()}" } });

            if (response.Error != null)
            {
                // Try to refresh the token once
                if (response.Error.Code == "ExpiredAccessToken" && forceTokenRefresh == false)
                {
                    return await UploadAsync(url, content, true);
                }
                throw new DwollaException(response.Error);
            }

            return response.Response.Headers.Location;
        }


        public Task<Customer> GetCustomerAsync(Guid customerId)
            => GetAsync<Customer>($"/customers/{customerId}");

        public Task<Uri> CreateCustomerAsync(CreateCustomerRequest customerRequest)
            => PostAsync("/customers", customerRequest);

        public Task<GetDocumentsResponse> GetCustomerDocumentsAsync(Guid customerId)
            => GetAsync<GetDocumentsResponse>($"/customers/{customerId}/documents");

        public Task<Uri> UploadCustomerDocumentAsync(Guid customerId, UploadDocumentRequest document)
            => UploadAsync($"/customers/{customerId}/documents", document);

        public Task<DocumentResponse> GetDocumentAsync(Guid documentId)
            => GetAsync<DocumentResponse>($"/documents/{documentId}");

        public Task<FundingSource> GetFundingSourceAsync(Guid fundingSourceId)
            => GetAsync<FundingSource>($"/funding-sources/{fundingSourceId}");

        public Task<BalanceResponse> GetFundingSourceBalanceAsync(Guid fundingSourceId)
            => GetAsync<BalanceResponse>($"/funding-sources/{fundingSourceId}/balance");

        public Task<TransferResponse> GetTransferAsync(Guid transferId)
            => GetAsync<TransferResponse>($"/transfers/{transferId}");

        public Task<Customer> UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest customerRequest)
            => PostAsync<UpdateCustomerRequest, Customer>($"/customers/{customerId}", customerRequest);

        public Task<BeneficialOwnerResponse> AddBeneficialOwner(Guid customerId, CreateBeneficialOwnerRequest createBeneficialOwnerRequest)
            => PostAsync<CreateBeneficialOwnerRequest, BeneficialOwnerResponse>($"/customers/{customerId}/beneficial-owners", createBeneficialOwnerRequest);
        public Task<BeneficialOwnershipResponse> CertifyBeneficialOwner(Guid customerId, CertifyBeneficialOwnershipRequest certifyBeneficialOwnershipRequest)
            => PostAsync<CertifyBeneficialOwnershipRequest, BeneficialOwnershipResponse>($"/customers/{customerId}/beneficial-ownership", certifyBeneficialOwnershipRequest);
    }
}
