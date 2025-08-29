using Xunit;
using Device.FingerPrint.Gateway.Model;
using Newtonsoft.Json;
using System;

namespace Device.FingerPrint.Gateway.Tests.Model
{
    public class MobileTransactionModelTests
    {
        [Fact]
        public void MobileTransactionModel_DefaultConstructor_InitializesSuccessfully()
        {
            // Act
            var model = new MobileTransactionModel();

            // Assert
            Assert.NotNull(model);
            Assert.Null(model.Account);
            Assert.Null(model.Subscriber);
            Assert.Null(model.LoginTime);
            Assert.Null(model.Action);
            Assert.Null(model.ActionResult);
            Assert.Null(model.AndroidCode);
            Assert.Null(model.MachineCode);
            Assert.Null(model.MediaDRMCode);
            Assert.Null(model.CustomCode);
            Assert.False(model.IsEmulator);
            Assert.Null(model.ModelName);
            Assert.Null(model.Manufacturer);
            Assert.Null(model.Brand);
            Assert.Null(model.SdkVersion);
            Assert.Null(model.OS);
            Assert.Null(model.Timezone);
            Assert.Null(model.SystemLanguage);
            Assert.Null(model.Country);
            Assert.Null(model.Exceptions);
            Assert.Equal(default(DateTime), model.TransDate);
            Assert.Null(model.DeviceDetails);
        }

        [Fact]
        public void MobileTransactionModel_Properties_CanBeSetAndGet()
        {
            // Arrange
            var model = new MobileTransactionModel();
            var testDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
            var loginTime = new DateTime(2024, 1, 15, 9, 15, 0, DateTimeKind.Utc);

            // Act
            model.Account = "TestAccount";
            model.Subscriber = "TestSubscriber";
            model.LoginTime = loginTime;
            model.Action = "Login";
            model.ActionResult = "Success";
            model.AndroidCode = "android123";
            model.MachineCode = "machine456";
            model.MediaDRMCode = "drm789";
            model.CustomCode = "custom101";
            model.IsEmulator = true;
            model.ModelName = "Samsung Galaxy S21";
            model.Manufacturer = "Samsung";
            model.Brand = "Samsung";
            model.SdkVersion = "30";
            model.OS = "Android 11";
            model.Timezone = "UTC+7";
            model.SystemLanguage = "en-US";
            model.Country = "VN";
            model.Exceptions = "No exceptions";
            model.TransDate = testDate;
            model.DeviceDetails = "Detailed device information";

            // Assert
            Assert.Equal("TestAccount", model.Account);
            Assert.Equal("TestSubscriber", model.Subscriber);
            Assert.Equal(loginTime, model.LoginTime);
            Assert.Equal("Login", model.Action);
            Assert.Equal("Success", model.ActionResult);
            Assert.Equal("android123", model.AndroidCode);
            Assert.Equal("machine456", model.MachineCode);
            Assert.Equal("drm789", model.MediaDRMCode);
            Assert.Equal("custom101", model.CustomCode);
            Assert.True(model.IsEmulator);
            Assert.Equal("Samsung Galaxy S21", model.ModelName);
            Assert.Equal("Samsung", model.Manufacturer);
            Assert.Equal("Samsung", model.Brand);
            Assert.Equal("30", model.SdkVersion);
            Assert.Equal("Android 11", model.OS);
            Assert.Equal("UTC+7", model.Timezone);
            Assert.Equal("en-US", model.SystemLanguage);
            Assert.Equal("VN", model.Country);
            Assert.Equal("No exceptions", model.Exceptions);
            Assert.Equal(testDate, model.TransDate);
            Assert.Equal("Detailed device information", model.DeviceDetails);
        }

        [Fact]
        public void MobileTransactionModel_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var model = new MobileTransactionModel
            {
                Account = "TestAccount",
                Subscriber = "TestSubscriber",
                LoginTime = new DateTime(2024, 1, 15, 9, 15, 0, DateTimeKind.Utc),
                Action = "Login",
                ActionResult = "Success",
                AndroidCode = "android123",
                MachineCode = "machine456",
                MediaDRMCode = "drm789",
                CustomCode = "custom101",
                IsEmulator = true,
                ModelName = "Samsung Galaxy S21",
                Manufacturer = "Samsung",
                Brand = "Samsung",
                SdkVersion = "30",
                OS = "Android 11",
                Timezone = "UTC+7",
                SystemLanguage = "en-US",
                Country = "VN",
                Exceptions = "No exceptions",
                TransDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
                DeviceDetails = "Detailed device information"
            };

            // Act
            var json = JsonConvert.SerializeObject(model);
            var deserializedModel = JsonConvert.DeserializeObject<MobileTransactionModel>(json);

            // Assert
            Assert.NotNull(deserializedModel);
            Assert.Equal(model.Account, deserializedModel.Account);
            Assert.Equal(model.Subscriber, deserializedModel.Subscriber);
            Assert.Equal(model.LoginTime, deserializedModel.LoginTime);
            Assert.Equal(model.Action, deserializedModel.Action);
            Assert.Equal(model.ActionResult, deserializedModel.ActionResult);
            Assert.Equal(model.AndroidCode, deserializedModel.AndroidCode);
            Assert.Equal(model.MachineCode, deserializedModel.MachineCode);
            Assert.Equal(model.MediaDRMCode, deserializedModel.MediaDRMCode);
            Assert.Equal(model.CustomCode, deserializedModel.CustomCode);
            Assert.Equal(model.IsEmulator, deserializedModel.IsEmulator);
            Assert.Equal(model.ModelName, deserializedModel.ModelName);
            Assert.Equal(model.Manufacturer, deserializedModel.Manufacturer);
            Assert.Equal(model.Brand, deserializedModel.Brand);
            Assert.Equal(model.SdkVersion, deserializedModel.SdkVersion);
            Assert.Equal(model.OS, deserializedModel.OS);
            Assert.Equal(model.Timezone, deserializedModel.Timezone);
            Assert.Equal(model.SystemLanguage, deserializedModel.SystemLanguage);
            Assert.Equal(model.Country, deserializedModel.Country);
            Assert.Equal(model.Exceptions, deserializedModel.Exceptions);
            Assert.Equal(model.TransDate, deserializedModel.TransDate);
            Assert.Equal(model.DeviceDetails, deserializedModel.DeviceDetails);
        }

        [Fact]
        public void MobileTransactionModel_JsonDeserialization_WithNullValues_HandlesGracefully()
        {
            // Arrange
            var json = @"{
                ""Account"": null,
                ""Subscriber"": null,
                ""LoginTime"": null,
                ""Action"": null,
                ""ActionResult"": null,
                ""AndroidCode"": null,
                ""MachineCode"": null,
                ""MediaDRMCode"": null,
                ""CustomCode"": null,
                ""IsEmulator"": false,
                ""ModelName"": null,
                ""Manufacturer"": null,
                ""Brand"": null,
                ""SdkVersion"": null,
                ""OS"": null,
                ""Timezone"": null,
                ""SystemLanguage"": null,
                ""Country"": null,
                ""Exceptions"": null,
                ""TransDate"": ""2024-01-15T10:30:00"",
                ""DeviceDetails"": null
            }";

            // Act
            var model = JsonConvert.DeserializeObject<MobileTransactionModel>(json);

            // Assert
            Assert.NotNull(model);
            Assert.Null(model.Account);
            Assert.Null(model.Subscriber);
            Assert.Null(model.LoginTime);
            Assert.Null(model.Action);
            Assert.Null(model.ActionResult);
            Assert.Null(model.AndroidCode);
            Assert.Null(model.MachineCode);
            Assert.Null(model.MediaDRMCode);
            Assert.Null(model.CustomCode);
            Assert.False(model.IsEmulator);
            Assert.Null(model.ModelName);
            Assert.Null(model.Manufacturer);
            Assert.Null(model.Brand);
            Assert.Null(model.SdkVersion);
            Assert.Null(model.OS);
            Assert.Null(model.Timezone);
            Assert.Null(model.SystemLanguage);
            Assert.Null(model.Country);
            Assert.Null(model.Exceptions);
            Assert.Equal(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc), model.TransDate);
            Assert.Null(model.DeviceDetails);
        }

        [Fact]
        public void MobileTransactionModel_JsonDeserialization_WithEmptyStrings_WorksCorrectly()
        {
            // Arrange
            var json = @"{
                ""Account"": """",
                ""Subscriber"": """",
                ""Action"": """",
                ""ActionResult"": """",
                ""AndroidCode"": """",
                ""MachineCode"": """",
                ""MediaDRMCode"": """",
                ""CustomCode"": """",
                ""IsEmulator"": true,
                ""ModelName"": """",
                ""Manufacturer"": """",
                ""Brand"": """",
                ""SdkVersion"": """",
                ""OS"": """",
                ""Timezone"": """",
                ""SystemLanguage"": """",
                ""Country"": """",
                ""Exceptions"": """",
                ""TransDate"": ""2024-01-15T10:30:00"",
                ""DeviceDetails"": """"
            }";

            // Act
            var model = JsonConvert.DeserializeObject<MobileTransactionModel>(json);

            // Assert
            Assert.NotNull(model);
            Assert.Equal(string.Empty, model.Account);
            Assert.Equal(string.Empty, model.Subscriber);
            Assert.Equal(string.Empty, model.Action);
            Assert.Equal(string.Empty, model.ActionResult);
            Assert.Equal(string.Empty, model.AndroidCode);
            Assert.Equal(string.Empty, model.MachineCode);
            Assert.Equal(string.Empty, model.MediaDRMCode);
            Assert.Equal(string.Empty, model.CustomCode);
            Assert.True(model.IsEmulator);
            Assert.Equal(string.Empty, model.ModelName);
            Assert.Equal(string.Empty, model.Manufacturer);
            Assert.Equal(string.Empty, model.Brand);
            Assert.Equal(string.Empty, model.SdkVersion);
            Assert.Equal(string.Empty, model.OS);
            Assert.Equal(string.Empty, model.Timezone);
            Assert.Equal(string.Empty, model.SystemLanguage);
            Assert.Equal(string.Empty, model.Country);
            Assert.Equal(string.Empty, model.Exceptions);
            Assert.Equal(new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc), model.TransDate);
            Assert.Equal(string.Empty, model.DeviceDetails);
        }

        [Fact]
        public void MobileTransactionModel_LoginTime_CanBeNull()
        {
            // Arrange & Act
            var model = new MobileTransactionModel
            {
                LoginTime = null
            };

            // Assert
            Assert.Null(model.LoginTime);
        }

        [Fact]
        public void MobileTransactionModel_LoginTime_CanBeSet()
        {
            // Arrange
            var testDate = new DateTime(2024, 1, 15, 9, 15, 30, DateTimeKind.Utc);
            var model = new MobileTransactionModel();

            // Act
            model.LoginTime = testDate;

            // Assert
            Assert.Equal(testDate, model.LoginTime);
        }

        [Fact]
        public void MobileTransactionModel_IsEmulator_DefaultsToFalse()
        {
            // Arrange & Act
            var model = new MobileTransactionModel();

            // Assert
            Assert.False(model.IsEmulator);
        }

        [Fact]
        public void MobileTransactionModel_IsEmulator_CanBeSetToTrue()
        {
            // Arrange
            var model = new MobileTransactionModel();

            // Act
            model.IsEmulator = true;

            // Assert
            Assert.True(model.IsEmulator);
        }

        [Fact]
        public void MobileTransactionModel_TransDate_CanBeSet()
        {
            // Arrange
            var testDate = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
            var model = new MobileTransactionModel();

            // Act
            model.TransDate = testDate;

            // Assert
            Assert.Equal(testDate, model.TransDate);
        }
    }
}
