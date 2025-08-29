using Xunit;
using Device.FingerPrint.Gateway.Model;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Device.FingerPrint.Gateway.Tests.Model
{
    public class MobileTransactionRequestTests
    {
        [Fact]
        public void MobileTransactionRequest_DefaultConstructor_ShouldCreateInstance()
        {
            // Act
            var request = new MobileTransactionRequest();

            // Assert
            Assert.NotNull(request);
            Assert.Null(request.Transaction);
        }

        [Fact]
        public void Transaction_Property_ShouldSetAndGetValue()
        {
            // Arrange
            var request = new MobileTransactionRequest();
            var expectedTransaction = "eyJBY2NvdW50IjoidGVzdCJ9"; // Base64 encoded JSON

            // Act
            request.Transaction = expectedTransaction;

            // Assert
            Assert.Equal(expectedTransaction, request.Transaction);
        }

        [Fact]
        public void Transaction_Property_ShouldAcceptNullValue()
        {
            // Arrange
            var request = new MobileTransactionRequest();

            // Act
            request.Transaction = null;

            // Assert
            Assert.Null(request.Transaction);
        }

        [Fact]
        public void Transaction_Property_ShouldAcceptEmptyString()
        {
            // Arrange
            var request = new MobileTransactionRequest();

            // Act
            request.Transaction = string.Empty;

            // Assert
            Assert.Equal(string.Empty, request.Transaction);
        }

        [Fact]
        public void Transaction_Property_ShouldAcceptWhitespaceString()
        {
            // Arrange
            var request = new MobileTransactionRequest();
            var whitespaceValue = "   ";

            // Act
            request.Transaction = whitespaceValue;

            // Assert
            Assert.Equal(whitespaceValue, request.Transaction);
        }

        [Fact]
        public void Transaction_Property_ShouldAcceptValidBase64String()
        {
            // Arrange
            var request = new MobileTransactionRequest();
            var validBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("test data"));

            // Act
            request.Transaction = validBase64;

            // Assert
            Assert.Equal(validBase64, request.Transaction);
        }

        [Fact]
        public void Transaction_Property_ShouldAcceptLongString()
        {
            // Arrange
            var request = new MobileTransactionRequest();
            var longString = new string('a', 10000);

            // Act
            request.Transaction = longString;

            // Assert
            Assert.Equal(longString, request.Transaction);
        }

        [Fact]
        public void Transaction_Property_ShouldAcceptSpecialCharacters()
        {
            // Arrange
            var request = new MobileTransactionRequest();
            var specialChars = "!@#$%^&*()_+-=[]{}|;':\",./<>?";

            // Act
            request.Transaction = specialChars;

            // Assert
            Assert.Equal(specialChars, request.Transaction);
        }

        [Fact]
        public void Transaction_Property_ShouldAcceptUnicodeCharacters()
        {
            // Arrange
            var request = new MobileTransactionRequest();
            var unicodeString = "こんにちは世界 🌍 Ñoël";

            // Act
            request.Transaction = unicodeString;

            // Assert
            Assert.Equal(unicodeString, request.Transaction);
        }

        [Fact]
        public void JsonSerialization_ShouldSerializeCorrectly()
        {
            // Arrange
            var request = new MobileTransactionRequest
            {
                Transaction = "eyJBY2NvdW50IjoidGVzdCJ9"
            };

            // Act
            var json = JsonConvert.SerializeObject(request);

            // Assert
            Assert.Contains("\"Transaction\":", json);
            Assert.Contains("\"eyJBY2NvdW50IjoidGVzdCJ9\"", json);
        }

        [Fact]
        public void JsonSerialization_WithNullTransaction_ShouldSerializeCorrectly()
        {
            // Arrange
            var request = new MobileTransactionRequest
            {
                Transaction = null
            };

            // Act
            var json = JsonConvert.SerializeObject(request);

            // Assert
            Assert.Contains("\"Transaction\":null", json);
        }

        [Fact]
        public void JsonDeserialization_ShouldDeserializeCorrectly()
        {
            // Arrange
            var json = "{\"Transaction\":\"eyJBY2NvdW50IjoidGVzdCJ9\"}";

            // Act
            var request = JsonConvert.DeserializeObject<MobileTransactionRequest>(json);

            // Assert
            Assert.NotNull(request);
            Assert.Equal("eyJBY2NvdW50IjoidGVzdCJ9", request!.Transaction);
        }

        [Fact]
        public void JsonDeserialization_WithNullTransaction_ShouldDeserializeCorrectly()
        {
            // Arrange
            var json = "{\"Transaction\":null}";

            // Act
            var request = JsonConvert.DeserializeObject<MobileTransactionRequest>(json);

            // Assert
            Assert.NotNull(request);
            Assert.Null(request!.Transaction);
        }

        [Fact]
        public void JsonDeserialization_WithEmptyJson_ShouldCreateEmptyObject()
        {
            // Arrange
            var json = "{}";

            // Act
            var request = JsonConvert.DeserializeObject<MobileTransactionRequest>(json);

            // Assert
            Assert.NotNull(request);
            Assert.Null(request!.Transaction);
        }

        [Fact]
        public void JsonDeserialization_WithMissingTransactionProperty_ShouldCreateObjectWithNullTransaction()
        {
            // Arrange
            var json = "{\"SomeOtherProperty\":\"value\"}";

            // Act
            var request = JsonConvert.DeserializeObject<MobileTransactionRequest>(json);

            // Assert
            Assert.NotNull(request);
            Assert.Null(request!.Transaction);
        }

        [Fact]
        public void JsonRoundTrip_ShouldMaintainDataIntegrity()
        {
            // Arrange
            var originalRequest = new MobileTransactionRequest
            {
                Transaction = "eyJBY2NvdW50IjoidGVzdCIsIlN1YnNjcmliZXIiOiJ0ZXN0U3ViIn0="
            };

            // Act
            var json = JsonConvert.SerializeObject(originalRequest);
            var deserializedRequest = JsonConvert.DeserializeObject<MobileTransactionRequest>(json);

            // Assert
            Assert.NotNull(deserializedRequest);
            Assert.Equal(originalRequest.Transaction, deserializedRequest!.Transaction);
        }

        [Fact]
        public void Object_Equality_TwoInstancesWithSameTransaction_ShouldNotBeEqual()
        {
            // Arrange
            var request1 = new MobileTransactionRequest { Transaction = "test" };
            var request2 = new MobileTransactionRequest { Transaction = "test" };

            // Act & Assert
            // Note: Default object equality is reference equality, not value equality
            Assert.False(request1.Equals(request2));
            Assert.False(request1 == request2);
        }

        [Fact]
        public void Object_Equality_SameInstance_ShouldBeEqual()
        {
            // Arrange
            var request = new MobileTransactionRequest { Transaction = "test" };

            // Act & Assert
            Assert.True(request.Equals(request));
            Assert.True(request == request);
        }

        [Fact]
        public void ToString_ShouldReturnClassName()
        {
            // Arrange
            var request = new MobileTransactionRequest { Transaction = "test" };

            // Act
            var result = request.ToString();

            // Assert
            Assert.Equal("Device.FingerPrint.Gateway.Model.MobileTransactionRequest", result);
        }

        [Fact]
        public void GetHashCode_ShouldReturnConsistentValue()
        {
            // Arrange
            var request = new MobileTransactionRequest { Transaction = "test" };

            // Act
            var hashCode1 = request.GetHashCode();
            var hashCode2 = request.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        [Fact]
        public void Transaction_PropertyType_ShouldBeString()
        {
            // Arrange
            var request = new MobileTransactionRequest();

            // Act
            var propertyInfo = typeof(MobileTransactionRequest).GetProperty("Transaction");

            // Assert
            Assert.NotNull(propertyInfo);
            Assert.Equal(typeof(string), propertyInfo!.PropertyType);
        }

        [Fact]
        public void Transaction_Property_ShouldHavePublicGetterAndSetter()
        {
            // Arrange
            var propertyInfo = typeof(MobileTransactionRequest).GetProperty("Transaction");

            // Act & Assert
            Assert.NotNull(propertyInfo);
            Assert.True(propertyInfo!.CanRead);
            Assert.True(propertyInfo.CanWrite);
            Assert.True(propertyInfo.GetMethod!.IsPublic);
            Assert.True(propertyInfo.SetMethod!.IsPublic);
        }

        [Theory]
        [InlineData("")]
        [InlineData("test")]
        [InlineData("eyJBY2NvdW50IjoidGVzdCJ9")]
        [InlineData("VGVzdCBkYXRhIHdpdGggc3BlY2lhbCBjaGFycyAhQCMkJV4mKigp")]
        public void Transaction_Property_ShouldAcceptVariousValidInputs(string input)
        {
            // Arrange
            var request = new MobileTransactionRequest();

            // Act
            request.Transaction = input;

            // Assert
            Assert.Equal(input, request.Transaction);
        }

        [Fact]
        public void Class_ShouldBeInCorrectNamespace()
        {
            // Arrange & Act
            var type = typeof(MobileTransactionRequest);

            // Assert
            Assert.Equal("Device.FingerPrint.Gateway.Model", type.Namespace);
        }

        [Fact]
        public void Class_ShouldBePublic()
        {
            // Arrange & Act
            var type = typeof(MobileTransactionRequest);

            // Assert
            Assert.True(type.IsPublic);
        }

        [Fact]
        public void Class_ShouldNotBeAbstract()
        {
            // Arrange & Act
            var type = typeof(MobileTransactionRequest);

            // Assert
            Assert.False(type.IsAbstract);
        }

        [Fact]
        public void Class_ShouldNotBeSealed()
        {
            // Arrange & Act
            var type = typeof(MobileTransactionRequest);

            // Assert
            Assert.False(type.IsSealed);
        }

        [Fact]
        public void Class_ShouldHaveParameterlessConstructor()
        {
            // Arrange & Act
            var constructor = typeof(MobileTransactionRequest).GetConstructor(Type.EmptyTypes);

            // Assert
            Assert.NotNull(constructor);
            Assert.True(constructor!.IsPublic);
        }
    }
}