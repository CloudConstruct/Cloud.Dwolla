using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Dwolla.Client.Models;
using Dwolla.Client.Models.Requests;
using Dwolla.Client.Models.Responses;

namespace Dwolla.Client
{
    public class DwollaService : IDwollaService
    {
        private readonly IServiceProvider serviceProvider;

        private readonly DwollaClient dwollaClient;

        private readonly string clientId;
        private readonly string clientSecret;
        private readonly Func<IServiceProvider, Task<DwollaToken>> fetchToken;

        private readonly Func<IServiceProvider, DwollaToken, Task> saveToken;

        private DwollaToken token;
        private readonly SemaphoreSlim singleton = new SemaphoreSlim(1, 1);

        public DwollaService(DwollaCredentials dwollaCredentials, Uri apiBaseUrl)
        {
            var httpClient = HttpClientFactory.Create();
            httpClient.BaseAddress = apiBaseUrl;
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ContentType));
            dwollaClient = new DwollaClient(httpClient);

            clientId = dwollaCredentials.ClientId;
            clientSecret = dwollaCredentials.ClientSecret;
        }

        internal DwollaService(
            DwollaClient dwollaClient,
            DwollaCredentials dwollaCredentials,
            IServiceProvider serviceProvider = null,
            Func<IServiceProvider, Task<DwollaToken>> fetchToken = null,
            Func<IServiceProvider, DwollaToken, Task> saveToken = null)
        {
            this.serviceProvider = serviceProvider;
            this.dwollaClient = dwollaClient;
            this.fetchToken = fetchToken;
            this.saveToken = saveToken;
            clientId = dwollaCredentials.ClientId;
            clientSecret = dwollaCredentials.ClientSecret;
        }

        #region Private

        private async Task<string> GetTokenAsync(bool force = false)
        {
            await singleton.WaitAsync();
            if (token == null && fetchToken != null)
            {
                token = await fetchToken(serviceProvider);
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
            singleton.Release();

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

        private async Task<TResponse> DeleteAsync<TResponse>(string url, bool forceTokenRefresh = false)
        {
            var response = await dwollaClient.DeleteAsync<TResponse>(url,
                new Headers { { "Authorization", $"Bearer {await GetTokenAsync()}" } });

            if (response.Error != null)
            {
                // Try to refresh the token once
                if (response.Error.Code == "ExpiredAccessToken" && forceTokenRefresh == false)
                {
                    return await DeleteAsync<TResponse>(url, true);
                }
                throw new DwollaException(response.Error);
            }

            return response.Content;
        }

        #endregion

        public Task<GetEventsResponse> GetEventsAsync()
            => throw new NotImplementedException();

        public Task<GetBusinessClassificationsResponse> GetBusinessClassificationsAsync()
            => throw new NotImplementedException();

        #region Webhooks

        public Task<Uri> CreateWebhookSubscriptionAsync(string url, string secret)
            => PostAsync("/webhook-subscriptions", new CreateWebhookSubscriptionRequest { Url = url, Secret = secret });

        public Task<WebhookSubscription> GetWebhookSubscriptionAsync(Guid webhookSubscriptionId)
            => GetAsync<WebhookSubscription>($"/webhook-subscriptions/{webhookSubscriptionId}");

        public Task<GetWebhookSubscriptionsResponse> GetWebhookSubscriptionsAsync()
            => GetAsync<GetWebhookSubscriptionsResponse>("/webhook-subscriptions");

        public Task<WebhookSubscription> DeleteWebhookSubscriptionAsync(Guid webhookSubscriptionId)
            => DeleteAsync<WebhookSubscription>($"/webhook-subscriptions/{webhookSubscriptionId}");

        #endregion

        #region Customers

        public Task<GetCustomersResponse> GetCustomersAsync()
            => GetAsync<GetCustomersResponse>("/customers");

        public Task<Customer> GetCustomerAsync(Guid customerId)
            => GetAsync<Customer>($"/customers/{customerId}");

        public Task<Uri> CreateCustomerAsync(CreateCustomerRequest customerRequest)
            => PostAsync("/customers", customerRequest);

        public Task<GetDocumentsResponse> GetCustomerDocumentsAsync(Guid customerId)
            => GetAsync<GetDocumentsResponse>($"/customers/{customerId}/documents");

        public Task<Uri> UploadDocumentAsync(Guid customerId, UploadDocumentRequest document)
            => UploadAsync($"/customers/{customerId}/documents", document);

        public Task<Customer> UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest customerRequest)
            => PostAsync<UpdateCustomerRequest, Customer>($"/customers/{customerId}", customerRequest);

        public Task<Uri> CreateBeneficialOwnerAsync(Guid customerId, CreateBeneficialOwnerRequest createBeneficialOwnerRequest)
            => PostAsync<CreateBeneficialOwnerRequest>($"/customers/{customerId}/beneficial-owners", createBeneficialOwnerRequest);

        public Task<BeneficialOwnershipResponse> CertifyBeneficialOwnershipAsync(Guid customerId)
            => PostAsync<CertifyBeneficialOwnershipRequest, BeneficialOwnershipResponse>($"/customers/{customerId}/beneficial-ownership",
                new CertifyBeneficialOwnershipRequest { Status = "certified" });

        public Task<BeneficialOwnershipResponse> GetBeneficialOwnershipAsync(Guid customerId)
            => throw new NotImplementedException();

        public Task<GetBeneficialOwnersResponse> GetBeneficialOwnersAsync(Guid customerId)
            => throw new NotImplementedException();

        public Task<BeneficialOwnerResponse> GetBeneficialOwnerAsync(Guid beneficialOwnerId)
            => throw new NotImplementedException();

        public Task<BeneficialOwnerResponse> DeleteBeneficialOwnerAsync(Guid beneficialOwnerId)
            => throw new NotImplementedException();

        public Task<GetFundingSourcesResponse> GetCustomerFundingSourcesAsync(Guid customerId, bool includeRemoved = true)
            => GetAsync<GetFundingSourcesResponse>($"/customers/{customerId}/funding-sources{(includeRemoved == false ? "?removed=false" : string.Empty)}");

        public Task<IavTokenResponse> GetCustomerIavTokenAsync(Guid customerId) =>
            throw new NotImplementedException();

        #endregion

        public Task<DocumentResponse> GetDocumentAsync(Guid documentId)
            => GetAsync<DocumentResponse>($"/documents/{documentId}");

        #region Funding Sources

        public Task<FundingSource> GetFundingSourceAsync(Guid fundingSourceId)
            => GetAsync<FundingSource>($"/funding-sources/{fundingSourceId}");

        public Task<BalanceResponse> GetFundingSourceBalanceAsync(Guid fundingSourceId)
            => GetAsync<BalanceResponse>($"/funding-sources/{fundingSourceId}/balance");

        public Task<MicroDepositsResponse> GetMicroDepositsAsync(Guid fundingSourceId)
            => throw new NotImplementedException();

        public Task<Uri> VerifyMicroDepositsAsync(Guid fundingSourceId, decimal amount1, decimal amount2)
            => throw new NotImplementedException();

        #endregion

        public Task<TransferResponse> GetTransferAsync(Guid transferId)
            => GetAsync<TransferResponse>($"/transfers/{transferId}");

        public Task<TransferFailureResponse> GetTransferFailureAsync(Guid transferId)
            => throw new NotImplementedException();

        public Task<Uri> CreateTransferAsync(Guid sourceFundingSourceId, Guid destinationFundingSourceId,
            decimal amount, decimal? fee, Uri chargeTo, string sourceAddenda, string destinationAddenda)
            => PostAsync($"/transfers",
                new CreateTransferRequest
                {
                    Amount = new Money
                    {
                        Currency = "USD",
                        Value = amount
                    },
                    Links = new Dictionary<string, Link>
                    {
                        {"source", new Link { Href = new Uri($"{dwollaClient.BaseAddress}/funding-sources/{sourceFundingSourceId}") }},
                        {"destination", new Link { Href = new Uri($"{dwollaClient.BaseAddress}/funding-sources/{destinationFundingSourceId}") }}
                    },
                    Fees = fee == null || fee == 0m
                        ? null
                        : new List<Fee>
                        {
                            new Fee
                            {
                                Amount = new Money {Value = fee.Value, Currency = "USD"},
                                Links = new Dictionary<string, Link> {{"charge-to", new Link {Href = chargeTo}}}
                            }
                        },
                    AchDetails = sourceAddenda == null || destinationAddenda == null
                        ? null
                        : new AchDetails
                        {
                            Source = new SourceAddenda
                            {
                                Addenda = new Addenda
                                {
                                    Values = new List<string> { sourceAddenda }
                                }
                            },
                            Destination = new DestinationAddenda
                            {
                                Addenda = new Addenda
                                {
                                    Values = new List<string> { destinationAddenda }
                                }
                            }
                        }
                });

        public Task<RootResponse> GetRootAsync()
            => GetAsync<RootResponse>(null);
    }
}
