[CmdletBinding()]
param(
    [string] $RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
)

$ErrorActionPreference = "Stop"

$excludedDirectories = @(".git", "bin", "obj", "node_modules")

function Get-RepositoryRelativePath {
    param([string] $Path)

    $rootWithSeparator = $RepositoryRoot.TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    $rootUri = [System.Uri]::new($rootWithSeparator)
    $pathUri = [System.Uri]::new($Path)

    return [System.Uri]::UnescapeDataString(
        $rootUri.MakeRelativeUri($pathUri).ToString()).Replace(
            '/', [System.IO.Path]::DirectorySeparatorChar)
}

$markdownFiles = Get-ChildItem -LiteralPath $RepositoryRoot -Recurse -File -Filter "*.md" |
    Where-Object {
        $relativePath = Get-RepositoryRelativePath $_.FullName
        -not ($excludedDirectories | Where-Object {
            $relativePath -split "[\\/]" -contains $_
        })
    }

$linkPattern = '!?(?:\[[^\]]*\])\((?<target>[^)]+)\)'
$failures = [System.Collections.Generic.List[string]]::new()
$checkedLinks = 0

foreach ($file in $markdownFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    $matches = [regex]::Matches($content, $linkPattern)

    foreach ($match in $matches) {
        $target = $match.Groups["target"].Value.Trim()

        if ($target.StartsWith("<") -and $target.EndsWith(">")) {
            $target = $target.Substring(1, $target.Length - 2)
        }

        if ([string]::IsNullOrWhiteSpace($target) -or
            $target.StartsWith("#") -or
            $target -match "^[a-zA-Z][a-zA-Z0-9+.-]*:") {
            continue
        }

        $pathPart = ($target -split "#", 2)[0]
        if ([string]::IsNullOrWhiteSpace($pathPart)) {
            continue
        }

        try {
            $pathPart = [System.Uri]::UnescapeDataString($pathPart)
        }
        catch {
            $relativeFile = Get-RepositoryRelativePath $file.FullName
            $failures.Add("$relativeFile -> invalid escaped path: $target")
            continue
        }

        $candidate = Join-Path $file.DirectoryName $pathPart
        $checkedLinks++

        if (-not (Test-Path -LiteralPath $candidate)) {
            $relativeFile = Get-RepositoryRelativePath $file.FullName
            $failures.Add("$relativeFile -> $target")
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Error ("Broken local Markdown links:`n- " + ($failures -join "`n- "))
}

Write-Host "Checked $checkedLinks local links across $($markdownFiles.Count) Markdown files."
