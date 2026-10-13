using System.Text;
using Chainge.Postmortem.Persistence.Json;
using Xunit;

namespace Chainge.Postmortem.Persistence.Json.Tests;

public sealed class EvidenceIntegrityVerifierTests
{
    private const string Digest = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task VerifyAsync_SupportedManifest_ShouldVerifyNestedInputs(bool array)
    {
        var root = Directory.CreateTempSubdirectory().FullName;
        var text = array ? "[{\"file\":\"nested/a.json\",\"sha256\":\"" + Digest + "\"}]" : "{\"files\":{\"nested/a.json\":\"" + Digest + "\"}}";
        Directory.CreateDirectory(Path.Combine(root, "nested"));
        await File.WriteAllTextAsync(Path.Combine(root, "manifest.json"), text, Encoding.UTF8, TestContext.Current.CancellationToken);
        var inputs = new Dictionary<string, string> { ["nested/a.json"] = Digest };
        var expected = new[] { "manifest.json" };

        var result = await EvidenceIntegrityVerifier.VerifyAsync(root, inputs, TestContext.Current.CancellationToken);
        var actual = result.Keys.ToArray();

        Assert.Equivalent(expected, actual, strict: true);
    }

    [Theory]
    [InlineData("a.json", "wrong")]
    [InlineData("missing.json", Digest)]
    [InlineData("../outside.json", Digest)]
    public async Task VerifyAsync_MismatchMissingEntryOrEscapedPath_ShouldReject(string path, string digest)
    {
        var root = Directory.CreateTempSubdirectory().FullName;
        await File.WriteAllTextAsync(Path.Combine(root, "manifest.json"), "{\"files\":{\"a.json\":\"" + Digest + "\"}}", Encoding.UTF8, TestContext.Current.CancellationToken);
        var inputs = new Dictionary<string, string> { [path] = digest };

        var actual = await Record.ExceptionAsync(() => EvidenceIntegrityVerifier.VerifyAsync(root, inputs, TestContext.Current.CancellationToken));

        Assert.IsType<InvalidDataException>(actual);
    }

    [Fact]
    public async Task VerifyAsync_NoManifest_ShouldReject()
    {
        var root = Directory.CreateTempSubdirectory().FullName;
        var inputs = new Dictionary<string, string> { ["a.json"] = Digest };

        var actual = await Record.ExceptionAsync(() => EvidenceIntegrityVerifier.VerifyAsync(root, inputs, TestContext.Current.CancellationToken));

        Assert.IsType<InvalidDataException>(actual);
    }
}
