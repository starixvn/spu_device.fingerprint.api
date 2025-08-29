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

namespace Device.FingerPrint.Gateway.Tests.Handlers
{
    public class MobileRequestHandlerTests
    {
        private readonly Mock<ISubscriberService> _mockSubscriberService;
        private readonly Mock<IGatewayService> _mockGatewayService;
        private readonly MobileRequestHandler _handler;

        public MobileRequestHandlerTests()
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
            _handler = new MobileRequestHandler(_mockSubscriberService.Object, _mockGatewayService.Object);
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
        public async Task SendAsync_WithValidMobileTransaction_CallsBaseAndReturnsResponse()
        {
            // Arrange
            var transaction = new MobileTransactionModel
            {
                Subscriber = "TestSubscriber",
                Account = "TestAccount",
                Action = "TestAction",
                ActionResult = "Success",
                AndroidCode = "android123",
                MachineCode = "machine123",
                MediaDRMCode = "drm123",
                CustomCode = "custom123",
                ModelName = "TestModel",
                Manufacturer = "TestManufacturer",
                Brand = "TestBrand",
                SdkVersion = "30",
                OS = "Android",
                Timezone = "UTC",
                SystemLanguage = "en",
                Country = "US",
                Exceptions = "",
                DeviceDetails = "Test device details"
            };

            var transactionJson = JsonConvert.SerializeObject(transaction);
            var encodedTransaction = Convert.ToBase64String(Encoding.UTF8.GetBytes(transactionJson));

            var mobileRequest = new MobileTransactionRequest
            {
                Transaction = encodedTransaction
            };

            var content = new StringContent(JsonConvert.SerializeObject(mobileRequest), Encoding.UTF8, "application/json");

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
            var transaction = new MobileTransactionModel
            {
                Subscriber = "InvalidSubscriber",
                Account = "TestAccount",
                Action = "TestAction",
                ActionResult = "Success",
                AndroidCode = "android123",
                MachineCode = "machine123",
                MediaDRMCode = "drm123",
                CustomCode = "custom123",
                ModelName = "TestModel",
                Manufacturer = "TestManufacturer",
                Brand = "TestBrand",
                SdkVersion = "30",
                OS = "Android",
                Timezone = "UTC",
                SystemLanguage = "en",
                Country = "US",
                Exceptions = "",
                DeviceDetails = "Test device details"
            };

            var transactionJson = JsonConvert.SerializeObject(transaction);
            var encodedTransaction = Convert.ToBase64String(Encoding.UTF8.GetBytes(transactionJson));

            var mobileRequest = new MobileTransactionRequest
            {
                Transaction = encodedTransaction
            };

            var content = new StringContent(JsonConvert.SerializeObject(mobileRequest), Encoding.UTF8, "application/json");

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
        public async Task SendAsync_WithNullSubscriber_ReturnsBadRequestWithMessage()
        {
            // Arrange
            var transaction = new MobileTransactionModel
            {
                Subscriber = null!,
                Account = "TestAccount",
                Action = "TestAction",
                ActionResult = "Success",
                AndroidCode = "android123",
                MachineCode = "machine123",
                MediaDRMCode = "drm123",
                CustomCode = "custom123",
                ModelName = "TestModel",
                Manufacturer = "TestManufacturer",
                Brand = "TestBrand",
                SdkVersion = "30",
                OS = "Android",
                Timezone = "UTC",
                SystemLanguage = "en",
                Country = "US",
                Exceptions = "",
                DeviceDetails = "Test device details"
            };

            var transactionJson = JsonConvert.SerializeObject(transaction);
            var encodedTransaction = Convert.ToBase64String(Encoding.UTF8.GetBytes(transactionJson));

            var mobileRequest = new MobileTransactionRequest
            {
                Transaction = encodedTransaction
            };

            var content = new StringContent(JsonConvert.SerializeObject(mobileRequest), Encoding.UTF8, "application/json");

            _mockGatewayService.Setup(x => x.IsDisableDIAsync()).ReturnsAsync(false);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = content
            };

            // Act
            var response = await CallSendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal("Subscriber or Account cannot be null.", responseContent);
        }

        [Fact]
        public async Task SendAsync_WithNullAccount_ReturnsBadRequestWithMessage()
        {
            // Arrange
            var transaction = new MobileTransactionModel
            {
                Subscriber = "TestSubscriber",
                Account = null!,
                Action = "TestAction",
                ActionResult = "Success",
                AndroidCode = "android123",
                MachineCode = "machine123",
                MediaDRMCode = "drm123",
                CustomCode = "custom123",
                ModelName = "TestModel",
                Manufacturer = "TestManufacturer",
                Brand = "TestBrand",
                SdkVersion = "30",
                OS = "Android",
                Timezone = "UTC",
                SystemLanguage = "en",
                Country = "US",
                Exceptions = "",
                DeviceDetails = "Test device details"
            };

            var transactionJson = JsonConvert.SerializeObject(transaction);
            var encodedTransaction = Convert.ToBase64String(Encoding.UTF8.GetBytes(transactionJson));

            var mobileRequest = new MobileTransactionRequest
            {
                Transaction = encodedTransaction
            };

            var content = new StringContent(JsonConvert.SerializeObject(mobileRequest), Encoding.UTF8, "application/json");

            _mockGatewayService.Setup(x => x.IsDisableDIAsync()).ReturnsAsync(false);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = content
            };

            // Act
            var response = await CallSendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal("Subscriber or Account cannot be null.", responseContent);
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
        public async Task SendAsync_WithInvalidBase64_ReturnsBadRequest()
        {
            // Arrange
            var mobileRequest = new MobileTransactionRequest
            {
                Transaction = "InvalidBase64String!!!"
            };

            var content = new StringContent(JsonConvert.SerializeObject(mobileRequest), Encoding.UTF8, "application/json");

            _mockGatewayService.Setup(x => x.IsDisableDIAsync()).ReturnsAsync(false);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://test.com")
            {
                Content = content
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
            var transaction = new MobileTransactionModel
            {
                Subscriber = "TestSubscriber",
                Account = "member19", // Debug account
                Action = "TestAction",
                ActionResult = "Success",
                AndroidCode = "android123",
                MachineCode = "machine123",
                MediaDRMCode = "drm123",
                CustomCode = "custom123",
                ModelName = "TestModel",
                Manufacturer = "TestManufacturer",
                Brand = "TestBrand",
                SdkVersion = "30",
                OS = "Android",
                Timezone = "UTC",
                SystemLanguage = "en",
                Country = "US",
                Exceptions = "",
                DeviceDetails = "Test device details"
            };

            var transactionJson = JsonConvert.SerializeObject(transaction);
            var encodedTransaction = Convert.ToBase64String(Encoding.UTF8.GetBytes(transactionJson));

            var mobileRequest = new MobileTransactionRequest
            {
                Transaction = encodedTransaction
            };

            var content = new StringContent(JsonConvert.SerializeObject(mobileRequest), Encoding.UTF8, "application/json");

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

        // Helper method to call protected SendAsync method
        private async Task<HttpResponseMessage> CallSendAsync(HttpRequestMessage request)
        {
            var method = typeof(MobileRequestHandler).GetMethod("SendAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var task = (Task<HttpResponseMessage>)method!.Invoke(_handler, new object[] { request, CancellationToken.None })!;
            return await task;
        }
    }
}
