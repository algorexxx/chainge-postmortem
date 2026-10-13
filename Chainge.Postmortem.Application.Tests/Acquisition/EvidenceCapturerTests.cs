using Chainge.Postmortem.Application.Acquisition;
using Chainge.Postmortem.Application.Acquisition.Interfaces;
using Chainge.Postmortem.Application.Acquisition.Models;
using NSubstitute;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Acquisition;

public sealed class EvidenceCapturerTests
{
    [Fact]
    public async Task CaptureEvidence_CachedAndNewResponses_ShouldResumeWithoutRepeatingSuccess()
    {
        var cached = CaptureFixture.Query("cached");
        var fresh = CaptureFixture.Query("fresh");
        var plan = CaptureFixture.Plan(cached, fresh);
        var client = Substitute.For<ICaptureClient>();
        var repository = Substitute.For<ICaptureRepository>();
        var progress = Substitute.For<IProgress<CaptureProgress>>();
        repository.HasSuccessfulCaptureAsync(cached, Arg.Any<CancellationToken>()).Returns(true);
        client.PostAsync(Arg.Is<CaptureQuery>(q => q.Name == "chain-id"), Arg.Any<CancellationToken>()).Returns(CaptureFixture.Response("{\"id\":\"chain-id\",\"result\":\"0x1\"}"));
        client.PostAsync(fresh, Arg.Any<CancellationToken>()).Returns(CaptureFixture.Response("{\"id\":\"fresh\",\"result\":[]}"));
        var capturer = new EvidenceCapturer(client, repository);
        var expected = new CaptureSummary { IdentityVerified = true, Captured = 1, Reused = 1, Failed = 0, Remaining = 0 };

        var actual = await capturer.CaptureEvidence(plan, progress, TestContext.Current.CancellationToken);

        Assert.Equivalent(expected, actual, strict: true);
        await client.DidNotReceive().PostAsync(cached, Arg.Any<CancellationToken>());
        await repository.Received(1).WriteManifestAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CaptureEvidence_WrongChain_ShouldCaptureFailureAndStopBeforeQueries()
    {
        var query = CaptureFixture.Query();
        var plan = CaptureFixture.Plan(query);
        var client = Substitute.For<ICaptureClient>();
        var repository = Substitute.For<ICaptureRepository>();
        client.PostAsync(Arg.Any<CaptureQuery>(), Arg.Any<CancellationToken>()).Returns(CaptureFixture.Response("{\"id\":\"chain-id\",\"result\":\"0x2\"}"));
        var capturer = new EvidenceCapturer(client, repository);
        var expected = new CaptureSummary { IdentityVerified = false, Captured = 0, Reused = 0, Failed = 0, Remaining = 1 };

        var actual = await capturer.CaptureEvidence(plan, null, TestContext.Current.CancellationToken);

        Assert.Equivalent(expected, actual, strict: true);
        await client.DidNotReceive().PostAsync(query, Arg.Any<CancellationToken>());
        await repository.Received(1).SaveAttemptAsync(Arg.Any<CaptureQuery>(), Arg.Any<CaptureResponse>(), false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CaptureEvidence_RepeatedFailure_ShouldStopWithExplicitRemainingCount()
    {
        var first = CaptureFixture.Query("first");
        var second = CaptureFixture.Query("second");
        var last = CaptureFixture.Query("last");
        var plan = CaptureFixture.Plan(first, second, last);
        var client = Substitute.For<ICaptureClient>();
        var repository = Substitute.For<ICaptureRepository>();
        client.PostAsync(Arg.Any<CaptureQuery>(), Arg.Any<CancellationToken>()).Returns(CaptureFixture.Response("rate limited", 429, false));
        client.PostAsync(Arg.Is<CaptureQuery>(q => q.Name == "chain-id"), Arg.Any<CancellationToken>()).Returns(CaptureFixture.Response("{\"id\":\"chain-id\",\"result\":\"0x1\"}"));
        var capturer = new EvidenceCapturer(client, repository);
        var expected = new CaptureSummary { IdentityVerified = true, Captured = 0, Reused = 0, Failed = 2, Remaining = 3 };

        var actual = await capturer.CaptureEvidence(plan, null, TestContext.Current.CancellationToken);

        Assert.Equivalent(expected, actual, strict: true);
        await client.DidNotReceive().PostAsync(last, Arg.Any<CancellationToken>());
    }
}
