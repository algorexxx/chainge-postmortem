using Chainge.Postmortem.Application.Catalogue.Models;
using Chainge.Postmortem.Domain;

namespace Chainge.Postmortem.Persistence.Json.Extensions;

public static class TokenRecordExtensions
{
    public static TokenListing ToTokenListing(this TokenRecord token)
    {
        return new TokenListing { Symbol = token.Symbol, Address = token.Address, Row = token.Row };
    }
}
