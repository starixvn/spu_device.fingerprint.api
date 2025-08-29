using Device.FingerPrint.Gateway.Handlers;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace Device.FingerPrint.Gateway.Tests.Handlers
{
    public class IPCaptureHandlerTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<HttpRequest> _mockHttpRequest;
        private readonly Mock<ConnectionInfo> _mockConnectionInfo;

        public IPCaptureHandlerTests()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockConnectionInfo = new Mock<ConnectionInfo>();
        }

        [Fact]
        public async Task SendAsync_WithXForwardedForHeader_AddsCorrectFpsHeader()
        {
            // Arrange
            var xForwardedFor = "192.168.1.1, 10.0.0.1";
            var headerDictionary = new HeaderDictionary
            {
                ["X-Forwarded-For"] = xForwardedFor
            };

            _mockHttpRequest.Setup(r => r.Headers).Returns(headerDictionary);
            _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(_mockHttpContext.Object);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            var handler = new IPCaptureHandler(_mockHttpContextAccessor.Object)
            {
                InnerHandler = mockHttpMessageHandler.Object
            };

            var httpClient = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

            // Act
            var response = await httpClient.SendAsync(request);

            // Assert
            Assert.True(request.Headers.Contains("Fps-Forwarded-For"));
            Assert.Equal(xForwardedFor, request.Headers.GetValues("Fps-Forwarded-For").First());
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task SendAsync_WithoutXForwardedForButWithRemoteIP_AddsRemoteIPToFpsHeader()
        {
            // Arrange
            var remoteIpAddress = IPAddress.Parse("203.0.113.1");
            var headerDictionary = new HeaderDictionary(); // Empty headers

            _mockHttpRequest.Setup(r => r.Headers).Returns(headerDictionary);
            _mockConnectionInfo.Setup(c => c.RemoteIpAddress).Returns(remoteIpAddress);
            _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
            _mockHttpContext.Setup(c => c.Connection).Returns(_mockConnectionInfo.Object);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(_mockHttpContext.Object);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            var handler = new IPCaptureHandler(_mockHttpContextAccessor.Object)
            {
                InnerHandler = mockHttpMessageHandler.Object
            };

            var httpClient = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

            // Act
            var response = await httpClient.SendAsync(request);

            // Assert
            Assert.True(request.Headers.Contains("Fps-Forwarded-For"));
            Assert.Equal("203.0.113.1", request.Headers.GetValues("Fps-Forwarded-For").First());
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task SendAsync_WithoutAnyIPInfo_DoesNotAddFpsHeader()
        {
            // Arrange
            var headerDictionary = new HeaderDictionary(); // Empty headers

            _mockHttpRequest.Setup(r => r.Headers).Returns(headerDictionary);
            _mockConnectionInfo.Setup(c => c.RemoteIpAddress).Returns((IPAddress?)null);
            _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
            _mockHttpContext.Setup(c => c.Connection).Returns(_mockConnectionInfo.Object);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(_mockHttpContext.Object);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            var handler = new IPCaptureHandler(_mockHttpContextAccessor.Object)
            {
                InnerHandler = mockHttpMessageHandler.Object
            };

            var httpClient = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

            // Act
            var response = await httpClient.SendAsync(request);

            // Assert
            Assert.False(request.Headers.Contains("Fps-Forwarded-For"));
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task SendAsync_WithNullHttpContext_DoesNotAddFpsHeader()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            var handler = new IPCaptureHandler(_mockHttpContextAccessor.Object)
            {
                InnerHandler = mockHttpMessageHandler.Object
            };

            var httpClient = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

            // Act
            var response = await httpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(request.Headers.Contains("Fps-Forwarded-For"));
        }

        [Fact]
        public async Task SendAsync_WithNullRequest_ReturnsBadRequest()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var handler = new IPCaptureHandler(_mockHttpContextAccessor.Object)
            {
                InnerHandler = mockHttpMessageHandler.Object
            };

            var httpClient = new HttpClient(handler);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => httpClient.SendAsync(null!));
        }

        [Fact]
        public async Task SendAsync_XForwardedForTakesPrecedenceOverRemoteIP()
        {
            // Arrange
            var xForwardedFor = "192.168.1.100";
            var remoteIpAddress = IPAddress.Parse("203.0.113.1");
            var headerDictionary = new HeaderDictionary
            {
                ["X-Forwarded-For"] = xForwardedFor
            };

            _mockHttpRequest.Setup(r => r.Headers).Returns(headerDictionary);
            _mockConnectionInfo.Setup(c => c.RemoteIpAddress).Returns(remoteIpAddress);
            _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
            _mockHttpContext.Setup(c => c.Connection).Returns(_mockConnectionInfo.Object);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(_mockHttpContext.Object);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            var handler = new IPCaptureHandler(_mockHttpContextAccessor.Object)
            {
                InnerHandler = mockHttpMessageHandler.Object
            };

            var httpClient = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

            // Act
            var response = await httpClient.SendAsync(request);

            // Assert
            Assert.True(request.Headers.Contains("Fps-Forwarded-For"));
            Assert.Equal(xForwardedFor, request.Headers.GetValues("Fps-Forwarded-For").First());
            Assert.NotEqual("203.0.113.1", request.Headers.GetValues("Fps-Forwarded-For").First());
            Assert.Equal(expectedResponse, response);
        }
    }
}
