using Chainge.Postmortem.Application.Reconciliation;
using Chainge.Postmortem.Application.Reconciliation.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class BalanceReviewerTests
{
    [Theory]
    [InlineData("100", false, true, "net-agrees-coverage-incomplete")]
    [InlineData("100", true, true, "net-agrees-covered")]
    [InlineData("99", true, false, "balance-mismatch")]
    [InlineData(null, true, false, "state-unavailable")]
    public void ReviewBalances_StateAndCoverage_ShouldKeepIndependentResults(string? observed, bool covered, bool agrees, string status)
    {
        var transfer = new ChainEvent { Id = "one", Chain = "chain", Token = "token", Transaction = "tx", BlockHash = "hash", Block = 1, LogIndex = 0, From = "0x0000000000000000000000000000000000000000", To = "holder", RawAmount = "100", Decimals = 6, TimestampSource = "fixture", ReceiptVerified = true, FullTime = true, Evidence = [] };
        var observation = new BalanceObservation { Chain = "chain", Token = "token", Holder = "holder", Block = 2, RawAmount = observed, Source = "state" };
        var incoming = new LogQueryWindow { Chain = "chain", Token = "token", Holder = "holder", Range = new BlockRange { From = 0, Through = 2 }, Direction = "incoming", Succeeded = covered, Source = "query" };
        var expected = new[] { new BalanceCheck { Observation = observation, ReplayedRawAmount = "100", NetAgrees = agrees, HistoryConsistent = true, Status = status } };

        var actual = BalanceReviewer.ReviewBalances([transfer], [observation], [incoming, incoming with { Direction = "outgoing" }]);

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public void ReviewBalances_EmptyAndRecoveredHistory_ShouldNotCertifyEither()
    {
        var outgoing = new ChainEvent { Id = "one", Chain = "chain", Token = "token", Transaction = "tx", BlockHash = "hash", Block = 1, LogIndex = 0, From = "holder", To = "other", RawAmount = "100", Decimals = 6, TimestampSource = "fixture", ReceiptVerified = true, FullTime = true, Evidence = [] };
        var incoming = outgoing with { Id = "two", Block = 2, From = "other", To = "holder" };
        var empty = new BalanceObservation { Chain = "chain", Token = "token", Holder = "absent", Block = 2, RawAmount = "0", Source = "state" };
        var recovered = empty with { Holder = "holder" };
        var expected = new[] { new BalanceCheck { Observation = empty, ReplayedRawAmount = "0", NetAgrees = true, HistoryConsistent = true, Status = "net-agrees-coverage-incomplete" }, new BalanceCheck { Observation = recovered, ReplayedRawAmount = "0", NetAgrees = true, HistoryConsistent = false, Status = "net-agrees-inconsistent-history" } };

        var actual = BalanceReviewer.ReviewBalances([outgoing, incoming], [empty, recovered], []);

        Assert.Equivalent(expected, actual, strict: true);
    }
}
