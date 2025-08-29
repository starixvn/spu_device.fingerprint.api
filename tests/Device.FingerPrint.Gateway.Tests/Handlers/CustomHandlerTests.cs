using Device.FingerPrint.Gateway.Handlers;
using Device.FingerPrint.Gateway.Model;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Xunit;

namespace Device.FingerPrint.Gateway.Tests.Handlers
{
    public class CustomHandlerTests
    {
        [Fact]
        public async Task SendAsync_WithValidTransactionRequest_AddsSubscriberHeader()
        {
            // Arrange
            var transactionRequest = new TransactionRequest
            {
                Subscriber = "TestSubscriber",
                Account = "TestAccount",
                Action = "TestAction",
                ActionResult = "Success",
                AcceptsTypes = "application/json",
                Platform = "Web",
                UserAgent = "Test",
                Screen = "1920x1080",
                Ip = "127.0.0.1",
                FakeIp = "",
                Url = "http://test.com",
                Di = "device-id",
                __di = "device-info",
                Bi = "browser-id",
                Token = "token",
                DetecasAnalysis = "analysis",
                CaptchaResult = "",
                SessionKey = "session"
            };

            var json = JsonConvert.SerializeObject(transactionRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            var customHandler = new CustomHandler
            {
                InnerHandler = mockHttpMessageHandler.Object
            };

            var httpClient = new HttpClient(customHandler);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = content
            };

            // Act
            var response = await httpClient.SendAsync(request);

            // Assert
            Assert.True(request.Headers.Contains("Subscriber"));
            Assert.Equal("TestSubscriber", request.Headers.GetValues("Subscriber").First());
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task SendAsync_WithNullSubscriber_AddsEmptySubscriberHeader()
        {
            // Arrange
            var transactionRequest = new TransactionRequest
            {
                Subscriber = null!,
                Account = "TestAccount",
                Action = "TestAction",
                ActionResult = "Success",
                AcceptsTypes = "application/json",
                Platform = "Web",
                UserAgent = "Test",
                Screen = "1920x1080",
                Ip = "127.0.0.1",
                FakeIp = "",
                Url = "http://test.com",
                Di = "device-id",
                __di = "device-info",
                Bi = "browser-id",
                Token = "token",
                DetecasAnalysis = "analysis",
                CaptchaResult = "",
                SessionKey = "session"
            };

            var json = JsonConvert.SerializeObject(transactionRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            var customHandler = new CustomHandler
            {
                InnerHandler = mockHttpMessageHandler.Object
            };

            var httpClient = new HttpClient(customHandler);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = content
            };

            // Act
            var response = await httpClient.SendAsync(request);

            // Assert
            Assert.True(request.Headers.Contains("Subscriber"));
            var subscriberValue = request.Headers.GetValues("Subscriber").First();
            Assert.True(string.IsNullOrEmpty(subscriberValue));
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task SendAsync_WithInvalidJson_ThrowsJsonException()
        {
            // Arrange
            var invalidJson = "{ invalid json }";
            var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            var customHandler = new CustomHandler
            {
                InnerHandler = mockHttpMessageHandler.Object
            };

            var httpClient = new HttpClient(customHandler);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = content
            };

            // Act & Assert
            await Assert.ThrowsAsync<JsonReaderException>(() => httpClient.SendAsync(request));
        }

        [Fact]
        public async Task SendAsync_WithEmptyContent_HandlesGracefully()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            var customHandler = new CustomHandler
            {
                InnerHandler = mockHttpMessageHandler.Object
            };

            var httpClient = new HttpClient(customHandler);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            // Act
            var response = await httpClient.SendAsync(request);

            // Assert
            Assert.True(request.Headers.Contains("Subscriber"));
            var subscriberValue = request.Headers.GetValues("Subscriber").First();
            Assert.True(string.IsNullOrEmpty(subscriberValue));
            Assert.Equal(expectedResponse, response);
        }
    }
}
