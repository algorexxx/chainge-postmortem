using System.Text.Json;
using System.Text.Json.Nodes;
using System.Buffers;
using Chainge.Postmortem.Application.Catalogue.Interfaces;
using Chainge.Postmortem.Application.Catalogue.Models;
using Chainge.Postmortem.Domain;
using Chainge.Postmortem.Persistence.Json.Extensions;
using Chainge.Postmortem.Persistence.Json.Models;
using Microsoft.VisualBasic.FileIO;

namespace Chainge.Postmortem.Persistence.Json.Repositories;

public sealed class CatalogueRepository : ICatalogueRepository
{
    private static readonly SearchValues<char> HexadecimalCharacters = SearchValues.Create("0123456789abcdefABCDEF");
    private readonly CatalogueSettings settings;

    public CatalogueRepository(CatalogueSettings settings)
    {
        this.settings = settings;
    }

    public async Task<CatalogueSource> ReadCatalogueAsync(CancellationToken cancellationToken)
    {
        var text = await File.ReadAllTextAsync(settings.CataloguePath, cancellationToken);
        var records = ReadTokenRows(text);
        await using var studiesFile = File.OpenRead(settings.StudiesPath);
        var studies = await JsonSerializer.DeserializeAsync<Dictionary<string, JsonElement>>(studiesFile, cancellationToken: cancellationToken)
            ?? throw new InvalidDataException("The case studies are null.");
        foreach (var pair in studies.ToList())
        {
            if (!pair.Value.TryGetProperty("movementsFile", out var reportFile)) { continue; }
            var reportPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(settings.StudiesPath)!, reportFile.GetString()!));
            var reportText = await File.ReadAllTextAsync(reportPath, cancellationToken);
            var study = JsonNode.Parse(pair.Value.GetRawText())!;
            study["automatedMovements"] = JsonNode.Parse(reportText);
            studies[pair.Key] = JsonSerializer.SerializeToElement(study);
        }
        return new CatalogueSource { Tokens = records.Select(token => token.ToTokenListing()).ToList(), Studies = studies };
    }

    private static List<TokenRecord> ReadTokenRows(string text)
    {
        using var reader = new StringReader(text);
        using var parser = new TextFieldParser(reader);
        parser.SetDelimiters(",");
        var header = parser.ReadFields();
        if (header is not ["Asset", "Address on Fusion"])
        {
            throw new InvalidDataException("Unexpected catalogue column headers.");
        }

        var records = new List<TokenRecord>();
        while (!parser.EndOfData)
        {
            var row = checked((int)parser.LineNumber);
            var fields = parser.ReadFields();
            if (fields is not { Length: 2 } || fields[1].Length != 42 || !fields[1].StartsWith("0x", StringComparison.Ordinal)
                || fields[1].AsSpan(2).IndexOfAnyExcept(HexadecimalCharacters) >= 0)
            {
                throw new InvalidDataException($"Invalid token catalogue row {row}.");
            }

            records.Add(new TokenRecord { Symbol = fields[0], Address = fields[1], Row = row });
        }

        return records;
    }
}
