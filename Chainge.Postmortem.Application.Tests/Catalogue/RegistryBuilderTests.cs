using System.Text.Json;
using Chainge.Postmortem.Application.Catalogue;
using Chainge.Postmortem.Application.Catalogue.Models;
using Xunit;

namespace Chainge.Postmortem.Application.Tests.Catalogue;

public sealed class RegistryBuilderTests
{
    [Fact]
    public async Task BuildRegistry_DuplicateAddress_PreservesRowsAndAliasesWithoutDoubleCounting()
    {
        var repository = new FixedCatalogueRepository
        {
            Source = new CatalogueSource
            {
                Tokens = [new() { Symbol = "B", Address = "0xAB", Row = 700 }, new() { Symbol = "A", Address = "0xcd", Row = 3 }, new() { Symbol = "Alias", Address = "0xab", Row = 2 }],
                Studies = new Dictionary<string, JsonElement> { ["0xAB"] = JsonSerializer.SerializeToElement("review") }
            }
        };
        var builder = new RegistryBuilder(repository);
        var expected = new RegistrySnapshot
        {
            Tokens = [new() { Symbol = "B / Alias", Address = "0xab", SourceRows = [2, 700], HasStudy = true }, new() { Symbol = "A", Address = "0xcd", SourceRows = [3], HasStudy = false }],
            Studies = new Dictionary<string, JsonElement> { ["0xab"] = JsonSerializer.SerializeToElement("review") }
        };

        var actual = await builder.BuildRegistry(TestContext.Current.CancellationToken);

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Fact]
    public async Task BuildRegistry_StudyWithoutToken_RejectsOrphanedEvidence()
    {
        var builder = new RegistryBuilder(new FixedCatalogueRepository
        {
            Source = new CatalogueSource { Tokens = [], Studies = new Dictionary<string, JsonElement> { ["0xab"] = JsonSerializer.SerializeToElement("review") } }
        });

        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => builder.BuildRegistry(TestContext.Current.CancellationToken));

        Assert.Equivalent("A case study references a contract absent from the catalogue.", exception.Message);
    }
}
