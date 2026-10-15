param([Parameter(Mandatory)][string[]]$Path)
$ErrorActionPreference = 'Stop'
foreach ($configurationPath in $Path) {
    $bytes = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $configurationPath).Path)
    if ($bytes -contains 13) {
        throw "Save $configurationPath with LF line endings before generating reports. Git stores LF; CRLF produces different provenance hashes after checkout. Do not normalize raw evidence."
    }
}
