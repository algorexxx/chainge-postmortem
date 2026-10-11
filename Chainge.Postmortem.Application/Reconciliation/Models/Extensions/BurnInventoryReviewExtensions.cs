namespace Chainge.Postmortem.Application.Reconciliation.Models.Extensions;

public static class BurnInventoryReviewExtensions
{
    public static string GetReviewQueue(this BurnInventoryReview review)
    {
        return review.Classification == "controller-observed-balance-clearing"
            ? "inventory-settlement-review"
            : "unresolved-burn-review";
    }
}
