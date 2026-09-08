param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath
)

$ErrorActionPreference = 'Stop'

$resolvedPackage = (Resolve-Path -LiteralPath $PackagePath).Path
$verifierPath = Join-Path $PSScriptRoot 'Verify-Package.ps1'
if (-not (Test-Path -LiteralPath $verifierPath -PathType Leaf)) {
    throw "Package verifier script is missing: $verifierPath"
}

& pwsh -NoProfile -File $verifierPath -PackagePath $resolvedPackage
if ($LASTEXITCODE -ne 0) {
    throw 'The package verifier rejected the valid inspection package.'
}

$testRoot = Join-Path ([System.IO.Path]::GetTempPath()) (
    'BootstrapSourceGrid-package-verifier-' + [Guid]::NewGuid().ToString('N'))
$extractPath = Join-Path $testRoot 'extracted'
$brokenPackage = Join-Path $testRoot 'missing-sourcegrid-dependency.nupkg'

try {
    New-Item -ItemType Directory -Path $extractPath | Out-Null
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory($resolvedPackage, $extractPath)

    $nuspecFiles = @(Get-ChildItem -LiteralPath $extractPath -Filter '*.nuspec')
    if ($nuspecFiles.Count -ne 1) {
        throw "Expected one nuspec in test package, found $($nuspecFiles.Count)."
    }

    [xml]$nuspec = Get-Content -Raw -LiteralPath $nuspecFiles[0].FullName
    $namespace = [System.Xml.XmlNamespaceManager]::new($nuspec.NameTable)
    $namespace.AddNamespace('n', $nuspec.DocumentElement.NamespaceURI)
    $net8Group = $nuspec.SelectSingleNode(
        '/n:package/n:metadata/n:dependencies/n:group[@targetFramework="net8.0-windows7.0"]',
        $namespace)
    if ($null -eq $net8Group) {
        throw 'The valid package did not contain the expected net8 dependency group.'
    }

    $sourceGridDependency = $net8Group.SelectSingleNode(
        'n:dependency[@id="SourceGrid"]',
        $namespace)
    if ($null -eq $sourceGridDependency) {
        throw 'The valid package did not contain the SourceGrid dependency to mutate.'
    }

    [void]$net8Group.RemoveChild($sourceGridDependency)
    $nuspec.Save($nuspecFiles[0].FullName)
    [System.IO.Compression.ZipFile]::CreateFromDirectory($extractPath, $brokenPackage)

    $brokenOutput = & pwsh -NoProfile -File $verifierPath -PackagePath $brokenPackage 2>&1
    $brokenExitCode = $LASTEXITCODE
    if ($brokenExitCode -eq 0) {
        throw 'The package verifier accepted a package with a missing SourceGrid dependency.'
    }

    $expectedFailure = "Expected package dependency 'SourceGrid' was not found in group 'net8.0-windows7.0'."
    if (($brokenOutput | Out-String) -notmatch [Regex]::Escape($expectedFailure)) {
        throw "The verifier failed for an unexpected reason:`n$($brokenOutput | Out-String)"
    }

    Write-Output 'Package verifier tests passed for valid and missing-dependency packages.'
}
finally {
    $resolvedTestRoot = [System.IO.Path]::GetFullPath($testRoot)
    $resolvedSystemTemp = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
    if ($resolvedTestRoot.StartsWith($resolvedSystemTemp, [StringComparison]::OrdinalIgnoreCase) -and
        [System.IO.Path]::GetFileName($resolvedTestRoot).StartsWith(
            'BootstrapSourceGrid-package-verifier-',
            [StringComparison]::Ordinal)) {
        Remove-Item -LiteralPath $resolvedTestRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}
