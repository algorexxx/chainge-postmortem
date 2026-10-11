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
var assets = Path.Combine(AppContext.BaseDirectory, "Assets");
var template = await File.ReadAllTextAsync(Path.Combine(assets, "index.html"));
var styles = await File.ReadAllTextAsync(Path.Combine(assets, "styles.css"));
var script = await File.ReadAllTextAsync(Path.Combine(assets, "app.js"));
var json = JsonSerializer.Serialize(snapshot, JsonSerializerOptions.Web);
var html = template.Replace("/* SITE_STYLES */", styles, StringComparison.Ordinal)
    .Replace("/* SITE_SCRIPT */", script, StringComparison.Ordinal)
    .Replace("REGISTRY_DATA", json, StringComparison.Ordinal);
await File.WriteAllTextAsync(Path.Combine(output, "index.html"), html);
Console.WriteLine($"Built {snapshot.Tokens.Count} contract entries with {snapshot.Studies.Count} case studies into {output}");
