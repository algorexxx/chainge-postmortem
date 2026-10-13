using System.Text.Json;
using Chainge.Postmortem.Application.Acquisition;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Acquisition;

public sealed class CaptureResponseInspectorTests
{
    [Theory]
    [InlineData("{\"id\":\"query\",\"result\":[]}", true)]
    [InlineData("{\"id\":\"query\",\"result\":null}", false)]
    [InlineData("{\"id\":\"query\",\"error\":{}}", false)]
    [InlineData("{\"id\":\"other\",\"result\":[]}", false)]
    [InlineData("{}", false)]
    [InlineData("[]", false)]
    public void IsSuccessful_SingleReplies_ShouldRequireMatchingIdAndNonNullResult(string body, bool expected)
    {
        var query = CaptureFixture.Query();
        var response = CaptureFixture.Response(body);

        var actual = CaptureResponseInspector.IsSuccessful(query, response);

        Assert.Equivalent(expected, actual);
    }

    [Theory]
    [InlineData("[{\"id\":\"two\",\"result\":[]},{\"id\":\"query\",\"result\":[]}]", true)]
    [InlineData("[{\"id\":\"query\",\"result\":[]},{\"id\":\"query\",\"result\":[]}]", false)]
    [InlineData("[{\"id\":\"query\",\"result\":[]},{\"id\":\"two\",\"error\":{}}]", false)]
    public void IsSuccessful_Batches_ShouldBindIdsAndRejectPartialSuccess(string body, bool expected)
    {
        var query = CaptureFixture.Query() with { Request = JsonSerializer.SerializeToElement(new[] { CaptureFixture.Query().Request, CaptureFixture.Query("two").Request }) };
        var response = CaptureFixture.Response(body);

        var actual = CaptureResponseInspector.IsSuccessful(query, response);

        Assert.Equivalent(expected, actual);
    }

    [Theory]
    [InlineData(429, false, "0x1", false)]
    [InlineData(200, false, "0x1", false)]
    [InlineData(200, true, "0x2", false)]
    [InlineData(200, true, "0x1", true)]
    public void IsSuccessful_IdentityAndTransport_ShouldKeepFailureExplicit(int status, bool valid, string expectedIdentity, bool expected)
    {
        var query = CaptureFixture.Query() with { Request = JsonSerializer.SerializeToElement(new { id = "query", method = "eth_chainId" }) };
        var response = CaptureFixture.Response("{\"id\":\"query\",\"result\":\"0x1\"}", status, valid);

        var actual = CaptureResponseInspector.IsSuccessful(query, response, expectedIdentity);

        Assert.Equivalent(expected, actual);
    }

    [Theory]
    [InlineData("eth_getLogs", "{}", false)]
    [InlineData("eth_getTransactionReceipt", "{}", true)]
    [InlineData("eth_getTransactionReceipt", "[]", false)]
    [InlineData("eth_call", "\"bad\"", false)]
    [InlineData("unknown", "[]", false)]
    public void IsSuccessful_ResultShape_ShouldMatchMethod(string method, string result, bool expected)
    {
        var query = CaptureFixture.Query() with { Request = JsonSerializer.SerializeToElement(new { id = "query", method }) };
        var response = CaptureFixture.Response("{\"id\":\"query\",\"result\":" + result + "}");

        var actual = CaptureResponseInspector.IsSuccessful(query, response);

        Assert.Equivalent(expected, actual);
    }
}
