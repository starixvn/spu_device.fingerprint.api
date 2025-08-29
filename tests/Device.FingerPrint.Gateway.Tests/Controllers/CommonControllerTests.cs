using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Device.FingerPrint.Gateway.Controllers;
using Device.FingerPrint.Gateway.Model;
using Device.FingerPrint.Gateway;
using System;
using System.Net;

namespace Device.FingerPrint.Gateway.Tests.Controllers
{
    public class CommonControllerTests
    {
        private readonly Mock<IAppSettings> _mockAppSettings;
        private readonly DefaultHttpContext _httpContext;
        private readonly CommonController _controller;

        public CommonControllerTests()
        {
        _mockAppSettings = new Mock<IAppSettings>();
        _mockAppSettings.SetupGet(a => a.Environment).Returns("TestEnv");

        _httpContext = new DefaultHttpContext();

        // Setup headers
        _httpContext.Request.Headers["X-Remote-IP"] = "1.2.3.4";
        _httpContext.Request.Headers["X-Actual-IP"] = "5.6.7.8";
        _httpContext.Request.Headers["X-Forwarded-For"] = "9.10.11.12,13.14.15.16";
        _httpContext.Request.Headers["X-Originating-IP"] = "17.18.19.20";
        _httpContext.Request.Headers["X-Remote-Addr"] = "21.22.23.24";

        // Setup connection feature
        var connectionFeature = new Mock<IHttpConnectionFeature>();
        connectionFeature.SetupGet(f => f.LocalIpAddress).Returns(IPAddress.Parse("127.0.0.1"));
        _httpContext.Features.Set<IHttpConnectionFeature>(connectionFeature.Object);

        _controller = new CommonController(_mockAppSettings.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };
    }

    [Fact]
    public void Index_ReturnsCorrectWhoamiModel()
    {
        // Act
        var result = _controller.Index();

        // Assert
        Assert.Equal("TestEnv", result.Environment);
        Assert.Equal(Environment.MachineName, result.ServerName);
        Assert.NotEqual(default(DateTime), result.ServerTime);
        Assert.Equal("127.0.0.1", result.LocalIP);
        Assert.Equal("1.2.3.4", result.ClientIP);
        Assert.Equal("9.10.11.12", result.FakeIP);
        Assert.Equal("5.6.7.8", result.RealIP);
        Assert.Equal("9.10.11.12,13.14.15.16", result.XFW);
        Assert.Equal("5.6.7.8", result.ActualIP);
    }

    [Fact]
    public void Index_HandlesMissingHeadersGracefully()
    {
        // Remove headers
        _httpContext.Request.Headers.Clear();

        var result = _controller.Index();

        Assert.Equal(string.Empty, result.ClientIP);
        Assert.Equal(string.Empty, result.FakeIP);
        Assert.Equal(_httpContext.Request.Host.ToString(), result.RealIP);
        Assert.Equal(_httpContext.Request.Host.ToString(), result.XFW);
        Assert.Equal(string.Empty, result.ActualIP);
    }

    [Fact]
    public void Index_WithOnlyXActualIPHeader_UsesActualIPForRealIP()
    {
        // Clear headers and set only X-Actual-IP
        _httpContext.Request.Headers.Clear();
        _httpContext.Request.Headers["X-Actual-IP"] = "10.20.30.40";

        var result = _controller.Index();

        Assert.Equal("10.20.30.40", result.RealIP);
        Assert.Equal("10.20.30.40", result.ActualIP);
        Assert.Equal(string.Empty, result.FakeIP);
    }
}
}