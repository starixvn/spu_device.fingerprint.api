using Device.FingerPrint.Core.Services;
using Device.FingerPrint.QueueConsumer;
using NSubstitute;
using Xunit;

namespace Device.FingerPrint.QueueConsumer.Tests
{
    public class UtilitiesTests
    {
        private readonly ICrypto _crypto;

        public UtilitiesTests()
        {
            _crypto = Substitute.For<ICrypto>();
        }

        [Fact]
        public void GetMD5HashFromObject_WithValidObject_ShouldReturnHash()
        {
            // Arrange
            var testObject = new { Name = "Test", Value = 123 };
            var expectedHash = "test-hash-value";
            _crypto.GetMD5Hash(Arg.Any<string>()).Returns(expectedHash);

            // Act
            var result = Utilities.GetMD5HashFromObject(_crypto, testObject);

            // Assert
            Assert.Equal(expectedHash, result);
            _crypto.Received(1).GetMD5Hash(Arg.Any<string>());
        }

        [Fact]
        public void GetMD5HashFromObject_WithNullObject_ShouldReturnHash()
        {
            // Arrange
            object testObject = null;
            var expectedHash = "null-hash-value";
            _crypto.GetMD5Hash(Arg.Any<string>()).Returns(expectedHash);

            // Act
            var result = Utilities.GetMD5HashFromObject(_crypto, testObject);

            // Assert
            Assert.Equal(expectedHash, result);
            _crypto.Received(1).GetMD5Hash(Arg.Any<string>());
        }

        [Theory]
        [InlineData("{\"name\":\"test\"}", true)]
        [InlineData("{\"name\":\"test\",\"value\":123}", true)]
        [InlineData("[1,2,3,4,5]", true)]
        [InlineData("invalid json", false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void IsValidJson_WithVariousInputs_ShouldReturnExpectedResult(string input, bool expected)
        {
            // Act
            var result = Utilities.IsValidJson(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
