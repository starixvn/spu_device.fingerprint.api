using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Device.FingerPrint.Gateway.Tests
{
    public class AppSettingsTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;

        public AppSettingsTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
        }

        [Fact]
        public void Constructor_WithValidConfiguration_ShouldInitializeAllProperties()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["IsUM"]).Returns("true");
            _mockConfiguration.Setup(x => x["Sentry:Dsn"]).Returns("https://test@sentry.io/123456");
            _mockConfiguration.Setup(x => x["GetSubscriberUrl"]).Returns("https://api.example.com/subscriber");
            _mockConfiguration.Setup(x => x["Logstash:Url"]).Returns("https://logstash.example.com");
            _mockConfiguration.Setup(x => x["Environment"]).Returns("Development");

            // Act
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Assert
            Assert.True(appSettings.IsUM);
            Assert.Equal("https://test@sentry.io/123456", appSettings.SentryDsn);
            Assert.Equal("https://api.example.com/subscriber", appSettings.GetSubscriberInfo);
            Assert.Equal("https://logstash.example.com", appSettings.LogstashUrl);
            Assert.Equal("Development", appSettings.Environment);
        }

        [Fact]
        public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AppSettings(null!));
        }

        [Fact]
        public void IsUM_Property_ShouldReturnTrueWhenConfigurationIsTrue()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["IsUM"]).Returns("true");
            _mockConfiguration.Setup(x => x["Sentry:Dsn"]).Returns("test-dsn");
            _mockConfiguration.Setup(x => x["GetSubscriberUrl"]).Returns("test-url");
            _mockConfiguration.Setup(x => x["Logstash:Url"]).Returns("test-logstash");
            _mockConfiguration.Setup(x => x["Environment"]).Returns("test-env");

            // Act
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Assert
            Assert.True(appSettings.IsUM);
        }

        [Fact]
        public void IsUM_Property_ShouldReturnFalseWhenConfigurationIsFalse()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["IsUM"]).Returns("false");
            SetupOtherConfigurationValues();

            // Act
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Assert
            Assert.False(appSettings.IsUM);
        }

        [Theory]
        [InlineData("TRUE")]
        [InlineData("True")]
        [InlineData("1")]
        public void IsUM_Property_ShouldReturnTrueForVariousTrueValues(string value)
        {
            // Arrange
            _mockConfiguration.Setup(x => x["IsUM"]).Returns(value);
            _mockConfiguration.Setup(x => x["Sentry:Dsn"]).Returns("test-dsn");
            _mockConfiguration.Setup(x => x["GetSubscriberUrl"]).Returns("test-url");
            _mockConfiguration.Setup(x => x["Logstash:Url"]).Returns("test-logstash");
            _mockConfiguration.Setup(x => x["Environment"]).Returns("test-env");

            // Act
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Assert
            Assert.True(appSettings.IsUM);
        }

        [Theory]
        [InlineData("FALSE")]
        [InlineData("False")]
        [InlineData("0")]
        public void IsUM_Property_ShouldReturnFalseForVariousFalseValues(string value)
        {
            // Arrange
            _mockConfiguration.Setup(x => x["IsUM"]).Returns(value);
            _mockConfiguration.Setup(x => x["Sentry:Dsn"]).Returns("test-dsn");
            _mockConfiguration.Setup(x => x["GetSubscriberUrl"]).Returns("test-url");
            _mockConfiguration.Setup(x => x["Logstash:Url"]).Returns("test-logstash");
            _mockConfiguration.Setup(x => x["Environment"]).Returns("test-env");

            // Act
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Assert
            Assert.False(appSettings.IsUM);
        }

        [Fact]
        public void SentryDsn_Property_ShouldReturnConfigurationValue()
        {
            // Arrange
            var expectedDsn = "https://user:password@sentry.io/project";
            _mockConfiguration.Setup(x => x["IsUM"]).Returns("false");
            _mockConfiguration.Setup(x => x["Sentry:Dsn"]).Returns(expectedDsn);
            _mockConfiguration.Setup(x => x["GetSubscriberUrl"]).Returns("test-url");
            _mockConfiguration.Setup(x => x["Logstash:Url"]).Returns("test-logstash");
            _mockConfiguration.Setup(x => x["Environment"]).Returns("test-env");

            // Act
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Assert
            Assert.Equal(expectedDsn, appSettings.SentryDsn);
        }

        [Fact]
        public void GetSubscriberInfo_Property_ShouldReturnConfigurationValue()
        {
            // Arrange
            var expectedUrl = "https://api.example.com/subscriber/info";
            _mockConfiguration.Setup(x => x["IsUM"]).Returns("false");
            _mockConfiguration.Setup(x => x["Sentry:Dsn"]).Returns("test-dsn");
            _mockConfiguration.Setup(x => x["GetSubscriberUrl"]).Returns(expectedUrl);
            _mockConfiguration.Setup(x => x["Logstash:Url"]).Returns("test-logstash");
            _mockConfiguration.Setup(x => x["Environment"]).Returns("test-env");

            // Act
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Assert
            Assert.Equal(expectedUrl, appSettings.GetSubscriberInfo);
        }

        [Fact]
        public void LogstashUrl_Property_ShouldReturnConfigurationValue()
        {
            // Arrange
            var expectedUrl = "https://logstash.example.com:5044";
            _mockConfiguration.Setup(x => x["IsUM"]).Returns("false");
            _mockConfiguration.Setup(x => x["Sentry:Dsn"]).Returns("test-dsn");
            _mockConfiguration.Setup(x => x["GetSubscriberUrl"]).Returns("test-url");
            _mockConfiguration.Setup(x => x["Logstash:Url"]).Returns(expectedUrl);
            _mockConfiguration.Setup(x => x["Environment"]).Returns("test-env");

            // Act
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Assert
            Assert.Equal(expectedUrl, appSettings.LogstashUrl);
        }

        [Fact]
        public void Environment_Property_ShouldReturnConfigurationValue()
        {
            // Arrange
            var expectedEnvironment = "Production";
            _mockConfiguration.Setup(x => x["IsUM"]).Returns("false");
            _mockConfiguration.Setup(x => x["Sentry:Dsn"]).Returns("test-dsn");
            _mockConfiguration.Setup(x => x["GetSubscriberUrl"]).Returns("test-url");
            _mockConfiguration.Setup(x => x["Logstash:Url"]).Returns("test-logstash");
            _mockConfiguration.Setup(x => x["Environment"]).Returns(expectedEnvironment);

            // Act
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Assert
            Assert.Equal(expectedEnvironment, appSettings.Environment);
        }

        [Theory]
        [InlineData("Development")]
        [InlineData("Staging")]
        [InlineData("Production")]
        [InlineData("UAT")]
        [InlineData("LOCAL")]
        public void Environment_Property_ShouldAcceptVariousEnvironmentValues(string environment)
        {
            // Arrange
            _mockConfiguration.Setup(x => x["IsUM"]).Returns("false");
            _mockConfiguration.Setup(x => x["Sentry:Dsn"]).Returns("test-dsn");
            _mockConfiguration.Setup(x => x["GetSubscriberUrl"]).Returns("test-url");
            _mockConfiguration.Setup(x => x["Logstash:Url"]).Returns("test-logstash");
            _mockConfiguration.Setup(x => x["Environment"]).Returns(environment);

            // Act
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Assert
            Assert.Equal(environment, appSettings.Environment);
        }

        [Fact]
        public void GetValue_WithStringType_ShouldReturnStringValue()
        {
            // Arrange
            var key = "TestKey";
            var expectedValue = "TestValue";
            _mockConfiguration.Setup(x => x[key]).Returns(expectedValue);
            SetupOtherConfigurationValues();
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Act
            var result = appSettings.GetValue<string>(key);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetValue_WithBoolType_ShouldReturnBooleanValue()
        {
            // Arrange
            var key = "TestBoolKey";
            _mockConfiguration.Setup(x => x[key]).Returns("true");
            SetupOtherConfigurationValues();
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Act
            var result = appSettings.GetValue<bool>(key);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetValue_WithIntType_ShouldReturnIntegerValue()
        {
            // Arrange
            var key = "TestIntKey";
            var expectedValue = 42;
            _mockConfiguration.Setup(x => x[key]).Returns(expectedValue.ToString());
            SetupOtherConfigurationValues();
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Act
            var result = appSettings.GetValue<int>(key);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetValue_WithDoubleType_ShouldReturnDoubleValue()
        {
            // Arrange
            var key = "TestDoubleKey";
            var expectedValue = 3.14;
            _mockConfiguration.Setup(x => x[key]).Returns(expectedValue.ToString());
            SetupOtherConfigurationValues();
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Act
            var result = appSettings.GetValue<double>(key);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetValue_WithInvalidTypeConversion_ShouldThrowInvalidCastException()
        {
            // Arrange
            var key = "TestKey";
            _mockConfiguration.Setup(x => x[key]).Returns("not-a-number");
            SetupOtherConfigurationValues();
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidCastException>(() => appSettings.GetValue<int>(key));
            Assert.Contains($"Key: {key}", exception.Message);
        }

        [Fact]
        public void GetValue_WithNullValue_ShouldThrowInvalidCastException()
        {
            // Arrange
            var key = "TestNullKey";
            _mockConfiguration.Setup(x => x[key]).Returns((string)null!);
            SetupOtherConfigurationValues();
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidCastException>(() => appSettings.GetValue<int>(key));
            Assert.Contains($"Key: {key}", exception.Message);
        }

        [Fact]
        public void GetValue_WithEmptyString_ForStringType_ShouldReturnEmptyString()
        {
            // Arrange
            var key = "TestEmptyKey";
            _mockConfiguration.Setup(x => x[key]).Returns("");
            SetupOtherConfigurationValues();
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Act
            var result = appSettings.GetValue<string>(key);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void GetValue_WithWhitespace_ForStringType_ShouldReturnWhitespace()
        {
            // Arrange
            var key = "TestWhitespaceKey";
            var whitespaceValue = "   ";
            _mockConfiguration.Setup(x => x[key]).Returns(whitespaceValue);
            SetupOtherConfigurationValues();
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Act
            var result = appSettings.GetValue<string>(key);

            // Assert
            Assert.Equal(whitespaceValue, result);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("1", true)]
        [InlineData("0", false)]
        [InlineData("yes", true)]
        [InlineData("no", false)]
        public void GetValue_WithBooleanStrings_ShouldConvertCorrectly(string configValue, bool expectedResult)
        {
            // Arrange
            var key = "TestBoolKey";
            _mockConfiguration.Setup(x => x[key]).Returns(configValue);
            SetupOtherConfigurationValues();
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Act
            var result = appSettings.GetValue<bool>(key);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Constructor_WithMissingConfigurationValues_ShouldThrowInvalidCastException()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["IsUM"]).Returns((string)null!);
            _mockConfiguration.Setup(x => x["Sentry:Dsn"]).Returns("test");
            _mockConfiguration.Setup(x => x["GetSubscriberUrl"]).Returns("test");
            _mockConfiguration.Setup(x => x["Logstash:Url"]).Returns("test");
            _mockConfiguration.Setup(x => x["Environment"]).Returns("test");

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => new AppSettings(_mockConfiguration.Object));
        }

        [Fact]
        public void Properties_ShouldBeReadOnly()
        {
            // Arrange & Act
            var type = typeof(AppSettings);

            // Assert
            var isUMProperty = type.GetProperty("IsUM");
            var sentryDsnProperty = type.GetProperty("SentryDsn");
            var getSubscriberInfoProperty = type.GetProperty("GetSubscriberInfo");
            var logstashUrlProperty = type.GetProperty("LogstashUrl");
            var environmentProperty = type.GetProperty("Environment");

            Assert.True(isUMProperty!.CanRead);
            Assert.False(isUMProperty.CanWrite);

            Assert.True(sentryDsnProperty!.CanRead);
            Assert.False(sentryDsnProperty.CanWrite);

            Assert.True(getSubscriberInfoProperty!.CanRead);
            Assert.False(getSubscriberInfoProperty.CanWrite);

            Assert.True(logstashUrlProperty!.CanRead);
            Assert.False(logstashUrlProperty.CanWrite);

            Assert.True(environmentProperty!.CanRead);
            Assert.False(environmentProperty.CanWrite);
        }

        [Fact]
        public void Class_ShouldImplementIAppSettings()
        {
            // Arrange & Act
            SetupOtherConfigurationValues();
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Assert
            Assert.IsAssignableFrom<IAppSettings>(appSettings);
        }

        [Fact]
        public void Interface_ShouldHaveCorrectProperties()
        {
            // Arrange
            var interfaceType = typeof(IAppSettings);

            // Act & Assert
            Assert.NotNull(interfaceType.GetProperty("IsUM"));
            Assert.NotNull(interfaceType.GetProperty("SentryDsn"));
            Assert.NotNull(interfaceType.GetProperty("GetSubscriberInfo"));
            Assert.NotNull(interfaceType.GetProperty("LogstashUrl"));
            Assert.NotNull(interfaceType.GetProperty("Environment"));
        }

        [Fact]
        public void Class_ShouldBeInCorrectNamespace()
        {
            // Arrange & Act
            var type = typeof(AppSettings);

            // Assert
            Assert.Equal("Device.FingerPrint.Gateway", type.Namespace);
        }

        [Fact]
        public void Class_ShouldBePublic()
        {
            // Arrange & Act
            var type = typeof(AppSettings);

            // Assert
            Assert.True(type.IsPublic);
        }

        [Fact]
        public void GetValue_Method_ShouldBePublic()
        {
            // Arrange
            var type = typeof(AppSettings);

            // Act
            var method = type.GetMethod("GetValue");

            // Assert
            Assert.NotNull(method);
            Assert.True(method!.IsPublic);
            Assert.True(method.IsGenericMethodDefinition);
        }

        [Fact]
        public void GetValue_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            var key = "TestSpecialKey";
            var specialValue = "!@#$%^&*()_+-=[]{}|;':\",./<>?";
            _mockConfiguration.Setup(x => x[key]).Returns(specialValue);
            SetupOtherConfigurationValues();
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Act
            var result = appSettings.GetValue<string>(key);

            // Assert
            Assert.Equal(specialValue, result);
        }

        [Fact]
        public void GetValue_WithUnicodeCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            var key = "TestUnicodeKey";
            var unicodeValue = "こんにちは世界 🌍 Ñoël";
            _mockConfiguration.Setup(x => x[key]).Returns(unicodeValue);
            SetupOtherConfigurationValues();
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Act
            var result = appSettings.GetValue<string>(key);

            // Assert
            Assert.Equal(unicodeValue, result);
        }

        [Fact]
        public void Properties_ShouldReturnSameValueOnMultipleCalls()
        {
            // Arrange
            SetupOtherConfigurationValues();
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Act
            var isUM1 = appSettings.IsUM;
            var isUM2 = appSettings.IsUM;
            var sentryDsn1 = appSettings.SentryDsn;
            var sentryDsn2 = appSettings.SentryDsn;

            // Assert
            Assert.Equal(isUM1, isUM2);
            Assert.Equal(sentryDsn1, sentryDsn2);
        }

        [Fact]
        public void Constructor_ShouldCallConfigurationForEachProperty()
        {
            // Arrange
            SetupOtherConfigurationValues();

            // Act
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Assert
            _mockConfiguration.Verify(x => x["IsUM"], Times.Once);
            _mockConfiguration.Verify(x => x["Sentry:Dsn"], Times.Once);
            _mockConfiguration.Verify(x => x["GetSubscriberUrl"], Times.Once);
            _mockConfiguration.Verify(x => x["Logstash:Url"], Times.Once);
            _mockConfiguration.Verify(x => x["Environment"], Times.Once);
        }

        [Fact]
        public void GetValue_WithInvalidBooleanValue_ShouldThrowInvalidCastException()
        {
            // Arrange
            var key = "TestInvalidBoolKey";
            _mockConfiguration.Setup(x => x[key]).Returns("maybe");
            SetupOtherConfigurationValues();
            var appSettings = new AppSettings(_mockConfiguration.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidCastException>(() => appSettings.GetValue<bool>(key));
            Assert.Contains($"Key: {key}", exception.Message);
        }

        private void SetupOtherConfigurationValues()
        {
            _mockConfiguration.Setup(x => x["IsUM"]).Returns("false");
            _mockConfiguration.Setup(x => x["Sentry:Dsn"]).Returns("test-dsn");
            _mockConfiguration.Setup(x => x["GetSubscriberUrl"]).Returns("test-url");
            _mockConfiguration.Setup(x => x["Logstash:Url"]).Returns("test-logstash");
            _mockConfiguration.Setup(x => x["Environment"]).Returns("test-env");
        }
    }
}