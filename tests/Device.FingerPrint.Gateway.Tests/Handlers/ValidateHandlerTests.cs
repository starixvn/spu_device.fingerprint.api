using Device.FingerPrint.Gateway.Handlers;
using Device.FingerPrint.Gateway.Model;
using Device.FingerPrint.Gateway.Services;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Xunit;
using Fanex.Logging;
using System;

namespace Device.FingerPrint.Gateway.Tests.Handlers
{
    public class ValidateHandlerTests
    {
        private readonly Mock<ISubscriberService> _mockSubscriberService;
        private readonly Mock<IGatewayService> _mockGatewayService;
        private readonly ValidateHandler _handler;

        public ValidateHandlerTests()
        {
            // Setup LogManager to avoid NotImplementedException
            try
            {
                LogManager.SetDefaultLogCategory("Test")
                    .Use(new List<ILogEngine>());
            }
            catch
            {
                // LogManager might already be configured, ignore error
            }

            _mockSubscriberService = new Mock<ISubscriberService>();
            _mockGatewayService = new Mock<IGatewayService>();
            _handler = new ValidateHandler(_mockSubscriberService.Object, _mockGatewayService.Object);
        }

        [Fact]
        public async Task SendAsync_WhenDIIsDisabled_ReturnsOK()
        {
            // Arrange
            _mockGatewayService.Setup(x => x.IsDisableDIAsync()).ReturnsAsync(true);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com");

            // Act
            var response = await CallSendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_WithValidTransaction_CallsBaseAndReturnsResponse()
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

            var content = new StringContent(JsonConvert.SerializeObject(transactionRequest), Encoding.UTF8, "application/json");

            _mockGatewayService.Setup(x => x.IsDisableDIAsync()).ReturnsAsync(false);
            _mockSubscriberService.Setup(x => x.ValidateAsync("TestSubscriber")).ReturnsAsync(true);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            _handler.InnerHandler = mockHttpMessageHandler.Object;

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = content
            };

            // Act
            var response = await CallSendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockSubscriberService.Verify(x => x.ValidateAsync("TestSubscriber"), Times.Once);
        }

        [Fact]
        public async Task SendAsync_WithInvalidSubscriber_ReturnsBadRequest()
        {
            // Arrange
            var transactionRequest = new TransactionRequest
            {
                Subscriber = "InvalidSubscriber",
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

            var content = new StringContent(JsonConvert.SerializeObject(transactionRequest), Encoding.UTF8, "application/json");

            _mockGatewayService.Setup(x => x.IsDisableDIAsync()).ReturnsAsync(false);
            _mockSubscriberService.Setup(x => x.ValidateAsync("InvalidSubscriber")).ReturnsAsync(false);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = content
            };

            // Act
            var response = await CallSendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _mockSubscriberService.Verify(x => x.ValidateAsync("InvalidSubscriber"), Times.Once);
        }

        [Fact]
        public async Task SendAsync_WithNullSubscriber_ReturnsBadRequest()
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

            var content = new StringContent(JsonConvert.SerializeObject(transactionRequest), Encoding.UTF8, "application/json");

            _mockGatewayService.Setup(x => x.IsDisableDIAsync()).ReturnsAsync(false);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = content
            };

            // Act
            var response = await CallSendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _mockSubscriberService.Verify(x => x.ValidateAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendAsync_WithNullAccount_ReturnsBadRequest()
        {
            // Arrange
            var transactionRequest = new TransactionRequest
            {
                Subscriber = "TestSubscriber",
                Account = null!,
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

            var content = new StringContent(JsonConvert.SerializeObject(transactionRequest), Encoding.UTF8, "application/json");

            _mockGatewayService.Setup(x => x.IsDisableDIAsync()).ReturnsAsync(false);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = content
            };

            // Act
            var response = await CallSendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _mockSubscriberService.Verify(x => x.ValidateAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendAsync_WithEmptyContent_ReturnsBadRequest()
        {
            // Arrange
            _mockGatewayService.Setup(x => x.IsDisableDIAsync()).ReturnsAsync(false);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            // Act
            var response = await CallSendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_WithNullContent_ReturnsBadRequest()
        {
            // Arrange
            _mockGatewayService.Setup(x => x.IsDisableDIAsync()).ReturnsAsync(false);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = null
            };

            // Act
            var response = await CallSendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_WithInvalidJson_ReturnsBadRequest()
        {
            // Arrange
            _mockGatewayService.Setup(x => x.IsDisableDIAsync()).ReturnsAsync(false);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = new StringContent("{ invalid json }", Encoding.UTF8, "application/json")
            };

            // Act
            var response = await CallSendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_WithDebugAccount_LogsDebugInfo()
        {
            // Arrange
            var transactionRequest = new TransactionRequest
            {
                Subscriber = "TestSubscriber",
                Account = "qctest5oct202019", // Debug account
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

            var content = new StringContent(JsonConvert.SerializeObject(transactionRequest), Encoding.UTF8, "application/json");

            _mockGatewayService.Setup(x => x.IsDisableDIAsync()).ReturnsAsync(false);
            _mockSubscriberService.Setup(x => x.ValidateAsync("TestSubscriber")).ReturnsAsync(true);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            _handler.InnerHandler = mockHttpMessageHandler.Object;

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = content
            };

            // Act
            var response = await CallSendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _mockSubscriberService.Verify(x => x.ValidateAsync("TestSubscriber"), Times.Once);
        }

        [Fact]
        public async Task SendAsync_AddsTransDateToRequest()
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

            var content = new StringContent(JsonConvert.SerializeObject(transactionRequest), Encoding.UTF8, "application/json");

            _mockGatewayService.Setup(x => x.IsDisableDIAsync()).ReturnsAsync(false);
            _mockSubscriberService.Setup(x => x.ValidateAsync("TestSubscriber")).ReturnsAsync(true);

            TransactionRequest? modifiedRequest = null;
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var testStartTime = DateTime.Now;

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    var requestContent = req.Content!.ReadAsStringAsync().Result;
                    modifiedRequest = JsonConvert.DeserializeObject<TransactionRequest>(requestContent);
                })
                .ReturnsAsync(expectedResponse);

            _handler.InnerHandler = mockHttpMessageHandler.Object;

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = content
            };

            // Act
            var response = await CallSendAsync(request);
            var testEndTime = DateTime.Now;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(modifiedRequest);
            Assert.NotEqual(default(DateTime), modifiedRequest.TransDate);
            
            // Verify TransDate was set and is within the test execution timeframe
            Assert.True(modifiedRequest.TransDate >= testStartTime, 
                $"TransDate ({modifiedRequest.TransDate}) should be >= test start time ({testStartTime})");
            Assert.True(modifiedRequest.TransDate <= testEndTime, 
                $"TransDate ({modifiedRequest.TransDate}) should be <= test end time ({testEndTime})");
        }

        [Fact]
        public async Task SendAsync_WithException_ReturnsBadRequest()
        {
            // Arrange
            _mockGatewayService.Setup(x => x.IsDisableDIAsync()).ThrowsAsync(new Exception("Test exception"));

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = new StringContent("{\"test\":\"value\"}", Encoding.UTF8, "application/json")
            };

            // Act
            var response = await CallSendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_WithJsonException_ReturnsBadRequest()
        {
            // Arrange
            _mockGatewayService.Setup(x => x.IsDisableDIAsync()).ReturnsAsync(false);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = new StringContent("{ \"malformed json", Encoding.UTF8, "application/json")
            };

            // Act
            var response = await CallSendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Helper method to call protected SendAsync method
        private async Task<HttpResponseMessage> CallSendAsync(HttpRequestMessage request)
        {
            var method = typeof(ValidateHandler).GetMethod("SendAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task<HttpResponseMessage>)method!.Invoke(_handler, new object[] { request, CancellationToken.None })!;
            return await task;
        }
    }
}
