using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Dwolla.Client.Models;
using Dwolla.Client.Models.Requests;
using Dwolla.Client.Models.Responses;
using Dwolla.Client.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

[assembly: InternalsVisibleTo("Dwolla.Client.Tests")]

namespace Dwolla.Client
{

    internal class DwollaClient
    {
        public Uri BaseAddress => _client.BaseAddress;

        private static readonly JsonSerializerSettings JsonSettings =
            new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

        private readonly RestClient _client;

        public DwollaClient(HttpClient httpClient) : this(new RestClient(CreateOrUpdateHttpClient(httpClient))) { }
        internal DwollaClient(RestClient client)
        {
            _client = client;
        }

        public async Task<RestResponse<TRes>> PostAuthAsync<TRes>(
            string uri, AppTokenRequest content) where TRes : IDwollaResponse
        {
            var formContentDict = new Dictionary<string, string> { { "grant_type", content.GrantType } };
            if (!string.IsNullOrEmpty(content.Key) && !string.IsNullOrEmpty(content.Secret))
            {
                formContentDict.Add("client_id", content.Key);
                formContentDict.Add("client_secret", content.Secret);
            }
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new FormUrlEncodedContent(formContentDict)
            };
            // Not adding this accept will result in a 401. For some reason.
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return await SendAsync<TRes>(request);
        }

        public async Task<RestResponse<TRes>> GetAsync<TRes>(
            string uri, Headers headers) where TRes : IDwollaResponse =>
            await SendAsync<TRes>(CreateRequest(HttpMethod.Get, uri, headers));

        public async Task<RestResponse<TRes>> PostAsync<TReq, TRes>(
            string uri, TReq content, Headers headers) where TRes : IDwollaResponse =>
            await SendAsync<TRes>(CreatePostRequest(uri, content, headers));

        public async Task<RestResponse<EmptyResponse>> UploadAsync(
            string uri, UploadDocumentRequest content, Headers headers) =>
            await SendAsync<EmptyResponse>(CreateUploadRequest(uri, content, headers));

        public async Task<RestResponse<TRes>> DeleteAsync<TRes>(string uri, Headers headers) =>
            await SendAsync<TRes>(CreateDeleteRequest(uri, headers));

        private async Task<RestResponse<TRes>> SendAsync<TRes>(HttpRequestMessage request) =>
            await _client.SendAsync<TRes>(request);

        private static HttpRequestMessage CreateDeleteRequest(
            string requestUri, Headers headers) =>
            CreateRequest(HttpMethod.Delete, requestUri, headers);

        private static HttpRequestMessage CreatePostRequest<TReq>(
            string requestUri, TReq content, Headers headers) =>
            CreateContentRequest(HttpMethod.Post, requestUri, headers, content);

        private static HttpRequestMessage CreateContentRequest<TReq>(
            HttpMethod method, string requestUri, Headers headers, TReq content)
        {
            var r = CreateRequest(method, requestUri, headers);
            r.Content = new StringContent(JsonConvert.SerializeObject(content, JsonSettings), Encoding.UTF8, Constants.ContentType);
            return r;
        }

        private static HttpRequestMessage CreateUploadRequest(string requestUri, UploadDocumentRequest content,
            Headers headers)
        {
            var r = CreateRequest(HttpMethod.Post, requestUri, headers);
            r.Content = new MultipartFormDataContent("----------Upload")
            {
                {new StringContent(content.DocumentType.ToString()), "\"documentType\""},
                GetFileContent(content.Document)
            };
            return r;
        }

        private static StreamContent GetFileContent(File file)
        {
            var fc = new StreamContent(file.Stream);
            fc.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
            fc.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"file\"",
                FileName = $"\"{file.Filename}\""
            };
            return fc;
        }

        private static HttpRequestMessage CreateRequest(HttpMethod method, string requestUri, Headers headers)
        {
            var r = new HttpRequestMessage(method, requestUri);
            foreach (var header in headers) r.Headers.Add(header.Key, header.Value);
            return r;
        }

        private static readonly string ClientVersion = typeof(DwollaClient).GetTypeInfo().Assembly
            .GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

        internal static HttpClient CreateOrUpdateHttpClient(HttpClient client = null)
        {
            client = client ?? new HttpClient(new HttpClientHandler { SslProtocols = SslProtocols.Tls12 });
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("dwolla-v2-csharp", ClientVersion));
            return client;
        }
    }
}
