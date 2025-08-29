using Device.FingerPrint.Gateway.Model;
using Newtonsoft.Json;
using Xunit;

namespace Device.FingerPrint.Gateway.Tests.Model
{
    public class TransactionRequestTests
    {
        [Fact]
        public void TransactionRequest_DefaultConstructor_ShouldCreateInstance()
        {
            // Act
            var request = new TransactionRequest();

            // Assert
            Assert.NotNull(request);
            Assert.Null(request.Account);
            Assert.Null(request.Action);
            Assert.Null(request.ActionResult);
            Assert.Null(request.AcceptsTypes);
            Assert.Null(request.Platform);
            Assert.Null(request.UserAgent);
            Assert.Null(request.Screen);
            Assert.Null(request.Ip);
            Assert.Null(request.FakeIp);
            Assert.Null(request.Url);
            Assert.Null(request.Di);
            Assert.Null(request.__di);
            Assert.Null(request.Bi);
            Assert.Null(request.Token);
            Assert.Null(request.DetecasAnalysis);
            Assert.Null(request.Subscriber);
            Assert.Equal(0, request.SubscriberId);
            Assert.Null(request.LoginTime);
            Assert.Null(request.CaptchaResult);
            Assert.Null(request.SessionKey);
            Assert.Equal(default(DateTime), request.TransDate);
        }

        [Fact]
        public void Account_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedAccount = "TestAccount";

            // Act
            request.Account = expectedAccount;

            // Assert
            Assert.Equal(expectedAccount, request.Account);
        }

        [Fact]
        public void Action_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedAction = "Login";

            // Act
            request.Action = expectedAction;

            // Assert
            Assert.Equal(expectedAction, request.Action);
        }

        [Fact]
        public void ActionResult_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedActionResult = "Success";

            // Act
            request.ActionResult = expectedActionResult;

            // Assert
            Assert.Equal(expectedActionResult, request.ActionResult);
        }

        [Fact]
        public void AcceptsTypes_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedAcceptsTypes = "application/json";

            // Act
            request.AcceptsTypes = expectedAcceptsTypes;

            // Assert
            Assert.Equal(expectedAcceptsTypes, request.AcceptsTypes);
        }

        [Fact]
        public void Platform_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedPlatform = "Web";

            // Act
            request.Platform = expectedPlatform;

            // Assert
            Assert.Equal(expectedPlatform, request.Platform);
        }

        [Fact]
        public void UserAgent_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

            // Act
            request.UserAgent = expectedUserAgent;

            // Assert
            Assert.Equal(expectedUserAgent, request.UserAgent);
        }

        [Fact]
        public void Screen_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedScreen = "1920x1080";

            // Act
            request.Screen = expectedScreen;

            // Assert
            Assert.Equal(expectedScreen, request.Screen);
        }

        [Fact]
        public void Ip_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedIp = "192.168.1.1";

            // Act
            request.Ip = expectedIp;

            // Assert
            Assert.Equal(expectedIp, request.Ip);
        }

        [Fact]
        public void FakeIp_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedFakeIp = "10.0.0.1";

            // Act
            request.FakeIp = expectedFakeIp;

            // Assert
            Assert.Equal(expectedFakeIp, request.FakeIp);
        }

        [Fact]
        public void Url_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedUrl = "https://example.com/login";

            // Act
            request.Url = expectedUrl;

            // Assert
            Assert.Equal(expectedUrl, request.Url);
        }

        [Fact]
        public void Di_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedDi = "device-id-123";

            // Act
            request.Di = expectedDi;

            // Assert
            Assert.Equal(expectedDi, request.Di);
        }

        [Fact]
        public void __di_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expected__di = "device-info-123";

            // Act
            request.__di = expected__di;

            // Assert
            Assert.Equal(expected__di, request.__di);
        }

        [Fact]
        public void Bi_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedBi = "browser-id-123";

            // Act
            request.Bi = expectedBi;

            // Assert
            Assert.Equal(expectedBi, request.Bi);
        }

        [Fact]
        public void Token_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedToken = "jwt-token-123";

            // Act
            request.Token = expectedToken;

            // Assert
            Assert.Equal(expectedToken, request.Token);
        }

        [Fact]
        public void DetecasAnalysis_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedDetecasAnalysis = "analysis-data";

            // Act
            request.DetecasAnalysis = expectedDetecasAnalysis;

            // Assert
            Assert.Equal(expectedDetecasAnalysis, request.DetecasAnalysis);
        }

        [Fact]
        public void Subscriber_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedSubscriber = "TestSubscriber";

            // Act
            request.Subscriber = expectedSubscriber;

            // Assert
            Assert.Equal(expectedSubscriber, request.Subscriber);
        }

        [Fact]
        public void SubscriberId_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedSubscriberId = 12345;

            // Act
            request.SubscriberId = expectedSubscriberId;

            // Assert
            Assert.Equal(expectedSubscriberId, request.SubscriberId);
        }

        [Fact]
        public void LoginTime_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedLoginTime = new DateTime(2023, 12, 25, 10, 30, 0);

            // Act
            request.LoginTime = expectedLoginTime;

            // Assert
            Assert.Equal(expectedLoginTime, request.LoginTime);
        }

        [Fact]
        public void LoginTime_Property_ShouldAcceptNullValue()
        {
            // Arrange
            var request = new TransactionRequest();

            // Act
            request.LoginTime = null;

            // Assert
            Assert.Null(request.LoginTime);
        }

        [Fact]
        public void CaptchaResult_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedCaptchaResult = "captcha-success";

            // Act
            request.CaptchaResult = expectedCaptchaResult;

            // Assert
            Assert.Equal(expectedCaptchaResult, request.CaptchaResult);
        }

        [Fact]
        public void SessionKey_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedSessionKey = "session-key-123";

            // Act
            request.SessionKey = expectedSessionKey;

            // Assert
            Assert.Equal(expectedSessionKey, request.SessionKey);
        }

        [Fact]
        public void TransDate_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedTransDate = new DateTime(2023, 12, 25, 15, 45, 30);

            // Act
            request.TransDate = expectedTransDate;

            // Assert
            Assert.Equal(expectedTransDate, request.TransDate);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("Test Account")]
        [InlineData("Account with special chars !@#$%")]
        [InlineData("Ñoël Account with unicode")]
        public void StringProperties_ShouldAcceptVariousInputs(string input)
        {
            // Arrange
            var request = new TransactionRequest();

            // Act & Assert
            request.Account = input;
            Assert.Equal(input, request.Account);

            request.Action = input;
            Assert.Equal(input, request.Action);

            request.ActionResult = input;
            Assert.Equal(input, request.ActionResult);

            request.Platform = input;
            Assert.Equal(input, request.Platform);

            request.Subscriber = input;
            Assert.Equal(input, request.Subscriber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SubscriberId_Property_ShouldAcceptVariousIntegerValues(int value)
        {
            // Arrange
            var request = new TransactionRequest();

            // Act
            request.SubscriberId = value;

            // Assert
            Assert.Equal(value, request.SubscriberId);
        }

        [Fact]
        public void JsonSerialization_ShouldSerializeAllProperties()
        {
            // Arrange
            var request = new TransactionRequest
            {
                Account = "TestAccount",
                Action = "Login",
                ActionResult = "Success",
                AcceptsTypes = "application/json",
                Platform = "Web",
                UserAgent = "Mozilla/5.0",
                Screen = "1920x1080",
                Ip = "192.168.1.1",
                FakeIp = "10.0.0.1",
                Url = "https://example.com",
                Di = "device-id",
                __di = "device-info",
                Bi = "browser-id",
                Token = "jwt-token",
                DetecasAnalysis = "analysis",
                Subscriber = "TestSubscriber",
                SubscriberId = 123,
                LoginTime = new DateTime(2023, 12, 25),
                CaptchaResult = "success",
                SessionKey = "session-key",
                TransDate = new DateTime(2023, 12, 25, 10, 30, 0)
            };

            // Act
            var json = JsonConvert.SerializeObject(request);

            // Assert
            Assert.Contains("\"Account\":\"TestAccount\"", json);
            Assert.Contains("\"Action\":\"Login\"", json);
            Assert.Contains("\"ActionResult\":\"Success\"", json);
            Assert.Contains("\"SubscriberId\":123", json);
            Assert.Contains("\"Subscriber\":\"TestSubscriber\"", json);
        }

        [Fact]
        public void JsonSerialization_WithNullValues_ShouldSerializeCorrectly()
        {
            // Arrange
            var request = new TransactionRequest
            {
                Account = null,
                LoginTime = null,
                SubscriberId = 0,
                TransDate = default(DateTime)
            };

            // Act
            var json = JsonConvert.SerializeObject(request);

            // Assert
            Assert.Contains("\"Account\":null", json);
            Assert.Contains("\"LoginTime\":null", json);
            Assert.Contains("\"SubscriberId\":0", json);
        }

        [Fact]
        public void JsonDeserialization_ShouldDeserializeCorrectly()
        {
            // Arrange
            var json = @"{
                ""Account"": ""TestAccount"",
                ""Action"": ""Login"",
                ""SubscriberId"": 123,
                ""LoginTime"": ""2023-12-25T10:30:00"",
                ""TransDate"": ""2023-12-25T15:45:30""
            }";

            // Act
            var request = JsonConvert.DeserializeObject<TransactionRequest>(json);

            // Assert
            Assert.NotNull(request);
            Assert.Equal("TestAccount", request!.Account);
            Assert.Equal("Login", request.Action);
            Assert.Equal(123, request.SubscriberId);
            Assert.Equal(new DateTime(2023, 12, 25, 10, 30, 0), request.LoginTime);
            Assert.Equal(new DateTime(2023, 12, 25, 15, 45, 30), request.TransDate);
        }

        [Fact]
        public void JsonDeserialization_WithMissingProperties_ShouldUseDefaults()
        {
            // Arrange
            var json = @"{""Account"": ""TestAccount""}";

            // Act
            var request = JsonConvert.DeserializeObject<TransactionRequest>(json);

            // Assert
            Assert.NotNull(request);
            Assert.Equal("TestAccount", request!.Account);
            Assert.Null(request.Action);
            Assert.Equal(0, request.SubscriberId);
            Assert.Null(request.LoginTime);
            Assert.Equal(default(DateTime), request.TransDate);
        }

        [Fact]
        public void JsonRoundTrip_ShouldMaintainDataIntegrity()
        {
            // Arrange
            var originalRequest = new TransactionRequest
            {
                Account = "TestAccount",
                Action = "Login",
                SubscriberId = 123,
                LoginTime = new DateTime(2023, 12, 25, 10, 30, 0),
                TransDate = new DateTime(2023, 12, 25, 15, 45, 30),
                Subscriber = "TestSubscriber"
            };

            // Act
            var json = JsonConvert.SerializeObject(originalRequest);
            var deserializedRequest = JsonConvert.DeserializeObject<TransactionRequest>(json);

            // Assert
            Assert.NotNull(deserializedRequest);
            Assert.Equal(originalRequest.Account, deserializedRequest!.Account);
            Assert.Equal(originalRequest.Action, deserializedRequest.Action);
            Assert.Equal(originalRequest.SubscriberId, deserializedRequest.SubscriberId);
            Assert.Equal(originalRequest.LoginTime, deserializedRequest.LoginTime);
            Assert.Equal(originalRequest.TransDate, deserializedRequest.TransDate);
            Assert.Equal(originalRequest.Subscriber, deserializedRequest.Subscriber);
        }

        [Fact]
        public void PropertyTypes_ShouldBeCorrect()
        {
            // Arrange
            var type = typeof(TransactionRequest);

            // Act & Assert
            Assert.Equal(typeof(string), type.GetProperty("Account")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("Action")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("ActionResult")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("AcceptsTypes")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("Platform")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("UserAgent")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("Screen")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("Ip")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("FakeIp")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("Url")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("Di")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("__di")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("Bi")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("Token")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("DetecasAnalysis")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("Subscriber")!.PropertyType);
            Assert.Equal(typeof(int), type.GetProperty("SubscriberId")!.PropertyType);
            Assert.Equal(typeof(DateTime?), type.GetProperty("LoginTime")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("CaptchaResult")!.PropertyType);
            Assert.Equal(typeof(string), type.GetProperty("SessionKey")!.PropertyType);
            Assert.Equal(typeof(DateTime), type.GetProperty("TransDate")!.PropertyType);
        }

        [Fact]
        public void AllProperties_ShouldHavePublicGettersAndSetters()
        {
            // Arrange
            var type = typeof(TransactionRequest);
            var properties = type.GetProperties();

            // Act & Assert
            foreach (var property in properties)
            {
                Assert.True(property.CanRead, $"Property {property.Name} should have a getter");
                Assert.True(property.CanWrite, $"Property {property.Name} should have a setter");
                Assert.True(property.GetMethod!.IsPublic, $"Property {property.Name} getter should be public");
                Assert.True(property.SetMethod!.IsPublic, $"Property {property.Name} setter should be public");
            }
        }

        [Fact]
        public void Class_ShouldHaveCorrectNumberOfProperties()
        {
            // Arrange
            var type = typeof(TransactionRequest);

            // Act
            var properties = type.GetProperties();

            // Assert
            Assert.Equal(21, properties.Length);
        }

        [Fact]
        public void Class_ShouldBeInCorrectNamespace()
        {
            // Arrange & Act
            var type = typeof(TransactionRequest);

            // Assert
            Assert.Equal("Device.FingerPrint.Gateway.Model", type.Namespace);
        }

        [Fact]
        public void Class_ShouldBePublic()
        {
            // Arrange & Act
            var type = typeof(TransactionRequest);

            // Assert
            Assert.True(type.IsPublic);
        }

        [Fact]
        public void Class_ShouldHaveParameterlessConstructor()
        {
            // Arrange & Act
            var constructor = typeof(TransactionRequest).GetConstructor(Type.EmptyTypes);

            // Assert
            Assert.NotNull(constructor);
            Assert.True(constructor!.IsPublic);
        }

        [Fact]
        public void ToString_ShouldReturnClassName()
        {
            // Arrange
            var request = new TransactionRequest();

            // Act
            var result = request.ToString();

            // Assert
            Assert.Equal("Device.FingerPrint.Gateway.Model.TransactionRequest", result);
        }

        [Fact]
        public void GetHashCode_ShouldReturnConsistentValue()
        {
            // Arrange
            var request = new TransactionRequest { Account = "test" };

            // Act
            var hashCode1 = request.GetHashCode();
            var hashCode2 = request.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        [Fact]
        public void Object_Equality_SameInstance_ShouldBeEqual()
        {
            // Arrange
            var request = new TransactionRequest { Account = "test" };

            // Act & Assert
            Assert.True(request.Equals(request));
            Assert.True(request == request);
        }

        [Fact]
        public void Object_Equality_TwoInstancesWithSameData_ShouldNotBeEqual()
        {
            // Arrange
            var request1 = new TransactionRequest { Account = "test", Action = "login" };
            var request2 = new TransactionRequest { Account = "test", Action = "login" };

            // Act & Assert
            // Default object equality is reference equality, not value equality
            Assert.False(request1.Equals(request2));
            Assert.False(request1 == request2);
        }

        [Fact]
        public void DateTime_Properties_ShouldHandleUtcAndLocalTime()
        {
            // Arrange
            var request = new TransactionRequest();
            var utcTime = DateTime.UtcNow;
            var localTime = DateTime.Now;

            // Act
            request.TransDate = utcTime;
            request.LoginTime = localTime;

            // Assert
            Assert.Equal(utcTime, request.TransDate);
            Assert.Equal(localTime, request.LoginTime);
        }

        [Fact]
        public void SpecialProperty___di_ShouldWorkCorrectly()
        {
            // Arrange
            var request = new TransactionRequest();
            var expectedValue = "special-device-info";

            // Act
            request.__di = expectedValue;

            // Assert
            Assert.Equal(expectedValue, request.__di);
        }

        [Fact]
        public void AllStringProperties_ShouldAcceptNullValues()
        {
            // Arrange
            var request = new TransactionRequest();

            // Act & Assert
            request.Account = null;
            Assert.Null(request.Account);

            request.Action = null;
            Assert.Null(request.Action);

            request.ActionResult = null;
            Assert.Null(request.ActionResult);

            request.AcceptsTypes = null;
            Assert.Null(request.AcceptsTypes);

            request.Platform = null;
            Assert.Null(request.Platform);

            request.UserAgent = null;
            Assert.Null(request.UserAgent);

            request.Screen = null;
            Assert.Null(request.Screen);

            request.Ip = null;
            Assert.Null(request.Ip);

            request.FakeIp = null;
            Assert.Null(request.FakeIp);

            request.Url = null;
            Assert.Null(request.Url);

            request.Di = null;
            Assert.Null(request.Di);

            request.__di = null;
            Assert.Null(request.__di);

            request.Bi = null;
            Assert.Null(request.Bi);

            request.Token = null;
            Assert.Null(request.Token);

            request.DetecasAnalysis = null;
            Assert.Null(request.DetecasAnalysis);

            request.Subscriber = null;
            Assert.Null(request.Subscriber);

            request.CaptchaResult = null;
            Assert.Null(request.CaptchaResult);

            request.SessionKey = null;
            Assert.Null(request.SessionKey);
        }
    }
}