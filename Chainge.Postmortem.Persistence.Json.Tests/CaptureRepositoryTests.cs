using System.Text;
using System.Text.Json;
using Chainge.Postmortem.Application.Acquisition.Models;
using Chainge.Postmortem.Persistence.Json.Models;
using Chainge.Postmortem.Persistence.Json.Repositories;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Chainge.Postmortem.Persistence.Json.Tests;

public sealed class CaptureRepositoryTests
{
    [Fact]
    public async Task SaveAttemptAsync_RepeatedQuery_ShouldKeepBothResponsesAndFixedCaptureTime()
    {
        var folder = Path.Combine(Path.GetTempPath(), "chainge-capture-tests", Guid.NewGuid().ToString("N"));
        var instant = DateTimeOffset.Parse("2026-01-02T03:04:05Z", System.Globalization.CultureInfo.InvariantCulture);
        var repository = new CaptureRepository(new CaptureStoreSettings { Directory = folder, Endpoint = new Uri("https://example.invalid") }, new FakeTimeProvider(instant));
        var query = CreateQuery();
        var response = new CaptureResponse { HttpStatus = 200, ValidJson = true, Body = Encoding.UTF8.GetBytes("{\"id\":1,\"result\":[]}") };
        await repository.SaveAttemptAsync(query, response, true, TestContext.Current.CancellationToken);
        var second = response with { Body = Encoding.UTF8.GetBytes("{\"id\":1,\"error\":{}}") };
        var expected = new[] { "{\"id\":1,\"result\":[]}", "{\"id\":1,\"error\":{}}" };

        await repository.SaveAttemptAsync(query, second, false, TestContext.Current.CancellationToken);
        var actual = Directory.GetFiles(folder, "*.response.json").Order(StringComparer.Ordinal).Select(File.ReadAllText).ToArray();
        var metadata = JsonSerializer.Deserialize<CaptureMetadata>(await File.ReadAllTextAsync(Path.Combine(folder, "logs-0002.capture.json"), TestContext.Current.CancellationToken), JsonSerializerOptions.Web)!;

        Assert.Equivalent(expected, actual, strict: true);
        Assert.Equivalent(instant, metadata.CapturedUtc);
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    public async Task HasSuccessfulCaptureAsync_ExistingCapture_ShouldRequireSamePinnedRequestAndEndpoint(bool changeEndpoint, bool changeRequest, bool expected)
    {
        var folder = Path.Combine(Path.GetTempPath(), "chainge-capture-tests", Guid.NewGuid().ToString("N"));
        var settings = new CaptureStoreSettings { Directory = folder, Endpoint = new Uri("https://example.invalid") };
        var clock = new FakeTimeProvider();
        var original = new CaptureRepository(settings, clock);
        var query = CreateQuery();
        await original.SaveAttemptAsync(query, new CaptureResponse { HttpStatus = 200, ValidJson = true, Body = Encoding.UTF8.GetBytes("{\"id\":1,\"result\":[]}") }, true, TestContext.Current.CancellationToken);
        var repository = new CaptureRepository(changeEndpoint ? settings with { Endpoint = new Uri("https://other.invalid") } : settings, clock);
        var lookup = changeRequest ? query with { Request = JsonSerializer.Deserialize<JsonElement>("{\"id\":1,\"method\":\"eth_getLogs\",\"params\":[{\"fromBlock\":\"0x0\",\"toBlock\":\"latest\"}]}") } : query;

        var actual = await repository.HasSuccessfulCaptureAsync(lookup, TestContext.Current.CancellationToken);

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task HasSuccessfulCaptureAsync_ChangedResponse_ShouldRejectTampering()
    {
        var folder = Path.Combine(Path.GetTempPath(), "chainge-capture-tests", Guid.NewGuid().ToString("N"));
        var repository = new CaptureRepository(new CaptureStoreSettings { Directory = folder, Endpoint = new Uri("https://example.invalid") }, new FakeTimeProvider());
        var query = CreateQuery();
        await repository.SaveAttemptAsync(query, new CaptureResponse { HttpStatus = 200, ValidJson = true, Body = Encoding.UTF8.GetBytes("{\"id\":1,\"result\":[]}") }, true, TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(folder, "logs-0001.response.json"), "{}", TestContext.Current.CancellationToken);

        var error = await Record.ExceptionAsync(() => repository.HasSuccessfulCaptureAsync(query, TestContext.Current.CancellationToken));

        Assert.IsType<InvalidDataException>(error);
    }

    private static CaptureQuery CreateQuery()
    {
        return new CaptureQuery { Name = "logs", Request = JsonSerializer.Deserialize<JsonElement>("{\"id\":1,\"method\":\"eth_getLogs\",\"params\":[{\"fromBlock\":\"0x0\",\"toBlock\":\"0x10\"}]}") };
    }
}
