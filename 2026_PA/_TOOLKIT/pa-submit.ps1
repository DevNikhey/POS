# =====================================================================
#  pa-submit.ps1 - sauberes ZIP (ohne bin/obj/.vs/.git). Von pa-submit.bat aufgerufen.
# =====================================================================
param([Parameter(ValueFromRemainingArguments = $true)][string[]]$P = @())

$src = if ($P.Count -ge 1) { $P[0] } else { "." }
$name = if ($P.Count -ge 2) { $P[1] } else { "" }
if (-not (Test-Path $src)) { Write-Host "Verwendung: pa-submit.bat <projekt-ordner> [zipname]"; exit 1 }

if (-not $name) {
    $sln = Get-ChildItem -Path $src -Recurse -Depth 2 -Filter *.sln -ErrorAction SilentlyContinue | Select-Object -First 1
    $name = if ($sln) { [IO.Path]::GetFileNameWithoutExtension($sln.Name) } else { (Resolve-Path $src | Split-Path -Leaf) }
}
$out = "$name.zip"
if (Test-Path $out) { Write-Host "FEHLER: '$out' existiert bereits."; exit 1 }

$tmp = Join-Path ([IO.Path]::GetTempPath()) ("pasubmit_" + [Guid]::NewGuid().ToString("N"))
# robocopy kopiert ohne bin/obj/.vs/.git und ohne *.user etc.
robocopy $src $tmp /E /NFL /NDL /NJH /NJS /XD bin obj .vs .git /XF *.user *.suo .DS_Store *.zip | Out-Null
Compress-Archive -Path (Join-Path $tmp '*') -DestinationPath $out -Force
Remove-Item $tmp -Recurse -Force

$sz = (Get-Item $out).Length
Write-Host ("Erstellt: {0}  ({1:N0} Bytes)" -f $out, $sz)
