using Device.FingerPrint.Gateway.Middleware;
using Fanex.Logging;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Device.FingerPrint.Gateway.Tests.Middleware
{
    public class ExceptionMiddlewareTests
    {
        private readonly Mock<RequestDelegate> _mockNext;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly ExceptionMiddleware _middleware;

        public ExceptionMiddlewareTests()
        {
            _mockNext = new Mock<RequestDelegate>();
            _mockLogger = new Mock<ILogger>();
            _mockHttpContext = new Mock<HttpContext>();
            _middleware = new ExceptionMiddleware(_mockNext.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task InvokeAsync_WhenNoExceptionThrown_ShouldCallNextDelegate()
        {
            // Arrange
            _mockNext.Setup(x => x(_mockHttpContext.Object))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_mockHttpContext.Object);

            // Assert
            _mockNext.Verify(x => x(_mockHttpContext.Object), Times.Once);
            _mockLogger.Verify(x => x.ErrorAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WhenIOExceptionThrown_ShouldLogIOException()
        {
            // Arrange
            var ioException = new IOException("IO operation failed");
            _mockNext.Setup(x => x(_mockHttpContext.Object))
                .ThrowsAsync(ioException);

            // Act
            await _middleware.InvokeAsync(_mockHttpContext.Object);

            // Assert
            _mockNext.Verify(x => x(_mockHttpContext.Object), Times.Once);
            _mockLogger.Verify(x => x.ErrorAsync(It.Is<string>(s => s.Contains("IO operation failed"))), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WhenIOExceptionThrown_ShouldLogWithExceptionInfo()
        {
            // Arrange
            var ioException = new IOException("IO operation failed");
            _mockNext.Setup(x => x(_mockHttpContext.Object))
                .ThrowsAsync(ioException);

            // Act
            await _middleware.InvokeAsync(_mockHttpContext.Object);

            // Assert
            _mockLogger.Verify(x => x.ErrorAsync(It.Is<string>(s =>
                s.Contains("IO operation failed") &&
                s.Contains("EXCEPTION INFO"))), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WhenGeneralExceptionThrown_ShouldLogGeneralException()
        {
            // Arrange
            var generalException = new InvalidOperationException("Invalid operation");
            _mockNext.Setup(x => x(_mockHttpContext.Object))
                .ThrowsAsync(generalException);

            // Act
            await _middleware.InvokeAsync(_mockHttpContext.Object);

            // Assert
            _mockNext.Verify(x => x(_mockHttpContext.Object), Times.Once);
            _mockLogger.Verify(x => x.ErrorAsync(It.Is<string>(s => s.Contains("Invalid operation"))), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WhenGeneralExceptionThrown_ShouldLogWithServerAndExceptionInfo()
        {
            // Arrange
            var generalException = new ArgumentException("Invalid argument");
            _mockNext.Setup(x => x(_mockHttpContext.Object))
                .ThrowsAsync(generalException);

            // Act
            await _middleware.InvokeAsync(_mockHttpContext.Object);

            // Assert
            _mockLogger.Verify(x => x.ErrorAsync(It.Is<string>(s =>
                s.Contains("Invalid argument") &&
                s.Contains("SERVER INFO") &&
                s.Contains("EXCEPTION INFO"))), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WhenIOExceptionThrown_ShouldNotLogServerInfo()
        {
            // Arrange
            var ioException = new IOException("IO operation failed");
            _mockNext.Setup(x => x(_mockHttpContext.Object))
                .ThrowsAsync(ioException);

            // Act
            await _middleware.InvokeAsync(_mockHttpContext.Object);

            // Assert
            _mockLogger.Verify(x => x.ErrorAsync(It.Is<string>(s =>
                s.Contains("IO operation failed") &&
                !s.Contains("SERVER INFO") &&
                s.Contains("EXCEPTION INFO"))), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WhenExceptionThrownWithNullMessage_ShouldHandleGracefully()
        {
            // Arrange
            var exception = new Exception(null);
            _mockNext.Setup(x => x(_mockHttpContext.Object))
                .ThrowsAsync(exception);

            // Act
            await _middleware.InvokeAsync(_mockHttpContext.Object);

            // Assert
            _mockNext.Verify(x => x(_mockHttpContext.Object), Times.Once);
            _mockLogger.Verify(x => x.ErrorAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WhenMultipleExceptionsThrown_ShouldHandleInCorrectOrder()
        {
            // Arrange
            // IOException should be caught first before general Exception
            var ioException = new IOException("IO specific error");
            _mockNext.Setup(x => x(_mockHttpContext.Object))
                .ThrowsAsync(ioException);

            // Act
            await _middleware.InvokeAsync(_mockHttpContext.Object);

            // Assert
            // Verify that IOException handler was used (no Server Info section)
            _mockLogger.Verify(x => x.ErrorAsync(It.Is<string>(s =>
                s.Contains("IO specific error") &&
                !s.Contains("SERVER INFO"))), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WhenNextDelegateIsNull_ShouldThrowException()
        {
            // Arrange
            var middleware = new ExceptionMiddleware(null!, _mockLogger.Object);

            // Act & Assert
            // When _next is null, calling it will throw NullReferenceException, which gets caught by general exception handler
            await middleware.InvokeAsync(_mockHttpContext.Object);
            
            // Verify that logger was called due to the exception
            _mockLogger.Verify(x => x.ErrorAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WhenHttpContextIsNull_ShouldPassToNextDelegate()
        {
            // Arrange
            _mockNext.Setup(x => x(null!))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(null!);

            // Assert
            _mockNext.Verify(x => x(null!), Times.Once);
        }
    }
}