using System.Text.Json;
using Chainge.Postmortem.Application.Catalogue;
using Chainge.Postmortem.Persistence.Json.Models;
using Chainge.Postmortem.Site.Extensions;
using Microsoft.Extensions.DependencyInjection;

ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, 3);
var input = Path.GetFullPath(args[0]);
var studiesPath = Path.GetFullPath(args[1]);
var output = Path.GetFullPath(args[2]);
var services = new ServiceCollection().AddResearchSite(new CatalogueSettings
{
    CataloguePath = input,
    StudiesPath = studiesPath
});
await using var provider = services.BuildServiceProvider();
var snapshot = await provider.GetRequiredService<RegistryBuilder>().BuildRegistry(CancellationToken.None);
Directory.CreateDirectory(output);
foreach (var file in Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "Assets")))
{
    File.Copy(file, Path.Combine(output, Path.GetFileName(file)), true);
}

var json = JsonSerializer.Serialize(snapshot, JsonSerializerOptions.Web);
await File.WriteAllTextAsync(Path.Combine(output, "registry.js"), "window.researchRegistry = " + json + ";\n");
Console.WriteLine($"Built {snapshot.Tokens.Count} contract entries with {snapshot.Studies.Count} case studies into {output}");
