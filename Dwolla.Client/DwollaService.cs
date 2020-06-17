using System;
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

            if (force || token == null || token.Expiration <= DateTimeOffset.UtcNow)
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

                // Save the token to DB if we can
                if (saveToken != null)
                {
                    await saveToken(serviceProvider, token);
                }
            }

            return token.AccessToken;
        }

        private async Task<TResponse> GetAsync<TResponse>(string url)
            where TResponse : IDwollaResponse
        {
            var response = await dwollaClient.GetAsync<TResponse>(
                url,
                new Headers { { "Authorization", $"Bearer {await GetTokenAsync()}" } });

            if (response.Error != null)
            {
                throw new DwollaException(response.Error);
            }

            return response.Content;
        }

        private async Task<Uri> UploadAsync(string url, UploadDocumentRequest content)
        {
            var response = await dwollaClient.UploadAsync(
                url,
                content,
                new Headers { { "Authorization", $"Bearer {await GetTokenAsync()}" } });

            if (response.Error != null)
            {
                throw new DwollaException(response.Error);
            }

            return response.Response.Headers.Location;
        }

        public Task<Customer> GetCustomerAsync(Guid customerId)
            => GetAsync<Customer>($"/customers/{customerId}");

        public Task<GetDocumentsResponse> GetCustomerDocumentsAsync(Guid customerId)
            => GetAsync<GetDocumentsResponse>($"/customers/{customerId}/documents");

        public Task<Uri> UploadCustomerDocumentAsync(Guid customerId, UploadDocumentRequest document)
            => UploadAsync($"/customers/{customerId}/documents", document);

        public Task<DocumentResponse> GetDocumentAsync(Guid documentId)
            => GetAsync<DocumentResponse>($"/documents/{documentId}");

        public Task<FundingSource> GetFundingSourceAsync(Guid fundingSourceId)
            => GetAsync<FundingSource>($"funding-sources/{fundingSourceId}");

        public Task<BalanceResponse> GetFundingSourceBalanceAsync(Guid fundingSourceId)
            => GetAsync<BalanceResponse>($"funding-sources/{fundingSourceId}/balance");

        public Task<TransferResponse> GetTransferAsync(Guid transferId)
            => GetAsync<TransferResponse>($"/transfers/{transferId}");
    }
}
