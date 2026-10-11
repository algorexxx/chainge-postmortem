using Chainge.Postmortem.Application.Reconciliation.Models;
using Chainge.Postmortem.Application.Reconciliation.Models.Extensions;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Reconciliation;

public sealed class BurnInventoryReviewExtensionsTests
{
    [Theory]
    [InlineData("controller-observed-balance-clearing", "inventory-settlement-review")]
    [InlineData("controller-burn-unresolved", "unresolved-burn-review")]
    [InlineData("holder-burn-unresolved", "unresolved-burn-review")]
    [InlineData("unknown", "unresolved-burn-review")]
    public void GetReviewQueue_Classification_ShouldOnlySeparateObservedClearing(string classification, string expected)
    {
        var review = new BurnInventoryReview { BurnEventId = "burn", Classification = classification };

        var actual = review.GetReviewQueue();

        Assert.Equivalent(expected, actual);
    }
}
