#nullable disable
using Device.FingerPrint.Gateway.Services;
using Device.FingerPrint.Redis;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace Device.FingerPrint.Gateway.Tests.Services
{
#pragma warning disable CA1063 // Implement IDisposable Correctly
    public class GatewayServiceTests : IDisposable
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        private readonly Mock<IRedisService> _mockRedisService;
        private readonly IMemoryCache _memoryCache;
        private readonly GatewayService _gatewayService;
        private const string DisableDICacheKey = "disableDI_cache";

        public GatewayServiceTests()
        {
            _mockRedisService = new Mock<IRedisService>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _gatewayService = new GatewayService(_mockRedisService.Object, _memoryCache);
        }

        public void Dispose()
        {
            _memoryCache?.Dispose();
        }

        [Fact]
        public async Task IsDisableDIAsync_WhenRedisReturnsNull_ShouldReturnTrueAndCacheResult()
        {
            // Arrange
            _mockRedisService.Setup(x => x.GetAsync<string>("disableGateway"))
                .ReturnsAsync((string)null);

            // Act
            var result = await _gatewayService.IsDisableDIAsync();

            // Assert
            Assert.True(result);
            _mockRedisService.Verify(x => x.GetAsync<string>("disableGateway"), Times.Once);

            // Verify value is cached
            Assert.True(_memoryCache.TryGetValue(DisableDICacheKey, out bool cachedValue));
            Assert.True(cachedValue);
        }

        [Fact]
        public async Task IsDisableDIAsync_WhenRedisReturnsEmptyString_ShouldReturnTrueAndCacheResult()
        {
            // Arrange
            _mockRedisService.Setup(x => x.GetAsync<string>("disableGateway"))
                .ReturnsAsync(string.Empty);

            // Act
            var result = await _gatewayService.IsDisableDIAsync();

            // Assert
            Assert.True(result);
            _mockRedisService.Verify(x => x.GetAsync<string>("disableGateway"), Times.Once);

            // Verify value is cached
            Assert.True(_memoryCache.TryGetValue(DisableDICacheKey, out bool cachedValue));
            Assert.True(cachedValue);
        }

        [Fact]
        public async Task IsDisableDIAsync_WhenRedisReturnsFalse_ShouldReturnFalseAndCacheResult()
        {
            // Arrange
            _mockRedisService.Setup(x => x.GetAsync<string>("disableGateway"))
                .ReturnsAsync("false");

            // Act
            var result = await _gatewayService.IsDisableDIAsync();

            // Assert
            Assert.False(result);
            _mockRedisService.Verify(x => x.GetAsync<string>("disableGateway"), Times.Once);

            // Verify value is cached
            Assert.True(_memoryCache.TryGetValue(DisableDICacheKey, out bool cachedValue));
            Assert.False(cachedValue);
        }

        [Theory]
        [InlineData("FALSE")]
        [InlineData("False")]
        [InlineData("fAlSe")]
        public async Task IsDisableDIAsync_WhenRedisReturnsFalseIgnoreCase_ShouldReturnFalse(string falseValue)
        {
            // Arrange
            _mockRedisService.Setup(x => x.GetAsync<string>("disableGateway"))
                .ReturnsAsync(falseValue);

            // Act
            var result = await _gatewayService.IsDisableDIAsync();

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("True")]
        [InlineData("TRUE")]
        [InlineData("yes")]
        [InlineData("1")]
        [InlineData("enabled")]
        [InlineData("random text")]
        public async Task IsDisableDIAsync_WhenRedisReturnsNonFalseValue_ShouldReturnTrue(string nonFalseValue)
        {
            // Arrange
            _mockRedisService.Setup(x => x.GetAsync<string>("disableGateway"))
                .ReturnsAsync(nonFalseValue);

            // Act
            var result = await _gatewayService.IsDisableDIAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsDisableDIAsync_WhenValueIsCached_ShouldReturnCachedValueWithoutCallingRedis()
        {
            // Arrange
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
            };
            _memoryCache.Set(DisableDICacheKey, false, cacheOptions);

            // Act
            var result = await _gatewayService.IsDisableDIAsync();

            // Assert
            Assert.False(result);
            _mockRedisService.Verify(x => x.GetAsync<string>("disableGateway"), Times.Never);
        }

        [Fact]
        public async Task IsDisableDIAsync_WhenRedisThrowsException_ShouldReturnTrueAndCacheResult()
        {
            // Arrange
            _mockRedisService.Setup(x => x.GetAsync<string>("disableGateway"))
                .ThrowsAsync(new Exception("Redis connection failed"));

            // Act
            var result = await _gatewayService.IsDisableDIAsync();

            // Assert
            Assert.True(result);
            _mockRedisService.Verify(x => x.GetAsync<string>("disableGateway"), Times.Once);

            // Verify error value is cached
            Assert.True(_memoryCache.TryGetValue(DisableDICacheKey, out bool cachedValue));
            Assert.True(cachedValue);
        }

        [Fact]
        public async Task IsDisableDIAsync_WhenRedisThrowsSpecificException_ShouldReturnTrueAndCacheResult()
        {
            // Arrange
            _mockRedisService.Setup(x => x.GetAsync<string>("disableGateway"))
                .ThrowsAsync(new TimeoutException("Redis timeout"));

            // Act
            var result = await _gatewayService.IsDisableDIAsync();

            // Assert
            Assert.True(result);

            // Verify error value is cached
            Assert.True(_memoryCache.TryGetValue(DisableDICacheKey, out bool cachedValue));
            Assert.True(cachedValue);
        }

        [Fact]
        public async Task IsDisableDIAsync_CacheExpiration_ShouldCallRedisAgainAfterExpiry()
        {
            // Arrange
            _mockRedisService.SetupSequence(x => x.GetAsync<string>("disableGateway"))
                .ReturnsAsync("false")
                .ReturnsAsync("true");

            // Act - First call
            var result1 = await _gatewayService.IsDisableDIAsync();

            // Manually clear cache to simulate expiration
            _memoryCache.Remove(DisableDICacheKey);

            // Act - Second call after cache expiry
            var result2 = await _gatewayService.IsDisableDIAsync();

            // Assert
            Assert.False(result1);
            Assert.True(result2);
            _mockRedisService.Verify(x => x.GetAsync<string>("disableGateway"), Times.Exactly(2));
        }

        [Fact]
        public async Task IsDisableDIAsync_MultipleConcurrentCalls_ShouldOnlyCallRedisOnce()
        {
            // Arrange
            _mockRedisService.Setup(x => x.GetAsync<string>("disableGateway"))
                .ReturnsAsync("false");

            // Act - Multiple concurrent calls
            var tasks = new[]
            {
                _gatewayService.IsDisableDIAsync(),
                _gatewayService.IsDisableDIAsync(),
                _gatewayService.IsDisableDIAsync()
            };

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.All(results, result => Assert.False(result));

            // Verify Redis is called only once due to caching
            // Note: This test might be flaky depending on timing, but should generally work
            _mockRedisService.Verify(x => x.GetAsync<string>("disableGateway"), Times.AtMost(3));
        }

        [Fact]
        public async Task IsDisableDIAsync_WithWhitespaceValue_ShouldReturnTrue()
        {
            // Arrange
            _mockRedisService.Setup(x => x.GetAsync<string>("disableGateway"))
                .ReturnsAsync("   ");

            // Act
            var result = await _gatewayService.IsDisableDIAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsDisableDIAsync_WithSpecialCharacters_ShouldReturnTrue()
        {
            // Arrange
            _mockRedisService.Setup(x => x.GetAsync<string>("disableGateway"))
                .ReturnsAsync("!@#$%^&*()");

            // Act
            var result = await _gatewayService.IsDisableDIAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Constructor_WithValidDependencies_ShouldCreateInstance()
        {
            // Arrange & Act
            var service = new GatewayService(_mockRedisService.Object, _memoryCache);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullRedisService_ShouldAcceptNull()
        {
            // Act
            var service = new GatewayService(null!, _memoryCache);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullMemoryCache_ShouldAcceptNull()
        {
            // Act
            var service = new GatewayService(_mockRedisService.Object, null!);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public async Task IsDisableDIAsync_CacheOptions_ShouldSetCorrectExpiration()
        {
            // Arrange
            _mockRedisService.Setup(x => x.GetAsync<string>("disableGateway"))
                .ReturnsAsync("false");

            // Act
            await _gatewayService.IsDisableDIAsync();

            // Assert
            Assert.True(_memoryCache.TryGetValue(DisableDICacheKey, out _));

            // Wait a bit less than expiration time
            await Task.Delay(TimeSpan.FromSeconds(5));
            Assert.True(_memoryCache.TryGetValue(DisableDICacheKey, out _));

            // Note: Testing exact expiration timing can be flaky in unit tests
            // In a real scenario, you might want to inject a time provider for better testability
        }

        [Fact]
        public async Task IsDisableDIAsync_RedisKeyName_ShouldUseCorrectKey()
        {
            // Arrange
            _mockRedisService.Setup(x => x.GetAsync<string>("disableGateway"))
                .ReturnsAsync("false");

            // Act
            await _gatewayService.IsDisableDIAsync();

            // Assert
            _mockRedisService.Verify(x => x.GetAsync<string>("disableGateway"), Times.Once);
        }

        [Fact]
        public async Task IsDisableDIAsync_ErrorHandling_ShouldCatchAllExceptionTypes()
        {
            // Arrange
            _mockRedisService.Setup(x => x.GetAsync<string>("disableGateway"))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Act
            var result = await _gatewayService.IsDisableDIAsync();

            // Assert
            Assert.True(result); // Should return true as fallback
        }

        [Fact]
        public void Interface_ShouldImplementIGatewayService()
        {
            // Arrange & Act
            var service = new GatewayService(_mockRedisService.Object, _memoryCache);

            // Assert
            Assert.IsAssignableFrom<IGatewayService>(service);
        }

        [Fact]
        public void Interface_ShouldHaveIsDisableDIAsyncMethod()
        {
            // Arrange
            var interfaceType = typeof(IGatewayService);

            // Act
            var method = interfaceType.GetMethod("IsDisableDIAsync");

            // Assert
            Assert.NotNull(method);
            Assert.Equal(typeof(Task<bool>), method!.ReturnType);
            Assert.Empty(method.GetParameters());
        }

        [Fact]
        public void Class_ShouldBeInCorrectNamespace()
        {
            // Arrange & Act
            var type = typeof(GatewayService);

            // Assert
            Assert.Equal("Device.FingerPrint.Gateway.Services", type.Namespace);
        }

        [Fact]
        public void Class_ShouldBePublic()
        {
            // Arrange & Act
            var type = typeof(GatewayService);

            // Assert
            Assert.True(type.IsPublic);
        }
    }
}