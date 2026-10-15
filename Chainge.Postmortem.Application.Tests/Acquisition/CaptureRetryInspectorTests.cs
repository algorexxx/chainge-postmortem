using Chainge.Postmortem.Application.Acquisition;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Acquisition;

public sealed class CaptureRetryInspectorTests
{
    [Theory]
    [InlineData(429, "busy", 0, 60)]
    [InlineData(503, "busy", 2, 240)]
    [InlineData(0, "{}", 8, 960)]
    [InlineData(200, "{\"error\":{\"code\":-32029,\"data\":{\"retry_after_ms\":125000}}}", 0, 125)]
    [InlineData(200, "[{\"error\":{\"code\":-32029}},{\"result\":[]}]", 0, 60)]
    [InlineData(200, "{\"error\":{\"data\":{\"retry_after_ms\":-1}}}", 0, null)]
    [InlineData(200, "{\"error\":{\"code\":-32601}}", 0, null)]
    [InlineData(200, "{\"result\":\"0x2\"}", 0, null)]
    [InlineData(403, "denied", 0, null)]
    [InlineData(200, "null", 0, null)]
    public void GetRetryDelay_Response_ShouldRespectProviderAndBackoff(int status, string body, int attempt, int? seconds)
    {
        var response = CaptureFixture.Response(body, status);
        var expected = seconds.HasValue ? TimeSpan.FromSeconds(seconds.Value) : (TimeSpan?)null;

        var actual = CaptureRetryInspector.GetRetryDelay(response, TimeSpan.FromMinutes(1), attempt);

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void GetRetryDelay_HeaderLongerThanBody_ShouldHonorHeader()
    {
        var response = CaptureFixture.Response("{\"error\":{\"data\":{\"retry_after_ms\":90000}}}", 429) with { RetryAfter = TimeSpan.FromMinutes(3) };
        var expected = TimeSpan.FromMinutes(3);

        var actual = CaptureRetryInspector.GetRetryDelay(response, TimeSpan.FromMinutes(1), 0);

        Assert.Equivalent(expected, actual);
    }
}
