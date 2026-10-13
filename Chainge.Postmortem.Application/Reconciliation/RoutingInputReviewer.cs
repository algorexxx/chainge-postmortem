using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using Chainge.Postmortem.Application.Reconciliation.Models;

namespace Chainge.Postmortem.Application.Reconciliation;

public static partial class RoutingInputReviewer
{
    private const string Zero = "0x0000000000000000000000000000000000000000";

    public static IReadOnlyList<RoutingObservation> ReviewInputs(IReadOnlyList<ChainEvent> events, MatchPolicy policy)
    {
        return events.Where(e => e.Chain == policy.FusionChain && e.To == Zero && e.From != Zero && policy.RoutingCallTargets.Contains(e.ReceiptTarget, StringComparer.OrdinalIgnoreCase) && e.TransactionInput.StartsWith("0x3f163c47", StringComparison.Ordinal))
            .Select(e => DecodeInput(e)).Select(r => FindCandidates(r, events, policy)).OrderBy(r => r.EventId, StringComparer.Ordinal).ToList();
    }

    private static RoutingObservation DecodeInput(ChainEvent e)
    {
        var rejected = new RoutingObservation { EventId = e.Id, Status = "invalid-or-unbound-call" };
        var hex = e.TransactionInput[10..];
        if (!e.ReceiptVerified || !e.ReceiptContextCaptured || !e.FullTime || e.TransactionSender != e.From || hex.Length < 320 || hex.Length % 64 != 0 || !hex.All(Uri.IsHexDigit)) { return rejected; }
        if (hex[..24] != new string('0', 24) || string.Concat("0x", hex.AsSpan(24, 40)) != e.Token || ReadInteger(hex.Substring(64, 64)).ToString(CultureInfo.InvariantCulture) != e.RawAmount || ReadInteger(hex.Substring(128, 64)) != 1 || ReadInteger(hex.Substring(192, 64)) != 128) { return rejected; }
        var length = ReadInteger(hex.Substring(256, 64));
        if (length <= 0 || length > (hex.Length - 320) / 2) { return rejected; }
        var count = (int)length;
        var padding = hex[(320 + count * 2)..];
        if (padding.Length >= 64 || padding.Any(c => c != '0')) { return rejected; }
        var bytes = Convert.FromHexString(hex.AsSpan(320, count * 2));
        if (bytes.Any(b => b < 32 || b > 126)) { return rejected; }
        return DecodeMemo(e.Id, Encoding.ASCII.GetString(bytes));
    }

    private static BigInteger ReadInteger(string word)
    {
        return BigInteger.Parse("0" + word, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }

    private static RoutingObservation DecodeMemo(string id, string memo)
    {
        if (memo.StartsWith('{')) { return DecodeJsonMemo(id, memo); }
        var unsupported = new RoutingObservation { EventId = id, Status = "unsupported-memo" };
        var fields = memo.Split(':');
        if (fields.Length < 6 || fields[0] != "chaingeappv1" || fields[1].Length == 0 || !fields[1].All(char.IsAsciiLetterOrDigit) || fields[2].Length == 0 || fields[3].Length == 0) { return unsupported; }
        var recipient = string.Join(':', fields[3..^2]);
        return new RoutingObservation { EventId = id, Status = "observed-colon-memo", ChainCode = fields[1], Token = fields[2], Recipient = recipient, MemoAmountRaw = ReadMemoAmount(fields[^1]) };
    }

    private static string? ReadMemoAmount(string extra)
    {
        var amounts = extra.Split(';').Where(f => f.StartsWith("1_", StringComparison.Ordinal)).ToList();
        var amount = amounts.Count == 1 ? amounts[0][2..] : string.Empty;
        return amount.Length > 0 && amount.All(char.IsAsciiDigit) ? BigInteger.Parse(amount, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) : null;
    }

    private static RoutingObservation DecodeJsonMemo(string id, string memo)
    {
        var unsupported = new RoutingObservation { EventId = id, Status = "unsupported-json-memo" };
        if (!FlatStringObject().IsMatch(memo)) { return unsupported; }
        var pairs = StringPair().Matches(memo).ToList();
        if (pairs.Select(p => p.Groups[1].Value).Distinct(StringComparer.Ordinal).Count() != pairs.Count) { return unsupported; }
        var fields = pairs.ToDictionary(p => p.Groups[1].Value, p => p.Groups[2].Value, StringComparer.Ordinal);
        if (fields.GetValueOrDefault("channel") is not ("chaingeappv1" or "chainge")) { return unsupported; }
        var chain = fields.GetValueOrDefault("toChain", string.Empty);
        var recipient = fields.GetValueOrDefault("toAddr", string.Empty);
        var index = fields.GetValueOrDefault("toIndex", string.Empty);
        if (chain.Length == 0 || !chain.All(char.IsAsciiLetterOrDigit) || recipient.Length == 0 || index.Length == 0 || !index.All(char.IsAsciiDigit)) { return unsupported; }
        return new RoutingObservation { EventId = id, Status = "observed-json-memo", ChainCode = chain, TokenIndex = index, Recipient = recipient, MemoAmountRaw = ReadMemoAmount(fields.GetValueOrDefault("extra", string.Empty)) };
    }

    [GeneratedRegex("""\A\{\s*"[^"\\\x00-\x1f]*"\s*:\s*"[^"\\\x00-\x1f]*"(?:\s*,\s*"[^"\\\x00-\x1f]*"\s*:\s*"[^"\\\x00-\x1f]*")*\s*\}\z""")]
    private static partial Regex FlatStringObject();

    [GeneratedRegex(""""([^"\\\x00-\x1f]*)"\s*:\s*"([^"\\\x00-\x1f]*)"""")]
    private static partial Regex StringPair();

    private static RoutingObservation FindCandidates(RoutingObservation observation, IReadOnlyList<ChainEvent> events, MatchPolicy policy)
    {
        if ((observation.Status != "observed-colon-memo" && observation.Status != "observed-json-memo") || observation.MemoAmountRaw is null || observation.MemoAmountRaw == "0" || !policy.RoutingChainNames.TryGetValue(observation.ChainCode, out var chain)) { return observation; }
        var burn = events.First(e => e.Id == observation.EventId);
        if (burn.Timestamp is null) { return observation; }
        var candidates = events.Where(e => e.Chain == chain && e.ReceiptVerified && e.FullTime && e.Timestamp is not null && e.Token.Equals(observation.Token, StringComparison.OrdinalIgnoreCase) && e.To.Equals(observation.Recipient, StringComparison.OrdinalIgnoreCase) && e.RawAmount == observation.MemoAmountRaw)
            .Where(e => e.Timestamp - burn.Timestamp >= TimeSpan.Zero && e.Timestamp - burn.Timestamp <= policy.RoutingReviewMaximumDelay)
            .Select(e => e.Id).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        return observation with { CandidateEventIds = candidates };
    }
}
