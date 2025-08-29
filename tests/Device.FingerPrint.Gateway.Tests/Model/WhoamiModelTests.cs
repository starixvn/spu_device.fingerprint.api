using Xunit;
using Device.FingerPrint.Gateway.Model;
using Newtonsoft.Json;

namespace Device.FingerPrint.Gateway.Tests.Model
{
    public class WhoamiModelTests
    {
        [Fact]
        public void WhoamiModel_CanBeInstantiated()
        {
            // Act
            var model = new WhoamiModel();

            // Assert
            Assert.NotNull(model);
        }

        [Fact]
        public void Environment_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var model = new WhoamiModel();
            var expected = "Production";

            // Act
            model.Environment = expected;

            // Assert
            Assert.Equal(expected, model.Environment);
        }

        [Fact]
        public void ServerName_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var model = new WhoamiModel();
            var expected = "WEB-SERVER-01";

            // Act
            model.ServerName = expected;

            // Assert
            Assert.Equal(expected, model.ServerName);
        }

        [Fact]
        public void LocalIP_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var model = new WhoamiModel();
            var expected = "192.168.1.100";

            // Act
            model.LocalIP = expected;

            // Assert
            Assert.Equal(expected, model.LocalIP);
        }

        [Fact]
        public void ServerTime_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var model = new WhoamiModel();
            var expected = new DateTime(2025, 8, 29, 10, 30, 45, DateTimeKind.Utc);

            // Act
            model.ServerTime = expected;

            // Assert
            Assert.Equal(expected, model.ServerTime);
        }

        [Fact]
        public void RealIP_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var model = new WhoamiModel();
            var expected = "203.123.45.67";

            // Act
            model.RealIP = expected;

            // Assert
            Assert.Equal(expected, model.RealIP);
        }

        [Fact]
        public void FakeIP_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var model = new WhoamiModel();
            var expected = "10.0.0.1";

            // Act
            model.FakeIP = expected;

            // Assert
            Assert.Equal(expected, model.FakeIP);
        }

        [Fact]
        public void XFW_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var model = new WhoamiModel();
            var expected = "192.168.1.1, 10.0.0.1";

            // Act
            model.XFW = expected;

            // Assert
            Assert.Equal(expected, model.XFW);
        }

        [Fact]
        public void ClientIP_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var model = new WhoamiModel();
            var expected = "192.168.1.150";

            // Act
            model.ClientIP = expected;

            // Assert
            Assert.Equal(expected, model.ClientIP);
        }

        [Fact]
        public void ActualIP_Property_CanBeSetAndRetrieved()
        {
            // Arrange
            var model = new WhoamiModel();
            var expected = "203.123.45.67";

            // Act
            model.ActualIP = expected;

            // Assert
            Assert.Equal(expected, model.ActualIP);
        }

        [Fact]
        public void WhoamiModel_AllProperties_CanBeSetSimultaneously()
        {
            // Arrange
            var model = new WhoamiModel();
            var environment = "Development";
            var serverName = "DEV-SERVER";
            var localIP = "127.0.0.1";
            var serverTime = new DateTime(2025, 8, 29, 14, 15, 30, DateTimeKind.Utc);
            var realIP = "203.0.113.1";
            var fakeIP = "192.0.2.1";
            var xfw = "198.51.100.1, 203.0.113.1";
            var clientIP = "203.0.113.100";
            var actualIP = "203.0.113.1";

            // Act
            model.Environment = environment;
            model.ServerName = serverName;
            model.LocalIP = localIP;
            model.ServerTime = serverTime;
            model.RealIP = realIP;
            model.FakeIP = fakeIP;
            model.XFW = xfw;
            model.ClientIP = clientIP;
            model.ActualIP = actualIP;

            // Assert
            Assert.Equal(environment, model.Environment);
            Assert.Equal(serverName, model.ServerName);
            Assert.Equal(localIP, model.LocalIP);
            Assert.Equal(serverTime, model.ServerTime);
            Assert.Equal(realIP, model.RealIP);
            Assert.Equal(fakeIP, model.FakeIP);
            Assert.Equal(xfw, model.XFW);
            Assert.Equal(clientIP, model.ClientIP);
            Assert.Equal(actualIP, model.ActualIP);
        }

        [Fact]
        public void WhoamiModel_StringProperties_CanBeSetToNull()
        {
            // Arrange
            var model = new WhoamiModel();

            // Act
            model.Environment = null;
            model.ServerName = null;
            model.LocalIP = null;
            model.RealIP = null;
            model.FakeIP = null;
            model.XFW = null;
            model.ClientIP = null;
            model.ActualIP = null;

            // Assert
            Assert.Null(model.Environment);
            Assert.Null(model.ServerName);
            Assert.Null(model.LocalIP);
            Assert.Null(model.RealIP);
            Assert.Null(model.FakeIP);
            Assert.Null(model.XFW);
            Assert.Null(model.ClientIP);
            Assert.Null(model.ActualIP);
        }

        [Fact]
        public void WhoamiModel_StringProperties_CanBeSetToEmptyString()
        {
            // Arrange
            var model = new WhoamiModel();

            // Act
            model.Environment = string.Empty;
            model.ServerName = string.Empty;
            model.LocalIP = string.Empty;
            model.RealIP = string.Empty;
            model.FakeIP = string.Empty;
            model.XFW = string.Empty;
            model.ClientIP = string.Empty;
            model.ActualIP = string.Empty;

            // Assert
            Assert.Equal(string.Empty, model.Environment);
            Assert.Equal(string.Empty, model.ServerName);
            Assert.Equal(string.Empty, model.LocalIP);
            Assert.Equal(string.Empty, model.RealIP);
            Assert.Equal(string.Empty, model.FakeIP);
            Assert.Equal(string.Empty, model.XFW);
            Assert.Equal(string.Empty, model.ClientIP);
            Assert.Equal(string.Empty, model.ActualIP);
        }

        [Fact]
        public void WhoamiModel_ServerTime_DefaultValue()
        {
            // Arrange & Act
            var model = new WhoamiModel();

            // Assert
            Assert.Equal(default(DateTime), model.ServerTime);
        }

        [Fact]
        public void WhoamiModel_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var model = new WhoamiModel
            {
                Environment = "Production",
                ServerName = "WEB-01",
                LocalIP = "192.168.1.100",
                ServerTime = new DateTime(2025, 8, 29, 10, 30, 45, DateTimeKind.Utc),
                RealIP = "203.123.45.67",
                FakeIP = "10.0.0.1",
                XFW = "192.168.1.1, 10.0.0.1",
                ClientIP = "192.168.1.150",
                ActualIP = "203.123.45.67"
            };

            // Act
            var json = JsonConvert.SerializeObject(model);
            var deserializedModel = JsonConvert.DeserializeObject<WhoamiModel>(json);

            // Assert
            Assert.NotNull(deserializedModel);
            Assert.Equal(model.Environment, deserializedModel.Environment);
            Assert.Equal(model.ServerName, deserializedModel.ServerName);
            Assert.Equal(model.LocalIP, deserializedModel.LocalIP);
            Assert.Equal(model.ServerTime, deserializedModel.ServerTime);
            Assert.Equal(model.RealIP, deserializedModel.RealIP);
            Assert.Equal(model.FakeIP, deserializedModel.FakeIP);
            Assert.Equal(model.XFW, deserializedModel.XFW);
            Assert.Equal(model.ClientIP, deserializedModel.ClientIP);
            Assert.Equal(model.ActualIP, deserializedModel.ActualIP);
        }

        [Fact]
        public void WhoamiModel_JsonDeserialization_WithMissingProperties_WorksCorrectly()
        {
            // Arrange
            var json = @"{
                ""Environment"": ""Test"",
                ""ServerName"": ""TEST-SERVER""
            }";

            // Act
            var model = JsonConvert.DeserializeObject<WhoamiModel>(json);

            // Assert
            Assert.NotNull(model);
            Assert.Equal("Test", model.Environment);
            Assert.Equal("TEST-SERVER", model.ServerName);
            Assert.Null(model.LocalIP);
            Assert.Equal(default(DateTime), model.ServerTime);
            Assert.Null(model.RealIP);
            Assert.Null(model.FakeIP);
            Assert.Null(model.XFW);
            Assert.Null(model.ClientIP);
            Assert.Null(model.ActualIP);
        }

        [Fact]
        public void WhoamiModel_JsonDeserialization_WithNullValues_WorksCorrectly()
        {
            // Arrange
            var json = @"{
                ""Environment"": null,
                ""ServerName"": null,
                ""LocalIP"": null,
                ""ServerTime"": ""2025-08-29T10:30:45Z"",
                ""RealIP"": null,
                ""FakeIP"": null,
                ""XFW"": null,
                ""ClientIP"": null,
                ""ActualIP"": null
            }";

            // Act
            var model = JsonConvert.DeserializeObject<WhoamiModel>(json);

            // Assert
            Assert.NotNull(model);
            Assert.Null(model.Environment);
            Assert.Null(model.ServerName);
            Assert.Null(model.LocalIP);
            Assert.Equal(new DateTime(2025, 8, 29, 10, 30, 45, DateTimeKind.Utc), model.ServerTime);
            Assert.Null(model.RealIP);
            Assert.Null(model.FakeIP);
            Assert.Null(model.XFW);
            Assert.Null(model.ClientIP);
            Assert.Null(model.ActualIP);
        }

        [Theory]
        [InlineData("Production")]
        [InlineData("Development")]
        [InlineData("Staging")]
        [InlineData("Test")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("VERY_LONG_ENVIRONMENT_NAME_WITH_SPECIAL_CHARS_123!@#")]
        public void Environment_Property_AcceptsVariousValues(string value)
        {
            // Arrange
            var model = new WhoamiModel();

            // Act
            model.Environment = value;

            // Assert
            Assert.Equal(value, model.Environment);
        }

        [Theory]
        [InlineData("192.168.1.1")]
        [InlineData("10.0.0.1")]
        [InlineData("172.16.0.1")]
        [InlineData("127.0.0.1")]
        [InlineData("203.0.113.1")]
        [InlineData("::1")]
        [InlineData("2001:db8::1")]
        [InlineData("invalid-ip")]
        [InlineData("")]
        public void IPAddressProperties_AcceptVariousValues(string ipValue)
        {
            // Arrange
            var model = new WhoamiModel();

            // Act
            model.LocalIP = ipValue;
            model.RealIP = ipValue;
            model.FakeIP = ipValue;
            model.ClientIP = ipValue;
            model.ActualIP = ipValue;

            // Assert
            Assert.Equal(ipValue, model.LocalIP);
            Assert.Equal(ipValue, model.RealIP);
            Assert.Equal(ipValue, model.FakeIP);
            Assert.Equal(ipValue, model.ClientIP);
            Assert.Equal(ipValue, model.ActualIP);
        }

        [Fact]
        public void WhoamiModel_Properties_AreIndependent()
        {
            // Arrange
            var model = new WhoamiModel();
            var value1 = "Value1";
            var value2 = "Value2";

            // Act
            model.Environment = value1;
            model.ServerName = value2;

            // Assert - Verify one property doesn't affect another
            Assert.Equal(value1, model.Environment);
            Assert.Equal(value2, model.ServerName);
            Assert.NotEqual(model.Environment, model.ServerName);
        }

        [Fact]
        public void WhoamiModel_StringProperties_HandleUnicodeCharacters()
        {
            // Arrange
            var model = new WhoamiModel();
            var unicodeValue = "Server-Ñáme-测试-🌟";

            // Act
            model.ServerName = unicodeValue;

            // Assert
            Assert.Equal(unicodeValue, model.ServerName);
        }

        [Fact]
        public void WhoamiModel_XFW_Property_HandlesMultipleIPs()
        {
            // Arrange
            var model = new WhoamiModel();
            var multipleIPs = "192.168.1.1, 10.0.0.1, 172.16.0.1, 203.0.113.1";

            // Act
            model.XFW = multipleIPs;

            // Assert
            Assert.Equal(multipleIPs, model.XFW);
        }
    }
}
