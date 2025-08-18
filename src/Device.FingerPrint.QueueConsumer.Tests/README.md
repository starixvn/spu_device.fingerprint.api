# Device.FingerPrint.QueueConsumer.Tests

Đây là project Unit Test cho Device.FingerPrint.QueueConsumer, sử dụng xUnit và NSubstitute.

## Cấu trúc Test

### Test Files
- **SimpleTest.cs**: Test cơ bản để kiểm tra xUnit framework hoạt động
- **UtilitiesTests.cs**: Test cho Utilities class - các helper method

## Cách chạy Test

### Sử dụng Visual Studio
1. Mở solution trong Visual Studio
2. Mở Test Explorer (Test > Test Explorer)
3. Chạy tất cả test hoặc chạy từng test riêng lẻ

### Sử dụng Command Line
```bash
# Chạy tất cả test
dotnet test

# Chạy test với coverage
dotnet test --collect:"XPlat Code Coverage"

# Chạy test với output chi tiết
dotnet test --logger "console;verbosity=detailed"

# Chạy test cụ thể
dotnet test --filter "FullyQualifiedName~SimpleTest"
```

### Sử dụng Visual Studio Code
1. Cài đặt extension C# và .NET Core Test Explorer
2. Mở Command Palette (Ctrl+Shift+P)
3. Chạy lệnh ".NET Core Test Explorer: Run All Tests"

## Test Coverage

Các test case hiện tại bao gồm:

### Simple Tests
- ✅ Basic arithmetic test
- ✅ Theory test với multiple test cases

### Utilities Tests
- ✅ MD5 hash generation với valid object
- ✅ MD5 hash generation với null object
- ✅ JSON validation với various inputs

## Dependencies

- **xUnit**: Framework test
- **NSubstitute**: Mocking framework
- **Microsoft.NET.Test.Sdk**: Test SDK

## Best Practices

1. **Arrange-Act-Assert**: Mỗi test method tuân theo pattern AAA
2. **Test Isolation**: Mỗi test độc lập, không phụ thuộc vào test khác
3. **Meaningful Names**: Tên test method mô tả rõ scenario và expected result
4. **Mock Dependencies**: Sử dụng NSubstitute để mock external dependencies
5. **Exception Testing**: Test cả happy path và exception scenarios

## Notes

- Tất cả test đều sử dụng Vietnamese comments để dễ hiểu
- Test data được tạo với các giá trị realistic
- Exception handling được test kỹ lưỡng
- Project đã được thêm vào solution file

## Kết quả Test

```
Test summary: total: 12, failed: 0, succeeded: 12, skipped: 0, duration: 0.8s
```

Tất cả test đều pass thành công!
