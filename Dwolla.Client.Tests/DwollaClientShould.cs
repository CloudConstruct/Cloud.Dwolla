using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dwolla.Client.Models;
using Dwolla.Client.Models.Requests;
using Dwolla.Client.Models.Responses;
using Dwolla.Client.Rest;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;
using File = Dwolla.Client.Models.File;

namespace Dwolla.Client.Tests
{
    public class DwollaClientShould
    {
        private const string JsonV1 = "application/vnd.dwolla.v1.hal+json";
        private const string RequestId = "some-id";
        private const string UserAgent = "dwolla-v2-csharp/5.1.1";
        private const string RequestUri = "https://api-sandbox.dwolla.com/foo";
        private const string AuthRequestUri = "https://accounts-sandbox.dwolla.com/foo";
        private static readonly Headers Headers = new Headers { { "key1", "value1" }, { "key2", "value2" } };
        private static readonly TestRequest Request = new TestRequest { Message = "requestTest" };
        private static readonly TestResponse Response = new TestResponse { Message = "responseTest" };

        private readonly Mock<HttpMessageHandler> messageHandler;
        private readonly DwollaClient _client;

        public DwollaClientShould()
        {
            messageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _client = new DwollaClient(new HttpClient(messageHandler.Object));
        }

        [Fact]
        public void ConfigureHttpClient()
        {
            var client = DwollaClient.CreateOrUpdateHttpClient();
            Assert.Equal(UserAgent, client.DefaultRequestHeaders.UserAgent.ToString());
        }

        [Fact]
        public async void CreatePostAuthRequestAndPassToClient()
        {
            var response = CreateRestResponse(HttpMethod.Post, Response);
            var req = new AppTokenRequest { Key = "key", Secret = "secret" };
            var request = CreateAuthHttpRequest(req);
            messageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((y, _) => AppTokenCallback(request, y))
                .ReturnsAsync(response.Response)
                .Verifiable();

            var actual = await _client.PostAuthAsync<TestResponse>(AuthRequestUri, req);

            Assert.Equal(response.Response, actual.Response);
        }

        [Fact]
        public async void CreateGetRequestAndPassToClient()
        {
            var response = CreateRestResponse(HttpMethod.Get, Response);
            SetupForGet(CreateRequest(HttpMethod.Get), response);

            var actual = await _client.GetAsync<TestResponse>(RequestUri, Headers);

            Assert.Equal(response.Response, actual.Response);
        }

        [Fact]
        public async void CreatePostRequestAndPassToClient()
        {
            var response = CreateRestResponse(HttpMethod.Post, Response);
            SetupForPost(CreatePostRequest(), response);

            var actual = await _client.PostAsync<TestRequest, TestResponse>(RequestUri, Request, Headers);

            Assert.Equal(response.Response, actual.Response);
        }

        [Fact]
        public async void CreateUploadRequestAndPassToClient()
        {
            var request = CreateUploadRequest();
            var response = CreateRestResponse<EmptyResponse>(HttpMethod.Post);
            SetupForUpload(CreateUploadRequest(request), response);

            var actual = await _client.UploadAsync(RequestUri, request, Headers);

            Assert.Equal(response.Response, actual.Response);
        }

        [Fact]
        public async void CreateDeleteRequestAndPassToClient()
        {
            var response = CreateRestResponse<EmptyResponse>(HttpMethod.Delete);
            SetupForDelete(CreateDeleteRequest(), response);

            var actual = await _client.DeleteAsync<EmptyResponse>(RequestUri, Headers);

            Assert.Equal(response.Response, actual.Response);
        }

        private static HttpRequestMessage CreatePostRequest() => CreateContentRequest(HttpMethod.Post, Request);

        private static UploadDocumentRequest CreateUploadRequest() => new UploadDocumentRequest
        {
            DocumentType = DocumentType.IdCard,
            Document = new File
            {
                ContentType = "image/png",
                Filename = "test.png",
                Stream = Mock.Of<Stream>()
            }
        };

        private static HttpRequestMessage CreateUploadRequest(UploadDocumentRequest request)
        {
            var r = CreateRequest(HttpMethod.Post);
            r.Content = new MultipartFormDataContent("----------Upload")
            {
                {new StringContent(request.DocumentType.ToString()), "\"documentType\""},
                GetFileContent(request.Document)
            };
            return r;
        }

        private static HttpRequestMessage CreateDeleteRequest() =>
            CreateRequest(HttpMethod.Delete);

        private static HttpRequestMessage CreateContentRequest(HttpMethod method, TestRequest content)
        {
            var r = CreateRequest(method);
            r.Content = content != null
                ? new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, JsonV1)
                : null;
            return r;
        }

        private static HttpRequestMessage CreateRequest(HttpMethod method)
        {
            var r = new HttpRequestMessage(method, RequestUri);
            r.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonV1));
            foreach (var (key, value) in Headers) r.Headers.Add(key, value);
            return r;
        }

        private static RestResponse<T> CreateRestResponse<T>(
            HttpMethod method, T content = null, string rawContent = null) where T : class
        {
            var r = new HttpResponseMessage
            {
                RequestMessage = new HttpRequestMessage { RequestUri = new Uri(RequestUri), Method = method }
            };
            r.Headers.Add("x-request-id", RequestId);
            return new RestResponse<T>(r, content ?? Activator.CreateInstance<T>(), rawContent);
        }

        private static HttpRequestMessage CreateAuthHttpRequest(AppTokenRequest req) =>
            new HttpRequestMessage(HttpMethod.Post, AuthRequestUri)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"client_id", req.Key}, {"client_secret", req.Secret}, {"grant_type", req.GrantType}
                })
            };

        private void SetupForSend(HttpRequestMessage expected, HttpResponseMessage response,
            Action<HttpRequestMessage, HttpRequestMessage> callback) =>
            messageHandler.Protected()
               .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((y, _) => callback(expected, y))
                .ReturnsAsync(response)
                .Verifiable();

        private void SetupForGet(HttpRequestMessage expected, RestResponse<TestResponse> response) =>
            SetupForSend(expected, response.Response, GetCallback);

        private void SetupForPost<T>(HttpRequestMessage expected, RestResponse<T> response) =>
            SetupForSend(expected, response.Response, PostCallback);

        private void SetupForUpload(HttpRequestMessage expected, RestResponse<EmptyResponse> response) =>
            SetupForSend(expected, response.Response, UploadCallback);

        private void SetupForDelete(HttpRequestMessage expected, RestResponse<EmptyResponse> response) =>
            SetupForSend(expected, response.Response, DeleteCallback);

        private static async void PostCallback(HttpRequestMessage expected, HttpRequestMessage actual)
        {
            GetCallback(expected, actual);
            Assert.Equal("{\"message\":\"requestTest\"}", await actual.Content.ReadAsStringAsync());
            Assert.Equal("application/vnd.dwolla.v1.hal+json; charset=utf-8",
                actual.Content.Headers.ContentType.ToString());
        }

        private static async void UploadCallback(HttpRequestMessage expected, HttpRequestMessage actual)
        {
            GetCallback(expected, actual);
            var content = await actual.Content.ReadAsStringAsync();
            Assert.Contains("----------Upload", content);
            Assert.Contains("documentType", content);
            Assert.Contains("file", content);
            Assert.Equal("multipart/form-data; boundary=\"----------Upload\"",
                actual.Content.Headers.ContentType.ToString());
        }

        private static void DeleteCallback(HttpRequestMessage expected, HttpRequestMessage actual) =>
            GetCallback(expected, actual);

        private static void GetCallback(HttpRequestMessage expected, HttpRequestMessage actual)
        {
            Assert.Equal(expected.Method, actual.Method);
            Assert.Equal(expected.RequestUri, actual.RequestUri);
            foreach (var key in Headers.Keys) Assert.True(AssertHeader(expected, actual, key));
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static async void AppTokenCallback(HttpRequestMessage expected, HttpRequestMessage actual)
        {
            Assert.Equal(expected.Method, actual.Method);
            Assert.Equal(expected.RequestUri, actual.RequestUri);
            Assert.Equal(
                new Dictionary<string, string> { { "client_id", "key" }, { "client_secret", "secret" }, { "grant_type", "client_credentials" } },
                (await actual.Content.ReadAsStringAsync())
                    .Split("&")
                    .ToDictionary(kvp => kvp.Split("=")[0], kvp => kvp.Split("=")[1]));
            Assert.Equal("application/x-www-form-urlencoded", actual.Content.Headers.ContentType.ToString());
        }

        private static bool AssertHeader(HttpRequestMessage expected, HttpRequestMessage actual, string key) =>
            expected.Headers.GetValues(key).ToString() == actual.Headers.GetValues(key).ToString();

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

        private class TestRequest
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Message { get; set; }
        }

        private class TestResponse : IDwollaResponse
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Message { get; set; }
        }

    }
}
