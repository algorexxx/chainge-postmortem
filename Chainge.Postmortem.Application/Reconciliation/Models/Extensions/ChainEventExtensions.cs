using System.Globalization;
using System.Numerics;

namespace Chainge.Postmortem.Application.Reconciliation.Models.Extensions;

public static class ChainEventExtensions
{
    public static BigInteger GetComparableUnits(this ChainEvent value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value.Decimals, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Decimals, 255);
        return BigInteger.Parse(value.RawAmount, CultureInfo.InvariantCulture) * BigInteger.Pow(10, 255 - value.Decimals);
    }
}
