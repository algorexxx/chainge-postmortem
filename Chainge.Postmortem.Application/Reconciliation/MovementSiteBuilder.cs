using System.Globalization;
using System.Numerics;
using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Application.Reconciliation;

public static class MovementSiteBuilder
{
    public static MovementSiteReport BuildReport(IReadOnlyList<ChainEvent> events, ReconciliationResult result)
    {
        var byId = events.ToDictionary(e => e.Id, StringComparer.Ordinal);
        var cases = result.Movements.Select(review => BuildCase(byId[review.SourceEventId], review)).ToList();
        var destinations = cases.Where(c => c.Review.Direction == "outgoing")
            .GroupBy(c => new { c.Chain, c.Token, c.To, c.Decimals })
            .Select(group => new DestinationSummary
            {
                Chain = group.Key.Chain, Token = group.Key.Token, Address = group.Key.To, Decimals = group.Key.Decimals,
                Count = group.Count(), GrossOutflowRaw = SumAmounts(group),
                UnmatchedCount = group.Count(c => c.Review.Classification == "unmatched"),
                UnmatchedOutflowRaw = SumAmounts(group.Where(c => c.Review.Classification == "unmatched"))
            }).ToList();
        return new MovementSiteReport { Cases = cases, Destinations = destinations };
    }

    private static MovementSiteCase BuildCase(ChainEvent e, MovementReview review)
    {
        return new MovementSiteCase
        {
            Id = e.Id, Chain = e.Chain, Token = e.Token, Transaction = e.Transaction, From = e.From, To = e.To,
            RawAmount = e.RawAmount, Decimals = e.Decimals, Timestamp = e.Timestamp, ReceiptVerified = e.ReceiptVerified,
            Evidence = e.Evidence, Review = review, DepletionBasisPoints = GetDepletion(e, review)
        };
    }

    private static int? GetDepletion(ChainEvent e, MovementReview review)
    {
        if (review.Direction != "outgoing" || review.ReconstructedBalanceBeforeRaw is null || review.Flags.Contains("inconsistent-balance-history") || review.Flags.Contains("negative-reconstructed-balance")) { return null; }
        var before = BigInteger.Parse(review.ReconstructedBalanceBeforeRaw, CultureInfo.InvariantCulture);
        var value = BigInteger.Parse(e.RawAmount, CultureInfo.InvariantCulture);
        if (before <= 0 || value < 0 || value > before) { return null; }
        return (int)(value * 10000 / before);
    }

    private static string SumAmounts(IEnumerable<MovementSiteCase> cases)
    {
        return cases.Aggregate(BigInteger.Zero, (total, c) => total + BigInteger.Parse(c.RawAmount, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture);
    }
}
