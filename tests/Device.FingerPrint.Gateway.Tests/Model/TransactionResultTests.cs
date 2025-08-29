using Xunit;
using Device.FingerPrint.Gateway.Model;

namespace Device.FingerPrint.Gateway.Tests.Model
{
    public class TransactionResultTests
    {
        [Fact]
        public void TransactionResult_Constants_HaveCorrectValues()
        {
            // Assert
            Assert.Equal("000", TransactionResult.Success);
            Assert.Equal("001", TransactionResult.Allow);
            Assert.Equal("002", TransactionResult.Review);
            Assert.Equal("003", TransactionResult.Deny);
            Assert.Equal("006", TransactionResult.EmptyAccount);
            Assert.Equal("007", TransactionResult.EmptyDevice);
            Assert.Equal("008", TransactionResult.NotExistAction);
            Assert.Equal("009", TransactionResult.NotExistSubscriber);
            Assert.Equal("010", TransactionResult.UnderMaintenance);
            Assert.Equal("1000", TransactionResult.Others);
        }

        [Fact]
        public void GetMessage_WithSuccessCode_ReturnsSuccessMessage()
        {
            // Act
            var message = TransactionResult.GetMessage(TransactionResult.Success);

            // Assert
            Assert.Equal("Success", message);
        }

        [Fact]
        public void GetMessage_WithAllowCode_ReturnsAllowMessage()
        {
            // Act
            var message = TransactionResult.GetMessage(TransactionResult.Allow);

            // Assert
            Assert.Equal("Allow", message);
        }

        [Fact]
        public void GetMessage_WithReviewCode_ReturnsReviewMessage()
        {
            // Act
            var message = TransactionResult.GetMessage(TransactionResult.Review);

            // Assert
            Assert.Equal("Review", message);
        }

        [Fact]
        public void GetMessage_WithDenyCode_ReturnsDenyMessage()
        {
            // Act
            var message = TransactionResult.GetMessage(TransactionResult.Deny);

            // Assert
            Assert.Equal("Deny", message);
        }

        [Fact]
        public void GetMessage_WithEmptyAccountCode_ReturnsEmptyAccountMessage()
        {
            // Act
            var message = TransactionResult.GetMessage(TransactionResult.EmptyAccount);

            // Assert
            Assert.Equal("Account is empty", message);
        }

        [Fact]
        public void GetMessage_WithEmptyDeviceCode_ReturnsEmptyDeviceMessage()
        {
            // Act
            var message = TransactionResult.GetMessage(TransactionResult.EmptyDevice);

            // Assert
            Assert.Equal("Device is empty", message);
        }

        [Fact]
        public void GetMessage_WithNotExistActionCode_ReturnsNotExistActionMessage()
        {
            // Act
            var message = TransactionResult.GetMessage(TransactionResult.NotExistAction);

            // Assert
            Assert.Equal("Action doesn't exist", message);
        }

        [Fact]
        public void GetMessage_WithNotExistSubscriberCode_ReturnsNotExistSubscriberMessage()
        {
            // Act
            var message = TransactionResult.GetMessage(TransactionResult.NotExistSubscriber);

            // Assert
            Assert.Equal("Subscriber is invalid", message);
        }

        [Fact]
        public void GetMessage_WithUnderMaintenanceCode_ReturnsUnderMaintenanceMessage()
        {
            // Act
            var message = TransactionResult.GetMessage(TransactionResult.UnderMaintenance);

            // Assert
            Assert.Equal("Under Maintenance", message);
        }

        [Fact]
        public void GetMessage_WithOthersCode_ReturnsOthersMessage()
        {
            // Act
            var message = TransactionResult.GetMessage(TransactionResult.Others);

            // Assert
            Assert.Equal("Other error", message);
        }

        [Fact]
        public void GetMessage_WithUnknownCode_ReturnsUnknownErrorMessage()
        {
            // Act
            var message = TransactionResult.GetMessage("999");

            // Assert
            Assert.Equal("Unknown error code", message);
        }

        [Fact]
        public void GetMessage_WithNullCode_ReturnsUnknownErrorMessage()
        {
            // Act
            var message = TransactionResult.GetMessage(null!);

            // Assert
            Assert.Equal("Unknown error code", message);
        }

        [Fact]
        public void GetMessage_WithEmptyCode_ReturnsUnknownErrorMessage()
        {
            // Act
            var message = TransactionResult.GetMessage(string.Empty);

            // Assert
            Assert.Equal("Unknown error code", message);
        }

        [Fact]
        public void GetMessage_WithWhitespaceCode_ReturnsUnknownErrorMessage()
        {
            // Act
            var message = TransactionResult.GetMessage("   ");

            // Assert
            Assert.Equal("Unknown error code", message);
        }

        [Fact]
        public void GetMessage_WithCaseSensitiveCode_ReturnsUnknownErrorMessage()
        {
            // Arrange - Testing case sensitivity
            var uppercaseCode = "SUCCESS";

            // Act
            var message = TransactionResult.GetMessage(uppercaseCode);

            // Assert
            Assert.Equal("Unknown error code", message);
        }

        [Theory]
        [InlineData("000", "Success")]
        [InlineData("001", "Allow")]
        [InlineData("002", "Review")]
        [InlineData("003", "Deny")]
        [InlineData("006", "Account is empty")]
        [InlineData("007", "Device is empty")]
        [InlineData("008", "Action doesn't exist")]
        [InlineData("009", "Subscriber is invalid")]
        [InlineData("010", "Under Maintenance")]
        [InlineData("1000", "Other error")]
        public void GetMessage_WithValidCodes_ReturnsCorrectMessages(string code, string expectedMessage)
        {
            // Act
            var message = TransactionResult.GetMessage(code);

            // Assert
            Assert.Equal(expectedMessage, message);
        }

        [Theory]
        [InlineData("999")]
        [InlineData("004")]
        [InlineData("005")]
        [InlineData("011")]
        [InlineData("abc")]
        [InlineData("")]
        [InlineData(" ")]
        public void GetMessage_WithInvalidCodes_ReturnsUnknownErrorMessage(string code)
        {
            // Act
            var message = TransactionResult.GetMessage(code);

            // Assert
            Assert.Equal("Unknown error code", message);
        }

        [Fact]
        public void TransactionResult_Constants_AreNotNull()
        {
            // Assert
            Assert.NotNull(TransactionResult.Success);
            Assert.NotNull(TransactionResult.Allow);
            Assert.NotNull(TransactionResult.Review);
            Assert.NotNull(TransactionResult.Deny);
            Assert.NotNull(TransactionResult.EmptyAccount);
            Assert.NotNull(TransactionResult.EmptyDevice);
            Assert.NotNull(TransactionResult.NotExistAction);
            Assert.NotNull(TransactionResult.NotExistSubscriber);
            Assert.NotNull(TransactionResult.UnderMaintenance);
            Assert.NotNull(TransactionResult.Others);
        }

        [Fact]
        public void TransactionResult_Constants_AreNotEmpty()
        {
            // Assert
            Assert.NotEmpty(TransactionResult.Success);
            Assert.NotEmpty(TransactionResult.Allow);
            Assert.NotEmpty(TransactionResult.Review);
            Assert.NotEmpty(TransactionResult.Deny);
            Assert.NotEmpty(TransactionResult.EmptyAccount);
            Assert.NotEmpty(TransactionResult.EmptyDevice);
            Assert.NotEmpty(TransactionResult.NotExistAction);
            Assert.NotEmpty(TransactionResult.NotExistSubscriber);
            Assert.NotEmpty(TransactionResult.UnderMaintenance);
            Assert.NotEmpty(TransactionResult.Others);
        }

        [Fact]
        public void TransactionResult_AllConstants_AreUnique()
        {
            // Arrange
            var constants = new[]
            {
                TransactionResult.Success,
                TransactionResult.Allow,
                TransactionResult.Review,
                TransactionResult.Deny,
                TransactionResult.EmptyAccount,
                TransactionResult.EmptyDevice,
                TransactionResult.NotExistAction,
                TransactionResult.NotExistSubscriber,
                TransactionResult.UnderMaintenance,
                TransactionResult.Others
            };

            // Act
            var uniqueConstants = constants.Distinct().ToArray();

            // Assert
            Assert.Equal(constants.Length, uniqueConstants.Length);
        }

        [Fact]
        public void GetMessage_IsThreadSafe()
        {
            // Arrange
            var tasks = new List<Task<string>>();
            var codes = new[] { "000", "001", "002", "003", "999" };

            // Act - Run multiple concurrent calls
            for (int i = 0; i < 100; i++)
            {
                var code = codes[i % codes.Length];
                tasks.Add(Task.Run(() => TransactionResult.GetMessage(code)));
            }

            var results = Task.WhenAll(tasks).Result;

            // Assert - All tasks should complete successfully
            Assert.Equal(100, results.Length);
            Assert.All(results, result => Assert.NotNull(result));
        }
    }
}
