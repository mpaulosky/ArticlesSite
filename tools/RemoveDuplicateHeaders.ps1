param(
    [string[]]$Paths
)

$ErrorActionPreference = 'Stop'

# Root of repo is parent of this script directory
$root = Split-Path -Parent $PSScriptRoot

# Determine target paths
if (-not $Paths -or $Paths.Count -eq 0) {
    $Paths = @($root)
} else {
    # Convert relative to absolute under repo
    $Paths = $Paths | ForEach-Object {
        if ([System.IO.Path]::IsPathRooted($_)) { $_ } else { Join-Path $root $_ }
    }
}

# Regex that matches two header banners in a row at the very top of a file (content may differ)
# A header banner: starts with // ===== line, followed by one or more //-comment lines, ends with // ===== line
# We will collapse any consecutive banners into a single banner (keep the first)
$doubleHeader = '(?ms)\A\s*(//\s*={6,}\r?\n(?://.*\r?\n)+//\s*={6,}\r?\n?)\s*(//\s*={6,}\r?\n(?://.*\r?\n)+//\s*={6,}\r?\n?)'

$changed = @()
$totalFiles = 0

foreach ($p in $Paths) {
    if (-not (Test-Path $p)) { continue }

    $files = @()
    if (Test-Path $p -PathType Leaf) {
        # Single file
        $fi = Get-Item -LiteralPath $p
        if ($fi.Extension -eq '.cs' -and $fi.FullName -notmatch '\\(bin|obj)\\') { $files += $fi }
    }
    else {
        # Directory: collect all .cs files under path, excluding bin/ and obj/
        $files = Get-ChildItem -Path $p -Recurse -File | Where-Object { $_.Extension -eq '.cs' -and $_.FullName -notmatch '\\(bin|obj)\\' }
    }

    $count = ($files | Measure-Object).Count
    $totalFiles += $count

    foreach ($f in $files) {
        $orig = Get-Content $f.FullName -Raw -Encoding UTF8
        $text = $orig

        # Repeatedly collapse duplicate headers at file start
        while ($text -match $doubleHeader) {
            $text = $text -replace $doubleHeader, '$1'
        }

        if ($text -ne $orig) {
            Set-Content -Path $f.FullName -Value $text -Encoding UTF8
            $changed += $f.FullName
        }
    }
}

Write-Host ("Processed {0} .cs files." -f $totalFiles)
Write-Host ("Updated {0} files with duplicate header removal." -f $changed.Count)
if ($changed.Count -gt 0) {
    Write-Host "Files changed:" -ForegroundColor Cyan
    $changed | ForEach-Object { Write-Host " - $_" }
}