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
            clientId = dwollaCredentials?.ClientId;
            clientSecret = dwollaCredentials?.ClientSecret;
        }

        #region Private

        private async Task<string> GetTokenAsync(bool force = false)
        {
            await singleton.WaitAsync().ConfigureAwait(false);
            if (token == null && fetchToken != null)
            {
                token = await fetchToken(serviceProvider).ConfigureAwait(false);
            }

            if (force || token?.AccessToken == null || token.Expiration <= DateTimeOffset.UtcNow)
            {
                var tokenResponse = await dwollaClient.PostAuthAsync<TokenResponse>(
                    "/token",
                    new AppTokenRequest
                    {
                        Key = clientId,
                        Secret = clientSecret
                    }).ConfigureAwait(false);

                if (tokenResponse.Error != null)
                {
                    throw new DwollaException(tokenResponse.Error);
                }

                DateTimeOffset responseDate = DateTimeOffset.UtcNow;
                // Try to get response header
                if (tokenResponse.Response.Headers.TryGetValues("Date", out IEnumerable<string> values) && values.Any())
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
                    await saveToken(serviceProvider, token).ConfigureAwait(false);
                }
            }
            singleton.Release();

            return token.AccessToken;
        }

        private async Task<TResponse> GetAsync<TResponse>(string url, Headers headers = null, bool forceTokenRefresh = false)
            where TResponse : IDwollaResponse
        {
            headers ??= new Headers();
            headers.Add("Authorization", $"Bearer {await GetTokenAsync(forceTokenRefresh).ConfigureAwait(false)}");

            var response = await dwollaClient.GetAsync<TResponse>(url, headers).ConfigureAwait(false);

            if (response.Error != null)
            {
                // Try to refresh the token once
                return response.Error.Code == "ExpiredAccessToken" && !forceTokenRefresh
                    ? await GetAsync<TResponse>(url, forceTokenRefresh: true).ConfigureAwait(false)
                    : throw new DwollaException(response.Error);
            }

            return response.Content;
        }

        private async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest body, Headers headers = null, bool forceTokenRefresh = false)
            where TResponse : IDwollaResponse
        {
            headers ??= new Headers();
            headers.Add("Authorization", $"Bearer {await GetTokenAsync(forceTokenRefresh).ConfigureAwait(false)}");

            var response = await dwollaClient.PostAsync<TRequest, TResponse>(
                url, body, headers).ConfigureAwait(false);

            if (response.Error != null)
            {
                // Try to refresh the token once
                return response.Error.Code == "ExpiredAccessToken" && !forceTokenRefresh
                    ? await PostAsync<TRequest, TResponse>(url, body, forceTokenRefresh: true).ConfigureAwait(false)
                    : throw new DwollaException(response.Error);
            }

            return response.Content;
        }

        private async Task<Uri> PostAsync<TRequest>(string url, TRequest body, Headers headers = null, bool forceTokenRefresh = false)
        {
            headers ??= new Headers();
            headers.Add("Authorization", $"Bearer {await GetTokenAsync(forceTokenRefresh).ConfigureAwait(false)}");

            var response = await dwollaClient.PostAsync<TRequest, EmptyResponse>(url, body, headers).ConfigureAwait(false);

            if (response.Error != null)
            {
                // Try to refresh the token once
                return response.Error.Code == "ExpiredAccessToken" && !forceTokenRefresh
                    ? await PostAsync(url, body, forceTokenRefresh: true).ConfigureAwait(false)
                    : throw new DwollaException(response.Error);
            }

            return response.Response.Headers.Location;
        }

        private async Task<Uri> PostAsync(string url, Headers headers = null, bool forceTokenRefresh = false)
        {
            headers ??= new Headers();
            headers.Add("Authorization", $"Bearer {await GetTokenAsync(forceTokenRefresh).ConfigureAwait(false)}");

            var response = await dwollaClient.PostAsync(url, headers).ConfigureAwait(false);

            if (response.Error != null)
            {
                // Try to refresh the token once
                return response.Error.Code == "ExpiredAccessToken" && !forceTokenRefresh
                    ? await PostAsync(url, headers, true).ConfigureAwait(false)
                    : throw new DwollaException(response.Error);
            }

            return response.Response.Headers.Location;
        }

        private async Task<Uri> UploadAsync(string url, UploadDocumentRequest content, Headers headers = null, bool forceTokenRefresh = false)
        {
            headers ??= new Headers();
            headers.Add("Authorization", $"Bearer {await GetTokenAsync(forceTokenRefresh).ConfigureAwait(false)}");

            var response = await dwollaClient.UploadAsync(url, content, headers).ConfigureAwait(false);

            if (response.Error != null)
            {
                // Try to refresh the token once
                return response.Error.Code == "ExpiredAccessToken" && !forceTokenRefresh
                    ? await UploadAsync(url, content, forceTokenRefresh: true).ConfigureAwait(false)
                    : throw new DwollaException(response.Error);
            }

            return response.Response.Headers.Location;
        }

        private async Task<TResponse> DeleteAsync<TResponse>(string url, Headers headers = null, bool forceTokenRefresh = false)
        {
            headers ??= new Headers();
            headers.Add("Authorization", $"Bearer {await GetTokenAsync(forceTokenRefresh).ConfigureAwait(false)}");

            var response = await dwollaClient.DeleteAsync<TResponse>(url, headers).ConfigureAwait(false);

            if (response.Error != null)
            {
                // Try to refresh the token once
                return response.Error.Code == "ExpiredAccessToken" && !forceTokenRefresh
                    ? await DeleteAsync<TResponse>(url, forceTokenRefresh: true).ConfigureAwait(false)
                    : throw new DwollaException(response.Error);
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
            => GetAsync<GetFundingSourcesResponse>($"/customers/{customerId}/funding-sources{(!includeRemoved ? "?removed=false" : string.Empty)}");

        public Task<IavTokenResponse> GetCustomerIavTokenAsync(Guid customerId) =>
            throw new NotImplementedException();

        #endregion

        public Task<DocumentResponse> GetDocumentAsync(Guid documentId)
            => GetAsync<DocumentResponse>($"/documents/{documentId}");

        #region Funding Sources

        public Task<Uri> CreateFundingSourceAsync(Guid customerId, string plaidToken, string name,
            Guid? onDemandAuthorization = null)
            => CreateFundingSourceAsync(customerId,
                new CreateFundingSourceRequest
                {
                    PlaidToken = plaidToken,
                    Name = name
                },
                onDemandAuthorization);

        public Task<Uri> CreateFundingSourceAsync(Guid customerId, string routingNumber, string accountNumber,
            BankAccountType bankAccountType, string name, string plaidToken = null,
            IEnumerable<string> channels = null, Guid? onDemandAuthorization = null)
            => CreateFundingSourceAsync(customerId,
                new CreateFundingSourceRequest
                {
                    RoutingNumber = routingNumber,
                    AccountNumber = accountNumber,
                    BankAccountType = bankAccountType,
                    Name = name,
                    Channels = channels
                },
                onDemandAuthorization);

        private Task<Uri> CreateFundingSourceAsync(Guid customerId, CreateFundingSourceRequest request,
            Guid? onDemandAuthorization)
        {
            if (onDemandAuthorization.HasValue)
            {
                request.Links["on-demand-authorization"] = new Link
                {
                    Href = new Uri($"{dwollaClient.BaseAddress}/on-demand-authorizations/{onDemandAuthorization.Value}")
                };
            }

            return PostAsync($"/customers/{customerId}/funding-sources", request);
        }

        public Task<FundingSource> GetFundingSourceAsync(Guid fundingSourceId)
            => GetAsync<FundingSource>($"/funding-sources/{fundingSourceId}");

        public Task<BalanceResponse> GetFundingSourceBalanceAsync(Guid fundingSourceId)
            => GetAsync<BalanceResponse>($"/funding-sources/{fundingSourceId}/balance");

        public Task<Uri> InitiateMicroDepositsAsync(Guid fundingSourceId)
            => PostAsync($"/funding-sources/{fundingSourceId}/micro-deposits");

        public Task<MicroDepositsResponse> GetMicroDepositsAsync(Guid fundingSourceId)
            => GetAsync<MicroDepositsResponse>($"/funding-sources/{fundingSourceId}/micro-deposits");

        public Task<BaseResponse> VerifyMicroDepositsAsync(Guid fundingSourceId, decimal amount1, decimal amount2)
            => PostAsync<MicroDepositsRequest, BaseResponse>($"/funding-sources/{fundingSourceId}/micro-deposits",
                new MicroDepositsRequest
                {
                    Amount1 = new Money
                    {
                        Currency = "USD",
                        Value = amount1
                    },
                    Amount2 = new Money
                    {
                        Currency = "USD",
                        Value = amount2
                    },
                });

        public Task<FundingSource> UpdateFundingSourceAsync(Guid fundingSourceId, string name)
            => PostAsync<UpdateFundingSourceRequest, FundingSource>($"/funding-sources/{fundingSourceId}",
                new UpdateFundingSourceRequest
                {
                    Name = name
                });

        public Task<FundingSource> RemoveFundingSourceAsync(Guid fundingSourceId)
            => PostAsync<RemoveFundingSourceRequest, FundingSource>($"/funding-sources/{fundingSourceId}", new RemoveFundingSourceRequest());

        #endregion

        public Task<TransferResponse> GetTransferAsync(Guid transferId)
            => GetAsync<TransferResponse>($"/transfers/{transferId}");

        public Task<TransferFailureResponse> GetTransferFailureAsync(Guid transferId)
            => throw new NotImplementedException();

        public Task<Uri> CreateTransferAsync(Guid sourceFundingSourceId, Guid destinationFundingSourceId,
            decimal amount, string idempotencyKey, decimal? fee = null, Guid? chargeTo = null,
            string sourceAddenda = null, string destinationAddenda = null, string correlationId = null, Clearing clearing = null)
            => PostAsync("/transfers",
                new CreateTransferRequest
                {
                    CorrelationId = correlationId,
                    Amount = new Money
                    {
                        Currency = "USD",
                        Value = amount
                    },
                    Links = new Dictionary<string, Link>
                    {
                        { "source", new Link { Href = new Uri($"{dwollaClient.BaseAddress}/funding-sources/{sourceFundingSourceId}") } },
                        { "destination", new Link { Href = new Uri($"{dwollaClient.BaseAddress}/funding-sources/{destinationFundingSourceId}") } }
                    },
                    Clearing = clearing,
                    Fees = fee == null || fee == 0m
                        ? null
                        : new List<Fee>
                        {
                            new Fee
                            {
                                Amount = new Money {Value = fee.Value, Currency = "USD"},
                                Links = new Dictionary<string, Link> {
                                    {
                                        "charge-to",
                                        new Link { Href = new Uri($"{dwollaClient.BaseAddress}/customers/{chargeTo}") }
                                    }
                                }
                            }
                        },
                    AchDetails = sourceAddenda == null && destinationAddenda == null
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
                },
                headers: new Headers { { "Idempotency-Key", idempotencyKey } });

        public Task<TransferResponse> CancelTransferAsync(Guid transferId)
            => PostAsync<TransferCancelRequest, TransferResponse>($"/transfers/{transferId}", new TransferCancelRequest());

        public Task<RootResponse> GetRootAsync()
            => GetAsync<RootResponse>(null);
    }
}
