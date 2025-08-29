using Xunit;
using Device.FingerPrint.Gateway.Model;
using Newtonsoft.Json;

namespace Device.FingerPrint.Gateway.Tests.Model
{
    public class SubscriberModelTests
    {
        [Fact]
        public void SubscriberModel_DefaultConstructor_InitializesSuccessfully()
        {
            // Act
            var model = new SubscriberModel();

            // Assert
            Assert.NotNull(model);
            Assert.Equal(0, model.SubscriberId);
            Assert.Null(model.SubscriberName);
        }

        [Fact]
        public void SubscriberModel_Properties_CanBeSetAndGet()
        {
            // Arrange
            var model = new SubscriberModel();
            var subscriberId = 12345;
            var subscriberName = "TestSubscriber";

            // Act
            model.SubscriberId = subscriberId;
            model.SubscriberName = subscriberName;

            // Assert
            Assert.Equal(subscriberId, model.SubscriberId);
            Assert.Equal(subscriberName, model.SubscriberName);
        }

        [Fact]
        public void SubscriberModel_SubscriberId_CanBeSetToZero()
        {
            // Arrange
            var model = new SubscriberModel();

            // Act
            model.SubscriberId = 0;

            // Assert
            Assert.Equal(0, model.SubscriberId);
        }

        [Fact]
        public void SubscriberModel_SubscriberId_CanBeSetToNegativeValue()
        {
            // Arrange
            var model = new SubscriberModel();

            // Act
            model.SubscriberId = -1;

            // Assert
            Assert.Equal(-1, model.SubscriberId);
        }

        [Fact]
        public void SubscriberModel_SubscriberId_CanBeSetToLargeValue()
        {
            // Arrange
            var model = new SubscriberModel();
            var largeId = int.MaxValue;

            // Act
            model.SubscriberId = largeId;

            // Assert
            Assert.Equal(largeId, model.SubscriberId);
        }

        [Fact]
        public void SubscriberModel_SubscriberName_CanBeSetToNull()
        {
            // Arrange
            var model = new SubscriberModel();

            // Act
            model.SubscriberName = null!;

            // Assert
            Assert.Null(model.SubscriberName);
        }

        [Fact]
        public void SubscriberModel_SubscriberName_CanBeSetToEmptyString()
        {
            // Arrange
            var model = new SubscriberModel();

            // Act
            model.SubscriberName = string.Empty;

            // Assert
            Assert.Equal(string.Empty, model.SubscriberName);
        }

        [Fact]
        public void SubscriberModel_SubscriberName_CanBeSetToWhitespace()
        {
            // Arrange
            var model = new SubscriberModel();
            var whitespace = "   ";

            // Act
            model.SubscriberName = whitespace;

            // Assert
            Assert.Equal(whitespace, model.SubscriberName);
        }

        [Fact]
        public void SubscriberModel_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var model = new SubscriberModel
            {
                SubscriberId = 12345,
                SubscriberName = "TestSubscriber"
            };

            // Act
            var json = JsonConvert.SerializeObject(model);
            var deserializedModel = JsonConvert.DeserializeObject<SubscriberModel>(json);

            // Assert
            Assert.NotNull(deserializedModel);
            Assert.Equal(model.SubscriberId, deserializedModel.SubscriberId);
            Assert.Equal(model.SubscriberName, deserializedModel.SubscriberName);
        }

        [Fact]
        public void SubscriberModel_JsonDeserialization_WithNullName_HandlesGracefully()
        {
            // Arrange
            var json = @"{
                ""SubscriberId"": 12345,
                ""SubscriberName"": null
            }";

            // Act
            var model = JsonConvert.DeserializeObject<SubscriberModel>(json);

            // Assert
            Assert.NotNull(model);
            Assert.Equal(12345, model.SubscriberId);
            Assert.Null(model.SubscriberName);
        }

        [Fact]
        public void SubscriberModel_JsonDeserialization_WithEmptyName_WorksCorrectly()
        {
            // Arrange
            var json = @"{
                ""SubscriberId"": 12345,
                ""SubscriberName"": """"
            }";

            // Act
            var model = JsonConvert.DeserializeObject<SubscriberModel>(json);

            // Assert
            Assert.NotNull(model);
            Assert.Equal(12345, model.SubscriberId);
            Assert.Equal(string.Empty, model.SubscriberName);
        }

        [Fact]
        public void SubscriberModel_JsonDeserialization_WithZeroId_WorksCorrectly()
        {
            // Arrange
            var json = @"{
                ""SubscriberId"": 0,
                ""SubscriberName"": ""TestSubscriber""
            }";

            // Act
            var model = JsonConvert.DeserializeObject<SubscriberModel>(json);

            // Assert
            Assert.NotNull(model);
            Assert.Equal(0, model.SubscriberId);
            Assert.Equal("TestSubscriber", model.SubscriberName);
        }

        [Fact]
        public void SubscriberModel_JsonDeserialization_WithNegativeId_WorksCorrectly()
        {
            // Arrange
            var json = @"{
                ""SubscriberId"": -1,
                ""SubscriberName"": ""TestSubscriber""
            }";

            // Act
            var model = JsonConvert.DeserializeObject<SubscriberModel>(json);

            // Assert
            Assert.NotNull(model);
            Assert.Equal(-1, model.SubscriberId);
            Assert.Equal("TestSubscriber", model.SubscriberName);
        }

        [Fact]
        public void SubscriberModel_JsonDeserialization_WithLargeId_WorksCorrectly()
        {
            // Arrange
            var json = $@"{{
                ""SubscriberId"": {int.MaxValue},
                ""SubscriberName"": ""TestSubscriber""
            }}";

            // Act
            var model = JsonConvert.DeserializeObject<SubscriberModel>(json);

            // Assert
            Assert.NotNull(model);
            Assert.Equal(int.MaxValue, model.SubscriberId);
            Assert.Equal("TestSubscriber", model.SubscriberName);
        }

        [Fact]
        public void SubscriberModel_JsonDeserialization_WithMissingProperties_UsesDefaults()
        {
            // Arrange
            var json = @"{}";

            // Act
            var model = JsonConvert.DeserializeObject<SubscriberModel>(json);

            // Assert
            Assert.NotNull(model);
            Assert.Equal(0, model.SubscriberId);
            Assert.Null(model.SubscriberName);
        }

        [Fact]
        public void SubscriberModel_JsonSerialization_WithSpecialCharacters_WorksCorrectly()
        {
            // Arrange
            var model = new SubscriberModel
            {
                SubscriberId = 12345,
                SubscriberName = "Test@Subscriber#123!$%"
            };

            // Act
            var json = JsonConvert.SerializeObject(model);
            var deserializedModel = JsonConvert.DeserializeObject<SubscriberModel>(json);

            // Assert
            Assert.NotNull(deserializedModel);
            Assert.Equal(model.SubscriberId, deserializedModel.SubscriberId);
            Assert.Equal(model.SubscriberName, deserializedModel.SubscriberName);
        }

        [Fact]
        public void SubscriberModel_JsonSerialization_WithUnicodeCharacters_WorksCorrectly()
        {
            // Arrange
            var model = new SubscriberModel
            {
                SubscriberId = 12345,
                SubscriberName = "Tên người dùng với ký tự Unicode"
            };

            // Act
            var json = JsonConvert.SerializeObject(model);
            var deserializedModel = JsonConvert.DeserializeObject<SubscriberModel>(json);

            // Assert
            Assert.NotNull(deserializedModel);
            Assert.Equal(model.SubscriberId, deserializedModel.SubscriberId);
            Assert.Equal(model.SubscriberName, deserializedModel.SubscriberName);
        }
    }
}
