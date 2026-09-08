param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath
)

$ErrorActionPreference = 'Stop'

$resolvedPackage = (Resolve-Path -LiteralPath $PackagePath).Path
Add-Type -AssemblyName System.IO.Compression.FileSystem
$packageZip = [System.IO.Compression.ZipFile]::OpenRead($resolvedPackage)

try {
    $entryNames = @($packageZip.Entries | ForEach-Object FullName)
    @(
        'README.md',
        'lib/net48/MyDmsVn.BootstrapSourceGrid.dll',
        'lib/net8.0-windows7.0/MyDmsVn.BootstrapSourceGrid.dll'
    ) | ForEach-Object {
        if ($_ -notin $entryNames) {
            throw "Missing package entry: $_"
        }
    }

    if ($entryNames -match '(^|/)(SourceGrid|MyDmsVn\.Bootstrap5WinFormUI)\.dll$') {
        throw 'A vendor binary was embedded in the integration package.'
    }

    $nuspecEntries = @($packageZip.Entries | Where-Object FullName -Like '*.nuspec')
    if ($nuspecEntries.Count -ne 1) {
        throw "Expected one nuspec in package, found $($nuspecEntries.Count)."
    }

    $reader = [System.IO.StreamReader]::new($nuspecEntries[0].Open())
    try {
        [xml]$nuspec = $reader.ReadToEnd()
    }
    finally {
        $reader.Dispose()
    }

    $namespace = [System.Xml.XmlNamespaceManager]::new($nuspec.NameTable)
    $namespace.AddNamespace('n', $nuspec.DocumentElement.NamespaceURI)

    $license = $nuspec.SelectSingleNode('/n:package/n:metadata/n:license', $namespace)
    if ($null -eq $license -or
        $license.type -ne 'expression' -or
        $license.InnerText -ne 'MIT') {
        throw 'MIT package license expression is missing.'
    }

    $readme = $nuspec.SelectSingleNode('/n:package/n:metadata/n:readme', $namespace)
    if ($null -eq $readme -or $readme.InnerText -ne 'README.md') {
        throw 'Package README metadata is missing.'
    }

    $dependencyGroups = @($nuspec.SelectNodes(
        '/n:package/n:metadata/n:dependencies/n:group',
        $namespace))
    $expectedDependencyIds = @(
        'MyDmsVn.Bootstrap5WinFormUI',
        'SourceGrid'
    )

    foreach ($expectedGroup in @('.NETFramework4.8', 'net8.0-windows7.0')) {
        $matchingGroups = @($dependencyGroups | Where-Object targetFramework -EQ $expectedGroup)
        if ($matchingGroups.Count -ne 1) {
            throw "Expected one dependency group '$expectedGroup', found $($matchingGroups.Count)."
        }

        $dependencyIds = @($matchingGroups[0].SelectNodes('n:dependency', $namespace) |
            ForEach-Object { $_.GetAttribute('id') })
        foreach ($expectedId in $expectedDependencyIds) {
            if ($dependencyIds -notcontains $expectedId) {
                throw "Expected package dependency '$expectedId' was not found in group '$expectedGroup'."
            }
        }
    }

    Write-Output "Package verification passed: $resolvedPackage"
}
finally {
    $packageZip.Dispose()
}
